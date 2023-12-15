using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations.Schema;

namespace SocialBookmarkingApp.Models
{
    public class ApplicationUser : IdentityUser
    {

        public virtual ICollection<Bookmark>? Bookmarks { get; set; }
        public virtual ICollection<Category>? Categories { get; set; }
        public virtual ICollection<Comment>? Comments { get; set; }
        public virtual ICollection<Review>? Reviews { get; set; }

        // atribute suplimentare adaugate pentru user
        public string? FirstName { get; set; }

        public string? LastName { get; set; }

        // variabila in care vom retine rolurile existente in baza de date
        // pentru popularea unui dropdown list
        [NotMapped]
        public IEnumerable<SelectListItem>? AllRoles { get; set; }

    }
}
