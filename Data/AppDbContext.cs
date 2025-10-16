using ClinicBooking.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace ClinicBooking.Web.Data;



public class AppDbContext : DbContext
{
  public AppDbContext(DbContextOptions<AppDbContext> opts) : base(opts) { }

  public DbSet<Patient> Patients => Set<Patient>();
  // Nuove tabelle per Dottori e Specializzazioni
  public DbSet<Doctor> Doctors => Set<Doctor>();
  public DbSet<Specialization> Specializations => Set<Specialization>();

  // Tabella ponte esplicita per la relazione N–N
  public DbSet<DoctorSpecialization> DoctorSpecializations => Set<DoctorSpecialization>();
  public DbSet<PatientDoctor> PatientDoctors => Set<PatientDoctor>(); // Pazienti↔Dottori

  // 1) Aggiungere questa proprietà tra i DbSet esistenti
  public DbSet<Booking> Bookings => Set<Booking>();


  protected override void OnModelCreating(ModelBuilder mb)
  {
    base.OnModelCreating(mb);

    // =======================================
    // CONFIGURAZIONE ENTITÀ: Booking
    // =======================================
    mb.Entity<Booking>(eb =>
    {
      eb.ToTable("Bookings");
      // Nome tabella sul database SQL

      eb.HasKey(b => b.BookingId);
      // Chiave primaria

      // =====================================================
      // GESTIONE CONCORRENZA (versione ottimistica)
      // =====================================================
      eb.Property(b => b.RowVersion).IsRowVersion();

      // =====================================================
      // NUOVE PROPRIETÀ (Status, Durata, Data/Ora separate)
      // =====================================================

      // Enum BookingStatus salvato come stringa nel DB
      eb.Property(b => b.Status)
    .HasConversion<string>()
    .HasMaxLength(20)
    .HasDefaultValue(BookingStatus.Scheduled); // ✅ enum CLR; il converter lo salva come 'Scheduled'

      // Durata in minuti, default 30
      eb.Property(b => b.DurationMinutes)
    .HasDefaultValue(30);

      // Conversione DateOnly → SQL date
      eb.Property(b => b.BookingDate)
    .HasConversion(
        v => v.ToDateTime(TimeOnly.MinValue),    // scrittura nel DB
        v => DateOnly.FromDateTime(v))          // lettura dal DB
    .HasColumnType("date");

      // Conversione TimeOnly → SQL time
      eb.Property(b => b.BookingTime)
    .HasConversion(
        v => v.ToTimeSpan(),                    // scrittura nel DB
        v => TimeOnly.FromTimeSpan(v))          // lettura dal DB
    .HasColumnType("time");

      // =====================================================
      // RELAZIONI (FK esplicite)
      // =====================================================

      eb.HasOne(b => b.Patient)
    .WithMany()
    .HasForeignKey(b => b.PatientId)
    .OnDelete(DeleteBehavior.Restrict)
    .HasConstraintName("FK_Bookings_Patients_PatientId");

      eb.HasOne(b => b.Doctor)
    .WithMany()
    .HasForeignKey(b => b.DoctorId)
    .OnDelete(DeleteBehavior.Restrict)
    .HasConstraintName("FK_Bookings_Doctors_DoctorId");

      eb.HasOne(b => b.Specialization)
    .WithMany()
    .HasForeignKey(b => b.SpecializationId)
    .OnDelete(DeleteBehavior.Restrict)
    .HasConstraintName("FK_Bookings_Specializations_SpecializationId");

      // =====================================================
      // INDICI per performance su ricerche e vista Agenda
      // =====================================================

      eb.HasIndex(b => b.DateTime)
    .HasDatabaseName("IX_Bookings_DateTime");

      eb.HasIndex(b => new { b.DoctorId, b.DateTime })
    .HasDatabaseName("IX_Bookings_Doctor_DateTime");

      // Indici per la vista calendario (nuovi)
      eb.HasIndex(b => b.BookingDate)
    .HasDatabaseName("IX_Bookings_Date");

      eb.HasIndex(b => new { b.DoctorId, b.BookingDate, b.BookingTime })
    .HasDatabaseName("IX_Bookings_Doctor_Date_Time");
    });





    // Configurazione chiave composta della tabella ponte (DoctorId, SpecializationId)
    mb.Entity<DoctorSpecialization>()
      .HasKey(ds => new { ds.DoctorId, ds.SpecializationId });

    // Relazione Doctor 1–N con DoctorSpecialization
    mb.Entity<DoctorSpecialization>()
      .HasOne(ds => ds.Doctor)
      .WithMany(d => d.DoctorSpecializations)
      .HasForeignKey(ds => ds.DoctorId)
      .OnDelete(DeleteBehavior.Cascade);

    // Relazione Specialization 1–N con DoctorSpecialization
    mb.Entity<DoctorSpecialization>()
      .HasOne(ds => ds.Specialization)
      .WithMany(s => s.DoctorSpecializations)
      .HasForeignKey(ds => ds.SpecializationId)
      .OnDelete(DeleteBehavior.Cascade);

    // (Esempio) Indice unico opzionale per nome specializzazione (se vuoi evitare duplicati)
    mb.Entity<Specialization>()
      .HasIndex(s => s.Name)
      .IsUnique(false); // metti true se vuoi vietare nomi duplicati


    // Patient↔Doctor (tabella ponte, CHIAVE COMPOSTA, senza colonne spurie)
    mb.Entity<PatientDoctor>(eb =>
    {
      eb.ToTable("PatientDoctors");                         // Nome tabella nel DB
      eb.HasKey(pd => new { pd.PatientId, pd.DoctorId });   // Chiave composta

      // Colonna RowVersion per la concorrenza
      eb.Property(pd => pd.RowVersion).IsRowVersion();

      // Relazione verso Patient → usa la collection Patient.PatientDoctors
      eb.HasOne(pd => pd.Patient)
    .WithMany(p => p.PatientDoctors)
    .HasForeignKey(pd => pd.PatientId)
    .OnDelete(DeleteBehavior.Cascade)
    .HasConstraintName("FK_PatientDoctors_Patients_PatientId");

      // Relazione verso Doctor → usa la collection Doctor.PatientDoctors
      eb.HasOne(pd => pd.Doctor)
    .WithMany(d => d.PatientDoctors)
    .HasForeignKey(pd => pd.DoctorId)
    .OnDelete(DeleteBehavior.Cascade)
    .HasConstraintName("FK_PatientDoctors_Doctors_DoctorId");

      // Indici (facoltativi ma consigliati)
      eb.HasIndex(pd => pd.DoctorId).HasDatabaseName("IX_PatientDoctors_DoctorId");
      eb.HasIndex(pd => pd.PatientId).HasDatabaseName("IX_PatientDoctors_PatientId");

      // Ignora esplicitamente eventuali shadow properties residue
      eb.Ignore("DoctorId1");
      eb.Ignore("PatientId1");
    });

  }

  // ➕ Protezione centrale: sincronizza sempre i campi Agenda prima del salvataggio
  public override int SaveChanges()
  {
    SyncBookingFields();
    return base.SaveChanges();
  }

  public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
  {
    SyncBookingFields();
    return await base.SaveChangesAsync(cancellationToken);
  }

  private void SyncBookingFields()
  {
    foreach (var entry in ChangeTracker.Entries<Booking>())
    {
      if (entry.State == EntityState.Added || entry.State == EntityState.Modified)
      {
        var dt = entry.Entity.DateTime;
        entry.Entity.BookingDate = DateOnly.FromDateTime(dt); // solo data
        entry.Entity.BookingTime = TimeOnly.FromDateTime(dt); // solo ora
      }
    }
  }


}