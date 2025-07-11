using Graduation_Project.Core.Models.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Secretary_Dashboard.MVC.Models;

namespace Secretary_Dashboard.MVC.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<AppUser> userManager;
        private readonly SignInManager<AppUser> signInManager;

        public AccountController(UserManager<AppUser> userManager,SignInManager<AppUser> signInManager)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
        }
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(SecretaryLoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                // check this email is exist or not 
                var user = await userManager.FindByEmailAsync(model.Email);
                if (user is not null)
                {
                    //if (user.EmailConfirmed is false)
                    //{
                    //    ModelState.AddModelError(string.Empty, "You need to confirm your email to log in.");
                    //    return View(model);
                    //}
                    // check password
                    var flag = await userManager.CheckPasswordAsync(user, model.Password);
                    if (flag)
                    {
                        var result = await signInManager.PasswordSignInAsync(user,model.Password,true ,false);
                        if (result.Succeeded)
                            return RedirectToAction("Index", "Home");
                    }
                }

                ModelState.AddModelError(string.Empty, "Invalid Username or Password");
            }
            return View(model);
        }

    }
}
