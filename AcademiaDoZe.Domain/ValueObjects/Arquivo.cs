using AcademiaDoZe.Domain.Common; //Samuel Braz dos Santos
namespace AcademiaDoZe.Domain.ValueObjects;

public record Arquivo
{
    public byte[] Conteudo { get; }
    private Arquivo(byte[] conteudo)
    {
        Conteudo = conteudo;
    }
    public static Result<Arquivo> Criar(byte[] conteudo)
    {
        if (conteudo == null)
            return Result<Arquivo>.Failure("Arquivo", "ARQUIVO_OBRIGATORIO");
        const int tamanhoMaximoBytes = 15 * 1024 * 1024; // 15MB
        if (conteudo.Length > tamanhoMaximoBytes)
            return Result<Arquivo>.Failure("Arquivo", "ARQUIVO_TIPO_TAMANHO");
        // cria e retorna o objeto
        return Result<Arquivo>.Success(new Arquivo(conteudo));
    }
}