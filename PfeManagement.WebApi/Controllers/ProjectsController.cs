using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PfeManagement.WebApi.Data;
using PfeManagement.WebApi.Models;

namespace PfeManagement.WebApi.Controllers
{
    [ApiController]
    [Route("api/project")]
    public class ProjectsController : ControllerBase
    {
        private readonly AppDbContext _db;

        public ProjectsController(AppDbContext db)
        {
            _db = db;
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateProject([FromBody] CreateProjectDto dto)
        {
            try
            {
                var studentId = await GetCurrentStudentIdAsync();
                if (!studentId.HasValue)
                {
                    return Unauthorized(new { success = false, message = "Student not authenticated" });
                }

                var student = await _db.Students.FindAsync(studentId.Value);
                if (student == null) throw new Exception("Student not found");
                if (student.ProjectId != null) throw new Exception("Student is already part of a project");

                var project = new Project
                {
                    Title = dto.Title,
                    Description = dto.Description,
                    StartDate = NormalizeUtc(dto.StartDate),
                    EndDate = NormalizeUtc(dto.EndDate)
                };

                project.Contributors.Add(student);

                foreach (var contributorId in dto.Contributors)
                {
                    var contributor = await _db.Students.FindAsync(contributorId);
                    if (contributor != null && contributor.Id != studentId.Value && contributor.ProjectId == null)
                    {
                        project.Contributors.Add(contributor);
                    }
                }

                _db.Projects.Add(project);
                await _db.SaveChangesAsync();

                return StatusCode(201, new { success = true, message = "Project created successfully", data = MapToDto(project) });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [Authorize]
        [HttpGet]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetProject([FromRoute] Guid? id)
        {
            try
            {
                var projectId = id ?? await GetCurrentProjectIdAsync();
                if (!projectId.HasValue)
                {
                    return NotFound(new { success = false, message = "Project not found for current user" });
                }

                var project = await _db.Projects.Include(p => p.Contributors).FirstOrDefaultAsync(p => p.Id == projectId.Value);
                if (project == null) throw new Exception("Project not found");

                return Ok(new { success = true, message = "Project fetched successfully", data = MapToDto(project) });
            }
            catch (Exception ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
        }

        [Authorize]
        [HttpPatch]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProject([FromRoute] Guid? id, [FromBody] UpdateProjectDto dto)
        {
            try
            {
                var projectId = id ?? await GetCurrentProjectIdAsync();
                if (!projectId.HasValue)
                {
                    return NotFound(new { success = false, message = "Project not found for current user" });
                }

                var project = await _db.Projects.Include(p => p.Contributors).FirstOrDefaultAsync(p => p.Id == projectId.Value);
                if (project == null) throw new Exception("Project not found");

                if (dto.Title != null) project.Title = dto.Title;
                if (dto.Description != null) project.Description = dto.Description;
                if (dto.StartDate.HasValue) project.StartDate = NormalizeUtc(dto.StartDate.Value);
                if (dto.EndDate.HasValue) project.EndDate = NormalizeUtc(dto.EndDate.Value);

                await _db.SaveChangesAsync();

                return Ok(new { success = true, message = "Project updated successfully", data = MapToDto(project) });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [Authorize]
        [HttpDelete]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProject([FromRoute] Guid? id)
        {
            try
            {
                var projectId = id ?? await GetCurrentProjectIdAsync();
                if (!projectId.HasValue)
                {
                    return NotFound(new { success = false, message = "Project not found for current user" });
                }

                var project = await _db.Projects.FindAsync(projectId.Value);
                if (project == null) throw new Exception("Project not found");

                _db.Projects.Remove(project);
                await _db.SaveChangesAsync();

                return Ok(new { success = true, message = "Project deleted successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [Authorize]
        [HttpGet("students/without-project")]
        [HttpGet("available-students")]
        public async Task<IActionResult> GetAvailableStudents()
        {
            var students = await _db.Students.Where(s => s.ProjectId == null).ToListAsync();
            var result = students.Select(s => new ContributorDto
            {
                Id = s.Id,
                FullName = s.FullName,
                Email = s.Email
            });
            return Ok(new { success = true, message = "Students fetched successfully", data = result });
        }

        [Authorize]
        [HttpPatch("students/add-contributors")]
        [HttpPost("{id}/contributors")]
        public async Task<IActionResult> AddContributors([FromRoute] Guid? id, [FromBody] AddRemoveContributorsDto dto)
        {
            try
            {
                var requesterId = GetCurrentUserId();
                if (!requesterId.HasValue)
                {
                    return Unauthorized(new { success = false, message = "User not authenticated" });
                }

                var projectId = id ?? await GetCurrentProjectIdAsync();
                if (!projectId.HasValue)
                {
                    return NotFound(new { success = false, message = "Project not found for current user" });
                }

                var project = await _db.Projects.Include(p => p.Contributors).FirstOrDefaultAsync(p => p.Id == projectId.Value);
                if (project == null) throw new Exception("Project not found");

                foreach (var studentId in dto.StudentIds)
                {
                    var student = await _db.Students.FindAsync(studentId);
                    if (student != null && student.ProjectId == null)
                    {
                        student.ProjectId = projectId.Value;
                        project.Contributors.Add(student);
                    }
                }
                await _db.SaveChangesAsync();

                return Ok(new { success = true, message = "Contributors added successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [Authorize]
        [HttpPatch("students/remove-contributors")]
        [HttpDelete("{id}/contributors")]
        public async Task<IActionResult> RemoveContributors([FromRoute] Guid? id, [FromBody] AddRemoveContributorsDto dto)
        {
            try
            {
                var requesterId = GetCurrentUserId();
                if (!requesterId.HasValue)
                {
                    return Unauthorized(new { success = false, message = "User not authenticated" });
                }

                var projectId = id ?? await GetCurrentProjectIdAsync();
                if (!projectId.HasValue)
                {
                    return NotFound(new { success = false, message = "Project not found for current user" });
                }

                var project = await _db.Projects.Include(p => p.Contributors).FirstOrDefaultAsync(p => p.Id == projectId.Value);
                if (project == null) throw new Exception("Project not found");

                foreach (var studentId in dto.StudentIds)
                {
                    var student = await _db.Students.FindAsync(studentId);
                    if (student != null && student.ProjectId == projectId.Value)
                    {
                        student.ProjectId = null;
                        project.Contributors.Remove(student);
                    }
                }
                await _db.SaveChangesAsync();

                return Ok(new { success = true, message = "Contributors removed successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        private Guid? GetCurrentUserId()
        {
            var sub = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            return Guid.TryParse(sub, out var userId) ? userId : null;
        }

        private async Task<Guid?> GetCurrentStudentIdAsync()
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue) return null;
            var student = await _db.Students.FindAsync(userId.Value);
            return student?.Id;
        }

        private async Task<Guid?> GetCurrentProjectIdAsync()
        {
            var studentId = await GetCurrentStudentIdAsync();
            if (!studentId.HasValue) return null;
            var student = await _db.Students.FindAsync(studentId.Value);
            return student?.ProjectId;
        }

        private static ProjectResponseDto MapToDto(Project project)
        {
            return new ProjectResponseDto
            {
                ProjectId = project.Id,
                Title = project.Title,
                Description = project.Description,
                StartDate = project.StartDate,
                EndDate = project.EndDate,
                Contributors = project.Contributors.Select(c => new ContributorDto
                {
                    Id = c.Id,
                    FullName = c.FullName,
                    Email = c.Email
                }).ToList()
            };
        }

        private static DateTime NormalizeUtc(DateTime value)
        {
            return value.Kind switch
            {
                DateTimeKind.Utc => value,
                DateTimeKind.Local => value.ToUniversalTime(),
                _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
            };
        }
    }
}
