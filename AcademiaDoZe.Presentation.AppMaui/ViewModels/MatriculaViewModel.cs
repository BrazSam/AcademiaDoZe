using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Application.Enums;
using AcademiaDoZe.Application.Interfaces;  
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.ComponentModel;
namespace AcademiaDoZe.Presentation.AppMaui.ViewModels;

[QueryProperty(nameof(MatriculaId), "Id")]
public partial class MatriculaViewModel : BaseViewModel
{
    public IEnumerable<AppMatriculaPlano> PlanoTipos { get; } = Enum.GetValues<AppMatriculaPlano>().Cast<AppMatriculaPlano>();
    // Itens para múltipla seleção (CheckBoxes) - restrições médicas - flags
    public ObservableCollection<RestricaoSelectable> RestricoesMulti { get; } = [];
    // flag de suspensão
    private bool _suspendSelectionSync = false;
    private readonly IMatriculaService _matriculaService;
    private readonly IAlunoService _alunoService;
    private MatriculaDto _matricula = new()
    {
        AlunoMatricula = new AlunoDto()
        {
            Nome = string.Empty,
            Cpf = string.Empty,
            DataNascimento = DateOnly.FromDateTime(DateTime.Today.AddYears(-18)),
            Telefone = string.Empty,
            Endereco = new LogradouroDto()
            {
                Cep = string.Empty,
                Nome = string.Empty,
                Bairro = string.Empty,
                Cidade = string.Empty,
                Estado = string.Empty,
                Pais = string.Empty,
            },
            Numero = string.Empty,
        },
        Plano = AppMatriculaPlano.Anual,
        DataInicio = DateOnly.FromDateTime(DateTime.Today),
        DataFim = DateOnly.FromDateTime(DateTime.Today.AddYears(1)),
        Objetivo = string.Empty,
        RestricoesMedicas = AppMatriculaRestricoes.None,
        ObservacoesRestricoes = string.Empty,
        LaudoMedico = null
    };
    public MatriculaDto Matricula
    {
        get => _matricula;
        set { if (SetProperty(ref _matricula, value)) { SyncSelectionFromFlags(); /* quando trocar a matrícula, atualiza seleção */ } }
    }
    private int _matriculaId;
    public int MatriculaId
    {
        get => _matriculaId;
        set => SetProperty(ref _matriculaId, value);
    }
    private bool _isEditMode;
    public bool IsEditMode
    {
        get => _isEditMode;
        set => SetProperty(ref _isEditMode, value);
    }
    // Métodos e comandos e outros complementos

    public bool HasAlunoVinculado => Matricula?.AlunoMatricula != null && Matricula.AlunoMatricula.Id > 0;
    public bool HasNoAlunoVinculado => !HasAlunoVinculado;
    public bool IsMenor16 => Matricula?.AlunoMatricula != null && Matricula.AlunoMatricula.DataNascimento != default && Matricula.AlunoMatricula.DataNascimento > DateOnly.FromDateTime(DateTime.Today.AddYears(-16));
    public bool HasRestricoes => Matricula != null && Matricula.RestricoesMedicas != AppMatriculaRestricoes.None;
    public bool ExigeLaudo => IsMenor16 || HasRestricoes;
    public bool HasLaudo => Matricula?.LaudoMedico?.Conteudo != null && Matricula.LaudoMedico.Conteudo.Length > 0;
    public bool LaudoPendente => ExigeLaudo && !HasLaudo;
    public bool LaudoOpcional => !ExigeLaudo && !HasLaudo;
    public void NotificarPropriedadesVisuais()
    {
        OnPropertyChanged(nameof(HasAlunoVinculado));
        OnPropertyChanged(nameof(HasNoAlunoVinculado));
        OnPropertyChanged(nameof(IsMenor16));
        OnPropertyChanged(nameof(HasRestricoes));
        OnPropertyChanged(nameof(ExigeLaudo));
        OnPropertyChanged(nameof(HasLaudo));
        OnPropertyChanged(nameof(LaudoPendente));
        OnPropertyChanged(nameof(LaudoOpcional));
    }
    public async Task InitializeAsync()
    {
        if (MatriculaId > 0)
        {
            IsEditMode = true;
            Title = "Editar Matricula";
            await LoadMatriculaAsync();
        }
        else
        {
            IsEditMode = false;
            Title = "Nova Matricula";
            SyncSelectionFromFlags();
        }
        NotificarPropriedadesVisuais();
    }
    public MatriculaViewModel(IMatriculaService matriculaService, IAlunoService alunoService)
    {
        _matriculaService = matriculaService;
        _alunoService = alunoService;
        Title = "Detalhes da Matricula";
        BuildRestricoesMulti();
    }

