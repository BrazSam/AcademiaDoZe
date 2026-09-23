using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Application.Interfaces;
using AcademiaDoZe.Application.Mappings;
using AcademiaDoZe.Application.Security;
using AcademiaDoZe.Domain.Repositories;
using AcademiaDoZe.Domain.ValueObjects;
namespace AcademiaDoZe.Application.Services;

public class AlunoService : IAlunoService
{
    private readonly Func<IAlunoRepository> _repoFactory;
    private readonly Func<ILogradouroRepository>? _logradouroRepoFactory;
    public AlunoService(Func<IAlunoRepository> repoFactory, Func<ILogradouroRepository>? logradouroRepoFactory = null)
    {
        _repoFactory = repoFactory ?? throw new ArgumentNullException(nameof(repoFactory));
        _logradouroRepoFactory = logradouroRepoFactory;
    }
    public async Task<bool> CpfJaExisteAsync(string cpf, int? id = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(cpf)) return false;
        var cpfResult = Cpf.Criar(cpf);
        if (cpfResult.IsFailure) return false;
        return await _repoFactory().CpfJaExiste(cpfResult.Value!, id, cancellationToken);
    }
    public async Task<bool> EmailJaExisteAsync(string email, int? id = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(email)) return false;
        var emailResult = Email.Criar(email);
        if (emailResult.IsFailure) return false;
        return await _repoFactory().EmailJaExiste(emailResult.Value!, id, cancellationToken);
    }
    // MÉTODOS AQUI

    public async Task<AlunoDto?> ObterPorIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var aluno = await _repoFactory().ObterPorId(id, cancellationToken);
        if (aluno == null) return null;
        if (_logradouroRepoFactory != null)
        {
            var logradouro = await _logradouroRepoFactory().ObterPorId(aluno.Endereco.LogradouroId, cancellationToken);
            return aluno.ToDto(logradouro);
        }
        return aluno.ToDto();
    }
    public async Task<IEnumerable<AlunoDto>> ObterTodosAsync(CancellationToken cancellationToken = default)
    {
        var alunos = (await _repoFactory().ObterTodos(cancellationToken)).ToList();
        if (alunos.Count == 0) return [];
        if (_logradouroRepoFactory != null)
        {
            var logradouroIds = alunos.Select(a => a.Endereco.LogradouroId).Distinct().ToList();
            var logradouros = new Dictionary<int, Domain.Entities.Logradouro>();
            foreach (var logId in logradouroIds)
            {
                var log = await _logradouroRepoFactory().ObterPorId(logId, cancellationToken);
                if (log != null) logradouros[logId] = log;
            }
            return [.. alunos.Select(a => a.ToDto(logradouros.GetValueOrDefault(a.Endereco.LogradouroId)))];
        }
        return [.. alunos.Select(a => a.ToDto())];
    }
    public async Task<AlunoDto?> ObterPorCpfAsync(string cpf, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(cpf))
            throw new ArgumentException("CPF não pode ser vazio.", nameof(cpf));
        var cpfResult = Cpf.Criar(cpf);
        if (cpfResult.IsFailure)
            throw new ArgumentException($"CPF inválido: {string.Join(", ", cpfResult.Notifications.Select(n => n.Mensagem))}", nameof(cpf));
        var aluno = await _repoFactory().ObterPorCpf(cpfResult.Value!, cancellationToken);
        return aluno?.ToDto();
    }

    public async Task<bool> RemoverAsync(int id, CancellationToken cancellationToken = default)
    {
        var aluno = await _repoFactory().ObterPorId(id, cancellationToken);
        if (aluno == null)
        {
            return false;
        }
        return await _repoFactory().Remover(id, cancellationToken);
    }
    public async Task<AlunoDto?> ObterPorEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email não pode ser vazio.", nameof(email));
        var emailResult = Email.Criar(email);
        if (emailResult.IsFailure)
            throw new ArgumentException($"Email inválido: {string.Join(", ", emailResult.Notifications.Select(n => n.Mensagem))}", nameof(email));
        var aluno = await _repoFactory().ObterPorEmail(emailResult.Value!, cancellationToken);
        return aluno?.ToDto();
    }
    public async Task<IEnumerable<AlunoDto>> ObterPorNomeAsync(string nome, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new ArgumentException("Nome não pode ser vazio.", nameof(nome));
        var alunos = await _repoFactory().ObterPorNome(nome.Trim(), cancellationToken);
        return [.. alunos.Select(a => a.ToDto())];
    }
    public async Task<bool> TrocarSenhaAsync(int id, string novaSenha, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(novaSenha))
            throw new ArgumentException("Nova senha não pode ser vazia.", nameof(novaSenha));
        var validacaoSenha = Senha.Criar(novaSenha);
        if (validacaoSenha.IsFailure)
        {
            throw new ArgumentException($"Nova senha inválida: {string.Join(", ", validacaoSenha.Notifications.Select(n => n.Mensagem))}", nameof(novaSenha));
        }
        var hash = PasswordHasher.Hash(novaSenha);
        var senhaHashVO = Senha.Criar(hash);
        if (senhaHashVO.IsFailure)
        {
            throw new InvalidOperationException("Falha ao gerar hash da nova senha.");
        }
        return await _repoFactory().TrocarSenha(id, senhaHashVO.Value!, cancellationToken);
    }


        public async Task<AlunoDto> AdicionarAsync(AlunoDto alunoDto, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(alunoDto);
        var cpfResult = Cpf.Criar(alunoDto.Cpf);
        if (cpfResult.IsFailure)
        {
            throw new ArgumentException($"CPF inválido: {string.Join(", ", cpfResult.Notifications.Select(n => n.Mensagem))}", nameof(alunoDto));
        }
        if (await _repoFactory().CpfJaExiste(cpfResult.Value!, null, cancellationToken))
        {
            throw new InvalidOperationException($"Já existe um aluno cadastrado com o CPF {alunoDto.Cpf}.");
        }
        if (!string.IsNullOrWhiteSpace(alunoDto.Email))
        {
            var emailResult = Email.Criar(alunoDto.Email);
            if (emailResult.IsFailure)
            {
                throw new ArgumentException($"Email inválido: {string.Join(", ", emailResult.Notifications.Select(n => n.Mensagem))}", nameof(alunoDto));
            }
            if (await _repoFactory().EmailJaExiste(emailResult.Value!, null, cancellationToken))
            {
                throw new InvalidOperationException($"Já existe um aluno cadastrado com o Email {alunoDto.Email}.");
            }
        }
        if (!string.IsNullOrWhiteSpace(alunoDto.Senha))
        {
            var senhaValidacao = Senha.Criar(alunoDto.Senha);
            if (senhaValidacao.IsFailure)
            {
                throw new ArgumentException($"Senha não atende aos requisitos mínimos: {string.Join(", ", senhaValidacao.Notifications.Select(n => n.Mensagem))}", nameof(alunoDto));
            }
            alunoDto.Senha = PasswordHasher.Hash(alunoDto.Senha);
        }
        Domain.Entities.Logradouro? logradouro = null;
        if (_logradouroRepoFactory != null && alunoDto.Endereco != null && alunoDto.Endereco.Id > 0)
        {
            logradouro = await _logradouroRepoFactory().ObterPorId(alunoDto.Endereco.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Logradouro com ID {alunoDto.Endereco.Id} não encontrado.");
        }
        var aluno = alunoDto.ToEntity(logradouro);
        var adicionado = await _repoFactory().Adicionar(aluno, cancellationToken);
        return adicionado.ToDto(logradouro);
    }

    public async Task<AlunoDto> AtualizarAsync(AlunoDto alunoDto, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(alunoDto);
        var alunoExistente = await _repoFactory().ObterPorId(alunoDto.Id, cancellationToken)
        ?? throw new KeyNotFoundException($"Aluno com ID {alunoDto.Id} não encontrado.");
        var cpfResult = Cpf.Criar(alunoDto.Cpf);
        if (cpfResult.IsFailure)
        {
            throw new ArgumentException($"CPF inválido: {string.Join(", ", cpfResult.Notifications.Select(n => n.Mensagem))}", nameof(alunoDto));
        }
        if (await _repoFactory().CpfJaExiste(cpfResult.Value!, alunoDto.Id, cancellationToken))
        {
            throw new InvalidOperationException($"Já existe outro aluno cadastrado com o CPF {alunoDto.Cpf}.");
        }
        if (!string.IsNullOrWhiteSpace(alunoDto.Email) && !string.Equals(alunoDto.Email, alunoExistente.Email.Valor, StringComparison.OrdinalIgnoreCase))
        {
            var emailResult = Email.Criar(alunoDto.Email);
            if (emailResult.IsFailure)
            {
                throw new ArgumentException($"Email inválido: {string.Join(", ", emailResult.Notifications.Select(n => n.Mensagem))}", nameof(alunoDto));
            }
            if (await _repoFactory().EmailJaExiste(emailResult.Value!, alunoDto.Id, cancellationToken))
            {
                throw new InvalidOperationException($"Já existe outro aluno cadastrado com o Email {alunoDto.Email}.");
            }
        }
        if (!string.IsNullOrWhiteSpace(alunoDto.Senha))
        {
            var senhaValidacao = Senha.Criar(alunoDto.Senha);
            if (senhaValidacao.IsFailure)
            {
                throw new ArgumentException($"Senha não atende aos requisitos mínimos: {string.Join(", ", senhaValidacao.Notifications.Select(n => n.Mensagem))}", nameof(alunoDto));
            }
            alunoDto.Senha = PasswordHasher.Hash(alunoDto.Senha);
        }
        Domain.Entities.Logradouro? logradouro = null;
        int logradouroId = (alunoDto.Endereco != null && alunoDto.Endereco.Id > 0)
        ? alunoDto.Endereco.Id
        : alunoExistente.Endereco.LogradouroId;
        if (_logradouroRepoFactory != null && logradouroId > 0)
        {
            logradouro = await _logradouroRepoFactory().ObterPorId(logradouroId, cancellationToken)
            ?? throw new KeyNotFoundException($"Logradouro com ID {logradouroId} não encontrado.");
        }
        var alunoAtualizado = alunoExistente.UpdateFromDto(alunoDto, logradouro);
        var atualizado = await _repoFactory().Atualizar(alunoAtualizado, cancellationToken);
        return atualizado.ToDto(logradouro);
    }
}