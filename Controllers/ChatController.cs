using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using AcademicResearchProtocol.Web.Hubs;
using AcademicResearchProtocol.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;

namespace AcademicResearchProtocol.Web.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ChatController : ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IHubContext<ChatHub> _hubContext;

        public ChatController(ApplicationDbContext dbContext, IHubContext<ChatHub> hubContext)
        {
            _dbContext = dbContext;
            _hubContext = hubContext;
        }

        [HttpGet("users")]
        public async Task<IActionResult> GetChatUsers()
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            var users = await _dbContext.Users
                .Where(u => u.Id != currentUserId)
                .Select(u => new { u.Id, u.Name })
                .ToListAsync();

            return Ok(users);
        }

        [HttpGet("history/{userId}")]
        public async Task<IActionResult> GetChatHistory(int userId)
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            var messages = await _dbContext.ChatMessages
                .Where(m => (m.SenderId == currentUserId && m.ReceiverId == userId) ||
                           (m.SenderId == userId && m.ReceiverId == currentUserId))
                .OrderBy(m => m.SentAt)
                .Select(m => new { m.SenderId, m.ReceiverId, m.Message, m.SentAt })
                .ToListAsync();

            return Ok(messages);
        }
    }
}