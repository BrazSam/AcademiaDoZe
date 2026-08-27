using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.Enums;
using AcademiaDoZe.Domain.Repositories;
using AcademiaDoZe.Domain.ValueObjects;
using AcademiaDoZe.Infrastructure.Data;
using AcademiaDoZe.Infrastructure.Exceptions;
using System.Data;
using System.Data.Common;
namespace AcademiaDoZe.Infrastructure.Repositories;

public class ColaboradorRepository : BaseRepository, IColaboradorRepository
{
    public ColaboradorRepository(string connectionString, DatabaseType databaseType) : base(connectionString, databaseType)
    {
    }
    private static string BaseSelectQuery => @"
SELECT
c.id_colaborador, c.cpf, c.nome, c.nascimento, c.telefone, c.email,
c.logradouro_id, c.numero, c.complemento, c.senha, c.foto, c.admissao, c.tipo, c.vinculo,
l.id_logradouro, l.cep, l.nome AS logradouro_nome, l.bairro, l.cidade, l.estado, l.pais
FROM tb_colaborador c
INNER JOIN tb_logradouro l ON c.logradouro_id = l.id_logradouro";
    // métodos aqui

    public async Task<Colaborador?> ObterPorId(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            string query = $"{BaseSelectQuery} WHERE c.id_colaborador = @Id";
            await using var command = await CreateCommandAsync(query, cancellationToken);
            command.AddParameter("@Id", id, DbType.Int32);
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            return await reader.ReadAsync(cancellationToken) ? Map(reader) : null;
        }
        catch (DbException ex)
        {
            throw new InfrastructureException("ERRO_OBTER_POR_ID", $"Erro ao obter colaborador por ID {id}: {ex.Message}", ex);
        }
    }
    public async Task<IEnumerable<Colaborador>> ObterTodos(CancellationToken cancellationToken = default)
    {
        try
        {
            string query = $"{BaseSelectQuery} ORDER BY c.nome";
            await using var command = await CreateCommandAsync(query, cancellationToken);
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            var colaboradores = new List<Colaborador>();
            while (await reader.ReadAsync(cancellationToken))
            {
                colaboradores.Add(Map(reader));
            }
            return colaboradores;
        }
        catch (DbException ex)
        {
            throw new InfrastructureException("ERRO_OBTER_TODOS", $"Erro ao obter todos os colaboradores: {ex.Message}", ex);
        }
    }

    public static Colaborador Map(DbDataReader reader, string nomeColumn = "nome")
    {
        try
        {
            int id = reader.GetInt32Value("id_colaborador");
            string cpf = reader.GetStringValue("cpf");
            string nome = reader.GetStringValue(nomeColumn);
            DateOnly dataNascimento = reader.GetDateOnlyValue("nascimento");
            string telefone = reader.GetStringValue("telefone");
            string email = reader.GetStringValue("email");
            string numero = reader.GetStringValue("numero");
            string complemento = reader.GetNullableString("complemento");
            string senha = reader.GetStringValue("senha");
            DateOnly dataAdmissao = reader.GetDateOnlyValue("admissao");
            ColaboradorTipo tipo = (ColaboradorTipo)reader.GetInt32Value("tipo");
            ColaboradorVinculo vinculo = (ColaboradorVinculo)reader.GetInt32Value("vinculo");
            byte[]? fotoBytes = reader.GetNullableBytes("foto");
            Arquivo? foto = fotoBytes != null ? Arquivo.Criar(fotoBytes).Value : null;
            var logradouro = LogradouroRepository.Map(reader, "logradouro_nome");
            var result = Colaborador.Criar(
            id: id,
            nome: nome,
            cpf: cpf,
            dataNascimento: dataNascimento,
            telefone: telefone,
            email: email,
            endereco: logradouro,
            numero: numero,
            complemento: complemento,
            senha: senha,
            foto: foto!,
            dataAdmissao: dataAdmissao,
            tipo: tipo,
            vinculo: vinculo
            );
            if (result.IsFailure)
            {
                throw new InfrastructureException("ERRO_DOMINIO_MAPEAMENTO", $"Erro de domínio ao mapear colaborador ID {id}: {string.Join(", ", result.Notifications.Select(n => n.Mensagem))}");
            }
            return result.Value!;
        }
        catch (Exception ex) when (ex is not InfrastructureException)
        {
            throw new InfrastructureException("ERRO_MAPEAMENTO_COLABORADOR", $"Erro ao mapear dados do colaborador: {ex.Message}", ex);
        }
    }

    public async Task<Colaborador> Adicionar(Colaborador entity, CancellationToken cancellationToken = default)
    {
        try
        {
            string query = FormatInsertQuery("INSERT INTO tb_colaborador (cpf, nome, nascimento, telefone, email, logradouro_id, numero, complemento, senha, foto, admissao, tipo, vinculo) VALUES (@Cpf, @Nome, @Nascimento, @Telefone, @Email, @LogradouroId, @Numero, @Complemento, @Senha, @Foto, @Admissao, @Tipo, @Vinculo)");
            await using var command = await CreateCommandAsync(query, cancellationToken);
            command.AddParameter("@Cpf", entity.Cpf.Valor, DbType.String);
            command.AddParameter("@Nome", entity.Nome, DbType.String);
            command.AddParameter("@Nascimento", entity.DataNascimento, DbType.Date);
            command.AddParameter("@Telefone", entity.Telefone.Valor, DbType.String);
            command.AddParameter("@Email", entity.Email.Valor, DbType.String);
            command.AddParameter("@LogradouroId", entity.Endereco.LogradouroId, DbType.Int32);
            command.AddParameter("@Numero", entity.Endereco.Numero, DbType.String);
            command.AddParameter("@Complemento", (object?)entity.Endereco.Complemento ?? DBNull.Value, DbType.String);
            command.AddParameter("@Senha", entity.Senha.Valor, DbType.String);
            command.AddParameter("@Foto", (object?)entity.Foto?.Conteudo ?? DBNull.Value, DbType.Binary);
            command.AddParameter("@Admissao", entity.DataAdmissao, DbType.Date);
            command.AddParameter("@Tipo", (int)entity.Tipo, DbType.Int32);
            command.AddParameter("@Vinculo", (int)entity.Vinculo, DbType.Int32);
            int id = await command.ExecuteScalarIdAsync("ERRO_ADICIONAR_COLABORADOR", "Falha ao obter ID inserido para o colaborador.", cancellationToken); var idProperty = typeof(Entity).GetProperty("Id");
            idProperty?.SetValue(entity, id);
            return entity;
        }
        catch (DbException ex)
        {
            throw new InfrastructureException("ERRO_ADICIONAR_COLABORADOR", $"Erro ao adicionar colaborador: {ex.Message}", ex);
        }
    }

    public async Task<Colaborador> Atualizar(Colaborador entity, CancellationToken cancellationToken = default)
    {
        try
        {
            string query = "UPDATE tb_colaborador SET cpf = @Cpf, nome = @Nome, nascimento = @Nascimento, telefone = @Telefone, email = @Email, logradouro_id = @LogradouroId, numero = @Numero, complemento = @Complemento, senha = @Senha, foto = @Foto, admissao = @Admissao, tipo = @Tipo, vinculo = @Vinculo WHERE id_colaborador = @Id";
            await using var command = await CreateCommandAsync(query, cancellationToken);
            command.AddParameter("@Id", entity.Id, DbType.Int32);
            command.AddParameter("@Cpf", entity.Cpf.Valor, DbType.String);
            command.AddParameter("@Nome", entity.Nome, DbType.String);
            command.AddParameter("@Nascimento", entity.DataNascimento, DbType.Date);
            command.AddParameter("@Telefone", entity.Telefone.Valor, DbType.String);
            command.AddParameter("@Email", entity.Email.Valor, DbType.String);
            command.AddParameter("@LogradouroId", entity.Endereco.LogradouroId, DbType.Int32);
            command.AddParameter("@Numero", entity.Endereco.Numero, DbType.String);
            command.AddParameter("@Complemento", (object?)entity.Endereco.Complemento ?? DBNull.Value, DbType.String);
            command.AddParameter("@Senha", entity.Senha.Valor, DbType.String);
            command.AddParameter("@Foto", (object?)entity.Foto?.Conteudo ?? DBNull.Value, DbType.Binary);
            command.AddParameter("@Admissao", entity.DataAdmissao, DbType.Date);
            command.AddParameter("@Tipo", (int)entity.Tipo, DbType.Int32);
            command.AddParameter("@Vinculo", (int)entity.Vinculo, DbType.Int32);
            int rowsAffected = await command.ExecuteNonQueryAsync(cancellationToken);
            if (rowsAffected == 0)
            {
                throw new InfrastructureException("REGISTRO_NAO_ENCONTRADO", $"Nenhum colaborador encontrado com ID {entity.Id} para atualização.");
            }
            return entity;
        }
        catch (DbException ex)
        {
            throw new InfrastructureException("ERRO_ATUALIZAR_COLABORADOR", $"Erro ao atualizar colaborador ID {entity.Id}: {ex.Message}", ex);
        }
    }

    public async Task<bool> Remover(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            string query = "DELETE FROM tb_colaborador WHERE id_colaborador = @Id";
            await using var command = await CreateCommandAsync(query, cancellationToken);
            command.AddParameter("@Id", id, DbType.Int32);
            var result = await command.ExecuteNonQueryAsync(cancellationToken);
            return result > 0;
        }
        catch (DbException ex)
        {
            throw new InfrastructureException("ERRO_REMOVER_COLABORADOR", $"Erro ao remover colaborador ID {id}: {ex.Message}", ex);
        }
    }
    public async Task<Colaborador?> ObterPorCpf(Cpf cpf, CancellationToken cancellationToken = default)
    {
        try
        {
            string query = $"{BaseSelectQuery} WHERE c.cpf = @Cpf";
            await using var command = await CreateCommandAsync(query, cancellationToken);
            command.AddParameter("@Cpf", cpf.Valor, DbType.String);
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            return await reader.ReadAsync(cancellationToken) ? Map(reader) : null;
        }
        catch (DbException ex)
        {
            throw new InfrastructureException("ERRO_OBTER_POR_CPF", $"Erro ao obter colaborador por CPF {cpf.Valor}: {ex.Message}", ex);
        }
    }

    public async Task<Colaborador?> ObterPorEmail(Email email, CancellationToken cancellationToken = default)
    {
        try
        {
            string query = $"{BaseSelectQuery} WHERE c.email = @Email";
            await using var command = await CreateCommandAsync(query, cancellationToken);
            command.AddParameter("@Email", email.Valor, DbType.String);
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            return await reader.ReadAsync(cancellationToken) ? Map(reader) : null;
        }
        catch (DbException ex)
        {
            throw new InfrastructureException("ERRO_OBTER_POR_EMAIL", $"Erro ao obter colaborador por Email {email.Valor}: {ex.Message}", ex);
        }
    }
    public async Task<bool> CpfJaExiste(Cpf cpf, int? id = null, CancellationToken cancellationToken = default)
    {
        try
        {
            string query = "SELECT COUNT(1) FROM tb_colaborador WHERE cpf = @Cpf AND (@Id IS NULL OR id_colaborador <> @Id)";
            await using var command = await CreateCommandAsync(query, cancellationToken);
            command.AddParameter("@Cpf", cpf.Valor, DbType.String);
            command.AddParameter("@Id", (object?)id ?? DBNull.Value, DbType.Int32);
            var count = Convert.ToInt32(await command.ExecuteScalarAsync(cancellationToken));
            return count > 0;
        }
        catch (DbException ex)
        {
            throw new InfrastructureException("ERRO_VERIFICAR_CPF", $"Erro ao verificar existência de CPF: {ex.Message}", ex);
        }
    }

    public async Task<bool> EmailJaExiste(Email email, int? id = null, CancellationToken cancellationToken = default)
    {
        try
        {
            string query = "SELECT COUNT(1) FROM tb_colaborador WHERE email = @Email AND (@Id IS NULL OR id_colaborador <> @Id)";
            await using var command = await CreateCommandAsync(query, cancellationToken);
            command.AddParameter("@Email", email.Valor, DbType.String);
            command.AddParameter("@Id", (object?)id ?? DBNull.Value, DbType.Int32);
            var count = Convert.ToInt32(await command.ExecuteScalarAsync(cancellationToken));
            return count > 0;
        }
        catch (DbException ex)
        {
            throw new InfrastructureException("ERRO_VERIFICAR_EMAIL", $"Erro ao verificar existência de Email: {ex.Message}", ex);
        }
    }
    public async Task<IEnumerable<Colaborador>> ObterPorTipo(ColaboradorTipo tipo, CancellationToken cancellationToken = default)
    {
        try
        {
            string query = $"{BaseSelectQuery} WHERE c.tipo = @Tipo ORDER BY c.nome";
            await using var command = await CreateCommandAsync(query, cancellationToken);
            command.AddParameter("@Tipo", (int)tipo, DbType.Int32);
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            var colaboradores = new List<Colaborador>();
            while (await reader.ReadAsync(cancellationToken))
            {
                colaboradores.Add(Map(reader));
            }
            return colaboradores;
        }
        catch (DbException ex)
        {
            throw new InfrastructureException("ERRO_OBTER_POR_TIPO", $"Erro ao obter colaboradores por tipo {tipo}: {ex.Message}", ex);
        }
    }

    public async Task<IEnumerable<Colaborador>> ObterPorVinculo(ColaboradorVinculo vinculo, CancellationToken cancellationToken = default)
    {
        try
        {
            string query = $"{BaseSelectQuery} WHERE c.vinculo = @Vinculo ORDER BY c.nome";
            await using var command = await CreateCommandAsync(query, cancellationToken);
            command.AddParameter("@Vinculo", (int)vinculo, DbType.Int32);
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            var colaboradores = new List<Colaborador>();
            while (await reader.ReadAsync(cancellationToken))
            {
                colaboradores.Add(Map(reader));
            }
            return colaboradores;
        }
        catch (DbException ex)
        {
            throw new InfrastructureException("ERRO_OBTER_POR_VINCULO", $"Erro ao obter colaboradores por vínculo {vinculo}: {ex.Message}", ex);
        }
    }
    public async Task<bool> TrocarSenha(int id, Senha novaSenha, CancellationToken cancellationToken = default)
    {
        try
        {
            string query = "UPDATE tb_colaborador SET senha = @Senha WHERE id_colaborador = @Id";
            await using var command = await CreateCommandAsync(query, cancellationToken);
            command.AddParameter("@Id", id, DbType.Int32);
            command.AddParameter("@Senha", novaSenha.Valor, DbType.String);
            int rowsAffected = await command.ExecuteNonQueryAsync(cancellationToken);
            return rowsAffected > 0;
        }
        catch (DbException ex)
        {
            throw new InfrastructureException("ERRO_TROCAR_SENHA", $"Erro ao trocar senha do colaborador ID {id}: {ex.Message}", ex);
        }
    }
}