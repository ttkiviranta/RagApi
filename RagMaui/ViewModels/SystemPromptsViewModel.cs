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
    public class SystemPromptsViewModel : ObservableObject
    {
        private readonly IRagApiService _ragApiService;
        private bool _isBusy;
        private string _errorMessage;
        private SystemPrompt? _selectedPrompt;
        private string _promptTitle = string.Empty;
        private string _promptContent = string.Empty;
        private bool _isDefault;

        public ObservableCollection<SystemPrompt> SystemPrompts { get; } = new ObservableCollection<SystemPrompt>();
        public ICommand LoadSystemPromptsCommand { get; }
        public ICommand CreateSystemPromptCommand { get; }
        public ICommand UpdateSystemPromptCommand { get; }
        public ICommand DeleteSystemPromptCommand { get; }
        public ICommand SelectPromptCommand { get; }

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

        public SystemPrompt? SelectedPrompt
        {
            get => _selectedPrompt;
            set
            {
                if (SetProperty(ref _selectedPrompt, value) && value != null)
                {
                    PromptTitle = value.Title;
                    PromptContent = value.Content;
                    IsDefault = value.IsDefault;
                }
            }
        }

        public string PromptTitle
        {
            get => _promptTitle;
            set => SetProperty(ref _promptTitle, value);
        }

        public string PromptContent
        {
            get => _promptContent;
            set => SetProperty(ref _promptContent, value);
        }

        public bool IsDefault
        {
            get => _isDefault;
            set => SetProperty(ref _isDefault, value);
        }

        public SystemPromptsViewModel(IRagApiService ragApiService)
        {
            _ragApiService = ragApiService;
            LoadSystemPromptsCommand = new AsyncRelayCommand(LoadSystemPromptsAsync);
            CreateSystemPromptCommand = new AsyncRelayCommand(CreateSystemPromptAsync);
            UpdateSystemPromptCommand = new AsyncRelayCommand<string>(UpdateSystemPromptAsync);
            DeleteSystemPromptCommand = new AsyncRelayCommand<string>(DeleteSystemPromptAsync);
            SelectPromptCommand = new RelayCommand<SystemPrompt>(OnPromptSelected);
        }

        private async Task LoadSystemPromptsAsync()
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;
                ErrorMessage = string.Empty;
                SystemPrompts.Clear();

                var prompts = await _ragApiService.GetSystemPromptsAsync();

                foreach (var prompt in prompts)
                {
                    SystemPrompts.Add(prompt);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Virhe järjestelmäkehotuksien hakemisessa: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void OnPromptSelected(SystemPrompt? prompt)
        {
            SelectedPrompt = prompt;
        }

        private async Task CreateSystemPromptAsync()
        {
            if (IsBusy || string.IsNullOrWhiteSpace(PromptTitle) || string.IsNullOrWhiteSpace(PromptContent))
                return;

            try
            {
                IsBusy = true;
                ErrorMessage = string.Empty;

                var newPrompt = new SystemPrompt
                {
                    Title = PromptTitle,
                    Content = PromptContent,
                    IsDefault = IsDefault
                };

                var createdPrompt = await _ragApiService.CreateSystemPromptAsync(newPrompt);

                SystemPrompts.Add(createdPrompt);

                // Tyhjennä kentät uutta varten
                PromptTitle = string.Empty;
                PromptContent = string.Empty;
                IsDefault = false;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Virhe järjestelmäkehotuksen luomisessa: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task UpdateSystemPromptAsync(string id)
        {
            if (IsBusy || string.IsNullOrWhiteSpace(id) ||
                string.IsNullOrWhiteSpace(PromptTitle) ||
                string.IsNullOrWhiteSpace(PromptContent))
                return;

            try
            {
                IsBusy = true;
                ErrorMessage = string.Empty;

                var updatedPrompt = new SystemPrompt
                {
                    Id = id,
                    Title = PromptTitle,
                    Content = PromptContent,
                    IsDefault = IsDefault
                };

                var result = await _ragApiService.UpdateSystemPromptAsync(id, updatedPrompt);

                // Päivitä listaus
                await LoadSystemPromptsAsync();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Virhe järjestelmäkehotuksen päivittämisessä: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task DeleteSystemPromptAsync(string id)
        {
            if (IsBusy || string.IsNullOrWhiteSpace(id)) return;

            try
            {
                IsBusy = true;
                ErrorMessage = string.Empty;

                await _ragApiService.DeleteSystemPromptAsync(id);

                // Poista listasta
                var promptToRemove = SystemPrompts.FirstOrDefault(p => p.Id == id);
                if (promptToRemove != null)
                {
                    SystemPrompts.Remove(promptToRemove);
                }

                // Tyhjennä valinta
                if (SelectedPrompt?.Id == id)
                {
                    SelectedPrompt = null;
                    PromptTitle = string.Empty;
                    PromptContent = string.Empty;
                    IsDefault = false;
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Virhe järjestelmäkehotuksen poistamisessa: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}