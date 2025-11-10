using RagMaui.ViewModels;

namespace RagMaui.Views
{
    public partial class ChatPage : ContentPage
    {
        private readonly ChatViewModel _viewModel;

        public ChatPage(ChatViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            // Create a new conversation if there are no messages
            if (_viewModel.Messages.Count == 0)
            {
                _viewModel.NewConversationCommand.Execute(null);
            }
        }
    }
}
