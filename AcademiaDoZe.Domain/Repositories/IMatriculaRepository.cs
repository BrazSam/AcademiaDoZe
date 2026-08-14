using AcademiaDoZe.Domain.Entities; //Samuel Braz dos Santos
using AcademiaDoZe.Domain.Enums;
namespace AcademiaDoZe.Domain.Repositories;

public interface IMatriculaRepository : IRepository<Matricula>
{
    // Métodos específicos do domínio
    Task<IEnumerable<Matricula>> ObterPorAluno(int alunoId, CancellationToken cancellationToken = default); 
    Task<Matricula?> ObterMatriculaAtivaPorAluno(int alunoId, CancellationToken cancellationToken = default); 
    Task<bool> PossuiMatriculaAtiva(int alunoId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Matricula>> ObterAtivas(int alunoId = 0, CancellationToken cancellationToken = default); 
    Task<IEnumerable<Matricula>> ObterVencendoEmDias(int dias, CancellationToken cancellationToken = default); 
    Task<IEnumerable<Matricula>> ObterPorPlano(MatriculaPlano plano, CancellationToken cancellationToken = default);
}