using AcademiaDoZe.Application.Enums; //Samuel Braz dos Santos
namespace AcademiaDoZe.Application.DTOs;

public class AcessoColaboradorDto
{
    public int Id { get; set; }
    public required ColaboradorDto Colaborador { get; set; }
    public required DateTime DataHora { get; set; }
    public required AppAcessoTipo Tipo { get; set; }
}