using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;
using System;
using System.Diagnostics;
using PhoneBook.Models;
using Microsoft.Extensions.Configuration;

public class PhoneBookRepository : IPhoneBookRepository
{
    private readonly string _connectionString;

    public PhoneBookRepository(IConfiguration config)
    {
        _connectionString = config.GetConnectionString("HRMDb");
    }

    private IDbConnection CreateConnection() => new SqlConnection(_connectionString);

    // 🔹 Lấy danh sách phòng ban và dựng cây
    public async Task<List<Department>> GetDepartmentsAsync()
    {
        const string sql = @"
            SELECT DepartmentId, DepartmentName, ParentId, Level, RootName, Status
            FROM H0_Departments
            WHERE Status > 0
            ORDER BY ParentId, DepartmentName";

        using var conn = CreateConnection();
        Debug.WriteLine("[GetDepartmentsAsync] Đang thực thi truy vấn SQL...");
        var all = (await conn.QueryAsync<Department>(sql)).ToList();
        Debug.WriteLine($"Đã lấy {all.Count} phòng ban từ DB.");

        if (!all.Any())
        {
            Debug.WriteLine("⚠️ Không có dữ liệu phòng ban nào được trả về (có thể Status != 1 hoặc bảng trống).");
        }

        // Dựng cây (Tree View)
        var lookup = all.ToDictionary(d => d.DepartmentId);
        foreach (var dept in all)
        {
            if (dept.ParentId != -1 && lookup.ContainsKey(dept.ParentId))
            {
                lookup[dept.ParentId].Children.Add(dept);
                Debug.WriteLine($"✓ Gắn {dept.DepartmentName} vào cha {lookup[dept.ParentId].DepartmentName}");
            }
        }

        var roots = all.Where(d => d.ParentId == -1).ToList();
        Debug.WriteLine($"Có {roots.Count} phòng ban gốc được tạo cây.");
        return roots;
    }

    // 🔹 Lấy nhân viên theo phòng ban
    public async Task<List<Employee>> GetEmployeesByDepartmentAsync(int departmentId)
    {
        const string sql = @"
        ;WITH cte AS (
            SELECT DepartmentId 
            FROM H0_Departments 
            WHERE DepartmentId = @departmentId
            UNION ALL
            SELECT d.DepartmentId
            FROM H0_Departments d
            INNER JOIN cte ON d.ParentId = cte.DepartmentId
        )
        SELECT DISTINCT 
            e.UserId, e.UserName, e.EmployeeCode, e.FullName, 
            e.WorkingPhone, e.HandPhone, e.HomePhone, e.Status
        FROM H0_DepartmentEmployee de
        INNER JOIN Employees e ON e.UserId = de.UserId
        INNER JOIN cte ON de.DepartmentId = cte.DepartmentId
        WHERE e.Status > 0
        ORDER BY e.FullName";

        using var conn = CreateConnection();
        var result = (await conn.QueryAsync<Employee>(sql, new { departmentId })).ToList();
        Debug.WriteLine($"✓ Đã lấy {result.Count} nhân viên (bao gồm cả phòng con) của phòng ban {departmentId}.");
        return result;
    }

    public async Task<List<Employee>> GetAllEmployeesAsync()
    {
        const string sql = @"
        SELECT 
            e.UserId, e.UserName, e.EmployeeCode, e.FullName, 
            e.WorkingPhone, e.HandPhone, e.HomePhone, e.Status
        FROM Employees e
        WHERE e.Status > 0
        ORDER BY e.UserId";

        using var connection = CreateConnection();
        var employees = await connection.QueryAsync<Employee>(sql);
        return employees.ToList();
    }

    // 🔹 Lấy thông tin nhân viên theo username (cho đăng nhập)
    public async Task<Employee> GetEmployeeByUsernameAsync(string username)
    {
        const string sql = @"
            SELECT 
                e.UserId, e.UserName, e.EmployeeCode, e.FullName, 
                e.Password, e.PositionName,
                e.WorkingPhone, e.HandPhone, e.HomePhone, e.Status,
                de.DepartmentId
            FROM Employees e
            LEFT JOIN H0_DepartmentEmployee de ON e.UserId = de.UserId
            WHERE e.UserName = @Username AND e.Status > 0";

        using var conn = CreateConnection();
        Debug.WriteLine($"[GetEmployeeByUsernameAsync] Đang tìm user: {username}");

        var employee = await conn.QueryFirstOrDefaultAsync<Employee>(sql, new { Username = username });

        if (employee != null)
        {
            Debug.WriteLine($"✓ Tìm thấy user: {employee.FullName} (UserId: {employee.UserId})");
        }
        else
        {
            Debug.WriteLine($"⚠️ Không tìm thấy user: {username}");
        }

        return employee;
    }

    // 🔹 Lấy thông tin nhân viên theo UserId
    public async Task<Employee> GetEmployeeByIdAsync(int userId)
    {
        const string sql = @"
            SELECT 
                e.UserId, e.UserName, e.EmployeeCode, e.FullName, 
                e.PositionName,
                e.WorkingPhone, e.HandPhone, e.HomePhone, e.Status,
                de.DepartmentId
            FROM Employees e
            LEFT JOIN H0_DepartmentEmployee de ON e.UserId = de.UserId
            WHERE e.UserId = @UserId AND e.Status > 0";

        using var conn = CreateConnection();
        Debug.WriteLine($"[GetEmployeeByIdAsync] Đang tìm user với ID: {userId}");

        var employee = await conn.QueryFirstOrDefaultAsync<Employee>(sql, new { UserId = userId });

        if (employee != null)
        {
            Debug.WriteLine($"✓ Tìm thấy user: {employee.FullName}");
        }
        else
        {
            Debug.WriteLine($"⚠️ Không tìm thấy user với ID: {userId}");
        }

        return employee;
    }
}