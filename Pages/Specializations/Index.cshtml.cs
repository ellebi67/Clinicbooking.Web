using ClinicBooking.Web.Data;
using ClinicBooking.Web.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ClinicBooking.Web.Pages.Specializations;

// Pagina elenco specializzazioni
public class IndexModel : PageModel
{
    private readonly AppDbContext _db;
    public IndexModel(AppDbContext db) => _db = db;

    // Elenco da mostrare in tabella
    public IList<Specialization> List { get; private set; } = new List<Specialization>();

    // GET: carica tutte le specializzazioni ordinate per nome
    public async Task OnGetAsync()
    {
        List = await _db.Specializations
                        .OrderBy(s => s.Name)
                        .ToListAsync();
    }
}
