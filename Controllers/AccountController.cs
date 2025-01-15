using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using MongoWebGallery.Models;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace MongoWebGallery.Controllers
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

        private async Task<ApplicationUser> GetCurrentUserAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                throw new UnauthorizedAccessException("Użytkownik niezalogowany.");
            }
            return user;
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
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
                ModelState.AddModelError(nameof(email), "Nieprawidłowy email lub hasło.");
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
            try
            {
                var user = await GetCurrentUserAsync();
                var userImages = await _imageCollection.Find(img => img.UserId == user.Id).ToListAsync();
                return View(userImages);
            }
            catch (UnauthorizedAccessException)
            {
                return RedirectToAction("Login", "Account");
            }
        }

        [Authorize]
        public async Task<IActionResult> Settings()
        {
            try
            {
                var user = await GetCurrentUserAsync();

                var model = new UserSettingsViewModel
                {
                    Name = user.UserName,
                    Email = user.Email
                };

                return View(model);
            }
            catch (UnauthorizedAccessException)
            {
                return RedirectToAction("Login", "Account");
            }
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Settings(UserSettingsViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var user = await GetCurrentUserAsync();

                    if (user.UserName != model.Name)
                    {
                        var existingUserByName = await _userManager.FindByNameAsync(model.Name);
                        if (existingUserByName != null)
                        {
                            ModelState.AddModelError("Name", "Nazwa użytkownika jest już zajęta.");
                            return View(model);
                        }
                        user.UserName = model.Name;
                    }

                    if (user.Email != model.Email)
                    {
                        var existingUserByEmail = await _userManager.FindByEmailAsync(model.Email);
                        if (existingUserByEmail != null)
                        {
                            ModelState.AddModelError("Email", "Email jest już zajęty.");
                            return View(model);
                        }
                        await _userManager.SetEmailAsync(user, model.Email);
                    }

                    if (!string.IsNullOrWhiteSpace(model.NewPassword))
                    {
                        var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
                        if (!result.Succeeded)
                        {
                            foreach (var error in result.Errors)
                            {
                                ModelState.AddModelError("", error.Description);
                            }
                            return View(model);
                        }
                    }

                    await _userManager.UpdateAsync(user);
                    ViewBag.Message = "Ustawienia zostały zaktualizowane pomyślnie.";
                    return RedirectToAction("Profile", "Account");
                }
                catch (UnauthorizedAccessException)
                {
                    return RedirectToAction("Login", "Account");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Wystąpił błąd: {ex.Message}");
                }
            }

            return View(model);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> UpdateUserName([Required] string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                ModelState.AddModelError("Name", "Nazwa użytkownika nie może być pusta.");
                return RedirectToAction("Settings");
            }

            try
            {
                var user = await GetCurrentUserAsync();

                if (user.UserName != name)
                {
                    var existingUserByName = await _userManager.FindByNameAsync(name);
                    if (existingUserByName != null)
                    {
                        ModelState.AddModelError("Name", "Nazwa użytkownika jest już zajęta.");
                        return RedirectToAction("Settings");
                    }

                    user.UserName = name;
                    await _userManager.UpdateAsync(user);
                    ViewBag.Message = "Nazwa użytkownika została zaktualizowana pomyślnie.";
                }
            }
            catch (UnauthorizedAccessException)
            {
                return RedirectToAction("Login", "Account");
            }

            return RedirectToAction("Settings");
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> UpdatePassword([Required] string currentPassword, [Required] string newPassword)
        {
            if (string.IsNullOrWhiteSpace(currentPassword) || string.IsNullOrWhiteSpace(newPassword))
            {
                ModelState.AddModelError("", "Obecne i nowe hasło są wymagane.");
                return RedirectToAction("Settings");
            }

            try
            {
                var user = await GetCurrentUserAsync();

                var changePasswordResult = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
                if (!changePasswordResult.Succeeded)
                {
                    foreach (var error in changePasswordResult.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                    return RedirectToAction("Settings");
                }

                ViewBag.Message = "Hasło zostało zaktualizowane pomyślnie.";
            }
            catch (UnauthorizedAccessException)
            {
                return RedirectToAction("Login", "Account");
            }

            return RedirectToAction("Settings");
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> DeleteAccount()
        {
            try
            {
                var user = await GetCurrentUserAsync();

                await _imageCollection.DeleteManyAsync(img => img.UserId == user.Id);

                var result = await _userManager.DeleteAsync(user);
                if (!result.Succeeded)
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                    return RedirectToAction("Settings");
                }

                await _signInManager.SignOutAsync();
                return RedirectToAction("Index", "Home");
            }
            catch (UnauthorizedAccessException)
            {
                return RedirectToAction("Login", "Account");
            }
        }
    }
}
