using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Antikvarnik.ViewModels
{
    public class ItemFormViewModel
    {
        [Required]
        [StringLength(120)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(2000)]
        public string Description { get; set; } = string.Empty;

        [Range(0, 1000000)]
        public decimal Price { get; set; }

        [Required]
        [StringLength(100)]
        public string Condition { get; set; } = string.Empty;

        [StringLength(80)]
        public string? YearOrPeriod { get; set; }

        [Display(Name = "Категория")]
        [Range(1, int.MaxValue, ErrorMessage = "Изберете категория.")]
        public int CategoryId { get; set; }

        [Display(Name = "Главна снимка")]
        public IFormFile? MainPicFile { get; set; }

        [Display(Name = "Допълнителни снимки")]
        public List<IFormFile> PicturesFiles { get; set; } = new();

        public IEnumerable<SelectListItem> Categories { get; set; } = Enumerable.Empty<SelectListItem>();
    }
}