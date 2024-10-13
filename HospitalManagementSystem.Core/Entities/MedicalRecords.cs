﻿
namespace HospitalManagementSystem.Core.Entities
{
    public class MedicalRecords
    {
        public int Id {  get; set; }
        public string? Description { set; get; }
       
        public int PatientID { set; get; }
        public int? DoctorID { set; get; }


        public Patient Patient { set; get; }
        public Doctor? Doctor  { set; get; }
    }
}
