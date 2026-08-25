-- ============================================================
-- SCRIPT DE CRIAÇÃO DO BANCO DE DADOS: db_academia_do_ze (SQLite)
-- Baseado na camada de domínio AcademiaDoZe.Domain
-- ============================================================
-- Habilitar suporte a Chaves Estrangeiras no SQLite
PRAGMA foreign_keys = ON;
CREATE TABLE IF NOT EXISTS tb_logradouro (
 id_logradouro INTEGER PRIMARY KEY AUTOINCREMENT,
 cep TEXT NOT NULL UNIQUE,
 nome TEXT NOT NULL,
 bairro TEXT NOT NULL,
 cidade TEXT NOT NULL,
 estado TEXT NOT NULL,
 pais TEXT NOT NULL DEFAULT 'Brasil'
);
CREATE INDEX IF NOT EXISTS ix_tb_logradouro_cep ON tb_logradouro(cep);
CREATE INDEX IF NOT EXISTS ix_tb_logradouro_cidade ON tb_logradouro(cidade);
CREATE TABLE IF NOT EXISTS tb_aluno (
 id_aluno INTEGER PRIMARY KEY AUTOINCREMENT,
 cpf TEXT NOT NULL UNIQUE,
 nome TEXT NOT NULL,
 nascimento TEXT NOT NULL, -- Data em formato ISO-8601 (YYYY-MM-DD)
 telefone TEXT NOT NULL,
 email TEXT NOT NULL,
 logradouro_id INTEGER NOT NULL,
 numero TEXT NOT NULL,
 complemento TEXT NULL,
 senha TEXT NOT NULL,
 foto BLOB NULL,
 FOREIGN KEY (logradouro_id) REFERENCES tb_logradouro(id_logradouro) ON DELETE RESTRICT ON UPDATE CASCADE
);
CREATE INDEX IF NOT EXISTS ix_tb_aluno_cpf ON tb_aluno(cpf);
CREATE TABLE IF NOT EXISTS tb_colaborador (
 id_colaborador INTEGER PRIMARY KEY AUTOINCREMENT,
 cpf TEXT NOT NULL UNIQUE,
 nome TEXT NOT NULL,
 nascimento TEXT NOT NULL, -- Data em formato ISO-8601 (YYYY-MM-DD)
 telefone TEXT NOT NULL,
 email TEXT NOT NULL,
 logradouro_id INTEGER NOT NULL,
 numero TEXT NOT NULL,
 complemento TEXT NULL,
 senha TEXT NOT NULL,
 foto BLOB NULL,
 admissao TEXT NOT NULL, -- Data em formato ISO-8601 (YYYY-MM-DD)
 tipo INTEGER NOT NULL, -- Enum ColaboradorTipo (0=Administrador, 1=Atendente, 2=Instrutor)
 vinculo INTEGER NOT NULL, -- Enum ColaboradorVinculo (0=CLT, 1=Estágio)
 FOREIGN KEY (logradouro_id) REFERENCES tb_logradouro(id_logradouro) ON DELETE RESTRICT ON UPDATE CASCADE
);
CREATE INDEX IF NOT EXISTS ix_tb_colaborador_cpf ON tb_colaborador(cpf);
CREATE TABLE IF NOT EXISTS tb_matricula (
 id_matricula INTEGER PRIMARY KEY AUTOINCREMENT,
 aluno_id INTEGER NOT NULL,
 plano INTEGER NOT NULL, -- Enum MatriculaPlano (0=Mensal, 1=Trimestral, 2=Semestral, 3=Anual)
 data_inicio TEXT NOT NULL, -- Data em formato ISO-8601 (YYYY-MM-DD)
 data_fim TEXT NOT NULL, -- Data em formato ISO-8601 (YYYY-MM-DD)
 objetivo TEXT NOT NULL,
 restricao_medica INTEGER NOT NULL DEFAULT 0, -- Enum [Flags] MatriculaRestricoes (Bitmask)
 obs_restricao TEXT NULL,
 laudo_medico BLOB NULL,
 FOREIGN KEY (aluno_id) REFERENCES tb_aluno(id_aluno) ON DELETE CASCADE ON UPDATE CASCADE
);
CREATE INDEX IF NOT EXISTS ix_tb_matricula_aluno_id ON tb_matricula(aluno_id);
CREATE INDEX IF NOT EXISTS ix_tb_matricula_data_fim ON tb_matricula(data_fim);
CREATE TABLE IF NOT EXISTS tb_acesso (
 id_acesso INTEGER PRIMARY KEY AUTOINCREMENT,
 pessoa_tipo INTEGER NOT NULL, -- 0 = Aluno (AcessoAluno), 1 = Colaborador (AcessoColaborador)
 pessoa_id INTEGER NOT NULL,
 data_hora TEXT NOT NULL DEFAULT (CURRENT_TIMESTAMP) -- Data/Hora em formato ISO-8601 (YYYY-MM-DD HH:MM:SS)
);
CREATE INDEX IF NOT EXISTS ix_tb_acesso_pessoa ON tb_acesso(pessoa_tipo, pessoa_id);
CREATE INDEX IF NOT EXISTS ix_tb_acesso_data_hora ON tb_acesso(data_hora); 