using RagMaui.Models;
using RagMaui.Services;
using System.Collections.ObjectModel;

namespace RagMaui.ViewModels
{
    public class ApplicationsViewModel : BaseViewModel
    {
        private readonly IRagApiService _apiService;

        public ObservableCollection<JobApplication> Applications { get; set; } = new();

        public ApplicationsViewModel(IRagApiService apiService)
        {
            _apiService = apiService;
            Title = "Applications";
        }

        public async Task LoadApplicationsAsync()
        {
            if (IsBusy)
                return;

            IsBusy = true;

            try
            {
                var applications = await _apiService.GetApplicationsAsync();

                Applications.Clear();
                foreach (var application in applications)
                {
                    Applications.Add(application);
                }
            }
            catch (Exception ex)
            {
                // Handle or log the exception
                Console.WriteLine($"Error loading applications: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
