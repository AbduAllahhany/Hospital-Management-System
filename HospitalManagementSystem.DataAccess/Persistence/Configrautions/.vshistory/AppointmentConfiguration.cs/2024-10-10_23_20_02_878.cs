using HospitalManagementSystem.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;


namespace HospitalManagementSystem.DataAccess.Persistence.Configrautions
{
    internal class AppointmentConfiguration : IEntityTypeConfiguration<Appointment>
    {
        public void Configure(EntityTypeBuilder<Appointment> builder)
        {
            builder.HasKey(a => a.Id);
            builder.Property(a => a.Id).ValueGeneratedOnAdd();

            builder.Property(a => a.PatientID).IsRequired();
            builder.Property(a => a.DoctorID).IsRequired();
            builder.Property(a => a.AppointmentDateTime).IsRequired();
            builder.Property(a => a.Reason).HasMaxLength(500);

            builder.HasOne(a => a.Patient)
                   .WithMany(p => p.Appointments)
                   .HasForeignKey(a => a.PatientID).OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(a => a.Doctor)
                   .WithMany(d => d.Appointments)
                   .HasForeignKey(a => a.DoctorID).OnDelete(DeleteBehavior.NoAction);
        }
    }
}