using AcademiaDoZe.Domain.Common;
using AcademiaDoZe.Domain.Services;
using AcademiaDoZe.Domain.ValueObjects;
namespace AcademiaDoZe.Domain.Entities;

public sealed class Logradouro : Entity
{
    // encapsulamento das propriedades, aplicando imutabilidade
    public Cep Cep { get; }
    public string Nome { get; }
    public string Bairro { get; }
    public string Cidade { get; }
    public string Estado { get; }
    public string Pais { get; }
    // construtor privado para evitar instância direta
    private Logradouro(int id, Cep cep, string nome, string bairro, string cidade, string estado, string pais) : base(id)
    {
        Cep = cep;
        Nome = nome;
        Bairro = bairro;
        Cidade = cidade;
        Estado = estado;
        Pais = pais;
    }
    public static Result<Logradouro> Criar(int id, string cep, string nome, string bairro, string cidade, string estado, string pais)
    {
        var notifications = new List<Notification>();
        var cepResult = Cep.Criar(cep);
        if (cepResult.IsFailure)
            notifications.AddRange(cepResult.Notifications);
        if (NormalizadoService.TextoVazioOuNulo(nome))
            notifications.Add(new Notification("Nome", "NOME_OBRIGATORIO"));
        else
            nome = NormalizadoService.LimparEspacos(nome);
        if (NormalizadoService.TextoVazioOuNulo(bairro))
            notifications.Add(new Notification("Bairro", "BAIRRO_OBRIGATORIO"));
        else
            bairro = NormalizadoService.LimparEspacos(bairro);
        if (NormalizadoService.TextoVazioOuNulo(cidade))
            notifications.Add(new Notification("Cidade", "CIDADE_OBRIGATORIO"));
        else
            cidade = NormalizadoService.LimparEspacos(cidade);
        if (NormalizadoService.TextoVazioOuNulo(estado))
            notifications.Add(new Notification("Estado", "ESTADO_OBRIGATORIO"));
        else
            estado = NormalizadoService.ParaMaiusculo(NormalizadoService.LimparTodosEspacos(estado));
        if (NormalizadoService.TextoVazioOuNulo(pais))
            notifications.Add(new Notification("Pais", "PAIS_OBRIGATORIO"));
        else
            pais = NormalizadoService.LimparEspacos(pais);
        if (notifications.Count != 0)
            return Result<Logradouro>.Failure(notifications);
        return Result<Logradouro>.Success(new Logradouro(id, cepResult.Value!, nome, bairro, cidade, estado, pais));
    }
}