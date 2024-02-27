using FutsalFusion.Domain.Helper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace FutsalFusion.Controllers;

public class CourtController : Controller
{
    private readonly FutsalReservationContext _context;
        
    private readonly List<int> _hourTimings = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12];

    public CourtController(FutsalReservationContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var courtReservations = _context.Court
            .Include(r => r.Reservations)
            .Include(t => t.Timings);

        return Json(await courtReservations.ToListAsync());
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var court = await _context.Court.FirstOrDefaultAsync(m => m.Id == id);
            
        if (court == null)
        {
            return NotFound();
        }

        return Json(court);
    }

    public IActionResult Create()
    {
        return Json(new {});
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Id,Name")] Court court)
    {
        if (!ModelState.IsValid) return Json(court);
            
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var court = await _context.Court.FindAsync(id);
            
        if (court == null)
        {
            return NotFound();
        }
            
        ViewData["HourTimings"] = new SelectList(_hourTimings);
            
        return Json(court);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Name")] Court court,string endTime,string startTime)
    {
        var timing = new Timing(startTime, endTime);
           
        if (id != court.Id)
        {
            return NotFound();
        }

        if (!ModelState.IsValid) return Json(court);
            
        try
        {
            var toUpdateCourt = await _context.Court.FindAsync(id);
                
            if (toUpdateCourt != null)
            {
                toUpdateCourt.Timings = new List<Timing>();
                toUpdateCourt.Timings.Add(timing);
                toUpdateCourt.Name = court.Name;
            }
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!CourtExists(court.Id))
            {
                return NotFound();
            }

            throw;
        }
            
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var court = await _context.Court.FirstOrDefaultAsync(m => m.Id == id);
            
        if (court == null)
        {
            return NotFound();
        }

        return Json(court);
    }

    [ValidateAntiForgeryToken]
    [HttpPost, ActionName("Delete")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var court = await _context.Court.FindAsync(id);
            
        if (court != null)
        {
            _context.Court.Remove(court);
        }
            
        return RedirectToAction(nameof(Index));
    }

    private bool CourtExists(int id)
    {
        return _context.Court.Any(e => e.Id == id);
    }

    public async Task<IActionResult> ShowReservations(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }
            
        var court = _context.Court.First(c => c.Id == id);
            
        var courtReservations = await _context.Reservation.Include(r => r.User).Where(r => r.CourtId == id).ToListAsync();
            
        if (courtReservations.Count == 0)
        {
            ViewData["Message"] = "No reservations for " + court.Name;
        }
            
        ViewData["UserId"] = id;
            
        return Json(courtReservations);
    }
}