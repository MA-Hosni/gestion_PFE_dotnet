using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PfeManagement.WebApi.Data;
using PfeManagement.WebApi.Interfaces;

namespace PfeManagement.WebApi.Controllers
{
    [ApiController]
    [Route("api")]
    public class UsersController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly IPasswordHasher _hasher;

        public UsersController(AppDbContext db, IPasswordHasher hasher)
        {
            _db = db;
            _hasher = hasher;
        }

        [HttpGet("supervisors/company")]
        public async Task<IActionResult> GetCompanySupervisors()
        {
            var supervisors = await _db.CompanySupervisors.ToListAsync();
            var data = supervisors.Select(s => new
            {
                id = s.Id, fullName = s.FullName, email = s.Email,
                companyName = s.CompanyName, badgeIMG = s.BadgeIMG
            });
            return Ok(new { success = true, message = "Company supervisors fetched successfully", data });
        }

        [HttpGet("supervisors/university")]
        public async Task<IActionResult> GetUniversitySupervisors()
        {
            var supervisors = await _db.UniversitySupervisors.ToListAsync();
            var data = supervisors.Select(s => new
            {
                id = s.Id, fullName = s.FullName, email = s.Email, badgeIMG = s.BadgeIMG
            });
            return Ok(new { success = true, message = "University supervisors fetched successfully", data });
        }

        [Authorize]
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            var userId = TryGetCurrentUserId();
            if (!userId.HasValue) return Unauthorized(new { success = false, message = "User not authenticated" });

            var user = await _db.Users.FindAsync(userId.Value);
            if (user == null) return NotFound(new { success = false, message = "User not found" });

            return Ok(new
            {
                success = true, message = "Profile fetched successfully",
                data = new
                {
                    id = user.Id, fullName = user.FullName, email = user.Email,
                    phoneNumber = user.PhoneNumber, role = user.Role.ToString(),
                    isVerified = user.IsVerified, isActive = user.IsActive
                }
            });
        }

        [Authorize]
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            var userId = TryGetCurrentUserId();
            if (!userId.HasValue) return Unauthorized(new { success = false, message = "User not authenticated" });

            var user = await _db.Users.FindAsync(userId.Value);
            if (user == null) return NotFound(new { success = false, message = "User not found" });

            if (!_hasher.VerifyPassword(request.CurrentPassword, user.PasswordHash))
                return BadRequest(new { success = false, message = "Current password is incorrect" });

            user.PasswordHash = _hasher.HashPassword(request.NewPassword);
            await _db.SaveChangesAsync();

            return Ok(new { success = true, message = "Password updated successfully" });
        }

        private Guid? TryGetCurrentUserId()
        {
            var sub = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            return Guid.TryParse(sub, out var id) ? id : null;
        }

        public class ChangePasswordRequest
        {
            public string CurrentPassword { get; set; } = string.Empty;
            public string NewPassword { get; set; } = string.Empty;
        }
    }
}
