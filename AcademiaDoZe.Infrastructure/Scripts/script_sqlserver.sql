-- ============================================================
-- SCRIPT DE CRIAÇÃO DO BANCO DE DADOS: db_academia_do_ze (SQL Server)
-- Baseado na camada de domínio AcademiaDoZe.Domain
-- ============================================================
IF OBJECT_ID(N'dbo.tb_logradouro', N'U') IS NULL
BEGIN
 CREATE TABLE tb_logradouro (
 id_logradouro INT IDENTITY(1,1) NOT NULL,
 cep VARCHAR(8) NOT NULL,
 nome VARCHAR(150) NOT NULL,
 bairro VARCHAR(100) NOT NULL,
 cidade VARCHAR(100) NOT NULL,
 estado CHAR(2) NOT NULL,
 pais VARCHAR(50) NOT NULL CONSTRAINT DF_tb_logradouro_pais DEFAULT 'Brasil',
 CONSTRAINT PK_tb_logradouro PRIMARY KEY (id_logradouro),
 CONSTRAINT UQ_tb_logradouro_cep UNIQUE (cep)
 );
 CREATE INDEX IX_tb_logradouro_cep ON tb_logradouro(cep);
 CREATE INDEX IX_tb_logradouro_cidade ON tb_logradouro(cidade);
END;
IF OBJECT_ID(N'dbo.tb_aluno', N'U') IS NULL
BEGIN
 CREATE TABLE tb_aluno (
 id_aluno INT IDENTITY(1,1) NOT NULL,
 cpf VARCHAR(11) NOT NULL,
 nome VARCHAR(150) NOT NULL,
 nascimento DATE NOT NULL,
 telefone VARCHAR(15) NOT NULL,
 email VARCHAR(150) NOT NULL,
 logradouro_id INT NOT NULL,
 numero VARCHAR(20) NOT NULL,
 complemento VARCHAR(100) NULL,
 senha VARCHAR(255) NOT NULL,
 foto VARBINARY(MAX) NULL,
 CONSTRAINT PK_tb_aluno PRIMARY KEY (id_aluno),
 CONSTRAINT UQ_tb_aluno_cpf UNIQUE (cpf),
 CONSTRAINT FK_tb_aluno_tb_logradouro FOREIGN KEY (logradouro_id)
 REFERENCES tb_logradouro(id_logradouro)
 );
 CREATE INDEX IX_tb_aluno_cpf ON tb_aluno(cpf);
END;
IF OBJECT_ID(N'dbo.tb_colaborador', N'U') IS NULL
BEGIN
 CREATE TABLE tb_colaborador (
 id_colaborador INT IDENTITY(1,1) NOT NULL,
 cpf VARCHAR(11) NOT NULL,
 nome VARCHAR(150) NOT NULL,
 nascimento DATE NOT NULL,
 telefone VARCHAR(15) NOT NULL,
 email VARCHAR(150) NOT NULL,
 logradouro_id INT NOT NULL,
 numero VARCHAR(20) NOT NULL,
 complemento VARCHAR(100) NULL,
 senha VARCHAR(255) NOT NULL,
 foto VARBINARY(MAX) NULL,
 admissao DATE NOT NULL,
 tipo INT NOT NULL, -- Enum ColaboradorTipo (0=Administrador, 1=Atendente, 2=Instrutor)
 vinculo INT NOT NULL, -- Enum ColaboradorVinculo (0=CLT, 1=Estágio)
 CONSTRAINT PK_tb_colaborador PRIMARY KEY (id_colaborador),
 CONSTRAINT UQ_tb_colaborador_cpf UNIQUE (cpf),
 CONSTRAINT FK_tb_colaborador_tb_logradouro FOREIGN KEY (logradouro_id)
 REFERENCES tb_logradouro(id_logradouro)
 );
 CREATE INDEX IX_tb_colaborador_cpf ON tb_colaborador(cpf);
END;
IF OBJECT_ID(N'dbo.tb_matricula', N'U') IS NULL
BEGIN
 CREATE TABLE tb_matricula (
 id_matricula INT IDENTITY(1,1) NOT NULL,
 aluno_id INT NOT NULL,
 plano INT NOT NULL, -- Enum MatriculaPlano (0=Mensal, 1=Trimestral, 2=Semestral, 3=Anual)
 data_inicio DATE NOT NULL,
 data_fim DATE NOT NULL,
 objetivo VARCHAR(500) NOT NULL,
 restricao_medica INT NOT NULL CONSTRAINT DF_tb_matricula_restricao DEFAULT 0, -- Enum [Flags] MatriculaRestricoes
 obs_restricao VARCHAR(500) NULL,
 laudo_medico VARBINARY(MAX) NULL,
 CONSTRAINT PK_tb_matricula PRIMARY KEY (id_matricula),
 CONSTRAINT FK_tb_matricula_tb_aluno FOREIGN KEY (aluno_id)
 REFERENCES tb_aluno(id_aluno) ON DELETE CASCADE
 );
 CREATE INDEX IX_tb_matricula_aluno_id ON tb_matricula(aluno_id);
 CREATE INDEX IX_tb_matricula_data_fim ON tb_matricula(data_fim);
END;
IF OBJECT_ID(N'dbo.tb_acesso', N'U') IS NULL
BEGIN
 CREATE TABLE tb_acesso (
 id_acesso INT IDENTITY(1,1) NOT NULL,
 pessoa_tipo INT NOT NULL, -- 0 = Aluno (AcessoAluno), 1 = Colaborador (AcessoColaborador)
 pessoa_id INT NOT NULL,
 data_hora DATETIME NOT NULL CONSTRAINT DF_tb_acesso_data_hora DEFAULT GETDATE(),
 CONSTRAINT PK_tb_acesso PRIMARY KEY (id_acesso)
 );
 CREATE INDEX IX_tb_acesso_pessoa ON tb_acesso(pessoa_tipo, pessoa_id);
 CREATE INDEX IX_tb_acesso_data_hora ON tb_acesso(data_hora);
END; 