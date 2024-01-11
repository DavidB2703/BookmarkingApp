using System.ComponentModel.DataAnnotations;


namespace SocialBookmarkingApp.Models
{
    public class Comment
    {
        [Key]
        public int Id { get; set; }
        
        [Required(ErrorMessage = "Continutul comentariului este obligatoriu")]
        public string? Content { get; set; }
        public DateTime Date { get; set; }

        public virtual ApplicationUser? User { get; set; }
        public int? BookmarkId { get; set; }
        public virtual Bookmark? Bookmark { get; set; }
    }
}
