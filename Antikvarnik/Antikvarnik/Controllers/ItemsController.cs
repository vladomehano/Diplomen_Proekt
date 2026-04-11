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
            
            var currentUserId = currentUser?.Id;

            Item[] items = await dbc.Items
                .AsNoTracking()
                .Include(i => i.Category)
                .Include("Images")
                .Where(i => !i.IsDeleted &&
                    (i.Status == ItemStatus.Available ||
                     i.Status == ItemStatus.Sold))
                .OrderByDescending(i => i.CreatedOn)
                .ToArrayAsync();
            if (currentUserId != null)
            {
                ViewBag.FavoriteItemIds = await dbc.Favorites
                    .AsNoTracking()
                    .Where(f => f.UserId == currentUserId)
                    .Select(f => f.ItemId)
                    .ToArrayAsync();
            }

            return View(items);
        }
        [HttpGet]
        public async Task<IActionResult> Sold()
        {
            Item[] soldItems = await dbc.Items
                .AsNoTracking()
                .Include(i => i.Category)
                .Include("Images")
                .Where(i => !i.IsDeleted && i.Status == ItemStatus.Sold)
                .OrderByDescending(i => i.UpdatedOn ?? i.CreatedOn)
                .ToArrayAsync();

            return View(soldItems);
        }


        public async Task<IActionResult> Details(int itemId)
        {
            Item? item = await dbc.Items
                .AsNoTracking()
                .Include(i => i.Category)
                .Include("Images")
                .Include(i => i.Messages.OrderBy(m => m.SentOn))
                    .ThenInclude(m => m.Sender)
                .FirstOrDefaultAsync(x => x.Id == itemId && !x.IsDeleted);

            if (item == null)
            {
                return NotFound();
            }
            if (!await CanViewItemAsync(item))
            {
                return Forbid();
            }

            var currentUser = await userManager.GetUserAsync(User);
            if (currentUser?.Id != null)
            {
                ViewBag.IsFavorite = await dbc.Favorites
                    .AsNoTracking()
                    .AnyAsync(f => f.UserId == currentUser.Id && f.ItemId == item.Id);
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
                .Include("Images")
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
        [HttpGet]
        public async Task<IActionResult> Edit(int itemId)
        {
            Item? item = await dbc.Items
                .AsNoTracking()
                .Include("Images")
                .FirstOrDefaultAsync(x => x.Id == itemId && !x.IsDeleted);

            if (item == null)
            {
                return NotFound();
            }


            var viewModel = new ItemFormViewModel
            {
                Id = item.Id,
                Name = item.Name,
                Description = item.Description,
                Price = item.Price,
                Condition = item.Condition,
                YearOrPeriod = item.YearOrPeriod,
                CategoryId = item.CategoryId,
                ExistingMainPicUrl = item.MainPicUrl,
                ExistingPictures = item.Pictures,
                Categories = await GetCategoryOptionsAsync()
            };

            return View(viewModel);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int itemId, ItemFormViewModel model)
        {
            Item? item = await dbc.Items
                .Include("Images")
                .FirstOrDefaultAsync(x => x.Id == itemId && !x.IsDeleted);

            if (item == null)
            {
                return NotFound();
            }


            if (!await dbc.Categories.AnyAsync(c => c.Id == model.CategoryId))
            {
                ModelState.AddModelError(nameof(model.CategoryId), "Избраната категория не съществува.");
            }

            if (!ModelState.IsValid)
            {
                model.Id = item.Id;
                model.ExistingMainPicUrl = item.MainPicUrl;
                model.ExistingPictures = item.Pictures;
                model.Categories = await GetCategoryOptionsAsync();
                return View(model);
            }

            item.Name = model.Name.Trim();
            item.Description = model.Description.Trim();
            item.Price = model.Price;
            item.Condition = model.Condition.Trim();
            item.YearOrPeriod = string.IsNullOrWhiteSpace(model.YearOrPeriod) ? null : model.YearOrPeriod.Trim();
            item.CategoryId = model.CategoryId;
            item.UpdatedOn = DateTime.UtcNow;

            var uploadsRoot = Path.Combine(env.WebRootPath ?? "wwwroot", "uploads", "items");
            Directory.CreateDirectory(uploadsRoot);

            if (model.MainPicFile != null && model.MainPicFile.Length > 0)
            {
                var newMainImageUrl = await SaveUploadedImageAsync(model.MainPicFile, uploadsRoot);
                var itemImages = GetImages(item);
                var currentMainImage = itemImages
                    .OrderByDescending(i => i.IsMain)
                    .ThenBy(i => i.SortOrder)
                    .FirstOrDefault();

                if (currentMainImage == null)
                {
                    itemImages.Add(new ItemImage
                    {
                        ImageUrl = newMainImageUrl,
                        IsMain = true,
                        SortOrder = 0
                    });
                }
                else
                {
                    currentMainImage.ImageUrl = newMainImageUrl;
                    currentMainImage.IsMain = true;
                }
            }

            var galleryImages = GetImages(item);
            var nextSortOrder = galleryImages.Any() ? galleryImages.Max(i => i.SortOrder) + 1 : 1;
            foreach (var file in model.PicturesFiles.Where(f => f != null && f.Length > 0))
            {
                var imageUrl = await SaveUploadedImageAsync(file, uploadsRoot);
                galleryImages.Add(new ItemImage
                {
                    ImageUrl = imageUrl,
                    IsMain = false,
                    SortOrder = nextSortOrder++
                });
            }

            TempData["StatusMessage"] = $"Артикулът \"{item.Name}\" беше редактиран успешно.";

            await dbc.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
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

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Buy(int itemId)
        {
            var currentUser = await userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Challenge();
            }

            var item = await dbc.Items.FirstOrDefaultAsync(i => i.Id == itemId && !i.IsDeleted);
            if (item == null)
            {
                return NotFound();
            }

            if (item.Status != ItemStatus.Available)
            {
                TempData["StatusMessage"] = "Този артикул вече не е наличен за покупка.";
                return RedirectToAction(nameof(Details), new { itemId });
            }

            item.Status = ItemStatus.Sold;
            item.ReservedByUserId = currentUser.Id;
            item.UpdatedOn = DateTime.UtcNow;

            await dbc.SaveChangesAsync();

            TempData["StatusMessage"] = $"Поръчката за \"{item.Name}\" е приета успешно.";
            return RedirectToAction(nameof(Details), new { itemId });
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddMessage(int itemId, string messageText)
        {
            if (string.IsNullOrWhiteSpace(messageText))
            {
                TempData["StatusMessage"] = "Съобщението не може да е празно.";
                return RedirectToAction(nameof(Details), new { itemId });
            }

            var currentUser = await userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Challenge();
            }

            var item = await dbc.Items.FirstOrDefaultAsync(i => i.Id == itemId && !i.IsDeleted);
            if (item == null)
            {
                return NotFound();
            }

            var isAdmin = User.IsInRole("Admin");
            var isOwner = item.SellerId == currentUser.Id;
            if (!isAdmin && !isOwner)
            {
                return Forbid();
            }

            dbc.ItemMessages.Add(new ItemMessage
            {
                ItemId = item.Id,
                SenderId = currentUser.Id,
                MessageText = messageText.Trim(),
                SentOn = DateTime.UtcNow
            });

            await dbc.SaveChangesAsync();
            return RedirectToAction(nameof(Details), new { itemId });
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAsAvailable(int itemId)
        {
            var item = await dbc.Items.FirstOrDefaultAsync(i => i.Id == itemId && !i.IsDeleted);
            if (item == null)
            {
                return NotFound();
            }

            if (item.Status != ItemStatus.Sold)
            {
                TempData["StatusMessage"] = "Само продаден артикул може да бъде върнат като наличен.";
                return RedirectToAction(nameof(Details), new { itemId });
            }

            item.Status = ItemStatus.Available;
            item.ReservedByUserId = null;
            item.UpdatedOn = DateTime.UtcNow;

            await dbc.SaveChangesAsync();
            TempData["StatusMessage"] = $"Артикулът \"{item.Name}\" е върнат обратно в каталога като наличен.";

            return RedirectToAction(nameof(Details), new { itemId });
        }

        // GET: Items/Deleted
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> Deleted()
        {
            Item[] deletedItems = await dbc.Items
                .AsNoTracking()
                .Include(i => i.Category)
                .Include("Images")
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
            var item = new Item
            {
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
            var itemImages = GetImages(item);
            itemImages.Add(new ItemImage
            {


                ImageUrl = mainImageUrl,
                IsMain = true,
                SortOrder = imageOrder++
            });

            foreach (var file in model.PicturesFiles.Where(f => f != null && f.Length > 0))
            {
                var imageUrl = await SaveUploadedImageAsync(file, uploadsRoot);
                itemImages.Add(new ItemImage
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

        private static ICollection<ItemImage> GetImages(Item item)
        {
            var images = item.GetType().GetProperty("Images")?.GetValue(item) as ICollection<ItemImage>;
            if (images == null)
            {
                throw new InvalidOperationException("Item.Images navigation is missing.");
            }

            return images;
        }


        private async Task<bool> CanViewItemAsync(Item item)
        {
            if (item.Status == ItemStatus.Available || item.Status == ItemStatus.Sold)
            {
                return true;
            }

            if (User.IsInRole("Admin"))
            {
                return true;
            }

            var currentUser = await userManager.GetUserAsync(User);
            return currentUser?.Id != null && item.SellerId == currentUser.Id;
        }

        private async Task<bool> CanEditItemAsync(Item item)
        {
            if (User.IsInRole("Admin"))
            {
                return true;
            }

            var currentUser = await userManager.GetUserAsync(User);
            return currentUser?.Id != null && item.SellerId == currentUser.Id;
        }
    }
}