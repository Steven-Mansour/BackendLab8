using DemoLab7.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DemoLab7.Controllers;
[Route("api/[controller]")]
[ApiController]
public class ClassController: ControllerBase
{
    private readonly UniversityDbContext _context;
    public ClassController(UniversityDbContext context)
    {
        _context = context;
    }
    [Authorize(Roles = "teacher")]
    [HttpPost("add")]
    public async Task<IActionResult> AddClass([FromBody] Class newClass)
    {
        var course = await _context.Courses.FindAsync(newClass.CourseId);
        var teacher = await _context.Teachers.FindAsync(newClass.TeacherId);

        if (course == null || teacher == null)
        {
            return BadRequest("Invalid course or teacher ID.");
        }


        _context.Classes.Add(newClass);
        await _context.SaveChangesAsync();

        return Ok("Class added successfully");
    }
    [Authorize]
    [HttpGet("all")]
    public async Task<IActionResult> GetAllClasses()
    {
        var classes = await _context.Classes
            .Include(c => c.Course)
            .Include(c => c.Teacher) 
            .ToListAsync();
        ;
        
        return Ok(classes);
    }
    [Authorize(Roles = "teacher")]
    [HttpPost("{classId}/assign-teacher/{teacherId}")]
    public async Task<IActionResult> AssignTeacherToClass(int classId, int teacherId)
    {
        var classObj = await _context.Classes.FindAsync(classId);
        var teacher = await _context.Teachers.FindAsync(teacherId);

        if (classObj == null || teacher == null) return NotFound();

        classObj.Teacher = teacher;
        await _context.SaveChangesAsync();
        return Ok("Teacher assigned successfully!");
    }
    
}