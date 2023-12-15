using Microsoft.AspNetCore.Mvc;
using SocialBookmarkingApp.Data;
using SocialBookmarkingApp.Models;

namespace SocialBookmarkingApp.Controllers
{
    public class ReviewsController : Controller
    {

        private readonly ApplicationDbContext db;
        public ReviewsController(ApplicationDbContext context)
        {
            db = context;
        }

        //functie de delete
        [HttpPost]
        public IActionResult Delete(int id)
        {
            Review rev = db.Reviews.Find(id);

            db.Reviews.Remove(rev);
            db.SaveChanges();
            return Redirect("/Bookmarks/Show/" + rev.BookmarkId);
        }

        //editare
        public IActionResult Edit(int id)
        {
            Review rev = db.Reviews.Find(id);
            return View(rev);
        }

        [HttpPost]

        public IActionResult Edit(int id, Review requestReview)
        {
            Review rev = db.Reviews.Find(id);
            if (ModelState.IsValid)
            {
                rev.Content = requestReview.Content;
                rev.Rating = requestReview.Rating;
                db.SaveChanges();

                return Redirect("/Bookmarks/Show/" + rev.BookmarkId);
            }
            else
            {
                return View(requestReview);
            }
        }

        public IActionResult Index()
        {
            return View();
        }

    }
}
