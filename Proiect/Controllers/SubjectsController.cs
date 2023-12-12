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
    //[Authorize]
    public class SubjectsController : Controller
    {

        // PASUL 10 - useri si roluri

        private readonly ApplicationDbContext db;
        public SubjectsController(ApplicationDbContext context)
        {
            db = context;
        }


        //private readonly UserManager<ApplicationUser> _userManager;

        //private readonly RoleManager<IdentityRole> _roleManager;

        //public SubjectsController(
        //    ApplicationDbContext context,
        //    UserManager<ApplicationUser> userManager,
        //    RoleManager<IdentityRole> roleManager
        //    )
        //{
        //    db = context;

        //    _userManager = userManager;

        //    _roleManager = roleManager;
        //}

        // Se afiseaza lista tuturor articolelor impreuna cu categoria 
        // din care fac parte
        // Pentru fiecare articol se afiseaza si userul care a postat articolul respectiv
        // HttpGet implicit
        //[Authorize(Roles = "User,Editor,Admin")]

        public IActionResult Index()
        {
            var subjects = db.Subjects.Include("Category");//.Include("User");

            // ViewBag.OriceDenumireSugestiva
            ViewBag.Subjects = subjects;

            if (TempData.ContainsKey("message"))
            {
                ViewBag.Message = TempData["message"];
                ViewBag.Alert = TempData["messageType"];
            }

            return View();
        }

        // Se afiseaza un singur articol in functie de id-ul sau 
        // impreuna cu categoria din care face parte
        // In plus sunt preluate si toate comentariile asociate unui articol
        // Se afiseaza si userul care a postat articolul respectiv
        // HttpGet implicit

        //[Authorize(Roles = "User,Editor,Admin")]
        public IActionResult Show(int id)
        {
            Subject subject = db.Subjects.Include("Category")
                                         //.Include("User")
                                         //.Include("Tags") //adaugat
                                         .Include("Answers")
                                         //.Include("Answers.User")
                                         .Where(subject => subject.Id == id)
                                         .First();


            SetAccessRights();

            return View(subject);
        }

        // Adaugarea unui raspuns asociat unui subiect in baza de date
        [HttpPost]
        public IActionResult Show([FromForm] Answer answer)
        {
            answer.Date = DateTime.Now;

            try
            {
                db.Answers.Add(answer);
                db.SaveChanges();
                return Redirect("/Subjects/Show/" + answer.SubjectId);
            }

            catch (Exception ex)
            {
                Subject subject = db.Subjects.Include("Category").Include("Answers") /*Include("Tags")*/
                               .Where(subject => subject.Id == answer.SubjectId)
                               .First();

                //return Redirect("/Articles/Show/" + comm.ArticleId);

                return View(subject);
            }
        }

        // Adaugarea unui tag asociat unui subiect in baza de date
        //[HttpPost]
        //public IActionResult Show([FromForm] Tag tag)
        //{
        //    try
        //    {
        //        db.Tags.Add(tag);
        //        db.SaveChanges();
        //        return Redirect("/Subjects/Show/" + tag.SubjectId);
        //    }

        //    catch (Exception ex)
        //    {
        //        Subject subject = db.Subjects.Include("Category").Include("Tags").Include("answers")
        //                       .Where(subject => subject.Id == tag.SubjectId)
        //                       .First();

        //        //return Redirect("/Articles/Show/" + comm.ArticleId);

        //        return View(subject);
        //    }
        //}


        // Se afiseaza formularul in care se vor completa datele unui subiect
        // impreuna cu selectarea categoriei din care face parte si tagurile
        // Doar utilizatorii cu rolul de Utilizator inregistrat sau Admin pot adauga articole in platforma
        // HttpGet implicit

        //[Authorize(Roles = "Editor,Admin")]
        public IActionResult New()
        {
            Subject subject = new Subject();

            // Se preia lista de categorii cu ajutorul metodei GetAllCategories()
            subject.Categ = GetAllCategories();

            //TO DO: cum initializam lista de taguri?
            //subject.Tags = GetAllTags();

            return View(subject);
        }

        // Se adauga articolul in baza de date
        // Doar utilizatorii cu rolul de utilizator inregistrat sau Admin pot adauga articole in platforma

        //[Authorize(Roles = "Editor,Admin")]
        [HttpPost]
        public IActionResult New(Subject subject)
        {
            subject.Date = DateTime.Now;
            subject.Categ = GetAllCategories();
            // preluam id-ul utilizatorului care posteaza articolul
            //Subject.UserId = _userManager.GetUserId(User);


            if (ModelState.IsValid)
            {
                db.Subjects.Add(subject);
                db.SaveChanges();
                TempData["message"] = "Articolul a fost adaugat";
                TempData["messageType"] = "alert-success";
                return RedirectToAction("Index");
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

        //[Authorize(Roles = "Editor,Admin")]
        public IActionResult Edit(int id)
        {

            Subject subject = db.Subjects.Include("Category")//.Include("Tags")
                                        .Where(subject => subject.Id == id)
                                        .First();

            subject.Categ = GetAllCategories();
           // subject.Tgs = GetAllTags();

            //de adaugat tag

            //if (subject.UserId == _userManager.GetUserId(User) || User.IsInRole("Admin"))
            //{
                return View(subject);
            //}

            //else
            //{
            //    TempData["message"] = "Nu aveti dreptul sa faceti modificari asupra unui articol care nu va apartine";
            //    TempData["messageType"] = "alert-danger";
            //    return RedirectToAction("Index");
            //}

        }

        // Se adauga subiectul de discutie modificat in baza de date
        // Verificam rolul utilizatorilor care au dreptul sa editeze (Creatorul sau Admin)        [HttpPost]
        //[Authorize(Roles = "Editor,Admin")]
        [HttpPost]
        public IActionResult Edit(int id, Subject requestSubject)
        {
            Subject subject = db.Subjects.Find(id);
            requestSubject.Categ = GetAllCategories();
            //requestSubject.Tgs = GetAllTags();

            if (ModelState.IsValid)
            {
                //if (subject.UserId == _userManager.GetUserId(User) || User.IsInRole("Admin"))
                //{
                    subject.Title = requestSubject.Title;
                    subject.Content = requestSubject.Content;
                    //taguri
                    subject.CategoryId = requestSubject.CategoryId;
                    TempData["message"] = "Subiectul a fost modificat";
                    TempData["messageType"] = "alert-success";
                    db.SaveChanges();
                    return RedirectToAction("Index");
                //}
                //else
                //{
                //    TempData["message"] = "Nu aveti dreptul sa faceti modificari asupra unui articol care nu va apartine";
                //    TempData["messageType"] = "alert-danger";
                //    return RedirectToAction("Index");
                //}
            }
            else
            {
                requestSubject.Categ = GetAllCategories();
                return View(requestSubject);
            }
        }


        // Se sterge un articol din baza de date
        // Utilizatorii cu rolul de Editor sau Admin pot sterge articole
        // Editorii pot sterge doar articolele publicate de ei
        // Adminii pot sterge orice articol din baza de date

        [HttpPost]
        //[Authorize(Roles = "Editor,Admin")]
        public ActionResult Delete(int id)
        {
            Subject subject = db.Subjects.Include("Answers")
                                         .Where(subject => subject.Id == id)
                                         .First();

           // if (subject.UserId == _userManager.GetUserId(User) || User.IsInRole("Admin"))
           // {
                db.Subjects.Remove(subject);
                db.SaveChanges();
                TempData["message"] = "Subiectul a fost sters";
                TempData["messageType"] = "alert-success";
                return RedirectToAction("Index");
            //}

            //else
            //{
            //    TempData["message"] = "Nu aveti dreptul sa stergeti un articol care nu va apartine";
            //    TempData["messageType"] = "alert-danger";
            //    return RedirectToAction("Index");
            //}
        }


        // Conditiile de afisare a butoanelor de editare si stergere
        private void SetAccessRights()
        {
            //ViewBag.AfisareButoane = false;

            //if (User.IsInRole("Editor"))
            //{
                ViewBag.AfisareButoane = true;
            //}

            //ViewBag.EsteAdmin = User.IsInRole("Admin");

            //ViewBag.UserCurent = _userManager.GetUserId(User);
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

        //[NonAction]
        //public IEnumerable<SelectListItem> GetAllTags()
        //{
        //    // generam o lista de tipul SelectListItem fara elemente
        //    var selectList = new List<SelectListItem>();

        //    // extragem toate tag-urile din baza de date
        //    var tags = from tag in db.Tags
        //                     select tag;

        //    // iteram prin tag-uri
        //    foreach (var t in tags)
        //    {
        //        // adaugam in lista elementele necesare pentru dropdown
        //        // id-ul categoriei si denumirea acesteia
        //        selectList.Add(new SelectListItem
        //        {
        //            Value = t.Id.ToString(),
        //            Text = t.TagName.ToString()
        //        });
        //    }
        //    // returnam lista de categorii
        //    return selectList;
        //}

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

