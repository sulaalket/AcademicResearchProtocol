using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using System.Security.Claims;
using AcademicResearchProtocol.Infrastructure.Data;
using AcademicResearchProtocol.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AcademicResearchProtocol.Web.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly ApplicationDbContext _dbContext;

        public ChatHub(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task SendMessage(int receiverId, string message)
        {
            var senderId = int.Parse(Context.UserIdentifier);
            var senderName = Context.User?.Identity?.Name ?? senderId.ToString();

            // Save message to database
            var chatMessage = new ChatMessage
            {
                SenderId = senderId,
                ReceiverId = receiverId,
                Message = message,
                SentAt = DateTime.UtcNow
            };

            _dbContext.ChatMessages.Add(chatMessage);
            await _dbContext.SaveChangesAsync();

            // Send to receiver if connected
            await Clients.User(receiverId.ToString()).SendAsync("ReceiveMessage", senderId, senderName, message, DateTime.UtcNow);
        }

        public async Task<List<ChatMessage>> GetChatHistory(int withUserId)
        {
            var currentUserId = int.Parse(Context.UserIdentifier);

            var messages = await _dbContext.ChatMessages
                .Where(m => (m.SenderId == currentUserId && m.ReceiverId == withUserId) ||
                           (m.SenderId == withUserId && m.ReceiverId == currentUserId))
                .OrderBy(m => m.SentAt)
                .ToListAsync();

            return messages;
        }
    }
}