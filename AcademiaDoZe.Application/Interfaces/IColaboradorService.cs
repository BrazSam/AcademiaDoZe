using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Application.Enums;
namespace AcademiaDoZe.Application.Interfaces;
/// <summary>
/// Contrato de serviço para operações de negócios relacionadas a Colaboradores (funcionários/estagiários).
/// </summary>
public interface IColaboradorService
{
    /// <summary>
    /// Obtém um colaborador pelo seu ID.
    /// </summary>
    Task<ColaboradorDto?> ObterPorIdAsync(int id, CancellationToken cancellationToken = default);
    /// <summary>
    /// Obtém todos os colaboradores cadastrados.
    /// </summary>
    Task<IEnumerable<ColaboradorDto>> ObterTodosAsync(CancellationToken cancellationToken = default);
    /// <summary>
    /// Cadastra um novo colaborador após validação de CPF, email, senha e aplicação de hash Argon2id.
    /// </summary>
    Task<ColaboradorDto> AdicionarAsync(ColaboradorDto colaboradorDto, CancellationToken cancellationToken = default);
    /// <summary>
    /// Atualiza os dados de um colaborador existente.
    /// </summary>
    Task<ColaboradorDto> AtualizarAsync(ColaboradorDto colaboradorDto, CancellationToken cancellationToken = default);
    /// <summary>
    /// Remove um colaborador pelo ID.
    /// </summary>
    Task<bool> RemoverAsync(int id, CancellationToken cancellationToken = default);
    /// <summary>
    /// Obtém um colaborador buscando pelo seu CPF.
    /// </summary>
    Task<ColaboradorDto?> ObterPorCpfAsync(string cpf, CancellationToken cancellationToken = default);
    /// <summary>
    /// Obtém um colaborador buscando pelo seu email.
    /// </summary>
    Task<ColaboradorDto?> ObterPorEmailAsync(string email, CancellationToken cancellationToken = default);
    /// <summary>
    /// Obtém colaboradores filtrando pelo cargo/tipo (ex: Instrutor, Recepcionista, etc.).
    /// </summary>
    Task<IEnumerable<ColaboradorDto>> ObterPorTipoAsync(AppColaboradorTipo tipo, CancellationToken cancellationToken = default);
    /// <summary>
    /// Obtém colaboradores filtrando pelo regime/vínculo (ex: CLT, Estágio).
    /// </summary>
    Task<IEnumerable<ColaboradorDto>> ObterPorVinculoAsync(AppColaboradorVinculo vinculo, CancellationToken cancellationToken = default);
    /// <summary>
    /// Verifica se já existe um colaborador com o CPF informado.
    /// </summary>
    Task<bool> CpfJaExisteAsync(string cpf, int? id = null, CancellationToken cancellationToken = default);
    /// <summary>
    /// Verifica se já existe um colaborador com o email informado.
    /// </summary>
    Task<bool> EmailJaExisteAsync(string email, int? id = null, CancellationToken cancellationToken = default);
    /// <summary>
    /// Realiza a troca de senha do colaborador gerando novo hash Argon2id.
    /// </summary>
    Task<bool> TrocarSenhaAsync(int id, string novaSenha, CancellationToken cancellationToken = default);
}