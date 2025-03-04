using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using DemoLab7.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DemoLab7.Controllers;
[Route("api/[controller]")]
[ApiController]
public class TeacherController: ControllerBase
{
    private readonly UniversityDbContext _context;
    private readonly BlobContainerClient _blobContainerClient;

    public TeacherController(UniversityDbContext context, BlobContainerClient blobContainerClient)
    {
        _blobContainerClient = blobContainerClient;
        _context = context;
    }
    [Authorize(Roles = "teacher")]
    [HttpPost("add")]
    public async Task<IActionResult> AddTeacher([FromBody] Teacher teacher)
    {
        _context.Teachers.Add(teacher);
        await _context.SaveChangesAsync();
        return Ok("Created teacher successfully");
    }
    [Authorize(Roles = "teacher")]
    [HttpGet("all")]
    public async Task<IActionResult> GetAllTeachers()
    {
        var teachers = await _context.Teachers
            .Include(t => t.Classes) 
            .ToListAsync();
        return Ok(teachers);
    }
    [Authorize(Roles = "teacher")]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetTeacherById(int id)
    {
        var teacher = await _context.Teachers
            .Include(t => t.Classes) 
            .FirstOrDefaultAsync(t => t.TeacherId == id);
        if (teacher == null) return NotFound();
        return Ok(teacher);
    }
    [Authorize(Roles = "teacher")]
    [HttpPost("profile/{userId}")]
    public async Task<IActionResult> UploadProfilePicture(int userId, IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("File is empty.");

        var blobName = $"teacher{userId}/profile";
        var blobClient = _blobContainerClient.GetBlobClient(blobName);

        using (var stream = file.OpenReadStream())
        {
            await blobClient.UploadAsync(stream, new BlobHttpHeaders { ContentType = file.ContentType });
        }

        var url = blobClient.Uri.ToString();
        var teacher = await _context.Teachers.FindAsync(userId);
        if (teacher == null)
            return NotFound("Teacher not found.");
        teacher.ProfilePictureUrl = url;
        await _context.SaveChangesAsync();
        return Ok(new { profilePictureUrl = url });
    }
    
    [Authorize(Roles = "teacher")]
    [HttpGet("profile/{userId}")]
    public async Task<IActionResult> DownloadProfilePicture(int userId)
    {
        var teacher = await _context.Teachers.FindAsync(userId);
        if (teacher == null)
            return NotFound("Teacher not found.");

        var blobName = $"teacher{userId}/profile";
        var blobClient = _blobContainerClient.GetBlobClient(blobName);

        if (!await blobClient.ExistsAsync())
            return NotFound("Profile picture not found.");
        var downloadInfo = await blobClient.DownloadAsync();
        string fileName = $"teacher{userId}_profile.jpg"; 

        return File(downloadInfo.Value.Content, downloadInfo.Value.ContentType, fileName);
        
    }
}