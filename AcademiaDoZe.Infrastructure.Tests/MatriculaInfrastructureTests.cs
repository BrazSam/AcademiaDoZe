using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.Enums;
using AcademiaDoZe.Domain.ValueObjects;
using AcademiaDoZe.Infrastructure.Exceptions;
using AcademiaDoZe.Infrastructure.Repositories;
namespace AcademiaDoZe.Infrastructure.Tests;

public class MatriculaInfrastructureTests : TestBase
{
    private readonly LogradouroRepository _logradouroRepo;
    private readonly AlunoRepository _alunoRepo;
    private readonly MatriculaRepository _matriculaRepo;
    public MatriculaInfrastructureTests()
    {
        _logradouroRepo = new LogradouroRepository(ConnectionString, DatabaseType);
        _alunoRepo = new AlunoRepository(ConnectionString, DatabaseType);
        _matriculaRepo = new MatriculaRepository(ConnectionString, DatabaseType);
    }
    private async Task<Matricula> CriarEInserirMatriculaAsync(
    Aluno aluno,
    MatriculaPlano plano = MatriculaPlano.Mensal,
    DateOnly? dataInicio = null,
    MatriculaRestricoes restricoes = MatriculaRestricoes.None,
    string obsRestricao = "",
    Arquivo? laudo = null)
    {
        var inicio = dataInicio ?? DateOnly.FromDateTime(DateTime.Today);
        if (restricoes != MatriculaRestricoes.None && laudo == null)
        {
            laudo = Arquivo.Criar(new byte[] { 1, 2, 3, 4 }).Value;
        }
        var matriculaResult = Matricula.Criar(
        id: 0, aluno: aluno, plano: plano, dataInicio: inicio, objetivo: "Condicionamento Físico", restricoesMedicas: restricoes, laudoMedico: laudo, observacoesRestricoes: obsRestricao);
        if (matriculaResult.IsFailure)
        {
            throw new Exception($"Falha ao criar Matricula no Helper: {string.Join(", ", matriculaResult.Notifications.Select(n => n.Mensagem))}");
        }
        return await _matriculaRepo.Adicionar(matriculaResult.Value!);
    }
    // MÉTODOS DOS TESTES AQUI

    [Fact]
    public async Task Matricula_Adicionar_E_ObterPorId_Sucesso()
    {
        var aluno = await AlunoInfrastructureTests.CriarEInserirAlunoAsync(_alunoRepo, _logradouroRepo, "Samuel", "Braz", "SenhaTeste123");
        var restricoesComb = MatriculaRestricoes.Diabetes | MatriculaRestricoes.PressaoAlta;
        var laudo = Arquivo.Criar(new byte[] { 100, 101, 102 }).Value;
        var inserida = await CriarEInserirMatriculaAsync(
        aluno: aluno,
        plano: MatriculaPlano.Mensal,
        restricoes: restricoesComb,
        obsRestricao: "Usar medicação ao acordar",
        laudo: laudo
        );
        Assert.NotNull(inserida);
        Assert.True(inserida.Id > 0);
        Assert.Equal(aluno.Id, inserida.AlunoId);
        Assert.Equal(MatriculaPlano.Mensal, inserida.Plano);
        Assert.Equal(restricoesComb, inserida.RestricoesMedicas);
        Assert.True(inserida.RestricoesMedicas.HasFlag(MatriculaRestricoes.Diabetes));
        Assert.True(inserida.RestricoesMedicas.HasFlag(MatriculaRestricoes.PressaoAlta));
        var obtida = await _matriculaRepo.ObterPorId(inserida.Id);
        Assert.NotNull(obtida);
        Assert.Equal(inserida.Id, obtida.Id);
        Assert.Equal(aluno.Id, obtida.AlunoId);
        Assert.Equal(MatriculaPlano.Mensal, obtida.Plano);
        Assert.Equal(restricoesComb, obtida.RestricoesMedicas);
        Assert.Equal("Usar medicação ao acordar", obtida.ObservacoesRestricoes);
        Assert.NotNull(obtida.LaudoMedico);
        Assert.Equal(laudo!.Conteudo, obtida.LaudoMedico.Conteudo);
    }

