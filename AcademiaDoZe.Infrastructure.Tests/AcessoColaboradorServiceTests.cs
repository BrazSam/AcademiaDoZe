using AcademiaDoZe.Application.Enums; //Samuel Braz dos Santos
using AcademiaDoZe.Application.Services;
using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Infrastructure.Repositories;
namespace AcademiaDoZe.Infrastructure.Tests;

public class AcessoColaboradorServiceTests : TestBase
{
    // Relógio parado num horário escolhido pelo teste
    private sealed class RelogioFixo(DateTime agora) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => new DateTimeOffset(agora).ToUniversalTime();
    }

    private AcessoColaboradorService CriarServico(DateTime agora) => new(
        () => new AcessoColaboradorRepository(ConnectionString, DatabaseType),
        () => new ColaboradorRepository(ConnectionString, DatabaseType),
        new RelogioFixo(agora));

    private Task<Colaborador> NovoColaboradorAsync() =>
        ColaboradorInfrastructureTests.CriarEInserirColaboradorAsync(
            new ColaboradorRepository(ConnectionString, DatabaseType),
            new LogradouroRepository(ConnectionString, DatabaseType),
            "Acesso", "Servico", "SenhaTeste123");

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
    public async Task RegistrarAcesso_RegistraEntradaDepoisSaida()
    {
        var colaborador = await NovoColaboradorAsync();

        var entrada = await CriarServico(DateTime.Today.AddHours(8)).RegistrarAcessoAsync(colaborador.Cpf.Valor);
        Assert.Equal(AppAcessoTipo.Entrada, entrada.Tipo);
        Assert.Equal(colaborador.Nome, entrada.Colaborador.Nome);
        Assert.Equal(DateTime.Today.AddHours(8), entrada.DataHora);
        Assert.True(entrada.Id > 0);

        var saida = await CriarServico(DateTime.Today.AddHours(12)).RegistrarAcessoAsync(colaborador.Cpf.Valor);
        Assert.Equal(AppAcessoTipo.Saida, saida.Tipo);
    }

    [Fact]
    public async Task RegistrarAcesso_EntradaDeOntemSemSaida_NaoViraSaidaHoje()
    {
        var colaborador = await NovoColaboradorAsync();
        var ontem = DateTime.Today.AddDays(-1);

        await CriarServico(ontem.AddHours(8)).RegistrarAcessoAsync(colaborador.Cpf.Valor); // esqueceu de sair ontem

        var hoje = await CriarServico(DateTime.Today.AddHours(8)).RegistrarAcessoAsync(colaborador.Cpf.Valor);
        Assert.Equal(AppAcessoTipo.Entrada, hoje.Tipo);
    }

    [Fact]
    public async Task RegistrarAcesso_ForaDoHorario_Falha()
    {
        var colaborador = await NovoColaboradorAsync();
        var servico = CriarServico(DateTime.Today.AddHours(5));

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => servico.RegistrarAcessoAsync(colaborador.Cpf.Valor));
        Assert.Contains("06h e 22h", ex.Message);
    }
}