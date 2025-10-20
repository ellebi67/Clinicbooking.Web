
using ClinicBooking.Web.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.AspNetCore.HttpOverrides;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
//builder.Services.AddRazorPages();

// Trova AddRazorPages() e sostituisci con:
// ATTIVA Razor Pages e proteggi per default tutte le pagine (tranne Privacy)
builder.Services.AddRazorPages(options =>
{
    // Richiede autenticazione per TUTTE le pagine per default
    options.Conventions.AuthorizeFolder("/");

    // Eccezioni: pagine accessibili senza login
    options.Conventions.AllowAnonymousToPage("/Privacy");
});

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


// Configurazione sistema Identity per autenticazione
builder.Services.AddDefaultIdentity<IdentityUser>(options =>
{
    // Configurazione password semplificata per ambiente di sviluppo/corso
    options.Password.RequireDigit = false;           // Non richiede numeri nella password
    options.Password.RequireLowercase = false;       // Non richiede lettere minuscole
    options.Password.RequireUppercase = false;       // Non richiede lettere maiuscole
    options.Password.RequireNonAlphanumeric = false; // Non richiede caratteri speciali (@, !, etc.)
    options.Password.RequiredLength = 4;             // Password minima di 4 caratteri (per semplicità corso)
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<AppDbContext>(); // Collega Identity al nostro DbContext per salvare utenti nel database


// Configura redirect automatico per pagine protette
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;   // cookie trasmesso solo su HTTPS
    options.LoginPath = "/Identity/Account/Login";        // Pagina di login
    options.LogoutPath = "/Identity/Account/Logout";      // Pagina di logout  
    options.AccessDeniedPath = "/Identity/Account/AccessDenied"; // Accesso negato
});

var app = builder.Build();

// Seeding automatico utenti demo all'avvio applicazione
using (var scope = app.Services.CreateScope()) // Crea scope per dependency injection
{
    //var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>(); // Ottiene UserManager
    //await SeedData.SeedUsersAsync(userManager); // Esegue il seeding degli utenti demo

    var services = scope.ServiceProvider;

    // Ottieni i manager necessari per utenti e ruoli
    var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

    var context = services.GetRequiredService<AppDbContext>(); // Aggiungi DbContext

    // Esegui seeding nell'ordine corretto
    await SeedData.SeedRolesAsync(roleManager);           // Prima crea i ruoli
    await SeedData.SeedUsersAsync(userManager);           // Poi crea gli utenti
    await SeedData.AssignRolesToUsersAsync(userManager);  // Infine assegna ruoli agli utenti

    await SeedData.SyncDoctorsWithUsersAsync(userManager, context); // Nuova sincronizzazione
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

// Pipeline di autenticazione - ORDINE IMPORTANTE
app.UseAuthentication(); // Verifica chi è l'utente (identifica utente dal cookie/token)
//app.UseAuthorization();  // Verifica cosa può fare l'utente (controlla permessi)

app.UseRouting();
app.UseAuthorization();

//app.MapStaticAssets();
//app.MapRazorPages()
//   .WithStaticAssets();

app.UseStaticFiles(); // Versione .NET 8 per i file statici
app.MapRazorPages();  // Senza WithStaticAssets che non esiste in .NET 8

app.Run();
