using BookManager.DataAccess.Data;
using BookManager.DataAccess.Repository.IRepository;
using BookManager.Models;
using BookManager.Models.ViewModel;
using BookManager.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
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
        private readonly UserManager<IdentityUser> _userManager;
        [BindProperty]
        public RoleManagerVM RoleManager { get; set; }

        public UserController(ApplicationDbContext db, UserManager<IdentityUser> userManager)
        {
            _db = db;
            _userManager = userManager;
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

        public IActionResult Pemission(string id)
        {
            string roleID = _db.UserRoles.FirstOrDefault(x => x.UserId == id).RoleId;
            RoleManager = new()
            {
                ApplicationUser = _db.ApplicationUsers.Include(x => x.Company).FirstOrDefault(x => x.Id == id),
                RoleList = _db.Roles.Select(x => new SelectListItem
                {
                    Text = x.Name,
                    Value = x.Name
                }),
                CompanyList = _db.Companies.Select(x => new SelectListItem
                {
                    Text = x.Name,
                    Value = x.Id.ToString()
                })
            };

            RoleManager.ApplicationUser.Role = _db.Roles.FirstOrDefault(x => x.Id == roleID).Name;
            return View(RoleManager);
        }

        [HttpPost]
        public IActionResult Pemission()
        {
            string roleID = _db.UserRoles.FirstOrDefault(x => x.UserId == RoleManager.ApplicationUser.Id).RoleId;
            string oldRole = _db.Roles.FirstOrDefault(x => x.Id == roleID).Name;

            // Update happening
            if (RoleManager.ApplicationUser.Role != oldRole)
            {
                var applicationUser = _db.ApplicationUsers.FirstOrDefault(x => x.Id == RoleManager.ApplicationUser.Id);
                if (RoleManager.ApplicationUser.Role == StaticDetail.Role_Company)
                {
                    applicationUser.CompanyID = RoleManager.ApplicationUser.CompanyID;
                }
                //if old role == role Company -> new role not be company
                if (oldRole == StaticDetail.Role_Company)
                {
                    applicationUser.CompanyID = null;
                }

                _db.Update(applicationUser);
                _db.SaveChanges();

                // Update UserRole table
                _userManager.RemoveFromRoleAsync(applicationUser, oldRole).GetAwaiter().GetResult();
                _userManager.AddToRoleAsync(applicationUser, RoleManager.ApplicationUser.Role).GetAwaiter().GetResult();

                TempData["success"] = "Update Pemission Successfully.";
                return RedirectToAction("Index");
            }

            TempData["error"] = "Update Pemission Fail.";
            return View(RoleManager);
        }

        #region Api Method

        [HttpPost]
        public IActionResult LockUnlockUser(string id)
        {
            var userFromDB = _db.ApplicationUsers.FirstOrDefault(x => x.Id == id);

            if (userFromDB != null)
            {
                // User is currently locked -> Unlock User
                if (userFromDB.LockoutEnd != null && userFromDB.LockoutEnd > DateTime.Now)
                {
                    userFromDB.LockoutEnd = DateTime.Now;
                    _db.Update(userFromDB);
                    _db.SaveChanges();
                    TempData["success"] = "Unlocking User Successfully.";
                    return Json(new { success = true });

                }
                else
                {
                    // User is currently unlocked -> Lock User
                    userFromDB.LockoutEnd = DateTime.Now.AddYears(1000);
                    _db.Update(userFromDB);
                    _db.SaveChanges();
                    TempData["success"] = "locking User Successfully.";
                    return Json(new { success = true });
                }
            }
            // running here means bug
            TempData["error"] = "Error while Locking/Unlocking User.";
            return Json(new { success = false });
        }
        #endregion

    }
}
