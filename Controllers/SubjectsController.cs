using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Proiect.Data;
using Proiect.Models;
using System.Data;
using Microsoft.EntityFrameworkCore;

namespace Proiect.Controllers
{
    public class SubjectsController : Controller
    {
        // Se afiseaza lista tuturor subiectelor 
        // HttpGet implicit
        //[Authorize(Roles = "User,Editor,Admin")]

        // Se afiseaza un singur articol in functie de id-ul sau 
        // impreuna cu categoria din care face parte
        // In plus sunt preluate si toate comentariile asociate unui articol
        // Se afiseaza si userul care a postat articolul respectiv
        // HttpGet
        //
        private readonly ApplicationDbContext db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        public SubjectsController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            db = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public IActionResult Show(int id)
        {
            Subject subject = db.Subjects.Include("Category")
                                         .Include("User")
                                         .Include("Answers")
                                         .Include("Answers.User")
                                         .Where(subject => subject.Id == id)
                                         .First();

            var answersOfSubject = subject.Answers;

            // SORTARE
            var sortOrder = Convert.ToString(HttpContext.Request.Query["sortOrder"]);
            switch (sortOrder)
            {
                case "date_desc":
                    answersOfSubject = answersOfSubject.OrderByDescending(a => a.Date).ToList();
                    break;
                case "date_asc":
                    answersOfSubject = answersOfSubject.OrderBy(a => a.Date).ToList();
                    break;
                case "alphabetically_asc":
                    answersOfSubject = answersOfSubject.OrderBy(a => a.Content).ToList();
                    break;
                case "alphabetically_desc":
                    answersOfSubject = answersOfSubject.OrderByDescending(a => a.Content).ToList();
                    break;
                default:
                    answersOfSubject = answersOfSubject.OrderByDescending(a => a.Content).ToList();
                    break;
            }
            ViewBag.Answers = answersOfSubject;

            if (TempData.ContainsKey("message"))
            {
                ViewBag.Message = TempData["message"];
                ViewBag.Alert = TempData["messageType"];
            }

            SetAccessRights();

            return View(subject);
        }

        // Adaugarea unui raspuns asociat unui subiect in baza de date
        [HttpPost]
        [Authorize(Roles = "User,Admin")]
        public IActionResult Show([FromForm] Answer answer)
        {
            answer.Date = DateTime.Now;
            answer.UserId = _userManager.GetUserId(User);

            if (ModelState.IsValid)
            {
                db.Answers.Add(answer);
                db.SaveChanges();
                return Redirect("/Subjects/Show/" + answer.SubjectId);
            }
            else
            {
                Subject subject = db.Subjects.Include("Category")
                                         .Include("User")
                                         .Include("Answers")
                                         .Include("Answers.User")
                               .Where(subject => subject.Id == answer.SubjectId)
                               .First();

                //return Redirect("/Articles/Show/" + comm.ArticleId);
                SetAccessRights();

                return View(subject);
            }
        }

        // Se afiseaza formularul in care se vor completa datele unui subiect
        // impreuna cu selectarea categoriei din care face parte si tagurile
        // Doar utilizatorii cu rolul de Utilizator inregistrat sau Admin pot adauga articole in platforma
        // HttpGet implicit

        [Authorize(Roles = "User,Admin")]
        public IActionResult New()
        {
            Subject subject = new Subject();

            // Se preia lista de categorii cu ajutorul metodei GetAllCategories()
            subject.Categ = GetAllCategories();

            return View(subject);
        }

        // Se adauga articolul in baza de date
        // Doar utilizatorii cu rolul de utilizator inregistrat sau Admin pot adauga articole in platforma

        [Authorize(Roles = "User,Admin")]
        [HttpPost]
        public IActionResult New(Subject subject)
        {
            subject.Date = DateTime.Now;
            subject.Categ = GetAllCategories();
            // preluam id-ul utilizatorului care posteaza articolul
            subject.UserId = _userManager.GetUserId(User);


            if (ModelState.IsValid)
            {
                db.Subjects.Add(subject);
                db.SaveChanges();
                TempData["message"] = "Subiectul a fost adaugat";
                TempData["messageType"] = "alert-success";
                return Redirect("/Categories/Show/" + subject.CategoryId);
            }
            else
            {

                subject.Categ = GetAllCategories();
                //de luat tagurile: la editare exista optiunea sa 
                return View(subject);
            }
        }

        // Se editeaza un subiect existent in baza de date impreuna cu
        // categoria din care face parte (si tagurile)
        // Categoria se selecteaza dintr-un dropdown
        // Se afiseaza formularul impreuna cu datele aferente articolului din baza de date
        // Doar utilizatorii care au creat subiectul sau Admin pot edita
        // subiectul respectiv
        // Adminii pot edita orice subiect din baza de date
        // HttpGet implicit

        [Authorize(Roles = "User,Admin")]
        public IActionResult Edit(int id)
        {

            Subject subject = db.Subjects.Include("Category")//.Include("Tags")
                                        .Where(subject => subject.Id == id)
                                        .First();

            subject.Categ = GetAllCategories();

            //de adaugat tag

            if (subject.UserId == _userManager.GetUserId(User))
            {
                return View(subject);
            }

            else
            {
                TempData["message"] = "Nu aveti dreptul sa faceti modificari asupra unui subiect care nu va apartine";
                TempData["messageType"] = "alert-danger";
                return Redirect("/Categories/Show/" + subject.CategoryId);
            }

        }

