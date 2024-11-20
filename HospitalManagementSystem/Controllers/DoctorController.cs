

using HospitalManagementSystem.Application.Models.Doctors;

namespace HospitalManagementSystem.Controllers
{
    public class DoctorController : Controller
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly UserManager<ApplicationUser> userManager;

        public DoctorController(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager)
        {
            this.unitOfWork = unitOfWork;
            this.userManager = userManager;
        }

        [HttpGet]
        [Authorize(Roles = $"{SD.Admin}")]
        public async Task<IActionResult> Index()
        {
            var temp = await unitOfWork.Doctors.GetAllAsync();
            IEnumerable<DoctorIndexModel> model = temp.Select(p => new DoctorIndexModel { UserId = p.UserId, Name = p.Name });
            return View(model);
        }

        [HttpGet]
        [Authorize(Roles = SD.Doctor)]
        public async Task<IActionResult> Dashboard()
        {
            var user = await userManager.GetUserAsync(User);
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var doctor = await unitOfWork.Doctors.FindAsync(p => p.UserId == userId);

            var appointments = await unitOfWork.Appointments.FindAllAsync(a => a.DoctorId == doctor.Id && a.Status == Status.Confirmed, a => a.DateTime, includes: ["Patient"]);
            var appointment = appointments.FirstOrDefault();
            if (doctor == null || user == null)
                return View("Error");

            var model = new DoctorDashBoardModel
            {
                Age = doctor.Age,
                Gender = user.Gender,
                DoctorName = doctor.Name,
                AppointmentDateTime = appointment?.DateTime,
                AppointmentPatientName = appointment?.Patient?.Name
            };

            return View(model);
        }

        [HttpGet]
        [Authorize(Roles = SD.Admin)]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = SD.Admin)]
        public async Task<IActionResult> Create(CreateDoctorModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = new ApplicationUser
            {
                UserName = model.UserName,
                Email = model.Email,
                PhoneNumber = model.PhoneNumber,
                DateOfbirth = model.DateOfBirth,
                Address = model.Address,
                SSN = model.SSN,
                Gender = model.Gender,
            };
            await userManager.CreateAsync(user, "Doctor123");
            var year = (model.DateOfBirth.HasValue) ? model.DateOfBirth.Value.Year : default;
            await unitOfWork.Doctors.AddAsync(new()
            {
                Age = (year != 0) ? DateTime.Now.Year - year : 0,
                Name = model.FirstName + " " + model.LastName,
                User = user,

            });
            await userManager.AddToRoleAsync(user, SD.Doctor);
            await unitOfWork.CompleteAsync();
            return RedirectToAction("Index");
        }

        [HttpGet]
        [Authorize(Roles = $"{SD.Admin},{SD.Doctor}")]
        public async Task<IActionResult> Edit(string Id = null)
        {
            if (Id == null)
            {
                Id = User.FindFirstValue(ClaimTypes.NameIdentifier);
            }
            var user = await userManager.FindByIdAsync(Id);
            if (user == null) return View("Error");

            var doctor = await unitOfWork.Doctors.FindAsync(d => d.UserId == Id);
            if (doctor == null) return View("Error");

            var model = new EditDoctorModel
            {
                FirstName = doctor.Name.Split(' ')?.FirstOrDefault(),
                LastName = doctor.Name.Split(' ')?.LastOrDefault(),
                Address = user.Address,
                DateOfBirth = user.DateOfbirth,
                Email = user.Email,
                SSN = user.SSN,
                PhoneNumber = user.PhoneNumber,
                UserName = user.UserName
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = $"{SD.Admin},{SD.Doctor}")]
        public async Task<IActionResult> Edit(EditDoctorModel model)
        {
            if (!ModelState.IsValid) return View("Error");
            var year = (model.DateOfBirth.HasValue) ? model.DateOfBirth.Value.Year : default;
            var doctor = new Doctor()
            {
                Name = model.FirstName + " " + model.LastName,
                Age = (year != 0) ? DateTime.Now.Year - year : 0,

            };
            if (doctor == null) return View("Error");
            unitOfWork.Doctors.Update(doctor);
            await unitOfWork.CompleteAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = SD.Admin)]
        public async Task<IActionResult> Delete(string Id)
        {
            var user = await userManager.FindByIdAsync(Id);
            if (user == null) return View("Error");
            await userManager.DeleteAsync(user);

            var temp = await unitOfWork.Doctors.GetAllAsync();
            return RedirectToAction("Index");
        }

        [HttpGet]
        [Authorize(Roles = SD.Doctor)]
        public async Task<IActionResult> Profile()
        {

            var doctor = await unitOfWork.Doctors.FindAsync(p => p.UserId == User.FindFirstValue(ClaimTypes.NameIdentifier));
            var user = await userManager.GetUserAsync(User);
            if (user == null || doctor == null) return View("Error");
            return View(new DoctorProfileModel()
            {
                Name = doctor.Name,
                Address = user.Address,
                Email = user.Email,
                IsEmailConfirmed = user.EmailConfirmed,
                NationalIdOrPassport = user.SSN,
                PhoneNumber = user.PhoneNumber,
                Gender = user.Gender,
                IsTwoFactorEnabled = user.TwoFactorEnabled,
                DateOfBirth = user.DateOfbirth,
            });
        }

        [HttpGet]
        [Authorize(Roles = $"{SD.Admin},{SD.Doctor}")]
        public async Task<IActionResult> Appointments(string Id = null)
        {
            if (Id == null)
                Id = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var patient = await unitOfWork.Doctors.FindAsync(x => x.UserId == Id);
            if (patient == null) return View("Error");
            var temp = await unitOfWork.Appointments.FindAllAsync(a => a.DoctorId == patient.Id && a.Status == Status.Confirmed, ["Patient"]);
            if (temp == null) return View(new List<DoctorAppointmentsModel>());

            IEnumerable<DoctorAppointmentsModel> model = temp.Select(a => new DoctorAppointmentsModel
            {

                AppointmentId = a.Id,
                DateTime = a.DateTime,
                Reason = a.Reason,
                Status = a.Status,
                PatientName = a.Patient?.Name,
            });
            return View(model);

        }






        [HttpGet]
        [Authorize(Roles = $"{SD.Doctor},{SD.Admin}")]
        public async Task<IActionResult> MedicalRecords(string Id = null)
        {
            if (Id == null)
            {
                Id = User.FindFirstValue(ClaimTypes.NameIdentifier);
            }
            var user = await userManager.FindByIdAsync(Id);
            var doctor = await unitOfWork.Doctors.FindAsync(p => p.UserId == Id);

            var temp = await unitOfWork.MedicalRecords.FindAllAsync(r => r.DoctorId == doctor.Id, includes: ["Patient", "Appointment"]);
            var model = temp.Select(r => new DoctorMedicalRecordIndexModel
            {
                PatientName = r.Patient.Name,
                AppointmentDateTime = r.Appointment.DateTime,
                MedicalRecordId = r.Id,

            });
            return View(model);
        }
    }
}