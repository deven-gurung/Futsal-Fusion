using FutsalFusion.Domain.Helper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace FutsalFusion.Controllers;

public class ReservationController : Controller
{
    private readonly FutsalReservationContext _context;
    
    private readonly List<String> _timings =
    [
        "5am - 6am", "6am - 7am", "7am - 8am", "8am - 9am", "9am - 10am", "10am - 11am", "11am - 12pm", "12pm - 1pm",
        "1pm - 2pm", "2pm - 3pm", "3pm - 4pm", "4pm - 5pm", "5pm - 6pm", "6pm - 7pm", "7pm - 8pm", "8pm - 9pm",
        "9pm - 10pm"
    ];
    
    private readonly DateTime _date = DateTime.Now;
    
    private List<String> _availableTimings = [];
    
    public ReservationController(FutsalReservationContext context)
    {
        _context = context;
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Index()
    {
        var currentUserName = User.Claims.First(c => c.Type == "UserName").Value;
        
        var currentUser = _context.User.First(u => u.UserName == currentUserName);
        
        ViewData["UserId"] = currentUser.Id;
        
        var futsalReservationContext = _context.Reservation.Include(r => r.Court).Include(r => r.User);
        
        return Json(await futsalReservationContext.ToListAsync());
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var reservation = await _context.Reservation
            .Include(r => r.Court)
            .Include(r => r.User)
            .FirstOrDefaultAsync(m => m.ReservationId == id);
        
        if (reservation == null)
        {
            return NotFound();
        }

        return Json(reservation);
    }

    [Authorize(Roles = "Admin, Normal")]
    public IActionResult UserCreate(int? id)
    {
        var today = $"{_date.Year}/{_date.Month}/{_date.Day}";
        
        AvailableTimings(today);
        
        ViewData["Today"] = $"{_date.Year}/{_date.Month}/{_date.Day}";
        
        ViewData["Timings"] = new SelectList(_availableTimings);
        
        ViewData["CourtId"] = new SelectList(_context.Court, "Id", "Name");
        
        ViewData["UserId"] = new SelectList("", "Id", "Email");
        
        return Json(new {});
    }

    [Authorize(Roles = "Admin")]
    public IActionResult Create()
    {
        var today = $"{_date.Year}/{_date.Month}/{_date.Day}";
        
        AvailableTimings(today);
        
        ViewData["Today"] = $"{_date.Year}/{_date.Month}/{_date.Day}";
        
        ViewData["Timings"] = new SelectList(_timings);
        
        ViewData["CourtId"] = new SelectList(_context.Court, "Id", "Name");
        
        ViewData["UserId"] = new SelectList(_context.User, "Id", "Email");
        
        return Json(new {});
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([Bind("ReservationId,UserId,CourtId,ReservationDate,ReservationTime")] Reservation reservation)
    {
        if (ModelState.IsValid)
        {
            return RedirectToAction(nameof(Index));
        }
        
        ViewData["CourtId"] = new SelectList(_context.Court, "Id", "Name", reservation.CourtId);
        
        ViewData["UserId"] = new SelectList(_context.User, "Id", "Email", reservation.UserId);
        
        return Json(reservation);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Normal")]
    public async Task<IActionResult> UserCreate([Bind("ReservationId,UserId,CourtId,ReservationDate,ReservationTime")] Reservation reservation)
    {
        if (ModelState.IsValid)
        {
            var userName = User.Claims.First(c => c.Type == "UserName").Value;
            
            var user = _context.User.First(u => u.UserName == userName);
            
            return Json(user);
        }
        
        ViewData["CourtId"] = new SelectList(_context.Court, "Id", "Name", reservation.CourtId);
        
        ViewData["UserId"] = new SelectList(_context.User, "Id", "Email", reservation.UserId);
        
        return Json(reservation);
    }

    [Authorize(Roles = "Admin, Normal")]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var reservation = await _context.Reservation.FindAsync(id);
        
        if (reservation == null)
        {
            return NotFound();
        }
        
        var today = $"{_date.Year}/{_date.Month}/{_date.Day}";
        
        AvailableTimings(today);
        
        ViewData["Today"] = $"{_date.Year}/{_date.Month}/{_date.Day}";
        
        ViewData["Timings"] = new SelectList(_availableTimings);
        
        ViewData["CourtId"] = new SelectList(_context.Court, "Id", "Name", reservation.CourtId);
        
        ViewData["UserId"] = new SelectList(_context.User, "Id", "Email", reservation.UserId);
        
        return Json(reservation);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin, Normal")]
    public async Task<IActionResult> Edit(int id, [Bind("ReservationId,UserId,CourtId,ReservationDate,ReservationTime")] Reservation reservation)
    {
        if (id != reservation.ReservationId)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ReservationExists(reservation.ReservationId))
                {
                    return NotFound();
                }

                throw;
            }
            return RedirectToAction(nameof(Index));
        }
        
        ViewData["CourtId"] = new SelectList(_context.Court, "Id", "Name", reservation.CourtId);
        
        ViewData["UserId"] = new SelectList(_context.User, "Id", "Email", reservation.UserId);
        
        return Json(reservation);
    }

    [Authorize(Roles = "Admin, Normal")]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var reservation = await _context.Reservation
            .Include(r => r.Court)
            .Include(r => r.User)
            .FirstOrDefaultAsync(m => m.ReservationId == id);
        
        if (reservation == null)
        {
            return NotFound();
        }

        return Json(reservation);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin, Normal")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var reservation = await _context.Reservation.FindAsync(id);
        
        if (reservation != null)
        {
            _context.Reservation.Remove(reservation);
        }
            
        return RedirectToAction(nameof(Index));
    }

    private bool ReservationExists(int id)
    {
        return _context.Reservation.Any(e => e.ReservationId == id);
    }

    public void AvailableTimings(string? date)
    {
        var reservationsForToday = _context.Reservation.Where(r => r.ReservationDate == date).ToList();
        
        if (reservationsForToday.Count == 0)
        {
            _availableTimings = _timings.ToList();
        }
        else
        {
            var reservedTimings = reservationsForToday.Select(reservation => reservation.ReservationTime).ToList();
            
            _availableTimings = _timings.Except(reservedTimings).ToList();
        }
    }
}