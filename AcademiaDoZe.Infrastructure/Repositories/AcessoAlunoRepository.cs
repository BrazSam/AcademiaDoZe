using AcademiaDoZe.Domain.Entities; //Samuel Braz dos Santos
using AcademiaDoZe.Domain.Repositories;
using AcademiaDoZe.Infrastructure.Data;
using AcademiaDoZe.Infrastructure.Exceptions;
using System.Data;
using System.Data.Common;
namespace AcademiaDoZe.Infrastructure.Repositories;

public class AcessoAlunoRepository : BaseRepository, IAcessoAlunoRepository
{
    // tb_acesso é compartilhada: 0 = Aluno, 1 = Colaborador
    private const int PessoaTipoAluno = 0;

    public AcessoAlunoRepository(string connectionString, DatabaseType databaseType) : base(connectionString, databaseType)
    {
    }

    private static string BaseSelectQuery => $"""
SELECT
ac.id_acesso, ac.pessoa_id, ac.data_hora,
a.id_aluno, a.cpf, a.nome AS aluno_nome, a.nascimento, a.telefone, a.email, a.logradouro_id, a.numero, a.complemento, a.senha, a.foto,
l.id_logradouro, l.cep, l.nome AS logradouro_nome, l.bairro, l.cidade, l.estado, l.pais
FROM tb_acesso ac
INNER JOIN tb_aluno a ON ac.pessoa_id = a.id_aluno
INNER JOIN tb_logradouro l ON a.logradouro_id = l.id_logradouro
WHERE ac.pessoa_tipo = {PessoaTipoAluno}
""";
    public static AcessoAluno Map(DbDataReader reader)
    {
        try
        {
            int id = reader.GetInt32Value("id_acesso");
            DateTime dataHora = reader.GetDateTimeValue("data_hora");
            var aluno = AlunoRepository.Map(reader, "aluno_nome");
            var result = AcessoAluno.Criar(id, aluno, dataHora);
            if (result.IsFailure)
            {
                throw new InfrastructureException("ERRO_DOMINIO_MAPEAMENTO", $"Erro de domínio ao mapear acesso de aluno ID {id}: {string.Join(", ", result.Notifications.Select(n => n.Mensagem))}");
            }
            return result.Value!;
        }
        catch (Exception ex) when (ex is not InfrastructureException)
        {
            throw new InfrastructureException("ERRO_MAPEAMENTO_ACESSO_ALUNO", $"Erro ao mapear dados do acesso de aluno: {ex.Message}", ex);
        }
    }

