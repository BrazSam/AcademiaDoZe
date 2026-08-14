using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace AcademiaDoZe.Domain.Services;

public static partial class NormalizacaoService
{
    // verifica se o texto é nulo ou vazio
    public static bool TextoVazioOuNulo(string? texto) => string.IsNullOrWhiteSpace(texto);
    // remove espaços repetidos e espaços no início e no final do texto
    public static string LimparEspacos(string? texto) => string.IsNullOrWhiteSpace(texto) ? string.Empty : EspacosRegex().Replace(texto, " ").Trim();// limpa todos os espaços
    public static string LimparTodosEspacos(string? texto) => string.IsNullOrWhiteSpace(texto) ? string.Empty : texto.Replace(" ", string.Empty);// converte o texto para maiúsculo
    public static string ParaMaiusculo(string? texto) => string.IsNullOrEmpty(texto) ? string.Empty : texto.ToUpperInvariant();// manter somente digitos numericos
    public static string LimparEDigitos(string? texto) => string.IsNullOrEmpty(texto) ? string.Empty : new string([.. texto.Where(char.IsDigit)]); [GeneratedRegex(@"\s+")]
    private static partial Regex EspacosRegex();
}
