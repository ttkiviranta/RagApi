using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using RagApi.Data;
using RagApi.Interfaces;
using RagApi.Mapping;
using RagApi.Models;

namespace RagApi.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DocumentsController : BaseController
    {
        private readonly IDocumentService _documentService;
        private readonly ApplicationDbContext _dbContext;
        private readonly ILogger<DocumentsController> _logger;

        public DocumentsController(
            IDocumentService documentService,
            ApplicationDbContext dbContext,
            IRequestContext requestContext,
            ILogger<DocumentsController> logger)
            : base(requestContext)
        {
            _documentService = documentService;
            _dbContext = dbContext;
            _logger = logger;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            try
            {
                var document = await _documentService.GetByIdAsync(id);
                return Success(document);
            }
            catch (KeyNotFoundException)
            {
                return NotFoundError("Document not found");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving document with ID {DocumentId}", id);
                return Error($"Error getting document: {ex.Message}");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var documents = await _documentService.GetByEntityIdAsync(string.Empty);
                return Success(documents ?? new List<Document>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all documents");
                return Success(new List<Document>());
            }
        }

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
                return NotFoundError("Document not found");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downloading document content for ID {DocumentId}", id);
                return Error($"Error downloading document: {ex.Message}");
            }
        }

        [HttpGet("entity/{entityId}")]
        public async Task<IActionResult> GetByEntityId(string entityId)
        {
            try
            {
                var documents = await _documentService.GetByEntityIdAsync(entityId);
                return Success(documents);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving documents for entity {EntityId}", entityId);
                return Error($"Error getting documents: {ex.Message}");
            }
        }

        [HttpPost]
        public async Task<IActionResult> UploadDocument(IFormFile file, [FromForm] string documentType, [FromForm] string entityId)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return BadRequestError("No file was uploaded");

                // Käytä RequestContext-palvelua käyttäjän ID:n hakemiseen
                var userId = RequestContext.GetCurrentUserId();

                if (!string.IsNullOrEmpty(userId))
                {
                    var userExists = await _dbContext.Users.AnyAsync(u => u.Id == userId);
                    if (!userExists)
                    {
                        userId = null;
                    }
                }

                var documentId = await _documentService.UploadAndProcessDocumentAsync(file, documentType, entityId, userId);

                return Success(new { documentId });
            }
            catch (ArgumentException ex)
            {
                return BadRequestError(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading document. Type: {DocumentType}, EntityId: {EntityId}",
                    documentType, entityId);
                return Error($"Error uploading document: {ex.Message}");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                await _documentService.DeleteAsync(id);
                return Success(new { message = "Document deleted successfully" });
            }
            catch (KeyNotFoundException)
            {
                return NotFoundError("Document not found");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting document with ID {DocumentId}", id);
                return Error($"Error deleting document: {ex.Message}");
            }
        }
    }
}