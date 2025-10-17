using ClinicBooking.Web.Data;
using ClinicBooking.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ClinicBooking.Web.Pages.Bookings;

public class DeleteModel : PageModel
{
    private readonly AppDbContext _context;

    public DeleteModel(AppDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public Booking Booking { get; set; } = default!;

    /// <summary>
    /// GET: Mostra pagina di conferma eliminazione
    /// </summary>
    public async Task<IActionResult> OnGetAsync(int id)
    {
        // Carica l'appuntamento con tutte le entità correlate per visualizzazione
        var booking = await _context.Bookings
            .AsNoTracking()
            .Include(b => b.Patient)
            .Include(b => b.Doctor)
            .Include(b => b.Specialization)
            .FirstOrDefaultAsync(b => b.BookingId == id);

        if (booking == null)
        {
            return NotFound();
        }

        Booking = booking;
        return Page();
    }

    /// <summary>
    /// POST: Elimina definitivamente l'appuntamento
    /// </summary>
    public async Task<IActionResult> OnPostAsync(int id)
    {
        // Carica l'entità da eliminare (con tracking)
        var booking = await _context.Bookings.FindAsync(id);

        if (booking == null)
        {
            TempData["Message"] = "L'appuntamento non esiste o è già stato eliminato.";
            return RedirectToPage("./Index");
        }

        try
        {
            _context.Bookings.Remove(booking);
            await _context.SaveChangesAsync();

            TempData["Message"] = "Appuntamento eliminato con successo.";
            return RedirectToPage("./Index");
        }
        catch (DbUpdateException)
        {
            // Log dell'errore (se hai un sistema di logging)
            // _logger.LogError(ex, "Errore durante l'eliminazione dell'appuntamento {BookingId}", id);

            ModelState.AddModelError(string.Empty,
                "Si è verificato un errore durante l'eliminazione. " +
                "L'appuntamento potrebbe essere collegato ad altri dati. " +
                "Riprova o contatta l'amministratore.");

            // Ricarica i dati per mostrare nuovamente la pagina
            var bookingReload = await _context.Bookings
                .AsNoTracking()
                .Include(b => b.Patient)
                .Include(b => b.Doctor)
                .Include(b => b.Specialization)
                .FirstOrDefaultAsync(b => b.BookingId == id);

            if (bookingReload != null)
            {
                Booking = bookingReload;
            }

            return Page();
        }
    }
}