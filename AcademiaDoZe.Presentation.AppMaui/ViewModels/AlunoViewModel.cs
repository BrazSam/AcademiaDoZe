using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Application.Interfaces;
using CommunityToolkit.Mvvm.Input;
namespace AcademiaDoZe.Presentation.AppMaui.ViewModels;

[QueryProperty(nameof(AlunoId), "Id")]
public partial class AlunoViewModel : BaseViewModel
{
    private readonly IAlunoService _alunoService;
    private readonly ILogradouroService _logradouroService;
    private AlunoDto _aluno = CriarAlunoPadrao();
    public AlunoDto Aluno
    {
        get => _aluno;
        set => SetProperty(ref _aluno, value);
    }
    private int _alunoId;
    public int AlunoId
    {
        get => _alunoId;
        set => SetProperty(ref _alunoId, value);
    }
    private bool _isEditMode;
    public bool IsEditMode
    {
        get => _isEditMode;
        set => SetProperty(ref _isEditMode, value);
    }
    private string _confirmarSenha = string.Empty;
    public string ConfirmarSenha
    {
        get => _confirmarSenha;
        set => SetProperty(ref _confirmarSenha, value);
    }
    public bool HasEnderecoVinculado => Aluno?.Endereco != null && Aluno.Endereco.Id > 0;
    public bool HasNoEnderecoVinculado => !HasEnderecoVinculado;
    public AlunoViewModel(IAlunoService alunoService, ILogradouroService logradouroService)
    {
        _alunoService = alunoService;
        _logradouroService = logradouroService;
        Title = "Detalhes do Aluno";
    }

    // Incluir métodos e commandos aqui

    public async Task InitializeAsync()
    {
        ConfirmarSenha = string.Empty;
        if (AlunoId > 0)
        {
            IsEditMode = true;
            Title = "Editar Aluno";
            await LoadAlunoAsync();
        }
        else
        {
            IsEditMode = false;
            Title = "Novo Aluno";
            Aluno = CriarAlunoPadrao();
            NotificarStatusEndereco();
        }
    }
    private void NotificarStatusEndereco()
    {
        OnPropertyChanged(nameof(HasEnderecoVinculado));
        OnPropertyChanged(nameof(HasNoEnderecoVinculado));
    }
    private static AlunoDto CriarAlunoPadrao() => new()
    {
        Nome = string.Empty,
        Cpf = string.Empty,
        DataNascimento = DateOnly.FromDateTime(DateTime.Today.AddYears(-18)),
        Telefone = string.Empty,
        Email = string.Empty,
        Senha = string.Empty,
        Endereco = new LogradouroDto
        {
            Cep = string.Empty,
            Nome = string.Empty,
            Bairro = string.Empty,
            Cidade = string.Empty,
            Estado = string.Empty,
            Pais = "Brasil"
        },
        Numero = string.Empty,
        Complemento = string.Empty
    };


