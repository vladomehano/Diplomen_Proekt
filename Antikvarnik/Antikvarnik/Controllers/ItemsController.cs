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
            Item[] Items = dbc.Items.ToArray();
            return View(Items);
        }
        public IActionResult Details(int itemId)
        {
            Item itemFd = dbc.Items.FirstOrDefault(x => x.Id == itemId);
            return View(itemFd);
        }
    }
}
