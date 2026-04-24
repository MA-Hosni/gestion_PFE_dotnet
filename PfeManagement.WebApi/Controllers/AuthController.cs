using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using PfeManagement.WebApi.Data;
using PfeManagement.WebApi.Helpers;
using PfeManagement.WebApi.Models;

namespace PfeManagement.WebApi.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly IConfiguration _config;
        private static readonly ConcurrentDictionary<string, (Guid UserId, DateTime ExpiresAt)> RefreshTokens = new();

        public AuthController(AppDbContext db, IConfiguration config)
        {
            _db = db;
            _config = config;
        }

        [HttpPost("signup/student")]
        [HttpPost("register/student")]
        public async Task<IActionResult> SignupStudent([FromBody] StudentRegistrationDto dto)
        {
            try
            {
                // Check if email already exists
                var exists = await _db.Users.AnyAsync(u => u.Email == dto.Email);
                if (exists)
                    return BadRequest(new { success = false, message = "Email already registered" });

                // Create student directly - no factory pattern needed
                var student = new Student
                {
                    FullName = dto.FullName,
                    Email = dto.Email,
                    PhoneNumber = dto.PhoneNumber,
                    Role = UserRole.Student,
                    PasswordHash = PasswordHelper.HashPassword(dto.Password),
                    VerificationToken = Guid.NewGuid().ToString("N"),
                    Cin = dto.Cin,
                    StudentIdCardIMG = dto.StudentIdCardIMG,
                    CompanyName = dto.CompanyName,
                    Degree = dto.Degree,
                    DegreeType = dto.DegreeType,
                    CompSupervisorId = dto.CompSupervisorId,
                    UniSupervisorId = dto.UniSupervisorId
                };

                _db.Students.Add(student);
                await _db.SaveChangesAsync();

                // Try to send verification email
                try
                {
                    await EmailHelper.SendVerificationEmail(student.Email, student.FullName, student.VerificationToken!, _config);
                }
                catch { }

                return StatusCode(201, new
                {
                    success = true,
                    message = "Student registered successfully",
                    data = new
                    {
                        userId = student.Id,
                        email = student.Email,
                        role = student.Role.ToString()
                    }
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpPost("signup/supervisor-company")]
        [HttpPost("register/company-supervisor")]
        public async Task<IActionResult> SignupCompanySupervisor([FromBody] CompanySupervisorRegistrationDto dto)
        {
            try
            {
                var exists = await _db.Users.AnyAsync(u => u.Email == dto.Email);
                if (exists)
                    return BadRequest(new { success = false, message = "Email already registered" });

                // Create company supervisor directly
                var supervisor = new CompanySupervisor
                {
                    FullName = dto.FullName,
                    Email = dto.Email,
                    PhoneNumber = dto.PhoneNumber,
                    Role = UserRole.CompSupervisor,
                    PasswordHash = PasswordHelper.HashPassword(dto.Password),
                    VerificationToken = Guid.NewGuid().ToString("N"),
                    CompanyName = dto.CompanyName,
                    BadgeIMG = dto.BadgeIMG
                };

                _db.CompanySupervisors.Add(supervisor);
                await _db.SaveChangesAsync();

                try
                {
                    await EmailHelper.SendVerificationEmail(supervisor.Email, supervisor.FullName, supervisor.VerificationToken!, _config);
                }
                catch { }

                return StatusCode(201, new
                {
                    success = true,
                    message = "Company supervisor registered successfully",
                    data = new
                    {
                        userId = supervisor.Id,
                        email = supervisor.Email,
                        role = supervisor.Role.ToString()
                    }
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpPost("signup/supervisor-university")]
        public async Task<IActionResult> SignupUniversitySupervisor([FromBody] UniversitySupervisorRegistrationDto dto)
        {
            try
            {
                var exists = await _db.Users.AnyAsync(u => u.Email == dto.Email);
                if (exists)
                    return BadRequest(new { success = false, message = "Email already registered" });

                // Create university supervisor directly
                var supervisor = new UniversitySupervisor
                {
                    FullName = dto.FullName,
                    Email = dto.Email,
                    PhoneNumber = dto.PhoneNumber,
                    Role = UserRole.UniSupervisor,
                    PasswordHash = PasswordHelper.HashPassword(dto.Password),
                    VerificationToken = Guid.NewGuid().ToString("N"),
                    BadgeIMG = dto.BadgeIMG
                };

                _db.UniversitySupervisors.Add(supervisor);
                await _db.SaveChangesAsync();

                try
                {
                    await EmailHelper.SendVerificationEmail(supervisor.Email, supervisor.FullName, supervisor.VerificationToken!, _config);
                }
                catch { }

                return StatusCode(201, new
                {
                    success = true,
                    message = "University supervisor registered successfully",
                    data = new
                    {
                        userId = supervisor.Id,
                        email = supervisor.Email,
                        role = supervisor.Role.ToString()
                    }
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpGet("verify-email")]
        public async Task<IActionResult> VerifyEmail([FromQuery] string token)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(token))
                {
                    return BadRequest(new { success = false, message = "Verification token is required" });
                }

                var user = await _db.Users.FirstOrDefaultAsync(u => u.VerificationToken == token);
                if (user == null)
                {
                    throw new Exception("Invalid verification token");
                }

                user.IsVerified = true;
                user.VerificationToken = null;
                await _db.SaveChangesAsync();

                return Ok(new { success = true, message = "Email verified successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            try
            {
                var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
                if (user == null || !PasswordHelper.VerifyPassword(dto.Password, user.PasswordHash))
                {
                    throw new Exception("Invalid email or password");
                }

                var accessToken = JwtHelper.GenerateToken(user, _config);
                var refreshToken = Guid.NewGuid().ToString("N");
                RefreshTokens[refreshToken] = (user.Id, DateTime.UtcNow.AddDays(7));

                Response.Cookies.Append("refreshToken", refreshToken, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = false,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTimeOffset.UtcNow.AddDays(7)
                });

                return Ok(new
                {
                    success = true,
                    message = "Login successful",
                    data = new
                    {
                        user = new
                        {
                            id = user.Id,
                            email = user.Email,
                            role = user.Role.ToString(),
                            isVerified = user.IsVerified
                        },
                        accessToken
                    }
                });
            }
            catch (Exception ex)
            {
                return Unauthorized(new { success = false, message = ex.Message });
            }
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken()
        {
            try
            {
                if (!Request.Cookies.TryGetValue("refreshToken", out var refreshToken) || string.IsNullOrWhiteSpace(refreshToken))
                {
                    return Unauthorized(new { success = false, message = "Refresh token not provided" });
                }

                if (!RefreshTokens.TryGetValue(refreshToken, out var tokenData))
                {
                    throw new Exception("Invalid refresh token");
                }

                if (tokenData.ExpiresAt <= DateTime.UtcNow)
                {
                    RefreshTokens.TryRemove(refreshToken, out _);
                    throw new Exception("Refresh token expired");
                }

                var user = await _db.Users.FindAsync(tokenData.UserId);
                if (user == null)
                {
                    throw new Exception("User not found");
                }

                var accessToken = JwtHelper.GenerateToken(user, _config);

                return Ok(new
                {
                    success = true,
                    message = "Token refreshed successfully",
                    data = new { accessToken }
                });
            }
            catch (Exception ex)
            {
                return Unauthorized(new { success = false, message = ex.Message });
            }
        }

        [HttpPost("request-password-reset")]
        public async Task<IActionResult> RequestPasswordReset([FromBody] PasswordResetRequestDto dto)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (user != null)
            {
                user.PasswordResetToken = Guid.NewGuid().ToString("N");
                user.PasswordResetExpires = DateTime.UtcNow.AddHours(1);
                await _db.SaveChangesAsync();

                try
                {
                    await EmailHelper.SendPasswordResetEmail(user.Email, user.FullName, user.PasswordResetToken, _config);
                }
                catch { }
            }

            return Ok(new
            {
                success = true,
                message = "If the email exists, a reset link has been sent"
            });
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromQuery] string resetToken, [FromBody] PasswordResetDto dto)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(resetToken))
                {
                    return BadRequest(new { success = false, message = "Reset token is required" });
                }

                var user = await _db.Users.FirstOrDefaultAsync(u => u.PasswordResetToken == resetToken);
                if (user == null || !user.PasswordResetExpires.HasValue || user.PasswordResetExpires <= DateTime.UtcNow)
                {
                    throw new Exception("Invalid or expired reset token");
                }

                user.PasswordHash = PasswordHelper.HashPassword(dto.NewPassword);
                user.PasswordResetToken = null;
                user.PasswordResetExpires = null;
                await _db.SaveChangesAsync();

                return Ok(new { success = true, message = "Password reset successful" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [Authorize]
        [HttpPost("logout")]
        public Task<IActionResult> Logout()
        {
            if (Request.Cookies.TryGetValue("refreshToken", out var refreshToken) && !string.IsNullOrWhiteSpace(refreshToken))
            {
                RefreshTokens.TryRemove(refreshToken, out _);
            }

            Response.Cookies.Delete("refreshToken");
            return System.Threading.Tasks.Task.FromResult<IActionResult>(Ok(new { success = true, message = "Logout successful" }));
        }
    }
}
