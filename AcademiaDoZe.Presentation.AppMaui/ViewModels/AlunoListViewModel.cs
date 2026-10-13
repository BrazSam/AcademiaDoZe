using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Application.Interfaces;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
namespace AcademiaDoZe.Presentation.AppMaui.ViewModels;

public partial class AlunoListViewModel : BaseViewModel
{
    private readonly IAlunoService _alunoService;
    public ObservableCollection<string> FilterTypes { get; } = ["Nome", "CPF", "Id", "Email"];
    private ObservableCollection<AlunoDto> _alunos = [];
    public ObservableCollection<AlunoDto> Alunos
    {
        get => _alunos;
        set => SetProperty(ref _alunos, value);
    }
    private AlunoDto? _selectedAluno;
    public AlunoDto? SelectedAluno
    {
        get => _selectedAluno;
        set => SetProperty(ref _selectedAluno, value);
    }
    private string _searchText = string.Empty;
    public string SearchText
    {
        get => _searchText;
        set => SetProperty(ref _searchText, value);
    }
    private string _selectedFilterType = "Nome";
    public string SelectedFilterType
    {
        get => _selectedFilterType;
        set => SetProperty(ref _selectedFilterType, value);
    }
    public AlunoListViewModel(IAlunoService alunoService)
    {
        _alunoService = alunoService;
        Title = "Alunos";
    }
    // Métodos com [RelayCommand]
    [RelayCommand]
    private async Task AddAlunoAsync()
    {
        try
        {
            await Shell.Current.GoToAsync("aluno");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Erro", $"Erro ao navegar para tela de cadastro: {ex.Message}", "OK");
        }
    }
    [RelayCommand]
    private async Task EditAlunoAsync(AlunoDto aluno)
    {
        try
        {
            if (aluno == null)
                return;
            await Shell.Current.GoToAsync($"aluno?Id={aluno.Id}");
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
        await LoadAlunosAsync();
    }

    [RelayCommand]
    private async Task SearchAlunosAsync()
    {
        if (IsBusy)
            return;
        try
        {
            IsBusy = true;
            // Limpa a lista atual
            await MainThread.InvokeOnMainThreadAsync(() => { Alunos.Clear(); });
            IEnumerable<AlunoDto> resultados = [];
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            // Busca os alunos de acordo com o filtro selecionado
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                resultados = await _alunoService.ObterTodosAsync(cts.Token) ?? [];
            }
            else if (SelectedFilterType == "Nome")
            {
                var todos = await _alunoService.ObterTodosAsync(cts.Token) ?? [];
                var termo = SearchText.Trim();
                resultados = todos.Where(c => c.Nome.Contains(termo, StringComparison.OrdinalIgnoreCase));
            }
            else if (SelectedFilterType == "CPF")
            {
                var cpfLimpo = new string([.. SearchText.Where(char.IsDigit)]);
                if (cpfLimpo.Length != 11)
                {
                    await Shell.Current.DisplayAlertAsync("Validação", "Para buscar por CPF, informe os 11 dígitos numéricos.", "OK"); return;
                }
                var aluno = await _alunoService.ObterPorCpfAsync(cpfLimpo, cts.Token);
                if (aluno != null)
                    resultados = [aluno];
            }
            else if (SelectedFilterType == "Id")
            {
                if (!int.TryParse(SearchText.Trim(), out int id) || id <= 0)
                {
                    await Shell.Current.DisplayAlertAsync("Validação", "Para buscar por ID, informe um número inteiro positivo válido.", "OK"); return;
                }
                var aluno = await _alunoService.ObterPorIdAsync(id, cts.Token);
                if (aluno != null)
                    resultados = [aluno];
            }
            else if (SelectedFilterType == "Email")
            {
                var emailBusca = SearchText.Trim();
                var aluno = await _alunoService.ObterPorEmailAsync(emailBusca, cts.Token);
                if (aluno != null)
                    resultados = [aluno];
            }
            // Atualiza a coleção na thread principal
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                foreach (var item in resultados)
                {
                    Alunos.Add(item);
                }
                OnPropertyChanged(nameof(Alunos));
            });
        }
        catch (OperationCanceledException)
        {
            await Shell.Current.DisplayAlertAsync("Tempo Esgotado", "A busca de alunos expirou. Verifique a conexão com o banco.", "OK");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Erro", $"Erro ao buscar alunos: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task LoadAlunosAsync()
    {
        if (IsBusy)
            return;
        try
        {
            IsBusy = true;
            // Limpa a lista atual antes de carregar novos dados
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                Alunos.Clear();
                OnPropertyChanged(nameof(Alunos));
            });
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            var alunosList = await _alunoService.ObterTodosAsync(cts.Token);
            if (alunosList != null)
            {
                // Garantir que a atualização da UI aconteça na thread principal
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    foreach (var aluno in alunosList)
                    {
                        Alunos.Add(aluno);
                    }
                    OnPropertyChanged(nameof(Alunos));
                });
            }
        }
        catch (OperationCanceledException)
        {
            await Shell.Current.DisplayAlertAsync("Tempo Esgotado", "O carregamento dos alunos expirou. Verifique a conexão.", "OK");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Erro", $"Erro ao carregar alunos: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
            IsRefreshing = false;
        }
    }

    [RelayCommand]
    private async Task DeleteAlunoAsync(AlunoDto aluno)
    {
        if (aluno == null)
            return;
        bool confirm = await Shell.Current.DisplayAlertAsync(
        "Confirmar Exclusão",
       $"Deseja realmente excluir o aluno {aluno.Nome}?",
        "Sim", "Não");
        if (!confirm)
            return;
        try
        {
            IsBusy = true;
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            bool success = await _alunoService.RemoverAsync(aluno.Id, cts.Token);
            if (success)
            {
                Alunos.Remove(aluno);
                await Shell.Current.DisplayAlertAsync("Sucesso", "Aluno excluído com sucesso!", "OK");
            }
            else
            {
                await Shell.Current.DisplayAlertAsync("Erro", "Não foi possível excluir o aluno.", "OK");
            }
        }
        catch (OperationCanceledException)
        {
            await Shell.Current.DisplayAlertAsync("Tempo Esgotado", "A exclusão do aluno expirou. Verifique a conexão.", "OK");
        }
        catch (Exception ex)
        {
            if (ex.Message.Contains("FOREIGN KEY", StringComparison.OrdinalIgnoreCase) ||
            ex.Message.Contains("constraint", StringComparison.OrdinalIgnoreCase) ||
            ex.Message.Contains("REFERENCE", StringComparison.OrdinalIgnoreCase))
            {
                await Shell.Current.DisplayAlertAsync("Não Permitido",
                "Este aluno não pode ser excluído pois possui registros vinculados no sistema (ex: acessos, turmas ou matrículas).", "OK");
            }
            else
            {
                await Shell.Current.DisplayAlertAsync("Erro", $"Erro ao excluir aluno: {ex.Message}", "OK");
            }
        }
        finally
        {
            IsBusy = false;
        }
    }
}