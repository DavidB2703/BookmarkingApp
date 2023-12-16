using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SocialBookmarkingApp.Data;
using SocialBookmarkingApp.Models;

namespace SocialBookmarkingApp.Controllers;

public class BookmarksController : Controller {
    private readonly ApplicationDbContext _context;
    private readonly DbSet<Comment> _comments;
    private readonly DbSet<Bookmark> _bookmarks;

    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public static string UploadFolder = "uploads";

    public BookmarksController(ApplicationDbContext context, UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager) {
        _context = context;
        _bookmarks = context.Bookmarks;
        _comments = context.Comments;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    [Authorize(Roles = "User,Admin")]
    public async Task<IActionResult> Index() {
        //Luam tabelul Bookmarks din baza de date
        var bookmarks = _bookmarks
            .Include(b => b.Category)
            .Include(b => b.User)
            .Include(b => b.Comments)
            .Include(b => b.Reviews)
            .AsEnumerable()
            .OrderByDescending(b => b.AverageRating )
            .ThenByDescending(b => b.Date)
            .ToList();
        ViewBag.Bookmarks = bookmarks;
        //Returnam view-ul Index cu lista de bookmarks
        return View();
    }

    //adaugam o noua metoda care va adauga returna view -ul New
    [Authorize(Roles = "User,Admin")]
    public IActionResult New() {
        var bookmark = new BookmarkCreate();
        //Luam toate categoriile din baza de date
        bookmark.Categ = GetAllCategories();
        //Returnam view-ul New cu un obiect de tip Bookmark
        return View(bookmark);
    }

    [Authorize(Roles = "User,Admin")]
    [HttpPost]
    public ActionResult New(BookmarkCreate bookmark) {
        if (ModelState.IsValid) {
            // Get current user
            var user = _userManager.GetUserAsync(HttpContext.User).Result;
            var model = new Bookmark {
                Title = bookmark.Title,
                Description = bookmark.Description,
                Link = bookmark.Link,
                CategoryId = bookmark.CategoryId,
                Date = DateTime.UtcNow,
                User = user
            };
            if (bookmark.Media != null) {
                // Save image to wwwroot/uploads

                // Check if media is image or video
                var ext = Path.GetExtension(bookmark.Media.FileName);
                MediaType mediaType;

                switch (ext) {
                    case ".jpg":
                    case ".png":
                    case ".jpeg":
                        mediaType = MediaType.LocalImage;
                        break;
                    case ".mp4":
                    case ".avi":
                    case ".mov":
                        mediaType = MediaType.LocalVideo;
                        break;
                    default:
                        return View(bookmark);
                }
                
                // Create unique filename
                string fileName;
                string path;
                do {
                    fileName = Guid.NewGuid() + ext;
                    path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", UploadFolder, fileName);
                } while (System.IO.File.Exists(path));

                // Save file
                using (var fileStream = new FileStream(path, FileMode.Create)) {
                    bookmark.Media.CopyTo(fileStream);
                }

                model.MediaType = mediaType;
                var uri = new Uri(Path.Combine(UploadFolder, fileName), UriKind.Relative);
                model.MediaUrl = "/" + uri.ToString();
            }

            _bookmarks.Add(model);
            _context.SaveChanges();
            // Redirectare către o altă acțiune sau o pagină
            return RedirectToAction("Index");
        }

        // Dacă modelul nu este valid, revenim la formularul de adăugare cu erori
        return View(bookmark);
    }

    //adaugam o metoda care sterge un bookmark
    [Authorize(Roles = "User,Admin")]
    [HttpPost]
    public IActionResult Delete(int? id) {
        if (id == null) {
            return NotFound();
        }

        var bookmark = _bookmarks.Find(id);
        if (bookmark == null) {
            return NotFound();
        }

        _bookmarks.Remove(bookmark);
        //stergem si comentariile asociate
        var comments = _comments.Where(c => c.BookmarkId == id);
        foreach (var comment in comments) {
            _comments.Remove(comment);
        }

        _context.SaveChanges();
        return RedirectToAction("Index");
    }

    //adaugam o metoda care editeaza un bookmark
    [Authorize(Roles = "User,Admin")]
    [HttpGet]
    public IActionResult Edit(int id) {
        Bookmark bookmark = _bookmarks.Include("Category")
            .Include("Comments")
            .Where(art => art.Id == id)
            .First();

        bookmark.Categ = GetAllCategories();
        return View(bookmark);
    }

    [Authorize(Roles = "User,Admin")]
    [HttpPost]
    public IActionResult Edit(int id, Bookmark requestBookmark) {
        Bookmark bookmark = _bookmarks.Find(id);


        if (ModelState.IsValid) {
            bookmark.Title = requestBookmark.Title;
            bookmark.CategoryId = requestBookmark.CategoryId;
            bookmark.Description = requestBookmark.Description;
            TempData["message"] = "Articolul a fost modificat";
            TempData["messageType"] = "alert-success";
            _context.SaveChanges();
            return RedirectToAction("Index");
        }
        else {
            requestBookmark.Categ = GetAllCategories();
            return View(requestBookmark);
        }
    }

    [Authorize(Roles = "User,Admin")]
    public async Task<IActionResult> Show(int id) {
        var bookmark = _bookmarks
            .Include("Category")
            .Include(b => b.User)
            .ThenInclude(u => u.Bookmarks)
            .Include("Comments")
            .Include("Reviews")
            .First(art => art.Id == id);
        bookmark.RelatedBookmarks = await GetRelatedBookmarks(bookmark);
        return View(bookmark);
    }

    [HttpPost]
    [Authorize(Roles = "User,Admin")]
    public async Task<IActionResult> NewComment([FromForm] Comment comment) {
        if (ModelState.IsValid) {
            comment.Date = DateTime.UtcNow;
            comment.User = await _userManager.GetUserAsync(HttpContext.User);
            _comments.Add(comment);
            await _context.SaveChangesAsync();
            return RedirectToAction("Show", "Bookmarks", new { id = comment.BookmarkId });
        }

        else {
            var bookmark = _bookmarks
                .Include("Category")
                .Include("User")
                .Include("Comments")
                .Include("Comments.User")
                .First(art => art.Id == comment.BookmarkId);

            return RedirectToAction("Show", bookmark);
        }
    }
    
    [HttpPost]
    [Authorize(Roles = "User,Admin")]
    public async Task<IActionResult> DeleteComment(int id) {
        var comment = _comments.Find(id);
        // Check if user is owner of comment
        var user = await _userManager.GetUserAsync(HttpContext.User);
        if (comment?.User != user) {
            return Unauthorized();
        }
        _comments.Remove(comment);
        await _context.SaveChangesAsync();
        return RedirectToAction("Show", "Bookmarks", new { id = comment.BookmarkId });
    }

    [Authorize(Roles = "User,Admin")]
    public IActionResult ShowReviews([FromForm] Review review) {
        if (ModelState.IsValid) {
            _context.Reviews.Add(review);
            _context.SaveChanges();
            return RedirectToAction("Show", "Bookmarks", new { id = review.BookmarkId });
        }
        else {
            Bookmark bookmark = _bookmarks.Include("Category")
                .Include("User")
                .Include("Comments")
                .Include("Comments.User")
                .Where(art => art.Id == review.BookmarkId)
                .First();

            return RedirectToAction("Show", "Bookmarks", new { id = review.BookmarkId });
        }
    }
    
    [HttpPost("[controller]/[action]/{id:int}")]
    [Authorize(Roles = "User,Admin")]
    public async Task<IActionResult> Review([FromRoute] int id, [FromQuery] int? rating) {
        if (rating == null) {
            return BadRequest();
        }
        var user = await _userManager.GetUserAsync(HttpContext.User);
        var review = _context.Reviews
            .Include(r => r.Bookmark)
            .Include(r => r.User)
            .FirstOrDefault(r => r.BookmarkId == id && r.User == user);
        if (review == null) {
            review = new Review {
                BookmarkId = id,
                User = user,
                Rating = rating.Value
            };
            _context.Reviews.Add(review);
        }
        else {
            review.Rating = rating.Value;
        }
        await _context.SaveChangesAsync();
        return RedirectToAction("Show", "Bookmarks", new { id = review.BookmarkId });
    }

    [Authorize(Roles = "User,Admin")]
    public IEnumerable<SelectListItem> GetAllCategories() {
        // generam o lista de tipul SelectListItem fara elemente
        var selectList = new List<SelectListItem>();

        // extragem toate categoriile din baza de date
        var categories = from cat in _context.Categories
            select cat;

        // iteram prin categorii
        foreach (var category in categories) {
            // adaugam in lista elementele necesare pentru dropdown
            // id-ul categoriei si denumirea acesteia
            selectList.Add(new SelectListItem {
                Value = category.Id.ToString(),
                Text = category.CategoryName.ToString()
            });
        }

        // returnam lista de categorii
        return selectList;
    }
    
    [NonAction]
    public async Task<List<Bookmark>> GetRelatedBookmarks(Bookmark bookmark) {
        var relatedBookmarks = await _bookmarks
            .Include(b => b.Category)
            .Include(b => b.User)
            .Include(b => b.Comments)
            .Where(b => b.User == bookmark.User && b.Id != bookmark.Id)
            .ToListAsync();
        return relatedBookmarks;
    }
}