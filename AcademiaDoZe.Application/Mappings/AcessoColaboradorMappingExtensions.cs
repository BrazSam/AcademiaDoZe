using AcademiaDoZe.Application.DTOs; //Samuel Braz dos Santos
using AcademiaDoZe.Application.Enums;
using AcademiaDoZe.Domain.Entities;
namespace AcademiaDoZe.Application.Mappings;

public static class AcessoColaboradorMappingExtensions
{
    public static AcessoColaboradorDto ToDto(this AcessoColaborador acesso, ColaboradorDto colaboradorDto, AppAcessoTipo tipo)
    {
        ArgumentNullException.ThrowIfNull(acesso);
        ArgumentNullException.ThrowIfNull(colaboradorDto);
        return new AcessoColaboradorDto
        {
            Id = acesso.Id,
            Colaborador = colaboradorDto,
            DataHora = acesso.DataHora,
            Tipo = tipo
        };
    }
}