// Namespace coerente con il progetto
namespace ClinicBooking.Web.Pages.Pazienti;

using System.ComponentModel.DataAnnotations;                 // Attributi di validazione
using ClinicBooking.Web.Data;                                // AppDbContext
using ClinicBooking.Web.Models;                              // Patient, Doctor, PatientDoctor
using Microsoft.AspNetCore.Mvc;                              // PageModel, IActionResult, HiddenInput
using Microsoft.AspNetCore.Mvc.RazorPages;                   // PageModel
using Microsoft.EntityFrameworkCore;                         // EF Core
using System.Linq;
using System.Threading.Tasks;

public class DetailsModel(AppDbContext db) : PageModel
{
    // ✅ Paziente mostrato
    public Patient? Patient { get; set; }

    // ✅ Dottori già assegnati
    public IList<Doctor> Assigned { get; set; } = new List<Doctor>();

    // ✅ Dottori assegnabili (filtrati/paginati)
    public IList<Doctor> Assignable { get; set; } = new List<Doctor>();

    // ✅ Paginazione
    public int PageSize { get; } = 10;
    public int TotalCount { get; set; }
    public int TotalPages => (int)System.Math.Ceiling((double)TotalCount / PageSize);

    // ✅ Input per le azioni POST
    public class LinkInput
    {
        [HiddenInput]
        public int PatientId { get; set; }

        [Required(ErrorMessage = "Seleziona un dottore valido.")]
        public int DoctorId { get; set; }

        public string? Query { get; set; }
        public int Page { get; set; } = 1;
    }

    [BindProperty] public LinkInput Input { get; set; } = new();

    // GET: carica paziente + assegnati/assegnabili (con filtro/paginazione)
    public async Task<IActionResult> OnGetAsync(int id, string? q, int page = 1)
    {
        // 🔎 Paziente
        Patient = await db.Patients
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.PatientId == id);

        if (Patient is null) return NotFound();

        // 📋 Id dottori assegnati
        var assignedIds = await db.PatientDoctors
            .Where(pd => pd.PatientId == id)
            .Select(pd => pd.DoctorId)
            .ToListAsync();

        // 🧾 Lista assegnati (ordinati per Name)
        Assigned = await db.Doctors
            .Where(d => assignedIds.Contains(d.DoctorId))
            .OrderBy(d => d.Name)
            .ToListAsync();

        // 🔍 Base query assegnabili (non assegnati)
        var queryBase = db.Doctors
            .AsNoTracking()
            .Where(d => !assignedIds.Contains(d.DoctorId));

        // 🔤 Filtro testo su Name
        if (!string.IsNullOrWhiteSpace(q))
        {
            var qNorm = q.Trim();
            queryBase = queryBase.Where(d => d.Name.Contains(qNorm));
        }

        // 🔢 Conteggio + paginazione
        TotalCount = await queryBase.CountAsync();
        Assignable = await queryBase
            .OrderBy(d => d.Name)
            .Skip((page - 1) * PageSize)
            .Take(PageSize)
            .ToListAsync();

        // ♻️ Stato UI
        Input = new LinkInput { PatientId = id, Query = q, Page = page };
        return Page();
    }

    // POST: aggiungi dottore → paziente
    public async Task<IActionResult> OnPostAddAsync()
    {
        if (!ModelState.IsValid)
            return await OnGetAsync(Input.PatientId, Input.Query, Input.Page);

        // Evita duplicati
        var exists = await db.PatientDoctors.AnyAsync(pd =>
            pd.PatientId == Input.PatientId && pd.DoctorId == Input.DoctorId);

        if (exists)
        {
            TempData["Messaggio"] = "Il dottore è già assegnato a questo paziente.";
            // ❗ Redirect alla stessa pagina (serve il nome della pagina)
            return RedirectToPage("./Details", new { id = Input.PatientId, q = Input.Query, page = Input.Page });
        }

        db.PatientDoctors.Add(new PatientDoctor
        {
            PatientId = Input.PatientId,
            DoctorId = Input.DoctorId
        });

        await db.SaveChangesAsync();

        TempData["Messaggio"] = "Dottore aggiunto con successo.";
        return RedirectToPage("./Details", new { id = Input.PatientId, q = Input.Query, page = Input.Page });
    }

    // POST: rimuovi dottore ← paziente
    public async Task<IActionResult> OnPostRemoveAsync()
    {
        var link = await db.PatientDoctors.FirstOrDefaultAsync(pd =>
            pd.PatientId == Input.PatientId && pd.DoctorId == Input.DoctorId);

        if (link is null)
        {
            TempData["Messaggio"] = "Associazione non trovata (forse già rimossa).";
            return RedirectToPage("./Details", new { id = Input.PatientId, q = Input.Query, page = Input.Page });
        }

        db.PatientDoctors.Remove(link);
        await db.SaveChangesAsync();

        TempData["Messaggio"] = "Dottore rimosso correttamente.";
        return RedirectToPage("./Details", new { id = Input.PatientId, q = Input.Query, page = Input.Page });
    }
}
