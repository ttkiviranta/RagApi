using RagMaui.Models;
using RagMaui.Services;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;

namespace RagMaui.ViewModels
{
    /// <summary>
    /// ViewModel for the chat interface
    /// </summary>
    public class ChatViewModel : ViewModelBase
    {
        private readonly IRagApiService _ragApiService;
        private string _userMessage;
        private string _conversationId;
        private string _conversationTitle;
        private bool _isBusy;

        /// <summary>
        /// Collection of chat messages
        /// </summary>
        public ObservableCollection<MessageViewModel> Messages { get; } = new ObservableCollection<MessageViewModel>();

        /// <summary>
        /// The user's message text
        /// </summary>
        public string UserMessage
        {
            get => _userMessage;
            set => SetProperty(ref _userMessage, value);
        }

        /// <summary>
        /// Title of the current conversation
        /// </summary>
        public string ConversationTitle
        {
            get => _conversationTitle;
            set => SetProperty(ref _conversationTitle, value);
        }

        /// <summary>
        /// Indicates if an operation is in progress
        /// </summary>
        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        /// <summary>
        /// Command to send a message
        /// </summary>
        public ICommand SendMessageCommand { get; }

        /// <summary>
        /// Command to create a new conversation
        /// </summary>
        public ICommand NewConversationCommand { get; }

        /// <summary>
        /// Command to load a conversation
        /// </summary>
        public ICommand LoadConversationCommand { get; }

        /// <summary>
        /// Constructor for the chat view model
        /// </summary>
        public ChatViewModel(IRagApiService ragApiService)
        {
            _ragApiService = ragApiService;
            SendMessageCommand = new AsyncRelayCommand(SendMessageAsync, () => !string.IsNullOrWhiteSpace(UserMessage) && !IsBusy);
            NewConversationCommand = new AsyncRelayCommand(CreateNewConversationAsync);
            LoadConversationCommand = new AsyncRelayCommand<string>(LoadConversationAsync);

            // Default title for a new conversation
            ConversationTitle = "Uusi keskustelu " + DateTime.Now.ToString("d.M.yyyy HH:mm");
        }

        /// <summary>
        /// Sends a message to the RAG API
        /// </summary>
        private async Task SendMessageAsync()
        {
            if (IsBusy || string.IsNullOrWhiteSpace(UserMessage))
                return;

            try
            {
                IsBusy = true;

                // Add user message to the conversation
                var userMessageText = UserMessage.Trim();
                Messages.Add(new MessageViewModel
                {
                    Content = userMessageText,
                    IsFromUser = true,
                    Timestamp = DateTime.Now
                });
                UserMessage = string.Empty;

                // Create conversation if it doesn't exist
                if (string.IsNullOrEmpty(_conversationId))
                {
                    var conversation = await _ragApiService.CreateConversationAsync(ConversationTitle);
                    _conversationId = conversation.Id;
                }

                // Add temporary loading message
                var tempIndex = Messages.Count;
                Messages.Add(new MessageViewModel
                {
                    Content = "Ladataan vastausta...",
                    IsFromUser = false,
                    IsLoading = true,
                    Timestamp = DateTime.Now
                });

                // Get response
                RagResponse response;
                if (!string.IsNullOrEmpty(_conversationId))
                {
                    response = await _ragApiService.QueryInConversationAsync(_conversationId, userMessageText);
                }
                else
                {
                    response = await _ragApiService.QueryAsync(userMessageText);
                }

                // Remove temporary message
                Messages.RemoveAt(tempIndex);

                // Add response
                var messageViewModel = new MessageViewModel
                {
                    Content = response.Answer,
                    IsFromUser = false,
                    Timestamp = DateTime.Now,
                    Sources = new ObservableCollection<SearchResult>(response.SourceResults)
                };
                Messages.Add(messageViewModel);
            }
            catch (Exception ex)
            {
                Messages.Add(new MessageViewModel
                {
                    Content = $"Virhe: {ex.Message}",
                    IsFromUser = false,
                    IsError = true,
                    Timestamp = DateTime.Now
                });
            }
            finally
            {
                IsBusy = false;
            }
        }

