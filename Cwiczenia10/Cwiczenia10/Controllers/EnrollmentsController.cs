using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cwiczenia10.DTO.Requests;
using Cwiczenia10.DTO.Responses;
using Cwiczenia10.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Cwiczenia10.Controllers
{
    [Route("api/enrollments")]
    [ApiController]
    public class EnrollmentsController : ControllerBase
    {
        private s19151Context _dbContext;

        public EnrollmentsController(s19151Context dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpPost]
        public IActionResult EnrollStudent(EnrollStudentRequest request)
        {
            var student = _dbContext.Student.Find(request.IndexNumber);

            if (student != null)
                return BadRequest("Podano zły nr indeksu");

            var studies = _dbContext.Studies.Where(s => s.Name.Equals(request.Studies));

            if (studies.Count() == 0)
                return NotFound("Podane studia nie istnieją");

            var enrollments = _dbContext.Enrollment.Where(en => en.IdStudy == studies.First().IdStudy && en.Semester == 1)
                .OrderByDescending(en => en.StartDate);
            Enrollment enrollment;
            if (enrollments.Count() == 0)
            {
                enrollment = new Enrollment();

                enrollment.IdEnrollment = Convert.ToInt32(_dbContext.Enrollment.Max(en => en.IdEnrollment)) + 1;
                enrollment.Semester = 1;
                enrollment.IdStudy = studies.First().IdStudy;
                enrollment.StartDate = DateTime.Now.Date;

                _dbContext.Enrollment.Add(enrollment);
            }
            else
            {
                enrollment = enrollments.First();
            }

            //dodanie studenta
            student = new Student();

            student.IndexNumber = request.IndexNumber;
            student.FirstName = request.FirstName;
            student.LastName = request.LastName;
            student.BirthDate = DateTime.Parse(request.BirthDate);
            student.IdEnrollment = enrollment.IdEnrollment;

            var response = new EnrollmentResponse();

            response.IdEnrollment = enrollment.IdEnrollment;
            response.Semester = enrollment.Semester;
            response.IdStudy = enrollment.IdStudy;
            response.StartDate = enrollment.StartDate;

            _dbContext.Student.Add(student);

            _dbContext.SaveChanges();

            return Created("", response);
        }

        [HttpPost("promotions")]
        public IActionResult PromoteStudent(PromoteStudentsRequest request)
        {
            var studies = _dbContext.Studies.Where(st => st.Name.Equals(request.Studies));

            if (studies.Count() == 0)
                return NotFound("Brak studiów o podanym kierunku");

            var study = studies.First();

            var enrollments = _dbContext.Enrollment.Where(en => en.IdStudy == study.IdStudy && en.Semester == request.Semester);

            if (enrollments.Count() == 0)
                return NotFound("Brak wpisów na 1 semestr dla podanego kierunku");

            var id = enrollments.First().IdEnrollment;

            _dbContext.Database.ExecuteSqlRaw($"promotestudents {request.Studies} {request.Semester}");

            _dbContext.SaveChanges();

            var enrollment = _dbContext.Enrollment.Find(id);

            var response = new EnrollmentResponse();

            response.IdEnrollment = enrollment.IdEnrollment;
            response.Semester = enrollment.Semester;
            response.IdStudy = enrollment.IdStudy;
            response.StartDate = enrollment.StartDate;

            return Created("", response);
        }
    }
}