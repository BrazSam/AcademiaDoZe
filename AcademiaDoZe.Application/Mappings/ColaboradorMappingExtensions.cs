using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.ValueObjects;
namespace AcademiaDoZe.Application.Mappings;

public static class ColaboradorMappingExtensions
{
    public static ColaboradorDto ToDto(this Colaborador colaborador, Logradouro? logradouro = null)
    {
        ArgumentNullException.ThrowIfNull(colaborador);
        return new ColaboradorDto
        {
            Id = colaborador.Id,
            Nome = colaborador.Nome,
            Cpf = colaborador.Cpf.Valor,
            DataNascimento = colaborador.DataNascimento,
            Telefone = colaborador.Telefone.Valor,
            Email = colaborador.Email?.Valor,
            Endereco = logradouro?.ToDto(),
            Numero = colaborador.Endereco?.Numero ?? string.Empty,
            Complemento = colaborador.Endereco?.Complemento,
            Senha = null,
            Foto = colaborador.Foto?.Conteudo != null ? new ArquivoDto { Conteudo = colaborador.Foto.Conteudo } : null,
            DataAdmissao = colaborador.DataAdmissao,
            Tipo = colaborador.Tipo.ToApplication(),
            Vinculo = colaborador.Vinculo.ToApplication()
        };
    }
    public static Colaborador ToEntity(this ColaboradorDto colaboradorDto, Logradouro? logradouro = null)
    {
        ArgumentNullException.ThrowIfNull(colaboradorDto);
        var logradouroEntidade = logradouro ?? (colaboradorDto.Endereco != null ? colaboradorDto.Endereco.ToEntity() : null) ?? throw new InvalidOperationException("Logradouro/Endereço é obrigatório para converter o Colaborador.");
        Arquivo? foto = null;
        if (colaboradorDto.Foto?.Conteudo != null)
        {
            var fotoResult = Arquivo.Criar(colaboradorDto.Foto.Conteudo);
            if (fotoResult.IsSuccess) foto = fotoResult.Value;
        }
        var result = Colaborador.Criar(
        colaboradorDto.Id,
        colaboradorDto.Nome,
        colaboradorDto.Cpf,
        colaboradorDto.DataNascimento,
        colaboradorDto.Telefone,
        colaboradorDto.Email ?? string.Empty,
        logradouroEntidade,
        colaboradorDto.Numero,
        colaboradorDto.Complemento ?? string.Empty,
        colaboradorDto.Senha ?? string.Empty,
        foto!,
        colaboradorDto.DataAdmissao,
        colaboradorDto.Tipo.ToDomain(),
        colaboradorDto.Vinculo.ToDomain()
        );
        if (result.IsFailure)
        {
            throw new InvalidOperationException($"Erro de validação ao converter Colaborador: {string.Join(", ", result.Notifications.Select(n => n.Mensagem))}");
        }
        return result.Value!;
    }
    public static Colaborador UpdateFromDto(this Colaborador colaborador, ColaboradorDto colaboradorDto, Logradouro? logradouro = null)
    {
        ArgumentNullException.ThrowIfNull(colaborador);
        ArgumentNullException.ThrowIfNull(colaboradorDto);
        var logradouroEntidade = logradouro ?? (colaboradorDto.Endereco != null ? colaboradorDto.Endereco.ToEntity() : null) ?? throw new InvalidOperationException("Logradouro/Endereço é obrigatório para atualizar o Colaborador.");
        Arquivo? foto = colaborador.Foto;
        if (colaboradorDto.Foto?.Conteudo != null)
        {
            var fotoResult = Arquivo.Criar(colaboradorDto.Foto.Conteudo);
            if (fotoResult.IsSuccess) foto = fotoResult.Value;
        }
        string senha = !string.IsNullOrWhiteSpace(colaboradorDto.Senha) ? colaboradorDto.Senha : colaborador.Senha.Valor;
        var result = Colaborador.Criar(
        colaborador.Id,
        colaboradorDto.Nome ?? colaborador.Nome,
        colaborador.Cpf.Valor,
        colaboradorDto.DataNascimento != default ? colaboradorDto.DataNascimento : colaborador.DataNascimento,
        colaboradorDto.Telefone ?? colaborador.Telefone.Valor,
        colaboradorDto.Email ?? colaborador.Email.Valor,
        logradouroEntidade,
        colaboradorDto.Numero ?? colaborador.Endereco.Numero,
        colaboradorDto.Complemento ?? colaborador.Endereco.Complemento,
        senha,
        foto!,
        colaboradorDto.DataAdmissao != default ? colaboradorDto.DataAdmissao : colaborador.DataAdmissao,
        colaboradorDto.Tipo != default ? colaboradorDto.Tipo.ToDomain() : colaborador.Tipo,
        colaboradorDto.Vinculo != default ? colaboradorDto.Vinculo.ToDomain() : colaborador.Vinculo
        );
        if (result.IsFailure)
        {
            throw new InvalidOperationException($"Erro de validação ao atualizar Colaborador: {string.Join(", ", result.Notifications.Select(n => n.Mensagem))}");
        }
        return result.Value!;
    }
}