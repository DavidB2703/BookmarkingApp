namespace SocialBookmarkingApp.Models
{
    public class Comment
    {
        public int Id { get; set; }
        public string? Content { get; set; }
        public DateTime Date { get; set; }

        public int? BookmarkId { get; set; }
        public virtual Bookmark? Bookmark { get; set; }
    }
}
