using AcademiaDoZe.Domain.Common;
using AcademiaDoZe.Domain.Services;
namespace AcademiaDoZe.Domain.ValueObjects;

public record Telefone
{
    public string Valor { get; }
    private Telefone(string valor)
    {
        Valor = valor;
    }
    public static Result<Telefone> Criar(string valor)
    {
        if (NormalizadoService.TextoVazioOuNulo(valor)) return Result<Telefone>.Failure("Telefone", "TELEFONE_OBRIGATORIO"); var textoLimpo = NormalizadoService.LimparEDigitos(valor); if (textoLimpo.Length != 11)
            return Result<Telefone>.Failure("Telefone", "TELEFONE_DIGITOS"); return Result<Telefone>.Success(new Telefone(textoLimpo));
    }
    public override string ToString() => Valor;
}