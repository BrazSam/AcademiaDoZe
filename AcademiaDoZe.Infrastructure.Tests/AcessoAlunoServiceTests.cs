using AcademiaDoZe.Application.Enums; //Samuel Braz dos Santos
using AcademiaDoZe.Application.Services;
using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.Enums;
using AcademiaDoZe.Infrastructure.Repositories;
namespace AcademiaDoZe.Infrastructure.Tests;

public class AcessoAlunoServiceTests : TestBase
{
    // Relógio parado num horário escolhido pelo teste
    private sealed class RelogioFixo(DateTime agora) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => new DateTimeOffset(agora).ToUniversalTime();
    }

    private AcessoAlunoService CriarServico(DateTime agora) => new(
        () => new AcessoAlunoRepository(ConnectionString, DatabaseType),
        () => new AlunoRepository(ConnectionString, DatabaseType),
        () => new MatriculaRepository(ConnectionString, DatabaseType),
        new RelogioFixo(agora));

    private async Task<Aluno> NovoAlunoAsync(bool comMatricula)
    {
        var aluno = await AlunoInfrastructureTests.CriarEInserirAlunoAsync(
            new AlunoRepository(ConnectionString, DatabaseType),
            new LogradouroRepository(ConnectionString, DatabaseType),
            "Acesso", "Servico", "SenhaTeste123");
        if (comMatricula)
        {
            var matricula = Matricula.Criar(0, aluno, MatriculaPlano.Mensal, DateOnly.FromDateTime(DateTime.Today),
                "Condicionamento Físico", MatriculaRestricoes.None, null).Value!;
            await new MatriculaRepository(ConnectionString, DatabaseType).Adicionar(matricula);
        }
        return aluno;
    }

    [Fact]
    public async Task RegistrarAcesso_CpfNaoCadastrado_Falha()
    {
        var servico = CriarServico(DateTime.Today.AddHours(10));
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => servico.RegistrarAcessoAsync(GerarCpf()));
        Assert.Contains("não encontrado", ex.Message);
    }

    [Fact]
    public async Task RegistrarAcesso_CpfInvalido_Falha()
    {
        var servico = CriarServico(DateTime.Today.AddHours(10));
        await Assert.ThrowsAsync<ArgumentException>(() => servico.RegistrarAcessoAsync("123"));
    }

    [Fact]
    public async Task RegistrarAcesso_AlunoSemMatricula_BloqueiaEntradaENaoGrava()
    {
        var aluno = await NovoAlunoAsync(comMatricula: false);
        var servico = CriarServico(DateTime.Today.AddHours(10));

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => servico.RegistrarAcessoAsync(aluno.Cpf.Valor));
        Assert.Contains("sem matrícula ativa", ex.Message);

        var acessoRepo = new AcessoAlunoRepository(ConnectionString, DatabaseType);
        Assert.Null(await acessoRepo.ObterUltimoAcesso(aluno.Id)); // nada foi gravado
    }

    [Fact]
    public async Task RegistrarAcesso_AlunoComMatricula_RegistraEntradaDepoisSaida()
    {
        var aluno = await NovoAlunoAsync(comMatricula: true);

        var entrada = await CriarServico(DateTime.Today.AddHours(10)).RegistrarAcessoAsync(aluno.Cpf.Valor);
        Assert.Equal(AppAcessoTipo.Entrada, entrada.Tipo);
        Assert.Equal(aluno.Nome, entrada.Aluno.Nome);
        Assert.Equal(DateTime.Today.AddHours(10), entrada.DataHora);
        Assert.True(entrada.Id > 0);

        var saida = await CriarServico(DateTime.Today.AddHours(11)).RegistrarAcessoAsync(aluno.Cpf.Valor);
        Assert.Equal(AppAcessoTipo.Saida, saida.Tipo);
    }

    [Fact]
    public async Task RegistrarAcesso_ForaDoHorario_Falha()
    {
        var aluno = await NovoAlunoAsync(comMatricula: true);
        var servico = CriarServico(DateTime.Today.AddHours(23));

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => servico.RegistrarAcessoAsync(aluno.Cpf.Valor));
        Assert.Contains("06h e 22h", ex.Message);
    }
}