using AcademiaDoZe.Application.DTOs; //Samuel Braz dos Santos
using AcademiaDoZe.Application.Enums;
using AcademiaDoZe.Application.Interfaces;
using CommunityToolkit.Mvvm.Input;
using System.Globalization;
namespace AcademiaDoZe.Presentation.AppMaui.ViewModels;

public partial class AcessoViewModel : BaseViewModel
{
    private readonly IAlunoService _alunoService;
    private readonly IColaboradorService _colaboradorService;
    private readonly IAcessoAlunoService _acessoAlunoService;
    private readonly IAcessoColaboradorService _acessoColaboradorService;
    private IDispatcherTimer? _timerRelogio;

    public AcessoViewModel(IAlunoService alunoService, IColaboradorService colaboradorService,
        IAcessoAlunoService acessoAlunoService, IAcessoColaboradorService acessoColaboradorService)
    {
        _alunoService = alunoService;
        _colaboradorService = colaboradorService;
        _acessoAlunoService = acessoAlunoService;
        _acessoColaboradorService = acessoColaboradorService;
        Title = "Acesso";
        AtualizarRelogio();
    }

    // ---------- Relógio do topo ----------
    private string _horaAtual = string.Empty;
    public string HoraAtual
    {
        get => _horaAtual;
        set => SetProperty(ref _horaAtual, value);
    }
    private string _dataAtual = string.Empty;
    public string DataAtual
    {
        get => _dataAtual;
        set => SetProperty(ref _dataAtual, value);
    }

    // ---------- CPF com máscara ----------
    private string _cpf = string.Empty;
    public string Cpf
    {
        get => _cpf;
        set => SetProperty(ref _cpf, FormatarCpf(value)); // aplica a máscara a cada tecla
    }
    private string CpfDigitos => new([.. Cpf.Where(char.IsDigit)]);

    // ---------- Painel de resultado ----------
    private bool _mostrarBoasVindas;
    public bool MostrarBoasVindas
    {
        get => _mostrarBoasVindas;
        set => SetProperty(ref _mostrarBoasVindas, value);
    }
    private bool _mostrarNegado;
    public bool MostrarNegado
    {
        get => _mostrarNegado;
        set => SetProperty(ref _mostrarNegado, value);
    }
    private bool _isEntrada; // verde na entrada, roxo na saída
    public bool IsEntrada
    {
        get => _isEntrada;
        set => SetProperty(ref _isEntrada, value);
    }
    private string _saudacao = string.Empty; // "Bom dia, Samuel!" / "Até logo, Samuel!"
    public string Saudacao
    {
        get => _saudacao;
        set => SetProperty(ref _saudacao, value);
    }
    private string _papel = string.Empty; // "Aluno" ou "Colaborador"
    public string Papel
    {
        get => _papel;
        set => SetProperty(ref _papel, value);
    }
    private string _movimento = string.Empty; // "Entrada · 08:15"
    public string Movimento
    {
        get => _movimento;
        set => SetProperty(ref _movimento, value);
    }
    private string _mensagem = string.Empty; // "Bom treino!" / motivo do acesso negado
    public string Mensagem
    {
        get => _mensagem;
        set => SetProperty(ref _mensagem, value);
    }
    private ArquivoDto? _foto;
    public ArquivoDto? Foto
    {
        get => _foto;
        set => SetProperty(ref _foto, value);
    }
    private bool _temFoto;
    public bool TemFoto
    {
        get => _temFoto;
        set => SetProperty(ref _temFoto, value);
    }
    private string _iniciais = string.Empty; // aparece no círculo quando não há foto
    public string Iniciais
    {
        get => _iniciais;
        set => SetProperty(ref _iniciais, value);
    }

    // ---------- Ciclo de vida da tela (chamados pela code-behind) ----------
    public void Iniciar()
    {
        _timerRelogio ??= Microsoft.Maui.Controls.Application.Current!.Dispatcher.CreateTimer();
        _timerRelogio.Interval = TimeSpan.FromSeconds(1);
        _timerRelogio.Tick -= OnTickRelogio;
        _timerRelogio.Tick += OnTickRelogio;
        _timerRelogio.Start();
        AtualizarRelogio();
    }
    public void Parar() => _timerRelogio?.Stop();
    private void OnTickRelogio(object? sender, EventArgs e) => AtualizarRelogio();
    private void AtualizarRelogio()
    {
        var agora = DateTime.Now;
        HoraAtual = agora.ToString("HH:mm");
        DataAtual = agora.ToString("dddd, dd 'de' MMMM", new CultureInfo("pt-BR"));
    }

