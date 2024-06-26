using BookManager.DataAccess.Data;
using BookManager.DataAccess.Repository.IRepository;
using BookManager.Models;
using BookManager.Models.ViewModel;
using BookManager.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using System.Collections.Generic;

namespace BookManagementWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = StaticDetail.Role_Admin)]
    public class UserController : Controller
    {
        private readonly ApplicationDbContext _db;
        public UserController(ApplicationDbContext db)
        {
            _db = db;
        }
        public IActionResult Index()
        {
            var listUserFromDB = _db.ApplicationUsers.Include(x => x.Company).ToList();

            foreach (var user in listUserFromDB)
            {
                string? roleID = _db.UserRoles.FirstOrDefault(x => x.UserId == user.Id)?.RoleId;
                string? roleName = _db.Roles.FirstOrDefault(x => x.Id == roleID)?.Name;

                if (!String.IsNullOrEmpty(roleName))
                {
                    user.Role = roleName;
                }
            }
            return View(listUserFromDB);
        }

        //public IActionResult CreateOrUpdate(int? id)
        //{
        //    Company company = new Company();
        //    if (id != null && id != 0) // update
        //    {
        //        company = _unitOfWork.Company.Get(x => x.Id == id);
        //    }
        //    return View(company);

        //}
        //[HttpPost]
        //public IActionResult CreateOrUpdate(Company company)
        //{
        //    if (ModelState.IsValid)
        //    {        

        //        if(company.Id == 0) // Create
        //        {
        //            _unitOfWork.Company.Add(company);
        //            _unitOfWork.Save();
        //            TempData["success"] = "Company add successfully";
        //        }else // update
        //        {
        //            _unitOfWork.Company.Update(company);
        //            _unitOfWork.Save();
        //            TempData["success"] = "Company edit successfully";
        //        }
        //        return RedirectToAction("Index");
        //    }
        //    else
        //    {       
        //        TempData["error"] = "Something went wrong..";
        //        return View(company);
        //    }
        //}
        #region Api Method

        //[HttpGet]
        //public IActionResult GetAll()
        //{
        //    List<Company> listCompany = _unitOfWork.Company.GetAll().ToList();
        //    return Json(new {data = listCompany});
        //}

        //[HttpDelete]
        //public IActionResult Delete(int id)
        //{
        //    Company? company = _unitOfWork.Company.Get(x => x.Id == id);

        //    if (company != null)
        //    {
        //        _unitOfWork.Company.Remove(company);
        //        _unitOfWork.Save();
        //        return Json(new { success = true, message = "Delete Succesful" });
        //    }
        //    else
        //    {
        //        return Json(new { success = false, message = "Delete Fail" });
        //    }
        //}

        #endregion

    }
}