    #region enum Flags <-> seleção múltipla(CheckBoxes)
    // Monta a lista de checkboxes a partir do enum (ignora None)
    private void BuildRestricoesMulti()
    {
        RestricoesMulti.Clear();
        foreach (var value in Enum.GetValues<AppMatriculaRestricoes>().Cast<AppMatriculaRestricoes>())
        {
            if (value == AppMatriculaRestricoes.None)
                continue;
            var item = new RestricaoSelectable
            {
                Value = value,
                IsSelected = Matricula.RestricoesMedicas.HasFlag(value)
            };
            item.PropertyChanged += RestricaoItem_PropertyChanged;
            RestricoesMulti.Add(item);
        }
    }
    // Atualiza DTO quando o usuário marca/desmarca
    private void RestricaoItem_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(RestricaoSelectable.IsSelected))
        {
            if (_suspendSelectionSync) return; // evita write-back durante Sync
            RecomputeRestricoesFlags();
        }
    }
    // Recalcula o bitmask a partir das seleções
    private void RecomputeRestricoesFlags()
    {
        var flags = AppMatriculaRestricoes.None;
        foreach (var item in RestricoesMulti)
        {
            if (item.IsSelected)
                flags |= item.Value;
        }
        Matricula.RestricoesMedicas = flags;
        OnPropertyChanged(nameof(Matricula));
        NotificarPropriedadesVisuais();
    }
    // Sincroniza seleção visual a partir do DTO (quando carrega ou substitui a matrícula)
    private void SyncSelectionFromFlags()
    {
        _suspendSelectionSync = true;
        try
        {
            foreach (var item in RestricoesMulti)
            {
                item.IsSelected = Matricula.RestricoesMedicas.HasFlag(item.Value);
            }
        }
        finally
        {
            _suspendSelectionSync = false;
        }
        NotificarPropriedadesVisuais();
    }
