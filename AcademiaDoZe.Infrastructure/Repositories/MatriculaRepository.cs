using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.Enums;
using AcademiaDoZe.Domain.Repositories;
using AcademiaDoZe.Domain.ValueObjects;
using AcademiaDoZe.Infrastructure.Data;
using AcademiaDoZe.Infrastructure.Exceptions;
using System.Data;
using System.Data.Common;
namespace AcademiaDoZe.Infrastructure.Repositories;

public class MatriculaRepository : BaseRepository, IMatriculaRepository
{
    public MatriculaRepository(string connectionString, DatabaseType databaseType) : base(connectionString, databaseType)
    {
    }
    private static string BaseSelectQuery => """
SELECT
m.id_matricula, m.aluno_id, m.plano, m.data_inicio, m.data_fim, m.objetivo, m.restricao_medica, m.obs_restricao, m.laudo_medico,a.id_aluno, a.cpf, a.nome AS aluno_nome, a.nascimento, a.telefone, a.email, a.logradouro_id, a.numero, a.complemento, a.senha, a.foto,l.id_logradouro, l.cep, l.nome AS logradouro_nome, l.bairro, l.cidade, l.estado, l.pais
FROM tb_matricula m
INNER JOIN tb_aluno a ON m.aluno_id = a.id_aluno
INNER JOIN tb_logradouro l ON a.logradouro_id = l.id_logradouro
""";
    //MÉTODOS AQUI

    public static Matricula Map(DbDataReader reader)
    {
        try
        {
            int id = reader.GetInt32Value("id_matricula");
            MatriculaPlano plano = (MatriculaPlano)reader.GetInt32Value("plano");
            DateOnly dataInicio = reader.GetDateOnlyValue("data_inicio");
            string objetivo = reader.GetStringValue("objetivo");
            MatriculaRestricoes restricoesMedicas = (MatriculaRestricoes)reader.GetInt32Value("restricao_medica");
            string obsRestricao = reader.GetNullableString("obs_restricao");
            byte[]? laudoBytes = reader.GetNullableBytes("laudo_medico");
            Arquivo? laudoMedico = laudoBytes != null ? Arquivo.Criar(laudoBytes).Value : null;
            var aluno = AlunoRepository.Map(reader, "aluno_nome");
            var result = Matricula.Criar(
            id: id,
            aluno: aluno,
            plano: plano,
            dataInicio: dataInicio,
            objetivo: objetivo,
            restricoesMedicas: restricoesMedicas,
            laudoMedico: laudoMedico,
            observacoesRestricoes: obsRestricao
            );
            if (result.IsFailure)
            {
                throw new InfrastructureException("ERRO_DOMINIO_MAPEAMENTO", $"Erro de domínio ao mapear matrícula ID {id}: {string.Join(", ", result.Notifications.Select(n => n.Mensagem))}");
            }
            return result.Value!;
        }
        catch (Exception ex) when (ex is not InfrastructureException)
        {
            throw new InfrastructureException("ERRO_MAPEAMENTO_MATRICULA", $"Erro ao mapear dados da matrícula: {ex.Message}", ex);
        }
    }

