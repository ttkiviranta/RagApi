using FluentAssertions;
using RagMaui.Tests.TestInfrastructure;
using Xunit;

namespace RagMaui.Tests.ViewModels
{
    /// <summary>
    /// Unit tests for ViewModelBase functionality
    /// </summary>
    public class ViewModelBaseTests
    {
        [Fact]
        public void SetProperty_WithDifferentValue_ShouldReturnTrueAndRaisePropertyChanged()
        {
            // Arrange
            var viewModel = new TestViewModel();
            var propertyChangedRaised = false;
            string? raisedPropertyName = null;
            viewModel.PropertyChanged += (sender, args) =>
            {
                raisedPropertyName = args.PropertyName;
                propertyChangedRaised = true; // Any property change counts
            };

            // Act
            var result = viewModel.SetTestProperty("New Value");

            // Assert
            result.Should().BeTrue();
            propertyChangedRaised.Should().BeTrue($"PropertyChanged should be raised, but was not. Property name was: {raisedPropertyName}");
            viewModel.TestProperty.Should().Be("New Value");
        }

        [Fact]
        public void SetProperty_WithSameValue_ShouldReturnFalseAndNotRaisePropertyChanged()
        {
            // Arrange
            var viewModel = new TestViewModel();
            viewModel.SetTestProperty("Initial Value");
            var propertyChangedRaised = false;
            viewModel.PropertyChanged += (sender, args) =>
            {
                propertyChangedRaised = true;
            };

            // Act
            var result = viewModel.SetTestProperty("Initial Value");

            // Assert
            result.Should().BeFalse();
            propertyChangedRaised.Should().BeFalse();
        }

        [Fact]
        public void OnPropertyChanged_ShouldRaisePropertyChangedEvent()
        {
            // Arrange
            var viewModel = new TestViewModel();
            var propertyChangedRaised = false;
            string? raisedPropertyName = null;
            viewModel.PropertyChanged += (sender, args) =>
            {
                propertyChangedRaised = true;
                raisedPropertyName = args.PropertyName;
            };

            // Act
            viewModel.RaisePropertyChanged("TestProperty");

            // Assert
            propertyChangedRaised.Should().BeTrue();
            raisedPropertyName.Should().Be("TestProperty");
        }
    }

    /// <summary>
    /// Test implementation of ViewModelBase for testing purposes
    /// </summary>
    public class TestViewModel : TestViewModelBase
    {
        private string _testProperty = "Initial Value";

        public string TestProperty
        {
            get => _testProperty;
            private set => SetProperty(ref _testProperty, value);
        }

        public bool SetTestProperty(string value)
        {
            return SetProperty(ref _testProperty, value);
        }

        public void RaisePropertyChanged(string propertyName)
        {
            OnPropertyChanged(propertyName);
        }
    }
}