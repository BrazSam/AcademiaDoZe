using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Application.Interfaces;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
namespace AcademiaDoZe.Presentation.AppMaui.ViewModels;

public partial class LogradouroListViewModel : BaseViewModel
{
    private readonly ILogradouroService _logradouroService;
    public ObservableCollection<string> FilterTypes { get; } = ["Cidade", "Id", "Cep"];
    private ObservableCollection<LogradouroDto> _logradouros = [];
    public ObservableCollection<LogradouroDto> Logradouros { get => _logradouros; set => SetProperty(ref _logradouros, value); }
    private LogradouroDto? _selectedLogradouro;
    public LogradouroDto? SelectedLogradouro { get => _selectedLogradouro; set => SetProperty(ref _selectedLogradouro, value); }
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
    public LogradouroListViewModel(ILogradouroService logradouroService)
    {
        _logradouroService = logradouroService;
        Title = "Logradouros";
    }
    // Incluir os Métodos com [RelayCommand]

    [RelayCommand]
    private async Task AddLogradouroAsync()
    {
        try
        {
            // GoToAsync é usado para navegar entre páginas no MAUI Shell.
            // logradouro é o nome da rota registrada no AppShell.xaml.cs
            await Shell.Current.GoToAsync("logradouro");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Erro", $"Erro ao navegar para tela de cadastro: {ex.Message}", "OK");
        }
    }
    [RelayCommand]
    private async Task EditLogradouroAsync(LogradouroDto logradouro)
    {
        try
        {
            if (logradouro == null)
                return;
            await Shell.Current.GoToAsync($"logradouro?Id={logradouro.Id}");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Erro", $"Erro ao navegar para tela de edição: {ex.Message}", "OK");
        }
    }
    [RelayCommand]
    private async Task RefreshAsync()
    {
        IsRefreshing = true;
        await LoadLogradourosAsync();
    }

    [RelayCommand]
    private async Task SearchLogradourosAsync()
    {
        if (IsBusy)
            return;
        try
        {
            IsBusy = true;
            // Limpa a lista atual
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                Logradouros.Clear();
            });
            IEnumerable<LogradouroDto> resultados = [];
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            // Busca os logradouros de acordo com o filtro
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                resultados = await _logradouroService.ObterTodosAsync(cts.Token) ?? [];
            }
            else if (SelectedFilterType == "Cidade")
            {
                resultados = await _logradouroService.ObterPorCidadeAsync(SearchText.Trim(), cts.Token) ?? [];
            }
            else if (SelectedFilterType == "Id")
            {
                if (!int.TryParse(SearchText.Trim(), out int id) || id <= 0)
                {
                    await Shell.Current.DisplayAlertAsync("Validação", "Para buscar por ID, informe um número inteiro positivo válido.", "OK");
                    return;
                }
                var logradouro = await _logradouroService.ObterPorIdAsync(id, cts.Token);
                if (logradouro != null)
                    resultados = [logradouro];
            }
            else if (SelectedFilterType == "Cep")
            {
                var cepLimpo = new string([.. SearchText.Where(char.IsDigit)]);
                if (cepLimpo.Length != 8)
                {
                    await Shell.Current.DisplayAlertAsync("Validação", "Para buscar por CEP, informe os 8 dígitos numéricos.", "OK");
                    return;
                }
                var logradouro = await _logradouroService.ObterPorCepAsync(cepLimpo, cts.Token);
                if (logradouro != null)
                    resultados = [logradouro];
            }
            // Atualiza a coleção na thread principal
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                foreach (var item in resultados)
                {
                    Logradouros.Add(item);
                }
                OnPropertyChanged(nameof(Logradouros));
            });
        }
        catch (OperationCanceledException)
        {
            await Shell.Current.DisplayAlertAsync("Tempo Esgotado", "A busca expirou. Verifique a conexão com o banco.", "OK");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Erro", $"Erro ao buscar logradouros: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task LoadLogradourosAsync()
    {
        if (IsBusy)
            return;
        try
        {
            IsBusy = true;
            // Limpa a lista atual antes de carregar novos dados
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                Logradouros.Clear();
                OnPropertyChanged(nameof(Logradouros));
            });
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            var logradourosList = await _logradouroService.ObterTodosAsync(cts.Token);
            if (logradourosList != null)
            {
                // Garantir que a atualização da UI aconteça na thread principal
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    foreach (var logradouro in logradourosList)
                    {
                        Logradouros.Add(logradouro);
                    }
                    OnPropertyChanged(nameof(Logradouros));
                });
            }
        }
        catch (OperationCanceledException)
        {
            await Shell.Current.DisplayAlertAsync("Tempo Esgotado", "O carregamento dos logradouros expirou. Verifique a conexão.", "OK");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Erro", $"Erro ao carregar logradouros: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
            IsRefreshing = false;
        }
    }

    [RelayCommand]
    private async Task DeleteLogradouroAsync(LogradouroDto logradouro)
    {
        if (logradouro == null)
            return;
        bool confirm = await Shell.Current.DisplayAlertAsync(
        "Confirmar Exclusão",
       $"Deseja realmente excluir o logradouro {logradouro.Nome}?",
        "Sim", "Não");
        if (!confirm)
            return;
        try
        {
            IsBusy = true;
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            bool success = await _logradouroService.RemoverAsync(logradouro.Id, cts.Token);
            if (success)
            {
                Logradouros.Remove(logradouro);
                await Shell.Current.DisplayAlertAsync("Sucesso", "Logradouro excluído com sucesso!", "OK");
            }
            else
            {
                await Shell.Current.DisplayAlertAsync("Erro", "Não foi possível excluir o logradouro.", "OK");
            }
        }
        catch (OperationCanceledException)
        {
            await Shell.Current.DisplayAlertAsync("Tempo Esgotado", "A exclusão do logradouro expirou. Verifique a conexão.", "OK");
        }
        catch (Exception ex)
        {
            if (ex.Message.Contains("FOREIGN KEY", StringComparison.OrdinalIgnoreCase) ||
            ex.Message.Contains("constraint", StringComparison.OrdinalIgnoreCase) ||
            ex.Message.Contains("REFERENCE", StringComparison.OrdinalIgnoreCase))
            {
                await Shell.Current.DisplayAlertAsync("Não Permitido",
                "Este logradouro não pode ser excluído pois está vinculado a alunos ou colaboradores cadastrados.", "OK");
            }
            else
            {
                await Shell.Current.DisplayAlertAsync("Erro", $"Erro ao excluir logradouro: {ex.Message}", "OK");
            }
        }
        finally
        {
            IsBusy = false;
        }
    }
}