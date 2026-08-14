using AcademiaDoZe.Domain.Entities; //Samuel Braz dos Santos
using AcademiaDoZe.Domain.Enums;
using AcademiaDoZe.Domain.ValueObjects;
namespace AcademiaDoZe.Domain.Repositories;

public interface IColaboradorRepository : IRepository<Colaborador>
{
    // Métodos específicos do domínio
    Task<Colaborador?> ObterPorCpf(Cpf cpf, CancellationToken cancellationToken = default);
    Task<Colaborador?> ObterPorEmail(Email email, CancellationToken cancellationToken = default);
    Task<bool> CpfJaExiste(Cpf cpf, int? id = null, CancellationToken cancellationToken = default);
    Task<bool> EmailJaExiste(Email email, int? id = null, CancellationToken cancellationToken = default);
    Task<IEnumerable<Colaborador>> ObterPorTipo(ColaboradorTipo tipo, CancellationToken cancellationToken = default); Task<IEnumerable<Colaborador>> ObterPorVinculo(ColaboradorVinculo vinculo, CancellationToken cancellationToken = default); Task<bool> TrocarSenha(int id, Senha novaSenha, CancellationToken cancellationToken = default);
}