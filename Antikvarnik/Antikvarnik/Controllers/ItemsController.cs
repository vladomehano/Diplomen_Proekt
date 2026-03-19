using Antikvarnik.Data;

using Antikvarnik.Models;
using Antikvarnik.Models.Enums;
using Antikvarnik.ViewModels;
using Microsoft.AspNetCore.Authorization;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;


namespace Antikvarnik.Controllers
{
    public class ItemsController : Controller
    {
        private readonly ApplicationDbContext dbc;
        private readonly IWebHostEnvironment env;
        private readonly UserManager<AppUser> userManager;

      
        public ItemsController(ApplicationDbContext dbContext, IWebHostEnvironment env, UserManager<AppUser> userManager)
        {
            
            dbc = dbContext;
            this.env = env;
            this.userManager = userManager;
        }


        public async Task<IActionResult> Index()
        {
            var currentUser = await userManager.GetUserAsync(User);
            var isAdmin = User.IsInRole("Admin");
            var currentUserId = currentUser?.Id;

            Item[] items = await dbc.Items
                .AsNoTracking()
                .Include(i => i.Category)
                .Include(i => i.Images)
                .Where(i => !i.IsDeleted &&
                    (i.Status == ItemStatus.Available ||
                     i.Status == ItemStatus.Reserved ||
                     i.Status == ItemStatus.Sold ||
                     (currentUserId != null && i.SellerId == currentUserId) ||
                     isAdmin))
                .OrderByDescending(i => i.CreatedOn)
                .ToArrayAsync();

            return View(items);
        }

        public async Task<IActionResult> Details(int itemId)
        {
            Item? item = await dbc.Items
                .AsNoTracking()
                .Include(i => i.Category)
                .Include(i => i.Images.OrderBy(ii => ii.SortOrder))
                .FirstOrDefaultAsync(x => x.Id == itemId && !x.IsDeleted);

            if (item == null)
            {
                return NotFound();
            }

            return View(item);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> Waiting()
        {
            Item[] waitingItems = await dbc.Items
                .AsNoTracking()
                .Include(i => i.Category)
                .Include(i => i.Images)
                .Include(i => i.Seller)
                .Where(i => !i.IsDeleted && i.Status == ItemStatus.Waiting)
                .OrderBy(i => i.CreatedOn)
                .ToArrayAsync();

            return View(waitingItems);
        }
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int itemId)
        {
            Item? item = await dbc.Items.FirstOrDefaultAsync(x => x.Id == itemId && !x.IsDeleted);
            if (item == null)
            {
                return NotFound();
            }

            if (item.Status != ItemStatus.Waiting)
            {
                return BadRequest();
            }

            var currentUser = await userManager.GetUserAsync(User);
            item.Status = ItemStatus.Available;
            item.ApprovedByAdminId = currentUser?.Id;
            item.UpdatedOn = DateTime.UtcNow;

            await dbc.SaveChangesAsync();
            TempData["StatusMessage"] = $"Артикулът \"{item.Name}\" беше одобрен и е вече видим в каталога.";

            return RedirectToAction(nameof(Waiting));
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int itemId)
        {
            Item? item = await dbc.Items.FirstOrDefaultAsync(x => x.Id == itemId && !x.IsDeleted);
            if (item == null)
            {
                return NotFound();
            }

            if (item.Status != ItemStatus.Waiting)
            {
                return BadRequest();
            }

            item.Status = ItemStatus.Rejected;
            item.ApprovedByAdminId = null;
            item.UpdatedOn = DateTime.UtcNow;

            await dbc.SaveChangesAsync();
            TempData["StatusMessage"] = $"Артикулът \"{item.Name}\" беше отхвърлен.";

            return RedirectToAction(nameof(Waiting));
        }


