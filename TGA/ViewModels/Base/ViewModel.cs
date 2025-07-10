using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace TGA.ViewModels.Base
{
    internal class ViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string PropertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(PropertyName));
        }

        protected virtual bool Set<T>(ref T field, T value, [CallerMemberName] string PropertName = null)
        {
            if (Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(PropertName);
            return true;
        }
    }
}
