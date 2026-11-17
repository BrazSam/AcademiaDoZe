using System.Globalization; //Samuel Braz dos Santos
using System.Text;
namespace AcademiaDoZe.Presentation.AppMaui.Converters;

// Regra única de máscara: cada '#' recebe um dígito, o resto é copiado (pontos, traço, parênteses)
public static class MascaraFormatador
{
    public static string Aplicar(string? texto, string mascara)
    {
        var digitos = new string([.. (texto ?? string.Empty).Where(char.IsDigit)]);
        var resultado = new StringBuilder();
        int i = 0;
        foreach (var c in mascara)
        {
            if (i >= digitos.Length) break; // acabaram os dígitos: para antes do próximo símbolo
            resultado.Append(c == '#' ? digitos[i++] : c);
        }
        return resultado.ToString();
    }
}

// Para exibir nas listagens: {Binding Cpf, Converter=..., ConverterParameter='###.###.###-##'}
public class MascaraConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => parameter is string mascara ? MascaraFormatador.Aplicar(value?.ToString(), mascara) : value;

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}