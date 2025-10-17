using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ClinicBooking.Web.Data;
using ClinicBooking.Web.Models;

namespace ClinicBooking.Web.Pages.Bookings;

[Authorize(Roles = "Doctor")] // Solo utenti con ruolo Doctor possono accedere
public class MyAppointmentsModel : PageModel
{
    private readonly AppDbContext _context;
    private readonly UserManager<IdentityUser> _userManager;

    public MyAppointmentsModel(AppDbContext context, UserManager<IdentityUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // Proprietà per la vista
    public IList<Booking> TodayAppointments { get; set; } = new List<Booking>();
    public string DoctorName { get; set; } = "";
    public DateOnly Today { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    public async Task<IActionResult> OnGetAsync()
    {
        // Ottieni l'utente attualmente loggato
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser?.Email == null)
        {
            return RedirectToPage("/Identity/Account/Login");
        }

        // Trova il Doctor corrispondente tramite email
        var doctor = await _context.Doctors
            .FirstOrDefaultAsync(d => d.Email == currentUser.Email);

        if (doctor == null)
        {
            // Doctor non trovato - problema di sincronizzazione
            ModelState.AddModelError("", "Profilo dottore non trovato. Contattare l'amministratore.");
            return Page();
        }

        DoctorName = doctor.Name;

        // Carica appuntamenti di oggi per questo dottore
        TodayAppointments = await _context.Bookings
            .Include(b => b.Patient)           // Include dati paziente
            .Include(b => b.Specialization)    // Include specializzazione
            .Where(b => b.DoctorId == doctor.DoctorId &&  // Solo questo dottore
                       b.BookingDate == Today)             // Solo appuntamenti di oggi
            .OrderBy(b => b.BookingTime)                   // Ordinati per ora
            .ToListAsync();

        return Page();
    }

    /// <summary>
    /// Gestisce il cambio di stato degli appuntamenti dal dottore
    /// </summary>
    public async Task<IActionResult> OnPostUpdateStatusAsync(int bookingId, BookingStatus newStatus)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser?.Email == null) return RedirectToPage("/Identity/Account/Login");

        // Trova il Doctor per verificare autorizzazione
        var doctor = await _context.Doctors
            .FirstOrDefaultAsync(d => d.Email == currentUser.Email);

        if (doctor == null) return BadRequest();

        // Trova la prenotazione e verifica che appartenga a questo dottore
        var booking = await _context.Bookings
            .FirstOrDefaultAsync(b => b.BookingId == bookingId && b.DoctorId == doctor.DoctorId);

        if (booking == null) return NotFound();

        // Aggiorna stato solo se transizione valida
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

        return RedirectToPage();
    }

    /// <summary>
    /// Verifica se la transizione di stato è valida per il dottore
    /// </summary>
    private static bool IsValidTransition(BookingStatus currentStatus, BookingStatus newStatus)
    {
        return currentStatus switch
        {
            BookingStatus.Scheduled => newStatus is BookingStatus.Confirmed or BookingStatus.Cancelled,
            BookingStatus.Confirmed => newStatus is BookingStatus.Completed or BookingStatus.Cancelled,
            _ => false  // Eseguita e Annullata sono stati finali
        };
    }
}