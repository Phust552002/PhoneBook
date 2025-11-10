using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using PhoneBook.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

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

        public async Task<IActionResult> Index()
        {
            var fullName = User.FindFirst("FullName")?.Value ?? User.Identity.Name;
            var positionName = User.FindFirst("PositionName")?.Value ?? "Nhân viên";
            var isAdmin = User.FindFirst("IsAdmin")?.Value == "True";
            ViewBag.UserName = fullName;
            ViewBag.UserPosition = positionName;
            ViewBag.isAdmin = isAdmin;
            var departments = await _repo.GetDepartmentsAsync();
            return View(departments);
        }

        // lấy danh sách nhân viên theo phòng ban
        [HttpGet]
        public async Task<IActionResult> GetEmployees(int departmentId)
        {
            var employees = await _repo.GetEmployeesByDepartmentAsync(departmentId);
            return Json(employees);
        }

        [HttpGet]
        public async Task<IActionResult> GetDepartments()
        {
            var departments = await _repo.GetDepartmentsAsync();

            var tree = departments.Select(d => new
            {
                id = d.DepartmentId,
                text = d.DepartmentName,
                parentId = d.ParentId,
                expanded = false,
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
        [HttpPost]
        public async Task<IActionResult> GetAllEmployees()
        {
                var employees = await _repo.GetAllEmployeesAsync();
                return Json(employees);
        }

        //Đọc dữ liệu từ request 
        //Cố gắng gán giá trị cho từng thuộc tính của employee
        //Kiểm tra các attribute validation trên model(như[Required], [StringLength], [Range], v.v.)
        //Lưu kết quả vào ModelState:
        //Nếu dữ liệu hợp lệ → ModelState.IsValid = true
        [HttpPost]
        public async Task<IActionResult> UpdateEmployee([DataSourceRequest] DataSourceRequest request, Employee employee)
        {
            var isAdmin = User.FindFirst("IsAdmin")?.Value == "True";
            if(!isAdmin)
            {
                ModelState.AddModelError("", "You do not have permission to update employee information.");
                return Json(new[] { employee }.ToDataSourceResult(request, ModelState));
            }  
            
            if (employee == null || employee.UserId <= 0)
            {
                return Json(new[] { employee }.ToDataSourceResult(request));
            }

            if (!ModelState.IsValid)
                {
                    return Json(new[] { employee }.ToDataSourceResult(request, ModelState));
                }

                // Update employee
                var result = await _repo.UpdateEmployeeAsync(employee);

            if (result)
                {

                return Json(new[] { employee }.ToDataSourceResult(request, ModelState));
                }
            else
                {
                    ModelState.AddModelError("", "Failed to update employee");
                    return Json(new[] { employee }.ToDataSourceResult(request, ModelState));
                }

        }

        public IActionResult SetLanguage(string culture, string returnUrl = "/")
        {
            Response.Cookies.Append(
                CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
                new CookieOptions { Expires = DateTimeOffset.UtcNow.AddMinutes(5)}
            );
            return LocalRedirect(returnUrl);
        }


    }
}
