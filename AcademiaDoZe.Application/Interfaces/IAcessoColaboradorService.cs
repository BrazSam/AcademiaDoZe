using AcademiaDoZe.Application.DTOs; //Samuel Braz dos Santos
namespace AcademiaDoZe.Application.Interfaces;

public interface IAcessoColaboradorService
{
    /// <summary>
    /// Registra a passagem do colaborador pelo CPF: se estiver fora, registra ENTRADA;
    /// se estiver dentro, registra SAÍDA.
    /// </summary>
    Task<AcessoColaboradorDto> RegistrarAcessoAsync(string cpf, CancellationToken cancellationToken = default);
}