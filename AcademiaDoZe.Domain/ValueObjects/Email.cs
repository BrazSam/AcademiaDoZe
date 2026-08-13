using AcademiaDoZe.Domain.Common;
using AcademiaDoZe.Domain.Services;
namespace AcademiaDoZe.Domain.ValueObjects;

public record Email
{
    public string Valor { get; }
    private Email(string valor)
    {
        Valor = valor;
    }
    public static Result<Email> Criar(string valor)
    {
        var textoLimpo = NormalizadoService.LimparEspacos(valor); if (string.IsNullOrWhiteSpace(textoLimpo) || !ValidarFormato(textoLimpo)) return Result<Email>.Failure("Email", "EMAIL_FORMATO"); return Result<Email>.Success(new Email(textoLimpo));
    }
    private static bool ValidarFormato(string email)
    {
        var partes = email.Split('@');
        if (partes.Length != 2) return false;
        if (string.IsNullOrWhiteSpace(partes[0])) returnfalse; var dominio = partes[1];
        if (string.IsNullOrWhiteSpace(dominio)) returnfalse; if (dominio.StartsWith('.') || dominio.EndsWith('.')) returnfalse; var labels = dominio.Split('.');
        if (labels.Length < 2) return false;
        if (labels.Any(l => string.IsNullOrWhiteSpace(l))) returnfalse; return true;
    }
    public override string ToString() => Valor;
}