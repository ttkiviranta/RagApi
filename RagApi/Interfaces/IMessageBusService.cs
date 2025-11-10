using System.Threading.Tasks;

namespace RagApi.Interfaces
{
    /// <summary>
    /// Interface for Service Bus messaging operations
    /// </summary>
    public interface IMessageBusService
    {
        /// <summary>
        /// Send a message to a Service Bus queue or topic
        /// </summary>
        /// <typeparam name="T">Type of the message content</typeparam>
        /// <param name="queueOrTopic">Name of the queue or topic</param>
        /// <param name="message">Message object to send</param>
        /// <returns>Task representing the send operation</returns>
        Task SendMessageAsync<T>(string queueOrTopic, T message);
    }
}


