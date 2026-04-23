using AcademicResearchProtocol.Application.DTOs;
using AcademicResearchProtocol.Application.Services;
using AcademicResearchProtocol.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace AcademicResearchProtocol.Web.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ResearchTopicsController : ControllerBase
    {
        private readonly IResearchTopicService _topicService;
        private readonly ICurrentUserService _currentUserService;
        private readonly IFileStorageService _fileStorageService;
        private readonly IAiService _aiService;

        public ResearchTopicsController(
      IResearchTopicService topicService,
      ICurrentUserService currentUserService,
      IFileStorageService fileStorageService,
       IAiService aiService)  
        {
            _topicService = topicService;
            _currentUserService = currentUserService;
            _fileStorageService = fileStorageService;
            _aiService = aiService;
        }

        // GET: api/ResearchTopics - Role-based filtering
        [HttpGet]
        public IEnumerable<ResearchTopicDto> GetAllTopics()
        {
            var userId = _currentUserService.GetCurrentUserId();
            var roles = User.Claims
                .Where(c => c.Type == ClaimTypes.Role)
                .Select(c => c.Value)
                .ToList();

            return _topicService.GetAllTopics(userId, roles);
        }

        // GET: api/ResearchTopics/student/{studentId}
        [HttpGet("student/{studentId}")]
        public IEnumerable<ResearchTopicDto> GetTopicsByStudent(int studentId)
        {
            return _topicService.GetTopicsByStudentId(studentId);
        }

        // GET: api/ResearchTopics/professor/{professorId}
        [HttpGet("professor/{professorId}")]
        public IEnumerable<ResearchTopicDto> GetTopicsByProfessor(int professorId)
        {
            return _topicService.GetTopicsByProfessorId(professorId);
        }

        // GET: api/ResearchTopics/status/{status}
        [HttpGet("status/{status}")]
        public IEnumerable<ResearchTopicDto> GetTopicsByStatus(string status)
        {
            return _topicService.GetTopicsByStatus(status);
        }

        // GET: api/ResearchTopics/{id}
        [HttpGet("{id}")]
        public ActionResult<ResearchTopicDto> GetTopicById(int id)
        {
            var userId = _currentUserService.GetCurrentUserId();
            var roles = User.Claims
                .Where(c => c.Type == ClaimTypes.Role)
                .Select(c => c.Value)
                .ToList();

            var topic = _topicService.GetTopicById(id, userId, roles);
            if (topic == null)
                return NotFound();
            return Ok(topic);
        }

        // POST: api/ResearchTopics - Students only
        [Authorize(Roles = "Student")]
        [HttpPost]
        public IActionResult CreateTopic([FromBody] CreateResearchTopicDto dto)
        {
            try
            {
                var topic = _topicService.CreateTopic(dto);
                return CreatedAtAction(nameof(GetTopicById), new { id = topic.Id }, topic);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // POST: api/ResearchTopics/{topicId}/assign - Professors claim topic
        [Authorize(Roles = "Professor,Admin")]
        [HttpPost("{topicId}/assign")]
        public IActionResult AssignTopic(int topicId)
        {
            try
            {
                var professorId = _currentUserService.GetCurrentUserId();
                var topic = _topicService.AssignTopic(topicId, professorId);
                return Ok(topic);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // POST: api/ResearchTopics/{topicId}/start-work - Professor starts work (optional)
        [Authorize(Roles = "Professor,Admin")]
        [HttpPost("{topicId}/start-work")]
        public IActionResult StartWork(int topicId)
        {
            try
            {
                var topic = _topicService.StartWork(topicId);
                return Ok(topic);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // POST: api/ResearchTopics/{topicId}/complete - Mark as completed
        [Authorize(Roles = "Professor,Admin")]
        [HttpPost("{topicId}/complete")]
        public IActionResult CompleteTopic(int topicId)
        {
            try
            {
                var topic = _topicService.CompleteTopic(topicId);
                return Ok(topic);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // POST: api/ResearchTopics/{topicId}/reject
        [Authorize(Roles = "Professor,Admin")]
        [HttpPost("{topicId}/reject")]
        public IActionResult RejectTopic(int topicId, [FromBody] string reason)
        {
            try
            {
                var topic = _topicService.RejectTopic(topicId, reason);
                return Ok(topic);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // POST: api/ResearchTopics/{topicId}/milestones
        [Authorize(Roles = "Professor,Admin")]
        [HttpPost("{topicId}/milestones")]
        public IActionResult AddMilestone(int topicId, [FromBody] CreateMilestoneDto dto)
        {
            try
            {
                _topicService.AddMilestone(topicId, dto);
                return Ok("Milestone added successfully");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // GET: api/ResearchTopics/{topicId}/milestones
        [HttpGet("{topicId}/milestones")]
        public IActionResult GetMilestones(int topicId)
        {
            var userId = _currentUserService.GetCurrentUserId();
            var roles = User.Claims
                .Where(c => c.Type == ClaimTypes.Role)
                .Select(c => c.Value)
                .ToList();

            // Check permission first
            var topic = _topicService.GetTopicById(topicId, userId, roles);
            if (topic == null)
                return NotFound("Topic not found or you don't have permission");

            var milestones = _topicService.GetMilestones(topicId);
            return Ok(milestones);
        }

        // POST: api/ResearchTopics/milestones/{milestoneId}/submit
        [HttpPost("milestones/{milestoneId}/submit")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> SubmitMilestone(int milestoneId, [FromForm] SubmitMilestoneDto dto)
        {
            try
            {
                var studentId = _currentUserService.GetCurrentUserId();
                await _topicService.SubmitMilestone(milestoneId, dto, studentId);
                return Ok("Milestone submitted successfully");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // POST: api/ResearchTopics/milestones/{milestoneId}/grade
        [Authorize(Roles = "Professor,Admin")]
        [HttpPost("milestones/{milestoneId}/grade")]
        public IActionResult GradeMilestone(int milestoneId, [FromBody] GradeMilestoneDto dto)
        {
            try
            {
                _topicService.GradeMilestone(milestoneId, dto);
                return Ok("Milestone graded successfully");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public IActionResult DeleteTopic(int id)
        {
            try
            {
                _topicService.DeleteTopic(id);
                return Ok("Topic deleted successfully");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("milestones/{milestoneId}/download")]
        public async Task<IActionResult> DownloadMilestoneFile(int milestoneId)
        {
            var milestone = _topicService.GetMilestoneById(milestoneId);
            if (milestone == null)
                return NotFound("Milestone not found");

            // Check permissions (student or professor assigned to topic)
            var userId = _currentUserService.GetCurrentUserId();
            var roles = User.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).ToList();
            var topic = _topicService.GetTopicById(milestone.ResearchTopicId, userId, roles);

            if (topic == null)
                return Forbid();

            if (string.IsNullOrEmpty(milestone.SubmissionFilePath))
                return NotFound("No file attached");

            var fileBytes = await _fileStorageService.GetFileAsync(milestone.SubmissionFilePath);
            return File(fileBytes, milestone.SubmissionFileType ?? "application/octet-stream", milestone.SubmissionFileName);
        }

        [HttpPost("{topicId}/summarize")]
        [Authorize(Roles = "Professor,Admin")]
        public async Task<IActionResult> SummarizeTopic(int topicId)
        {
            try
            {
                Console.WriteLine($"🎯 SummarizeTopic called for topic ID: {topicId}");

                var userId = _currentUserService.GetCurrentUserId();
                var roles = User.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).ToList();
                var topic = _topicService.GetTopicById(topicId, userId, roles);

                if (topic == null)
                {
                    Console.WriteLine($"❌ Topic {topicId} not found");
                    return NotFound("Topic not found");
                }

                Console.WriteLine($"📄 Topic found: {topic.Title}");

                var summary = await _aiService.SummarizeAbstractAsync(topic.Title, topic.Description);

                Console.WriteLine($"📝 Final summary: {summary}");

                return Ok(new { summary });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"💥 Exception in SummarizeTopic: {ex.Message}");
                return BadRequest($"Error: {ex.Message}");
            }
        }

        [Authorize(Roles = "Professor,Admin")]
        [HttpPost("{topicId}/calculate-final-grade")]
        public IActionResult CalculateFinalGrade(int topicId)
        {
            var topic = _topicService.UpdateFinalGrade(topicId);
            return Ok(new { finalGrade = topic.FinalGrade, gradedAt = topic.GradedAt });
        }

        [HttpPost("milestones/{milestoneId}/ai-summarize")]
        [Authorize(Roles = "Professor,Admin")]
        public async Task<IActionResult> AISummarizeMilestone(int milestoneId)
        {
            var milestone = _topicService.GetMilestoneById(milestoneId);
            if (milestone == null)
                return NotFound("Milestone not found");

            // Build text for AI
            var text = $"Milestone: {milestone.Name}\n";
            text += $"Description: {milestone.Description ?? "N/A"}\n";
            text += $"Student Notes: {milestone.SubmissionNotes ?? "No notes provided"}\n";

            // Extract text from PDF if file exists
            if (!string.IsNullOrEmpty(milestone.SubmissionFilePath) && milestone.SubmissionFileType == "application/pdf")
            {
                var pdfText = ExtractTextFromPdf(milestone.SubmissionFilePath);
                if (!string.IsNullOrEmpty(pdfText))
                {
                    // Limit text length to avoid token limits
                    pdfText = pdfText.Length > 3000 ? pdfText.Substring(0, 3000) + "..." : pdfText;
                    text += $"\n\nFile Content:\n{pdfText}";
                }
            }

            var summary = await _aiService.SummarizeTextAsync(text);

            return Ok(new { summary, milestoneName = milestone.Name });
        }

        // Add this helper method
        private string ExtractTextFromPdf(string filePath)
        {
            // You'll need to install: Install-Package UglyToad.PdfPig
            using (var pdf = UglyToad.PdfPig.PdfDocument.Open(filePath))
            {
                var text = new StringBuilder();
                foreach (var page in pdf.GetPages())
                {
                    text.Append(page.Text);
                }
                return text.ToString();
            }
        }
    }
}