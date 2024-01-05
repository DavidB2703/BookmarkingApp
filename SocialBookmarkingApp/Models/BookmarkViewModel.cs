namespace SocialBookmarkingApp.Models; 

public class BookmarkViewModel {
    public Bookmark Bookmark { get; set; }
    public bool CanDelete { get; set; } = false;
    public bool ShowOwner { get; set; } = true;
}