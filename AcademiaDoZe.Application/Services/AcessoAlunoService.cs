using AcademiaDoZe.Application.DTOs; //Samuel Braz dos Santos
using AcademiaDoZe.Application.Enums;
using AcademiaDoZe.Application.Interfaces;
using AcademiaDoZe.Application.Mappings;
using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.Repositories;
using AcademiaDoZe.Domain.ValueObjects;
namespace AcademiaDoZe.Application.Services;

public class AcessoAlunoService : IAcessoAlunoService
{
    private readonly Func<IAcessoAlunoRepository> _acessoRepoFactory;
    private readonly Func<IAlunoRepository> _alunoRepoFactory;
    private readonly Func<IMatriculaRepository> _matriculaRepoFactory;
    public AcessoAlunoService(Func<IAcessoAlunoRepository> acessoRepoFactory, Func<IAlunoRepository> alunoRepoFactory, Func<IMatriculaRepository> matriculaRepoFactory, TimeProvider? relogio = null)
    {
        _acessoRepoFactory = acessoRepoFactory ?? throw new ArgumentNullException(nameof(acessoRepoFactory));
        _alunoRepoFactory = alunoRepoFactory ?? throw new ArgumentNullException(nameof(alunoRepoFactory));
        _matriculaRepoFactory = matriculaRepoFactory ?? throw new ArgumentNullException(nameof(matriculaRepoFactory));
        _relogio = relogio ?? TimeProvider.System; // no app: relógio real; nos testes: relógio fixo
    }

    public async Task<AcessoAlunoDto> RegistrarAcessoAsync(string cpf, CancellationToken cancellationToken = default)
    {
        // 1. Validar o CPF e buscar o aluno
        if (string.IsNullOrWhiteSpace(cpf))
            throw new ArgumentException("Informe o CPF.", nameof(cpf));
        var cpfResult = Cpf.Criar(cpf);
        if (cpfResult.IsFailure)
            throw new ArgumentException("CPF inválido.", nameof(cpf));
        
        using var alunoRepo = _alunoRepoFactory();
        var aluno = await alunoRepo.ObterPorCpf(cpfResult.Value!, cancellationToken)
            ?? throw new InvalidOperationException("Aluno não encontrado para o CPF informado.");

        // 2. Descobrir se é entrada ou saída: se já está dentro, esta passagem é a saída
        using var acessoRepo = _acessoRepoFactory();
        bool estaNaAcademia = await acessoRepo.EstaNaAcademia(aluno.Id, cancellationToken);
        var tipo = estaNaAcademia ? AppAcessoTipo.Saida : AppAcessoTipo.Entrada;

        // 3. Na ENTRADA, exigir matrícula ativa (a saída é sempre permitida para quem já entrou)
        if (tipo == AppAcessoTipo.Entrada)
        {
            using var matriculaRepo = _matriculaRepoFactory();
            if (!await matriculaRepo.PossuiMatriculaAtiva(aluno.Id, cancellationToken))
                throw new InvalidOperationException("Aluno sem matrícula ativa. Entrada não permitida.");
        }

        // 4. Criar a entidade (o domínio valida o horário de 06h às 22h) e gravar
        var acessoResult = AcessoAluno.Criar(0, aluno, _relogio.GetLocalNow().DateTime);
        if (acessoResult.IsFailure)
        {
            if (acessoResult.Notifications.Any(n => n.Mensagem == "DATA_HORA_INTERVALO_INVALIDO"))
                throw new InvalidOperationException("Acesso permitido somente entre 06h e 22h.");
            throw new InvalidOperationException(string.Join(", ", acessoResult.Notifications.Select(n => n.Mensagem)));
        }
        var acesso = await acessoRepo.Adicionar(acessoResult.Value!, cancellationToken);

        // 5. Devolver o DTO para a tela mostrar nome, data/hora e tipo
        return acesso.ToDto(aluno.ToDto(), tipo);
    }
    private readonly TimeProvider _relogio;
}