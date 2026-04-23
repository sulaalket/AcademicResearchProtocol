using AcademicResearchProtocol.Application.DTOs;
using AcademicResearchProtocol.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace AcademicResearchProtocol.Web.Controllers
{
    [Authorize(Roles = "Admin,Professor,Manager")]  // ← Admin and Professor can manage tracking
    [ApiController]
    [Route("api/[controller]")]
    public class DocumentTrackingController : ControllerBase
    {
        private readonly IDocumentTrackingService _trackingService;
        private readonly ICurrentUserService _currentUserService;

        public DocumentTrackingController(IDocumentTrackingService trackingService, ICurrentUserService currentUserService)
        {
            _trackingService = trackingService;
            _currentUserService = currentUserService;
        }

        [HttpGet]
        public IEnumerable<DocumentTrackingDto> GetAllTrackings()
        {
            return _trackingService.GetAllTrackings();
        }

        [HttpGet("document/{documentId}")]
        public IEnumerable<DocumentTrackingDto> GetDocumentHistory(int documentId)
        {
            return _trackingService.GetDocumentHistory(documentId);
        }

        [HttpGet("user/{userId}")]
        public IEnumerable<DocumentTrackingDto> GetUserAssignments(int userId)
        {
            return _trackingService.GetUserAssignments(userId);
        }

        [HttpPost]
        public IActionResult AssignDocument([FromBody] CreateTrackingDto dto)
        {
            try
            {
                var assignedByUserId = _currentUserService.GetCurrentUserId();
                var tracking = _trackingService.AssignDocument(dto, assignedByUserId);
                return CreatedAtAction(nameof(GetDocumentHistory), new { documentId = tracking.DocumentId }, tracking);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("{trackingId}/complete")]
        public IActionResult CompleteAssignment(int trackingId, [FromBody] string? comments)
        {
            try
            {
                _trackingService.CompleteAssignment(trackingId, comments);
                return Ok("Assignment completed");
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }
    }
}