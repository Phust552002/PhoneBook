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
            WHERE Status =3
            ORDER BY Sortby";

        using var conn = CreateConnection();
        var all = (await conn.QueryAsync<Department>(sql)).ToList();


        // Dựng cây (Tree View)
        var lookup = all.ToDictionary(d => d.DepartmentId);
        foreach (var dept in all)
        {
            if (dept.ParentId != -1 && lookup.ContainsKey(dept.ParentId))
            {
                lookup[dept.ParentId].Children.Add(dept);
            }
        }

        var roots = all.Where(d => d.ParentId == -1).ToList();
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
            v.UserId, 
            v.UserName, 
            v.EmployeeCode, 
            v.FullName, 
            v.WorkingPhone, 
            v.HandPhone, 
            v.BusinessEmail, 
            v.Status,
            v.Sortby,
            v.LevelPosition
        FROM View_H0_DepartmentEmployee v
        INNER JOIN cte ON v.DepartmentId = cte.DepartmentId
        WHERE v.Status = 1
        ORDER BY v.Sortby,v.LevelPosition";

        using var conn = CreateConnection();
        var result = (await conn.QueryAsync<Employee>(sql, new { departmentId })).ToList();
        return result;
    }

    public async Task<List<Employee>> GetAllEmployeesAsync()
    {
        const string sql = @"
        SELECT 
            v.UserId, v.UserName, v.EmployeeCode, v.FullName, 
            v.WorkingPhone, v.HandPhone, v.BusinessEmail, v.Status, v.Sortby, v.LevelPosition
        FROM View_H0_DepartmentEmployee v
        WHERE v.Status = 1
        ORDER BY v.Sortby,v.LevelPosition";

        using var connection = CreateConnection();
        var employees = await connection.QueryAsync<Employee>(sql);
        return employees.ToList();
    }

    // Lấy thông tin nhân viên theo username - nav
    public async Task<Employee> GetEmployeeByUsernameAsync(string username)
    {
        const string sql = @"
            SELECT 
                v.UserId, v.UserName, v.EmployeeCode, v.FullName, 
                v.Password, v.PositionName,
                v.WorkingPhone, v.HandPhone, v.BusinessEmail, v.Status,
                de.DepartmentId
            FROM View_H0_DepartmentEmployee v
            LEFT JOIN H0_DepartmentEmployee de ON v.UserId = de.UserId
            WHERE v.UserName = @Username AND v.Status = 1 ";

        using var conn = CreateConnection();
        var employee = await conn.QueryFirstOrDefaultAsync<Employee>(sql, new { Username = username });
        return employee;
    }

    // 🔹 Lấy thông tin nhân viên theo UserId
    public async Task<Employee> GetEmployeeByIdAsync(int userId)
    {
        const string sql = @"
            SELECT 
                v.UserId, v.UserName, v.EmployeeCode, v.FullName, 
                v.Password, v.PositionName,
                v.WorkingPhone, v.HandPhone, v.BusinessEmail, v.Status,
                de.DepartmentId
            FROM View_H0_DepartmentEmployee v
            LEFT JOIN H0_DepartmentEmployee de ON v.UserId = de.UserId
            WHERE v.UserId = @UserId AND v.Status =1 ";

        using var conn = CreateConnection();
        var employee = await conn.QueryFirstOrDefaultAsync<Employee>(sql, new { UserId = userId });
        return employee;
    }
    public async Task<bool> UpdateEmployeeAsync(Employee employee)
    {
            using var connection = CreateConnection();

            var query = @"
                    UPDATE View_H0_DepartmentEmployee 
                    SET 
                        FullName = @FullName,
                        WorkingPhone = @WorkingPhone,
                        HandPhone = @HandPhone,
                        BusinessEmail = @BusinessEmail
                    WHERE UserId = @UserId";

            var parameters = new
            {
                employee.UserId,
                employee.FullName,
                employee.WorkingPhone,
                employee.HandPhone,
                employee.BusinessEmail,
            };

            var rowsAffected = await connection.ExecuteAsync(query, parameters);
            return rowsAffected > 0;
        
    }
}