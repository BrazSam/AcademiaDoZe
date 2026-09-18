using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Application.Enums;
namespace AcademiaDoZe.Application.Interfaces;
/// <summary>
/// Contrato de serviço para operações de negócios relacionadas a Matrículas de alunos.
/// </summary>
public interface IMatriculaService
{
    /// <summary>
    /// Obtém uma matrícula pelo ID, enriquecida com os dados do aluno.
    /// </summary>
    Task<MatriculaDto?> ObterPorIdAsync(int id, CancellationToken cancellationToken = default);
    /// <summary>
    /// Obtém todas as matrículas cadastradas.
    /// </summary>
    Task<IEnumerable<MatriculaDto>> ObterTodasAsync(CancellationToken cancellationToken = default);
    /// <summary>
    /// Cadastra uma nova matrícula para o aluno, calculando automaticamente a DataFim e validando que o aluno não possua outra matrícula ativa.
    /// </summary>
    Task<MatriculaDto> AdicionarAsync(MatriculaDto matriculaDto, CancellationToken cancellationToken = default);
    /// <summary>
    /// Atualiza os dados de uma matrícula existente.
    /// </summary>
    Task<MatriculaDto> AtualizarAsync(MatriculaDto matriculaDto, CancellationToken cancellationToken = default);
    /// <summary>
    /// Remove uma matrícula pelo ID.
    /// </summary>
    Task<bool> RemoverAsync(int id, CancellationToken cancellationToken = default);
    /// <summary>
    /// Obtém o histórico de matrículas de um aluno específico.
    /// </summary>
    Task<IEnumerable<MatriculaDto>> ObterPorAlunoIdAsync(int alunoId, CancellationToken cancellationToken = default);
    /// <summary>
    /// Obtém a matrícula ativa vigente de um aluno, se houver.
    /// </summary>
    Task<MatriculaDto?> ObterMatriculaAtivaPorAlunoAsync(int alunoId, CancellationToken cancellationToken = default);
    /// <summary>
    /// Verifica se o aluno possui atualmente uma matrícula ativa.
    /// </summary>
    Task<bool> PossuiMatriculaAtivaAsync(int alunoId, CancellationToken cancellationToken = default);
    /// <summary>
    /// Obtém todas as matrículas ativas no sistema ou filtradas por aluno.
    /// </summary>
    Task<IEnumerable<MatriculaDto>> ObterAtivasAsync(int alunoId = 0, CancellationToken cancellationToken = default);
    /// <summary>
    /// Obtém as matrículas que estão prestes a vencer dentro do número de dias especificado.
    /// </summary>
    Task<IEnumerable<MatriculaDto>> ObterVencendoEmDiasAsync(int dias, CancellationToken cancellationToken = default);
    /// <summary>
    /// Obtém matrículas filtrando pelo tipo de plano contratado (Mensal, Trimestral, Semestral, Anual).
    /// </summary>
    Task<IEnumerable<MatriculaDto>> ObterPorPlanoAsync(AppMatriculaPlano plano, CancellationToken cancellationToken = default);
}   