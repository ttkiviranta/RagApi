using Azure.Messaging.ServiceBus;
using RagApi.Interfaces;
using System.Text.Json;
using System.Threading.Tasks;

namespace RagApi.Services
{
    /// <summary>
    /// Service for sending messages to Azure Service Bus
    /// </summary>
    public class MessageBusService : IMessageBusService
    {
        private readonly ServiceBusClient _serviceBusClient;

        /// <summary>
        /// Constructor for MessageBusService
        /// </summary>
        /// <param name="serviceBusClient">Azure Service Bus client</param>
        public MessageBusService(ServiceBusClient serviceBusClient)
        {
            _serviceBusClient = serviceBusClient;
        }

        /// <summary>
        /// Send a message to a Service Bus queue or topic
        /// </summary>
        /// <typeparam name="T">Type of the message content</typeparam>
        /// <param name="queueOrTopic">Name of the queue or topic</param>
        /// <param name="message">Message object to send</param>
        /// <returns>Task representing the send operation</returns>
        public async Task SendMessageAsync<T>(string queueOrTopic, T message)
        {
            // Create a sender for the specified queue or topic
            var sender = _serviceBusClient.CreateSender(queueOrTopic);

            // Serialize the message to JSON
            var messageContent = JsonSerializer.Serialize(message);

            // Create a Service Bus message
            var serviceBusMessage = new ServiceBusMessage(messageContent);

            // Send the message to Service Bus
            await sender.SendMessageAsync(serviceBusMessage);
        }
    }
}


