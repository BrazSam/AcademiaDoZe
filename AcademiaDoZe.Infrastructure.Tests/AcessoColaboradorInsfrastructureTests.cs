using AcademiaDoZe.Domain.Entities; //Samuel Braz dos Santos
using AcademiaDoZe.Infrastructure.Repositories;
namespace AcademiaDoZe.Infrastructure.Tests;

public class AcessoColaboradorInfrastructureTests : TestBase
{
    private readonly LogradouroRepository _logradouroRepo;
    private readonly ColaboradorRepository _colaboradorRepo;
    private readonly AcessoColaboradorRepository _acessoRepo;
    public AcessoColaboradorInfrastructureTests()
    {
        _logradouroRepo = new LogradouroRepository(ConnectionString, DatabaseType);
        _colaboradorRepo = new ColaboradorRepository(ConnectionString, DatabaseType);
        _acessoRepo = new AcessoColaboradorRepository(ConnectionString, DatabaseType);
    }

    private async Task<AcessoColaborador> RegistrarPassagemAsync(Colaborador colaborador, DateTime dataHora)
    {
        var result = AcessoColaborador.Criar(0, colaborador, dataHora);
        if (result.IsFailure)
        {
            throw new Exception($"Falha ao criar AcessoColaborador no Helper: {string.Join(", ", result.Notifications.Select(n => n.Mensagem))}");
        }
        return await _acessoRepo.Adicionar(result.Value!);
    }

    private Task<Colaborador> NovoColaboradorAsync() =>
        ColaboradorInfrastructureTests.CriarEInserirColaboradorAsync(_colaboradorRepo, _logradouroRepo, "Acesso", "Teste", "SenhaTeste123");

    [Fact]
    public async Task AcessoColaborador_Adicionar_E_ObterPorId_Sucesso()
    {
        var colaborador = await NovoColaboradorAsync();
        var dataHora = DateTime.Today.AddHours(8);

        var inserido = await RegistrarPassagemAsync(colaborador, dataHora);
        Assert.True(inserido.Id > 0);

        var obtido = await _acessoRepo.ObterPorId(inserido.Id);
        Assert.NotNull(obtido);
        Assert.Equal(colaborador.Id, obtido.ColaboradorId);
        Assert.Equal(dataHora, obtido.DataHora);
    }

    [Fact]
    public async Task AcessoColaborador_ObterUltimoAcesso_RetornaOMaisRecente()
    {
        var colaborador = await NovoColaboradorAsync();
        await RegistrarPassagemAsync(colaborador, DateTime.Today.AddHours(8));
        var ultimo = await RegistrarPassagemAsync(colaborador, DateTime.Today.AddHours(12));

        var obtido = await _acessoRepo.ObterUltimoAcesso(colaborador.Id);
        Assert.NotNull(obtido);
        Assert.Equal(ultimo.Id, obtido.Id);
    }

    [Fact]
    public async Task AcessoColaborador_ObterAcessosPorColaboradorPeriodo_FiltraPorColaboradorEData()
    {
        var colaborador = await NovoColaboradorAsync();
        await RegistrarPassagemAsync(colaborador, DateTime.Today.AddDays(-1).AddHours(8));
        await RegistrarPassagemAsync(colaborador, DateTime.Today.AddHours(8));

        var hoje = DateOnly.FromDateTime(DateTime.Today);
        var acessos = (await _acessoRepo.ObterAcessosPorColaboradorPeriodo(colaborador.Id, hoje, hoje)).ToList();

        Assert.Single(acessos);
        Assert.Equal(DateTime.Today.AddHours(8), acessos[0].DataHora);
    }

    [Fact]
    public async Task AcessoColaborador_ObterHorasTrabalhadasNoDia_SomaOsParesFechados()
    {
        var colaborador = await NovoColaboradorAsync();
        var dia = DateTime.Today.AddDays(-1);
        await RegistrarPassagemAsync(colaborador, dia.AddHours(8));   // entrada
        await RegistrarPassagemAsync(colaborador, dia.AddHours(12));  // saída  -> 4h
        await RegistrarPassagemAsync(colaborador, dia.AddHours(13));  // entrada
        await RegistrarPassagemAsync(colaborador, dia.AddHours(17));  // saída  -> 4h

        var horas = await _acessoRepo.ObterHorasTrabalhadasNoDia(colaborador.Id, DateOnly.FromDateTime(dia));

        Assert.Equal(TimeSpan.FromHours(8), horas);
    }

    [Fact]
    public async Task AcessoColaborador_ObterHorasTrabalhadasNoDia_IgnoraEntradaSemSaida()
    {
        var colaborador = await NovoColaboradorAsync();
        var dia = DateTime.Today.AddDays(-1);
        await RegistrarPassagemAsync(colaborador, dia.AddHours(8));   // entrada
        await RegistrarPassagemAsync(colaborador, dia.AddHours(12));  // saída  -> 4h
        await RegistrarPassagemAsync(colaborador, dia.AddHours(13));  // entrada sem saída

        var horas = await _acessoRepo.ObterHorasTrabalhadasNoDia(colaborador.Id, DateOnly.FromDateTime(dia));

        Assert.Equal(TimeSpan.FromHours(4), horas);
    }

    [Fact]
    public async Task AcessoColaborador_ObterHorasTrabalhadasNoDia_RetornaZeroSemPassagens()
    {
        var colaborador = await NovoColaboradorAsync();

        var horas = await _acessoRepo.ObterHorasTrabalhadasNoDia(colaborador.Id, DateOnly.FromDateTime(DateTime.Today));

        Assert.Equal(TimeSpan.Zero, horas);
    }

    [Fact]
    public async Task AcessoColaborador_NaoEnxergaAcessoDeAluno()
    {
        // Mesma tabela tb_acesso: o pessoa_tipo precisa separar aluno de colaborador
        var alunoRepo = new AlunoRepository(ConnectionString, DatabaseType);
        var acessoAlunoRepo = new AcessoAlunoRepository(ConnectionString, DatabaseType);
        var aluno = await AlunoInfrastructureTests.CriarEInserirAlunoAsync(alunoRepo, _logradouroRepo, "Acesso", "Teste", "SenhaTeste123");
        var acessoAluno = await acessoAlunoRepo.Adicionar(AcessoAluno.Criar(0, aluno, DateTime.Today.AddHours(8)).Value!);

        Assert.Null(await _acessoRepo.ObterPorId(acessoAluno.Id));
        Assert.False(await _acessoRepo.Remover(acessoAluno.Id));
        Assert.NotNull(await acessoAlunoRepo.ObterPorId(acessoAluno.Id)); // continua lá para o aluno
    }

    [Fact]
    public async Task AcessoColaborador_Remover_Sucesso()
    {
        var colaborador = await NovoColaboradorAsync();
        var inserido = await RegistrarPassagemAsync(colaborador, DateTime.Today.AddHours(8));

        Assert.True(await _acessoRepo.Remover(inserido.Id));
        Assert.Null(await _acessoRepo.ObterPorId(inserido.Id));
    }
}