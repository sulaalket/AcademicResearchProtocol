using AcademicResearchProtocol.Application.DTOs;
using AcademicResearchProtocol.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace AcademicResearchProtocol.Web.Controllers
{
    [Authorize]  // ← All authenticated users can access, but with role restrictions per method
    [ApiController]
    [Route("api/[controller]")]
    public class DocumentsController : ControllerBase
    {
        private readonly IDocumentService _documentService;
        private readonly ICurrentUserService _currentUserService;
        private readonly IPdfGeneratorService _pdfGeneratorService;

        public DocumentsController(
            IDocumentService documentService,
            ICurrentUserService currentUserService,
            IPdfGeneratorService pdfGeneratorService)  
        {
            _documentService = documentService;
            _currentUserService = currentUserService;
            _pdfGeneratorService = pdfGeneratorService;  
        }

        // ✅ All authenticated users can view documents (including Students)
        [HttpGet]
        public IEnumerable<DocumentDto> GetAllDocuments()
        {
            return _documentService.GetAllDocuments();
        }

        // ✅ All authenticated users can view a document by ID
        [HttpGet("{id}")]
        public ActionResult<DocumentDto> GetDocumentById(int id)
        {
            var document = _documentService.GetDocumentById(id);
            if (document == null)
                return NotFound();
            return Ok(document);
        }

        // ✅ All authenticated users can view their own documents
        [HttpGet("user/{userId}")]
        public IEnumerable<DocumentDto> GetDocumentsForUser(int userId)
        {
            return _documentService.GetDocumentsForUser(userId);
        }

        // ❌ Only Admin, Professor, Manager can create documents
        [Authorize(Roles = "Admin,Professor")]
        [HttpPost]
        public IActionResult CreateDocument([FromBody] CreateDocumentDto dto)
        {
            var userId = _currentUserService.GetCurrentUserId();
            var document = _documentService.CreateDocument(dto, userId);
            return CreatedAtAction(nameof(GetDocumentById), new { id = document.Id }, document);
        }

        [HttpPut("{id}/status")]
        [Authorize]  
        public IActionResult UpdateStatus(int id, [FromBody] string status)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var isStudent = User.IsInRole("Student");

            var document = _documentService.GetDocumentById(id);
            if (document == null)
                return NotFound();

           
            if (isStudent && document.AssignedToUserId != userId)
                return Forbid("You can only update documents assigned to you");

            if (isStudent)
            {
                if (document.Status == "Pending" && status != "InProgress")
                    return BadRequest("You can only start work on pending documents");

                if (document.Status == "InProgress" && status != "Completed")
                    return BadRequest("You can only complete documents that are in progress");
            }

            _documentService.UpdateDocumentStatus(id, status);
            return Ok();
        }

        // ❌ Only Admin can delete documents
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public IActionResult DeleteDocument(int id)
        {
            _documentService.DeleteDocument(id);
            return Ok("Document deleted");
        }

        [HttpPost("{id}/upload")]
        [Authorize]  
        public async Task<IActionResult> Upload(int id, IFormFile file)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var isStudent = User.IsInRole("Student");

            // Get document to check assignment
            var doc = _documentService.GetDocumentById(id);
            if (doc == null) return NotFound();

            // Students can only upload to documents assigned to them
            if (isStudent && doc.AssignedToUserId != userId)
                return Forbid("You can only upload to documents assigned to you");

            await _documentService.UploadFileAsync(id, file);
            return Ok(new { message = "File uploaded successfully" });
        }

        // ✅ All authenticated users can download files (if they have access)
        [HttpGet("{id}/download")]
        public async Task<IActionResult> DownloadFile(int id)
        {
            try
            {
                var document = _documentService.GetDocumentById(id);
                if (document == null)
                    return NotFound("Document not found");

                var fileBytes = await _documentService.DownloadFileAsync(id);
                return File(fileBytes, document.FileType ?? "application/octet-stream", document.FileName);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("protocol-book")]
        [Authorize(Roles = "Admin,Professor")]
        public async Task<IActionResult> DownloadProtocolBook()
        {
            try
            {
                var pdfBytes = await _pdfGeneratorService.GenerateProtocolBookAsync();
                return File(pdfBytes, "application/pdf", $"ProtocolBook_{DateTime.UtcNow:yyyyMMdd}.pdf");
            }
            catch (Exception ex)
            {
                return BadRequest($"Error generating PDF: {ex.Message}");
            }
        }

        // GET: api/Documents/reports/delayed
        [Authorize(Roles = "Admin,Professor")]
        [HttpGet("reports/delayed")]
        public IEnumerable<DocumentDto> GetDelayedDocuments()
        {
            var documents = _documentService.GetAllDocuments();
            return documents.Where(d => d.IsOverdue && d.Status != "Completed");
        }

        // GET: api/Documents/reports/by-priority/{priority}
        [Authorize(Roles = "Admin,Professor")]
        [HttpGet("reports/by-priority/{priority}")]
        public IEnumerable<DocumentDto> GetDocumentsByPriority(int priority)
        {
            var documents = _documentService.GetAllDocuments();
            return documents.Where(d => d.Priority == priority);
        }
    }
}