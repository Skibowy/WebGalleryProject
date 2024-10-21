using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using WebGalleryProject.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using WebGalleryProject.Models;

namespace WebGalleryProject.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IMongoCollection<Image> _imageCollection;

        public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IMongoCollection<Image> imageCollection)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _imageCollection = imageCollection;
        }

        public IActionResult Login()
        {
            return View();
        }

        //zrezygnowac?
        public IActionResult AccessDenied()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login([Required][EmailAddress] string email, [Required] string password)
        {
            if (ModelState.IsValid)
            {
                ApplicationUser appUser = await _userManager.FindByEmailAsync(email);
                if (appUser != null)
                {
                    var result = await _signInManager.PasswordSignInAsync(appUser, password, false, false);
                    if (result.Succeeded)
                    {
                        return RedirectToAction("Profile", "Account");
                    }
                }
                ModelState.AddModelError(nameof(email), "Login Failed: Invalid Email or Password");
            }
            return View();
        }

        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        [Authorize]
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User); // Uzyskaj obiekt użytkownika

            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var userId = user.Id; // Id to Guid

            var userImages = await _imageCollection.Find(img => img.UserId == userId).ToListAsync();
            return View(userImages);
        }

        [Authorize]
        public async Task<IActionResult> Settings()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var model = new UserSettingsViewModel
            {
                Name = user.UserName,
                Email = user.Email
            };

            return View(model);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Settings(UserSettingsViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return RedirectToAction("Login", "Account");
                }

                // Zmiana nazwy użytkownika
                if (user.UserName != model.Name)
                {
                    var existingUserByName = await _userManager.FindByNameAsync(model.Name);
                    if (existingUserByName != null)
                    {
                        ModelState.AddModelError("Name", "User name is already in use.");
                        return View(model);
                    }
                    user.UserName = model.Name;
                }

                // Zmiana adresu email
                if (user.Email != model.Email)
                {
                    var existingUserByEmail = await _userManager.FindByEmailAsync(model.Email);
                    if (existingUserByEmail != null)
                    {
                        ModelState.AddModelError("Email", "Email is already in use.");
                        return View(model);
                    }
                    await _userManager.SetEmailAsync(user, model.Email); // Użyj SetEmailAsync
                }

                // Zmiana hasła
                if (!string.IsNullOrWhiteSpace(model.NewPassword))
                {
                    var passwordValidator = new PasswordValidator<ApplicationUser>();
                    var passwordValidationResult = await passwordValidator.ValidateAsync(_userManager, user, model.NewPassword);
                    if (passwordValidationResult.Succeeded)
                    {
                        await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
                    }
                    else
                    {
                        foreach (var error in passwordValidationResult.Errors)
                        {
                            ModelState.AddModelError("", error.Description);
                        }
                        return View(model);
                    }
                }

                await _userManager.UpdateAsync(user); // Uaktualnij użytkownika
                ViewBag.Message = "Settings updated successfully";
                return RedirectToAction("Profile", "Account");
            }

            return View(model);
        }

    }
}
