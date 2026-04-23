using AcademicResearchProtocol.Application.DTOs;
using AcademicResearchProtocol.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace AcademicResearchProtocol.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        // GET: api/Users/all - Available to Professors and Admins for assignment
        [Authorize(Roles = "Admin,Professor")]
        [HttpGet("all")]
        public IEnumerable<UserDto> GetAllUsersForAssignment()
        {
            return _userService.GetAllUsers();
        }

        // GET: api/Users - Admin only
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public IEnumerable<UserDto> GetUsers()
        {
            return _userService.GetAllUsers();
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("{id}")]
        public ActionResult<UserDto> GetUserById(int id)
        {
            var user = _userService.GetUserById(id);
            if (user == null)
                return NotFound();
            return Ok(user);
        }

        [HttpPost]
        [AllowAnonymous]  // Allow registration without login
        public IActionResult CreateUser([FromBody] CreateUserDto dto)
        {
            _userService.CreateUser(dto);
            return Ok("User created successfully");
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public IActionResult DeleteUser(int id)
        {
            _userService.DeleteUser(id);
            return Ok("User deleted");
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("{userId}/assign-role/{roleId}")]
        public IActionResult AssignRole(int userId, int roleId)
        {
            _userService.AssignRole(userId, roleId);
            return Ok("Role assigned successfully");
        }

        // GET: api/Users/chat-list - Available to all authenticated users for chat
        [Authorize]
        [HttpGet("chat-list")]
        public IActionResult GetChatUsers()
        {
            var users = _userService.GetAllUsers();
            return Ok(users);
        }


    }
}