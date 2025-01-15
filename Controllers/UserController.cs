using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MongoWebGallery.Models;

namespace MongoWebGallery.Controllers
{
    public class UserController : Controller
    {
        private UserManager<ApplicationUser> _userManager;
        private RoleManager<ApplicationRole> _roleManager;
        public UserController(UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public IActionResult Create()
        {
            return View();
        }

        public IActionResult CreateRole()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(User user)
        {
            if (ModelState.IsValid)
            {
                var existingUserByEmail = await _userManager.FindByEmailAsync(user.Email);
                if (existingUserByEmail != null)
                {
                    ModelState.AddModelError("Email", "Adres e-mail jest już w użyciu.");
                    return View(user);
                }

                var existingUserByName = await _userManager.FindByNameAsync(user.Name);
                if (existingUserByName != null)
                {
                    ModelState.AddModelError("UserName", "Nazwa użytkownika jest już w użyciu.");
                    return View(user);
                }

                ApplicationUser appUser = new ApplicationUser
                {
                    UserName = user.Name,
                    Email = user.Email
                };

                IdentityResult result = await _userManager.CreateAsync(appUser, user.Password);

                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(appUser, "Admin");
                    ViewBag.Message = "Użytkownik został pomyślnie utworzony.";
                }
                else
                {
                    foreach (IdentityError error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                }
            }
            return View(user);
        }

        [HttpPost]
        public async Task<IActionResult> CreateRole(UserRole userRole)
        {
            if (ModelState.IsValid)
            {
                bool roleExists = await _roleManager.RoleExistsAsync(userRole.RoleName);
                if (roleExists)
                {
                    ModelState.AddModelError("RoleName", "Rola o takiej nazwie już istnieje.");
                    return View(userRole);
                }

                IdentityResult result = await _roleManager.CreateAsync(new ApplicationRole() { Name = userRole.RoleName });
                if (result.Succeeded)
                {
                    ViewBag.Message = "Rola została pomyślnie utworzona.";
                }
                else
                {
                    foreach (IdentityError error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                }
            }
            return View(userRole);
        }

        [HttpGet]
        public async Task<IActionResult> GetUserName(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            return Json(user?.UserName ?? "Nieznany");
        }
    }
}
