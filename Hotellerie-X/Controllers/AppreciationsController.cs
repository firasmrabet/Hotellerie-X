using Hotellerie_X.Models.HotellerieModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Hotellerie_X.Controllers
{
    public class AppreciationsController : Controller
    {
        private readonly HotellerieDbContext _context;

        public AppreciationsController(HotellerieDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var list = await _context.Appreciations.Include(a => a.Hotel).ToListAsync();
            return View(list);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var app = await _context.Appreciations.Include(a => a.Hotel).FirstOrDefaultAsync(a => a.Id == id);
            if (app == null) return NotFound();
            return View(app);
        }

        public IActionResult Create()
        {
            var hotels = _context.Hotels.Select(h => new { h.Id, h.Nom }).ToList();
            ViewBag.Hotels = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(hotels, "Id", "Nom");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Appreciation appreciation)
        {
            if (ModelState.IsValid)
            {
                _context.Add(appreciation);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            var hotels = _context.Hotels.Select(h => new { h.Id, h.Nom }).ToList();
            ViewBag.Hotels = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(hotels, "Id", "Nom");
            return View(appreciation);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var app = await _context.Appreciations.FindAsync(id);
            if (app == null) return NotFound();
            var hotels = _context.Hotels.Select(h => new { h.Id, h.Nom }).ToList();
            ViewBag.Hotels = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(hotels, "Id", "Nom");
            return View(app);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Appreciation appreciation)
        {
            if (id != appreciation.Id) return NotFound();
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(appreciation);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Appreciations.Any(e => e.Id == appreciation.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            var hotels = _context.Hotels.Select(h => new { h.Id, h.Nom }).ToList();
            ViewBag.Hotels = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(hotels, "Id", "Nom");
            return View(appreciation);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var app = await _context.Appreciations.Include(a => a.Hotel).FirstOrDefaultAsync(a => a.Id == id);
            if (app == null) return NotFound();
            return View(app);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var app = await _context.Appreciations.FindAsync(id);
            if (app != null)
            {
                _context.Appreciations.Remove(app);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
