using Antikvarnik.Data;
using Antikvarnik.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Antikvarnik.Controllers
{
    public class ReviewsController : Controller
    {
        private readonly ApplicationDbContext dbc;
        private readonly UserManager<AppUser> userManager;

        public ReviewsController(ApplicationDbContext dbContext, UserManager<AppUser> userManager)
        {
            dbc = dbContext;
            this.userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var reviews = await dbc.Reviews
                .AsNoTracking()
                .Include(r => r.User)
                .Where(r => r.IsApproved && !r.IsHidden)
                .OrderByDescending(r => r.CreatedOn)
                .ToArrayAsync();

            return View(reviews);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string content, int rating)
        {
            var currentUser = await userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Challenge();
            }

            if (string.IsNullOrWhiteSpace(content))
            {
                TempData["ReviewStatusMessage"] = "Моля въведи текст за отзива.";
                return RedirectToAction(nameof(Index));
            }

            if (rating < 1 || rating > 5)
            {
                TempData["ReviewStatusMessage"] = "Оценката трябва да е между 1 и 5.";
                return RedirectToAction(nameof(Index));
            }

            var review = new Review
            {
                Content = content.Trim(),
                Rating = rating,
                UserId = currentUser.Id,
                CreatedOn = DateTime.UtcNow,
                IsApproved = true,
                IsHidden = false
            };

            dbc.Reviews.Add(review);
            await dbc.SaveChangesAsync();

            TempData["ReviewStatusMessage"] = "Благодарим! Отзивът е публикуван успешно.";
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Hide(int reviewId)
        {
            var review = await dbc.Reviews.FirstOrDefaultAsync(r => r.Id == reviewId);
            if (review == null)
            {
                return NotFound();
            }

            review.IsHidden = true;
            await dbc.SaveChangesAsync();
            TempData["ReviewStatusMessage"] = "Отзивът е скрит.";

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int reviewId)
        {
            var review = await dbc.Reviews.FirstOrDefaultAsync(r => r.Id == reviewId);
            if (review == null)
            {
                return NotFound();
            }

            dbc.Reviews.Remove(review);
            await dbc.SaveChangesAsync();
            TempData["ReviewStatusMessage"] = "Отзивът е изтрит.";

            return RedirectToAction(nameof(Index));
        }
    }
}