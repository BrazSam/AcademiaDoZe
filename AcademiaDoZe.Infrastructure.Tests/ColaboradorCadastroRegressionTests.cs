using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Application.Enums;
using AcademiaDoZe.Application.Mappings;
using AcademiaDoZe.Application.Security;
using AcademiaDoZe.Application.Services;
using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.Enums;
using AcademiaDoZe.Infrastructure.Data;
using AcademiaDoZe.Infrastructure.Repositories;

namespace AcademiaDoZe.Infrastructure.Tests;

// Banco isolado: estes testes não usam o servidor nem os dados do aplicativo.
public sealed class ColaboradorCadastroRegressionTests : IDisposable
{
    private readonly string _arquivo = Path.Combine(Path.GetTempPath(), $"academia-colaborador-{Guid.NewGuid():N}.db");
    private string Conexao => $"Data Source={_arquivo};Pooling=False;Foreign Keys=True";
    private ColaboradorRepository Repo() => new(Conexao, DatabaseType.Sqlite);
    private LogradouroRepository LogradouroRepo() => new(Conexao, DatabaseType.Sqlite);
    private ColaboradorService Service() => new(() => Repo(), () => LogradouroRepo());

    private async Task<ColaboradorDto> NovoAsync()
    {
        using var repo = LogradouroRepo();
        var logradouro = await repo.Adicionar(Logradouro.Criar(0, "88501001", "Rua Teste", "Centro", "Lages", "SC", "Brasil").Value!);
        return new ColaboradorDto
        {
            Nome = "Colaborador Teste", Cpf = "529.982.247-25",
            DataNascimento = new DateOnly(1995, 1, 1), Telefone = "49999999999",
            Email = "colaborador@teste.com", Numero = "123", Senha = "SenhaTeste123",
            Endereco = logradouro.ToDto(), DataAdmissao = DateOnly.FromDateTime(DateTime.Today),
            Tipo = AppColaboradorTipo.Atendente, Vinculo = AppColaboradorVinculo.CLT
        };
    }

    [Theory]
    [InlineData(AppColaboradorTipo.Administrador, ColaboradorTipo.Administrador)]
    [InlineData(AppColaboradorTipo.Atendente, ColaboradorTipo.Atendente)]
    [InlineData(AppColaboradorTipo.Instrutor, ColaboradorTipo.Instrutor)]
    public void Tipos_PreservamSignificadoNasDuasDirecoes(AppColaboradorTipo app, ColaboradorTipo dominio)
    {
        Assert.Equal(dominio, app.ToDomain());
        Assert.Equal(app, dominio.ToApplication());
    }

    [Theory]
    [InlineData(AppColaboradorVinculo.CLT, ColaboradorVinculo.CLT)]
    [InlineData(AppColaboradorVinculo.Estagio, ColaboradorVinculo.Estagio)]
    public void Vinculos_PreservamSignificadoNasDuasDirecoes(AppColaboradorVinculo app, ColaboradorVinculo dominio)
    {
        Assert.Equal(dominio, app.ToDomain());
        Assert.Equal(app, dominio.ToApplication());
    }

    [Theory]
    [InlineData(AppColaboradorTipo.Administrador, AppColaboradorVinculo.CLT)]
    [InlineData(AppColaboradorTipo.Atendente, AppColaboradorVinculo.CLT)]
    [InlineData(AppColaboradorTipo.Instrutor, AppColaboradorVinculo.CLT)]
    [InlineData(AppColaboradorTipo.Atendente, AppColaboradorVinculo.Estagio)]
    [InlineData(AppColaboradorTipo.Instrutor, AppColaboradorVinculo.Estagio)]
    public async Task Cadastro_PersisteTipoVinculoEHashSemAlterarSenhaDoFormulario(AppColaboradorTipo tipo, AppColaboradorVinculo vinculo)
    {
        var dto = await NovoAsync();
        dto.Tipo = tipo;
        dto.Vinculo = vinculo;
        var salvo = await Service().AdicionarAsync(dto);
        Assert.True(salvo.Id > 0);
        Assert.Null(salvo.Senha);
        Assert.Equal("SenhaTeste123", dto.Senha);
        using var repo = Repo();
        var entidade = (await repo.ObterPorId(salvo.Id))!;
        Assert.Equal(tipo.ToDomain(), entidade.Tipo);
        Assert.Equal(vinculo.ToDomain(), entidade.Vinculo);
        Assert.StartsWith("ARGON2ID:", entidade.Senha.Valor);
        Assert.True(PasswordHasher.Verify(dto.Senha!, entidade.Senha.Valor));
    }

