﻿using Microsoft.AspNetCore.Mvc;

namespace HospitalManagementSystem.Controllers
{
    public class MedicalRecordController : Controller
    {
        [HttpGet]
        [Authorize(Roles = SD.Admin)]
        public IActionResult Index()
        {
            return View();
        }
    }
}