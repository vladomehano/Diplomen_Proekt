using System.ComponentModel.DataAnnotations;

namespace Antikvarnik.Models
{
    public class Category
    {
        public int Id { get; set; }

        [Required]
        [StringLength(80)]
        public string Name { get; set; } = string.Empty;

        [StringLength(300)]
        public string? Description { get; set; }

        public ICollection<Item> Items { get; set; } = new List<Item>();
    }
}