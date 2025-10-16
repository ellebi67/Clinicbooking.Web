using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClinicBooking.Web.Data;
using ClinicBooking.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace ClinicBooking.Web.Pages.Agenda
{
    public class AgendaModel : PageModel
    {
        private readonly AppDbContext _db; public AgendaModel(AppDbContext db) { _db = db; }
        [BindProperty(SupportsGet = true)] public DateOnly Date { get; set; } = DateOnly.FromDateTime(DateTime.Today);
        [BindProperty(SupportsGet = true)] public int? DoctorId { get; set; }
        [BindProperty(SupportsGet = true)] public int? SpecializationId { get; set; }
        public IList<Booking> Items { get; set; } = new List<Booking>();
        public SelectList Doctors { get; set; } = default!; public SelectList Specializations { get; set; } = default!;
        public async Task OnGetAsync()
        {
            var doctors = await _db.Doctors.OrderBy(d => d.Name).Select(d => new { d.DoctorId, DisplayName = "Dr. " + d.Name }).ToListAsync();
            Doctors = new SelectList(doctors, "DoctorId", "DisplayName", DoctorId);
            var specs = await _db.Specializations.OrderBy(s => s.Name).Select(s => new { s.SpecializationId, s.Name }).ToListAsync();
            Specializations = new SelectList(specs, "SpecializationId", "Name", SpecializationId);
            var query = _db.Bookings.Include(b => b.Patient).Include(b => b.Doctor).Include(b => b.Specialization).Where(b => b.BookingDate == Date);
            if (DoctorId.HasValue) query = query.Where(b => b.DoctorId == DoctorId.Value);
            if (SpecializationId.HasValue) query = query.Where(b => b.SpecializationId == SpecializationId.Value);
            Items = await query.OrderBy(b => b.BookingTime).ToListAsync();
        }
        public async Task<IActionResult> OnPostUpdateStatusAsync(int id, BookingStatus status, string rowVersionBase64, DateOnly date, int? doctorId, int? specializationId)
        {
            var entity = await _db.Bookings.FirstOrDefaultAsync(b => b.BookingId == id); if (entity == null) { TempData["Message"] = "Prenotazione non trovata."; return RedirectToPage("./Agenda", new { Date = date, DoctorId = doctorId, SpecializationId = specializationId }); }
            if (!string.IsNullOrEmpty(rowVersionBase64)) { _db.Entry(entity).Property(b => b.RowVersion).OriginalValue = Convert.FromBase64String(rowVersionBase64); }
            entity.Status = status; try { await _db.SaveChangesAsync(); TempData["Message"] = "Stato aggiornato."; } catch (DbUpdateConcurrencyException) { TempData["Message"] = "Conflitto di concorrenza: la prenotazione è stata modificata da un altro utente."; }
            return RedirectToPage("./Agenda", new { Date = date, DoctorId = doctorId, SpecializationId = specializationId });
        }
        public async Task<IActionResult> OnPostBulkConfirmAsync(DateOnly date, int? doctorId, int? specializationId) { var query = _db.Bookings.Where(b => b.BookingDate == date); if (doctorId.HasValue) query = query.Where(b => b.DoctorId == doctorId.Value); if (specializationId.HasValue) query = query.Where(b => b.SpecializationId == specializationId.Value); var list = await query.ToListAsync(); foreach (var b in list) b.Status = BookingStatus.Confirmed; await _db.SaveChangesAsync(); TempData["Message"] = $"Confermate {list.Count} prenotazioni."; return RedirectToPage("./Agenda", new { Date = date, DoctorId = doctorId, SpecializationId = specializationId }); }
    }
}
