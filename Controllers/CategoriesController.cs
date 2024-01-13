using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Humanizer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Proiect.Data;
using Proiect.Models;
using static NuGet.Packaging.PackagingConstants;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Proiect.Controllers
{
    public class CategoriesController : Controller
    {
        private readonly ApplicationDbContext db;

        public CategoriesController(ApplicationDbContext context)
        {
            db = context;
        }


        public ActionResult Index()
        {
            if (TempData.ContainsKey("message"))
            {
                ViewBag.message = TempData["message"].ToString();
                ViewBag.Alert = TempData["messageType"];
            }

            var categories = from category in db.Categories
                             orderby category.CategoryName
                             select category;
            ViewBag.Categories = categories;
            return View();
        }

        public ActionResult Show(int id)
        {
            if (TempData.ContainsKey("message"))
            {
                ViewBag.message = TempData["message"].ToString();
                ViewBag.Alert = TempData["messageType"];
            }

            Category category = db.Categories.Include("Subjects")
                                         .Include("Subjects.User")
                                         .Where(category => category.Id == id)
                                         .First();
            var subjectsofCateg = category.Subjects;


            //////////////////////////////////////////////////////
            ///Partea de cautare
            ///

            var search = "";

            // MOTOR DE CAUTARE
            if (Convert.ToString(HttpContext.Request.Query["search"]) != null)
            {
                // eliminam spatiile libere
                search = Convert.ToString(HttpContext.Request.Query["search"]).Trim();

                // Cautare in articol (Title si Content)
                List<int> subjectIds = subjectsofCateg.Where(
                                                 sub => sub.Title.Contains(search)
                                                 || sub.Content.Contains(search)
                                                            ).Select(a => a.Id).ToList();

                // Cautare in raspunsuri
                //selectam raspunsurile
                var allAnswers = db.Answers;
                var answersOfSubj = db.Answers.Where(
                                            answer => subjectsofCateg.Contains(answer.Subject)
                                            ).ToList();

                List<int> subjectIdsOfAnswersWithSearchString = answersOfSubj
                .Where
                (
                c => c.Content.Contains(search)
                ).Select(c => (int)c.SubjectId).ToList();

                // Se formeaza o singura lista formata din toate id-urile selectate anterior
                List<int> mergedIds = subjectIds.Union(subjectIdsOfAnswersWithSearchString).ToList();

                // Lista subiect care contin cuvantul cautat
                // fie in articol -> Title si Content
                // fie in raspunsuri -> Content

                //aici de vazut!!!!!
                subjectsofCateg = category.Subjects
                                    .Where(subject => mergedIds.Contains(subject.Id))
                                    .OrderBy(s => s.Date)
                                    //.Include(s => s.Category)
                                    //.Include(s => s.User)
                                    .ToList();
            }
            ViewBag.SearchString = search;

            // SORTARE
            var sortOrder = Convert.ToString(HttpContext.Request.Query["sortOrder"]);
            switch (sortOrder)
            {
                case "date_desc":
                    subjectsofCateg = subjectsofCateg.OrderByDescending(s => s.Date).ToList();
                    break;
                case "date_asc":
                    subjectsofCateg = subjectsofCateg.OrderBy(s => s.Date).ToList();
                    break;
                case "alphabetically_asc":
                    subjectsofCateg = subjectsofCateg.OrderBy(s => s.Title).ToList();
                    break;
                case "alphabetically_desc":
                    subjectsofCateg = subjectsofCateg.OrderByDescending(s => s.Title).ToList();
                    break;
                default:
                    subjectsofCateg = subjectsofCateg.OrderByDescending(s => s.Date).ToList();
                    break;
            }

            // AFISARE PAGINATA

            int _perPage = 3;

            // Fiind un numar variabil , verificam de fiecare data utilizand metoda Count()
            int totalItems = subjectsofCateg.Count();

            // Se preia pagina curenta din View-ul asociat
            // Numarul paginii este valoarea parametrului page din ruta
            // /Category/Show?page=valoare
            var currentPage = Convert.ToInt32(HttpContext.Request.Query["page"]);

            // Pentru prima pagina offsetul o sa fie zero
            // Pentru pagina 2 o sa fie 3
            var offset = 0;

            // Se calculeaza offsetul in functie de numarul paginii la care suntem
            if (!currentPage.Equals(0))
            {
                offset = (currentPage - 1) * _perPage;
            }

            var paginatedSubjects = subjectsofCateg.Skip(offset).Take(_perPage);

            // Preluam numarul ultimei pagini
            ViewBag.lastPage = Math.Ceiling((float)totalItems / (float)_perPage);

            ViewBag.Subjects = paginatedSubjects;

            //////end PAGINARE

            // ATENTIE AICI AM SCHIMBAT RUTA!!!!!!
            if (search != "")
            {
                if (sortOrder == null)
                {
                    ViewBag.PaginationBaseUrl = "/Categories/Show/" + id + "?search=" + search + "&page";
                }
                else
                {
                    ViewBag.PaginationBaseUrl = "/Categories/Show/" + id + "?sortOrder=" + sortOrder +  "&search=" + search + "&page";
                }
            }
            else
            {
                ViewBag.PaginationBaseUrl = "/Categories/Show/" + id + "?sortOrder=" + sortOrder + "&page";
            }

            /////////
            return View(category); 
        }

        [Authorize(Roles = "Admin")]
        public ActionResult New()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult New(Category cat)
        {
            if (ModelState.IsValid)
            {
                db.Categories.Add(cat);
                db.SaveChanges();
                TempData["message"] = "Categoria a fost adaugata";
                return RedirectToAction("Index");
            }

            else
            {
                return View(cat);
            }
        }

        [Authorize(Roles = "Admin")]
        public ActionResult Edit(int id)
        {
            Category category = db.Categories.Find(id);
            return View(category);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult Edit(int id, Category requestCategory)
        {
            Category category = db.Categories.Find(id);

            if (ModelState.IsValid)
            {

                category.CategoryName = requestCategory.CategoryName;
                db.SaveChanges();
                TempData["message"] = "Categoria a fost modificata!";
                return RedirectToAction("Index");
            }
            else
            {
                return View(requestCategory);
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult Delete(int id)
        {
            Category category = db.Categories.Include("Subjects")
                                         .Where(category => category.Id == id)
                                         .First();

            var subjectsOfCateg = category.Subjects;
            var allAnswers = db.Answers;

            var answersOfSubj = from s in subjectsOfCateg
                                join a in allAnswers on
                                s.Id equals a.SubjectId
                                select a;

            foreach(var answer in answersOfSubj)
            {
                db.Answers.Remove(answer);
            }

            db.Categories.Remove(category);
            TempData["message"] = "Categoria a fost stearsa";
            db.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}