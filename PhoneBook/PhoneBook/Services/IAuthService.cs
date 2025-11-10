using PhoneBook.Models;

namespace PhoneBook.Services
{
    public interface IAuthService
    {
        Task<Employee> AuthenticateAsync(string username, string password);
        string HashPassword(string password);
        bool VerifyPassword(string password, string hashedPassword);
        Task<List<int>> GetUserRolesAsync(int userId);
    }
}