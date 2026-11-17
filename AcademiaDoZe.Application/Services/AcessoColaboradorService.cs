using AcademiaDoZe.Application.DTOs; //Samuel Braz dos Santos
using AcademiaDoZe.Application.Enums;
using AcademiaDoZe.Application.Interfaces;
using AcademiaDoZe.Application.Mappings;
using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.Repositories;
using AcademiaDoZe.Domain.ValueObjects;
namespace AcademiaDoZe.Application.Services;

public class AcessoColaboradorService : IAcessoColaboradorService
{
    private readonly Func<IAcessoColaboradorRepository> _acessoRepoFactory;
    private readonly Func<IColaboradorRepository> _colaboradorRepoFactory;
    private readonly TimeProvider _relogio;
    public AcessoColaboradorService(Func<IAcessoColaboradorRepository> acessoRepoFactory, Func<IColaboradorRepository> colaboradorRepoFactory, TimeProvider? relogio = null)
    {
        _acessoRepoFactory = acessoRepoFactory ?? throw new ArgumentNullException(nameof(acessoRepoFactory));
        _colaboradorRepoFactory = colaboradorRepoFactory ?? throw new ArgumentNullException(nameof(colaboradorRepoFactory));
        _relogio = relogio ?? TimeProvider.System; // no app: relógio real; nos testes: relógio fixo
    }

    public async Task<AcessoColaboradorDto> RegistrarAcessoAsync(string cpf, CancellationToken cancellationToken = default)
    {
        // 1. Validar o CPF e buscar o colaborador
        if (string.IsNullOrWhiteSpace(cpf))
            throw new ArgumentException("Informe o CPF.", nameof(cpf));
        var cpfResult = Cpf.Criar(cpf);
        if (cpfResult.IsFailure)
            throw new ArgumentException("CPF inválido.", nameof(cpf));

        using var colaboradorRepo = _colaboradorRepoFactory();
        // ObterPorCpf busca por prefixo (LIKE); garantimos o CPF exato
        var colaborador = (await colaboradorRepo.ObterPorCpf(cpfResult.Value!.Valor, cancellationToken))
            .FirstOrDefault(c => c.Cpf.Valor == cpfResult.Value!.Valor)
            ?? throw new InvalidOperationException("Colaborador não encontrado para o CPF informado.");

        // 2. Entrada ou saída: quantidade ÍMPAR de passagens hoje = está dentro, então esta é a saída
        var agora = _relogio.GetLocalNow().DateTime;
        var hoje = DateOnly.FromDateTime(agora);
        using var acessoRepo = _acessoRepoFactory();
        var passagensHoje = await acessoRepo.ObterAcessosPorColaboradorPeriodo(colaborador.Id, hoje, hoje, cancellationToken);
        var tipo = passagensHoje.Count() % 2 == 1 ? AppAcessoTipo.Saida : AppAcessoTipo.Entrada;

        // 3. Criar a entidade (o domínio valida o horário de 06h às 22h) e gravar
        var acessoResult = AcessoColaborador.Criar(0, colaborador, agora);
        if (acessoResult.IsFailure)
        {
            if (acessoResult.Notifications.Any(n => n.Mensagem == "DATA_HORA_INTERVALO_INVALIDO"))
                throw new InvalidOperationException("Acesso permitido somente entre 06h e 22h.");
            throw new InvalidOperationException(string.Join(", ", acessoResult.Notifications.Select(n => n.Mensagem)));
        }
        var acesso = await acessoRepo.Adicionar(acessoResult.Value!, cancellationToken);

        // 4. Devolver o DTO para a tela mostrar nome, data/hora e tipo
        return acesso.ToDto(colaborador.ToDto(), tipo);
    }
}