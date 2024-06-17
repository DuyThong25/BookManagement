// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using BookManager.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BookManagementWeb.Areas.Identity.Pages.Account.Manage
{
    public class IndexModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;

        public IndexModel(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [TempData]
        public string StatusMessage { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public class InputModel
        {
            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Phone(ErrorMessage = "The Phone number field is not a valid phone number.")]
            [Display(Name = "Phone number")]
            public string PhoneNumber { get; set; }

            [Required]
            [Display(Name = "Full name")]
            public string Name { get; set; }
            [Display(Name = "Date of Birth")]
            public DateTime? BirthDay { get; set; }

            public string? Address { get; set; }
            public string? Ward { get; set; }
            public string? District { get; set; }
            public string? City { get; set; }


        }

        private async Task LoadAsync(IdentityUser user)
        {
            ApplicationUser applicationUser = (ApplicationUser)await _userManager.FindByIdAsync(user.Id);
            //var userName = await _userManager.GetUserNameAsync(user);
            //var phoneNumber = await _userManager.GetPhoneNumberAsync(user);

            var userName = applicationUser.UserName;
            var phoneNumber = applicationUser.PhoneNumber;

            Username = userName;
            Input = new InputModel
            {
                PhoneNumber = phoneNumber,
                Name = applicationUser.Name,
                Address = applicationUser.Address,
                Ward = applicationUser.Ward,
                District = applicationUser.District,
                City = applicationUser.City,
                BirthDay = applicationUser.BirthDay,
            };
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            await LoadAsync(user);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            //var user = await _userManager.GetUserAsync(User);
            ApplicationUser user = (ApplicationUser)await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (!ModelState.IsValid)
            {
                await LoadAsync(user);
                return Page();
            }

            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
            if (Input.PhoneNumber != phoneNumber)
            {
                var setPhoneResult = await _userManager.SetPhoneNumberAsync(user, Input.PhoneNumber);
                if (!setPhoneResult.Succeeded)
                {
                    StatusMessage = "Unexpected error when trying to set phone number.";
                    return RedirectToPage();
                }
            }

            if (Input.Name != user.Name)
            {
                user.Name = Input.Name;
            }

            if (Input.Address != user.Address)
            {
                user.Address = Input.Address;
            }

            if (Input.Ward != user.Ward)
            {
                user.Ward = Input.Ward;
            }

            if (Input.District != user.District)
            {
                user.District = Input.District;
            }

            if (Input.City != user.City)
            {
                user.City = Input.City;
            }

            if (Input.BirthDay != user.BirthDay)
            {
                if ((DateTime.Now.Year - DateTime.Parse(Input.BirthDay.ToString()).Year ) < 12 )
                {
                    ModelState.AddModelError("Input.BirthDay", "Must be 12+ years old");
                    return Page();
                }
                else
                {
                    user.BirthDay = Input.BirthDay;  
                }
            }
            try
            {
                await _userManager.UpdateAsync(user);
            }
            catch
            {
                StatusMessage = "Unexpected error when trying to update your profile.";
                return RedirectToPage();

            }
            await _signInManager.RefreshSignInAsync(user);
            StatusMessage = "Your profile has been updated";
            return RedirectToPage();
        }
    }
}
