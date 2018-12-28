using Laba3_4.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Laba3_4.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UserController : Controller
    {
        private ApplicationUserManager userManager;
        private IEnumerable<ApplicationUser> users;

        public ApplicationUserManager UserManager
        {
            get
            {
                return userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                userManager = value;
            }
        }

        public ActionResult Index()
        {
            users = new List<ApplicationUser>(UserManager.Users);
            return View(users);
        }

        public ActionResult Edit(String id)
        {
            ApplicationUser user = UserManager.FindByIdAsync(id).Result;
            EditUser editUserForm = new EditUser()
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Patronymic = user.Patronymic,
                Email = user.Email == null ? user.UserName : user.Email
            };
            return View(editUserForm);
        }

        [HttpPost]
        public ActionResult Edit(EditUser editedUser)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    ApplicationUser user = UserManager.FindByIdAsync(editedUser.Id).Result;
                    user.FirstName = editedUser.FirstName;
                    user.LastName = editedUser.LastName;
                    user.Patronymic = editedUser.Patronymic;
                    user.Email = editedUser.Email;
                    UserManager.UpdateUser(user);
                    return RedirectToAction("Index");
                }
                catch
                {
                    return View(editedUser);
                }
            }
            return View(editedUser);
        }

        public ActionResult Delete(String id)
        {
            ApplicationUser user = UserManager.FindByIdAsync(id).Result;
            UserManager.DeleteUser(user);
            return RedirectToAction("Index", "User");
        }

        public ActionResult ToggleAdmin(String id)
        {
            ApplicationUser user = UserManager.FindByIdAsync(id).Result;
            IEnumerable<string> userRoles = UserManager.GetRoles(user.Id);
            if (userRoles.Contains("Admin"))
                UserManager.RemoveFromRole(user.Id, "Admin");
            else
                UserManager.AddToRole(user.Id, "Admin");
            return RedirectToAction("Index", "User");
        }
    }
}