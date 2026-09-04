using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.ValueObjects;
using AcademiaDoZe.Infrastructure.Exceptions;
using AcademiaDoZe.Infrastructure.Repositories;

namespace AcademiaDoZe.Infrastructure.Tests;

public class AlunoInfrastructureTests : TestBase
{
    // Requisitos do enunciado
    private const string NomeTeste = "Samuel";          // seu nome
    private const string SobrenomeTeste = "Braz"; // seu sobrenome

    private readonly LogradouroRepository _logradouroRepo;
    private readonly AlunoRepository _alunoRepo;

    public AlunoInfrastructureTests()
    {
        _logradouroRepo = new LogradouroRepository(ConnectionString, DatabaseType);
        _alunoRepo = new AlunoRepository(ConnectionString, DatabaseType);
    }

    private string SenhaSgbd() => $"Senha{ObterSiglaSgbd()}123"; // ex: SenhaSQLite123

    internal static async Task<Aluno> CriarEInserirAlunoAsync(AlunoRepository alunoRepo, LogradouroRepository logradouroRepo, string nome, string sobrenome, string senha)
    {
        var logradouro = await LogradouroInfrastructureTests.CriarEInserirLogradouroAsync(logradouroRepo);
        var foto = Arquivo.Criar(new byte[] { 5, 6, 7, 8 }).Value!;
        var alunoResult = Aluno.Criar(
            id: 0,
            nome: nome,
            cpf: GerarCpf(),
            dataNascimento: new DateOnly(2005, 3, 10),
            telefone: GerarTelefone(),
            email: GerarEmail(),
            endereco: logradouro,
            numero: "100",
            complemento: sobrenome,
            senha: senha,
            foto: foto
        );
        if (alunoResult.IsFailure)
        {
            throw new Exception($"Falha ao criar Aluno: {string.Join(", ", alunoResult.Notifications.Select(n => n.Mensagem))}");
        }
        return await alunoRepo.Adicionar(alunoResult.Value!);
    }

    [Fact]
    public async Task Aluno_Adicionar_E_ObterPorId_Sucesso()
    {
        var aluno = await CriarEInserirAlunoAsync(_alunoRepo, _logradouroRepo, NomeTeste, SobrenomeTeste, SenhaSgbd());
        Assert.NotNull(aluno);
        Assert.True(aluno.Id > 0);
        Assert.Equal(NomeTeste, aluno.Nome);
        Assert.Equal(SobrenomeTeste, aluno.Endereco.Complemento);
        Assert.Equal(SenhaSgbd(), aluno.Senha.Valor);

        var obtido = await _alunoRepo.ObterPorId(aluno.Id);
        Assert.NotNull(obtido);
        Assert.Equal(aluno.Id, obtido.Id);
        Assert.Equal(aluno.Cpf.Valor, obtido.Cpf.Valor);
    }

    [Fact]
    public async Task Aluno_ObterPorId_RetornaNuloQuandoInexistente()
    {
        var obtido = await _alunoRepo.ObterPorId(999999);
        Assert.Null(obtido);
    }

    [Fact]
    public async Task Aluno_ObterTodos_Sucesso()
    {
        await CriarEInserirAlunoAsync(_alunoRepo, _logradouroRepo, NomeTeste, SobrenomeTeste, SenhaSgbd());
        var todos = await _alunoRepo.ObterTodos();
        Assert.NotNull(todos);
        Assert.NotEmpty(todos);
    }

    [Fact]
    public async Task Aluno_Atualizar_Sucesso()
    {
        var logradouro = await LogradouroInfrastructureTests.CriarEInserirLogradouroAsync(_logradouroRepo);
        var aluno = await CriarEInserirAlunoAsync(_alunoRepo, _logradouroRepo, NomeTeste, SobrenomeTeste, SenhaSgbd());
        var novoNome = "Samuel Editado";
        var atualizado = await _alunoRepo.Atualizar(Aluno.Criar(
            aluno.Id, novoNome, aluno.Cpf.Valor, aluno.DataNascimento, aluno.Telefone.Valor,
            aluno.Email.Valor, logradouro, "200", SobrenomeTeste, SenhaSgbd(), aluno.Foto).Value!);
        Assert.Equal(novoNome, atualizado.Nome);
        var noBanco = await _alunoRepo.ObterPorId(aluno.Id);
        Assert.Equal(novoNome, noBanco!.Nome);
    }

