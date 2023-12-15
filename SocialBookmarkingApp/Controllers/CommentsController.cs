using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SocialBookmarkingApp.Data;
using SocialBookmarkingApp.Models;

namespace SocialBookmarkingApp.Controllers
{
    public class CommentsController : Controller
    {
      private readonly ApplicationDbContext db;
        public CommentsController(ApplicationDbContext context)
        {
            db = context;
        }

        public IActionResult Delete(int id)
        {
            Comment comm = db.Comments.Find(id);

            db.Comments.Remove(comm);
            db.SaveChanges();
            return Redirect("/Bookmarks/Show/" + comm.BookmarkId);
        }

        // In acest moment vom implementa editarea intr-o pagina View separata
        // Se editeaza un comentariu existent
        // Editarea unui comentariu asociat unui articol din baza de date
        // Se poate edita comentariul doar de catre userii cu rolul Admin
        // sau de catre userii cu rolul User sau Editor doar daca comentariul 
        // a fost lasat de acestia
       // [Authorize(Roles = "User,Editor,Admin")]
        public IActionResult Edit(int id)
        {
            Comment comm = db.Comments.Find(id);
            return View(comm);
        }

        [HttpPost]

        public IActionResult Edit(int id, Comment requestComment)
        {
            Comment comm = db.Comments.Find(id);
            if (ModelState.IsValid)
            {
                comm.Content = requestComment.Content;
                
                db.SaveChanges();

                return Redirect("/Bookmarks/Show/" + comm.BookmarkId);
            }
            else
            {
                return View(requestComment);
            }
        }

    }
}
