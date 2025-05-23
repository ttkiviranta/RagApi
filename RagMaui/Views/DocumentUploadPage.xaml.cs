using RagMaui.ViewModels;

namespace RagMaui.Views
{
    public partial class DocumentUploadPage : ContentPage
    {
        public DocumentUploadPage(DocumentUploadViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }
    }
}