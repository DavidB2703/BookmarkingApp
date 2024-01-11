using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Build.Framework;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SocialBookmarkingApp.Models;
public enum MediaType {
    LocalImage,
    LocalVideo,
    EmbeddedImage,
    EmbeddedVideo,
}

public class Bookmark {
    public int Id { get; set; }

    [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Titlul este obligatoriu")]
    [StringLength(100, ErrorMessage = "Titlul nu poate avea  mai mult de 100 de caractere")]
    [MinLength(5, ErrorMessage = "Titlul trebuie sa aiba maimult de 5 caractere")]    
    public string? Title { get; set; }

   [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Continutul este obligatoriu")] 
    public string? Description { get; set; }

    [System.ComponentModel.DataAnnotations.Required] public string? Link { get; set; }

   
    [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Continutul media este obligatoriu")] 
    public MediaType? MediaType { get; set; }

    [System.ComponentModel.DataAnnotations.Required] public string? MediaUrl { get; set; }
    
    [System.ComponentModel.DataAnnotations.Required] public DateTime? Date { get; set; }

    public virtual ApplicationUser? User { get; set; }


    [NotMapped] public IEnumerable<SelectListItem>? Categ { get; set; }

    public virtual ICollection<Comment>? Comments { get; set; }

    public virtual ICollection<Review>? Reviews { get; set; }
    
    public int AverageRating => Reviews?.Count > 0 ? Reviews.Sum(r => r.Rating) / Reviews.Count : 0;
    
    [NotMapped]
    public IEnumerable<Bookmark> RelatedBookmarks { get; set; } = null!;
    
    public virtual ICollection<Category> Categories { get; set; }
}