    [Fact]
    public async Task Matricula_RestricoesMedicas_ComVariacoesMultiplaEscolha_PersisteEObtemCorretamente()
    {
        var aluno = await AlunoInfrastructureTests.CriarEInserirAlunoAsync(_alunoRepo, _logradouroRepo, "Samuel", "Braz", "SenhaTeste123");
        var laudo = Arquivo.Criar(new byte[] { 10, 20, 30, 40, 50 }).Value!;
        // Combinação de Múltipla Escolha: Pressão Alta + Labirintite + Problemas Respiratórios + Remédio Contínuo
        var restricoesMultiplas = MatriculaRestricoes.PressaoAlta
        | MatriculaRestricoes.Labirintite
        | MatriculaRestricoes.ProblemasRespiratorios
        | MatriculaRestricoes.RemedioContinuo;
        var matricula = await CriarEInserirMatriculaAsync(
        aluno: aluno,
        plano: MatriculaPlano.Semestral,
        restricoes: restricoesMultiplas,
        obsRestricao: "Evitar exercícios de alto impacto e hipertensão",
        laudo: laudo
        );
        var obtida = await _matriculaRepo.ObterPorId(matricula.Id);
        Assert.NotNull(obtida);
        Assert.Equal(restricoesMultiplas, obtida.RestricoesMedicas);
        // Verificação Bitwise das flags selecionadas (Múltipla Escolha)
        Assert.True(obtida.RestricoesMedicas.HasFlag(MatriculaRestricoes.PressaoAlta));
        Assert.True(obtida.RestricoesMedicas.HasFlag(MatriculaRestricoes.Labirintite));
        Assert.True(obtida.RestricoesMedicas.HasFlag(MatriculaRestricoes.ProblemasRespiratorios));
        Assert.True(obtida.RestricoesMedicas.HasFlag(MatriculaRestricoes.RemedioContinuo));
        // Verificação de que flags não marcadas retornam false
        Assert.False(obtida.RestricoesMedicas.HasFlag(MatriculaRestricoes.Diabetes));
        Assert.False(obtida.RestricoesMedicas.HasFlag(MatriculaRestricoes.Alergias));
        Assert.Equal("Evitar exercícios de alto impacto e hipertensão", obtida.ObservacoesRestricoes);
        Assert.NotNull(obtida.LaudoMedico);
        Assert.Equal(laudo.Conteudo, obtida.LaudoMedico.Conteudo);
    }

    [Fact]
    public async Task Matricula_ObterPorId_RetornaNuloQuandoInexistente()
    {
        var obtida = await _matriculaRepo.ObterPorId(999999);
        Assert.Null(obtida);
    }
    [Fact]
    public async Task Matricula_ObterTodos_Sucesso()
    {
        var aluno = await AlunoInfrastructureTests.CriarEInserirAlunoAsync(_alunoRepo, _logradouroRepo, "Samuel", "Braz", "SenhaTeste123");
        await CriarEInserirMatriculaAsync(aluno);
        var todas = await _matriculaRepo.ObterTodos();
        Assert.NotNull(todas);
        Assert.NotEmpty(todas);
    }
    [Fact]
    public async Task Matricula_Atualizar_LancaExcecaoQuandoInexistente()
    {
        var aluno = await AlunoInfrastructureTests.CriarEInserirAlunoAsync(_alunoRepo, _logradouroRepo, "Samuel", "Braz", "SenhaTeste123");
        var matriculaInexistente = Matricula.Criar(
        id: 999999,
        aluno: aluno,
        plano: MatriculaPlano.Mensal,
        dataInicio: DateOnly.FromDateTime(DateTime.Today),
        objetivo: "Teste Inexistente",
        restricoesMedicas: MatriculaRestricoes.None,
        laudoMedico: null
        ).Value!;
        var ex = await Assert.ThrowsAsync<InfrastructureException>(() => _matriculaRepo.Atualizar(matriculaInexistente));
        Assert.Equal("REGISTRO_NAO_ENCONTRADO", ex.ErrorCode);
    }

    [Fact]
    public async Task Matricula_Atualizar_Sucesso()
    {
        var aluno = await AlunoInfrastructureTests.CriarEInserirAlunoAsync(_alunoRepo, _logradouroRepo, "Samuel", "Braz", "SenhaTeste123");
        var inserida = await CriarEInserirMatriculaAsync(aluno, MatriculaPlano.Mensal, restricoes: MatriculaRestricoes.Alergias);
        var novasRestricoes = MatriculaRestricoes.Alergias | MatriculaRestricoes.Diabetes | MatriculaRestricoes.Labirintite;
        var laudoAtualizado = Arquivo.Criar(new byte[] { 99, 88, 77 }).Value!;
        var matriculaAtualizada = Matricula.Criar(
        id: inserida.Id,
        aluno: aluno,
        plano: MatriculaPlano.Anual,
        dataInicio: inserida.DataInicio,
        objetivo: "Ganho de Massa Muscular",
        restricoesMedicas: novasRestricoes,
        laudoMedico: laudoAtualizado,
        observacoesRestricoes: "Restrição médica atualizada com novas opções"
        ).Value!;
        var resultado = await _matriculaRepo.Atualizar(matriculaAtualizada);
        Assert.NotNull(resultado);
        Assert.Equal(MatriculaPlano.Anual, resultado.Plano);
        Assert.Equal("Ganho de Massa Muscular", resultado.Objetivo);
        Assert.Equal(novasRestricoes, resultado.RestricoesMedicas);
        var noBanco = await _matriculaRepo.ObterPorId(inserida.Id);
        Assert.NotNull(noBanco);
        Assert.Equal(MatriculaPlano.Anual, noBanco.Plano);
        Assert.Equal("Ganho de Massa Muscular", noBanco.Objetivo);
        Assert.Equal(novasRestricoes, noBanco.RestricoesMedicas);
        Assert.True(noBanco.RestricoesMedicas.HasFlag(MatriculaRestricoes.Alergias));
        Assert.True(noBanco.RestricoesMedicas.HasFlag(MatriculaRestricoes.Diabetes));
        Assert.True(noBanco.RestricoesMedicas.HasFlag(MatriculaRestricoes.Labirintite));
    }

