using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PhoneBook.Models;
using System.Security.Claims;

namespace PhoneBook.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly IPhoneBookRepository _repo;

        public HomeController(IPhoneBookRepository repo)
        {
            _repo = repo;
        }

        // Trang chính
        public async Task<IActionResult> Index()
        {
            var fullName = User.FindFirst("FullName")?.Value ?? User.Identity.Name;
            var positionName = User.FindFirst("PositionName")?.Value ?? "Nhân viên";

            ViewBag.UserName = fullName;
            ViewBag.UserPosition = positionName;
            var departments = await _repo.GetDepartmentsAsync();
            return View(departments);
        }

        // API lấy danh sách nhân viên theo phòng ban
        [HttpGet]
        public async Task<IActionResult> GetEmployees(int departmentId)
        {
            //if (departmentId <= 0)
            //    return Json(Array.Empty<Employee>());

            var employees = await _repo.GetEmployeesByDepartmentAsync(departmentId);
            return Json(employees);
        }
        [HttpGet]
        public async Task<IActionResult> GetDepartments()
        {
            var departments = await _repo.GetDepartmentsAsync();

            // Chuẩn hóa dữ liệu cho Kendo TreeView
            var tree = departments.Select(d => new
            {
                id = d.DepartmentId,
                text = d.DepartmentName,
                parentId = d.ParentId,        // ✳️ thêm parentId
                expanded = false, // hiển thị sẵn
                items = BuildTree(d.Children)
            });

            return Json(tree);
        }

        private List<object> BuildTree(List<Department> children)
        {
            if (children == null || children.Count == 0)
                return new List<object>();

            return children.Select(c => new
            {
                id = c.DepartmentId,
                text = c.DepartmentName,
                parentId = c.ParentId,
                expanded = false,
                items = BuildTree(c.Children)
            }).ToList<object>();
        }
        [HttpGet]
        public async Task<IActionResult> GetEmployeesByDepartment(int departmentId)
        {
            var result = await _repo.GetEmployeesByDepartmentAsync(departmentId);
            return Json(result);
        }
        [HttpGet]
        public async Task<IActionResult> GetAllEmployees()
        {
            try
            {
                var employees = await _repo.GetAllEmployeesAsync();
                return Json(employees);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi GetAllEmployees: " + ex.Message);
                return StatusCode(500, "Lỗi khi lấy danh sách nhân viên.");
            }
        }
    }

}