    public async Task<AcessoAluno?> ObterPorId(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            string query = $"{BaseSelectQuery} AND ac.id_acesso = @Id";
            await using var command = await CreateCommandAsync(query, cancellationToken);
            command.AddParameter("@Id", id, DbType.Int32);
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            return await reader.ReadAsync(cancellationToken) ? Map(reader) : null;
        }
        catch (DbException ex)
        {
            throw new InfrastructureException("ERRO_OBTER_POR_ID", $"Erro ao obter acesso de aluno por ID {id}: {ex.Message}", ex);
        }
    }

    public async Task<IEnumerable<AcessoAluno>> ObterTodos(CancellationToken cancellationToken = default)
    {
        try
        {
            string query = $"{BaseSelectQuery} ORDER BY ac.data_hora DESC, ac.id_acesso DESC";
            await using var command = await CreateCommandAsync(query, cancellationToken);
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            var acessos = new List<AcessoAluno>();
            while (await reader.ReadAsync(cancellationToken))
            {
                acessos.Add(Map(reader));
            }
            return acessos;
        }
        catch (DbException ex)
        {
            throw new InfrastructureException("ERRO_OBTER_TODOS", $"Erro ao obter todos os acessos de alunos: {ex.Message}", ex);
        }
    }

    public async Task<AcessoAluno> Adicionar(AcessoAluno entity, CancellationToken cancellationToken = default)
    {
        try
        {
            string query = FormatInsertQuery("INSERT INTO tb_acesso (pessoa_tipo, pessoa_id, data_hora) VALUES (@PessoaTipo, @PessoaId, @DataHora)");
            await using var command = await CreateCommandAsync(query, cancellationToken);
            command.AddParameter("@PessoaTipo", PessoaTipoAluno, DbType.Int32);
            command.AddParameter("@PessoaId", entity.AlunoId, DbType.Int32);
            command.AddParameter("@DataHora", entity.DataHora, DbType.DateTime);
            int id = await command.ExecuteScalarIdAsync("ERRO_ADICIONAR_ACESSO_ALUNO", "Falha ao obter ID inserido para o acesso do aluno.", cancellationToken);
            var idProperty = typeof(Entity).GetProperty("Id");
            idProperty?.SetValue(entity, id);
            return entity;
        }
        catch (DbException ex)
        {
            throw new InfrastructureException("ERRO_ADICIONAR_ACESSO_ALUNO", $"Erro ao adicionar acesso do aluno ID {entity.AlunoId}: {ex.Message}", ex);
        }
    }

    public async Task<AcessoAluno> Atualizar(AcessoAluno entity, CancellationToken cancellationToken = default)
    {
        try
        {
            string query = "UPDATE tb_acesso SET pessoa_id = @PessoaId, data_hora = @DataHora WHERE id_acesso = @Id AND pessoa_tipo = @PessoaTipo";
            await using var command = await CreateCommandAsync(query, cancellationToken);
            command.AddParameter("@Id", entity.Id, DbType.Int32);
            command.AddParameter("@PessoaTipo", PessoaTipoAluno, DbType.Int32);
            command.AddParameter("@PessoaId", entity.AlunoId, DbType.Int32);
            command.AddParameter("@DataHora", entity.DataHora, DbType.DateTime);
            int rowsAffected = await command.ExecuteNonQueryAsync(cancellationToken);
            if (rowsAffected == 0)
            {
                throw new InfrastructureException("REGISTRO_NAO_ENCONTRADO", $"Nenhum acesso de aluno encontrado com o ID {entity.Id} para atualização.");
            }
            return entity;
        }
        catch (DbException ex)
        {
            throw new InfrastructureException("ERRO_ATUALIZAR_ACESSO_ALUNO", $"Erro ao atualizar acesso ID {entity.Id} do aluno ID {entity.AlunoId}: {ex.Message}", ex);
        }
    }

    public async Task<bool> Remover(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            string query = "DELETE FROM tb_acesso WHERE id_acesso = @Id AND pessoa_tipo = @PessoaTipo";
            await using var command = await CreateCommandAsync(query, cancellationToken);
            command.AddParameter("@Id", id, DbType.Int32);
            command.AddParameter("@PessoaTipo", PessoaTipoAluno, DbType.Int32);
            var result = await command.ExecuteNonQueryAsync(cancellationToken);
            return result > 0;
        }
        catch (DbException ex)
        {
            throw new InfrastructureException("ERRO_REMOVER_ACESSO_ALUNO", $"Erro ao remover acesso de aluno ID {id}: {ex.Message}", ex);
        }
    }

    public async Task<IEnumerable<AcessoAluno>> ObterAcessosPorAlunoPeriodo(int? alunoId = null, DateOnly? inicio = null, DateOnly? fim = null, CancellationToken cancellationToken = default)
    {
        try
        {
            // Filtros opcionais: só entram no SQL quando forem informados
            string query = BaseSelectQuery;
            if (alunoId.HasValue) query += " AND ac.pessoa_id = @AlunoId";
            if (inicio.HasValue) query += " AND CAST(ac.data_hora AS DATE) >= @Inicio";
            if (fim.HasValue) query += " AND CAST(ac.data_hora AS DATE) <= @Fim";
            query += " ORDER BY ac.data_hora, ac.id_acesso";

            await using var command = await CreateCommandAsync(query, cancellationToken);
            if (alunoId.HasValue) command.AddParameter("@AlunoId", alunoId.Value, DbType.Int32);
            if (inicio.HasValue) command.AddParameter("@Inicio", inicio.Value, DbType.Date);
            if (fim.HasValue) command.AddParameter("@Fim", fim.Value, DbType.Date);
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            var acessos = new List<AcessoAluno>();
            while (await reader.ReadAsync(cancellationToken))
            {
                acessos.Add(Map(reader));
            }
            return acessos;
        }
        catch (DbException ex)
        {
            throw new InfrastructureException("ERRO_OBTER_ACESSOS_PERIODO", $"Erro ao obter acessos de alunos por período: {ex.Message}", ex);
        }
    }

    public async Task<AcessoAluno?> ObterUltimoAcesso(int alunoId, CancellationToken cancellationToken = default)
    {
        try
        {
            // TOP 1 + ORDER BY DESC = só o registro mais recente
            string query = BaseSelectQuery.Replace("SELECT", "SELECT TOP 1")
                + " AND ac.pessoa_id = @AlunoId ORDER BY ac.data_hora DESC, ac.id_acesso DESC";
            await using var command = await CreateCommandAsync(query, cancellationToken);
            command.AddParameter("@AlunoId", alunoId, DbType.Int32);
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            return await reader.ReadAsync(cancellationToken) ? Map(reader) : null;
        }
        catch (DbException ex)
        {
            throw new InfrastructureException("ERRO_OBTER_ULTIMO_ACESSO", $"Erro ao obter último acesso do aluno ID {alunoId}: {ex.Message}", ex);
        }
    }

    public async Task<bool> EstaNaAcademia(int alunoId, CancellationToken cancellationToken = default)
    {
        try
        {
            // Regra: no mesmo dia, 1ª passagem = entrada, 2ª = saída, 3ª = entrada...
            // Quantidade ÍMPAR de passagens hoje = entrou e ainda não saiu.
            string query = """
SELECT COUNT(*) FROM tb_acesso
WHERE pessoa_tipo = @PessoaTipo AND pessoa_id = @AlunoId AND CAST(data_hora AS DATE) = @Hoje
""";
            await using var command = await CreateCommandAsync(query, cancellationToken);
            command.AddParameter("@PessoaTipo", PessoaTipoAluno, DbType.Int32);
            command.AddParameter("@AlunoId", alunoId, DbType.Int32);
            command.AddParameter("@Hoje", DateOnly.FromDateTime(DateTime.Now), DbType.Date);
            int passagensHoje = Convert.ToInt32(await command.ExecuteScalarAsync(cancellationToken));
            return passagensHoje % 2 == 1;
        }
        catch (DbException ex)
        {
            throw new InfrastructureException("ERRO_VERIFICAR_PRESENCA", $"Erro ao verificar se o aluno ID {alunoId} está na academia: {ex.Message}", ex);
        }
    }


    // Base dos relatórios: numera as passagens de cada aluno em cada dia e pega a passagem seguinte.
    // ordem ímpar = ENTRADA; "proxima" = a SAÍDA correspondente (NULL se ainda não saiu).
    private static string PassagensDoMesCte => """
WITH passagens AS (
 SELECT data_hora,
  ROW_NUMBER() OVER (PARTITION BY pessoa_id, CAST(data_hora AS DATE) ORDER BY data_hora, id_acesso) AS ordem,
  LEAD(data_hora) OVER (PARTITION BY pessoa_id, CAST(data_hora AS DATE) ORDER BY data_hora, id_acesso) AS proxima
 FROM tb_acesso
 WHERE pessoa_tipo = @PessoaTipo AND YEAR(data_hora) = @Ano AND MONTH(data_hora) = @Mes
)
""";

    private static void ValidarMes(int mes)
    {
        if (mes < 1 || mes > 12)
            throw new InfrastructureException("MES_INVALIDO", $"Mês inválido: {mes}. Informe um valor entre 1 e 12.");
    }

    public async Task<Dictionary<TimeOnly, int>> ObterHorarioMaisProcuradoPorMes(int mes, CancellationToken cancellationToken = default)
    {
        ValidarMes(mes);
        try
        {
            string query = PassagensDoMesCte + """
SELECT DATEPART(HOUR, data_hora) AS hora, COUNT(*) AS quantidade
FROM passagens
WHERE ordem % 2 = 1
GROUP BY DATEPART(HOUR, data_hora)
ORDER BY quantidade DESC, hora
""";
            await using var command = await CreateCommandAsync(query, cancellationToken);
            command.AddParameter("@PessoaTipo", PessoaTipoAluno, DbType.Int32);
            command.AddParameter("@Ano", DateTime.Today.Year, DbType.Int32);
            command.AddParameter("@Mes", mes, DbType.Int32);
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            var horarios = new Dictionary<TimeOnly, int>();
            while (await reader.ReadAsync(cancellationToken))
            {
                horarios.Add(new TimeOnly(reader.GetInt32Value("hora"), 0), reader.GetInt32Value("quantidade"));
            }
            return horarios;
        }
        catch (DbException ex)
        {
            throw new InfrastructureException("ERRO_OBTER_HORARIO_MAIS_PROCURADO", $"Erro ao obter horário mais procurado do mês {mes}: {ex.Message}", ex);
        }
    }

    public async Task<Dictionary<int, TimeSpan>> ObterPermanenciaMediaPorMes(int mes, CancellationToken cancellationToken = default)
    {
        ValidarMes(mes);
        try
        {
            string query = PassagensDoMesCte + """
SELECT AVG(CAST(DATEDIFF(SECOND, data_hora, proxima) AS BIGINT)) AS media_segundos
FROM passagens
WHERE ordem % 2 = 1 AND proxima IS NOT NULL
""";
            await using var command = await CreateCommandAsync(query, cancellationToken);
            command.AddParameter("@PessoaTipo", PessoaTipoAluno, DbType.Int32);
            command.AddParameter("@Ano", DateTime.Today.Year, DbType.Int32);
            command.AddParameter("@Mes", mes, DbType.Int32);
            var resultado = await command.ExecuteScalarAsync(cancellationToken);
            var media = resultado is null || resultado is DBNull ? TimeSpan.Zero : TimeSpan.FromSeconds(Convert.ToInt64(resultado));
            return new Dictionary<int, TimeSpan> { [mes] = media };
        }
        catch (DbException ex)
        {
            throw new InfrastructureException("ERRO_OBTER_PERMANENCIA_MEDIA", $"Erro ao obter permanência média do mês {mes}: {ex.Message}", ex);
        }
    }

    public async Task<IEnumerable<Aluno>> ObterAlunosSemAcessoNosUltimosDias(int dias, CancellationToken cancellationToken = default)
    {
        try
        {
            string query = """
SELECT
a.id_aluno, a.cpf, a.nome, a.nascimento, a.telefone, a.email, a.logradouro_id, a.numero, a.complemento, a.senha, a.foto,
l.id_logradouro, l.cep, l.nome AS logradouro_nome, l.bairro, l.cidade, l.estado, l.pais
FROM tb_aluno a
INNER JOIN tb_logradouro l ON a.logradouro_id = l.id_logradouro
WHERE NOT EXISTS (
 SELECT 1 FROM tb_acesso ac
 WHERE ac.pessoa_tipo = @PessoaTipo AND ac.pessoa_id = a.id_aluno AND ac.data_hora >= @Limite
)
ORDER BY a.nome
""";
            await using var command = await CreateCommandAsync(query, cancellationToken);
            command.AddParameter("@PessoaTipo", PessoaTipoAluno, DbType.Int32);
            command.AddParameter("@Limite", DateTime.Today.AddDays(-dias), DbType.DateTime);
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            var alunos = new List<Aluno>();
            while (await reader.ReadAsync(cancellationToken))
            {
                alunos.Add(AlunoRepository.Map(reader));
            }
            return alunos;
        }
        catch (DbException ex)
        {
            throw new InfrastructureException("ERRO_OBTER_ALUNOS_SEM_ACESSO", $"Erro ao obter alunos sem acesso nos últimos {dias} dias: {ex.Message}", ex);
        }
    }
}