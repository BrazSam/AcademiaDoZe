using AcademiaDoZe.Application.Enums;
using AcademiaDoZe.Domain.Enums;
namespace AcademiaDoZe.Application.Mappings;

public static class ColaboradorEnumMappingExtensions
{
    public static ColaboradorTipo ToDomain(this AppColaboradorTipo appTipo)
    {
        return appTipo switch
        {
            AppColaboradorTipo.Administrador => ColaboradorTipo.Administrador,
            AppColaboradorTipo.Atendente => ColaboradorTipo.Atendente,
            AppColaboradorTipo.Instrutor => ColaboradorTipo.Instrutor,
            _ => throw new ArgumentOutOfRangeException(nameof(appTipo))
        };
    }
    public static AppColaboradorTipo ToApplication(this ColaboradorTipo domainTipo)
    {
        return domainTipo switch
        {
            ColaboradorTipo.Administrador => AppColaboradorTipo.Administrador,
            ColaboradorTipo.Atendente => AppColaboradorTipo.Atendente,
            ColaboradorTipo.Instrutor => AppColaboradorTipo.Instrutor,
            _ => throw new ArgumentOutOfRangeException(nameof(domainTipo))
        };
    }
    public static ColaboradorVinculo ToDomain(this AppColaboradorVinculo appVinculo)
    {
        return appVinculo switch
        {
            AppColaboradorVinculo.CLT => ColaboradorVinculo.CLT,
            AppColaboradorVinculo.Estagio => ColaboradorVinculo.Estagio,
            _ => throw new ArgumentOutOfRangeException(nameof(appVinculo))
        };
    }
    public static AppColaboradorVinculo ToApplication(this ColaboradorVinculo domainVinculo)
    {
        return domainVinculo switch
        {
            ColaboradorVinculo.CLT => AppColaboradorVinculo.CLT,
            ColaboradorVinculo.Estagio => AppColaboradorVinculo.Estagio,
            _ => throw new ArgumentOutOfRangeException(nameof(domainVinculo))
        };
    }
}
