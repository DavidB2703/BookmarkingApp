using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace SocialBookmarkingApp.Models; 

public class BookmarkCreate {
    public int Id { get; set; }

    [Required(ErrorMessage = "Titlul este obligatoriu")]
    [StringLength(100, ErrorMessage = "Titlul nu poate avea  mai mult de 100 de caractere")]
    [MinLength(5, ErrorMessage = "Titlul trebuie sa aiba maimult de 5 caractere")]
    public string Title { get; set; }

    [Required(ErrorMessage = "Description is required")]
    [StringLength(10000, ErrorMessage = "Descrierea nu poate avea mai mult de 10000 de caractere")]
    public string Description { get; set; }

    public string? Link { get; set; }

    public IFormFile? Media { get; set; }

    public string? MediaUrl { get; set; }

    public int? CategoryId { get; set; }

    public virtual Category? Category { get; set; }

    public virtual ApplicationUser? User { get; set; }

    [NotMapped] public IEnumerable<SelectListItem>? Categ { get; set; }

    public virtual ICollection<Comment>? Comments { get; set; }

    public virtual ICollection<Review>? Reviews { get; set; }
}