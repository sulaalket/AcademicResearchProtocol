using AcademicResearchProtocol.Application.DTOs;
using AcademicResearchProtocol.Application.Services;
using Microsoft.AspNetCore.Mvc;
using BCrypt.Net;
using AcademicResearchProtocol.Infrastructure.Data;

namespace AcademicResearchProtocol.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ApplicationDbContext _context;

        public AuthController(IUserService userService, ApplicationDbContext context)
        {
            _userService = userService;
            _context = context;
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginDto loginDto)
        {
            var response = _userService.Login(loginDto.Email, loginDto.Password);
            if (response == null)
                return Unauthorized(new { message = "Invalid email or password" });

            return Ok(response);
        }

        [HttpPost("change-password")]
        public IActionResult ChangePassword([FromBody] ChangePasswordDto dto)
        {
            var user = _context.Users.Find(dto.UserId);
            if (user == null)
                return NotFound();

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            user.MustChangePassword = false;

            _context.SaveChanges();

            return Ok();
        }
    }
}