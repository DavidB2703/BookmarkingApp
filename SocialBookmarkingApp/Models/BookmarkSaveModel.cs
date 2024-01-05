using Microsoft.AspNetCore.Mvc.Rendering;

namespace SocialBookmarkingApp.Models; 

public class BookmarkSaveModel {
    public IEnumerable<SelectListItem> Categories { get; set; }
    public int BookmarkId { get; set; }
    public int CategoryId { get; set; }
}