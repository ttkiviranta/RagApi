using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RagApi.Interfaces;

namespace RagApi.Services
{
    /// <summary>
    /// Background service for processing PDF files from Service Bus queue
    /// Runs as a continuous service to handle asynchronous PDF processing
    /// </summary>
    public class PdfProcessingService : BackgroundService
    {
        private readonly ServiceBusClient _serviceBusClient;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<PdfProcessingService> _logger;
        private ServiceBusProcessor _processor;
        private const string PDF_QUEUE_NAME = "pdf-processing-queue";

        /// <summary>
        /// Constructor for PDF processing service
        /// </summary>
        /// <param name="serviceBusClient">Azure Service Bus client</param>
        /// <param name="serviceProvider">Service provider for dependency resolution</param>
        /// <param name="logger">Logger for the service</param>
        public PdfProcessingService(
            ServiceBusClient serviceBusClient,
            IServiceProvider serviceProvider,
            ILogger<PdfProcessingService> logger)
        {
            _serviceBusClient = serviceBusClient;
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        /// <summary>
        /// Main execution method that runs when the service starts
        /// </summary>
        /// <param name="stoppingToken">Token to signal when the service should stop</param>
        /// <returns>Task representing the execution</returns>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Create a processor to handle messages from the PDF processing queue
            _processor = _serviceBusClient.CreateProcessor(PDF_QUEUE_NAME);

            // Register event handlers for message processing
            _processor.ProcessMessageAsync += ProcessMessageAsync;
            _processor.ProcessErrorAsync += ProcessErrorAsync;

            // Start processing messages
            await _processor.StartProcessingAsync(stoppingToken);

            try
            {
                // Keep the service running until cancellation is requested
                await Task.Delay(Timeout.Infinite, stoppingToken);
            }
            catch (TaskCanceledException)
            {
                // Expected exception when the service is being stopped
                _logger.LogInformation("PDF Processing Service is shutting down");
            }
            finally
            {
                // Clean up by stopping the processor
                await _processor.StopProcessingAsync(CancellationToken.None);
            }
        }

        /// <summary>
        /// Process a single PDF message from the Service Bus queue
        /// </summary>
        /// <param name="args">Message processing event arguments</param>
        /// <returns>Task representing the message processing</returns>
        private async Task ProcessMessageAsync(ProcessMessageEventArgs args)
        {
            var body = args.Message.Body.ToString();
            _logger.LogInformation("Received PDF processing message: {MessageBody}", body);

            try
            {
                // Deserialize the message
                var message = JsonSerializer.Deserialize<PdfMessage>(body, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                // Create a service scope to resolve scoped services
                using (var scope = _serviceProvider.CreateScope())
                {
                    // Get required services
                    var pdfService = scope.ServiceProvider.GetRequiredService<IPdfService>();
                    var vectorSearchService = scope.ServiceProvider.GetRequiredService<IVectorSearchService>();

                    // Extract text from the PDF
                    var documentChunks = await pdfService.ExtractTextFromPdfAsync(message.BlobName);

                    // Index the document chunks for vector search
                    await vectorSearchService.IndexDocumentChunksAsync(documentChunks);

                    _logger.LogInformation("PDF {BlobName} processed successfully with {ChunkCount} chunks",
                        message.BlobName, documentChunks.Count);
                }

                // Mark the message as complete
                await args.CompleteMessageAsync(args.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing PDF message: {MessageBody}", body);

                // For error handling, you have options:
                // 1. Abandon the message (will be retried):
                await args.AbandonMessageAsync(args.Message);

                // 2. Dead letter the message (won't be retried), use this for permanent failures:
                // await args.DeadLetterMessageAsync(args.Message, ex.Message);
            }
        }

        /// <summary>
        /// Handle errors that occur during message processing
        /// </summary>
        /// <param name="args">Error event arguments</param>
        /// <returns>Task representing the error handling</returns>
        private Task ProcessErrorAsync(ProcessErrorEventArgs args)
        {
            _logger.LogError(args.Exception, "Error in PDF processing service");
            return Task.CompletedTask;
        }

        /// <summary>
        /// Message class for PDF processing
        /// </summary>
        private class PdfMessage
        {
            public string BlobName { get; set; }
            public string FileName { get; set; }
            public DateTime UploadTime { get; set; }
        }
    }
}

