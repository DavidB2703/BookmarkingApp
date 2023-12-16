using MessagePack;
using System.ComponentModel.DataAnnotations;

namespace SocialBookmarkingApp.Models
{
    public class Review
    {
        [System.ComponentModel.DataAnnotations.Key]
        public int Id { get; set; }
        public ApplicationUser? User { get; set; }
        public int BookmarkId { get; set; }
        public virtual Bookmark Bookmark { get; set; } = null!;
        public int Rating { get; set; }
       
    }
}
