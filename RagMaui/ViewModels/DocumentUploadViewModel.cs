using RagMaui.Services;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;

namespace RagMaui.ViewModels
{
    /// <summary>
    /// ViewModel for document upload functionality
    /// </summary>
    public class DocumentUploadViewModel : ViewModelBase
    {
        private readonly IRagApiService _ragApiService;
        private bool _isUploading;
        private string _statusMessage = "Valitse dokumentti ladattavaksi.";
        private bool _isError;
        private string _documentType = "Resume";

        /// <summary>
        /// Available document types
        /// </summary>
        public string[] DocumentTypes { get; } = new[]
        {
            "Resume",
            "CoverLetter",
            "JobPosting",
            "Document"
        };

        /// <summary>
        /// Indicates if a document is being uploaded
        /// </summary>
        public bool IsUploading
        {
            get => _isUploading;
            set => SetProperty(ref _isUploading, value);
        }

        /// <summary>
        /// Status message for upload operation
        /// </summary>
        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        /// <summary>
        /// Indicates if there was an upload error
        /// </summary>
        public bool IsError
        {
            get => _isError;
            set => SetProperty(ref _isError, value);
        }

        /// <summary>
        /// Type of document being uploaded
        /// </summary>
        public string DocumentType
        {
            get => _documentType;
            set => SetProperty(ref _documentType, value);
        }

        /// <summary>
        /// Command to upload a document
        /// </summary>
        public ICommand UploadDocumentCommand { get; }

        /// <summary>
        /// Constructor for the document upload view model
        /// </summary>
        public DocumentUploadViewModel(IRagApiService ragApiService)
        {
            _ragApiService = ragApiService;
            UploadDocumentCommand = new AsyncRelayCommand(UploadDocumentAsync);
        }

        /// <summary>
        /// Uploads a document
        /// </summary>
        private async Task UploadDocumentAsync()
        {
            if (IsUploading)
                return;

            try
            {
                IsUploading = true;
                IsError = false;
                StatusMessage = "Valitaan tiedosto...";

                // Pick a file
                var result = await FilePicker.PickAsync(new PickOptions
                {
                    FileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
                    {
                        { DevicePlatform.iOS, new[] { "com.adobe.pdf" } },
                        { DevicePlatform.Android, new[] { "application/pdf" } },
                        { DevicePlatform.WinUI, new[] { ".pdf" } },
                        { DevicePlatform.MacCatalyst, new[] { "pdf" } }
                    }),
                    PickerTitle = "Valitse PDF-tiedosto"
                });

                if (result == null)
                {
                    StatusMessage = "Tiedoston valinta peruutettu.";
                    return;
                }

                if (!result.FileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
                {
                    IsError = true;
                    StatusMessage = "Vain PDF-tiedostot ovat tuettuja.";
                    return;
                }

                StatusMessage = "Ladataan tiedostoa...";

                using (var stream = await result.OpenReadAsync())
                {
                    string documentId = await _ragApiService.UploadDocumentAsync(
                        stream,
                        result.FileName,
                        DocumentType);

                    StatusMessage = $"Tiedosto ladattu onnistuneesti!\nDokumentin ID: {documentId}";
                }
            }
            catch (Exception ex)
            {
                IsError = true;
                StatusMessage = $"Virhe: {ex.Message}";
            }
            finally
            {
                IsUploading = false;
            }
        }
    }
}