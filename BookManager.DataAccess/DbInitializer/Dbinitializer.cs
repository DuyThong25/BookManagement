using BookManager.DataAccess.Data;
using BookManager.Models;
using BookManager.Utility;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookManager.DataAccess.DbInitializer
{
    public class Dbinitializer : IDbinitializer
    {
        private readonly ApplicationDbContext _db;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<Dbinitializer> _logger;

        public Dbinitializer(
             ApplicationDbContext db,
             RoleManager<IdentityRole> roleManager,
             UserManager<IdentityUser> userManager,
             ILogger<Dbinitializer> logger
            )
        {
            _db = db;
            _roleManager = roleManager;
            _userManager = userManager;
            _logger = logger;
        }
        public void Initializer()
        {
            // apply all migration 
            try
            {
                if (_db.Database.GetPendingMigrations().Count() > 0)
                {
                    _db.Database.Migrate();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error: {ex.Message}");
            }

            // create role if they are not created
            if (!_roleManager.RoleExistsAsync(StaticDetail.Role_Customer).GetAwaiter().GetResult())
            {
                _roleManager.CreateAsync(new IdentityRole(StaticDetail.Role_Employee)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(StaticDetail.Role_Customer)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(StaticDetail.Role_Company)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(StaticDetail.Role_Admin)).GetAwaiter().GetResult();
                
                // if roles are not created, then will create account admin user
                _userManager.CreateAsync(new ApplicationUser
                {
                    UserName = "admin@gmail.com",
                    Email = "admin@gmail.com",
                    Name = "Dang Duy Thong",
                    PhoneNumber = "123456789",
                    Address = "25 Test",
                    Ward = "9",
                    District = "Test",
                    City = "Ho Chi Minh"
                },"Admin@123").GetAwaiter().GetResult();

                // After Create User -> Asign Role to user
                ApplicationUser applicationUser = _db.ApplicationUsers.FirstOrDefault(x => x.Email == "admin@gmail.com");
                _userManager.AddToRoleAsync(applicationUser, StaticDetail.Role_Admin).GetAwaiter().GetResult();
            }

            return;
        }
    }
}
