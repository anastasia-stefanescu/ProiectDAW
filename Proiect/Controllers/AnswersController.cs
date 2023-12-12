using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Proiect.Data;
using Proiect.Models;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Proiect.Controllers
{
    public class AnswersController : Controller
    {
        private readonly ApplicationDbContext db;
        public AnswersController(ApplicationDbContext context)
        {
            db = context;
        }

        
        //de vazut daca e corect
        // Adaugarea unui comentariu asociat unui articol in baza de date
        [HttpPost]
        public IActionResult New(Answer ans)
        {
            ans.Date = DateTime.Now;

            if(ModelState.IsValid)
            {
                db.Answers.Add(ans);
                db.SaveChanges();
                return Redirect("/Subjects/Show/" + ans.SubjectId);
            }

            else
            {
                return Redirect("/Subjects/Show/" + ans.SubjectId);
            }

        }

        // Stergerea unui raspuns asociat unui subiect din baza de date
        [HttpPost]
        public IActionResult Delete(int id)
        {
            Answer ans = db.Answers.Find(id);
            db.Answers.Remove(ans);
            db.SaveChanges();
            return Redirect("/Subjects/Show/" + ans.SubjectId);
        }

        // In acest moment vom implementa editarea intr-o pagina View separata
        // Se editeaza un raspuns existent

        public IActionResult Edit(int id)
        {
            Answer ans = db.Answers.Find(id);

            return View(ans);
        }

        [HttpPost]
        public IActionResult Edit(int id, Answer requestAnswer)
        {
            Answer ans = db.Answers.Find(id);

            if (ModelState.IsValid)
            {

                ans.Content = requestAnswer.Content;

                //sa poate edita si lista de taguri

                db.SaveChanges();

                return Redirect("/Subjects/Show/" + ans.SubjectId);
            }
            else
            {
                return View(requestAnswer);
            }

        }
    }
}

