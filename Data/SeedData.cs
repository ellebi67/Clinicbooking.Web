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
}