using AcademiaDoZe.Domain.Common; //Samuel Braz dos Santos
using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.Services;
using static System.Runtime.InteropServices.JavaScript.JSType;
namespace AcademiaDoZe.Domain.ValueObjects;

public record Endereco
{
    public int LogradouroId { get; }
    public string Numero { get; }
    public string Complemento { get; }
    private Endereco(int logradouroId, string numero, string complemento)
    {
        LogradouroId = logradouroId;
        Numero = numero;
        Complemento = complemento;
    }
    public static Result<Endereco> Criar(Logradouro logradouro, string numero, string complemento)
    {
        var notifications = new List<Notification>();
        if (logradouro == null)
            notifications.Add(new Notification("Endereco", "LOGRADOURO_OBRIGATORIO")); if (NormalizacaoService.TextoVazioOuNulo(numero))
            notifications.Add(new Notification("Numero", "NUMERO_OBRIGATORIO"));
        else
            numero = NormalizacaoService.LimparEspacos(numero);
        complemento = NormalizacaoService.LimparEspacos(complemento);
        if (notifications.Count != 0)
            return Result<Endereco>.Failure(notifications);
        return Result<Endereco>.Success(new Endereco(logradouro!.Id, numero, complemento));
    }
}