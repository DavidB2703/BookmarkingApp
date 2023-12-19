using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SocialBookmarkingApp.Data;
using SocialBookmarkingApp.Models;

namespace SocialBookmarkingApp.Controllers
{
    public class CategoriesController : Controller
    {
        private readonly ApplicationDbContext db;
        private readonly DbSet<Category> _categories;
        private readonly UserManager<ApplicationUser> _userManager;
        public CategoriesController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            db = context;
            _userManager = userManager;
            _categories = context.Categories;
        }
        public IActionResult New()
        {
            return View();
        }
        [HttpPost]
        [Authorize(Roles = "User,Admin")]
        public async Task<IActionResult> CreateCategory(Category category)
        {
            if (ModelState.IsValid)
            {
                // Logica pentru salvarea categoriei în baza de date sau altă operație relevantă
                // Exemplu simplu: salvare într-o listă de categorii

                category.User = await _userManager.GetUserAsync(User);
                
                db.Categories.Add(category);
                await db.SaveChangesAsync();

                // Redirectare către o altă acțiune sau o pagină
                return RedirectToAction("Index", "Bookmarks");
            }

            // Dacă modelul nu este valid, revenim la formularul de adăugare cu erori
            return View("AdaugaCategorie", category);
        }

        [Authorize(Roles = "User,Admin")]
        public async Task<IActionResult> Show()
        {
            var user = await _userManager.GetUserAsync(User);
            
            ViewBag.categories = _categories.Where(c => c.User == user).ToList();
            return View();
        }

        //adaugam o metoda care sterge o categorie
        [HttpPost]
        [Authorize(Roles = "User,Admin")]
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