    [Fact]
    public async Task FalhaAposHash_PermiteCorrigirEnderecoETentarNovamenteComMesmaSenha()
    {
        var dto = await NovoAsync();
        var endereco = dto.Endereco!;
        dto.Endereco = new LogradouroDto { Id = int.MaxValue, Cep = endereco.Cep, Nome = endereco.Nome, Bairro = endereco.Bairro, Cidade = endereco.Cidade, Estado = endereco.Estado, Pais = endereco.Pais };
        await Assert.ThrowsAsync<KeyNotFoundException>(() => Service().AdicionarAsync(dto));
        Assert.Equal("SenhaTeste123", dto.Senha);
        dto.Endereco = endereco;
        Assert.True((await Service().AdicionarAsync(dto)).Id > 0);
    }

    [Fact]
    public async Task BuscaParcial_RetornaColecaoComEnderecoESemSenha()
    {
        var dto = await NovoAsync();
        var salvo = await Service().AdicionarAsync(dto);
        var encontrado = Assert.Single(await Service().ObterPorCpfAsync("529.982"));
        Assert.Equal(salvo.Id, encontrado.Id);
        Assert.Equal(dto.Endereco!.Id, encontrado.Endereco!.Id);
        Assert.Equal(dto.Endereco.Cep, encontrado.Endereco.Cep);
        Assert.Null(encontrado.Senha);
        Assert.Empty(await Service().ObterPorCpfAsync("00000000000"));
    }

    [Fact]
    public async Task BuscaParcial_PermiteMultiplosResultados()
    {
        var dto = await NovoAsync();
        var primeiro = await Service().AdicionarAsync(dto);
        dto.Cpf = "52998224806";
        dto.Email = "outro@teste.com";
        var segundo = await Service().AdicionarAsync(dto);
        var resultados = (await Service().ObterPorCpfAsync("52998224")).ToList();
        Assert.Equal(2, resultados.Count);
        Assert.Contains(resultados, c => c.Id == primeiro.Id);
        Assert.Contains(resultados, c => c.Id == segundo.Id);
        Assert.All(resultados, c => Assert.NotNull(c.Endereco));
    }

    [Fact]
    public async Task Cadastro_ContinuaRejeitandoAdministradorEstagiario()
    {
        var dto = await NovoAsync();
        dto.Tipo = AppColaboradorTipo.Administrador;
        dto.Vinculo = AppColaboradorVinculo.Estagio;
        var erro = await Assert.ThrowsAsync<InvalidOperationException>(() => Service().AdicionarAsync(dto));
        Assert.Contains("ADMINISTRADOR_CLT_INVALIDO", erro.Message);
    }

    [Fact]
    public async Task Edicao_PermiteAdministradorCltEPreservaSenhaQuandoVazia()
    {
        var dto = await NovoAsync();
        dto.Vinculo = AppColaboradorVinculo.Estagio;
        var salvo = await Service().AdicionarAsync(dto);
        using var repo = Repo();
        var hash = (await repo.ObterPorId(salvo.Id))!.Senha.Valor;
        salvo.Tipo = AppColaboradorTipo.Administrador;
        salvo.Vinculo = AppColaboradorVinculo.CLT;
        salvo.Senha = "";
        salvo.Cpf = "11144477735";
        var atualizado = await Service().AtualizarAsync(salvo);
        Assert.Equal(AppColaboradorTipo.Administrador, atualizado.Tipo);
        Assert.Equal(AppColaboradorVinculo.CLT, atualizado.Vinculo);
        Assert.Equal(salvo.Cpf, atualizado.Cpf);
        Assert.Equal(hash, (await repo.ObterPorId(salvo.Id))!.Senha.Valor);
    }

    [Fact]
    public async Task EdicaoComNovaSenha_NaoAlteraSenhaDoFormulario()
    {
        var salvo = await Service().AdicionarAsync(await NovoAsync());
        salvo.Senha = "NovaSenha123";
        await Service().AtualizarAsync(salvo);
        Assert.Equal("NovaSenha123", salvo.Senha);
        using var repo = Repo();
        Assert.True(PasswordHasher.Verify(salvo.Senha, (await repo.ObterPorId(salvo.Id))!.Senha.Valor));
    }

    [Theory]
    [InlineData("11111111111")]
    [InlineData("52998224724")]
    public async Task Cadastro_RejeitaCpfInvalido(string cpf)
    {
        var dto = await NovoAsync();
        dto.Cpf = cpf;
        await Assert.ThrowsAsync<ArgumentException>(() => Service().AdicionarAsync(dto));
    }

    [Fact]
    public async Task Cadastro_RejeitaCpfDuplicado()
    {
        var dto = await NovoAsync();
        await Service().AdicionarAsync(dto);
        await Assert.ThrowsAsync<InvalidOperationException>(() => Service().AdicionarAsync(dto));
    }

    public void Dispose()
    {
        if (File.Exists(_arquivo)) File.Delete(_arquivo);
    }
}
