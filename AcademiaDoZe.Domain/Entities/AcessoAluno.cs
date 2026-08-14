using AcademiaDoZe.Domain.Common; //Samuel Braz dos Santos  
namespace AcademiaDoZe.Domain.Entities;

public class AcessoAluno : Entity, IAggregateRoot
{
    public int AlunoId { get; private set; }
    public DateTime DataHora { get; private set; }
    private AcessoAluno(int id, int alunoId, DateTime dataHora) : base(id)
    {
        AlunoId = alunoId;
        DataHora = dataHora;
    }
    public static Result<AcessoAluno> Criar(int id, Aluno aluno, DateTime dataHora)
    {
        var notifications = new List<Notification>();
        if (aluno == null)
            notifications.Add(new Notification("Aluno", "ALUNO_INVALIDO"));
        if (dataHora.TimeOfDay < new TimeSpan(6, 0, 0) || dataHora.TimeOfDay > new TimeSpan(22, 0, 0))
            notifications.Add(new Notification("DataHora", "DATA_HORA_INTERVALO_INVALIDO"));
        if (notifications.Count != 0)
            return Result<AcessoAluno>.Failure(notifications);
        return Result<AcessoAluno>.Success(new AcessoAluno(id, aluno!.Id, dataHora));
    }
}
// Dependem da persistência:
// Validar se possui matrícula ativa
// Na entrada, mostrar quanto tempo ainda tem de plano
// Na saída, mostrar o tempo que permaneceu na academia