using AcademiaDoZe.Application.DTOs; //Samuel Braz dos Santos
using AcademiaDoZe.Application.Enums;
using AcademiaDoZe.Domain.Entities;
namespace AcademiaDoZe.Application.Mappings;

public static class AcessoAlunoMappingExtensions
{
    public static AcessoAlunoDto ToDto(this AcessoAluno acesso, AlunoDto alunoDto, AppAcessoTipo tipo)
    {
        ArgumentNullException.ThrowIfNull(acesso);
        ArgumentNullException.ThrowIfNull(alunoDto);
        return new AcessoAlunoDto
        {
            Id = acesso.Id,
            Aluno = alunoDto,
            DataHora = acesso.DataHora,
            Tipo = tipo
        };
    }
}