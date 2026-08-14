using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.ValueObjects;
namespace AcademiaDoZe.Domain.Repositories;

public interface IAlunoRepository : IRepository<Aluno>
{
    // Métodos específicos do domínio
    Task<Aluno?> ObterPorCpf(Cpf cpf, CancellationToken cancellationToken = default);
    Task<Aluno?> ObterPorEmail(Email email, CancellationToken cancellationToken = default);
    Task<bool> CpfJaExiste(Cpf cpf, int? id = null, CancellationToken cancellationToken = default);
    Task<bool> EmailJaExiste(Email email, int? id = null, CancellationToken cancellationToken = default);
    Task<IEnumerable<Aluno>> ObterPorNome(string nome, CancellationToken cancellationToken = default);
    Task<bool> TrocarSenha(int id, Senha novaSenha, CancellationToken cancellationToken = default);
}