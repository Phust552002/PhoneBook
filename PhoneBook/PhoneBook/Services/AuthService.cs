using PhoneBook.Models;
using System.Security.Cryptography;
using System.Text;
using System.Diagnostics;

namespace PhoneBook.Services
{
    public class AuthService : IAuthService
    {
        private readonly IPhoneBookRepository _repo;
        public AuthService(IPhoneBookRepository repo)
        {
            _repo = repo;
        }
        public async Task<Employee> AuthenticateAsync(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                return null;
            }

            var employee = await _repo.GetEmployeeByUsernameAsync(username);

            if (employee == null)
            {
                return null;
            }
            bool isPasswordValid = false;

            // Trường hợp 1: Password đã được hash
            if (!string.IsNullOrEmpty(employee.Password) && employee.Password.Length == 64)
            {
                isPasswordValid = VerifyPassword(password, employee.Password);
            }
            // Trường hợp 2: Password plain text (tạm thời để test)
            else if (!string.IsNullOrEmpty(employee.Password))
            {
                isPasswordValid = (employee.Password == password);
            }

            if (!isPasswordValid)
            {
                return null;
            }
            return employee;
        }

        public string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder builder = new StringBuilder();
                foreach (byte b in bytes)
                {
                    builder.Append(b.ToString("x2"));
                }
                return builder.ToString();
            }
        }

        public bool VerifyPassword(string password, string hashedPassword)
        {
            if (string.IsNullOrEmpty(hashedPassword))
                return false;

            string hashOfInput = HashPassword(password);
            return StringComparer.OrdinalIgnoreCase.Compare(hashOfInput, hashedPassword) == 0;
        }
    }
}