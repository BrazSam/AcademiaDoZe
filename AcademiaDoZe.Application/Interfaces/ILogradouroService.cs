using AcademiaDoZe.Application.DTOs;
namespace AcademiaDoZe.Application.Interfaces;
/// <summary>
/// Contrato de serviço para operações de negócios relacionadas a Logradouros (endereços).
/// </summary>
public interface ILogradouroService
{
    /// <summary>
    /// Obtém um logradouro pelo seu identificador único.
    /// </summary>
    Task<LogradouroDto?> ObterPorIdAsync(int id, CancellationToken cancellationToken = default);
    /// <summary>
    /// Obtém todos os logradouros cadastrados.
    /// </summary>
    Task<IEnumerable<LogradouroDto>> ObterTodosAsync(CancellationToken cancellationToken = default);
    /// <summary>
    /// Cadastra um novo logradouro após validar regras de formatação e unicidade de CEP.
    /// </summary>
    Task<LogradouroDto> AdicionarAsync(LogradouroDto logradouroDto, CancellationToken cancellationToken = default);
    /// <summary>
    /// Atualiza os dados de um logradouro existente.
    /// </summary>
    Task<LogradouroDto> AtualizarAsync(LogradouroDto logradouroDto, CancellationToken cancellationToken = default);
    /// <summary>
    /// Remove um logradouro pelo identificador.
    /// </summary>
    Task<bool> RemoverAsync(int id, CancellationToken cancellationToken = default);
    /// <summary>
    /// Obtém um logradouro pelo seu CEP.
    /// </summary>
    Task<LogradouroDto?> ObterPorCepAsync(string cep, CancellationToken cancellationToken = default);
    /// <summary>
    /// Verifica se já existe um logradouro com o CEP informado.
    /// </summary>
    Task<bool> CepJaExisteAsync(string cep, int? id = null, CancellationToken cancellationToken = default);
    /// <summary>
    /// Obtém todos os logradouros de uma cidade específica.
    /// </summary>
    Task<IEnumerable<LogradouroDto>> ObterPorCidadeAsync(string cidade, CancellationToken cancellationToken = default);
    /// <summary>
    /// Obtém todos os logradouros de um bairro específico em uma cidade.
    /// </summary>
    Task<IEnumerable<LogradouroDto>> ObterPorBairroAsync(string cidade, string bairro, CancellationToken cancellationToken = default);
}