using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Proiect.Data;
using Proiect.Models;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Proiect.Controllers
{
    public class AnswersController : Controller
    {
        private readonly ApplicationDbContext db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        public AnswersController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            db = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // Stergerea unui raspuns asociat unui subiect din baza de date
        [HttpPost]
        [Authorize(Roles = "User,Admin")]
        public IActionResult Delete(int id)
        {
            Answer ans = db.Answers.Find(id);
            if (ans.UserId == _userManager.GetUserId(User) || User.IsInRole("Admin"))
            {
                db.Answers.Remove(ans);
                db.SaveChanges();
                return Redirect("/Subjects/Show/" + ans.SubjectId);
            }
            else
            {
                TempData["message"] = "Nu aveti dreptul sa stergeti un raspuns care nu va apartine";
                TempData["messageType"] = "alert-danger";
                return Redirect("/Subjects/Show/" + ans.SubjectId);
            }
        }

        // In acest moment vom implementa editarea intr-o pagina View separata
        // Se editeaza un raspuns existent
        [Authorize(Roles = "User,Admin")]
        public IActionResult Edit(int id)
        {
            Answer ans = db.Answers.Find(id);

            if (ans.UserId == _userManager.GetUserId(User))
            {
                return View(ans);
            }
            else
            {
                TempData["message"] = "Nu aveti dreptul sa editati un raspuns care nu va apartine";
                TempData["messageType"] = "alert-danger";
                return Redirect("/Subjects/Show/" + ans.SubjectId);
            }
        }

        [HttpPost]
        [Authorize(Roles = "User,Admin")]
        public IActionResult Edit(int id, Answer requestAnswer)
        {
            Answer ans = db.Answers.Find(id);

            if (ModelState.IsValid)
            {
                if (ans.UserId == _userManager.GetUserId(User))
                {
                    ans.Content = requestAnswer.Content;

                    //sa poate edita si lista de taguri

                    db.SaveChanges();

                    return Redirect("/Subjects/Show/" + ans.SubjectId);
                }
                else
                {
                    TempData["message"] = "Nu aveti dreptul sa editati un raspuns care nu va apartine";
                    TempData["messageType"] = "alert-danger";
                    return Redirect("/Subjects/Show/" + ans.SubjectId);
                }
            }
            else
            {
                return View(requestAnswer);
            }

        }
    }
}