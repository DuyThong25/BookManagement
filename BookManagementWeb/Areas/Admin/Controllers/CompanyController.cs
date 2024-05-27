using BookManager.DataAccess.Repository.IRepository;
using BookManager.Models;
using BookManager.Models.ViewModel;
using BookManager.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.FileProviders;
using System.Collections.Generic;

namespace BookManagementWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = StaticDetail.Role_Admin)]

    public class CompanyController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public CompanyController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            return View(_unitOfWork.Company.GetAll());
        }

        public IActionResult CreateOrUpdate(int? id)
        {
            Company company = new Company();
            if (id != null && id != 0) // update
            {
                company = _unitOfWork.Company.Get(x => x.Id == id);
            }
            return View(company);

        }
        [HttpPost]
        public IActionResult CreateOrUpdate(Company company)
        {
            if (ModelState.IsValid)
            {        

                if(company.Id == 0) // Create
                {
                    _unitOfWork.Company.Add(company);
                    _unitOfWork.Save();
                    TempData["success"] = "Company add successfully";
                }else // update
                {
                    _unitOfWork.Company.Update(company);
                    _unitOfWork.Save();
                    TempData["success"] = "Company edit successfully";
                }
                return RedirectToAction("Index");
            }
            else
            {       
                TempData["error"] = "Something went wrong..";
                return View(company);
            }
        }
        #region Api Method
        
        [HttpGet]
        public IActionResult GetAll()
        {
            List<Company> listCompany = _unitOfWork.Company.GetAll().ToList();
            return Json(new {data = listCompany});
        }

        [HttpDelete]
        public IActionResult Delete(int id)
        {
            Company? company = _unitOfWork.Company.Get(x => x.Id == id);

            if (company != null)
            {
                _unitOfWork.Company.Remove(company);
                _unitOfWork.Save();
                return Json(new { success = true, message = "Delete Succesful" });
            }
            else
            {
                return Json(new { success = false, message = "Delete Fail" });
            }
        }

        #endregion

    }
}
