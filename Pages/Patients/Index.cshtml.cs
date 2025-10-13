using ClinicBooking.Web.Data;
using ClinicBooking.Web.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ClinicBooking.Web.Pages.Patients;

public class IndexModel : PageModel
{
    private readonly AppDbContext _db;
    public IndexModel(AppDbContext db) => _db = db;

    public IList<Patient> List { get; private set; } = new List<Patient>();

    public async Task OnGetAsync()
    {
        List = await _db.Patients.OrderBy(p => p.Name).ToListAsync();
    }
}