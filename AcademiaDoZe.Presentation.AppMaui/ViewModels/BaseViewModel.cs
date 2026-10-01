using CommunityToolkit.Mvvm.ComponentModel;

namespace AcademiaDoZe.Presentation.AppMaui.ViewModels;

public partial class BaseViewModel : ObservableObject
{
    private bool isBusy; // Indica se uma operação está em andamento, útil para mostrar indicadores de carregamento na UI.

    public bool IsBusy
    {
        get => isBusy;
        set => SetProperty(ref isBusy, value);
    }

    private string title = string.Empty; // Título da ViewModel, pode ser usado para definir o título da página na UI.

    public string Title
    {
        get => title;
        set => SetProperty(ref title, value);
    }

    private bool isRefreshing; // Indica se a ViewModel está em estado de atualização, útil para pull-to-refresh na UI.

    public bool IsRefreshing
    {
        get => isRefreshing;
        set => SetProperty(ref isRefreshing, value);
    }
}