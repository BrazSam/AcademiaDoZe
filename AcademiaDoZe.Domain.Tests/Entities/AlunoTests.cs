using AcademiaDoZe.Domain.Entities; //Samuel Braz dos Santos
using AcademiaDoZe.Domain.ValueObjects;

namespace AcademiaDoZe.Domain.Tests.Entities;

public class AlunoTests
{
    private static Logradouro GetValidLogradouro() => Logradouro.Criar(1, "12345-678", "Rua Teste", "Bairro", "Cidade", "SP", "Brasil").Value!;
    private static Arquivo GetValidArquivo() => Arquivo.Criar(new byte[] { 1, 2, 3 }).Value!;

    [Theory(DisplayName = "Aluno: criação bem-sucedida com nomes válidos (trim aplicado)")]
    [InlineData(" João da Silva ")]
    [InlineData("Maria")]
    public void Deve_Criar_Com_Sucesso_Quando_NomeValido(string nome)
    {
        var result = Aluno.Criar(
            1,
            nome,
            "529.982.247-25",
            DateOnly.FromDateTime(DateTime.Today.AddYears(-25)),
            "(11) 91234-5678",
            "user@example.com",
            GetValidLogradouro(),
            "123",
            "",
            "Abcdef",
            GetValidArquivo()
        );
        Assert.True(result.IsSuccess);
        Assert.Equal(nome.Trim(), result.Value!.Nome);
    }

    [Theory(DisplayName = "Aluno: nome vazio -> NOME_OBRIGATORIO")]
    [InlineData("")]
    [InlineData(" ")]
    public void Deve_Falhar_Criacao_Quando_NomeVazio(string nome)
    {
        var result = Aluno.Criar(
            1,
            nome,
            "529.982.247-25",
            DateOnly.FromDateTime(DateTime.Today.AddYears(-25)),
            "(11) 91234-5678",
            "user@example.com",
            GetValidLogradouro(),
            "123",
            "",
            "Abcdef",
            GetValidArquivo()
        );
        Assert.True(result.IsFailure);
        Assert.NotEmpty(result.Notifications);
        Assert.Contains(result.Notifications, n => n.Mensagem == "NOME_OBRIGATORIO");
    }

    [Theory(DisplayName = "Aluno: data nascimento -> obrigatoriedade e idade mínima")]
    [InlineData("default", "DATA_NASCIMENTO_OBRIGATORIO")]
    [InlineData("under12", "DATA_NASCIMENTO_MINIMA_INVALIDA")]
    public void Deve_Falhar_Criacao_Quando_DataNascimentoInvalida(string scenario, string expectedMessage)
    {
        DateOnly? data = scenario == "default" ? default(DateOnly?) : DateOnly.FromDateTime(DateTime.Today.AddYears(-10));
        var dataParam = data.HasValue ? data.Value : default(DateOnly);
        var result = Aluno.Criar(
            1,
            "João",
            "529.982.247-25",
            dataParam,
            "(11) 91234-5678",
            "user@example.com",
            GetValidLogradouro(),
            "123",
            "",
            "Abcdef",
            GetValidArquivo()
        );
        Assert.True(result.IsFailure);
        Assert.NotEmpty(result.Notifications);
        Assert.Contains(result.Notifications, n => n.Mensagem == expectedMessage);
    }
}