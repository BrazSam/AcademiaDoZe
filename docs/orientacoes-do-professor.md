# Academia do Zé — orientações da disciplina

Análise em 22/10/2026 da turma T2 - Desenvolvimento de Sistemas 2 - 2026/02, professor Luciano Coelho.

## Limite informado pelo aluno

Samuel informou expressamente que implementou SOMENTE ATÉ A ATIVIDADE 12. Não considerar as atividades posteriores como entregas já feitas nem como faltas dessa etapa. Nenhum código foi alterado nesta análise. A comparação com o projeto é contextual, não uma nova auditoria funcional completa.

## Fontes e método

Foram localizados 17 PDFs na turma. A leitura foi feita no leitor do Google Drive autenticado, navegando pelas páginas e consultando o texto renderizado, enunciados e exemplos. Os PDFs originais não foram baixados com sucesso. Foram percorridas as páginas dos materiais e conferidos os requisitos; isso não representa teste de execução dos exemplos nem auditoria linha a linha de todo código dos slides. A página visual da aula 13 foi inspecionada: cartaz da XXIX Mostra Científica 2026.

Turma: https://classroom.google.com/c/Nzk4NDUwMzIwMzk4

| Material | Páginas | Fonte |
|---|---:|---|
| 01 | 63 | https://drive.google.com/file/d/1YTjgWgOj0LHzK7JdtdALCAEEFYTLyGPj/view |
| 02 | 38 | https://drive.google.com/file/d/1rxLGPw-Z1AxR8hyXDiW4FwgAzpJDlNms/view |
| 03 | 35 | https://drive.google.com/file/d/1Y3BRBGZDWctTwGCk0cJSuV31b5FGU8TQ/view |
| 04 | 70 | https://drive.google.com/file/d/1zGcovnQaTKeXoLdt989UDEX5-pMl8hFD/view |
| 05 | 81 | https://drive.google.com/file/d/1fshlAQnrick1jPOjk8JCJqsCZTUB2FKt/view |
| 06 | 32 | https://drive.google.com/file/d/1X1noLTakb4XyiHkkxPhYdPdqgtMvypQc/view |
| 07 | 29 | https://drive.google.com/file/d/1k3Kj_wSkwyG4Yym4rb7YE5ZE7CWBvUxO/view |
| 08 | 73 | https://drive.google.com/file/d/1xu22Nth0A5Zu7OqJdX2k5ovcb_gNWiWj/view |
| 09 | 161 | https://drive.google.com/file/d/1H32vyGOXOQOZD81StW8gceFWFp0ug0YA/view |
| 10 | 63 | https://drive.google.com/file/d/1fHcxn5Xnw0KErPJUqokzsbwYKnqIu9t2/view |
| 11 | 65 | https://drive.google.com/file/d/1vNwleDp6TP4UO9qa55hu6_o-7VVwtOqi/view |
| 12 | 32 | https://drive.google.com/file/d/1NioyO1cDXXbvR_KRTcmDvgbG_wUJ1PJi/view |
| 13 — Mostra | 2 | https://drive.google.com/file/d/1ycSRE85lhNc_YWXYwtf9SQSJTfDEPhJc/view |
| 14 e 15 | 38 | https://drive.google.com/file/d/1R_ugFphA2whjY01C91flPjXCTpViBHsy/view |
| 16 — internacionalização | 56 | https://drive.google.com/file/d/16X-hqgqL8NPuVSLzWR6eS-43SjFxRxT-/view |
| Avaliação 03 | 4 | https://drive.google.com/file/d/128llRTBzERun8viHJqVzQN2IgWFUS9Ph/view |
| Cronograma final | 1 | https://drive.google.com/file/d/1F1SwVEmNuu2u2TjXXno-xTVrQ3JRLcrC/view |

## Sequência até a atividade 12

