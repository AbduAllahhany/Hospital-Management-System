﻿using HospitalManagementSystem.Core.Entities;
using HospitalManagementSystem.DataAccess.Identity;
using HospitalManagementSystem.DataAccess.Persistence.Configrautions;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using System.Reflection.Emit;

namespace HospitalManagementSystem.DataAccess.Persistence
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions options):base(options) { }
       
        public DbSet<Doctor> Doctors { get; set; }
        public DbSet<Nurse> Nurses { get; set; }
        public DbSet<Patient> Patients { get; set; }

        public ApplicationDbContext()
        {
            
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.ApplyConfigurationsFromAssembly(typeof(DoctorConfiguration).Assembly);

            base.OnModelCreating(builder);
        }
    }


}
