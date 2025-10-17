using ClinicBooking.Web.Data;
using ClinicBooking.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace ClinicBooking.Web.Pages.Bookings;

public class EditModel : PageModel
{
    private readonly AppDbContext _context;

    public EditModel(AppDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public Booking Booking { get; set; } = default!;

    // SelectList per i dropdown
    public SelectList PatientsSelectList { get; set; } = default!;
    public SelectList DoctorsSelectList { get; set; } = default!;
    public SelectList SpecializationsSelectList { get; set; } = default!;

    /// <summary>
    /// GET: Carica l'appuntamento da modificare
    /// </summary>
    public async Task<IActionResult> OnGetAsync(int id)
    {
        // Carica l'appuntamento con le entità correlate per visualizzazione
        var booking = await _context.Bookings
            .Include(b => b.Patient)
            .Include(b => b.Doctor)
            .Include(b => b.Specialization)
            .FirstOrDefaultAsync(b => b.BookingId == id);

        if (booking == null)
        {
            return NotFound();
        }

        Booking = booking;

        // Carica le liste per i dropdown
        await LoadSelectListsAsync();

        // Carica le specializzazioni del dottore corrente
        await LoadSpecializationsForDoctorAsync(Booking.DoctorId);

        return Page();
    }

    /// <summary>
    /// POST: Salva le modifiche all'appuntamento
    /// </summary>
    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            await LoadSelectListsAsync();
            await LoadSpecializationsForDoctorAsync(Booking.DoctorId);
            return Page();
        }

        // Carica l'entità da aggiornare (tracked)
        var bookingToUpdate = await _context.Bookings
            .FirstOrDefaultAsync(b => b.BookingId == Booking.BookingId);

        if (bookingToUpdate == null)
        {
            return NotFound();
        }

        // Verifica che la specializzazione appartenga al dottore selezionato
        var isValidSpecialization = await _context.DoctorSpecializations
            .AnyAsync(ds => ds.DoctorId == Booking.DoctorId &&
                           ds.SpecializationId == Booking.SpecializationId);

        if (!isValidSpecialization)
        {
            ModelState.AddModelError("Booking.SpecializationId",
                "La specializzazione selezionata non è valida per questo dottore.");
            await LoadSelectListsAsync();
            await LoadSpecializationsForDoctorAsync(Booking.DoctorId);
            return Page();
        }

        // Copia i campi modificabili
        bookingToUpdate.PatientId = Booking.PatientId;
        bookingToUpdate.DoctorId = Booking.DoctorId;
        bookingToUpdate.SpecializationId = Booking.SpecializationId;
        bookingToUpdate.DateTime = Booking.DateTime;
        bookingToUpdate.DurationMinutes = Booking.DurationMinutes;
        bookingToUpdate.Notes = Booking.Notes;

        // ➕ PATCH A: aggiorno i campi per la vista Agenda in base al nuovo DateTime
        bookingToUpdate.BookingDate = DateOnly.FromDateTime(bookingToUpdate.DateTime);
        bookingToUpdate.BookingTime = TimeOnly.FromDateTime(bookingToUpdate.DateTime);

        // ➕ Vincolo slot 30' (minuti 00 o 30)
        var minuti = bookingToUpdate.BookingTime.Minute;
        if (minuti != 0 && minuti != 30)
        {
            ModelState.AddModelError("Booking.BookingTime", "Usare slot da 30 minuti (es. 10:00, 10:30).");
            await LoadSelectListsAsync();
            await LoadSpecializationsForDoctorAsync(bookingToUpdate.DoctorId);
            return Page();
        }



        // Imposta RowVersion originale per controllo concorrenza
        _context.Entry(bookingToUpdate).Property(b => b.RowVersion)
            .OriginalValue = Booking.RowVersion!;


        // ➕ PATCH B: controllo sovrapposizione con altri appuntamenti dello stesso dottore
        var start = bookingToUpdate.DateTime;
        var end = start.AddMinutes(bookingToUpdate.DurationMinutes);

        bool overlaps = await _context.Bookings.AnyAsync(b =>
            b.DoctorId == bookingToUpdate.DoctorId &&
            b.BookingId != bookingToUpdate.BookingId &&   // escludi se stesso
            b.DateTime < end &&
            start < b.DateTime.AddMinutes(b.DurationMinutes));

        if (overlaps)
        {
            ModelState.AddModelError("Booking.DateTime", "Conflitto: il dottore ha già un appuntamento in questo intervallo.");
            await LoadSelectListsAsync();
            await LoadSpecializationsForDoctorAsync(bookingToUpdate.DoctorId);
            return Page();
        }

        try
        {
            await _context.SaveChangesAsync();
            TempData["Message"] = "Appuntamento modificato con successo.";
            return RedirectToPage("./Index");
        }
        catch (DbUpdateConcurrencyException)
        {
            // Gestione conflitto di concorrenza
            var dbValues = await _context.Entry(bookingToUpdate)
                .GetDatabaseValuesAsync();

            if (dbValues == null)
            {
                ModelState.AddModelError(string.Empty,
                    "L'appuntamento è stato eliminato da un altro utente.");
                return Page();
            }

            var databaseBooking = (Booking)dbValues.ToObject();

            ModelState.AddModelError(string.Empty,
                "L'appuntamento è stato modificato da un altro utente. " +
                "Ricarica i dati, verifica le differenze e riprova.");

            // Aggiorna con i valori correnti del database
            Booking = databaseBooking;
            Booking.RowVersion = databaseBooking.RowVersion;

            await LoadSelectListsAsync();
            await LoadSpecializationsForDoctorAsync(Booking.DoctorId);

            return Page();
        }
    }

    /// <summary>
    /// Handler AJAX per ottenere le specializzazioni filtrate per dottore
    /// </summary>
    public async Task<IActionResult> OnGetSpecializationsByDoctorAsync(int doctorId)
    {
        var specializations = await _context.DoctorSpecializations
            .Where(ds => ds.DoctorId == doctorId)
            .Include(ds => ds.Specialization)
            .Select(ds => new
            {
                id = ds.SpecializationId,
                name = ds.Specialization!.Name
            })
            .OrderBy(s => s.name)
            .ToListAsync();

        return new JsonResult(specializations);
    }

    /// <summary>
    /// Carica le SelectList per pazienti e dottori
    /// </summary>
    private async Task LoadSelectListsAsync()
    {
        // Pazienti
        var patients = await _context.Patients
            .OrderBy(p => p.Name)
            .Select(p => new { p.PatientId, p.Name })
            .ToListAsync();

        PatientsSelectList = new SelectList(patients, "PatientId", "Name", Booking.PatientId);

        // Dottori
        var doctors = await _context.Doctors
            .OrderBy(d => d.Name)
            .Select(d => new
            {
                d.DoctorId,
                DisplayName = "Dr. " + d.Name
            })
            .ToListAsync();

        DoctorsSelectList = new SelectList(doctors, "DoctorId", "DisplayName", Booking.DoctorId);
    }

    /// <summary>
    /// Carica le specializzazioni per un dottore specifico
    /// </summary>
    private async Task LoadSpecializationsForDoctorAsync(int doctorId)
    {
        var specializations = await _context.DoctorSpecializations
            .Where(ds => ds.DoctorId == doctorId)
            .Include(ds => ds.Specialization)
            .Select(ds => new
            {
                ds.SpecializationId,
                ds.Specialization!.Name
            })
            .OrderBy(s => s.Name)
            .ToListAsync();

        SpecializationsSelectList = new SelectList(
            specializations,
            "SpecializationId",
            "Name",
            Booking?.SpecializationId ?? 0
        );
    }
}