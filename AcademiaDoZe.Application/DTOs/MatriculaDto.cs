using AcademiaDoZe.Application.Enums;
namespace AcademiaDoZe.Application.DTOs;

public class MatriculaDto
{
    public int Id { get; set; }
    public required AlunoDto AlunoMatricula { get; set; }
    public required AppMatriculaPlano Plano { get; set; }
    public required DateOnly DataInicio { get; set; }
    public DateOnly DataFim { get; set; }
    public required string Objetivo { get; set; }
    public required AppMatriculaRestricoes RestricoesMedicas { get; set; }
    public string? ObservacoesRestricoes { get; set; }
    public ArquivoDto? LaudoMedico { get; set; }
}