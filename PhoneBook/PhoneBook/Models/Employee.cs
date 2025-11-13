#nullable enable
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Localization;
namespace PhoneBook.Models
{
    public class Employee
    {
        public int UserId { get; set; }
        public string? UserName { get; set; }
        public string? EmployeeCode { get; set; }
        public string? FullName { get; set; }
        public string? WorkingPhone { get; set; }
        public string? HandPhone { get; set; }
        public string? BusinessEmail { get; set; }
        public int Status { get; set; }

        // Thêm các property cho authentication
        public string? Password { get; set; } // Chỉ dùng khi login, không hiển thị trong danh sách
        public string? PositionName { get; set; } // Chức danh
        public int? DepartmentId { get; set; } // ID phòng ban
    }

    public class LoginViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập tên đăng nhập")]
        [Display(Name = "Tên đăng nhập")]
        public string? Username { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu")]
        public string? Password { get; set; }

        [Display(Name = "Ghi nhớ đăng nhập")]
        public bool RememberMe { get; set; }
    }
}