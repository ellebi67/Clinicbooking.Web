// ==============================
// Modello Booking (in inglese)
// UI e commenti in italiano
// ==============================
using System;
using System.ComponentModel.DataAnnotations;

namespace ClinicBooking.Web.Models
{

    // Elenco degli stati gestiti a UI (badge), query e regole
    public enum BookingStatus
    {
        // Prenotazione pianificata ma non ancora confermata
        Scheduled = 0,

        // Confermata (es. telefonata/mail di conferma)
        Confirmed = 1,

        // Appuntamento eseguito
        Completed = 2,

        // Appuntamento annullato
        Cancelled = 3
    }
    // La classe rappresenta un appuntamento (Booking)
    public class Booking

    {
        // Id univoco dell'appuntamento
        public int BookingId { get; set; }

        // Riferimento al paziente (FK verso tabella Patients)
        [Required] // Obbligatorio: in UI daremo un messaggio chiaro
        public int PatientId { get; set; }

        // Navigazione opzionale verso Patient (caricata quando serve)
        public Patient? Patient { get; set; }

        // Riferimento al dottore (FK verso tabella Doctors)
        [Required]
        public int DoctorId { get; set; }

        // Navigazione verso Doctor
        public Doctor? Doctor { get; set; }

        // Riferimento alla specializzazione (FK verso tabella Specializations)
        [Required]
        public int SpecializationId { get; set; }

        // Navigazione verso Specialization
        public Specialization? Specialization { get; set; }

        // Data e ora dell'appuntamento (in locale)
        [Required]
        public DateTime DateTime { get; set; }

        // Note libere (facoltative) per l'operatore
        [MaxLength(500)]
        public string? Notes { get; set; }

        // RowVersion per la concorrenza ottimistica (timestamp)
        public byte[]? RowVersion { get; set; }

        public BookingStatus Status { get; set; } = BookingStatus.Scheduled;
        public int DurationMinutes { get; set; } = 30;
        public DateOnly BookingDate { get; set; }
        public TimeOnly BookingTime { get; set; }


    }
}
