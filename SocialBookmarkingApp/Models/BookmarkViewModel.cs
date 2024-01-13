namespace SocialBookmarkingApp.Models; 

public class BookmarkViewModel {
    public Bookmark Bookmark { get; set; }
    public Category? InCategory { get; set; }
    public bool ShowOwner { get; set; } = true;
}