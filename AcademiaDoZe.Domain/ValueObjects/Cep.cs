using AcademiaDoZe.Domain.Common; //Samuel Braz dos Santos
using AcademiaDoZe.Domain.Services;
namespace AcademiaDoZe.Domain.ValueObjects;

public record Cep
{
    public string Valor { get; }
    private Cep(string valor)
    {
        Valor = valor;
    }
    public static Result<Cep> Criar(string valor)
    {
        if (NormalizacaoService.TextoVazioOuNulo(valor)) return Result<Cep>.Failure("Cep", "CEP_OBRIGATORIO"); var textoLimpo = NormalizacaoService.LimparEDigitos(valor); if (textoLimpo.Length != 8)
            return Result<Cep>.Failure("Cep", "CEP_DIGITOS"); return Result<Cep>.Success(new Cep(textoLimpo));
    }
    public override string ToString() => Valor;
}