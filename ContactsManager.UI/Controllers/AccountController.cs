﻿using ContactsManager.Core.Domain.IdentityEntities;
using ContactsManager.Core.DTO;
using ContactsManager.Core.Enums;

using CRUDExample.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ContactsManager.UI.Controllers
{
    [Route("[controller]/[action]")]
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<ApplicationRole> _roleManager;

        public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, RoleManager<ApplicationRole> roleManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
        }

        [HttpGet]
        [Authorize("NotAuthorized")]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [Authorize("NotAuthorized")]
        //[ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterDTO registerDTO)
        {
            if (ModelState.IsValid == false)
            {
                ViewBag.Errors = ModelState.Values
                    .SelectMany(temp => temp.Errors)
                    .Select(temp => temp.ErrorMessage);

                return View(registerDTO);
            }

            ApplicationUser user = new ApplicationUser()
            {
                Email = registerDTO.Email,
                PhoneNumber = registerDTO.Phone,
                UserName = registerDTO.Email,
                PersonName = registerDTO.PersonName
            };

            IdentityResult result = await _userManager.CreateAsync(
                user: user,
                password: registerDTO.Password);

            if (result.Succeeded)
            {
                if (registerDTO.UserType == UserTypeOptions.Admin)
                {
                    if (await _roleManager.FindByNameAsync(UserTypeOptions.Admin.ToString()) is null)
                    {
                        ApplicationRole applicationRole = new ApplicationRole() { Name = UserTypeOptions.Admin.ToString() };
                        await _roleManager.CreateAsync(applicationRole);
                    }

                    // Add the new user into the 'Admin' role
                    await _userManager.AddToRoleAsync(user, UserTypeOptions.Admin.ToString());
                }
                else
                {
                    if (await _roleManager.FindByNameAsync(UserTypeOptions.User.ToString()) is null)
                    {
                        ApplicationRole applicationRole = new ApplicationRole() { Name = UserTypeOptions.User.ToString() };
                        await _roleManager.CreateAsync(applicationRole);
                    }
                    // Add the new user into 'User' role
                    await _userManager.AddToRoleAsync(user, UserTypeOptions.User.ToString());
                }

                await _signInManager.SignInAsync(
                    user: user,
                    isPersistent: false);

                return RedirectToAction(
                    actionName: nameof(PersonsController.Index),
                    controllerName: "Persons");
            }
            else
            {
                foreach (IdentityError error in result.Errors)
                {
                    ModelState.AddModelError(
                        key: "Register",
                        errorMessage: error.Description);
                }

                return View(registerDTO);
            }
        }

        [HttpGet]
        [Authorize("NotAuthorized")]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [Authorize("NotAuthorized")]
        public async Task<IActionResult> Login(LoginDTO loginDTO, string? ReturnUrl)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Errors = ModelState.Values
                    .SelectMany(temp => temp.Errors)
                    .Select(temp => temp.ErrorMessage);

                return View(loginDTO);
            }

            if (loginDTO.Email == null || loginDTO.Password == null)
            {
                ModelState.AddModelError(
                    key: "Login",
                    errorMessage: "Email and password can't be blank");
                return View(loginDTO);
            }

            var result = await _signInManager.PasswordSignInAsync(
                userName: loginDTO.Email,
                password: loginDTO.Password,
                isPersistent: false,
                lockoutOnFailure: false);

            if (result.Succeeded)
            {
                // Admin
                ApplicationUser? user = await _userManager.FindByEmailAsync(loginDTO.Email);

                if (user != null)
                {
                    if (await _userManager.IsInRoleAsync(user, UserTypeOptions.Admin.ToString()))
                    {
                        return RedirectToAction("Index", "Home", new { area = "Admin" });
                    }
                }

                if (!string.IsNullOrEmpty(ReturnUrl) && Url.IsLocalUrl(ReturnUrl))
                {
                    return LocalRedirect(ReturnUrl);
                }
                return RedirectToAction(
                    actionName: nameof(PersonsController.Index),
                    controllerName: "Persons");
            }

            ModelState.AddModelError(
                key: "Login",
                errorMessage: "Invalid email or password");

            return View(loginDTO);
        }

        [Authorize()]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction(
                actionName: nameof(AccountController.Login),
                controllerName: "Account");
        }

        public async Task<IActionResult> IsEmailAlreadyRegistered(string email)
        {
            ApplicationUser? user = await _userManager.FindByEmailAsync(email);

            if (user == null)
            {
                return Json(true); // valid
            }
            else
            {
                return Json(false); // invalid email address (already exists)
            }
        }
    }
}