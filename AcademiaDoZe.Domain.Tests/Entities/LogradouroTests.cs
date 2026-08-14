using AcademiaDoZe.Domain.Entities; //Samuel Braz dos Santos
namespace AcademiaDoZe.Domain.Tests.Entities;

public class LogradouroTests
{
    [Theory(DisplayName = "Logradouro: nome vazio -> NOME_OBRIGATORIO")]
    [InlineData("")]
    [InlineData(" ")]
    public void Deve_Falhar_Criacao_Quando_NomeVazio(string nome)
    {
        var result = Logradouro.Criar(1, "12345-678", nome, "Bairro", "Cidade", "SP", "Brasil");
        Assert.True(result.IsFailure);
        Assert.NotEmpty(result.Notifications);
        Assert.Contains(result.Notifications, n => n.Mensagem == "NOME_OBRIGATORIO");
    }
    [Theory(DisplayName = "Logradouro: normaliza estado removendo espaços e upper")]
    [InlineData(" s p ", "SP")]
    [InlineData(" sp ", "SP")]
    public void Deve_Normalizar_Estado_Quando_InputContemEspacos(string inputEstado, string expected)
    {
        var result = Logradouro.Criar(1, "12345-678", "Rua", "Bairro", "Cidade", inputEstado, "Brasil");
        Assert.True(result.IsSuccess);
        Assert.Equal(expected, result.Value!.Estado);
    }
    [Theory(DisplayName = "Logradouro: campos obrigatórios vazios -> mensagens específicas")]
    [InlineData("", "", "", "", "BAIRRO_OBRIGATORIO")]
    public void Deve_Falhar_Criacao_Quando_CamposObrigatoriosVazios(string rua, string bairro, string cidade, string estado, string expected)
    {
        var result = Logradouro.Criar(1, "12345-678", rua, bairro, cidade, estado, "");
        Assert.True(result.IsFailure);
        Assert.NotEmpty(result.Notifications);
        Assert.Contains(result.Notifications, n => n.Mensagem == expected);
    }

    [Theory(DisplayName = "Logradouro: criação bem-sucedida com dados válidos")]
    [InlineData("12345-678", "Rua Teste", "Bairro", "Cidade", "SP", "Brasil")]
    [InlineData("87654-321", "Av Principal", "Centro", "Outra Cidade", "RJ", "Brasil")]
    public void Deve_Criar_Com_Sucesso_Quando_DadosValidos(string cep, string nome, string bairro, string cidade, string estado, string pais)
    {
        var result = Logradouro.Criar(1, cep, nome, bairro, cidade, estado, pais);
        Assert.True(result.IsSuccess);
        Assert.Equal(nome, result.Value!.Nome);
    }

    [Theory(DisplayName = "Logradouro: campos obrigatórios vazios -> falha")]
    [InlineData("", "Rua Teste", "Bairro", "Cidade", "SP", "Brasil")]
    [InlineData("12345-678", "", "Bairro", "Cidade", "SP", "Brasil")]
    public void Deve_Falhar_Criacao_Quando_CepOuNomeVazios(string cep, string nome, string bairro, string cidade, string estado, string pais)
    {
        var result = Logradouro.Criar(1, cep, nome, bairro, cidade, estado, pais);
        Assert.True(result.IsFailure);
        Assert.NotEmpty(result.Notifications);
    }
}