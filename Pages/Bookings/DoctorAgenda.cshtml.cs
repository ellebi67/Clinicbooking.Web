using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ClinicBooking.Web.Data;
using ClinicBooking.Web.Models;

namespace ClinicBooking.Web.Pages.Bookings;

[Authorize(Roles = "Doctor")]
public class DoctorAgendaModel : PageModel
{
    private readonly AppDbContext _context;
    private readonly UserManager<IdentityUser> _userManager;

    public DoctorAgendaModel(AppDbContext context, UserManager<IdentityUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // Proprietà per la vista
    [BindProperty(SupportsGet = true)]
    public DateOnly SelectedDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    public IList<Booking> DayAppointments { get; set; } = new List<Booking>();
    public string DoctorName { get; set; } = "";
    public DateOnly Today { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    public async Task<IActionResult> OnGetAsync()
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser?.Email == null) return RedirectToPage("/Identity/Account/Login");

        var doctor = await _context.Doctors
            .FirstOrDefaultAsync(d => d.Email == currentUser.Email);

        if (doctor == null)
        {
            ModelState.AddModelError("", "Profilo dottore non trovato.");
            return Page();
        }

        DoctorName = doctor.Name;

        // Carica appuntamenti per la data selezionata
        DayAppointments = await _context.Bookings
            .Include(b => b.Patient)
            .Include(b => b.Specialization)
            .Where(b => b.DoctorId == doctor.DoctorId && b.BookingDate == SelectedDate)
            .OrderBy(b => b.BookingTime)
            .ToListAsync();

        return Page();
    }

    public async Task<IActionResult> OnPostUpdateStatusAsync(int bookingId, BookingStatus newStatus)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser?.Email == null) return RedirectToPage("/Identity/Account/Login");

        var doctor = await _context.Doctors
            .FirstOrDefaultAsync(d => d.Email == currentUser.Email);

        if (doctor == null) return BadRequest();

        var booking = await _context.Bookings
            .FirstOrDefaultAsync(b => b.BookingId == bookingId && b.DoctorId == doctor.DoctorId);

        if (booking == null) return NotFound();

        // Logica business: può modificare solo oggi e giorni passati
        if (booking.BookingDate > Today)
        {
            TempData["Error"] = "Non puoi modificare appuntamenti futuri.";
            return RedirectToPage(new { SelectedDate });
        }

        // Verifica transizione valida
        if (IsValidTransition(booking.Status, newStatus))
        {
            booking.Status = newStatus;
            await _context.SaveChangesAsync();
            TempData["Message"] = "Stato appuntamento aggiornato.";
        }
        else
        {
            TempData["Error"] = "Transizione di stato non valida.";
        }

        return RedirectToPage(new { SelectedDate });
    }

    private static bool IsValidTransition(BookingStatus currentStatus, BookingStatus newStatus)
    {
        return currentStatus switch
        {
            BookingStatus.Scheduled => newStatus is BookingStatus.Confirmed or BookingStatus.Cancelled,
            BookingStatus.Confirmed => newStatus is BookingStatus.Completed or BookingStatus.Cancelled,
            _ => false
        };
    }

    // Helper per determinare se data è modificabile (oggi o passato)
    public bool CanModifyDate(DateOnly date) => date <= Today;
}