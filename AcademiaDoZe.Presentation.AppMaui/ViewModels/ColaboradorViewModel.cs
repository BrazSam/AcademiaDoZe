using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Application.Enums;
using AcademiaDoZe.Application.Interfaces;
using CommunityToolkit.Mvvm.Input;
namespace AcademiaDoZe.Presentation.AppMaui.ViewModels;

[QueryProperty(nameof(ColaboradorId), "Id")]
public partial class ColaboradorViewModel : BaseViewModel
{
    public IEnumerable<AppColaboradorTipo> ColaboradorTipos { get; } = Enum.GetValues<AppColaboradorTipo>().Cast<AppColaboradorTipo>();
    public IEnumerable<AppColaboradorVinculo> ColaboradorVinculos { get; } = Enum.GetValues<AppColaboradorVinculo>().Cast<AppColaboradorVinculo>();
    private readonly IColaboradorService _colaboradorService;
    private readonly ILogradouroService _logradouroService;
    private ColaboradorDto _colaborador = CriarColaboradorPadrao();
    public ColaboradorDto Colaborador
    {
        get => _colaborador;
        set => SetProperty(ref _colaborador, value);
    }
    private int _colaboradorId;
    public int ColaboradorId
    {
        get => _colaboradorId;
        set => SetProperty(ref _colaboradorId, value);
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
    public bool HasEnderecoVinculado => Colaborador?.Endereco != null && Colaborador.Endereco.Id > 0;
    public bool HasNoEnderecoVinculado => !HasEnderecoVinculado;
    public ColaboradorViewModel(IColaboradorService colaboradorService, ILogradouroService logradouroService)
    {
        _colaboradorService = colaboradorService;
        _logradouroService = logradouroService;
        Title = "Detalhes do Colaborador";
    }

    // Incluir métodos e commandos aqui

    public async Task InitializeAsync()
    {
        ConfirmarSenha = string.Empty;
        if (ColaboradorId > 0)
        {
            IsEditMode = true;
            Title = "Editar Colaborador";
            await LoadColaboradorAsync();
        }
        else
        {
            IsEditMode = false;
            Title = "Novo Colaborador";
            Colaborador = CriarColaboradorPadrao();
            NotificarStatusEndereco();
        }
    }
    private void NotificarStatusEndereco()
    {
        OnPropertyChanged(nameof(HasEnderecoVinculado));
        OnPropertyChanged(nameof(HasNoEnderecoVinculado));
    }
    private static ColaboradorDto CriarColaboradorPadrao() => new()
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
        Complemento = string.Empty,
        DataAdmissao = DateOnly.FromDateTime(DateTime.Today),
        Tipo = AppColaboradorTipo.Atendente,
        Vinculo = AppColaboradorVinculo.CLT
    };