    [Fact]
    public async Task Aluno_Atualizar_LancaExcecaoQuandoInexistente()
    {
        var logradouro = await LogradouroInfrastructureTests.CriarEInserirLogradouroAsync(_logradouroRepo);
        var inexistente = Aluno.Criar(999999, "Inexistente", GerarCpf(), new DateOnly(2000, 1, 1),
            GerarTelefone(), GerarEmail(), logradouro, "1", "", SenhaSgbd(), Arquivo.Criar(new byte[] { 1 }).Value!).Value!;
        var ex = await Assert.ThrowsAsync<InfrastructureException>(() => _alunoRepo.Atualizar(inexistente));
        Assert.Equal("REGISTRO_NAO_ENCONTRADO", ex.ErrorCode);
    }

    [Fact]
    public async Task Aluno_Remover_Sucesso()
    {
        var aluno = await CriarEInserirAlunoAsync(_alunoRepo, _logradouroRepo, NomeTeste, SobrenomeTeste, SenhaSgbd());
        Assert.True(await _alunoRepo.Remover(aluno.Id));
        Assert.Null(await _alunoRepo.ObterPorId(aluno.Id));
    }

    [Fact]
    public async Task Aluno_Remover_RetornaFalseQuandoInexistente()
    {
        Assert.False(await _alunoRepo.Remover(999999));
    }

    [Fact]
    public async Task Aluno_ObterPorCpf_SucessoENulo()
    {
        var aluno = await CriarEInserirAlunoAsync(_alunoRepo, _logradouroRepo, NomeTeste, SobrenomeTeste, SenhaSgbd());
        var obtido = await _alunoRepo.ObterPorCpf(aluno.Cpf);
        Assert.NotNull(obtido);
        Assert.Equal(aluno.Id, obtido.Id);
        Assert.Null(await _alunoRepo.ObterPorCpf(Cpf.Criar(GerarCpf()).Value!));
    }

    [Fact]
    public async Task Aluno_ObterPorEmail_SucessoENulo()
    {
        var aluno = await CriarEInserirAlunoAsync(_alunoRepo, _logradouroRepo, NomeTeste, SobrenomeTeste, SenhaSgbd());
        var obtido = await _alunoRepo.ObterPorEmail(aluno.Email);
        Assert.NotNull(obtido);
        Assert.Equal(aluno.Id, obtido.Id);
        Assert.Null(await _alunoRepo.ObterPorEmail(Email.Criar(GerarEmail()).Value!));
    }

    [Fact]
    public async Task Aluno_CpfJaExiste_ValidacaoCorreta()
    {
        var aluno = await CriarEInserirAlunoAsync(_alunoRepo, _logradouroRepo, NomeTeste, SobrenomeTeste, SenhaSgbd());
        Assert.True(await _alunoRepo.CpfJaExiste(aluno.Cpf));
        Assert.False(await _alunoRepo.CpfJaExiste(aluno.Cpf, aluno.Id));
        Assert.False(await _alunoRepo.CpfJaExiste(Cpf.Criar(GerarCpf()).Value!));
    }

    [Fact]
    public async Task Aluno_EmailJaExiste_ValidacaoCorreta()
    {
        var aluno = await CriarEInserirAlunoAsync(_alunoRepo, _logradouroRepo, NomeTeste, SobrenomeTeste, SenhaSgbd());
        Assert.True(await _alunoRepo.EmailJaExiste(aluno.Email));
        Assert.False(await _alunoRepo.EmailJaExiste(aluno.Email, aluno.Id));
        Assert.False(await _alunoRepo.EmailJaExiste(Email.Criar(GerarEmail()).Value!));
    }

    [Fact]
    public async Task Aluno_ObterPorNome_FiltragemCorreta()
    {
        var aluno = await CriarEInserirAlunoAsync(_alunoRepo, _logradouroRepo, NomeTeste, SobrenomeTeste, SenhaSgbd());
        var resultados = await _alunoRepo.ObterPorNome(NomeTeste);
        Assert.Contains(resultados, a => a.Id == aluno.Id);
    }

    [Fact]
    public async Task Aluno_TrocarSenha_SucessoEFalha()
    {
        var aluno = await CriarEInserirAlunoAsync(_alunoRepo, _logradouroRepo, NomeTeste, SobrenomeTeste, SenhaSgbd());
        var novaSenha = Senha.Criar("NovaSenhaAluno123").Value!;
        Assert.True(await _alunoRepo.TrocarSenha(aluno.Id, novaSenha));
        Assert.Equal("NovaSenhaAluno123", (await _alunoRepo.ObterPorId(aluno.Id))!.Senha.Valor);
        Assert.False(await _alunoRepo.TrocarSenha(999999, novaSenha));
    }
}