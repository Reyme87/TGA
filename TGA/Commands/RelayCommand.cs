using TGA.Commands.Base;

namespace TGA.Commands
{
    internal class RelayCommand : Command
    {
        private Action<object> _execute;
        private Func<object, bool> _canExecute;

        public RelayCommand(Action<object> Execute, Func<object, bool> CanExecute)
        {
            _execute = Execute ?? throw new ArgumentNullException(nameof(Execute));
            _canExecute = CanExecute;
        }

        public override bool CanExecute(object? parameter) => _canExecute?.Invoke(parameter) ?? true;

        public override void Execute(object? parameter) => _execute(parameter);
    }
}
