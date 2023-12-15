using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SocialBookmarkingApp.Data;
using SocialBookmarkingApp.Models;

namespace SocialBookmarkingApp.Controllers
{
    public class BookmarksController : Controller
    {
        private readonly ApplicationDbContext db;

        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public BookmarksController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            db = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }
        [Authorize(Roles = "User,Admin")]
        public IActionResult Index()
        {
            //Luam tabelul Bookmarks din baza de date
            ViewBag.bookmarks = db.Bookmarks.Include(b => b.Category).Include(b => b.Comments);//.Include(b=>b.Reviews);
            //Returnam view-ul Index cu lista de bookmarks
            return View();
        }
        //adaugam o noua metoda care va adauga returna view -ul New
        [Authorize(Roles = "User,Admin")]
        public IActionResult New()
        {

            Bookmark bookmark = new Bookmark();
            //Luam toate categoriile din baza de date
            bookmark.Categ = GetAllCategories();
            //Returnam view-ul New cu un obiect de tip Bookmark
            return View(bookmark);
        }

        [Authorize(Roles = "User,Admin")]
        [HttpPost]
        public ActionResult New(Bookmark bookmark)
        {
            if (ModelState.IsValid)
            {
                // Logică pentru salvarea bookmark-ului în baza de date sau altă operație relevantă
                // Exemplu simplu: salvare într-o listă de bookmark-uri
                db.Bookmarks.Add(bookmark);
                db.SaveChanges();
                // Redirectare către o altă acțiune sau o pagină
                return RedirectToAction("Index");
            }

            // Dacă modelul nu este valid, revenim la formularul de adăugare cu erori
            return View(bookmark);
        }

        //adaugam o metoda care sterge un bookmark
        [Authorize(Roles = "User,Admin")]
        [HttpPost]
        public IActionResult Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var bookmark = db.Bookmarks.Find(id);
            if (bookmark == null)
            {
                return NotFound();
            }
            db.Bookmarks.Remove(bookmark);
            //stergem si comentariile asociate
            var comments = db.Comments.Where(c => c.BookmarkId == id);
            foreach (var comment in comments)
            {
                db.Comments.Remove(comment);
            }
            db.SaveChanges();
            return RedirectToAction("Index");
        }
        //adaugam o metoda care editeaza un bookmark
        [Authorize(Roles = "User,Admin")]
        [HttpGet]
        public IActionResult Edit(int id)
        {

            Bookmark article = db.Bookmarks.Include("Category")
                                        .Include("Comments")
                                        .Where(art => art.Id == id)
                                        .First();

            article.Categ = GetAllCategories();
            return View(article);

        }

        [Authorize(Roles = "User,Admin")]
        [HttpPost]
        public IActionResult Edit(int id, Bookmark requestBookmark)
        {
            Bookmark bookmark= db.Bookmarks.Find(id);


            if (ModelState.IsValid)
            {
          
                    bookmark.Title = requestBookmark.Title;
                    bookmark.CategoryId = requestBookmark.CategoryId;
                    bookmark.Description = requestBookmark.Description;
                    TempData["message"] = "Articolul a fost modificat";
                    TempData["messageType"] = "alert-success";
                    db.SaveChanges();
                    return RedirectToAction("Index");
            }
            else
            {
                requestBookmark.Categ = GetAllCategories();
                return View(requestBookmark);
            }
        }

        [Authorize(Roles = "User,Admin")]
        public IActionResult Show(int id)
        {
            Bookmark bookmark = db.Bookmarks.Include("Category")
                                        .Include("Comments")
                                        .Include("Reviews")
                                        .Where(art => art.Id == id)
                                        .First();
            return View(bookmark);
        }

        [HttpPost]
        [Authorize(Roles = "User,Admin")]
        public IActionResult Show([FromForm] Comment comment)
        {
            if (ModelState.IsValid)
            {
                db.Comments.Add(comment);
                db.SaveChanges();
                return RedirectToAction("Show", "Bookmarks", new { id = comment.BookmarkId });
              
            }

            else
            {
                Bookmark bookmark = db.Bookmarks.Include("Category")
                                         .Include("User")
                                         .Include("Comments")
                                         .Include("Comments.User")
                                         .Where(art => art.Id == comment.BookmarkId)
                                         .First();

                return View();
            }
        }

        [Authorize(Roles = "User,Admin")]
        public IActionResult ShowReviews([FromForm] Review review)
        {
            if (ModelState.IsValid)
            {
                db.Reviews.Add(review);
                db.SaveChanges();
                return RedirectToAction("Show", "Bookmarks", new { id = review.BookmarkId });
            }
            else
            {
                Bookmark bookmark = db.Bookmarks.Include("Category")
                                         .Include("User")
                                         .Include("Comments")
                                         .Include("Comments.User")
                                         .Where(art => art.Id == review.BookmarkId)
                                         .First();

                return View();
            }
        }

        [Authorize(Roles = "User,Admin")]
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
            // returnam lista de categorii
            return selectList;
        }



    }
   
}
