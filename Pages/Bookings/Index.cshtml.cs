// ==============================
// Pagina Index per Bookings (elenco appuntamenti)
// Commenti UI in italiano riga per riga
// ==============================
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClinicBooking.Web.Data;
using ClinicBooking.Web.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ClinicBooking.Web.Pages.Bookings
{
    // PageModel della pagina Index (elenco)
    public class IndexModel : PageModel
    {
        // DbContext iniettato tramite costruttore
        private readonly AppDbContext _db;
        public IndexModel(AppDbContext db) { _db = db; }

        // Elenco degli appuntamenti da mostrare in tabella
        public IList<Booking> Items { get; set; } = new List<Booking>();

        // GET: carica gli appuntamenti con le entità correlate (per mostrare nomi)
        public async Task OnGetAsync()
        {
            // Query con Include per caricare Patient/Doctor/Specialization
            Items = await _db.Set<Booking>()
                .Include(b => b.Patient)
                .Include(b => b.Doctor)
                .Include(b => b.Specialization)
                .OrderBy(b => b.DateTime)       // Ordino per data/ora
                .ToListAsync();                 // Eseguo la query
        }
    }
}