using ClinicBooking.Web.Data;
using ClinicBooking.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ClinicBooking.Web.Pages.Doctors;

// Pagina creazione dottore con selezione di più specializzazioni
public class CreateModel : PageModel
{
    private readonly AppDbContext _db;
    public CreateModel(AppDbContext db) => _db = db;

    // ViewModel della form
    [BindProperty] public DoctorFormVM Input { get; set; } = new();

    // GET: prepara la form con l'elenco delle specializzazioni selezionabili
    public async Task OnGetAsync()
    {
        Input.Options = await _db.Specializations
                                .OrderBy(s => s.Name)
                                .Select(s => new ValueTuple<int, string>(s.SpecializationId, s.Name))
                                .ToListAsync();
    }

    // POST: crea il dottore e salva i collegamenti N–N
    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            // Ricarica le opzioni se la validazione fallisce
            Input.Options = await _db.Specializations
                                     .OrderBy(s => s.Name)
                                     .Select(s => new ValueTuple<int, string>(s.SpecializationId, s.Name))
                                     .ToListAsync();
            return Page();
        }

        // Crea la nuova entità dottore
        var doc = new Doctor
        {
            Name = Input.Name,
            Email = Input.Email,
            Phone = Input.Phone
        };

        // Aggiunge i collegamenti nella tabella ponte per ogni ID selezionato
        foreach (var specId in Input.SelectedSpecializationIds.Distinct())
        {
            doc.DoctorSpecializations.Add(new DoctorSpecialization
            {
                SpecializationId = specId
            });
        }

        _db.Doctors.Add(doc);
        await _db.SaveChangesAsync();

        return RedirectToPage("Index");
    }
}