#endregion

    [RelayCommand]
    public void RemoverLaudo()
    {
        Matricula.LaudoMedico = null;
        OnPropertyChanged(nameof(Matricula));
        NotificarPropriedadesVisuais();
    }
    [RelayCommand]
    public async Task LoadMatriculaAsync()
    {
        if (MatriculaId <= 0)
            return;
        try
        {
            IsBusy = true;
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            var matriculaData = await _matriculaService.ObterPorIdAsync(MatriculaId, cts.Token);
            if (matriculaData != null)
            {
                Matricula = matriculaData;
                SyncSelectionFromFlags();
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Erro", $"Erro ao carregar matrícula: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }
    [RelayCommand]
    private async Task CancelAsync()
    {
        await Shell.Current.GoToAsync("..");
    }

    [RelayCommand]
    public async Task SaveMatriculaAsync()
    {
        if (IsBusy)
            return;
        // garante que o DTO está com os flags corretos
        RecomputeRestricoesFlags();
        if (!await ValidateMatriculaAsync(Matricula))
            return;
        try
        {
            IsBusy = true;
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            AlunoDto? alunoData = null;
            if (Matricula.AlunoMatricula?.Id > 0)
            {
                alunoData = await _alunoService.ObterPorIdAsync(Matricula.AlunoMatricula.Id, cts.Token);
            }
            else if (!string.IsNullOrWhiteSpace(Matricula.AlunoMatricula?.Cpf))
            {
                var cpfDigitos = new string(Matricula.AlunoMatricula.Cpf.Where(char.IsDigit).ToArray());
                alunoData = await _alunoService.ObterPorCpfAsync(cpfDigitos, cts.Token)
                ?? await _alunoService.ObterPorCpfAsync(Matricula.AlunoMatricula.Cpf, cts.Token);
            }
            if (alunoData == null)
            {
                await Shell.Current.DisplayAlertAsync("Erro", "O Aluno informado não foi encontrado. Busque um aluno válido pelo CPF.", "OK");
                return;
            }
            Matricula.AlunoMatricula = alunoData;
            // Revalida caso dados do aluno completo (idade) exijam laudo
            if (!await ValidateMatriculaAsync(Matricula))
                return;
            if (IsEditMode)
            {
                await _matriculaService.AtualizarAsync(Matricula, cts.Token);
                await Shell.Current.DisplayAlertAsync("Sucesso", "Matrícula atualizada com sucesso!", "OK");
            }
            else
            {
                await _matriculaService.AdicionarAsync(Matricula, cts.Token);
                await Shell.Current.DisplayAlertAsync("Sucesso", "Matrícula criada com sucesso!", "OK");
            }
            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Erro", $"Erro ao salvar matrícula: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    public async Task SearchByCpfAsync()
    {
        if (string.IsNullOrWhiteSpace(Matricula.AlunoMatricula?.Cpf))
        {
            await Shell.Current.DisplayAlertAsync("Aviso", "Informe o CPF para realizar a busca.", "OK");
            return;
        }
        try
        {
            IsBusy = true;
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            var cpfLimpo = new string(Matricula.AlunoMatricula.Cpf.Where(char.IsDigit).ToArray());
            var alunoData = await _alunoService.ObterPorCpfAsync(cpfLimpo, cts.Token)
            ?? await _alunoService.ObterPorCpfAsync(Matricula.AlunoMatricula.Cpf, cts.Token);
            if (alunoData != null)
            {
                Matricula.AlunoMatricula = alunoData;
                OnPropertyChanged(nameof(Matricula));
                NotificarPropriedadesVisuais();
                await Shell.Current.DisplayAlertAsync("Aviso", "Aluno encontrado! Dados preenchidos automaticamente.", "OK");
            }
            else
            {
                await Shell.Current.DisplayAlertAsync("Aviso", "Aluno não encontrado.", "OK");
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Erro", $"Erro ao buscar CPF: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    public async Task SelecionarFotoAsync()
    {
        try
        {
            string escolha = await Shell.Current.DisplayActionSheetAsync("Origem do Laudo Médico", "Cancelar", null, "Galeria", "Câmera", "Remover Laudo"); if (escolha == "Remover Laudo")
            {
                Matricula.LaudoMedico = null;
                OnPropertyChanged(nameof(Matricula));
                NotificarPropriedadesVisuais();
                return;
            }
            FileResult? result = null;
            if (escolha == "Galeria")
            {
                result = await FilePicker.Default.PickAsync(new PickOptions
                {
                    PickerTitle = "Selecione o Laudo Médico",
                    FileTypes = FilePickerFileType.Images
                });
            }
            else if (escolha == "Câmera")
            {
                if (MediaPicker.Default.IsCaptureSupported)
                {
                    result = await MediaPicker.Default.CapturePhotoAsync();
                }
                else
                {
                    await Shell.Current.DisplayAlertAsync("Erro", "Captura de foto não suportada neste dispositivo.", "OK");
                    return;
                }
            }
            if (result != null)
            {
                using var stream = await result.OpenReadAsync();
                using var ms = new MemoryStream();
                await stream.CopyToAsync(ms);
                Matricula.LaudoMedico = new ArquivoDto { Conteudo = ms.ToArray() };
                OnPropertyChanged(nameof(Matricula));
                NotificarPropriedadesVisuais();
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Erro", $"Erro ao selecionar imagem: {ex.Message}", "OK");
        }
    }

    /*
 Validação utilizando o padrão Notification Pattern, retornando uma lista de erros para o usuário, em vez de fail-fast exceptions.
 Isso permite que o usuário veja todos os problemas de uma vez e corrija-os antes de tentar salvar novamente.
 */
    private static async Task<bool> ValidateMatriculaAsync(MatriculaDto matricula)
    {
        var errors = new List<string>();
        // Aluno
        if (matricula.AlunoMatricula == null || (matricula.AlunoMatricula.Id <= 0 && string.IsNullOrWhiteSpace(matricula.AlunoMatricula.Cpf)))
        {
            errors.Add("• Aluno é obrigatório. Informe o CPF e realize a busca.");
        }
        // Data de Início
        if (matricula.DataInicio == default)
        {
            errors.Add("• Data de início é obrigatória.");
        }
        // Objetivo
        if (string.IsNullOrWhiteSpace(matricula.Objetivo))
        {
            errors.Add("• Objetivo da matrícula é obrigatório.");
        }
        // Plano
        if (!Enum.IsDefined(matricula.Plano))
        {
            errors.Add("• Plano da matrícula é inválido.");
        }
        // Laudo Médico para menor de 16 anos
        bool menor16 = matricula.AlunoMatricula != null
        && matricula.AlunoMatricula.DataNascimento != default
        && matricula.AlunoMatricula.DataNascimento > DateOnly.FromDateTime(DateTime.Today.AddYears(-16));
        bool possuiLaudo = matricula.LaudoMedico?.Conteudo != null && matricula.LaudoMedico.Conteudo.Length > 0;
        if (menor16 && !possuiLaudo)
        {
            errors.Add("• Alunos menores de 16 anos devem obrigatoriamente apresentar laudo médico.");
        }
        // Laudo Médico para restrições médicas
        if (matricula.RestricoesMedicas != AppMatriculaRestricoes.None && !possuiLaudo)
        {
            errors.Add("• Alunos com restrições médicas cadastradas devem obrigatoriamente apresentar laudo médico.");
        }
        if (errors.Count > 0)
        {
            var mensagem = "Por favor, corrija os seguintes campos:\n\n" + string.Join("\n", errors);
            await Shell.Current.DisplayAlertAsync("Erros de Validação", mensagem, "OK");
            return false;
        }
        return true;
    }
}