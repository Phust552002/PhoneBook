using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Runtime.ConstrainedExecution;
using Telerik.SvgIcons;

namespace PhoneBook.Controllers
{
    public class GridController : Controller
    {
        private readonly IPhoneBookRepository _repo;

        public GridController(IPhoneBookRepository repo)
        {
            _repo = repo;
        }

        //  PDF Export
        public IActionResult Pdf_Export()
        {
            return View(); 
        }

        //  dữ liệu cho  Grid PDF Export
        [HttpPost]
        public async Task<IActionResult> Pdf_Export_Read([DataSourceRequest] Kendo.Mvc.UI.DataSourceRequest request)
        {
            var employees = await _repo.GetAllEmployeesAsync();

            // các trường cần hiển thị trong PDF
            var result = employees.Select(e => new
            {
                e.UserName,
                e.WorkingPhone,
                e.HandPhone,
                e.BusinessEmail
            });

            return Json(result.ToDataSourceResult(request));
        }

        // Lưu file PDF từ client
        [HttpPost]
        [IgnoreAntiforgeryToken]
        [HttpPost]
        public IActionResult Pdf_Export_Save(string contentType, string base64, string fileName)
        {
            try
            {
                // decode base64
                byte[] fileContents;
                try
                {
                    fileContents = Convert.FromBase64String(base64);
                }
                catch (Exception ex)
                {
                    return BadRequest($"Invalid base64 data: {ex.Message}");
                }


                return File(fileContents, contentType, fileName);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    error = ex.Message,
                    stackTrace = ex.StackTrace
                });
            }
        }
    }
}
