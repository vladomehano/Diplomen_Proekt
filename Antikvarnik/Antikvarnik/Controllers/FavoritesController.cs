using Antikvarnik.Data;
using Antikvarnik.Models;
using Antikvarnik.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Antikvarnik.Controllers
{
    [Authorize]
    public class FavoritesController : Controller
    {
        private readonly ApplicationDbContext dbc;
        private readonly UserManager<AppUser> userManager;

        public FavoritesController(ApplicationDbContext dbContext, UserManager<AppUser> userManager)
        {
            dbc = dbContext;
            this.userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Mine()
        {
            var currentUser = await userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Challenge();
            }

            var items = await dbc.Items
     .AsNoTracking()
     .Include(i => i.Category)
     .Include(i => i.Images)
     .Where(i => !i.IsDeleted &&
         i.FavoritedByUsers.Any(f => f.UserId == currentUser.Id) &&
         (i.Status == ItemStatus.Available || i.Status == ItemStatus.Reserved || i.Status == ItemStatus.Sold))
     .OrderByDescending(i => i.CreatedOn)
     .ToArrayAsync();

            return View(items);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Toggle(int itemId, string? returnUrl = null)
        {
            var currentUser = await userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Challenge();
            }

            var itemExists = await dbc.Items.AnyAsync(i => i.Id == itemId && !i.IsDeleted);
            if (!itemExists)
            {
                return NotFound();
            }

            var favorite = await dbc.Favorites.FindAsync(currentUser.Id, itemId);
            if (favorite == null)
            {
                dbc.Favorites.Add(new Favorite
                {
                    UserId = currentUser.Id,
                    ItemId = itemId,
                    CreatedOn = DateTime.UtcNow
                });
                TempData["StatusMessage"] = "Артикулът е добавен в Любими.";
            }
            else
            {
                dbc.Favorites.Remove(favorite);
                TempData["StatusMessage"] = "Артикулът е премахнат от Любими.";
            }

            await dbc.SaveChangesAsync();

            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Index", "Items");
        }
    }
}