using Antikvarnik.Data;
using Antikvarnik.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Antikvarnik.Controllers
{
    public class ItemsController : Controller
    {
        private readonly ApplicationDbContext dbc;
        private readonly IWebHostEnvironment env;

        public ItemsController(ApplicationDbContext dbContext, IWebHostEnvironment env)
        {
            this.dbc = dbContext;
            this.env = env;
        }

        public IActionResult Index()
        {
            Item[] Items = dbc.Items.Where(i => i.IsDeleted == false).ToArray();
            return View(Items);
        }

        public IActionResult Details(int itemId)
        {
            Item itemFd = dbc.Items.FirstOrDefault(x => x.Id == itemId);
            return View(itemFd);
        }

        public IActionResult Delete(int itemId)
        {
            Item itemFd = dbc.Items.FirstOrDefault(x => x.Id == itemId);
            if (itemFd != null)
            {
                itemFd.IsDeleted = true;
                dbc.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        // GET: Items/Add
        [HttpGet]
        public IActionResult Add()
        {
            return View();
        }

        // POST: Items/Add
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add([Bind("Name,Description,Price,MainPicUrl,PicturesUrls")] Item item, IFormFile? MainPicFile, List<IFormFile>? PicturesFiles)
        {
            if (item == null)
                return BadRequest();

            // Basic validation
            if (string.IsNullOrWhiteSpace(item.Name))
            {
                ModelState.AddModelError(nameof(item.Name), "Name is required.");
            }
            if (item.Price < 0)
            {
                ModelState.AddModelError(nameof(item.Price), "Price must be >= 0.");
            }

            // Require at least one main picture source: uploaded file OR URL
            var hasMainFile = MainPicFile != null && MainPicFile.Length > 0;
            var hasMainUrl = !string.IsNullOrWhiteSpace(item.MainPicUrl);
            if (!hasMainFile && !hasMainUrl)
            {
                // Use a model-level error (shown in validation summary)
                ModelState.AddModelError(string.Empty, "Provide a main picture by uploading a file or entering a URL.");
            }

            if (!ModelState.IsValid)
            {
                return View(item);
            }

            // Ensure upload directory exists
            var uploadsRoot = Path.Combine(env.WebRootPath ?? "wwwroot", "uploads", "items");
            Directory.CreateDirectory(uploadsRoot);

            // If user uploaded a main picture file, save it and override MainPicUrl
            if (hasMainFile)
            {
                var mainFileName = $"{Guid.NewGuid()}{Path.GetExtension(MainPicFile!.FileName)}";
                var mainPath = Path.Combine(uploadsRoot, mainFileName);
                using (var stream = new FileStream(mainPath, FileMode.Create))
                {
                    await MainPicFile.CopyToAsync(stream);
                }
                item.MainPicUrl = "/uploads/items/" + mainFileName;
            }

            // If user uploaded additional pictures, save them and append their URLs to PicturesUrls (pipe separated)
            if (PicturesFiles != null && PicturesFiles.Count > 0)
            {
                var saved = new List<string>();
                foreach (var file in PicturesFiles.Where(f => f != null && f.Length > 0))
                {
                    var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
                    var filePath = Path.Combine(uploadsRoot, fileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }
                    saved.Add("/uploads/items/" + fileName);
                }

                var existing = string.IsNullOrWhiteSpace(item.PicturesUrls) ? Array.Empty<string>() : item.PicturesUrls.Split("|", StringSplitOptions.RemoveEmptyEntries);
                item.PicturesUrls = string.Join("|", existing.Concat(saved));
            }

            item.IsDeleted = false;
            dbc.Items.Add(item);
            await dbc.SaveChangesAsync();

            return RedirectToAction("Index");
        }
    }
}
