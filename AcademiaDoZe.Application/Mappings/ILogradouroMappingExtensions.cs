using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Domain.Entities;
namespace AcademiaDoZe.Application.Mappings;

public static class LogradouroMappingExtensions
{
    public static LogradouroDto ToDto(this Logradouro logradouro)
    {
        ArgumentNullException.ThrowIfNull(logradouro);
        return new LogradouroDto
        {
            Id = logradouro.Id,
            Cep = logradouro.Cep.Valor,
            Nome = logradouro.Nome,
            Bairro = logradouro.Bairro,
            Cidade = logradouro.Cidade,
            Estado = logradouro.Estado,
            Pais = logradouro.Pais
        };
    }
    public static Logradouro ToEntity(this LogradouroDto logradouroDto)
    {
        ArgumentNullException.ThrowIfNull(logradouroDto);
        var result = Logradouro.Criar(
        logradouroDto.Id,
        logradouroDto.Cep,
        logradouroDto.Nome,
        logradouroDto.Bairro,
        logradouroDto.Cidade,
        logradouroDto.Estado,
        logradouroDto.Pais
        );
        if (result.IsFailure)
        {
            throw new InvalidOperationException($"Erro de validação ao converter Logradouro: {string.Join(", ", result.Notifications.Select(n => n.Mensagem))}");
        }
        return result.Value!;
    }
    public static Logradouro UpdateFromDto(this Logradouro logradouro, LogradouroDto logradouroDto)
    {
        ArgumentNullException.ThrowIfNull(logradouro);
        ArgumentNullException.ThrowIfNull(logradouroDto);
        var result = Logradouro.Criar(
        logradouro.Id,
        logradouroDto.Cep ?? logradouro.Cep.Valor,
        logradouroDto.Nome ?? logradouro.Nome,
        logradouroDto.Bairro ?? logradouro.Bairro,
        logradouroDto.Cidade ?? logradouro.Cidade,
        logradouroDto.Estado ?? logradouro.Estado,
        logradouroDto.Pais ?? logradouro.Pais
        );
        if (result.IsFailure)
        {
            throw new InvalidOperationException($"Erro de validação ao atualizar Logradouro: {string.Join(", ", result.Notifications.Select(n => n.Mensagem))}");
        }
        return result.Value!;
    }
}