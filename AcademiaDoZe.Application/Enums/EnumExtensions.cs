using System.ComponentModel.DataAnnotations;
using System.Reflection;
namespace AcademiaDoZe.Application.Enums;

public static class EnumExtensions
{
    public static string GetDisplayName(this Enum value)
    {
        var type = value.GetType();
        var field = type.GetField(value.ToString());
        var attribute = field?.GetCustomAttribute<DisplayAttribute>();
        if (attribute != null)
            return attribute.Name ?? value.ToString();
        // Tratamento para enum com atributo [Flags] onde múltiplos valores podem estar combinados
        if (type.GetCustomAttribute<FlagsAttribute>() != null)
        {
            var names = new List<string>();
            foreach (Enum flag in Enum.GetValues(type))
            {
                if (Convert.ToInt64(flag) != 0 && value.HasFlag(flag))
                {
                    var flagField = type.GetField(flag.ToString());
                    var flagAttr = flagField?.GetCustomAttribute<DisplayAttribute>();
                    names.Add(flagAttr?.Name ?? flag.ToString());
                }
            }
            if (names.Count > 0)
                return string.Join(", ", names);
        }
        return value.ToString();
    }
}
// Console.WriteLine( MatriculaRestricoes.ProblemasRespiratorios.GetDisplayName() );
// Exibe: Problemas Respiratórios