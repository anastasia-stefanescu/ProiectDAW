//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;
//using Microsoft.AspNetCore.Mvc;
//using Proiect.Data;
//using Proiect.Models;

//// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

//namespace Proiect.Controllers
//{
//    public class TagsController : Controller
//    {
//        private readonly ApplicationDbContext db;

//        public TagsController(ApplicationDbContext context)
//        {
//            db = context;
//        }


//        public ActionResult Index()
//        {
//            if (TempData.ContainsKey("message"))
//            {
//                ViewBag.message = TempData["message"].ToString();
//            }

//            var tags = from tag in db.Tags
//                             orderby tag.TagName
//                             select tag;
//            ViewBag.Tags = tags;
//            return View();
//        }

//        public ActionResult Show(int id)
//        {
//            Tag tag = db.Tags.Find(id);
//            return View(tag);
//        }

//        public ActionResult New()
//        {
//            return View();
//        }

//        [HttpPost]
//        public ActionResult New(Tag cat)
//        {
//            if (ModelState.IsValid)
//            {
//                db.Tags.Add(cat);
//                db.SaveChanges();
//                TempData["message"] = "Categoria a fost adaugata";
//                return RedirectToAction("Index");
//            }

//            else
//            {
//                return View(cat);
//            }
//        }

//        public ActionResult Edit(int id)
//        {
//            Tag tag = db.Tags.Find(id);
//            return View(tag);
//        }

//        [HttpPost]
//        public ActionResult Edit(int id, Tag requestTag)
//        {
//            Tag tag = db.Tags.Find(id);

//            if (ModelState.IsValid)
//            {

//                tag.TagName = requestTag.TagName;
//                db.SaveChanges();
//                TempData["message"] = "Categoria a fost modificata!";
//                return RedirectToAction("Index");
//            }
//            else
//            {
//                return View(requestTag);
//            }
//        }

//        [HttpPost]
//        public ActionResult Delete(int id)
//        {
//            Tag tag = db.Tags.Find(id);
//            db.Tags.Remove(tag);
//            TempData["message"] = "Categoria a fost stearsa";
//            db.SaveChanges();
//            return RedirectToAction("Index");
//        }
//    }
//}