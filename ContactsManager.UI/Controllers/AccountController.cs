using ContactsManager.Core.DTO;

using CRUDExample.Controllers;

using Microsoft.AspNetCore.Mvc;

namespace ContactsManager.UI.Controllers
{
    [Route("[controller]/[action]")]
    public class AccountController : Controller
    {
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Register(RegisterDTO registerDTO)
        {
            // TO DO: Store the user registration details in the identity database
            return RedirectToAction(
                actionName: nameof(PersonsController.Index),
                controllerName: "Persons");
        }
    }
}