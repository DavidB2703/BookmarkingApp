using MessagePack;
using System.ComponentModel.DataAnnotations;

namespace SocialBookmarkingApp.Models
{
    public enum rating
    {
        OneStar = 1,
        TwoStars = 2,
        ThreeStars = 3,
        FourStars = 4,
        FiveStars = 5
    }
    public class Review
    {
        [System.ComponentModel.DataAnnotations.Key]
        public int Id { get; set; }
        public string? Content { get; set; }
        public DateTime Date { get; set; }
        public int? BookmarkId { get; set; }
        public virtual Bookmark? Bookmark { get; set; }
        public rating Rating { get; set; }
       
    }
}
