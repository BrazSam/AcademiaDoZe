# Memória de trabalho do projeto

Última atualização: 22/10/2026.

## Acordo com Samuel

Samuel pediu que o contexto do código e das orientações do professor fosse salvo para continuar o projeto nos próximos dias. O objetivo é trabalhar dentro da proposta da disciplina, explicando mudanças e respeitando as etapas cursadas.

**Limite declarado: implementado somente até a atividade 12.** Manter essa referência até nova informação do usuário. Leitura de materiais posteriores não significa autorização para implementá-los.

As orientações e fontes estão em [orientacoes-do-professor.md](orientacoes-do-professor.md). O arquivo raiz `AGENTS.md` direciona futuros trabalhos para estas notas.

## Estrutura observada no código

- `AcademiaDoZe.slnx`: solução .NET 10.
- `AcademiaDoZe.Domain`: entidades, enums, objetos de valor, Result/Notification, normalização e contratos de repositório. Sem referência a outros projetos.
- `AcademiaDoZe.Infrastructure`: SQL manual/ADO.NET; provedores SQLite, MySQL e SQL Server; scripts incorporados, inicialização, conversões de leitura e repositórios concretos.
- `AcademiaDoZe.Application`: DTOs, enums de aplicação, mapeamentos, interfaces e serviços, configuração de DI e PasswordHasher com Argon2id. Referencia Domain e Infrastructure, como no material.
- `AcademiaDoZe.Presentation.AppMaui`: XAML, MVVM/CommunityToolkit, Views, ViewModels, converters, Shell, configurações e mensageria. Referencia Application.
- `AcademiaDoZe.Domain.Tests` e `AcademiaDoZe.Infrastructure.Tests`: testes xUnit.

## Funcionalidades encontradas na leitura inicial

- Dashboard com totais e atalhos.
- Telas de listagem e cadastro/edição de Logradouro, Colaborador e Aluno.
- Configurações de tema e credenciais de banco, persistidas em Preferences e aplicadas via WeakReferenceMessenger.
- Matrícula com domínio, repositório, serviço, DTOs e testes; sem tela/rota implementada. A ausência da tela é compatível com o limite até 12.
- Entidades e interfaces de acesso de alunos/colaboradores já existem, mas o fluxo completo de entrada/saída pertence à Avaliação 03.
- Recursos complementares da aula 12 encontrados: debounce de 300 ms na listagem de colaboradores; controle de carregamento e assinatura do evento de tema após inicialização; validação de e-mail em Unfocused; mostrar senha com Switch; toque na foto para selecionar imagem.

Existência de código não comprova funcionamento. Ler e verificar os arquivos relevantes a cada pedido, especialmente após mudanças feitas no Visual Studio pelo usuário.

## Achados técnicos e sua relação com as aulas

- `AlunoMappingExtensions.UpdateFromDto` usa o CPF da entidade antiga, mesmo com outro no DTO. O mesmo código aparece no PDF 08, página 39. Se for corrigido, explicar como limitação do comportamento atual, não como desvio exclusivo de Samuel.
- `MatriculaMappingExtensions.UpdateFromDto` trata enum zero como ausência de alteração, impedindo voltar ao plano mensal ou limpar restrições em certos casos. O mesmo padrão aparece no PDF 08, página 43, e no serviço, página 63.
- Caminho Windows fixo para SQLite e credenciais padrão aparecem nos exemplos didáticos. Os valores concretos não devem ser reproduzidos nestas notas. Avaliar melhorias de configuração conforme o pedido.
- DTOs de saída de Aluno/Colaborador ocultam a senha. A atualização preserva a senha quando não preenchida; existem diferenças entre os serviços no tratamento do DTO recebido.
- UI deve conhecer os contratos da Application; não deve criar entidades nem acessar repositórios diretamente.

## Validação conhecida — não assumir sucesso

Na análise inicial, `dotnet --version` retornou 10.0.401. Tentou-se executar os testes de domínio; a execução iniciou, ficou sem resultado final e foi interrompida. A compilação dos testes de infraestrutura informou falha sem erro explicativo no resumo, exibindo avisos NU1902/NU1903 sobre uma dependência. Não houve conclusão de diagnóstico ou correção. Não afirmar que as suítes passaram.

`Infrastructure.Tests/TestBase.cs` apontava para SQL Server local; não executar indiscriminadamente contra o banco configurado. `ColaboradorCadastroRegressionTests` usa um arquivo SQLite temporário isolado.

Não foram executadas ou validadas as telas durante a leitura dos PDFs. Até a criação desta memória, nenhuma implementação foi alterada por este chat; apenas documentação foi adicionada.

## Como atualizar estas notas

Registrar mudanças importantes, etapa explicitamente informada pelo usuário, decisões e resultados reais de validação. Remover ou marcar achados resolvidos quando houver evidência. Manter os exemplos do professor como referência histórica, sem congelar bugs ou impedir correções solicitadas.

## Avaliação 03 — escopo conferido visualmente em 22/10/2026

Samuel pediu uma explicação simples de como começar a Avaliação 03, sem solicitar implementação. Isso não muda sua declaração de ter implementado até a atividade 12.

Fonte local enviada: `../devAval03.pdf`. As páginas 1–3 foram extraídas e renderizadas para inspeção visual. **Na página 3 estão riscados** os requisitos de exibir tempo restante do plano, calcular/exibir permanência e limitar/somar horas de colaborador. A extração textual não preserva esse risco. Para esta versão da avaliação, considerar essas funções fora do escopo obrigatório, salvo nova orientação. Corrige/amenda a leitura anterior das notas, que listava todas essas regras como se continuassem exigidas.

Permanecem: registrar aluno/colaborador e data/hora de entrada e saída; validar matrícula ativa do aluno (aplicar na entrada, permitindo saída de quem já entrou); demonstrar tentativa com aluno sem matrícula e entrada/saída com aluno matriculado, além de colaborador. Vídeo com nomes completos conforme enunciado, release ZIP, endereço do Git; logo com nome no Dashboard. T2: 19/11.

Conferência atual do código: entidades AcessoAluno/AcessoColaborador e testes básicos existem; ambas guardam ID da pessoa e DataHora, validando pessoa e horário de 06h a 22h. Interfaces IAcessoAlunoRepository/IAcessoColaboradorRepository existem. Scripts definem tb_acesso com pessoa_tipo, pessoa_id e data_hora; isso não comprova a existência da tabela no banco em uso. Ainda não existe campo explícito de entrada/saída, portanto definir a representação dos movimentos antes de implementar. Repositórios concretos, serviços/DTOs/mapeamentos e telas de acesso não foram encontrados; registros de DI desses componentes estão comentados. Matrícula já possui repositório e serviços reutilizáveis, sem necessidade de concluir sua tela para iniciar os serviços de acesso.

Sequência sugerida (somente plano): definir movimentos; implementar e testar persistência; serviço de entrada/saída de Aluno com validação de matrícula e estado; tela simples; repetir para Colaborador; validar cenários e preparar entrega. Preservar padrões da disciplina e não implementar automaticamente relatórios ou cálculos riscados.
