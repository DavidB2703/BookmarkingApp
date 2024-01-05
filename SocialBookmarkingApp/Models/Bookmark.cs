using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.Build.Framework;

namespace SocialBookmarkingApp.Models;

public enum MediaType {
    LocalImage,
    LocalVideo,
    EmbeddedImage,
    EmbeddedVideo,
}

public class Bookmark {
    public int Id { get; set; }

    [Required] public string? Title { get; set; }

    [Required] public string? Description { get; set; }

    [Required] public string? Link { get; set; }

    [Required] public MediaType? MediaType { get; set; }

    [Required] public string? MediaUrl { get; set; }
    
    [Required] public DateTime? Date { get; set; }

    public virtual ApplicationUser? User { get; set; }

    // public virtual ICollection<ApplicationUser>? SavedBy { get; set; }

    [NotMapped] public IEnumerable<SelectListItem>? Categ { get; set; }

    public virtual ICollection<Comment>? Comments { get; set; }

    public virtual ICollection<Review>? Reviews { get; set; }
    
    public int AverageRating => Reviews?.Count > 0 ? Reviews.Sum(r => r.Rating) / Reviews.Count : 0;
    
    [NotMapped]
    public IEnumerable<Bookmark> RelatedBookmarks { get; set; } = null!;
    
    public virtual ICollection<Category> Categories { get; set; }
}