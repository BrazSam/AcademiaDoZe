using AcademiaDoZe.Application.Interfaces;
using AcademiaDoZe.Application.Services;
using AcademiaDoZe.Domain.Repositories;
using AcademiaDoZe.Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection;
namespace AcademiaDoZe.Application.DependencyInjection;
//Samuel Braz dos Santos
public static class ApplicationDependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Registra os serviços da camada de aplicação
        services.AddTransient<ILogradouroService, LogradouroService>();
        services.AddTransient<IColaboradorService, ColaboradorService>();
        services.AddTransient<IAlunoService, AlunoService>();
        services.AddTransient<IMatriculaService, MatriculaService>();
        /*services.AddTransient<IAcessoAlunoService, AcessoAlunoService>();
        services.AddTransient<IAcessoColaboradorService, AcessoColaboradorService>();*/
        // AddScoped: cria uma instância do serviço por requisição HTTP.
        // AddSingleton: cria uma única instância do serviço durante toda a vida útil da aplicação.
        // AddTransient: cria uma nova instância do serviço toda vez que ele é solicitado.
        // Registra as fábricas Func<IRepo> para criar instâncias sob demanda nos services
        services.AddTransient(provider =>
        {
            var config = provider.GetRequiredService<RepositoryConfig>();
            return (Func<ILogradouroRepository>)(() => new LogradouroRepository(config.ConnectionString, config.DatabaseType));
        });
        services.AddTransient(provider =>
        {
            var config = provider.GetRequiredService<RepositoryConfig>();
            return (Func<IColaboradorRepository>)(() => new ColaboradorRepository(config.ConnectionString, config.DatabaseType));
        });
        services.AddTransient(provider =>
        {
            var config = provider.GetRequiredService<RepositoryConfig>();
            return (Func<IAlunoRepository>)(() => new AlunoRepository(config.ConnectionString, config.DatabaseType));
        });
        services.AddTransient(provider =>
        {
            var config = provider.GetRequiredService<RepositoryConfig>();
            return (Func<IMatriculaRepository>)(() => new MatriculaRepository(config.ConnectionString, config.DatabaseType));
        });
        /*services.AddTransient(provider =>
        {
            var config = provider.GetRequiredService<RepositoryConfig>();
            return (Func<IAcessoAlunoRepository>)(() => new AcessoAlunoRepository(config.ConnectionString, config.DatabaseType));
        });
        services.AddTransient(provider =>
        {
            var config = provider.GetRequiredService<RepositoryConfig>();
            return (Func<IAcessoColaboradorRepository>)(() => new AcessoColaboradorRepository(config.ConnectionString, config.DatabaseType));
        });*/
        return services;
    }
}