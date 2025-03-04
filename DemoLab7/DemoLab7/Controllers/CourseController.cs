
using DemoLab7.Models;
using DemoLab7.RabbitMQ;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Lab4_CodeFirst.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CourseController : ControllerBase
{
    private readonly EnrollmentEventPublisher _enrollmentEventPublisher;
    private readonly CourseEventPublisher _courseEventPublisher;
    private readonly UniversityDbContext _context;

    public CourseController(UniversityDbContext context, EnrollmentEventPublisher enrollmentEventPublisher
    , CourseEventPublisher courseEventPublisher)
    {
        _context = context;
        _enrollmentEventPublisher = enrollmentEventPublisher;
        _courseEventPublisher = courseEventPublisher;
    }
    [Authorize(Roles = "teacher")]
    [HttpPost("add")]
    public async Task<IActionResult> AddCourse([FromBody] Course course)
    {
        _context.Courses.Add(course);
        await _context.SaveChangesAsync();
        return Ok("Course created successfully");
    }
    [Authorize]
    [HttpGet("all")]
    public async Task<IActionResult> GetAllCourses()
    {
        var courses = await _context.Courses
            .Include(c => c.Classes) 
            .ToListAsync();

        return Ok(courses);
    }
    [Authorize]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetCourseById(int id)
    {
        var course = await _context.Courses
            .Include(c => c.Classes) 
            .FirstOrDefaultAsync(c => c.CourseId == id);

        if (course == null)
        {
            return NotFound();
        }
        
        return Ok(course);
    }
    
    [HttpPost("register/{ClassId}-{StudentId}")]
    public async Task<IActionResult> RegisterClass(int ClassId, int StudentId)
    { 
        var studentExists = await _context.Students.AnyAsync(s => s.StudentId == StudentId);
        if (!studentExists)
        {
            return NotFound("Student not found.");
        }
        _enrollmentEventPublisher.PublishEnrollmentEvent(StudentId, ClassId);
        return Ok();
    }
    [HttpPost("Admin/createCourse")]
    public async Task<IActionResult> createCourse([FromBody] Course course)
    { 
        
        _courseEventPublisher.PublishCourseEvent(course);
        return Ok();
    }
    
}