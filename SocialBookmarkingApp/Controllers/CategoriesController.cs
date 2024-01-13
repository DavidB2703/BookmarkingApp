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
                TempData["successMessage"] = "Category added successfully!";
                return RedirectToAction("Saved", "Bookmarks");
            }

            // Dacă modelul nu este valid, revenim la formularul de adăugare cu erori
            return View("New", category);
        }
        //adaugam o metoda care sterge o categorie
        [HttpPost("[controller]/[action]/{id:int}")]
        [Authorize(Roles = "User,Admin")]
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            var category = await db.Categories.FindAsync(id);
            if (category == null)
            {
                TempData["errorMessage"] = "Category not found!";
                return RedirectToAction("Saved", "Bookmarks");
            }
            db.Categories.Remove(category);
            await db.SaveChangesAsync();
            TempData["successMessage"] = "Category deleted successfully!";
            return RedirectToAction("Saved", "Bookmarks");
        }

        //adaugam o metoda care editeaza o categorie
        [HttpGet("[controller]/[action]/{id:int}")]
        public async Task<IActionResult> Edit([FromRoute] int id)
        {
            var category = await db.Categories.FindAsync(id);
            if (category != null) return View(category);
            TempData["errorMessage"] = "Category not found!";
            return RedirectToAction("Saved", "Bookmarks");
        }
        [HttpPost]
        public async Task<IActionResult> Edit([FromForm] Category category)
        {
            if (!ModelState.IsValid) return View(category);
            db.Categories.Update(category);
            await db.SaveChangesAsync();
            TempData["successMessage"] = "Category updated successfully!";
            return RedirectToAction("Saved", "Bookmarks");
        }





    }
}
