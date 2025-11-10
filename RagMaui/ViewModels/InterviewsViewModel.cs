using RagMaui.Models;
using RagMaui.Services;
using System.Collections.ObjectModel;

namespace RagMaui.ViewModels
{
    public class InterviewsViewModel : BaseViewModel
    {
        private readonly IRagApiService _apiService;

        public ObservableCollection<Interview> Interviews { get; set; } = new();

        public InterviewsViewModel(IRagApiService apiService)
        {
            _apiService = apiService;
            Title = "Interviews";
        }

        public async Task LoadInterviewsAsync()
        {
            if (IsBusy)
                return;

            IsBusy = true;

            try
            {
                var interviews = await _apiService.GetInterviewsAsync();

                Interviews.Clear();
                foreach (var interview in interviews)
                {
                    Interviews.Add(interview);
                }
            }
            catch (Exception ex)
            {
                // Handle or log the exception
                Console.WriteLine($"Error loading interviews: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}