using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Application.Interfaces;
using AcademiaDoZe.Application.Mappings;
using AcademiaDoZe.Domain.Repositories;
using AcademiaDoZe.Domain.ValueObjects;
namespace AcademiaDoZe.Application.Services;

public class LogradouroService : ILogradouroService
{
    // Func que cria instâncias do repositório quando necessário, garantindo freshness e evitando problemas de ciclo de vida
    // freshness indica que cada chamada cria uma nova instância do repositório, evitando problemas de ciclo de vida
    private readonly Func<ILogradouroRepository> _repoFactory;
    public LogradouroService(Func<ILogradouroRepository> repoFactory)
    {
        _repoFactory = repoFactory ?? throw new ArgumentNullException(nameof(repoFactory));
    }
    public async Task<bool> CepJaExisteAsync(string cep, int? id = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(cep)) return false;
        var cepResult = Cep.Criar(cep);
        if (cepResult.IsFailure) return false;
        return await _repoFactory().CepJaExiste(cepResult.Value!, id, cancellationToken);
    }
    // MÉTODOS AQUI

    public async Task<LogradouroDto?> ObterPorIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var logradouro = await _repoFactory().ObterPorId(id, cancellationToken);
        return logradouro?.ToDto();
    }
    public async Task<IEnumerable<LogradouroDto>> ObterTodosAsync(CancellationToken cancellationToken = default)
    {
        var logradouros = await _repoFactory().ObterTodos(cancellationToken);
        return [.. logradouros.Select(l => l.ToDto())]; // expressão de interpolação para criar uma nova lista de DTOs
    }
    public async Task<LogradouroDto?> ObterPorCepAsync(string cep, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(cep))
            throw new ArgumentException("CEP não pode ser vazio.", nameof(cep));
        var cepResult = Cep.Criar(cep);
        if (cepResult.IsFailure)
            throw new ArgumentException($"CEP inválido: {string.Join(", ", cepResult.Notifications.Select(n => n.Mensagem))}", nameof(cep)); var logradouro = await _repoFactory().ObterPorCep(cepResult.Value!, cancellationToken);
        return logradouro?.ToDto();
    }
    public async Task<IEnumerable<LogradouroDto>> ObterPorCidadeAsync(string cidade, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(cidade))
            throw new ArgumentException("Cidade não pode ser vazia.", nameof(cidade));
        var logradouros = await _repoFactory().ObterPorCidade(cidade.Trim(), cancellationToken);
        return [.. logradouros.Select(l => l.ToDto())];
    }
    public async Task<IEnumerable<LogradouroDto>> ObterPorBairroAsync(string cidade, string bairro, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(cidade))
            throw new ArgumentException("Cidade não pode ser vazia.", nameof(cidade));
        if (string.IsNullOrWhiteSpace(bairro))
            throw new ArgumentException("Bairro não pode ser vazio.", nameof(bairro));
        var logradouros = await _repoFactory().ObterPorBairro(cidade.Trim(), bairro.Trim(), cancellationToken);
        return [.. logradouros.Select(l => l.ToDto())];
    }


    public async Task<LogradouroDto> AdicionarAsync(LogradouroDto logradouroDto, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(logradouroDto);
        var cepResult = Cep.Criar(logradouroDto.Cep);
        if (cepResult.IsFailure)
        {
            throw new ArgumentException($"CEP inválido: {string.Join(", ", cepResult.Notifications.Select(n => n.Mensagem))}", nameof(logradouroDto));
        }
        if (await _repoFactory().CepJaExiste(cepResult.Value!, null, cancellationToken))
        {
            throw new InvalidOperationException($"Já existe um logradouro cadastrado com o CEP {logradouroDto.Cep}.");
        }
        var logradouro = logradouroDto.ToEntity();
        var adicionado = await _repoFactory().Adicionar(logradouro, cancellationToken);
        return adicionado.ToDto();
    }
    public async Task<LogradouroDto> AtualizarAsync(LogradouroDto logradouroDto, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(logradouroDto);
        var logradouroExistente = await _repoFactory().ObterPorId(logradouroDto.Id, cancellationToken)
        ?? throw new KeyNotFoundException($"Logradouro com ID {logradouroDto.Id} não encontrado.");
        var cepResult = Cep.Criar(logradouroDto.Cep);
        if (cepResult.IsFailure)
        {
            throw new ArgumentException($"CEP inválido: {string.Join(", ", cepResult.Notifications.Select(n => n.Mensagem))}", nameof(logradouroDto));
        }
        if (await _repoFactory().CepJaExiste(cepResult.Value!, logradouroDto.Id, cancellationToken))
        {
            throw new InvalidOperationException($"Já existe outro logradouro cadastrado com o CEP {logradouroDto.Cep}.");
        }
        var logradouroAtualizado = logradouroExistente.UpdateFromDto(logradouroDto);
        var atualizado = await _repoFactory().Atualizar(logradouroAtualizado, cancellationToken);
        return atualizado.ToDto();
    }
    public async Task<bool> RemoverAsync(int id, CancellationToken cancellationToken = default)
    {
        var logradouro = await _repoFactory().ObterPorId(id, cancellationToken);
        if (logradouro == null)
        {
            return false;
        }
        return await _repoFactory().Remover(id, cancellationToken);

    }
}