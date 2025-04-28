using RagApi.Models;

public class Document
{
    public string Id { get; set; }
    public string FileName { get; set; }
    public string BlobStoragePath { get; set; }
    public string DocumentType { get; set; } // Resume, CoverLetter, JobPosting, etc.
    public string ContentType { get; set; } // MIME-tyyppi
    public string EntityId { get; set; } // Viittaus johonkin entiteettiin (kandidaatti, työpaikka, jne.)
    public string Metadata { get; set; } // JSON-muotoista metadataa
    public DateTime UploadedDate { get; set; }
    public string UploadedByUserId { get; set; }

    // Navigointiproperties
    public User UploadedByUser { get; set; }
}
