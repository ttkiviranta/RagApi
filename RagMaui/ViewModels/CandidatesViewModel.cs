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
    public class CandidatesViewModel : ObservableObject
    {
        private readonly IRagApiService _ragApiService;
        private bool _isBusy;
        private string _errorMessage;
        private Candidate? _selectedCandidate;

        public ObservableCollection<Candidate> Candidates { get; } = new ObservableCollection<Candidate>();
        public ICommand LoadCandidatesCommand { get; }
        public ICommand SelectCandidateCommand { get; }
        public ICommand MatchCandidateWithJobsCommand { get; }

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

        public Candidate? SelectedCandidate
        {
            get => _selectedCandidate;
            set => SetProperty(ref _selectedCandidate, value);
        }

        public CandidatesViewModel(IRagApiService ragApiService)
        {
            _ragApiService = ragApiService;
            LoadCandidatesCommand = new AsyncRelayCommand(LoadCandidatesAsync);
            SelectCandidateCommand = new RelayCommand<Candidate>(OnCandidateSelected);
            MatchCandidateWithJobsCommand = new AsyncRelayCommand<string>(MatchCandidateWithJobsAsync);
        }

        private async Task LoadCandidatesAsync()
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;
                ErrorMessage = string.Empty;
                Candidates.Clear();

                var candidates = await _ragApiService.GetCandidatesAsync();

                foreach (var candidate in candidates)
                {
                    Candidates.Add(candidate);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Virhe kandidaattien hakemisessa: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void OnCandidateSelected(Candidate? candidate)
        {
            SelectedCandidate = candidate;
            // Tässä voisi esimerkiksi navigoida kandidaatin tarkempiin tietoihin
        }

        private async Task MatchCandidateWithJobsAsync(string candidateId)
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;
                ErrorMessage = string.Empty;

                var matchResults = await _ragApiService.MatchCandidateWithJobsAsync(candidateId, 10);

                // Tässä voisi näyttää tulokset käyttöliittymässä
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Virhe kandidaatin ja työpaikkojen yhteensovittamisessa: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