    [Fact]
    public async Task Matricula_Remover_Sucesso()
    {
        var aluno = await AlunoInfrastructureTests.CriarEInserirAlunoAsync(_alunoRepo, _logradouroRepo, "Samuel", "Braz", "SenhaTeste123");
        var inserida = await CriarEInserirMatriculaAsync(aluno);
        var removida = await _matriculaRepo.Remover(inserida.Id);
        Assert.True(removida);
        var noBanco = await _matriculaRepo.ObterPorId(inserida.Id);
        Assert.Null(noBanco);
    }
    [Fact]
    public async Task Matricula_Remover_RetornaFalseQuandoInexistente()
    {
        var removida = await _matriculaRepo.Remover(999999);
        Assert.False(removida);
    }
    [Fact]
    public async Task Matricula_ObterPorAluno_FiltragemCorreta()
    {
        var aluno = await AlunoInfrastructureTests.CriarEInserirAlunoAsync(_alunoRepo, _logradouroRepo, "Samuel", "Braz", "SenhaTeste123");
        await CriarEInserirMatriculaAsync(aluno);
        var matriculas = await _matriculaRepo.ObterPorAluno(aluno.Id);
        Assert.NotNull(matriculas);
        Assert.NotEmpty(matriculas);
        Assert.All(matriculas, m => Assert.Equal(aluno.Id, m.AlunoId));
    }
    [Fact]
    public async Task Matricula_ObterMatriculaAtivaPorAluno_E_PossuiMatriculaAtiva()
    {
        var aluno = await AlunoInfrastructureTests.CriarEInserirAlunoAsync(_alunoRepo, _logradouroRepo, "Samuel", "Braz", "SenhaTeste123");
        var possuiAntes = await _matriculaRepo.PossuiMatriculaAtiva(aluno.Id);
        Assert.False(possuiAntes);
        await CriarEInserirMatriculaAsync(aluno, MatriculaPlano.Mensal, DateOnly.FromDateTime(DateTime.Today));
        var possuiDepois = await _matriculaRepo.PossuiMatriculaAtiva(aluno.Id);
        Assert.True(possuiDepois);
        var ativa = await _matriculaRepo.ObterMatriculaAtivaPorAluno(aluno.Id);
        Assert.NotNull(ativa);
        Assert.Equal(aluno.Id, ativa.AlunoId);
    }

    [Fact]
    public async Task Matricula_ObterAtivas_FiltragemCorreta()
    {
        var aluno = await AlunoInfrastructureTests.CriarEInserirAlunoAsync(_alunoRepo, _logradouroRepo, "Samuel", "Braz", "SenhaTeste123");
        await CriarEInserirMatriculaAsync(aluno, MatriculaPlano.Semestral, DateOnly.FromDateTime(DateTime.Today));
        var ativasGeral = await _matriculaRepo.ObterAtivas();
        Assert.NotNull(ativasGeral);
        Assert.NotEmpty(ativasGeral);
        var ativasPorAluno = await _matriculaRepo.ObterAtivas(aluno.Id);
        Assert.NotNull(ativasPorAluno);
        Assert.NotEmpty(ativasPorAluno);
        Assert.All(ativasPorAluno, m => Assert.Equal(aluno.Id, m.AlunoId));
    }
    [Fact]
    public async Task Matricula_ObterVencendoEmDias_RetornaMatriculasProximasDoVencimento()
    {
        var aluno = await AlunoInfrastructureTests.CriarEInserirAlunoAsync(_alunoRepo, _logradouroRepo, "Samuel", "Braz", "SenhaTeste123");
        var inicio = DateOnly.FromDateTime(DateTime.Today.AddDays(-25));
        await CriarEInserirMatriculaAsync(aluno, MatriculaPlano.Mensal, inicio);
        var vencendoEm30Dias = await _matriculaRepo.ObterVencendoEmDias(30);
        Assert.NotNull(vencendoEm30Dias);
        Assert.Contains(vencendoEm30Dias, m => m.AlunoId == aluno.Id);
    }
    [Fact]
    public async Task Matricula_ObterPorPlano_FiltragemCorreta()
    {
        var aluno = await AlunoInfrastructureTests.CriarEInserirAlunoAsync(_alunoRepo, _logradouroRepo, "Samuel", "Braz", "SenhaTeste123");
        await CriarEInserirMatriculaAsync(aluno, MatriculaPlano.Trimestral);
        var trimestrais = await _matriculaRepo.ObterPorPlano(MatriculaPlano.Trimestral);
        Assert.NotNull(trimestrais);
        Assert.Contains(trimestrais, m => m.AlunoId == aluno.Id && m.Plano == MatriculaPlano.Trimestral);
    }
}