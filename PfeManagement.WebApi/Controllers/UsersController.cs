using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PfeManagement.Application.Interfaces;
using PfeManagement.Domain.Interfaces;

namespace PfeManagement.WebApi.Controllers
{
    [ApiController]
    [Route("api")]
    public class UsersController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPasswordHasher _passwordHasher;

        public UsersController(IUnitOfWork unitOfWork, IPasswordHasher passwordHasher)
        {
            _unitOfWork = unitOfWork;
            _passwordHasher = passwordHasher;
        }

        [HttpGet("supervisors/company")]
        public async Task<IActionResult> GetCompanySupervisors()
        {
            var supervisors = await _unitOfWork.CompanySupervisors.GetAllAsync();
            var data = supervisors.Select(s => new
            {
                id = s.Id,
                fullName = s.FullName,
                email = s.Email,
                companyName = s.CompanyName,
                badgeIMG = s.BadgeIMG
            });

            return Ok(new
            {
                success = true,
                message = "Company supervisors fetched successfully",
                data
            });
        }

        [HttpGet("supervisors/university")]
        public async Task<IActionResult> GetUniversitySupervisors()
        {
            var supervisors = await _unitOfWork.UniversitySupervisors.GetAllAsync();
            var data = supervisors.Select(s => new
            {
                id = s.Id,
                fullName = s.FullName,
                email = s.Email,
                badgeIMG = s.BadgeIMG
            });

            return Ok(new
            {
                success = true,
                message = "University supervisors fetched successfully",
                data
            });
        }

        [Authorize]
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            var userId = TryGetCurrentUserId();
            if (!userId.HasValue)
            {
                return Unauthorized(new { success = false, message = "User not authenticated" });
            }

            var user = await _unitOfWork.Users.GetByIdAsync(userId.Value);
            if (user == null)
            {
                return NotFound(new { success = false, message = "User not found" });
            }

            return Ok(new
            {
                success = true,
                message = "Profile fetched successfully",
                data = new
                {
                    id = user.Id,
                    fullName = user.FullName,
                    email = user.Email,
                    phoneNumber = user.PhoneNumber,
                    role = user.Role.ToString(),
                    isVerified = user.IsVerified,
                    isActive = user.IsActive
                }
            });
        }

        [Authorize]
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            var userId = TryGetCurrentUserId();
            if (!userId.HasValue)
            {
                return Unauthorized(new { success = false, message = "User not authenticated" });
            }

            var user = await _unitOfWork.Users.GetByIdAsync(userId.Value);
            if (user == null)
            {
                return NotFound(new { success = false, message = "User not found" });
            }

            if (!_passwordHasher.VerifyPassword(request.CurrentPassword, user.PasswordHash))
            {
                return BadRequest(new { success = false, message = "Current password is incorrect" });
            }

            user.PasswordHash = _passwordHasher.HashPassword(request.NewPassword);
            await _unitOfWork.Users.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync();

            return Ok(new { success = true, message = "Password updated successfully" });
        }

        private Guid? TryGetCurrentUserId()
        {
            var subClaim = User.FindFirstValue(ClaimTypes.NameIdentifier)
                           ?? User.FindFirstValue("sub");

            if (Guid.TryParse(subClaim, out var userId))
            {
                return userId;
            }

            return null;
        }

        public class ChangePasswordRequest
        {
            public string CurrentPassword { get; set; } = string.Empty;
            public string NewPassword { get; set; } = string.Empty;
        }
    }
}
