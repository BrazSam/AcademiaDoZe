using AcademiaDoZe.Domain.Entities; //Samuel Braz dos Santos
using AcademiaDoZe.Infrastructure.Repositories;
namespace AcademiaDoZe.Infrastructure.Tests;

public class AcessoAlunoInfrastructureTests : TestBase
{
    private readonly LogradouroRepository _logradouroRepo;
    private readonly AlunoRepository _alunoRepo;
    private readonly AcessoAlunoRepository _acessoRepo;
    public AcessoAlunoInfrastructureTests()
    {
        _logradouroRepo = new LogradouroRepository(ConnectionString, DatabaseType);
        _alunoRepo = new AlunoRepository(ConnectionString, DatabaseType);
        _acessoRepo = new AcessoAlunoRepository(ConnectionString, DatabaseType);
    }

    private async Task<AcessoAluno> RegistrarPassagemAsync(Aluno aluno, DateTime dataHora)
    {
        var result = AcessoAluno.Criar(0, aluno, dataHora);
        if (result.IsFailure)
        {
            throw new Exception($"Falha ao criar AcessoAluno no Helper: {string.Join(", ", result.Notifications.Select(n => n.Mensagem))}");
        }
        return await _acessoRepo.Adicionar(result.Value!);
    }

    private Task<Aluno> NovoAlunoAsync() =>
        AlunoInfrastructureTests.CriarEInserirAlunoAsync(_alunoRepo, _logradouroRepo, "Acesso", "Teste", "SenhaTeste123");

    [Fact]
    public async Task AcessoAluno_Adicionar_E_ObterPorId_Sucesso()
    {
        var aluno = await NovoAlunoAsync();
        var dataHora = DateTime.Today.AddHours(8);

        var inserido = await RegistrarPassagemAsync(aluno, dataHora);
        Assert.True(inserido.Id > 0);

        var obtido = await _acessoRepo.ObterPorId(inserido.Id);
        Assert.NotNull(obtido);
        Assert.Equal(aluno.Id, obtido.AlunoId);
        Assert.Equal(dataHora, obtido.DataHora);
    }

    [Fact]
    public async Task AcessoAluno_EstaNaAcademia_AlternaEntreEntradaESaida()
    {
        var aluno = await NovoAlunoAsync();
        Assert.False(await _acessoRepo.EstaNaAcademia(aluno.Id)); // nenhuma passagem

        await RegistrarPassagemAsync(aluno, DateTime.Today.AddHours(8));  // 1ª = entrada
        Assert.True(await _acessoRepo.EstaNaAcademia(aluno.Id));

        await RegistrarPassagemAsync(aluno, DateTime.Today.AddHours(9));  // 2ª = saída
        Assert.False(await _acessoRepo.EstaNaAcademia(aluno.Id));
    }

    [Fact]
    public async Task AcessoAluno_ObterUltimoAcesso_RetornaOMaisRecente()
    {
        var aluno = await NovoAlunoAsync();
        await RegistrarPassagemAsync(aluno, DateTime.Today.AddHours(8));
        var ultimo = await RegistrarPassagemAsync(aluno, DateTime.Today.AddHours(10));

        var obtido = await _acessoRepo.ObterUltimoAcesso(aluno.Id);
        Assert.NotNull(obtido);
        Assert.Equal(ultimo.Id, obtido.Id);
    }

    [Fact]
    public async Task AcessoAluno_ObterUltimoAcesso_RetornaNullSemAcessos()
    {
        var aluno = await NovoAlunoAsync();
        Assert.Null(await _acessoRepo.ObterUltimoAcesso(aluno.Id));
    }

    [Fact]
    public async Task AcessoAluno_ObterAcessosPorAlunoPeriodo_FiltraPorAlunoEData()
    {
        var aluno = await NovoAlunoAsync();
        var ontem = DateTime.Today.AddDays(-1);
        await RegistrarPassagemAsync(aluno, ontem.AddHours(8));
        await RegistrarPassagemAsync(aluno, DateTime.Today.AddHours(8));

        var hoje = DateOnly.FromDateTime(DateTime.Today);
        var acessos = (await _acessoRepo.ObterAcessosPorAlunoPeriodo(aluno.Id, hoje, hoje)).ToList();

        Assert.Single(acessos);
        Assert.Equal(DateTime.Today.AddHours(8), acessos[0].DataHora);
    }

    [Fact]
    public async Task AcessoAluno_Remover_Sucesso()
    {
        var aluno = await NovoAlunoAsync();
        var inserido = await RegistrarPassagemAsync(aluno, DateTime.Today.AddHours(8));

        Assert.True(await _acessoRepo.Remover(inserido.Id));
        Assert.Null(await _acessoRepo.ObterPorId(inserido.Id));
    }

    [Fact]
    public async Task AcessoAluno_Remover_RetornaFalseQuandoInexistente()
    {
        Assert.False(await _acessoRepo.Remover(999999));
    }
}