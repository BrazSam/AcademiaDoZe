using AcademiaDoZe.Application.DTOs;
using System.Globalization;
namespace AcademiaDoZe.Presentation.AppMaui.Converters;

public class FotoToImageSourceConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is ArquivoDto foto && foto.Conteudo != null && foto.Conteudo.Length > 0)
        {
            return ImageSource.FromStream(() => new MemoryStream(foto.Conteudo));
        }
        return null;
    }
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}