        /// <summary>
        /// Creates a new conversation
        /// </summary>
        private async Task CreateNewConversationAsync()
        {
            if (IsBusy)
                return;

            try
            {
                IsBusy = true;

                // Clear existing messages
                Messages.Clear();

                // Create new conversation
                ConversationTitle = "Uusi keskustelu " + DateTime.Now.ToString("d.M.yyyy HH:mm");
                var conversation = await _ragApiService.CreateConversationAsync(ConversationTitle);
                _conversationId = conversation.Id;

                // Add welcome message
                Messages.Add(new MessageViewModel
                {
                    Content = "Tervetuloa uuteen keskusteluun! Miten voin auttaa sinua tänään?",
                    IsFromUser = false,
                    Timestamp = DateTime.Now
                });
            }
            catch (Exception ex)
            {
                Messages.Add(new MessageViewModel
                {
                    Content = $"Virhe keskustelun luomisessa: {ex.Message}",
                    IsFromUser = false,
                    IsError = true,
                    Timestamp = DateTime.Now
                });
            }
            finally
            {
                IsBusy = false;
            }
        }

        /// <summary>
        /// Loads an existing conversation
        /// </summary>
        private async Task LoadConversationAsync(string conversationId)
        {
            if (IsBusy || string.IsNullOrEmpty(conversationId))
                return;

            try
            {
                IsBusy = true;
                Messages.Clear();

                var conversation = await _ragApiService.GetConversationAsync(conversationId);
                _conversationId = conversation.Id;
                ConversationTitle = conversation.Title;

                foreach (var message in conversation.Messages)
                {
                    Messages.Add(new MessageViewModel
                    {
                        Content = message.Content,
                        IsFromUser = message.Type == MessageType.User,
                        Timestamp = message.CreatedAt,
                        Sources = message.SearchResults != null
                            ? new ObservableCollection<SearchResult>(message.SearchResults)
                            : new ObservableCollection<SearchResult>()
                    });
                }
            }
            catch (Exception ex)
            {
                Messages.Add(new MessageViewModel
                {
                    Content = $"Virhe keskustelun lataamisessa: {ex.Message}",
                    IsFromUser = false,
                    IsError = true,
                    Timestamp = DateTime.Now
                });
            }
            finally
            {
                IsBusy = false;
            }
        }

        /// <summary>
        /// Sets the current conversation
        /// </summary>
        public void SetConversation(Conversation conversation)
        {
            if (conversation != null)
            {
                _conversationId = conversation.Id;
                ConversationTitle = conversation.Title;
                Task.Run(() => LoadConversationAsync(conversation.Id));
            }
        }
    }

    /// <summary>
    /// ViewModel for a chat message
    /// </summary>
    public class MessageViewModel : ViewModelBase
    {
        private string _content;
        private bool _isFromUser;
        private bool _isError;
        private bool _isLoading;
        private DateTime _timestamp;
        private ObservableCollection<SearchResult> _sources = new ObservableCollection<SearchResult>();

        /// <summary>
        /// The message text
        /// </summary>
        public string Content
        {
            get => _content;
            set => SetProperty(ref _content, value);
        }

        /// <summary>
        /// Whether the message is from the user (true) or system (false)
        /// </summary>
        public bool IsFromUser
        {
            get => _isFromUser;
            set => SetProperty(ref _isFromUser, value);
        }

        /// <summary>
        /// Whether the message represents an error
        /// </summary>
        public bool IsError
        {
            get => _isError;
            set => SetProperty(ref _isError, value);
        }

        /// <summary>
        /// Whether the message is a loading indicator
        /// </summary>
        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        /// <summary>
        /// When the message was created
        /// </summary>
        public DateTime Timestamp
        {
            get => _timestamp;
            set => SetProperty(ref _timestamp, value);
        }

        /// <summary>
        /// Source documents for this message
        /// </summary>
        public ObservableCollection<SearchResult> Sources
        {
            get => _sources;
            set => SetProperty(ref _sources, value);
        }

        /// <summary>
        /// Indicates if this message has sources
        /// </summary>
        public bool HasSources => Sources.Count > 0;
    }
}
