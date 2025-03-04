using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using DemoLab7.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DemoLab7.Controllers;

[Route("api/[controller]")]
[ApiController]
public class StudentController: ControllerBase
{
    private readonly UniversityDbContext _context;
    private readonly BlobContainerClient _blobContainerClient;
    public StudentController(UniversityDbContext context, BlobContainerClient blobContainerClient)
    {
        _context = context;
        _blobContainerClient = blobContainerClient;
    }
    [Authorize(Roles = "teacher")]
    [HttpGet("all")]
    public async Task<IActionResult> GetAllStudents()
    {
        var students = await _context.Students
            .Include(s => s.Classes)
            .ThenInclude(c => c.Course)
            .Include(s => s.Classes)
            .ThenInclude(c => c.Teacher)
            .ToListAsync();
        
        return Ok(students);
    }
    [Authorize(Roles = "teacher")]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetStudentById(int id)
    {
        var student = await _context.Students
            .Include(s => s.Classes)
            .ThenInclude(c => c.Course)
            .Include(s => s.Classes)
            .ThenInclude(c => c.Teacher)
            .FirstOrDefaultAsync(s => s.StudentId == id);
        if (student == null) return NotFound();
        return Ok(student);
    }
    [Authorize(Roles = "teacher")]
    [HttpPost("add")]
    public async Task<IActionResult> AddStudent([FromBody] Student student)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        _context.Students.Add(student);
        await _context.SaveChangesAsync();
        return Ok("Student created successfully");
    }
    [Authorize(Roles = "student")]
    [HttpPost("{studentId}/enroll/{classId}")]
    public async Task<IActionResult> EnrollStudentInClass(int studentId, int classId)
    {
        var student = await _context.Students.FindAsync(studentId);
        var classObj = await _context.Classes.FindAsync(classId);
        if (student == null || classObj == null) return NotFound();

        student.Classes.Append(classObj);
        await _context.SaveChangesAsync();
        return Ok("Student enrolled in class successfully!");
    }
    [Authorize(Roles = "student")]
    [HttpDelete("{studentId}/remove/{classId}")]
    public async Task<IActionResult> RemoveStudentFromClass(int studentId, int classId)
    {
        var student = await _context.Students
            .Include(s => s.Classes)
            .FirstOrDefaultAsync(s => s.StudentId == studentId);
        if (student == null) return NotFound();

        var classObj = student.Classes
            .FirstOrDefault(c => c.ClassId == classId);
        if (classObj == null) return NotFound();

        student.Classes.Remove(classObj);
        await _context.SaveChangesAsync();
        return Ok("Student removed from class successfully!");
    }
    [Authorize(Roles = "student")]
    [HttpPost("profile/{userId}")]
    public async Task<IActionResult> UploadProfilePicture(int userId, IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("File is empty.");

        var blobName = $"student{userId}/profile";
        var blobClient = _blobContainerClient.GetBlobClient(blobName);

        using (var stream = file.OpenReadStream())
        {
            await blobClient.UploadAsync(stream, new BlobHttpHeaders { ContentType = file.ContentType });
        }

        var url = blobClient.Uri.ToString();
        var student = await _context.Students.FindAsync(userId);
        student.ProfilePictureUrl = url;
        if (student == null)
            return NotFound("Student not found.");
        await _context.SaveChangesAsync();
        return Ok(new { profilePictureUrl = url });
    }
    [Authorize(Roles = "student")]
    [HttpGet("profile/{userId}")]
    public async Task<IActionResult> DownloadProfilePicture(int userId)
    {
        var student = await _context.Students.FindAsync(userId);
        if (student == null)
            return NotFound("Student not found.");

        var blobName = $"student{userId}/profile";
        var blobClient = _blobContainerClient.GetBlobClient(blobName);

        if (!await blobClient.ExistsAsync())
            return NotFound("Profile picture not found.");
        var downloadInfo = await blobClient.DownloadAsync();
        string fileName = $"student{userId}_profile.jpg"; 

        return File(downloadInfo.Value.Content, downloadInfo.Value.ContentType, fileName);
        
    }
    
}