﻿using Microsoft.AspNetCore.Mvc;

namespace HospitalManagementSystem.Controllers
{
    public class MedicalRecordController : Controller
    {
        private readonly IUnitOfWork unitOfWork;

        public MedicalRecordController(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }
        [HttpGet]
        [Authorize(Roles = SD.Admin)]
        public IActionResult Index()
        {
            return View();
        }
        [HttpGet]
        [Authorize(Roles = SD.Doctor)]
        public IActionResult Create()
        {
            return View();

        }
        [HttpPost]
        [Authorize(Roles = SD.Doctor)]
        public IActionResult Create() 
        { 
            unitOfWork.MME
        M
        
        }
    }
}
