using ClinicBooking.Web.Data;
using ClinicBooking.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace ClinicBooking.Web.Pages.Bookings;

public class CreateModel : PageModel
{
    private readonly AppDbContext _context;

    public CreateModel(AppDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public Booking Booking { get; set; } = default!;

    // SelectList per i dropdown
    public SelectList PatientsSelectList { get; set; } = default!;
    public SelectList DoctorsSelectList { get; set; } = default!;

    public async Task<IActionResult> OnGetAsync()
    {
        // Inizializza la data/ora: oggi + arrotonda all'ora successiva
        Booking = new Booking
        {
            DateTime = RoundToNextHour(DateTime.Now)
        };

        await LoadSelectListsAsync();

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            await LoadSelectListsAsync();
            return Page();
        }
        // ➕ Calcolo automatico dei campi calendario per la vista Agenda
        Booking.BookingDate = DateOnly.FromDateTime(Booking.DateTime);  // solo data (per filtri Agenda)
        Booking.BookingTime = TimeOnly.FromDateTime(Booking.DateTime);  // solo ora  (per ordinamenti/slot)

        // (opzionale) vincolo slot 30'
        // ➕ Controllo slot 30 minuti
        var minutes = Booking.BookingTime.Minute;
        if (minutes != 0 && minutes != 30)
        {
            ModelState.AddModelError("Booking.BookingTime", "Usare slot da 30 minuti (es. 10:00, 10:30).");
            // Ricarica il form con l’errore
            await LoadSelectListsAsync();   // ✅ usa il tuo helper
            return Page();
        }

        _context.Bookings.Add(Booking);
        await _context.SaveChangesAsync();

        return RedirectToPage("./Index");
    }

    /// <summary>
    /// Handler AJAX per ottenere le specializzazioni filtrate per un dottore specifico
    /// Chiamato via JavaScript quando viene selezionato un dottore
    /// </summary>
    public async Task<IActionResult> OnGetSpecializationsByDoctorAsync(int doctorId)
    {
        // Query sulla tabella ponte DoctorSpecializations
        var specializations = await _context.DoctorSpecializations
            .Where(ds => ds.DoctorId == doctorId)
            .Include(ds => ds.Specialization)
            .Select(ds => new
            {
                id = ds.SpecializationId,
                name = ds.Specialization.Name
            })
            .OrderBy(s => s.name)
            .ToListAsync();

        return new JsonResult(specializations);
    }

    /// <summary>
    /// Carica le SelectList per Pazienti e Dottori
    /// Nessun elemento è pre-selezionato (primo elemento vuoto)
    /// </summary>
    private async Task LoadSelectListsAsync()
    {
        // Pazienti: carica tutti i pazienti ordinati per nome
        var patients = await _context.Patients
            .OrderBy(p => p.Name)
            .Select(p => new
            {
                p.PatientId,
                p.Name
            })
            .ToListAsync();

        PatientsSelectList = new SelectList(
            patients,
            "PatientId",
            "Name",
            null  // Nessuna selezione di default
        );

        // Dottori: carica tutti i dottori ordinati per nome
        var doctors = await _context.Doctors
            .OrderBy(d => d.Name)
            .Select(d => new
            {
                d.DoctorId,
                DisplayName = "Dr. " + d.Name
            })
            .ToListAsync();

        DoctorsSelectList = new SelectList(
            doctors,
            "DoctorId",
            "DisplayName",
            null  // Nessuna selezione di default
        );
    }

    /// <summary>
    /// Arrotonda la data/ora all'ora successiva
    /// Es: 14:30 → 15:00, 14:00 → 14:00
    /// </summary>
    private static DateTime RoundToNextHour(DateTime dt)
    {
        var rounded = new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, 0, 0);

        // Se ci sono minuti o secondi, passa all'ora successiva
        if (dt.Minute > 0 || dt.Second > 0)
        {
            rounded = rounded.AddHours(1);
        }

        return rounded;
    }
}