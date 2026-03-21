using Antikvarnik.Data;
using Antikvarnik.Models;
using Antikvarnik.Models.Enums;
using Antikvarnik.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
public class ReservationsController : Controller
{
    private readonly ApplicationDbContext dbc;
    private readonly UserManager<AppUser> userManager;

    public ReservationsController(ApplicationDbContext dbContext, UserManager<AppUser> userManager)
    {
        dbc = dbContext;
        this.userManager = userManager;
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(int itemId)
    {
        var currentUser = await userManager.GetUserAsync(User);
        if (currentUser == null)
        {
            return Challenge();
        }

        Item? item = await dbc.Items
            .FirstOrDefaultAsync(i => i.Id == itemId && !i.IsDeleted);

        if (item == null)
        {
            return NotFound();
        }

        if (item.Status != ItemStatus.Available)
        {
            TempData["StatusMessage"] = "Този артикул в момента не е наличен за резервация.";
            return RedirectToAction("Details", "Items", new { itemId });
        }

        bool hasActiveReservation = await dbc.Reservations
            .AnyAsync(r => r.ItemId == itemId && r.Status == ReservationStatus.Active);

        if (hasActiveReservation)
        {
            TempData["StatusMessage"] = "За този артикул вече има активна резервация.";
            return RedirectToAction("Details", "Items", new { itemId });
        }

        var reservation = new Reservation
        {
            ItemId = item.Id,
            UserId = currentUser.Id,
            ReservedOn = DateTime.UtcNow,
            Status = ReservationStatus.Active
        };

        item.Status = ItemStatus.Reserved;
        item.ReservedByUserId = currentUser.Id;
        item.UpdatedOn = DateTime.UtcNow;

        dbc.Reservations.Add(reservation);
        await dbc.SaveChangesAsync();

        TempData["StatusMessage"] = $"Успешно заяви резервация за \"{item.Name}\".";
        return RedirectToAction(nameof(Mine));
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> Mine()
    {
        var currentUser = await userManager.GetUserAsync(User);
        if (currentUser == null)
        {
            return Challenge();
        }

        Reservation[] reservations = await dbc.Reservations
            .AsNoTracking()
            .Include(r => r.Item)
                .ThenInclude(i => i.Images)
            .Include(r => r.User)
            .Where(r => r.UserId == currentUser.Id)
            .OrderByDescending(r => r.ReservedOn)
            .ToArrayAsync();

        return View(MapReservationViewModels(reservations));
    }

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        Reservation[] reservations = await dbc.Reservations
            .AsNoTracking()
            .Include(r => r.Item)
                .ThenInclude(i => i.Images)
            .Include(r => r.User)
            .OrderByDescending(r => r.ReservedOn)
            .ToArrayAsync();

        return View(MapReservationViewModels(reservations));
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int reservationId)
    {
        Reservation? reservation = await dbc.Reservations
            .Include(r => r.Item)
            .FirstOrDefaultAsync(r => r.Id == reservationId);

        if (reservation == null)
        {
            return NotFound();
        }

        var currentUser = await userManager.GetUserAsync(User);
        var isAdmin = User.IsInRole("Admin");
        if (currentUser == null || (!isAdmin && reservation.UserId != currentUser.Id))
        {
            return Forbid();
        }

        if (reservation.Status != ReservationStatus.Active)
        {
            TempData["StatusMessage"] = "Тази резервация вече е обработена.";
            return RedirectToAction(isAdmin ? nameof(Index) : nameof(Mine));
        }

        reservation.Status = ReservationStatus.Cancelled;
        reservation.ProcessedOn = DateTime.UtcNow;
        reservation.ProcessedByAdminId = isAdmin ? currentUser.Id : null;

        reservation.Item.Status = ItemStatus.Available;
        reservation.Item.ReservedByUserId = null;
        reservation.Item.UpdatedOn = DateTime.UtcNow;

        await dbc.SaveChangesAsync();

        TempData["StatusMessage"] = $"Резервацията за \"{reservation.Item.Name}\" беше отменена.";
        return RedirectToAction(isAdmin ? nameof(Index) : nameof(Mine));
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Complete(int reservationId)
    {
        Reservation? reservation = await dbc.Reservations
            .Include(r => r.Item)
            .FirstOrDefaultAsync(r => r.Id == reservationId);

        if (reservation == null)
        {
            return NotFound();
        }

        if (reservation.Status != ReservationStatus.Active)
        {
            TempData["StatusMessage"] = "Тази резервация вече е обработена.";
            return RedirectToAction(nameof(Index));
        }

        var currentUser = await userManager.GetUserAsync(User);
        reservation.Status = ReservationStatus.Completed;
        reservation.ProcessedOn = DateTime.UtcNow;
        reservation.ProcessedByAdminId = currentUser?.Id;

        reservation.Item.Status = ItemStatus.Sold;
        reservation.Item.UpdatedOn = DateTime.UtcNow;

        await dbc.SaveChangesAsync();

        TempData["StatusMessage"] = $"Артикулът \"{reservation.Item.Name}\" беше маркиран като продаден.";
        return RedirectToAction(nameof(Index));
    }
    private static ReservationListItemViewModel[] MapReservationViewModels(IEnumerable<Reservation> reservations)
    {
        return reservations
            .Select(r => new ReservationListItemViewModel
            {
                ReservationId = r.Id,
                ItemId = r.ItemId,
                ItemName = r.Item.Name,
                ItemImageUrl = r.Item.MainPicUrl,
                ItemPrice = r.Item.Price,
                ItemStatus = r.Item.Status,
                ReservationStatus = r.Status,
                ReservedOn = r.ReservedOn,
                ProcessedOn = r.ProcessedOn,
                UserEmail = r.User.Email ?? string.Empty
            })
            .ToArray();
    }
}