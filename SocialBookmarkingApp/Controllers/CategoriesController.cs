using Microsoft.AspNetCore.Mvc;
using SocialBookmarkingApp.Data;
using SocialBookmarkingApp.Models;

namespace SocialBookmarkingApp.Controllers
{
    public class CategoriesController : Controller
    {
        private readonly ApplicationDbContext db;
        public CategoriesController(ApplicationDbContext context)
        {
            db = context;
        }
        public IActionResult New()
        {
            return View();
        }
        [HttpPost]
        public IActionResult CreateCategory(Category category)
        {
            if (ModelState.IsValid)
            {
                // Logica pentru salvarea categoriei în baza de date sau altă operație relevantă
                // Exemplu simplu: salvare într-o listă de categorii
                
                db.Categories.Add(category);
                db.SaveChanges();

                // Redirectare către o altă acțiune sau o pagină
                return RedirectToAction("Index", "Bookmarks");
            }

            // Dacă modelul nu este valid, revenim la formularul de adăugare cu erori
            return View("AdaugaCategorie", category);
        }

        public IActionResult Show()
        {
            ViewBag.categories = db.Categories;
            return View();
        }

        //adaugam o metoda care sterge o categorie
        [HttpPost]
        public IActionResult Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var category = db.Categories.Find(id);
            if (category == null)
            {
                return NotFound();
            }
            db.Categories.Remove(category);
            db.SaveChanges();
            return RedirectToAction("Show", "Categories");
        }

        //adaugam o metoda care editeaza o categorie
        public IActionResult Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var category = db.Categories.Find(id);
            if (category == null)
            {
                return NotFound();
            }
            return View(category);
        }
        [HttpPost]
        public IActionResult Edit(Category category)
        {
            if (ModelState.IsValid)
            {
                db.Categories.Update(category);
                db.SaveChanges();
                return RedirectToAction("Show", "Categories");
            }
            return View(category);
        }





    }
}
