using System;

namespace Antikvarnik.Models
{
    public class Item
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public string? MainPicUrl { get; set; }
        public string? PicturesUrls { get; set; }

        public bool IsDeleted { get; set; }

        public string[] Pictures => string.IsNullOrWhiteSpace(PicturesUrls)
            ? Array.Empty<string>()
            : PicturesUrls.Split("|", StringSplitOptions.RemoveEmptyEntries);
    }
}
