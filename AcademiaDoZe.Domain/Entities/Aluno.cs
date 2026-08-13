using AcademiaDoZe.Domain.Common; //Samuel Braz dos Santos
using AcademiaDoZe.Domain.Enums;
using AcademiaDoZe.Domain.Services;
using AcademiaDoZe.Domain.ValueObjects;

namespace AcademiaDoZe.Domain.Entities;

public class Aluno : Pessoa
{
    // encapsulamento das propriedades, aplicando imutabilidade

    // construtor privado para evitar instância direta
    private Aluno(int id, string nome, Cpf cpf, DateOnly dataNascimento, Telefone telefone, Email email, Endereco endereco, Senha senha, Arquivo foto)
        : base(id, nome, cpf, dataNascimento, telefone, email, endereco, senha, foto)
    {
    }


    // método de fábrica, ponto de entrada para criar um objeto válido
    public static Result<Aluno> Criar(int id, string nome, string cpf, DateOnly dataNascimento, string telefone, string email, Logradouro endereco, string numero, string complemento,
    string senha, Arquivo foto)
    {
        var notifications = new List<Notification>();
        // Validações e normalizações
        if (NormalizadoService.TextoVazioOuNulo(nome))
            notifications.Add(new Notification("Nome", "NOME_OBRIGATORIO"));
        else
            nome = NormalizadoService.LimparEspacos(nome);
        if (dataNascimento == default)
            notifications.Add(new Notification("DataNascimento", "DATA_NASCIMENTO_OBRIGATORIO"));
        else if (dataNascimento > DateOnly.FromDateTime(DateTime.Today.AddYears(-12)))
            notifications.Add(new Notification("DataNascimento", "DATA_NASCIMENTO_MINIMA_INVALIDA"));
        if (dataAdmissao == default)
            notifications.Add(new Notification("DataAdmissao", "DATA_ADMISSAO_OBRIGATORIO"));
        else if (dataAdmissao > DateOnly.FromDateTime(DateTime.Today))
            notifications.Add(new Notification("DataAdmissao", "DATA_ADMISSAO_MAIOR_ATUAL"));
        if (!Enum.IsDefined(tipo))
            notifications.Add(new Notification("Tipo", "TIPO_COLABORADOR_INVALIDO"));
        if (!Enum.IsDefined(vinculo))
            notifications.Add(new Notification("Vinculo", "VINCULO_COLABORADOR_INVALIDO"));
        if (Enum.IsDefined(tipo) && Enum.IsDefined(vinculo) && tipo == ColaboradorTipo.Administrador && vinculo != ColaboradorVinculo.CLT)
            notifications.Add(new Notification("Vinculo", "ADMINISTRADOR_CLT_INVALIDO"));
        // Instanciação e validação via Value Objects
        var cpfResult = Cpf.Criar(cpf);
        if (cpfResult.IsFailure) notifications.AddRange(cpfResult.Notifications);
        var telefoneResult = Telefone.Criar(telefone);
        if (telefoneResult.IsFailure) notifications.AddRange(telefoneResult.Notifications);
        var emailResult = Email.Criar(email);
        if (emailResult.IsFailure) notifications.AddRange(emailResult.Notifications);
        var senhaResult = Senha.Criar(senha);
        if (senhaResult.IsFailure) notifications.AddRange(senhaResult.Notifications);
        var enderecoResult = Endereco.Criar(endereco, numero, complemento);
        if (enderecoResult.IsFailure) notifications.AddRange(enderecoResult.Notifications);
        if (notifications.Count != 0)
            return Result<Colaborador>.Failure(notifications);
        // criação e retorno do objeto
        var colaborador = new Colaborador(id, nome, cpfResult.Value!, dataNascimento, telefoneResult.Value!, emailResult.Value!, enderecoResult.Value!, senhaResult.Value!, foto, dataAdmissao, tipo, vinculo);
        return Result<Colaborador>.Success(colaborador);
    }
}