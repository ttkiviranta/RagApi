using RagMaui.Models;
using RagMaui.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace RagMaui.ViewModels
{
    /// <summary>
    /// ViewModel for the conversation list
    /// </summary>
    public class ConversationListViewModel : ViewModelBase
    {
        private readonly IRagApiService _ragApiService;
        private bool _isLoading;
        private Conversation _selectedConversation;

        /// <summary>
        /// Collection of conversations
        /// </summary>
        public ObservableCollection<Conversation> Conversations { get; } = new ObservableCollection<Conversation>();

        /// <summary>
        /// Indicates if data is currently being loaded
        /// </summary>
        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        /// <summary>
        /// Currently selected conversation
        /// </summary>
        public Conversation SelectedConversation
        {
            get => _selectedConversation;
            set => SetProperty(ref _selectedConversation, value);
        }

        /// <summary>
        /// Command to load all conversations
        /// </summary>
        public ICommand LoadConversationsCommand { get; }

        /// <summary>
        /// Command to create a new conversation
        /// </summary>
        public ICommand CreateConversationCommand { get; }

        /// <summary>
        /// Command to refresh the conversation list
        /// </summary>
        public ICommand RefreshCommand { get; }

        /// <summary>
        /// Event fired when a conversation is selected
        /// </summary>
        public event Action<Conversation> ConversationSelected;

        /// <summary>
        /// Constructor for the ConversationListViewModel
        /// </summary>
        /// <param name="ragApiService">Service for RAG API operations</param>
        public ConversationListViewModel(IRagApiService ragApiService)
        {
            _ragApiService = ragApiService;

            LoadConversationsCommand = new AsyncRelayCommand(LoadConversationsAsync);
            CreateConversationCommand = new AsyncRelayCommand(CreateConversationAsync);
            RefreshCommand = new AsyncRelayCommand(RefreshAsync);
        }

        /// <summary>
        /// Loads all conversations from the API
        /// </summary>
        private async Task LoadConversationsAsync()
        {
            if (IsLoading)
                return;

            try
            {
                IsLoading = true;
                Conversations.Clear();

                var conversations = await _ragApiService.GetConversationsAsync();

                // Order by most recent first
                foreach (var conversation in conversations.OrderByDescending(c => c.UpdatedAt))
                {
                    Conversations.Add(conversation);
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert(
                    "Error",
                    $"Failed to load conversations: {ex.Message}",
                    "OK");
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Creates a new conversation
        /// </summary>
        private async Task CreateConversationAsync()
        {
            if (IsLoading)
                return;

            try
            {
                IsLoading = true;

                string title = "New Conversation " + DateTime.Now.ToString("MM/dd/yyyy HH:mm");

                var conversation = await _ragApiService.CreateConversationAsync(title);
                Conversations.Insert(0, conversation);

                SelectedConversation = conversation;
                OnConversationSelected(conversation);
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert(
                    "Error",
                    $"Failed to create conversation: {ex.Message}",
                    "OK");
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Refreshes the conversation list
        /// </summary>
        private async Task RefreshAsync()
        {
            await LoadConversationsAsync();
        }

        /// <summary>
        /// Handles item selection in the UI
        /// </summary>
        public void OnItemSelected()
        {
            if (SelectedConversation != null)
            {
                OnConversationSelected(SelectedConversation);
            }
        }

        /// <summary>
        /// Raises the ConversationSelected event
        /// </summary>
        /// <param name="conversation">Selected conversation</param>
        private void OnConversationSelected(Conversation conversation)
        {
            ConversationSelected?.Invoke(conversation);
        }
    }
}