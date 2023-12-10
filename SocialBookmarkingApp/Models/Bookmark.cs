using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations.Schema;

namespace SocialBookmarkingApp.Models
{
    public class Bookmark
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }

        public int? CategoryId { get; set; }

        public virtual Category? Category { get; set; }

        [NotMapped]
        public IEnumerable<SelectListItem>? Categ { get; set; }

        public virtual ICollection<Comment>? Comments { get; set; }

    }
}
