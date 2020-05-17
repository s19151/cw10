using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cwiczenia10.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Cwiczenia10.Controllers
{
    [Route("api/students")]
    [ApiController]
    public class StudentsController : ControllerBase
    {
        private s19151Context _dbContext;

        public StudentsController(s19151Context dbContext) {
            _dbContext = dbContext;
        }

        [HttpGet]
        public IActionResult getStudentList()
        {
            var studentList = _dbContext.Student.ToList();

            if (studentList.Count > 0)
                return Ok(studentList);

            return NotFound();
        }

        [HttpPost("modify")]
        public IActionResult modifyStudent(Student student)
        {
            var s = _dbContext.Student.Find(student.IndexNumber);

            if (s != null)
            {
                //_dbContext.Entry(s).CurrentValues.SetValues(student);

                s.FirstName = student.FirstName;
                s.LastName = student.LastName;
                s.BirthDate = student.BirthDate;
                s.IdEnrollment = student.IdEnrollment;
                s.Password = student.Password;

                _dbContext.SaveChanges();

                return Ok(s);
            }

            return BadRequest("Podany student nie istnieje");
        }

        [HttpDelete("delete/{id}")]
        public IActionResult deleteStudent(string id)
        {
            var student = _dbContext.Student.Find(id);

            if (student != null)
            {
                _dbContext.Student.Remove(student);
                _dbContext.SaveChanges();

                return Ok(student);
            }

            return BadRequest("Brak podanego studenta");
        }
    }
}