        // Se adauga subiectul de discutie modificat in baza de date
        // Verificam rolul utilizatorilor care au dreptul sa editeze (Creatorul sau Admin)        [HttpPost]
        [Authorize(Roles = "User,Admin")]
        [HttpPost]
        public IActionResult Edit(int id, Subject requestSubject)
        {
            Subject subject = db.Subjects.Find(id);
            requestSubject.Categ = GetAllCategories();

            if (ModelState.IsValid)
            {
                if (subject.UserId == _userManager.GetUserId(User))
                {
                    subject.Title = requestSubject.Title;
                    subject.Content = requestSubject.Content;
                    //taguri
                    subject.CategoryId = requestSubject.CategoryId;
                    TempData["message"] = "Subiectul a fost modificat";
                    TempData["messageType"] = "alert-success";
                    db.SaveChanges();
                    return Redirect("/Categories/Show/" + subject.CategoryId);
                }
                else
                {
                    TempData["message"] = "Nu aveti dreptul sa faceti modificari asupra unui subiect care nu va apartine";
                    TempData["messageType"] = "alert-danger";
                    return Redirect("/Categories/Show/" + subject.CategoryId);
                }
            }
            else
            {
                requestSubject.Categ = GetAllCategories();
                return View(requestSubject);
            }
        }



        [Authorize(Roles = "Admin")]
        public IActionResult ChangeCategory(int id)
        {

            Subject subject = db.Subjects.Include("Category")
                                        .Where(subject => subject.Id == id)
                                        .First();

            subject.Categ = GetAllCategories();

            if (User.IsInRole("Admin"))
            {
                return View(subject);
            }

            else
            {
                TempData["message"] = "Nu aveti dreptul sa schimbati categoria daca nu sunteti admin";
                TempData["messageType"] = "alert-danger";
                return Redirect("/Subject/Show/" + subject.Id);
            }

        }

        // Se adauga subiectul de discutie modificat in baza de date
        // Verificam rolul utilizatorilor care au dreptul sa editeze (Creatorul sau Admin)        [HttpPost]
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public IActionResult ChangeCategory(int id, Subject requestSubject)
        {
            Subject subject = db.Subjects.Find(id);
            requestSubject.Categ = GetAllCategories();

            if (ModelState.IsValid)
            {
                if (User.IsInRole("Admin"))
                {
                    subject.Title = subject.Title;
                    subject.Content = subject.Content;
                    subject.CategoryId = requestSubject.CategoryId;
                    TempData["message"] = "Subiectul a fost modificat";
                    TempData["messageType"] = "alert-success";
                    db.SaveChanges();
                    return Redirect("/Subject/Show/" + subject.Id);
                }
                else
                {
                    TempData["message"] = "Nu aveti dreptul sa schimbati categoria daca nu sunteti admin";
                    TempData["messageType"] = "alert-danger";
                    return Redirect("/Subject/Show/" + subject.Id);
                }
            }
            else
            {
                requestSubject.Categ = GetAllCategories();
                return View(requestSubject);
            }
        }




        // Se sterge un articol din baza de date
        // Utilizatorii cu rolul de User sau Admin pot sterge articole
        // Userii pot sterge doar articolele publicate de ei
        // Adminii pot sterge orice articol din baza de date

        [HttpPost]
        [Authorize(Roles = "User,Admin")]
        public ActionResult Delete(int id)
        {
            Subject subject = db.Subjects.Include("Answers")
                                         .Where(subject => subject.Id == id)
                                         .First();

            if (subject.UserId == _userManager.GetUserId(User) || User.IsInRole("Admin"))
            {
                db.Subjects.Remove(subject);
                db.SaveChanges();
                TempData["message"] = "Subiectul a fost sters";
                TempData["messageType"] = "alert-success";
                return Redirect("/Categories/Show/" + subject.CategoryId);
            }
            else
            {
                TempData["message"] = "Nu aveti dreptul sa stergeti un subiect care nu va apartine";
                TempData["messageType"] = "alert-danger";
                return Redirect("/Categories/Show/" + subject.CategoryId);
            }
        }


        // Conditiile de afisare a butoanelor de editare si stergere
        private void SetAccessRights()
        {
            ViewBag.AfisareButoane = false;

            if (User.IsInRole("User"))
            {
                ViewBag.AfisareButoane = true;
            }

            ViewBag.EsteAdmin = User.IsInRole("Admin");

            ViewBag.UserCurent = _userManager.GetUserId(User);
        }

        [NonAction]
        public IEnumerable<SelectListItem> GetAllCategories()
        {
            // generam o lista de tipul SelectListItem fara elemente
            var selectList = new List<SelectListItem>();

            // extragem toate categoriile din baza de date
            var categories = from cat in db.Categories
                             select cat;

            // iteram prin categorii
            foreach (var category in categories)
            {
                // adaugam in lista elementele necesare pentru dropdown
                // id-ul categoriei si denumirea acesteia
                selectList.Add(new SelectListItem
                {
                    Value = category.Id.ToString(),
                    Text = category.CategoryName.ToString()
                });
            }
            /* Sau se poate implementa astfel: 
             * 
            foreach (var category in categories)
            {
                var listItem = new SelectListItem();
                listItem.Value = category.Id.ToString();
                listItem.Text = category.CategoryName.ToString();

                selectList.Add(listItem);
             }*/


            // returnam lista de categorii
            return selectList;
        }

        // Metoda utilizata pentru exemplificarea Layout-ului
        // Am adaugat un nou Layout in Views -> Shared -> numit _LayoutNou.cshtml
        // Aceasta metoda are un View asociat care utilizeaza noul layout creat
        // in locul celui default generat de framework numit _Layout.cshtml
        public IActionResult IndexNou()
        {
            return View();
        }
    }
}