1. Fundamentos: requisitos, SOLID, camadas, domínio anêmico versus rico, DDD, Clean Architecture. Domínio independente da interface, framework e banco. C#/.NET 10 e MAUI para Windows, macOS, Android e iOS.
2. Criar a solução e Domain: somente propriedades e construtores inicialmente. Enums ColaboradorTipo, ColaboradorVinculo, MatriculaPlano, MatriculaRestricoes; VOs Arquivo, Cep, Cpf, Email, Endereco, Senha, Telefone; entidades Entity, Logradouro, Pessoa, Aluno, Colaborador, Matricula, AcessoAluno e AcessoColaborador. Herança, encapsulamento, records, abstract e Flags.
3. Consolidar Domain: construtores privados, fábricas Criar, normalização, Result<T>, Notification, DomainException quando justificada, invariantes e objetos válidos. Regras dependentes de dados persistidos ficam para serviços/camadas superiores.
4. Agregados e IAggregateRoot; referências entre agregados por ID. Matrícula recebe Aluno na fábrica para validar idade, mas armazena AlunoId. Interfaces de repositórios no Domain, restrição Entity/IAggregateRoot, Task e CancellationToken. Testes xUnit completos; entrega pede ao menos 120 testes com sucesso. Alterações futuras de comportamento são exemplificadas, mas várias são simplificadas pelo escopo didático.
5. Infrastructure para SQLite, MySQL e SQL Server. SQL puro/ADO.NET escolhido intencionalmente para aprendizado; não substituir automaticamente por ORM. Scripts por SGBD como EmbeddedResource, DbProvider, DbInitializer, DataReaderExtensions, BaseRepository e LogradouroRepository. Database servidor previamente criado; tabelas inicializadas pelo aplicativo; SQLite local. Testar Logradouro nos três bancos e entregar seis prints personalizados.
6. Repositórios concretos e testes de Aluno e Colaborador, todos os contratos funcionando nos três bancos. Dados personalizados nos testes; entrega pede 12 prints.
7. Repositório de Matrícula e testes: consultas por aluno/plano, ativas e vencendo, persistência de múltiplas restrições e laudo. Entrega pede dois prints e solução baixada do Git. A camada de apresentação de Matrícula ainda não é exigida.
8. Application de Logradouro, Colaborador, Aluno e Matrícula: DTOs, enums próprios com Display, interfaces, mappings, services, DI e PasswordHasher Argon2id. Services orquestram e validam unicidade e outras condições que dependem de persistência. DTO de saída não expõe senha. Projeto Application referencia Domain e Infrastructure no exemplo; não julgar esse arranjo como desvio unilateral do aluno. Prévia de que AcessoAluno e AcessoColaborador serão Avaliação 03.
9. Presentation MAUI: XAML e MVVM, BaseViewModel/ObservableObject, RelayCommand, binding, Shell/rotas, DI, Dashboard e CRUD completo de Logradouro. UI usa apenas Application, sem instanciar entidades nem repositórios. Sem pasta Models duplicada na UI. Styles e identidade visual própria. Dados de conexão fixos aceitos temporariamente no exemplo. Logo no Dashboard contendo nome do aluno e Git próprio são requisitos explícitos. Entrega com link, release ZIP, estrutura de arquivos e vídeo de CRUD.
10. Configurações de tema e banco: Preferences, WeakReferenceMessenger, atualização durante execução, Light/Dark/System; credenciais específicas por SGBD. Ícones nos controles/menu, fonte Material Icons, ícone e título do app personalizados. O exemplo usa code-behind para essa tela e credenciais padrão de desenvolvimento.
11. CRUD completo de Colaborador e Aluno. Responsividade com OnPlatform/OnIdiom, converters de enum/DateOnly/foto, câmera ou galeria e permissões por plataforma; senha em branco preservada na edição e confirmação de senha. Vídeo deve mostrar criação, filtro por CPF, edição de nome/foto e exclusão dos dois cadastros. Identidade visual própria, Git e logo com nome reiterados.
12. Aula livre para colocar projeto em dia, material complementar NÃO AVALIATIVO. Eventos e melhorias de UX: aplicar tema ao selecionar; prevenir eventos na inicialização; busca com debounce de 300 ms e CPF parcial usando LIKE/retorno de coleção; escolher registro quando há vários resultados; mostrar/ocultar senha; tocar foto para selecionar; validação de e-mail em Unfocused via classe parcial. Não transformar essas melhorias opcionais em novos requisitos avaliativos obrigatórios.

## Requisitos de negócio gerais

