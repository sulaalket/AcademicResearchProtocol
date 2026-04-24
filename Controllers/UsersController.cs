using AcademicResearchProtocol.Application.DTOs;
using AcademicResearchProtocol.Application.Services;
using AcademicResearchProtocol.Domain.Interfaces;
using AcademicResearchProtocol.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;

namespace AcademicResearchProtocol.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IUserRepository _userRepository;

        public UsersController(IUserService userService, IUserRepository userRepository)
        {
            _userService = userService;
            _userRepository = userRepository;
        }

        [Authorize(Roles = "Admin,Professor")]
        [HttpGet("all")]
        public IActionResult GetAllUsersForAssignment()
        {
            return Ok(_userService.GetAllUsers());
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public IActionResult GetUsers()
        {
            return Ok(_userService.GetAllUsers());
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("{id}")]
        public IActionResult GetUserById(int id)
        {
            var user = _userService.GetUserById(id);
            if (user == null) return NotFound();
            return Ok(user);
        }

        [HttpPost]
        [AllowAnonymous]
        public IActionResult CreateUser([FromBody] CreateUserDto dto)
        {
            _userService.CreateUser(dto);
            return Ok("User created successfully");
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public IActionResult DeleteUser(int id)
        {
            try
            {
                _userRepository.Delete(id);
                return Ok(new { message = "User deleted successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("{userId}/assign-role/{roleId}")]
        public IActionResult AssignRole(int userId, int roleId)
        {
            _userService.AssignRole(userId, roleId);
            return Ok("Role assigned successfully");
        }

        [Authorize]
        [HttpGet("chat-list")]
        public IActionResult GetChatUsers()
        {
            return Ok(_userService.GetAllUsers());
        }
    }
}