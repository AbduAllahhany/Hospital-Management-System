﻿

namespace HospitalManagementSystem.Core.Entities
{
    public class Doctor :BaseEntity
    {
        public int Id {  get; set; }
        public string Name { get; set; }

        public List<Patient>? Patients { get; set; }
        public List<Appointment>? Appointments {  get; set; }
        public List<MedicalRecords>? MedicalRecords { get; set; }
    }
}
