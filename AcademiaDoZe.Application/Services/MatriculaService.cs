using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Application.Enums;
using AcademiaDoZe.Application.Interfaces;
using AcademiaDoZe.Application.Mappings;
using AcademiaDoZe.Domain.Repositories;
namespace AcademiaDoZe.Application.Services;

public class MatriculaService : IMatriculaService
{
    private readonly Func<IMatriculaRepository> _matriculaRepoFactory;
    private readonly Func<IAlunoRepository> _alunoRepoFactory;
    public MatriculaService(Func<IMatriculaRepository> matriculaRepoFactory, Func<IAlunoRepository> alunoRepoFactory)
    {
        _matriculaRepoFactory = matriculaRepoFactory ?? throw new ArgumentNullException(nameof(matriculaRepoFactory));
        _alunoRepoFactory = alunoRepoFactory ?? throw new ArgumentNullException(nameof(alunoRepoFactory));
    }

    public async Task<MatriculaDto?> ObterPorIdAsync(int id, CancellationToken cancellationToken = default)
    {
        using var matriculaRepo = _matriculaRepoFactory();
        using var alunoRepo = _alunoRepoFactory();
        var matricula = await matriculaRepo.ObterPorId(id, cancellationToken);
        if (matricula == null) return null;
        var aluno = await alunoRepo.ObterPorId(matricula.AlunoId, cancellationToken)
        ?? throw new InvalidOperationException($"Aluno associado à matrícula {matricula.Id} não encontrado.");
        return matricula.ToDto(aluno.ToDto());
    }
    public async Task<IEnumerable<MatriculaDto>> ObterTodasAsync(CancellationToken cancellationToken = default)
    {
        using var matriculaRepo = _matriculaRepoFactory();
        var matriculas = await matriculaRepo.ObterTodos(cancellationToken);
        return await EnriquecerComAlunosAsync(matriculas, cancellationToken);
    }
    public async Task<IEnumerable<MatriculaDto>> ObterPorAlunoIdAsync(int alunoId, CancellationToken cancellationToken = default)
    {
        using var alunoRepo = _alunoRepoFactory();
        var aluno = await alunoRepo.ObterPorId(alunoId, cancellationToken)
        ?? throw new InvalidOperationException($"Aluno com ID {alunoId} não encontrado.");
        var alunoDto = aluno.ToDto();
        using var matriculaRepo = _matriculaRepoFactory();
        var matriculas = await matriculaRepo.ObterPorAluno(alunoId, cancellationToken);
        return [.. matriculas.Select(m => m.ToDto(alunoDto))];
    }
    public async Task<MatriculaDto?> ObterMatriculaAtivaPorAlunoAsync(int alunoId, CancellationToken cancellationToken = default)
    {
        using var matriculaRepo = _matriculaRepoFactory();
        var matricula = await matriculaRepo.ObterMatriculaAtivaPorAluno(alunoId, cancellationToken);
        if (matricula == null) return null;
        using var alunoRepo = _alunoRepoFactory();
        var aluno = await alunoRepo.ObterPorId(alunoId, cancellationToken)
        ?? throw new InvalidOperationException($"Aluno com ID {alunoId} não encontrado.");
        return matricula.ToDto(aluno.ToDto());
    }
    public async Task<bool> PossuiMatriculaAtivaAsync(int alunoId, CancellationToken cancellationToken = default)
    {
        using var matriculaRepo = _matriculaRepoFactory();
        return await matriculaRepo.PossuiMatriculaAtiva(alunoId, cancellationToken);
    }
    public async Task<IEnumerable<MatriculaDto>> ObterAtivasAsync(int alunoId = 0, CancellationToken cancellationToken = default)
    {
        using var matriculaRepo = _matriculaRepoFactory();
        var matriculas = await matriculaRepo.ObterAtivas(alunoId, cancellationToken);
        return await EnriquecerComAlunosAsync(matriculas, cancellationToken);
    }
    public async Task<IEnumerable<MatriculaDto>> ObterVencendoEmDiasAsync(int dias, CancellationToken cancellationToken = default)
    {
        using var matriculaRepo = _matriculaRepoFactory();
        var matriculas = await matriculaRepo.ObterVencendoEmDias(dias, cancellationToken);
        return await EnriquecerComAlunosAsync(matriculas, cancellationToken);
    }

