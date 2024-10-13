﻿using HospitalManagementSystem.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalManagementSystem.Application.Models.Patients
{
    public class PatientProfileModel
    {

        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public Gender Gender { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? NationalIdOrPassport { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Address { get; set; } = string.Empty;
        public bool IsEmailConfirmed { get; set; } = false;
        public bool IsTwoFactorEnabled { get; set; } = false;
        public string? BloodGroup { get; set; } = string.Empty;
        public string? Allergies { get; set; } = string.Empty;
        public string? ChronicDiseases { get; set; } = string.Empty;
        public string? CurrentMedications { get; set; } = string.Empty;
    }
}