    [RelayCommand]
    private async Task CancelAsync()
    {
        await Shell.Current.GoToAsync("..");
    }
    [RelayCommand]
    public async Task LoadColaboradorAsync()
    {
        if (ColaboradorId <= 0)
            return;
        try
        {
            IsBusy = true;
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            var colaboradorData = await _colaboradorService.ObterPorIdAsync(ColaboradorId, cts.Token);
            if (colaboradorData != null)
            {
                // Garante que o Endereço não seja nulo para evitar erros de bind
                colaboradorData.Endereco ??= new LogradouroDto
                {
                    Cep = string.Empty,
                    Nome = string.Empty,
                    Bairro = string.Empty,
                    Cidade = string.Empty,
                    Estado = string.Empty,
                    Pais = "Brasil"
                };
                // A senha não é retornada por motivos de segurança; limpamos para preenchimento opcional
                colaboradorData.Senha = string.Empty;
                ConfirmarSenha = string.Empty;
                Colaborador = colaboradorData;
                NotificarStatusEndereco();
            }
        }
        catch (OperationCanceledException)
        {
            await Shell.Current.DisplayAlertAsync("Tempo Esgotado", "O carregamento do colaborador expirou. Verifique a conexão.", "OK");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Erro", $"Erro ao carregar colaborador: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }


    [RelayCommand]
         public async Task SearchByCpfAsync()
            {
                if (string.IsNullOrWhiteSpace(Colaborador.Cpf))
                    return;
                try
                {
                    IsBusy = true;
                    // normaliza para apenas dígitos (o repositório espera dígitos)
                    var cpfNormalized = new string(Colaborador.Cpf.Where(char.IsDigit).ToArray());
                    var resultados = (await _colaboradorService.ObterPorCpfAsync(cpfNormalized))?.ToList() ?? new List<ColaboradorDto>();
                    if (!resultados.Any())
                    {
                        await Shell.Current.DisplayAlertAsync("Aviso", "CPF não encontrado.", "OK"); return;
                    }
                    if (resultados.Count == 1)
                    {
                        await CarregarParaEdicaoAsync(resultados.First().Id);
                        await Shell.Current.DisplayAlertAsync("Aviso", "Colaborador já cadastrado! Dados carregados para edição.", "OK"); return;
                    }
                    // múltiplos resultados -> perguntar ao usuário qual selecionar
                    var options = resultados.Select(c => $"{c.Id} - {c.Nome} ({c.Cpf})").ToArray();
                    var escolha = await Shell.Current.DisplayActionSheetAsync("Vários colaboradores encontrados", "Cancelar", null, options);
                    if (string.IsNullOrWhiteSpace(escolha) || escolha == "Cancelar")
                        return;
                    // extrai ID a partir da string selecionada ("{Id} - ...")
                    var idStr = escolha.Split('-', 2).FirstOrDefault()?.Trim();
                    if (int.TryParse(idStr, out var selId))
                    {
                        var selecionado = resultados.FirstOrDefault(c => c.Id == selId);
                        if (selecionado != null)
                        {
                            await CarregarParaEdicaoAsync(selecionado.Id);
                            await Shell.Current.DisplayAlertAsync("Aviso", "Colaborador selecionado: dados carregados para edição.", "OK");
                        }
                    }
                }
                catch (Exception ex) { await Shell.Current.DisplayAlertAsync("Erro", $"Erro ao buscar CPF: {ex.Message}", "OK"); }
                finally { IsBusy = false; }
            }


    private async Task CarregarParaEdicaoAsync(int id)
    {
        ColaboradorId = id;
        IsEditMode = true;
        Title = "Editar Colaborador";
        await LoadColaboradorAsync();
    }

    [RelayCommand]
    public async Task SearchByCepAsync()
    {
        if (Colaborador.Endereco == null || string.IsNullOrWhiteSpace(Colaborador.Endereco.Cep))
        {
            await Shell.Current.DisplayAlertAsync("Aviso", "Informe o CEP para realizar a busca.", "OK");
            return;
        }
        var apenasDigitos = new string([.. Colaborador.Endereco.Cep.Where(char.IsDigit)]);
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
                Colaborador.Endereco = logradouroData;
                OnPropertyChanged(nameof(Colaborador));
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
    public async Task SaveColaboradorAsync()
    {
        if (IsBusy)
            return;
        if (!await ValidateColaboradorAsync(Colaborador, IsEditMode, ConfirmarSenha))
            return;
        try
        {
            IsBusy = true;
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            // Se o endereço foi digitado mas não tem ID ainda, tenta buscar pelo CEP
            if (Colaborador.Endereco != null && Colaborador.Endereco.Id <= 0 && !string.IsNullOrWhiteSpace(Colaborador.Endereco.Cep))
            {
                var cepLimpo = new string([.. Colaborador.Endereco.Cep.Where(char.IsDigit)]);
                if (cepLimpo.Length == 8)
                {
                    var logradouroData = await _logradouroService.ObterPorCepAsync(cepLimpo, cts.Token);
                    if (logradouroData != null)
                    {
                        Colaborador.Endereco = logradouroData;
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
            Colaborador.Nome = Colaborador.Nome.Trim();
            Colaborador.Cpf = new string([.. Colaborador.Cpf.Where(char.IsDigit)]);
            Colaborador.Telefone = new string([.. Colaborador.Telefone.Where(char.IsDigit)]);
            Colaborador.Numero = Colaborador.Numero.Trim();
            Colaborador.Complemento = string.IsNullOrWhiteSpace(Colaborador.Complemento) ? null : Colaborador.Complemento.Trim();
            Colaborador.Email = string.IsNullOrWhiteSpace(Colaborador.Email) ? null : Colaborador.Email.Trim();
            if (IsEditMode)
            {
                await _colaboradorService.AtualizarAsync(Colaborador, cts.Token);
                await Shell.Current.DisplayAlertAsync("Sucesso", "Colaborador atualizado com sucesso!", "OK");
            }
            else
            {
                await _colaboradorService.AdicionarAsync(Colaborador, cts.Token);
                await Shell.Current.DisplayAlertAsync("Sucesso", "Colaborador criado com sucesso!", "OK");
            }
            await Shell.Current.GoToAsync("..");
        }
        catch (OperationCanceledException)
        {
            await Shell.Current.DisplayAlertAsync("Tempo Esgotado", "A gravação do colaborador expirou. Verifique a conexão.", "OK");
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
            await Shell.Current.DisplayAlertAsync("Erro", $"Erro ao salvar colaborador: {ex.Message}", "OK");
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
                Colaborador.Foto = null;
                OnPropertyChanged(nameof(Colaborador));
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
                Colaborador.Foto = new ArquivoDto { Conteudo = ms.ToArray() };
                OnPropertyChanged(nameof(Colaborador));
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
    private static async Task<bool> ValidateColaboradorAsync(ColaboradorDto colaborador, bool isEditMode, string confirmarSenha)
    {
        var errors = new List<string>();
        if (string.IsNullOrWhiteSpace(colaborador.Nome))
        {
            errors.Add("• Nome é obrigatório.");
        }
        if (string.IsNullOrWhiteSpace(colaborador.Cpf))
        {
            errors.Add("• CPF é obrigatório.");
        }
        else
        {
            var apenasDigitosCpf = new string([.. colaborador.Cpf.Where(char.IsDigit)]);
            if (apenasDigitosCpf.Length != 11)
            {
                errors.Add("• CPF deve conter exatamente 11 dígitos numéricos.");
            }
            else if (AcademiaDoZe.Domain.ValueObjects.Cpf.Criar(apenasDigitosCpf).IsFailure)
            {
                errors.Add("• CPF inválido. Confira os dígitos verificadores; apenas ter 11 dígitos não é suficiente.");
            }
        }
        if (colaborador.DataNascimento == default)
        {
            errors.Add("• Data de nascimento é obrigatória.");
        }
        else
        {
            var hoje = DateOnly.FromDateTime(DateTime.Today);
            if (colaborador.DataNascimento > hoje)
            {
                errors.Add("• Data de nascimento não pode ser futura.");
            }
            else if (colaborador.DataNascimento > DateOnly.FromDateTime(DateTime.Today.AddYears(-12)))
            {
                errors.Add("• O colaborador deve ter no mínimo 12 anos de idade.");
            }
        }
        if (string.IsNullOrWhiteSpace(colaborador.Telefone))
        {
            errors.Add("• Telefone é obrigatório.");
        }
        else
        {
            var apenasDigitosTel = new string([.. colaborador.Telefone.Where(char.IsDigit)]);
            if (apenasDigitosTel.Length != 11)
            {
                errors.Add("• Telefone deve conter 11 dígitos numéricos com DDD.");
            }
        }
        if (string.IsNullOrWhiteSpace(colaborador.Email))
        {
            errors.Add("• E-mail é obrigatório.");
        }
        else
        {
            var emailLimpo = colaborador.Email.Trim();
            if (!ValidarFormatoEmail(emailLimpo))
            {
                errors.Add("• Formato de e-mail inválido.");
            }
        }
        // Senha: obrigatória no cadastro; na edição só valida se foi preenchida
        if (!isEditMode)
        {
            if (string.IsNullOrWhiteSpace(colaborador.Senha))
            {
                errors.Add("• Senha é obrigatória no cadastro.");
            }
            else
            {
                var senhaLimpa = colaborador.Senha.Trim();
                if (senhaLimpa.Length < 6 || !senhaLimpa.Any(char.IsUpper))
                {
                    errors.Add("• A senha deve ter no mínimo 6 caracteres e pelo menos uma letra maiúscula.");
                }
                if (colaborador.Senha != confirmarSenha)
                {
                    errors.Add("• A confirmação de senha não confere com a senha informada.");
                }
            }
        }
        else if (!string.IsNullOrWhiteSpace(colaborador.Senha))
        {
            var senhaLimpa = colaborador.Senha.Trim();
            if (senhaLimpa.Length < 6 || !senhaLimpa.Any(char.IsUpper))
            {
                errors.Add("• A nova senha deve ter no mínimo 6 caracteres e pelo menos uma letra maiúscula.");
            }
            if (colaborador.Senha != confirmarSenha)
            {
                errors.Add("• A confirmação de senha não confere com a nova senha informada.");
            }
        }
        if (colaborador.Endereco == null || string.IsNullOrWhiteSpace(colaborador.Endereco.Cep))
        {
            errors.Add("• CEP do endereço é obrigatório.");
        }
        if (string.IsNullOrWhiteSpace(colaborador.Numero))
        {
            errors.Add("• Número do endereço é obrigatório.");
        }
        if (colaborador.DataAdmissao == default)
        {
            errors.Add("• Data de admissão é obrigatória.");
        }
        else if (colaborador.DataAdmissao > DateOnly.FromDateTime(DateTime.Today))
        {
            errors.Add("• Data de admissão não pode ser maior que a data atual.");
        }
        // Regra de Vínculo de Administrador: Administrador precisa ser CLT
        if (!Enum.IsDefined(colaborador.Tipo))
            errors.Add("• Selecione um tipo de colaborador válido.");
        if (!Enum.IsDefined(colaborador.Vinculo))
            errors.Add("• Selecione um vínculo válido.");
        if (colaborador.Tipo == AppColaboradorTipo.Administrador && colaborador.Vinculo != AppColaboradorVinculo.CLT)
        {
            errors.Add("• Colaborador com tipo Administrador deve ter vínculo CLT.");
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
