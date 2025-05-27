using RagMaui.ViewModels;

namespace RagMaui.Views
{
    public partial class ConversationListPage : ContentPage
    {
        private readonly ConversationListViewModel _viewModel;

        public ConversationListPage(ConversationListViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _viewModel.LoadConversationsCommand.Execute(null);
        }
    }
}
