using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SuiviPortefolio.Consultations.ConsultationsViewModel;

public class RelayCommand : ICommand
{
    private readonly Func<object, Task> _executeAsync;
    private readonly Func<object, bool> _canExecute;

    public RelayCommand(Func<object, Task> executeAsync, Func<object, bool> canExecute = null)
    {
        _executeAsync = executeAsync ?? throw new ArgumentNullException(nameof(executeAsync));
        _canExecute = canExecute ?? (_ => true);
    }

    public RelayCommand(Action<object> execute, Func<object, bool> canExecute = null)
        : this(param => { execute(param); return Task.CompletedTask; }, canExecute)
    {
    }

    public bool CanExecute(object parameter) => _canExecute(parameter);

    public async void Execute(object parameter)
    {
        try
        {
            await _executeAsync(parameter).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            // Ne pas remonter d'exception non gérée depuis un async void si l'app le souhaite
            System.Diagnostics.Debug.WriteLine(ex);
            throw;
        }
    }

    public event EventHandler CanExecuteChanged;

    public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
}