    [RelayCommand]
    private async Task CancelAsync()
    {
        await Shell.Current.GoToAsync("..");
    }
    [RelayCommand]
    public async Task LoadAlunoAsync()
    {
        if (AlunoId <= 0)
            return;
        try
        {
            IsBusy = true;
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            var alunoData = await _alunoService.ObterPorIdAsync(AlunoId, cts.Token);
            if (alunoData != null)
            {
                // Garante que o Endereço não seja nulo para evitar erros de bind
                alunoData.Endereco ??= new LogradouroDto
                {
                    Cep = string.Empty,
                    Nome = string.Empty,
                    Bairro = string.Empty,
                    Cidade = string.Empty,
                    Estado = string.Empty,
                    Pais = "Brasil"
                };
                // A senha não é retornada por motivos de segurança; limpamos para preenchimento opcional
                alunoData.Senha = string.Empty;
                ConfirmarSenha = string.Empty;
                Aluno = alunoData;
                NotificarStatusEndereco();
            }
        }
        catch (OperationCanceledException)
        {
            await Shell.Current.DisplayAlertAsync("Tempo Esgotado", "O carregamento do aluno expirou. Verifique a conexão.", "OK");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Erro", $"Erro ao carregar aluno: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }


    [RelayCommand]
    public async Task SearchByCpfAsync()
    {
        if (string.IsNullOrWhiteSpace(Aluno.Cpf))
        {
            await Shell.Current.DisplayAlertAsync("Aviso", "Informe o CPF para realizar a busca.", "OK");
            return;
        }
        var apenasDigitos = new string([.. Aluno.Cpf.Where(char.IsDigit)]);
        if (apenasDigitos.Length != 11)
        {
            await Shell.Current.DisplayAlertAsync("Validação", "O CPF deve conter exatamente 11 dígitos numéricos.", "OK");
            return;
        }
        try
        {
            IsBusy = true;
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            var alunoData = await _alunoService.ObterPorCpfAsync(apenasDigitos, cts.Token);
            if (alunoData != null)
            {
                alunoData.Endereco ??= new LogradouroDto
                {
                    Cep = string.Empty,
                    Nome = string.Empty,
                    Bairro = string.Empty,
                    Cidade = string.Empty,
                    Estado = string.Empty,
                    Pais = "Brasil"
                };
                alunoData.Senha = string.Empty;
                ConfirmarSenha = string.Empty;
                Aluno = alunoData;
                AlunoId = alunoData.Id;
                IsEditMode = true;
                Title = "Editar Aluno";
                NotificarStatusEndereco();
                await Shell.Current.DisplayAlertAsync("Aviso", "Aluno já cadastrado! Dados carregados para edição.", "OK");
            }
            else
            {
                await Shell.Current.DisplayAlertAsync("Aviso", "CPF não encontrado.", "OK");
            }
        }
        catch (OperationCanceledException)
        {
            await Shell.Current.DisplayAlertAsync("Tempo Esgotado", "A busca por CPF expirou. Verifique a conexão.", "OK");
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
    public async Task SearchByCepAsync()
    {
        if (Aluno.Endereco == null || string.IsNullOrWhiteSpace(Aluno.Endereco.Cep))
        {
            await Shell.Current.DisplayAlertAsync("Aviso", "Informe o CEP para realizar a busca.", "OK");
            return;
        }
        var apenasDigitos = new string([.. Aluno.Endereco.Cep.Where(char.IsDigit)]);
        if (apenasDigitos.Length != 8)
        {
            await Shell.Current.DisplayAlertAsync("Validação", "O CEP deve conter exatamente 8 dígitos numéricos.", "OK");
            return;
        }
        try
        {
            IsBusy = true;
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            var logradouroData = await _logradouroService.ObterPorCepAsync(apenasDigitos, cts.Token);
            if (logradouroData != null)
            {
                Aluno.Endereco = logradouroData;
                OnPropertyChanged(nameof(Aluno));
                NotificarStatusEndereco();
                await Shell.Current.DisplayAlertAsync("Aviso", "CEP encontrado! Endereço preenchido automaticamente.", "OK");
            }
            else
            {
                await Shell.Current.DisplayAlertAsync("Aviso", "CEP não encontrado no cadastro de logradouros. Cadastre o logradouro primeiro.", "OK");
            }
        }
        catch (OperationCanceledException)
        {
            await Shell.Current.DisplayAlertAsync("Tempo Esgotado", "A busca de CEP expirou. Verifique a conexão com o banco.", "OK");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Erro", $"Erro ao buscar CEP: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }



    [RelayCommand]
    public async Task SaveAlunoAsync()
    {
        if (IsBusy)
            return;
        if (!await ValidateAlunoAsync(Aluno, IsEditMode, ConfirmarSenha))
            return;
        try
        {
            IsBusy = true;
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            // Se o endereço foi digitado mas não tem ID ainda, tenta buscar pelo CEP
            if (Aluno.Endereco != null && Aluno.Endereco.Id <= 0 && !string.IsNullOrWhiteSpace(Aluno.Endereco.Cep))
            {
                var cepLimpo = new string([.. Aluno.Endereco.Cep.Where(char.IsDigit)]);
                if (cepLimpo.Length == 8)
                {
                    var logradouroData = await _logradouroService.ObterPorCepAsync(cepLimpo, cts.Token);
                    if (logradouroData != null)
                    {
                        Aluno.Endereco = logradouroData;
                    }
                    else
                    {
                        await Shell.Current.DisplayAlertAsync("Endereço Obrigatório", "O CEP informado não está cadastrado como Logradouro. Cadastre-o previamente.", "OK");
                        return;
                    }
                }
                else
                {
                    await Shell.Current.DisplayAlertAsync("Validação", "Informe um CEP válido com 8 dígitos para o endereço.", "OK");
                    return;
                }
            }
            // Normaliza os dados antes de persistir
            Aluno.Nome = Aluno.Nome.Trim();
            Aluno.Cpf = new string([.. Aluno.Cpf.Where(char.IsDigit)]);
            Aluno.Telefone = new string([.. Aluno.Telefone.Where(char.IsDigit)]);
            Aluno.Numero = Aluno.Numero.Trim();
            Aluno.Complemento = string.IsNullOrWhiteSpace(Aluno.Complemento) ? null : Aluno.Complemento.Trim();
            Aluno.Email = string.IsNullOrWhiteSpace(Aluno.Email) ? null : Aluno.Email.Trim();
            Aluno.Senha = string.IsNullOrWhiteSpace(Aluno.Senha) ? null : Aluno.Senha.Trim();
            if (IsEditMode)
            {
                await _alunoService.AtualizarAsync(Aluno, cts.Token);
                await Shell.Current.DisplayAlertAsync("Sucesso", "Aluno atualizado com sucesso!", "OK");
            }
            else
            {
                await _alunoService.AdicionarAsync(Aluno, cts.Token);
                await Shell.Current.DisplayAlertAsync("Sucesso", "Aluno criado com sucesso!", "OK");
            }
            await Shell.Current.GoToAsync("..");
        }
        catch (OperationCanceledException)
        {
            await Shell.Current.DisplayAlertAsync("Tempo Esgotado", "A gravação do aluno expirou. Verifique a conexão.", "OK");
        }
        catch (InvalidOperationException ex)
        {
            await Shell.Current.DisplayAlertAsync("Regra de Negócio", ex.Message, "OK");
        }
        catch (ArgumentException ex)
        {
            await Shell.Current.DisplayAlertAsync("Validação", ex.Message, "OK");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Erro", $"Erro ao salvar aluno: {ex.Message}", "OK");
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
            string escolha = await Shell.Current.DisplayActionSheetAsync("Origem da Imagem", "Cancelar", "Remover Foto", "Galeria", "Câmera");
            if (escolha == "Remover Foto")
            {
                Aluno.Foto = null;
                OnPropertyChanged(nameof(Aluno));
                return;
            }
            FileResult? result = null;
            if (escolha == "Galeria")
            {
                result = await FilePicker.Default.PickAsync(new PickOptions
                {
                    PickerTitle = "Selecione uma imagem",
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
                Aluno.Foto = new ArquivoDto { Conteudo = ms.ToArray() };
                OnPropertyChanged(nameof(Aluno));
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
    private static async Task<bool> ValidateAlunoAsync(AlunoDto aluno, bool isEditMode, string confirmarSenha)
    {
        var errors = new List<string>();
        if (string.IsNullOrWhiteSpace(aluno.Nome))
        {
            errors.Add("• Nome é obrigatório.");
        }
        if (string.IsNullOrWhiteSpace(aluno.Cpf))
        {
            errors.Add("• CPF é obrigatório.");
        }
        else
        {
            var apenasDigitosCpf = new string([.. aluno.Cpf.Where(char.IsDigit)]);
            if (apenasDigitosCpf.Length != 11)
            {
                errors.Add("• CPF deve conter exatamente 11 dígitos numéricos.");
            }
        }
        if (aluno.DataNascimento == default)
        {
            errors.Add("• Data de nascimento é obrigatória.");
        }
        else
        {
            var hoje = DateOnly.FromDateTime(DateTime.Today);
            if (aluno.DataNascimento > hoje)
            {
                errors.Add("• Data de nascimento não pode ser futura.");
            }
            else if (aluno.DataNascimento > DateOnly.FromDateTime(DateTime.Today.AddYears(-12)))
            {
                errors.Add("• O aluno deve ter no mínimo 12 anos de idade.");
            }
        }
        if (string.IsNullOrWhiteSpace(aluno.Telefone))
        {
            errors.Add("• Telefone é obrigatório.");
        }
        else
        {
            var apenasDigitosTel = new string([.. aluno.Telefone.Where(char.IsDigit)]);
            if (apenasDigitosTel.Length != 11)
            {
                errors.Add("• Telefone deve conter 11 dígitos numéricos com DDD.");
            }
        }
        if (!string.IsNullOrWhiteSpace(aluno.Email))
        {
            var emailLimpo = aluno.Email.Trim();
            if (!ValidarFormatoEmail(emailLimpo))
            {
                errors.Add("• Formato de e-mail inválido.");
            }
        }
        // Senha: obrigatória no cadastro; na edição só valida se foi preenchida
        if (!isEditMode)
        {
            if (string.IsNullOrWhiteSpace(aluno.Senha))
            {
                errors.Add("• Senha é obrigatória no cadastro.");
            }
            else
            {
                var senhaLimpa = aluno.Senha.Trim();
                if (senhaLimpa.Length < 6 || !senhaLimpa.Any(char.IsUpper))
                {
                    errors.Add("• A senha deve ter no mínimo 6 caracteres e pelo menos uma letra maiúscula.");
                }
                if (aluno.Senha != confirmarSenha)
                {
                    errors.Add("• A confirmação de senha não confere com a senha informada.");
                }
            }
        }
        else if (!string.IsNullOrWhiteSpace(aluno.Senha))
        {
            var senhaLimpa = aluno.Senha.Trim();
            if (senhaLimpa.Length < 6 || !senhaLimpa.Any(char.IsUpper))
            {
                errors.Add("• A nova senha deve ter no mínimo 6 caracteres e pelo menos uma letra maiúscula.");
            }
            if (aluno.Senha != confirmarSenha)
            {
                errors.Add("• A confirmação de senha não confere com a nova senha informada.");
            }
        }
        if (aluno.Endereco == null || string.IsNullOrWhiteSpace(aluno.Endereco.Cep))
        {
            errors.Add("• CEP do endereço é obrigatório.");
        }
        if (string.IsNullOrWhiteSpace(aluno.Numero))
        {
            errors.Add("• Número do endereço é obrigatório.");
        }
        if (errors.Count > 0)
        {
            var mensagem = "Por favor, corrija os seguintes campos:\n\n" + string.Join("\n", errors);
            await Shell.Current.DisplayAlertAsync("Erros de Validação", mensagem, "OK");
            return false;
        }
        return true;
    }


    private static bool ValidarFormatoEmail(string email)
    {
        var partes = email.Split('@');
        if (partes.Length != 2) return false;
        if (string.IsNullOrWhiteSpace(partes[0])) return false;
        var dominio = partes[1];
        if (string.IsNullOrWhiteSpace(dominio)) return false;
        if (dominio.StartsWith('.') || dominio.EndsWith('.')) return false;
        var labels = dominio.Split('.');
        if (labels.Length < 2) return false;
        if (labels.Any(string.IsNullOrWhiteSpace)) return false;
        return true;
    }
}