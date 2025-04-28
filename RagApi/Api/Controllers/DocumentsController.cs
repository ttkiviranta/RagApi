using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RagApi.Interfaces;

namespace RagApi.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class DocumentsController : ControllerBase
    {
        private readonly IDocumentService _documentService;

        public DocumentsController(IDocumentService documentService)
        {
            _documentService = documentService;
        }

        /// <summary>
        /// Get a document by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            try
            {
                var document = await _documentService.GetByIdAsync(id);
                return Ok(new { error = false, data = document });
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { error = true, message = "Document not found" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = true, message = $"Error getting document: {ex.Message}" });
            }
        }

        /// <summary>
        /// Download document content
        /// </summary>
        [HttpGet("{id}/content")]
        public async Task<IActionResult> DownloadContent(string id)
        {
            try
            {
                var document = await _documentService.GetByIdAsync(id);
                var content = await _documentService.GetDocumentContentAsync(id);

                return File(content, document.ContentType, document.FileName);
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { error = true, message = "Document not found" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = true, message = $"Error downloading document: {ex.Message}" });
            }
        }

        /// <summary>
        /// Get documents by entity ID
        /// </summary>
        [HttpGet("entity/{entityId}")]
        public async Task<IActionResult> GetByEntityId(string entityId)
        {
            try
            {
                var documents = await _documentService.GetByEntityIdAsync(entityId);
                return Ok(new { error = false, data = documents });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = true, message = $"Error getting documents: {ex.Message}" });
            }
        }

        /// <summary>
        /// Upload a document
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> UploadDocument(IFormFile file, [FromForm] string documentType, [FromForm] string entityId)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return BadRequest(new { error = true, message = "No file was uploaded" });

                var userId = GetCurrentUserId();
                var documentId = await _documentService.UploadAndProcessDocumentAsync(file, documentType, entityId, userId);

                return Ok(new { error = false, data = new { documentId } });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = true, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = true, message = $"Error uploading document: {ex.Message}" });
            }
        }

        /// <summary>
        /// Delete a document
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                await _documentService.DeleteAsync(id);
                return Ok(new { error = false, message = "Document deleted successfully" });
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { error = true, message = "Document not found" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = true, message = $"Error deleting document: {ex.Message}" });
            }
        }

        private string GetCurrentUserId()
        {
            // Get the logged-in user's ID
            return User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }
    }
}
