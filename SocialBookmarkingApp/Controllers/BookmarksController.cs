using System.Text.RegularExpressions;
using AngleSharp.Dom;
using Ganss.Xss;
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
    private IWebHostEnvironment _env;

    public static string UploadFolder = "uploads";

    public BookmarksController(ApplicationDbContext context, UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager, IWebHostEnvironment env) {
        _context = context;
        _bookmarks = context.Bookmarks;
        _comments = context.Comments;
        _userManager = userManager;
        _roleManager = roleManager;
        _env = env;
    }

    [NonAction]
    private IEnumerable<Bookmark> SearchBookmarksEnumerable(IEnumerable<Bookmark> bookmarks, string search,
        IEnumerable<string> categories) {
        if (string.IsNullOrEmpty(search)) {
            return bookmarks;
        }

        // Turn search string into array of keywords
        var keywords = search.Split(' ');
        // Turn keywords into regex patterns that match misspelled words
        var patterns = keywords.Select(k => { return k.Aggregate("", (current, c) => current + $".*[{c}].*"); }
        ).ToArray();
        // Search for keywords in title, description, and categories owned by user
        foreach (var b in bookmarks) {
            var x = b.Categories.Select(c => c.CategoryName).Intersect(categories);
        }

        return bookmarks.Where(b => {
            var title = b.Title ?? "";
            var description = b.Description ?? "";
            return patterns.Any(pattern =>
                Regex.IsMatch(title, pattern, RegexOptions.IgnoreCase) ||
                Regex.IsMatch(description, pattern, RegexOptions.IgnoreCase) ||
                b.Categories.Select(c => c.CategoryName).Intersect(categories).Any(
                    c => Regex.IsMatch(c ?? "", pattern, RegexOptions.IgnoreCase)
                ));
        });
    }

    [NonAction]
    private IQueryable<Bookmark> SearchBookmarks(IQueryable<Bookmark> bookmarks, string search,
        IQueryable<string> categories) {
        if (string.IsNullOrEmpty(search)) {
            return bookmarks;
        }

        // Turn search string into array of keywords
        var keywords = search.Split(' ');
        // Turn keywords into regex patterns that match misspelled words
        var patterns = keywords.Select(k => { return k.Aggregate("", (current, c) => current + $"%{c}%"); }
        );
        // Join patterns by OR operator
        var matchedQueries = patterns.Select(pattern =>
            bookmarks.Where(b => EF.Functions.Like(b.Title, pattern) ||
                                 EF.Functions.Like(b.Description, pattern) ||
                                 b.Categories.Any(c => EF.Functions.Like(c.CategoryName, pattern))
            )
        );

        // Union queries
        var matchedBookmarks = matchedQueries.Aggregate((current, next) => current.Union(next));

        return matchedBookmarks.OrderBy(b => b.Title);
    }

    [NonAction]
    private IQueryable<Bookmark> OrderBookmarks(IQueryable<Bookmark> bookmarks) {
        // Order by average rating descending (using .Average())
        return bookmarks.OrderByDescending(b => b.Reviews.Average(r => r.Rating))
            .ThenByDescending(b => b.Date);
    }

    // [Authorize(Roles = "User,Admin")]
    public async Task<IActionResult> Index([FromQuery] bool listView = false, [FromQuery] int pageSize = 5,
        [FromQuery] int page = 1, [FromQuery] string search = "") {
        //Luam tabelul Bookmarks din baza de date
        IQueryable<Bookmark> query = _bookmarks
            .Include(b => b.Categories)
            .Include(b => b.User)
            .Include(b => b.Comments)
            .Include(b => b.Reviews);
        var user = await _userManager.GetUserAsync(HttpContext.User);
        IQueryable<string> categories = _context.Categories
            .Where(c => c.User == user)
            .Select(c => c.CategoryName ?? "");
        query = !string.IsNullOrEmpty(search) ? SearchBookmarks(query, search, categories) : OrderBookmarks(query);

        var count = query.Count();
        if (listView) {
            query = query.Skip((page - 1) * pageSize).Take(pageSize);
        }

        var bookmarks = await query.ToListAsync();

        ViewBag.Bookmarks = bookmarks;
        // ViewBag.Bookmarks = new List<Bookmark>();
        ViewBag.ListView = listView;
        ViewBag.PageSize = pageSize;
        ViewBag.Page = page;
        ViewBag.Search = search;
        ViewBag.PageCount = (int)Math.Ceiling((double)count / pageSize);
        //Returnam view-ul Index cu lista de bookmarks
        return View();
    }

    //adaugam o noua metoda care va adauga returna view -ul New
    [Authorize(Roles = "User,Admin")]
    public IActionResult New() {
        var bookmark = new BookmarkCreate();
        //Luam toate categoriile din baza de date
        //bookmark.Categ = GetAllCategories();
        //Returnam view-ul New cu un obiect de tip Bookmark
        return View(bookmark);
    }

    [Authorize(Roles = "User,Admin")]
    [HttpPost]
    public async Task<IActionResult> New(BookmarkCreate bookmark) {
        if (!ModelState.IsValid) return View(bookmark);
        // Get current user
        var user = _userManager.GetUserAsync(HttpContext.User).Result;
        var sanitizer = new HtmlSanitizer();
        var model = new Bookmark {
            Title = bookmark.Title,
            Description = sanitizer.Sanitize(bookmark.Description),
            Link = bookmark.Link,
            Date = DateTime.UtcNow,
            User = user,
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
                    TempData["errorMessage"] = "Invalid File Type";
                    return View(bookmark);
            }

            // Create unique filename
            string fileName;
            string path;
            do {
                fileName = Guid.NewGuid() + ext;
                path = Path.Combine(_env.WebRootPath, UploadFolder, fileName);
            } while (System.IO.File.Exists(path));

            // Save file
            using (var fileStream = new FileStream(path, FileMode.Create)) {
                await bookmark.Media.CopyToAsync(fileStream);
            }

            model.MediaType = mediaType;
            var uri = new Uri(Path.Combine(UploadFolder, fileName), UriKind.Relative);
            model.MediaUrl = "/" + uri;
        }
        else if (bookmark.MediaUrl != null) {
            var url = new Url(bookmark.MediaUrl);
            if (url.IsInvalid) {
                TempData["errorMessage"] = "Invalid URL";
                return View(bookmark);
            }

            var extention = Path.GetExtension(url.Path);
            MediaType mediaType;
            switch (extention) {
                case ".jpg":
                case ".png":
                case ".jpeg":
                    mediaType = MediaType.EmbeddedImage;
                    break;
                case ".mp4":
                case ".avi":
                case ".mov":
                    mediaType = MediaType.EmbeddedVideo;
                    break;
                default:
                    TempData["errorMessage"] = "Invalid File Type";
                    return View(bookmark);
            }

            model.MediaType = mediaType;
            model.MediaUrl = bookmark.MediaUrl;
        }
        else {
            TempData["errorMessage"] = "Image/Video is required";
            return View(bookmark);
        }

        await _bookmarks.AddAsync(model);
        await _context.SaveChangesAsync();
        TempData["successMessage"] = "Bookmark added successfully!";
        // Redirectare către o altă acțiune sau o pagină
        return RedirectToAction("Index");
    }

    //adaugam o metoda care sterge un bookmark
    [Authorize(Roles = "User,Admin")]
    [HttpPost("[controller]/[action]/{id:int}")]
    public async Task<IActionResult> Delete([FromRoute] int id) {
        var bookmark = await _bookmarks.FindAsync(id);
        if (bookmark == null) {
            TempData["errorMessage"] = "Bookmark not found";
            return RedirectToAction("Index");
        }

        _bookmarks.Remove(bookmark);
        //stergem si comentariile asociate
        var comments = _comments.Where(c => c.BookmarkId == id);
        foreach (var comment in comments) {
            _comments.Remove(comment);
        }

        await _context.SaveChangesAsync();
        TempData["successMessage"] = "Bookmark deleted successfully!";
        return RedirectToAction("Index");
    }

    //adaugam o metoda care editeaza un bookmark
    [Authorize(Roles = "User,Admin")]
    [HttpGet]
    public IActionResult Edit(int id) {
        Bookmark bookmark = _bookmarks.Include("User")
            .Include("Comments")
            .Include("Reviews")
            .Where(art => art.Id == id)
            .First();

        // bookmark.Categ = GetAllCategories();
        return View(bookmark);
    }

    [Authorize(Roles = "User,Admin")]
    [HttpPost]
    public async Task<IActionResult> Edit(int id, Bookmark requestBookmark) {
        Bookmark bookmark = _bookmarks.Find(id);


        if (ModelState.IsValid) {  
            bookmark.Title = requestBookmark.Title;
            bookmark.Description = requestBookmark.Description;
            TempData["message"] = "Articolul a fost modificat";
            TempData["messageType"] = "alert-success";
            _context.SaveChanges();
            return RedirectToAction("Show", "Bookmarks", new { id = bookmark.Id });
        }
        else {
            if(bookmark != null)
                requestBookmark.Categ = await GetAllCategories(bookmark);
            return View(requestBookmark);
        }
    }


    public async Task<IActionResult> Show(int id) {
        var bookmark = _bookmarks
            .Include(b => b.User)
            .ThenInclude(u => u.Bookmarks)
            .Include(b => b.Comments)
            .ThenInclude(c => c.User)
            .Include("Reviews")
            .First(art => art.Id == id);
        bookmark.RelatedBookmarks = await GetRelatedBookmarks(bookmark);
        // Get available categories for current user (bookmark is not saved in these categories)
        bookmark.Categ = await GetAllCategories(bookmark);
        return View(bookmark);
    }

    [HttpPost]
    [Authorize(Roles = "User,Admin")]
    public async Task<IActionResult> NewComment([FromForm] Comment comment) {
        if (!ModelState.IsValid) {
            // Get validation errors
            var errors = ModelState.Values.SelectMany(v => v.Errors);
            // Convert errors to string
            var errorMessage = errors.Aggregate("", (current, error) => current + (error.ErrorMessage + "\n"));
            TempData["errorMessage"] = errorMessage;
            return RedirectToAction("Show", "Bookmarks", new { id = comment.BookmarkId });
        }

        comment.Date = DateTime.UtcNow;
        comment.User = await _userManager.GetUserAsync(HttpContext.User);
        _comments.Add(comment);
        await _context.SaveChangesAsync();
        return RedirectToAction("Show", "Bookmarks", new { id = comment.BookmarkId });
    }

    [HttpPost]
    [Authorize(Roles = "User,Admin")]
    public async Task<IActionResult> DeleteComment(int id) {
        var comment = await _comments.FindAsync(id);
        if (comment == null) {
            TempData["errorMessage"] = "Comment not found";
            return RedirectToAction("Index");
        }

        // Check if user is owner of comment
        var user = await _userManager.GetUserAsync(HttpContext.User);
        if (comment.User != user && !await _userManager.IsInRoleAsync(user, "Admin")) {
            TempData["errorMessage"] = "User not authorized";
            return RedirectToAction("Show", "Bookmarks", new { id = comment.BookmarkId });
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
    public async Task<IEnumerable<SelectListItem>> GetAllCategories(Bookmark bookmark) {
        // generam o lista de tipul SelectListItem fara elemente
        var selectList = new List<SelectListItem>();

        var user = await _userManager.GetUserAsync(HttpContext.User);
        // extragem toate categoriile din baza de date
        var categories = await _context.Categories
            .Where(c => c.User == user)
            .Where(c => !c.Bookmarks.Contains(bookmark))
            .ToListAsync();


        // iteram prin categorii
        foreach (var category in categories) {
            // adaugam in lista elementele necesare pentru dropdown
            // id-ul categoriei si denumirea acesteia
            
            //adaugam doar daca bookmark-ul nu este salvat in acea categorie
            selectList.Add(new SelectListItem
            {
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
            .Include(b => b.User)
            .Include(b => b.Comments)
            .Where(b => b.User == bookmark.User && b.Id != bookmark.Id)
            .ToListAsync();
        return relatedBookmarks;
    }

    [Authorize(Roles = "User,Admin")]
    public async Task<IActionResult> Saved() {
        var identity = await _userManager.GetUserAsync(HttpContext.User);
        var user = _context.Users
            .Include(u => u.Categories)
            .ThenInclude(c => c.Bookmarks)
            .ThenInclude(b => b.User)
            .First(u => u.Id == identity.Id);
        // .Include(u => u.SavedBookmarks)
        return View(user);
    }

    [Authorize(Roles = "User,Admin")]
    [HttpPost]
    public async Task<IActionResult> Save([FromForm] BookmarkSaveModel model) {
        var user = await _userManager.GetUserAsync(HttpContext.User);
        var bookmark = await _bookmarks.FindAsync(model.BookmarkId);
        if (bookmark == null) {
            TempData["errorMessage"] = "Bookmark not found";
            return RedirectToAction("Show", "Bookmarks", new { id = bookmark.Id });
        }

        if (model.CategoryId == null) {
            TempData["errorMessage"] = "Category is required";
            return RedirectToAction("Show", "Bookmarks", new { id = bookmark.Id });
        }

        var category = await _context.Categories
            .Include(c => c.Bookmarks)
            .Include(c => c.User)
            .FirstOrDefaultAsync(c => c.Id == model.CategoryId);
        if (category == null) {
            TempData["errorMessage"] = "Category not found";
            return RedirectToAction("Show", "Bookmarks", new { id = bookmark.Id });
        }

        // Check if user owns category or if bookmark is already saved in category
        if (category.User != user && _userManager.IsInRoleAsync(user, "Admin").Result) {
            TempData["errorMessage"] = "User not authorized";
            return RedirectToAction("Show", "Bookmarks", new { id = bookmark.Id });
        }

        if (!(category.Bookmarks?.Contains(bookmark) ?? true)) {
            category.Bookmarks.Add(bookmark);
            await _context.SaveChangesAsync();
        }

        TempData["successMessage"] = "Bookmark saved successfully!";

        return RedirectToAction("Saved", "Bookmarks");
    }

    [Authorize(Roles = "User,Admin")]
    [HttpPost("[controller]/Remove/{bookmarkId:int}/{categoryId:int}")]
    public async Task<IActionResult> RemoveFromCategory([FromRoute] int bookmarkId, [FromRoute] int categoryId) {
        var user = await _userManager.GetUserAsync(HttpContext.User);
        var category = await _context.Categories
            .Include(c => c.Bookmarks)
            .Include(c => c.User)
            .FirstOrDefaultAsync(c => c.Id == categoryId);
        if (category == null) {
            TempData["errorMessage"] = "Category not found";
            return RedirectToAction("Saved", "Bookmarks");
        }

        if (category.User != user) {
            TempData["errorMessage"] = "User not authorized";
            return RedirectToAction("Saved", "Bookmarks");
        }

        var bookmark = await _bookmarks
            .Include(b => b.Categories)
            .FirstOrDefaultAsync(b => b.Id == bookmarkId);
        if (bookmark == null) {
            TempData["errorMessage"] = "Bookmark not found";
            return RedirectToAction("Saved", "Bookmarks");
        }

        if (!(category.Bookmarks?.Contains(bookmark) ?? false)) {
            TempData["errorMessage"] = "Bookmark not found in category";
            return RedirectToAction("Saved", "Bookmarks");
        }

        // Remove bookmark from category
        category.Bookmarks.Remove(bookmark);
        await _context.SaveChangesAsync();
        TempData["successMessage"] = "Bookmark removed from category";
        return RedirectToAction("Saved", "Bookmarks");
    }

    [HttpGet("[action]/{id}")]
    public async Task<IActionResult> Profile([FromRoute] string id) {
        var user = await _context.Users
            .Include(u => u.Categories)
            .ThenInclude(c => c.Bookmarks)
            .ThenInclude(b => b.User)
            .Include(u => u.Bookmarks)
            .FirstOrDefaultAsync(u => u.Id == id);
        if (user != null) return View(user);
        TempData["errorMessage"] = "User not found";
        return RedirectToAction("Index", "Bookmarks");
    }
}