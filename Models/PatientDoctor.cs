// Questa classe rappresenta la tabella ponte per la relazione molti-a-molti
// tra Paziente e Dottore. Serve per memorizzare le associazioni tra i due.
namespace ClinicBooking.Web.Models
{
    public class PatientDoctor
    {
        // Parte della chiave composta: Id del Paziente
        public int PatientId { get; set; }

        // Parte della chiave composta: Id del Dottore
        public int DoctorId { get; set; }

        // Navigazione verso il Paziente
        public Patient? Patient { get; set; }

        // Navigazione verso il Dottore
        public Doctor? Doctor { get; set; }

        // Campo rowversion per la gestione della concorrenza
        public byte[]? RowVersion { get; set; }
    }
}