using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Build.Framework;

namespace SocialBookmarkingApp.Models; 

public class BookmarkCreate {
    public int Id { get; set; }

    [Required] public string? Title { get; set; }

    [Required] public string? Description { get; set; }

    [Required] public string? Link { get; set; }

    public IFormFile? Media { get; set; }

    public string? MediaUrl { get; set; }

    public int? CategoryId { get; set; }

    public virtual Category? Category { get; set; }

    public virtual ApplicationUser? User { get; set; }

    [NotMapped] public IEnumerable<SelectListItem>? Categ { get; set; }

    public virtual ICollection<Comment>? Comments { get; set; }

    public virtual ICollection<Review>? Reviews { get; set; }
}