    public async Task<bool> RemoverAsync(int id, CancellationToken cancellationToken = default)
    {
        using var matriculaRepo = _matriculaRepoFactory();
        var matricula = await matriculaRepo.ObterPorId(id, cancellationToken);
        if (matricula == null)
            return false;
        return await matriculaRepo.Remover(id, cancellationToken);
    }
    public async Task<IEnumerable<MatriculaDto>> ObterPorPlanoAsync(AppMatriculaPlano plano, CancellationToken cancellationToken = default)
    {
        using var matriculaRepo = _matriculaRepoFactory();
        var matriculas = await matriculaRepo.ObterPorPlano(plano.ToDomain(), cancellationToken);
        return await EnriquecerComAlunosAsync(matriculas, cancellationToken);
    }
    private async Task<IEnumerable<MatriculaDto>> EnriquecerComAlunosAsync(IEnumerable<Domain.Entities.Matricula> matriculas, CancellationToken cancellationToken)
    {
        using var alunoRepo = _alunoRepoFactory();
        var matriculasList = matriculas.ToList();
        if (matriculasList.Count == 0) return [];
        var alunoIds = matriculasList.Select(m => m.AlunoId).Distinct().ToList();
        var alunosDict = new Dictionary<int, AlunoDto>();
        foreach (var id in alunoIds)
        {
            var aluno = await alunoRepo.ObterPorId(id, cancellationToken);
            if (aluno != null)
            {
                alunosDict[id] = aluno.ToDto();
            }
        }
        return [.. matriculasList.Select(m =>
        {
            if (!alunosDict.TryGetValue(m.AlunoId, out var alunoDto))
            {
                throw new InvalidOperationException($"Aluno associado à matrícula {m.Id} não encontrado.");
            }
            return m.ToDto(alunoDto);
        })];
    }

    public async Task<MatriculaDto> AdicionarAsync(MatriculaDto matriculaDto, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(matriculaDto);
        if (matriculaDto.AlunoMatricula == null || matriculaDto.AlunoMatricula.Id <= 0)
            throw new InvalidOperationException("Aluno não informado ou com ID inválido para matrícula.");
        using var alunoRepo = _alunoRepoFactory();
        var aluno = await alunoRepo.ObterPorId(matriculaDto.AlunoMatricula.Id, cancellationToken)
        ?? throw new InvalidOperationException($"Aluno com ID {matriculaDto.AlunoMatricula.Id} não encontrado.");
        using var matriculaRepo = _matriculaRepoFactory();
        if (await matriculaRepo.PossuiMatriculaAtiva(aluno.Id, cancellationToken))
            throw new InvalidOperationException("Já existe uma matrícula ativa para este aluno.");
        bool menorDe16 = aluno.DataNascimento > DateOnly.FromDateTime(DateTime.Today.AddYears(-16));
        bool possuiLaudo = matriculaDto.LaudoMedico?.Conteudo != null && matriculaDto.LaudoMedico.Conteudo.Length > 0;
        if (menorDe16 && !possuiLaudo)
        {
            throw new InvalidOperationException("Alunos menores de 16 anos devem obrigatoriamente apresentar um laudo médico que os autorize a praticar atividades físicas.");
        }
        if (matriculaDto.RestricoesMedicas != AppMatriculaRestricoes.None && !possuiLaudo)
        {
            throw new InvalidOperationException("Alunos com restrições de saúde registradas devem apresentar um parecer médico autorizando a realização de atividades físicas.");
        }
        var matricula = matriculaDto.ToEntity(aluno);
        var adicionada = await matriculaRepo.Adicionar(matricula, cancellationToken);
        return adicionada.ToDto(aluno.ToDto());
    }

    public async Task<MatriculaDto> AtualizarAsync(MatriculaDto matriculaDto, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(matriculaDto);
        using var matriculaRepo = _matriculaRepoFactory();
        var matriculaExistente = await matriculaRepo.ObterPorId(matriculaDto.Id, cancellationToken)
        ?? throw new KeyNotFoundException($"Matrícula com ID {matriculaDto.Id} não encontrada.");
        using var alunoRepo = _alunoRepoFactory();
        var aluno = await alunoRepo.ObterPorId(matriculaExistente.AlunoId, cancellationToken)
        ?? throw new InvalidOperationException($"Aluno associado à matrícula {matriculaDto.Id} não encontrado.");
        bool menorDe16 = aluno.DataNascimento > DateOnly.FromDateTime(DateTime.Today.AddYears(-16));
        bool possuiLaudo = (matriculaDto.LaudoMedico?.Conteudo != null && matriculaDto.LaudoMedico.Conteudo.Length > 0)
        || (matriculaDto.LaudoMedico == null && matriculaExistente.LaudoMedico != null);
        if (menorDe16 && !possuiLaudo)
        {
            throw new InvalidOperationException("Alunos menores de 16 anos devem obrigatoriamente apresentar um laudo médico que os autorize a praticar atividades físicas.");
        }
        var restricoes = matriculaDto.RestricoesMedicas != default
        ? matriculaDto.RestricoesMedicas
        : matriculaExistente.RestricoesMedicas.ToApplication();
        if (restricoes != AppMatriculaRestricoes.None && !possuiLaudo)
        {
            throw new InvalidOperationException("Alunos com restrições de saúde registradas devem apresentar um parecer médico autorizando a realização de atividades físicas.");
        }
        var matriculaAtualizada = matriculaExistente.UpdateFromDto(matriculaDto, aluno);
        var atualizada = await matriculaRepo.Atualizar(matriculaAtualizada, cancellationToken);
        return atualizada.ToDto(aluno.ToDto());
    }
}