    public async Task<Matricula?> ObterPorId(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            string query = $"{BaseSelectQuery} WHERE m.id_matricula = @Id";
            await using var command = await CreateCommandAsync(query, cancellationToken);
            command.AddParameter("@Id", id, DbType.Int32);
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            return await reader.ReadAsync(cancellationToken) ? Map(reader) : null;
        }
        catch (DbException ex)
        {
            throw new InfrastructureException("ERRO_OBTER_POR_ID", $"Erro ao obter matrícula por ID {id}: {ex.Message}", ex);
        }
    }
    public async Task<IEnumerable<Matricula>> ObterTodos(CancellationToken cancellationToken = default)
    {
        try
        {
            string query = $"{BaseSelectQuery} ORDER BY m.data_inicio DESC";
            await using var command = await CreateCommandAsync(query, cancellationToken);
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            var matriculas = new List<Matricula>();
            while (await reader.ReadAsync(cancellationToken))
            {
                matriculas.Add(Map(reader));
            }
            return matriculas;
        }
        catch (DbException ex)
        {
            throw new InfrastructureException("ERRO_OBTER_TODOS", $"Erro ao obter todas as matrículas: {ex.Message}", ex);
        }
    }

    public async Task<Matricula> Adicionar(Matricula entity, CancellationToken cancellationToken = default)
    {
        try
        {
            string query = FormatInsertQuery("INSERT INTO tb_matricula (aluno_id, plano, data_inicio, data_fim, objetivo, restricao_medica, obs_restricao, laudo_medico) VALUES (@AlunoId, @Plano, @DataInicio, @DataFim, @Objetivo, @RestricaoMedica, @ObsRestricao, @LaudoMedico)");
            await using var command = await CreateCommandAsync(query, cancellationToken);
            command.AddParameter("@AlunoId", entity.AlunoId, DbType.Int32);
            command.AddParameter("@Plano", (int)entity.Plano, DbType.Int32);
            command.AddParameter("@DataInicio", entity.DataInicio, DbType.Date);
            command.AddParameter("@DataFim", entity.DataFim, DbType.Date);
            command.AddParameter("@Objetivo", entity.Objetivo, DbType.String);
            command.AddParameter("@RestricaoMedica", (int)entity.RestricoesMedicas, DbType.Int32);
            command.AddParameter("@ObsRestricao", (object?)entity.ObservacoesRestricoes ?? DBNull.Value, DbType.String);
            command.AddParameter("@LaudoMedico", (object?)entity.LaudoMedico?.Conteudo ?? DBNull.Value, DbType.Binary);
            int id = await command.ExecuteScalarIdAsync("ERRO_ADICIONAR_MATRICULA", "Falha ao obter ID inserido para a matrícula.", cancellationToken); var idProperty = typeof(Entity).GetProperty("Id");
            idProperty?.SetValue(entity, id);
            return entity;
        }
        catch (DbException ex)
        {
            throw new InfrastructureException("ERRO_ADICIONAR_MATRICULA", $"Erro ao adicionar matrícula para o aluno ID {entity.AlunoId}: {ex.Message}", ex);
        }
    }

    public async Task<Matricula> Atualizar(Matricula entity, CancellationToken cancellationToken = default)
    {
        try
        {
            string query = "UPDATE tb_matricula SET aluno_id = @AlunoId, plano = @Plano, data_inicio = @DataInicio, data_fim = @DataFim, objetivo = @Objetivo, restricao_medica = @RestricaoMedica, obs_restricao = @ObsRestricao, laudo_medico = @LaudoMedico WHERE id_matricula = @Id";
            await using var command = await CreateCommandAsync(query, cancellationToken);
            command.AddParameter("@Id", entity.Id, DbType.Int32);
            command.AddParameter("@AlunoId", entity.AlunoId, DbType.Int32);
            command.AddParameter("@Plano", (int)entity.Plano, DbType.Int32);
            command.AddParameter("@DataInicio", entity.DataInicio, DbType.Date);
            command.AddParameter("@DataFim", entity.DataFim, DbType.Date);
            command.AddParameter("@Objetivo", entity.Objetivo, DbType.String);
            command.AddParameter("@RestricaoMedica", (int)entity.RestricoesMedicas, DbType.Int32);
            command.AddParameter("@ObsRestricao", (object?)entity.ObservacoesRestricoes ?? DBNull.Value, DbType.String);
            command.AddParameter("@LaudoMedico", (object?)entity.LaudoMedico?.Conteudo ?? DBNull.Value, DbType.Binary);
            int rowsAffected = await command.ExecuteNonQueryAsync(cancellationToken);
            if (rowsAffected == 0)
            {
                throw new InfrastructureException("REGISTRO_NAO_ENCONTRADO", $"Nenhuma matrícula encontrada com o ID {entity.Id} para atualização.");
            }
            return entity;
        }
        catch (DbException ex)
        {
            throw new InfrastructureException("ERRO_ATUALIZAR_MATRICULA", $"Erro ao atualizar matrícula ID {entity.Id} do aluno ID {entity.AlunoId}: {ex.Message}", ex);
        }
    }

    public async Task<bool> Remover(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            string query = "DELETE FROM tb_matricula WHERE id_matricula = @Id";
            await using var command = await CreateCommandAsync(query, cancellationToken);
            command.AddParameter("@Id", id, DbType.Int32);
            var result = await command.ExecuteNonQueryAsync(cancellationToken);
            return result > 0;
        }
        catch (DbException ex)
        {
            throw new InfrastructureException("ERRO_REMOVER_MATRICULA", $"Erro ao remover matrícula ID {id}: {ex.Message}", ex);
        }
    }
    public async Task<IEnumerable<Matricula>> ObterPorAluno(int alunoId, CancellationToken cancellationToken = default)
    {
        try
        {
            string query = $"{BaseSelectQuery} WHERE m.aluno_id = @AlunoId ORDER BY m.data_inicio DESC";
            await using var command = await CreateCommandAsync(query, cancellationToken);
            command.AddParameter("@AlunoId", alunoId, DbType.Int32);
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            var matriculas = new List<Matricula>();
            while (await reader.ReadAsync(cancellationToken))
            {
                matriculas.Add(Map(reader));
            }
            return matriculas;
        }
        catch (DbException ex)
        {
            throw new InfrastructureException("ERRO_OBTER_POR_ALUNO", $"Erro ao obter matrículas do aluno ID {alunoId}: {ex.Message}", ex);
        }
    }

    public async Task<Matricula?> ObterMatriculaAtivaPorAluno(int alunoId, CancellationToken cancellationToken = default)
    {
        try
        {
            string query = $"{BaseSelectQuery} WHERE m.aluno_id = @AlunoId AND m.data_fim >= {GetCurrentDateFunction()} ORDER BY m.data_fim DESC"; await using var command = await CreateCommandAsync(query, cancellationToken);
            command.AddParameter("@AlunoId", alunoId, DbType.Int32);
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            return await reader.ReadAsync(cancellationToken) ? Map(reader) : null;
        }
        catch (DbException ex)
        {
            throw new InfrastructureException("ERRO_OBTER_MATRICULA_ATIVA", $"Erro ao obter matrícula ativa do aluno ID {alunoId}: {ex.Message}", ex);
        }
    }
    public async Task<bool> PossuiMatriculaAtiva(int alunoId, CancellationToken cancellationToken = default)
    {
        var matricula = await ObterMatriculaAtivaPorAluno(alunoId, cancellationToken);
        return matricula != null;
    }

    public async Task<IEnumerable<Matricula>> ObterAtivas(int alunoId = 0, CancellationToken cancellationToken = default)
    {
        try
        {
            string query = $"{BaseSelectQuery} WHERE m.data_fim >= {GetCurrentDateFunction()} {(alunoId > 0 ? "AND m.aluno_id = @id" : "")} ORDER BY m.data_fim ASC"; await using var command = await CreateCommandAsync(query, cancellationToken);
            if (alunoId > 0)
            {
                command.AddParameter("@id", alunoId, DbType.Int32);
            }
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            var matriculas = new List<Matricula>();
            while (await reader.ReadAsync(cancellationToken))
            {
                matriculas.Add(Map(reader));
            }
            return matriculas;
        }
        catch (DbException ex)
        {
            throw new InfrastructureException("ERRO_OBTER_MATRICULAS_ATIVAS", $"Erro ao obter matrículas ativas: {ex.Message}", ex);
        }
    }
    public async Task<IEnumerable<Matricula>> ObterVencendoEmDias(int dias, CancellationToken cancellationToken = default)
    {
        try
        {
            string currentDate = GetCurrentDateFunction();
            string maxDate = GetDateAddDaysExpression(currentDate, "@Dias");
            string query = $"{BaseSelectQuery} WHERE m.data_fim >= {currentDate} AND m.data_fim <= {maxDate} ORDER BY m.data_fim ASC";
            await using var command = await CreateCommandAsync(query, cancellationToken);
            command.AddParameter("@Dias", dias, DbType.Int32);
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            var matriculas = new List<Matricula>();
            while (await reader.ReadAsync(cancellationToken))
            {
                matriculas.Add(Map(reader));
            }
            return matriculas;
        }
        catch (DbException ex)
        {
            throw new InfrastructureException("ERRO_OBTER_MATRICULAS_VENCENDO", $"Erro ao obter matrículas vencendo em {dias} dias: {ex.Message}", ex);
        }
    }

    public async Task<IEnumerable<Matricula>> ObterPorPlano(MatriculaPlano plano, CancellationToken cancellationToken = default)
    {
        try
        {
            string query = $"{BaseSelectQuery} WHERE m.plano = @Plano ORDER BY a.nome";
            await using var command = await CreateCommandAsync(query, cancellationToken);
            command.AddParameter("@Plano", (int)plano, DbType.Int32);
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            var matriculas = new List<Matricula>();
            while (await reader.ReadAsync(cancellationToken))
            {
                matriculas.Add(Map(reader));
            }
            return matriculas;
        }
        catch (DbException ex)
        {
            throw new InfrastructureException("ERRO_OBTER_POR_PLANO", $"Erro ao obter matrículas pelo plano {plano}: {ex.Message}", ex);
        }
    }
}