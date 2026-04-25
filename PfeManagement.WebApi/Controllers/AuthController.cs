using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PfeManagement.Application.DTOs.Auth;
using PfeManagement.Application.Interfaces;

namespace PfeManagement.WebApi.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        // GRASP Pattern: Controller. The API Controller delegates work to Application services.
        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("signup/student")]
        [HttpPost("register/student")]
        public async Task<IActionResult> SignupStudent([FromBody] StudentRegistrationDto dto)
        {
            try
            {
                var result = await _authService.RegisterAsync(dto);
                return StatusCode(201, new
                {
                    success = true,
                    message = "Student registered successfully",
                    data = new
                    {
                        userId = result.UserId,
                        email = result.Email,
                        role = result.Role.ToString()
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
                var result = await _authService.RegisterAsync(dto);
                return StatusCode(201, new
                {
                    success = true,
                    message = "Company supervisor registered successfully",
                    data = new
                    {
                        userId = result.UserId,
                        email = result.Email,
                        role = result.Role.ToString()
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
                var result = await _authService.RegisterAsync(dto);
                return StatusCode(201, new
                {
                    success = true,
                    message = "University supervisor registered successfully",
                    data = new
                    {
                        userId = result.UserId,
                        email = result.Email,
                        role = result.Role.ToString()
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

                await _authService.VerifyEmailAsync(token);
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
                var result = await _authService.LoginAsync(dto);
                Response.Cookies.Append("refreshToken", result.RefreshToken, new CookieOptions
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
                            id = result.UserId,
                            email = result.Email,
                            role = result.Role.ToString(),
                            isVerified = result.IsVerified
                        },
                        accessToken = result.AccessToken
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

                var result = await _authService.RefreshTokenAsync(new RefreshTokenDto { RefreshToken = refreshToken });

                return Ok(new
                {
                    success = true,
                    message = "Token refreshed successfully",
                    data = new
                    {
                        accessToken = result.AccessToken
                    }
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
            await _authService.RequestPasswordResetAsync(dto);
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

                dto.ResetToken = resetToken;
                await _authService.ResetPasswordAsync(dto);
                return Ok(new { success = true, message = "Password reset successful" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            if (Request.Cookies.TryGetValue("refreshToken", out var refreshToken) && !string.IsNullOrWhiteSpace(refreshToken))
            {
                await _authService.LogoutAsync(refreshToken);
            }

            Response.Cookies.Delete("refreshToken");
            return Ok(new { success = true, message = "Logout successful" });
        }
    }
}