            [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int itemId)
        {
                Item? item = await dbc.Items.FirstOrDefaultAsync(x => x.Id == itemId);
            if (item != null)
            {
                item.IsDeleted = true;
                item.UpdatedOn = DateTime.UtcNow;
                await dbc.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Items/Deleted
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> Deleted()
        {
            Item[] deletedItems = await dbc.Items
                .AsNoTracking()
                .Include(i => i.Category)
                .Include(i => i.Images)
                .Where(i => i.IsDeleted)
                .OrderByDescending(i => i.UpdatedOn ?? i.CreatedOn)
                .ToArrayAsync();

            return View(deletedItems);
        }

        // POST: Items/Restore
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Restore(int itemId)
        {
                Item? item = await dbc.Items.FirstOrDefaultAsync(x => x.Id == itemId);
            if (item != null)
            {
                item.IsDeleted = false;
                item.UpdatedOn = DateTime.UtcNow;
                await dbc.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Items/Add
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Add()
        {
            var viewModel = new ItemFormViewModel
            {
                Categories = await GetCategoryOptionsAsync()
            };

            return View(viewModel);
        }

        // POST: Items/Add
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(ItemFormViewModel model)
        {
            if (model.MainPicFile == null || model.MainPicFile.Length == 0)
            {
                ModelState.AddModelError(nameof(model.MainPicFile), "Главната снимка е задължителна.");
            }

            if (!await dbc.Categories.AnyAsync(c => c.Id == model.CategoryId))
            {
                ModelState.AddModelError(nameof(model.CategoryId), "Избраната категория не съществува.");
            }
            if (!ModelState.IsValid)
            {
                model.Categories = await GetCategoryOptionsAsync();
                return View(model);
            }


            // Ensure upload directory exists
            var uploadsRoot = Path.Combine(env.WebRootPath ?? "wwwroot", "uploads", "items");
            Directory.CreateDirectory(uploadsRoot);

            // If user uploaded a main picture file, save it and override MainPicUrl
            var currentUser = await userManager.GetUserAsync(User);
            var item = new Item { 
                Name = model.Name.Trim(),
                Description = model.Description.Trim(),
                Price = model.Price,
                Condition = model.Condition.Trim(),
                YearOrPeriod = string.IsNullOrWhiteSpace(model.YearOrPeriod) ? null : model.YearOrPeriod.Trim(),
                CategoryId = model.CategoryId,
                SellerId = currentUser?.Id,
                Status = User.IsInRole("Admin") ? ItemStatus.Available : ItemStatus.Waiting,
                CreatedOn = DateTime.UtcNow,
                UpdatedOn = DateTime.UtcNow,
            };

    var imageOrder = 0;
    var mainImageUrl = await SaveUploadedImageAsync(model.MainPicFile!, uploadsRoot);
    item.Images.Add(new ItemImage
            {
                
            
                ImageUrl = mainImageUrl,
                IsMain = true,
                SortOrder = imageOrder++
            });

    foreach (var file in model.PicturesFiles.Where(f => f != null && f.Length > 0))
    {
        var imageUrl = await SaveUploadedImageAsync(file, uploadsRoot);
        item.Images.Add(new ItemImage
        {
            ImageUrl = imageUrl,
            IsMain = false,
            SortOrder = imageOrder++
        });
    }

dbc.Items.Add(item);

await dbc.SaveChangesAsync();

return RedirectToAction(nameof(Index));
        }

private async Task<string> SaveUploadedImageAsync(IFormFile file, string uploadsRoot)
{
    var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
    var filePath = Path.Combine(uploadsRoot, fileName);

    await using var stream = new FileStream(filePath, FileMode.Create);
    await file.CopyToAsync(stream);

    return "/uploads/items/" + fileName;
}

private async Task<IEnumerable<SelectListItem>> GetCategoryOptionsAsync()
{
    return await dbc.Categories
        .AsNoTracking()
        .OrderBy(c => c.Name)
        .Select(c => new SelectListItem
        {
            Value = c.Id.ToString(),
            Text = c.Name
        })
        .ToListAsync();
}
    }
}