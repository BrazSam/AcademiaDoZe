using AcademiaDoZe.Domain.Entities;
namespace AcademiaDoZe.Domain.Repositories;

public interface IAcessoAlunoRepository : IRepository<AcessoAluno>
{
    Task<IEnumerable<AcessoAluno>> ObterAcessosPorAlunoPeriodo(int? alunoId = null, DateOnly? inicio = null, DateOnly? fim = null, CancellationToken cancellationToken= default); Task<AcessoAluno?> ObterUltimoAcesso(int alunoId, CancellationToken cancellationToken = default);
    Task<bool> EstaNaAcademia(int alunoId, CancellationToken cancellationToken = default);
    // horário mensal de maior procura, baseado na entrada, por exemplo, em dezembro o horário de maior procura é entre 18h e 20h
    // Retorna um dicionário onde a chave é o horário e o valor é a quantidade de acessos nesse horário
    Task<Dictionary<TimeOnly, int>> ObterHorarioMaisProcuradoPorMes(int mes, CancellationToken cancellationToken = default);
    // Permanência média dos alunos na academia, mensal.
    // retorna um dicionário onde a chave é o mês e o valor é a média de permanência dos alunos nesse mês
    Task<Dictionary<int, TimeSpan>> ObterPermanenciaMediaPorMes(int mes, CancellationToken cancellationToken = default);
    // alunos que não registraram acesso nos últimos x dias
    Task<IEnumerable<Aluno>> ObterAlunosSemAcessoNosUltimosDias(int dias, CancellationToken cancellationToken = default);
}
// Validar se possui matrícula ativa.
// Na entrada, mostrar quanto tempo ainda tem de plano.
// Na saída, mostrar o tempo que permaneceu na academia.