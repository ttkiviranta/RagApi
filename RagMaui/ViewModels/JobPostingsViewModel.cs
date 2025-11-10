using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RagMaui.Models;
using RagMaui.Services;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;

namespace RagMaui.ViewModels
{
    public class JobPostingsViewModel : ObservableObject
    {
        private readonly IRagApiService _ragApiService;
        private bool _isBusy;
        private string _errorMessage;
        private JobPosting? _selectedJobPosting;

        public ObservableCollection<JobPosting> JobPostings { get; } = new ObservableCollection<JobPosting>();
        public ICommand LoadJobPostingsCommand { get; }
        public ICommand SelectJobPostingCommand { get; }

        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        public JobPosting? SelectedJobPosting
        {
            get => _selectedJobPosting;
            set => SetProperty(ref _selectedJobPosting, value);
        }

        public JobPostingsViewModel(IRagApiService ragApiService)
        {
            _ragApiService = ragApiService;
            LoadJobPostingsCommand = new AsyncRelayCommand(LoadJobPostingsAsync);
            SelectJobPostingCommand = new RelayCommand<JobPosting>(OnJobPostingSelected);
        }

        private async Task LoadJobPostingsAsync()
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;
                ErrorMessage = string.Empty;
                JobPostings.Clear();

                var jobPostings = await _ragApiService.GetJobPostingsAsync();

                foreach (var jobPosting in jobPostings)
                {
                    JobPostings.Add(jobPosting);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Virhe työpaikkojen hakemisessa: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void OnJobPostingSelected(JobPosting? jobPosting)
        {
            SelectedJobPosting = jobPosting;
            // Tässä voisi esimerkiksi navigoida työpaikan tarkempiin tietoihin
        }
    }
}