    // ---------- Registrar a passagem ----------
    [RelayCommand]
    private async Task RegistrarAsync()
    {
        if (IsBusy)
            return;
        if (CpfDigitos.Length != 11)
        {
            MostrarAcessoNegado("Digite os 11 números do CPF.");
            return;
        }
        try
        {
            IsBusy = true;
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            var cpf = CpfDigitos;

            // 1. Procurar primeiro nos alunos...
            var aluno = await _alunoService.ObterPorCpfAsync(cpf, cts.Token);
            if (aluno != null)
            {
                var acesso = await _acessoAlunoService.RegistrarAcessoAsync(cpf, cts.Token);
                MostrarBemVindo(acesso.Aluno, "Aluno", acesso.DataHora, acesso.Tipo);
                return;
            }
            // 2. ...depois nos colaboradores (a busca é por prefixo, então conferimos o CPF exato)
            var colaborador = (await _colaboradorService.ObterPorCpfAsync(cpf, cts.Token))
                .FirstOrDefault(c => c.Cpf == cpf);
            if (colaborador != null)
            {
                var acesso = await _acessoColaboradorService.RegistrarAcessoAsync(cpf, cts.Token);
                MostrarBemVindo(acesso.Colaborador, "Colaborador", acesso.DataHora, acesso.Tipo);
                return;
            }
            // 3. Não está em nenhum cadastro
            MostrarAcessoNegado("CPF não encontrado no cadastro.");
        }
        catch (OperationCanceledException)
        {
            MostrarAcessoNegado("Tempo esgotado. Verifique a conexão com o banco.");
        }
        catch (InvalidOperationException ex) // regras: sem matrícula, fora do horário...
        {
            MostrarAcessoNegado(ex.Message);
        }
        catch (ArgumentException) // CPF inválido (dígitos verificadores não batem)
        {
            MostrarAcessoNegado("CPF inválido. Confira os números digitados.");
        }
        catch (Exception ex)
        {
            MostrarAcessoNegado($"Erro ao registrar acesso: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
            Cpf = string.Empty; // pronto para a próxima pessoa
        }
    }

    private void MostrarBemVindo(PessoaDto pessoa, string papel, DateTime dataHora, AppAcessoTipo tipo)
    {
        var primeiroNome = pessoa.Nome.Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? pessoa.Nome;
        IsEntrada = tipo == AppAcessoTipo.Entrada;
        Saudacao = IsEntrada ? $"{SaudacaoPorHorario(dataHora)}, {primeiroNome}!" : $"Até logo, {primeiroNome}!";
        Papel = papel;
        Movimento = $"{tipo.GetDisplayName()} · {dataHora:HH:mm}";
        Mensagem = IsEntrada ? (papel == "Aluno" ? "Bom treino!" : "Bom trabalho!") : "Bom descanso!";
        Foto = pessoa.Foto;
        TemFoto = pessoa.Foto?.Conteudo is { Length: > 0 };
        Iniciais = GerarIniciais(pessoa.Nome);
        MostrarNegado = false;
        MostrarBoasVindas = true;
    }

    private void MostrarAcessoNegado(string motivo)
    {
        Mensagem = motivo;
        MostrarBoasVindas = false;
        MostrarNegado = true;
    }

    // ---------- Auxiliares ----------
    private static string SaudacaoPorHorario(DateTime hora) => hora.Hour switch
    {
        < 12 => "Bom dia",
        < 18 => "Boa tarde",
        _ => "Boa noite"
    };

    private static string GerarIniciais(string nome)
    {
        var partes = nome.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (partes.Length == 0) return "?";
        var primeira = partes[0][0];
        var ultima = partes.Length > 1 ? partes[^1][0] : ' ';
        return $"{primeira}{ultima}".Trim().ToUpper();
    }

    private static string FormatarCpf(string? texto)
    {
        var d = new string([.. (texto ?? string.Empty).Where(char.IsDigit)]);
        if (d.Length > 11) d = d[..11];
        return d.Length switch
        {
            <= 3 => d,
            <= 6 => $"{d[..3]}.{d[3..]}",
            <= 9 => $"{d[..3]}.{d[3..6]}.{d[6..]}",
            _ => $"{d[..3]}.{d[3..6]}.{d[6..9]}-{d[9..]}"
        };
    }
}