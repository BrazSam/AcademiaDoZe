using AcademiaDoZe.Domain.Common; //Samuel Braz dos Santos
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
        if (NormalizacaoService.TextoVazioOuNulo(valor))
            return Result<Email>.Failure("Email", "EMAIL_OBRIGATORIO");
        var textoLimpo = NormalizacaoService.LimparEspacos(valor);
        if (!ValidarFormato(textoLimpo))
            return Result<Email>.Failure("Email", "EMAIL_FORMATO");
        return Result<Email>.Success(new Email(textoLimpo));
    }
    private static bool ValidarFormato(string email)
    {
        var partes = email.Split('@');
        if (partes.Length != 2) return false;
        if (string.IsNullOrWhiteSpace(partes[0])) return false;
        var dominio = partes[1];
        if (string.IsNullOrWhiteSpace(dominio)) return false;
        if (dominio.StartsWith('.') || dominio.EndsWith('.')) return false;
        var labels = dominio.Split('.');
        if (labels.Length < 2) return false;
        if (labels.Any(l => string.IsNullOrWhiteSpace(l))) return false;
        return true;
    }
    public override string ToString() => Valor;
}