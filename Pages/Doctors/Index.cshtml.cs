using ClinicBooking.Web.Data;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ClinicBooking.Web.Pages.Doctors;

// Pagina elenco Dottori
public class IndexModel : PageModel
{
    private readonly AppDbContext _db;
    public IndexModel(AppDbContext db) => _db = db;

    // Riga della lista: dottore + specializzazioni in forma di stringa
    public record Row(int DoctorId, string Name, string? Email, string? Phone, string Specs);

    public IList<Row> List { get; private set; } = new List<Row>();

    // GET: carica i dottori con le loro specializzazioni
    public async Task OnGetAsync()
    {
        // Opzione 1: Carica tutto in memoria e poi processa
        var doctors = await _db.Doctors
            .Include(d => d.DoctorSpecializations)
                .ThenInclude(ds => ds.Specialization)
            .OrderBy(d => d.Name)
            .ToListAsync();

        List = doctors.Select(d => new Row(
            d.DoctorId,
            d.Name,
            d.Email,
            d.Phone,
            string.Join(", ",
                d.DoctorSpecializations
                 .Select(ds => ds.Specialization!.Name)
                 .OrderBy(n => n))
        )).ToList();
    }
}
