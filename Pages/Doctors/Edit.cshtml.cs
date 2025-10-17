using ClinicBooking.Web.Data;
using ClinicBooking.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace ClinicBooking.Web.Pages.Doctors;

// Pagina modifica dottore con aggiornamento delle specializzazioni selezionate
public class EditModel : PageModel
{
    private readonly AppDbContext _db;

    private readonly UserManager<IdentityUser> _userManager;

    //public EditModel(AppDbContext db) => _db = db;

    public EditModel(AppDbContext db, UserManager<IdentityUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }



    [BindProperty] public DoctorFormVM Input { get; set; } = new();

    // GET: carica dottore + selezione corrente di specializzazioni
    public async Task<IActionResult> OnGetAsync(int id)
    {
        // Carica il dottore con le specializzazioni attuali
        var doc = await _db.Doctors
                           .Include(d => d.DoctorSpecializations)
                           .ThenInclude(ds => ds.Specialization)
                           .FirstOrDefaultAsync(d => d.DoctorId == id);

        if (doc is null) return NotFound();

        // Popola il ViewModel con i dati correnti
        Input.DoctorId = doc.DoctorId;
        Input.Name = doc.Name;
        Input.Email = doc.Email;
        Input.Phone = doc.Phone;

        // ID delle specializzazioni già associate
        Input.SelectedSpecializationIds = doc.DoctorSpecializations
                                             .Select(ds => ds.SpecializationId)
                                             .ToList();

        // Opzioni disponibili per la form
        Input.Options = await _db.Specializations
                                 .OrderBy(s => s.Name)
                                 .Select(s => new ValueTuple<int, string>(s.SpecializationId, s.Name))
                                 .ToListAsync();

        return Page();
    }

    // POST: salva dati anagrafici e aggiorna le associazioni N–N
    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            // Se fallisce la validazione, ricarica le opzioni e ripresenta la form
            Input.Options = await _db.Specializations
                                     .OrderBy(s => s.Name)
                                     .Select(s => new ValueTuple<int, string>(s.SpecializationId, s.Name))
                                     .ToListAsync();
            return Page();
        }

        // Carica l'entità con le associazioni correnti
        var doc = await _db.Doctors
                           .Include(d => d.DoctorSpecializations)
                           .FirstOrDefaultAsync(d => d.DoctorId == Input.DoctorId);

        if (doc is null) return NotFound();

        // Memorizza l'email precedente per aggiornare IdentityUser
        string oldEmail = doc.Email ?? "";


        // Aggiorna campi base
        doc.Name = Input.Name;
        doc.Email = Input.Email;
        doc.Phone = Input.Phone;

        // Calcola i set "prima" e "dopo" per la relazione N–N
        var current = doc.DoctorSpecializations.Select(x => x.SpecializationId).ToHashSet();
        var desired = Input.SelectedSpecializationIds.Distinct().ToHashSet();

        // Da rimuovere: presenti prima ma non più desiderati
        var toRemove = current.Except(desired).ToList();
        // Da aggiungere: desiderati ma non ancora presenti
        var toAdd = desired.Except(current).ToList();

        // Rimuovi collegamenti non più necessari
        doc.DoctorSpecializations = doc.DoctorSpecializations
                                       .Where(x => !toRemove.Contains(x.SpecializationId))
                                       .ToList();

        // Aggiungi nuovi collegamenti
        foreach (var specId in toAdd)
        {
            doc.DoctorSpecializations.Add(new DoctorSpecialization
            {
                DoctorId = doc.DoctorId,
                SpecializationId = specId
            });
        }

        // Se l'email è cambiata, aggiorna anche l'IdentityUser corrispondente
        if (!string.IsNullOrEmpty(oldEmail) && oldEmail != Input.Email)
        {
            await UpdateIdentityUserEmailAsync(oldEmail, Input.Email ?? "");
        }

        await _db.SaveChangesAsync();
        return RedirectToPage("Index");
    }


    /// <summary>
    /// Aggiorna email dell'IdentityUser quando cambia email del Doctor
    /// </summary>
    private async Task UpdateIdentityUserEmailAsync(string oldEmail, string newEmail)
    {
        // Trova l'utente con la vecchia email
        var user = await _userManager.FindByEmailAsync(oldEmail);

        if (user != null && !string.IsNullOrEmpty(newEmail))
        {
            // Aggiorna email e username (che di solito coincide con email)
            user.Email = newEmail;
            user.UserName = newEmail;
            user.NormalizedEmail = newEmail.ToUpper();
            user.NormalizedUserName = newEmail.ToUpper();

            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                Console.WriteLine($"✅ Email IdentityUser aggiornata: {oldEmail} → {newEmail}");
            }
            else
            {
                Console.WriteLine($"❌ Errore aggiornamento IdentityUser: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }
        }
    }


}
