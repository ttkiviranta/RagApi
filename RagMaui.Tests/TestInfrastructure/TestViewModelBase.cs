using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace RagMaui.Tests.TestInfrastructure
{
    /// <summary>
    /// Simple ViewModelBase implementation for testing
  /// </summary>
    public class TestViewModelBase : INotifyPropertyChanged
    {
    public event PropertyChangedEventHandler? PropertyChanged;

    protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string? propertyName = null)
        {
  if (Equals(storage, value))
        return false;

            storage = value;
      OnPropertyChanged(propertyName);
         return true;
     }

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
{
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}