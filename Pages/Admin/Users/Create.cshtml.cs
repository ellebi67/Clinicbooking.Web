using System.ComponentModel.DataAnnotations;                 // Per attributi di validazione
using Microsoft.AspNetCore.Authorization;                    // Per [Authorize]
using Microsoft.AspNetCore.Identity;                         // Per UserManager/RoleManager
using Microsoft.AspNetCore.Mvc;                              // Per IActionResult
using Microsoft.AspNetCore.Mvc.RazorPages;                   // Per PageModel
using Microsoft.AspNetCore.Mvc.Rendering;                    // Per SelectListItem
using System.Linq;                                           // Per LINQ
using ClinicBooking.Web.Data;                           // Per AppDbContext
using ClinicBooking.Web.Models;                         // Per entità Doctor

using System.Threading.Tasks;                                // Per async/await

namespace ClinicBooking.Web.Pages.Admin.Users;

// Restringe l'accesso alla pagina solo agli amministratori
[Authorize(Roles = "Admin")]
public class CreateModel : PageModel
{
    // Gestore utenti di Identity (creazione, update, ruoli, ecc.)
    private readonly UserManager<IdentityUser> _userManager;

    // Gestore ruoli di Identity (lista ruoli disponibili, assegnazioni)
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly AppDbContext _db;                               // DbContext per inserire Doctor

    // Costruttore: il runtime inietterà i servizi UserManager e RoleManager e il DbContext
    public CreateModel(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, AppDbContext db)
    {
        _userManager = userManager;   // Salviamo la dipendenza per usarla nei metodi della pagina
        _roleManager = roleManager;   // Idem per i ruoli
        _db = db;                     // Salviamo il contesto EF Core per scrivere su tabella Doctors
    }

    // Classe di input legata al form (Model Binding)
    public class InputModel
    {
        // Nome e cognome del medico (obbligatorio se si seleziona il ruolo Doctor)
        [Display(Name = "Nome e Cognome *")]
        [Required(ErrorMessage = "Il nome è obbligatorio")]
        [StringLength(80, ErrorMessage = "Massimo 80 caratteri")]
        public string Name { get; set; } = string.Empty;

        // Telefono (facoltativo)
        [Display(Name = "Telefono")]
        [Phone(ErrorMessage = "Inserisci un numero di telefono valido")]
        public string? Phone { get; set; }

        // Campo Email obbligatorio con validazione standard .NET
        [Required(ErrorMessage = "L'email è obbligatoria")]
        [EmailAddress(ErrorMessage = "Inserisci un indirizzo email valido")]
        [Display(Name = "Email *")]
        public string Email { get; set; } = string.Empty;

        // Password con lunghezza minima tipica (verrà validata anche da Identity al CreateAsync)
        [Required(ErrorMessage = "La password è obbligatoria")]
        [StringLength(100, ErrorMessage = "La {0} deve essere lunga almeno {2} caratteri.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password *")]
        public string Password { get; set; } = string.Empty;

        // Conferma password: deve coincidere con Password
        [Required(ErrorMessage = "Conferma password obbligatoria")]
        [DataType(DataType.Password)]
        [Compare(nameof(Password), ErrorMessage = "Le password non coincidono")]
        [Display(Name = "Conferma password *")]
        public string ConfirmPassword { get; set; } = string.Empty;

        // Elenco dei ruoli selezionati dall'amministratore (multi-select)
        [Display(Name = "Ruoli")]
        public List<string> SelectedRoles { get; set; } = new();
    }

    // Questa proprietà ospita i dati del form
    [BindProperty]
    public InputModel Input { get; set; } = new();

    // SelectList di tutti i ruoli esistenti: usata dalla <select multiple>
    public List<SelectListItem> AllRoles { get; set; } = new();

    // GET: carica la pagina con la lista dei ruoli disponibili
    public async Task OnGetAsync()
    {
        // Preleva tutti i ruoli definiti nel database di Identity
        AllRoles = _roleManager.Roles
                               .Where(r => r.Name == "Doctor") // Limita ai soli dottori
                               .OrderBy(r => r.Name)
                               .Select(r => new SelectListItem { Value = r.Name!, Text = r.Name! })
                               .ToList();
        // Preseleziona Doctor per comodità
        if (!Input.SelectedRoles.Any()) Input.SelectedRoles = new List<string> { "Doctor" };
    }

    // POST: crea l'utente e assegna eventuali ruoli selezionati
    public async Task<IActionResult> OnPostAsync()
    {
        // Ricostruisce la lista ruoli per il caso in cui ci siano errori di validazione
        AllRoles = _roleManager.Roles
                               .Where(r => r.Name == "Doctor") // Limita ai soli dottori
                               .OrderBy(r => r.Name)
                               .Select(r => new SelectListItem { Value = r.Name!, Text = r.Name! })
                               .ToList();
        // Preseleziona Doctor per comodità
        if (!Input.SelectedRoles.Any()) Input.SelectedRoles = new List<string> { "Doctor" };

        // Se i dati del form non superano la validazione lato server, torna alla pagina con gli errori
        if (!ModelState.IsValid)
        {
            return Page();
        }

        // Crea una nuova entità utente di Identity con la sola Email (user name = email)
        var user = new IdentityUser
        {
            UserName = Input.Email, // In molti scenari si usa l'email come username
            Email = Input.Email,
            EmailConfirmed = true   // Facoltativo: in un corso possiamo evitare la conferma email
        };

        // Prova a creare l'utente nel database di Identity
        var result = await _userManager.CreateAsync(user, Input.Password);

        // Se la creazione fallisce, aggiunge gli errori al ModelState e ritorna al form
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                // Inserisce ogni errore nella ValidationSummary
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return Page(); // Mostra la pagina con gli errori
        }

        // Per sicurezza: consenti solo ruoli ammessi (solo "Doctor")
        var allowed = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "Doctor" };
        Input.SelectedRoles = (Input.SelectedRoles ?? new()).Where(r => allowed.Contains(r)).Distinct(StringComparer.OrdinalIgnoreCase).ToList();

        // Se sono stati scelti dei ruoli, li assegna all'utente appena creato
        if (Input.SelectedRoles.Any())
        {
            var addRoleResult = await _userManager.AddToRolesAsync(user, Input.SelectedRoles);

            // In caso di errore nell'assegnazione dei ruoli, mostriamo gli errori e rimaniamo nella pagina
            if (!addRoleResult.Succeeded)
            {
                foreach (var error in addRoleResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return Page();
            }
        }

        // Se l'utente è un DOCTOR, creiamo anche la riga in tabella Doctors
        if (Input.SelectedRoles.Contains("Doctor", StringComparer.OrdinalIgnoreCase))
        {
            var doctor = new Doctor
            {
                Name = Input.Name,         // Obbligatorio
                Email = Input.Email,       // Usata anche come username
                Phone = Input.Phone        // Facoltativo
            };

            _db.Doctors.Add(doctor);       // Traccia l'entità come "Added"
            await _db.SaveChangesAsync();  // Salva su database
        }

        // Tutto ok: ritorniamo alla lista utenti
        return RedirectToPage("Index");
    }
}
