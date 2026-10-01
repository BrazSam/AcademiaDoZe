using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
namespace AcademiaDoZe.Presentation.AppMaui.ViewModels;

public partial class LogradouroListViewModel : BaseViewModel
{
    public ObservableCollection<string> FilterTypes { get; } = ["Cidade", "Id", "Cep"];
    private string _searchText = string.Empty;
    public string SearchText
    {
        get => _searchText;
        set => SetProperty(ref _searchText, value);
    }
    private string _selectedFilterType = "Cidade"; // Cidade, Id, Cep
    public string SelectedFilterType
    {
        get => _selectedFilterType;
        set => SetProperty(ref _selectedFilterType, value);
    }
    // inicialmente só vamos incluir aqui o comando para navegar para a tela de cadastro
    [RelayCommand]
    private async Task AddLogradouroAsync()
    {
        try
        {
            // GoToAsync é usado para navegar entre páginas no MAUI Shell.
            // logradouro é o nome da rota que será registrada no AppShell.xaml.cs
            await Shell.Current.GoToAsync("logradouro");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Erro", $"Erro ao navegar para tela de cadastro: {ex.Message}", "OK");
        }
    }
}