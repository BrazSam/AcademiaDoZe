using AcademiaDoZe.Domain.Common; //Samuel Braz dos Santos
using AcademiaDoZe.Domain.Enums;
using AcademiaDoZe.Domain.Services;
using AcademiaDoZe.Domain.ValueObjects;
namespace AcademiaDoZe.Domain.Entities;

public class Matricula : Entity, IAggregateRoot
{
    // encapsulamento das propriedades, aplicando imutabilidade
    public int AlunoId { get; private set; }
    public MatriculaPlano Plano { get; private set; }
    public DateOnly DataInicio { get; private set; }
    public DateOnly DataFim { get; private set; }
    public string Objetivo { get; private set; }
    public MatriculaRestricoes RestricoesMedicas { get; private set; }
    public string ObservacoesRestricoes { get; private set; }
    public Arquivo? LaudoMedico { get; private set; }
    // construtor privado para evitar instância direta
    private Matricula(int id, int alunoId, MatriculaPlano plano, DateOnly dataInicio, DateOnly dataFim, string objetivo, MatriculaRestricoes restricoesMedicas, Arquivo? laudoMedico, string observacoesRestricoes = "") : base(id)
    {
        AlunoId = alunoId;
        Plano = plano;
        DataInicio = dataInicio;
        DataFim = dataFim;
        Objetivo = objetivo;
        RestricoesMedicas = restricoesMedicas;
        LaudoMedico = laudoMedico;
        ObservacoesRestricoes = observacoesRestricoes;
    }
    // método de fábrica, ponto de entrada para criar um ojeto válido
    public static Result<Matricula> Criar(int id, Aluno aluno, MatriculaPlano plano, DateOnly dataInicio, string objetivo, MatriculaRestricoes restricoesMedicas, Arquivo? laudoMedico, string observacoesRestricoes = "")
    {
        var notifications = new List<Notification>();
        // Validações e normalizações
        if (aluno == null)
        {
            notifications.Add(new Notification("Aluno", "ALUNO_INVALIDO"));
        }
        else if (aluno.DataNascimento > DateOnly.FromDateTime(DateTime.Today.AddYears(-16)) && laudoMedico == null)
        {
            notifications.Add(new Notification("LaudoMedico", "MENOR_16_LAUDO_OBRIGATORIO"));
        }
        if (!Enum.IsDefined(plano)) notifications.Add(new Notification("Plano", "PLANO_INVALIDO"));
        if (dataInicio == default) notifications.Add(new Notification("DataInicio", "DATA_INICIO_OBRIGATORIO"));
        // Cálculo da DataFim no domínio baseado no tipo de plano
        DateOnly dataFim = default;
        if (Enum.IsDefined(plano) && dataInicio != default)
        {
            dataFim = plano switch
            {
                MatriculaPlano.Mensal => dataInicio.AddMonths(1),
                MatriculaPlano.Trimestral => dataInicio.AddMonths(3),
                MatriculaPlano.Semestral => dataInicio.AddMonths(6),
                MatriculaPlano.Anual => dataInicio.AddMonths(12),
                _ => default
            };
        }
        if (NormalizacaoService.TextoVazioOuNulo(objetivo)) notifications.Add(new Notification("Objetivo", "OBJETIVO_OBRIGATORIO"));
        else objetivo = NormalizacaoService.LimparEspacos(objetivo);
        if (restricoesMedicas != MatriculaRestricoes.None && laudoMedico == null) notifications.Add(new Notification("LaudoMedico", "RESTRICOES_LAUDO_OBRIGATORIO"));
        observacoesRestricoes = NormalizacaoService.LimparEspacos(observacoesRestricoes);
        if (notifications.Count != 0) return Result<Matricula>.Failure(notifications);
        // criação e retorno do objeto
        var matricula = new Matricula(id, aluno!.Id, plano, dataInicio, dataFim, objetivo, restricoesMedicas, laudoMedico, observacoesRestricoes);
        return Result<Matricula>.Success(matricula);
    }
}
