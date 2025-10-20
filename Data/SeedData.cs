using Microsoft.AspNetCore.Identity;
using ClinicBooking.Web.Models;        // Per Doctor entity
using Microsoft.EntityFrameworkCore;   // Per FirstOrDefaultAsync, ToListAsync

namespace ClinicBooking.Web.Data;

public static class SeedData
{



    // Aggiungi questo metodo alla classe SeedData esistente:

    /// <summary>
    /// Crea i ruoli del sistema se non esistono già
    /// </summary>
    public static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
    {
        // Array dei ruoli necessari nel sistema
        string[] roleNames = { "Admin", "Doctor", "Secretary" };

        foreach (var roleName in roleNames)
        {
            // Controlla se il ruolo esiste già
            var roleExists = await roleManager.RoleExistsAsync(roleName);
            if (!roleExists)
            {
                // Crea il ruolo se non esiste
                var role = new IdentityRole(roleName);
                var result = await roleManager.CreateAsync(role);

                if (result.Succeeded)
                {
                    Console.WriteLine($"✅ Ruolo creato: {roleName}");
                }
                else
                {
                    Console.WriteLine($"❌ Errore creazione ruolo {roleName}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }
        }
    }

    /// <summary>
    /// Assegna ruoli agli utenti demo esistenti
    /// </summary>
    public static async Task AssignRolesToUsersAsync(UserManager<IdentityUser> userManager)
    {
        // Mappatura email -> ruolo per utenti demo
        var userRoleMapping = new Dictionary<string, string>
    {
        { "admin@clinic.com", "Admin" },
        { "dottore@clinic.com", "Doctor" },
        { "segretaria@clinic.com", "Secretary" }
    };

        foreach (var mapping in userRoleMapping)
        {
            var user = await userManager.FindByEmailAsync(mapping.Key);
            if (user != null)
            {
                // Controlla se l'utente ha già il ruolo assegnato
                var hasRole = await userManager.IsInRoleAsync(user, mapping.Value);
                if (!hasRole)
                {
                    // Assegna il ruolo all'utente
                    var result = await userManager.AddToRoleAsync(user, mapping.Value);
                    if (result.Succeeded)
                    {
                        Console.WriteLine($"✅ Ruolo {mapping.Value} assegnato a {mapping.Key}");
                    }
                }
            }
        }
    }


    /// <summary>
    /// Crea automaticamente record in tabella Doctors per utenti con ruolo Doctor
    /// </summary>
    public static async Task SyncDoctorsWithUsersAsync(UserManager<IdentityUser> userManager, AppDbContext context)
    {
        // Trova tutti gli utenti con ruolo Doctor
        var doctorUsers = await userManager.GetUsersInRoleAsync("Doctor");

        foreach (var doctorUser in doctorUsers)
        {
            // Controlla se esiste già un Doctor con questa email
            var existingDoctor = await context.Doctors
                .FirstOrDefaultAsync(d => d.Email == doctorUser.Email);

            if (existingDoctor == null && doctorUser.Email != null)
            {
                // Crea nuovo record Doctor collegato all'utente
                var newDoctor = new Doctor
                {
                    Name = doctorUser.Email.Split('@')[0], // Nome dal prefisso email (es. "dottore" da "dottore@clinic.com")
                    Email = doctorUser.Email,              // Email identica per collegamento
                    Phone = null                           // Può essere aggiornato successivamente
                };

                context.Doctors.Add(newDoctor);
                Console.WriteLine($"✅ Doctor creato per utente: {doctorUser.Email}");
            }
        }

        await context.SaveChangesAsync();
    }



    /// <summary>
    /// Crea utenti demo per il corso se non esistono già
    /// </summary>
    public static async Task SeedUsersAsync(UserManager<IdentityUser> userManager)
    {
        // Array di utenti demo da creare
        var demoUsers = new[]
        {
            new { Email = "admin@clinic.com", Password = "admin123" },      // Utente amministratore
            new { Email = "dottore@clinic.com", Password = "dott123" },     // Utente dottore  
            new { Email = "segretaria@clinic.com", Password = "segr123" }   // Utente segretaria
        };

        foreach (var userData in demoUsers)
        {
            // Controlla se l'utente esiste già
            var existingUser = await userManager.FindByEmailAsync(userData.Email);

            if (existingUser == null) // Se non esiste, crealo
            {
                var user = new IdentityUser
                {
                    UserName = userData.Email,    // Username = email
                    Email = userData.Email,       // Email dell'utente
                    EmailConfirmed = true         // Email già confermata (per semplificare corso)
                };

                // Crea l'utente con la password specificata
                var result = await userManager.CreateAsync(user, userData.Password);

                // Log del risultato per debug
                if (result.Succeeded)
                {
                    Console.WriteLine($"✅ Utente creato: {userData.Email}");
                }
                else
                {
                    Console.WriteLine($"❌ Errore creazione utente {userData.Email}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }
        }
    }

    /// <summary>
    /// Popola le tabelle Themes e RoleThemes con i dati iniziali
    /// Crea i 3 temi (Admin, Secretary, Doctor) e li associa ai rispettivi ruoli
    /// </summary>
    public static async Task SeedThemesAsync(AppDbContext context)
    {
        // Verifica se ci sono già temi nel database
        if (await context.Themes.AnyAsync())
        {
            // Temi già presenti, non fare nulla
            return;
        }

        // ============================================
        // CREAZIONE DEI 3 TEMI
        // ============================================

        var themes = new List<Theme>
    {
        // TEMA ADMIN - Layout minimale con Bootstrap standard
        new Theme
        {
            ThemeName = "Admin Standard",
            Description = "Layout minimale per amministratori con menu ridotto",
            LayoutFileName = "_LayoutAdmin",
            CssFileName = "admin-theme.css",
            IsActive = true
        },

        // TEMA SECRETARY - Layout completo con colori blu professionali
        new Theme
        {
            ThemeName = "Secretary Professional",
            Description = "Layout completo per segreteria con tutti i menu operativi",
            LayoutFileName = "_LayoutSecretary",
            CssFileName = "secretary-theme.css",
            IsActive = true
        },

        // TEMA DOCTOR - Layout semplificato con colori verdi rilassanti
        new Theme
        {
            ThemeName = "Doctor Relaxed",
            Description = "Layout semplificato per medici con vista appuntamenti",
            LayoutFileName = "_LayoutDoctor",
            CssFileName = "doctor-theme.css",
            IsActive = true
        }
    };

        // Aggiungi i temi al database
        await context.Themes.AddRangeAsync(themes);
        await context.SaveChangesAsync();

        // ============================================
        // ASSOCIAZIONE RUOLI → TEMI
        // ============================================

        // Recupera i temi appena creati (ora hanno gli ID assegnati dal database)
        var adminTheme = await context.Themes
            .FirstAsync(t => t.ThemeName == "Admin Standard");

        var secretaryTheme = await context.Themes
            .FirstAsync(t => t.ThemeName == "Secretary Professional");

        var doctorTheme = await context.Themes
            .FirstAsync(t => t.ThemeName == "Doctor Relaxed");

        var roleThemes = new List<RoleTheme>
    {
        // Admin → Admin Standard
        new RoleTheme
        {
            RoleName = "Admin",
            ThemeId = adminTheme.ThemeId,
            IsDefault = true
        },

        // Secretary → Secretary Professional
        new RoleTheme
        {
            RoleName = "Secretary",
            ThemeId = secretaryTheme.ThemeId,
            IsDefault = true
        },

        // Doctor → Doctor Relaxed
        new RoleTheme
        {
            RoleName = "Doctor",
            ThemeId = doctorTheme.ThemeId,
            IsDefault = true
        }
    };

        // Aggiungi le associazioni al database
        await context.RoleThemes.AddRangeAsync(roleThemes);
        await context.SaveChangesAsync();

        Console.WriteLine("✅ Temi e associazioni ruolo-tema creati con successo!");
    }


}