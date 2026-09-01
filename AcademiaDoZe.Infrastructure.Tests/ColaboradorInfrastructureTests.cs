using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.Enums;
using AcademiaDoZe.Domain.ValueObjects;
using AcademiaDoZe.Infrastructure.Data;
using AcademiaDoZe.Infrastructure.Repositories;
using AcademiaDoZe.Infrastructure.Tests;

public class ColaboradorInfrastructureTests : TestBase
{
    // Requisitos do enunciado
    private const string NomeTeste = "Samuel";            // seu nome
    private const string SobrenomeTeste = "Braz"; // seu sobrenome

    private readonly LogradouroRepository _logradouroRepo;
    private readonly ColaboradorRepository _colaboradorRepo;

    public ColaboradorInfrastructureTests()
    {
        _logradouroRepo = new LogradouroRepository(ConnectionString, DatabaseType);
        _colaboradorRepo = new ColaboradorRepository(ConnectionString, DatabaseType);
    }

    private string SenhaSgbd() => $"Senha{ObterSiglaSgbd()}123"; // ex: SenhaSQLServer123

    internal static async Task<Colaborador> CriarEInserirColaboradorAsync(ColaboradorRepository colaboradorRepo, LogradouroRepository logradouroRepo, string nome, string sobrenome, string senha)
    {
        var logradouro = await LogradouroInfrastructureTests.CriarEInserirLogradouroAsync(logradouroRepo);
        var foto = Arquivo.Criar(new byte[] { 5, 6, 7, 8 }).Value!;
        var colaboradorResult = Colaborador.Criar(
            id: 0,
            nome: nome,
            cpf: GerarCpf(),
            dataNascimento: new DateOnly(1995, 5, 15),
            telefone: GerarTelefone(),
            email: GerarEmail(),
            endereco: logradouro,
            numero: "200",
            complemento: sobrenome,
            senha: senha,
            foto: foto,
            dataAdmissao: new DateOnly(2023, 1, 1),
            tipo: ColaboradorTipo.Instrutor,
            vinculo: ColaboradorVinculo.CLT
        );
        if (colaboradorResult.IsFailure)
        {
            throw new Exception($"Falha ao criar Colaborador: {string.Join(", ", colaboradorResult.Notifications.Select(n => n.Mensagem))}");
        }
        return await colaboradorRepo.Adicionar(colaboradorResult.Value!);
    }
    // ... o restante dos [Fact] continua igual, mas chame o helper passando NomeTeste, SobrenomeTeste, SenhaSgbd()
}