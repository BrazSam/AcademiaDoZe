using AcademiaDoZe.Application.Enums;
using AcademiaDoZe.Domain.Enums;
namespace AcademiaDoZe.Application.Mappings;

public static class MatriculaEnumMappingExtensions
{
    public static MatriculaPlano ToDomain(this AppMatriculaPlano appPlano)
    {
        return (MatriculaPlano)appPlano;
    }
    public static AppMatriculaPlano ToApplication(this MatriculaPlano domainPlano)
    {
        return (AppMatriculaPlano)domainPlano;
    }
    public static MatriculaRestricoes ToDomain(this AppMatriculaRestricoes appRestricoes)
    {
        return (MatriculaRestricoes)appRestricoes;
    }
    public static AppMatriculaRestricoes ToApplication(this MatriculaRestricoes domainRestricoes)
    {
        return (AppMatriculaRestricoes)domainRestricoes;
    }
}