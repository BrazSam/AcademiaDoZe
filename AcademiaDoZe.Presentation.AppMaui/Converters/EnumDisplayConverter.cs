using AcademiaDoZe.Application.Enums;
using System.Globalization;
namespace AcademiaDoZe.Presentation.AppMaui.Converters;
// Converte qualquer Enum para o Display(Name)
public sealed class EnumDisplayConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) => value is Enum e ? e.GetDisplayName() : string.Empty;
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotSupportedException();
}