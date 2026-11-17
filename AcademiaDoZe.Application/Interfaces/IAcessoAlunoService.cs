using AcademiaDoZe.Application.DTOs; //Samuel Braz dos Santos
namespace AcademiaDoZe.Application.Interfaces;

public interface IAcessoAlunoService
{
    /// <summary>
    /// Registra a passagem do aluno pelo CPF: se estiver fora, registra ENTRADA (exige matrícula ativa);
    /// se estiver dentro, registra SAÍDA.
    /// </summary>
    Task<AcessoAlunoDto> RegistrarAcessoAsync(string cpf, CancellationToken cancellationToken = default);
}