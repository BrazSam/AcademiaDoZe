using AcademiaDoZe.Application.Enums; //Samuel Braz dos Santos
namespace AcademiaDoZe.Application.DTOs;

public class AcessoAlunoDto
{
    public int Id { get; set; }
    public required AlunoDto Aluno { get; set; }
    public required DateTime DataHora { get; set; }
    public required AppAcessoTipo Tipo { get; set; }
}