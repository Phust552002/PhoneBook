using PhoneBook.Models;

public interface IPhoneBookRepository
{
    // Existing methods
    Task<List<Department>> GetDepartmentsAsync();
    Task<List<Employee>> GetEmployeesByDepartmentAsync(int departmentId);
    Task<List<Employee>> GetAllEmployeesAsync();

    // New methods for authentication
    Task<Employee> GetEmployeeByUsernameAsync(string username);
    Task<Employee> GetEmployeeByIdAsync(int userId);
}