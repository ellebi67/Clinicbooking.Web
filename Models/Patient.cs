using System.ComponentModel.DataAnnotations;

namespace ClinicBooking.Web.Models;

public class Patient
{
    public int PatientId { get; set; }

    [Required, StringLength(80)]
    public string Name { get; set; } = string.Empty;

    [StringLength(16)]
    public string? TaxCode { get; set; }

    [EmailAddress]
    public string? Email { get; set; }

    [Phone]
    public string? Phone { get; set; }

    // Token di concorrenza: EF/SQL Server lo usa per capire se il record
    // è cambiato tra il GET (quando abbiamo caricato i dati) e il POST (salvataggio).
    // è cambiato tra il GET (quando abbiamo caricato i dati) e il POST (salvataggio).
    // [Timestamp] mappa a una colonna ROWVERSION (byte[]) in SQL Server.
    [Timestamp]
    public byte[]? RowVersion { get; set; }

    public ICollection<PatientDoctor> PatientDoctors { get; set; } = new List<PatientDoctor>();
}