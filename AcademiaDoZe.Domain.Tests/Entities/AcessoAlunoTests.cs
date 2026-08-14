using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.ValueObjects;

namespace AcademiaDoZe.Domain.Tests.Entities;

public class AcessoAlunoTests
{
    private static Logradouro GetValidLogradouro() => Logradouro.Criar(1, "12345-678", "Rua Teste", "Bairro", "Cidade", "SP", "Brasil").Value!;
    private static Arquivo GetValidArquivo() => Arquivo.Criar(new byte[] { 1, 2, 3 }).Value!;
    private static Aluno GetValidAluno() => Aluno.Criar(
        1,
        "João",
        "529.982.247-25",
        DateOnly.FromDateTime(DateTime.Today.AddYears(-20)),
        "(11) 91234-5678",
        "user@example.com",
        GetValidLogradouro(),
        "123",
        string.Empty,
        "Abcdef",
        GetValidArquivo()
    ).Value!;

    [Theory(DisplayName = "AcessoAluno: aluno nulo -> ALUNO_INVALIDO")]
    [InlineData(true)]
    [InlineData(false)]
    public void Deve_Falhar_Criacao_Quando_AlunoEhNulo(bool alunoNull)
    {
        var aluno = alunoNull ? null : GetValidAluno();
        var result = AcessoAluno.Criar(1, aluno!, DateTime.Today.AddHours(10));
        if (alunoNull)
        {
            Assert.True(result.IsFailure);
            Assert.NotEmpty(result.Notifications);
            Assert.Contains(result.Notifications, n => n.Mensagem == "ALUNO_INVALIDO");
        }
        else
        {
            Assert.True(result.IsSuccess);
        }
    }

    [Theory(DisplayName = "AcessoAluno: horario fora do intervalo -> DATAHORA_INTERVALO")]
    [InlineData(5, 59)]
    [InlineData(22, 1)]
    public void Deve_Falhar_Criacao_Quando_HorarioForaDoIntervalo(int hour, int minute)
    {
        var aluno = GetValidAluno();
        var dataHora = DateTime.Today.AddHours(hour).AddMinutes(minute);
        var result = AcessoAluno.Criar(1, aluno, dataHora);
        Assert.True(result.IsFailure);
        Assert.NotEmpty(result.Notifications);
        Assert.Contains(result.Notifications, n => n.Mensagem == "DATA_HORA_INTERVALO_INVALIDO");
    }

    [Theory(DisplayName = "AcessoAluno: criação bem-sucedida em horários permitidos")]
    [InlineData(10)]
    [InlineData(11)]
    public void Deve_Criar_Com_Sucesso_Quando_HorarioValido(int hour)
    {
        var aluno = GetValidAluno();
        var dataHora = DateTime.Today.AddHours(hour); // 10:00 or 11:00
        var result = AcessoAluno.Criar(1, aluno, dataHora);
        Assert.True(result.IsSuccess);
        Assert.Equal(aluno.Id, result.Value!.AlunoId);
        Assert.Equal(dataHora, result.Value.DataHora);
    }

    [Theory(DisplayName = "AcessoAluno: permite horários de borda 06:00 e 22:00")]
    [InlineData(6)]
    [InlineData(22)]
    public void Deve_Permitir_HorariosDeBorda_06_00_e_22_00(int hour)
    {
        var aluno = GetValidAluno();
        var time = DateTime.Today.AddHours(hour);
        var r = AcessoAluno.Criar(1, aluno, time);
        Assert.True(r.IsSuccess);
    }
}