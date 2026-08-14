using AcademiaDoZe.Domain.Common; //Samuel Braz dos Santos 
using AcademiaDoZe.Domain.Entities;
namespace AcademiaDoZe.Domain.Repositories;
// Interface genérica para repositórios. Restrita apenas a Raízes de Agregado (Aggregate Roots) no DDD.// Define os contratos essenciais para a persistência de dados.
// Herda de Entity para garantir que TEntity seja uma entidade válida, e seu uso somente no domain.
// Métodos assíncronos (Task), alinhados com práticas modernas de acesso a dados.
public interface IRepository<TEntity> where TEntity : Entity, IAggregateRoot
{
    Task<TEntity?> ObterPorId(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<TEntity>> ObterTodos(CancellationToken cancellationToken = default);
    Task<TEntity> Adicionar(TEntity entity, CancellationToken cancellationToken = default);
    Task<TEntity> Atualizar(TEntity entity, CancellationToken cancellationToken = default);
    Task<bool> Remover(int id, CancellationToken cancellationToken = default);
}