- Aluno: nome, CPF único, nascimento, telefone, senha, endereço/número obrigatórios; e-mail e foto aparecem opcionais no levantamento inicial. Os exemplos de VO Email acabam exigindo formato válido, portanto existe uma diferença entre levantamento e exemplo a esclarecer caso relevante.
- Colaborador: dados pessoais, e-mail, admissão, tipo Administrador/Atendente/Instrutor e vínculo CLT/Estágio. Exemplos contêm validações adicionais e algumas inconsistências; avaliar o caso concreto antes de mudar regras.
- Matrícula: aluno, plano mensal/trimestral/semestral/anual, início, término calculado pelo plano, objetivo, Flags de restrições, observações e laudo. Sem controle financeiro. Não permitir nova matrícula com matrícula ativa. Texto inicial diz 12 a 16 anos; exemplos implementam menor de 16. Restrições médicas exigem parecer/laudo armazenado.
- Entrada/saída (etapa futura): aluno deve ter matrícula ativa; mostrar restante do plano na entrada e permanência na saída. Colaborador: limite diário de 8h CLT/6h estágio e soma dos períodos do dia.
- Há visão geral de perfis/permissões e relatórios estatísticos. Isso não prova que todas essas funções já eram entregáveis até a 12.

## Etapas posteriores: não cobrar como já implementadas

**Atualização de escopo em 22/10/2026:** a inspeção visual do PDF local `../devAval03.pdf`, enviado por Samuel, revelou regras riscadas na página 3: tempo restante do plano, permanência e limite/soma de horas de colaborador. Essas regras constam do levantamento geral e da leitura textual anterior abaixo, mas não devem ser cobradas como obrigatórias nesta versão da Avaliação 03. O detalhe atualizado está em `contexto-do-projeto.md`.

- 13: Mostra Científica, sem novo desenvolvimento descrito no PDF.
- 14/15: interface completa de Matrícula. Selecionar aluno, cálculo de término, múltiplas restrições, laudo, CRUD e vídeo. Entrega única indicada: 08/11. Sua ausência no projeto atual é compatível com o limite até 12.
- 16: internacionalização pt-BR/en-US/es-ES com .resx, LocalizationManager, TranslateExtension, Preferences e Messenger, troca em runtime e formatação cultural. Cronograma classifica como conteúdo auxiliar não avaliativo.
- Avaliação 03: implementar entrada/saída de Alunos e Colaboradores em todas as camadas. Prova prática 2,5 pontos, T2 19/11; recuperação T2 26/11 no cronograma. Vídeo com aluno sem matrícula e aluno com matrícula, entrada/saída do mesmo aluno e colaborador, nome completo e release Git.
- Há erros de digitação em datas de alguns slides. Consultar o prazo do Classroom em caso de decisão de entrega; não tentar corrigir os materiais do professor.

## Relação com o projeto de Samuel

A estrutura encontrada previamente corresponde à trilha: quatro camadas, xUnit Domain/Infrastructure, SQL puro, três provedores, factories/VOs, DTOs/mappings, Argon2id, MAUI/MVVM, Dashboard, cadastros e configurações. Não foi feita nova execução de testes ou telas durante esta leitura.

Uma busca pontual confirmou no código recursos da aula 12: controle _isCarregando e assinatura do tema após carregar; debounce de 300ms na lista de colaboradores; Unfocused no e-mail; Switch para mostrar senha e toque na foto. Isso indica aplicação de parte do material complementar, sem provar ausência de erros em execução.

Recontextualização da análise anterior:

- Falta da tela de Matrícula: etapa futura (14/15), não falha de entrega até a 12.
- CPF mantido em AlunoMappingExtensions.UpdateFromDto: o mesmo uso de aluno.Cpf.Valor aparece no PDF 08, página 39. Pode gerar comportamento inesperado se a tela permitir mudar o CPF, mas não é divergência originada necessariamente pelo aluno.
- Plano/Flags zero ignorados na edição de Matrícula: o mesmo padrão != default aparece no PDF 08, página 43, e no serviço de matrícula, página 63. Limitação herdada do exemplo; só corrigir se solicitado e preservando a proposta didática.
- Caminho local fixo, credenciais padrão, DI em Application e alguns eventos no code-behind aparecem na orientação do curso. Diferenciar adequação à disciplina de melhorias para produção.
- Layout do professor é explicitamente exemplificativo; o aluno deve dar identidade visual própria. Não copiar automaticamente a aparência nem refatorar a arquitetura para outra tecnologia sem pedido.

Para próximos pedidos: preservar o limite ATÉ 12; distinguir exigência, exemplo, conteúdo complementar e etapa futura; propor mudanças pequenas e explicadas dentro dos padrões ensinados. Não antecipar 14/15, 16 ou Avaliação 03 sem pedido do usuário.
