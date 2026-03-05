using Antikvarnik.Data;
using Antikvarnik.Models;
using Microsoft.AspNetCore.Mvc;

namespace Antikvarnik.Controllers
{
    public class ItemsController : Controller
    {
        private readonly ApplicationDbContext dbc;

        public ItemsController(ApplicationDbContext dbContext)
        {
            this.dbc = dbContext;
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
    }
}
