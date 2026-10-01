using CommunityToolkit.Mvvm.Input;
using Phoneword.Models;

namespace Phoneword.PageModels
{
    public interface IProjectTaskPageModel
    {
        IAsyncRelayCommand<ProjectTask> NavigateToTaskCommand { get; }
        bool IsBusy { get; }
    }
}