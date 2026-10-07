using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Application.Interfaces;
using AcademiaDoZe.Application.Mappings;
using AcademiaDoZe.Domain.Repositories;
using AcademiaDoZe.Domain.ValueObjects;
namespace AcademiaDoZe.Application.Services;

public class LogradouroService : ILogradouroService
{
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
        using var repo = _repoFactory();
        return await repo.CepJaExiste(cepResult.Value!, id, cancellationToken);
    }

    public async Task<LogradouroDto?> ObterPorIdAsync(int id, CancellationToken cancellationToken = default)
    {
        using var repo = _repoFactory();
        var logradouro = await repo.ObterPorId(id, cancellationToken);
        return logradouro?.ToDto();
    }

    public async Task<IEnumerable<LogradouroDto>> ObterTodosAsync(CancellationToken cancellationToken = default)
    {
        using var repo = _repoFactory();
        var logradouros = await repo.ObterTodos(cancellationToken);
        return [.. logradouros.Select(l => l.ToDto())];
    }

    public async Task<LogradouroDto?> ObterPorCepAsync(string cep, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(cep))
            throw new ArgumentException("CEP não pode ser vazio.", nameof(cep));
        var cepResult = Cep.Criar(cep);
        if (cepResult.IsFailure)
            throw new ArgumentException($"CEP inválido: {string.Join(", ", cepResult.Notifications.Select(n => n.Mensagem))}", nameof(cep));
        using var repo = _repoFactory();
        var logradouro = await repo.ObterPorCep(cepResult.Value!, cancellationToken);
        return logradouro?.ToDto();
    }

    public async Task<IEnumerable<LogradouroDto>> ObterPorCidadeAsync(string cidade, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(cidade))
            throw new ArgumentException("Cidade não pode ser vazia.", nameof(cidade));
        using var repo = _repoFactory();
        var logradouros = await repo.ObterPorCidade(cidade.Trim(), cancellationToken);
        return [.. logradouros.Select(l => l.ToDto())];
    }

    public async Task<IEnumerable<LogradouroDto>> ObterPorBairroAsync(string cidade, string bairro, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(cidade))
            throw new ArgumentException("Cidade não pode ser vazia.", nameof(cidade));
        if (string.IsNullOrWhiteSpace(bairro))
            throw new ArgumentException("Bairro não pode ser vazio.", nameof(bairro));
        using var repo = _repoFactory();
        var logradouros = await repo.ObterPorBairro(cidade.Trim(), bairro.Trim(), cancellationToken);
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
        using var repo = _repoFactory();
        if (await repo.CepJaExiste(cepResult.Value!, null, cancellationToken))
        {
            throw new InvalidOperationException($"Já existe um logradouro cadastrado com o CEP {logradouroDto.Cep}.");
        }
        var logradouro = logradouroDto.ToEntity();
        var adicionado = await repo.Adicionar(logradouro, cancellationToken);
        return adicionado.ToDto();
    }

    public async Task<LogradouroDto> AtualizarAsync(LogradouroDto logradouroDto, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(logradouroDto);
        using var repo = _repoFactory();
        var logradouroExistente = await repo.ObterPorId(logradouroDto.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Logradouro com ID {logradouroDto.Id} não encontrado.");
        var cepResult = Cep.Criar(logradouroDto.Cep);
        if (cepResult.IsFailure)
        {
            throw new ArgumentException($"CEP inválido: {string.Join(", ", cepResult.Notifications.Select(n => n.Mensagem))}", nameof(logradouroDto));
        }
        if (await repo.CepJaExiste(cepResult.Value!, logradouroDto.Id, cancellationToken))
        {
            throw new InvalidOperationException($"Já existe outro logradouro cadastrado com o CEP {logradouroDto.Cep}.");
        }
        var logradouroAtualizado = logradouroExistente.UpdateFromDto(logradouroDto);
        var atualizado = await repo.Atualizar(logradouroAtualizado, cancellationToken);
        return atualizado.ToDto();
    }

    public async Task<bool> RemoverAsync(int id, CancellationToken cancellationToken = default)
    {
        using var repo = _repoFactory();
        var logradouro = await repo.ObterPorId(id, cancellationToken);
        if (logradouro == null)
        {
            return false;
        }
        return await repo.Remover(id, cancellationToken);
    }
}