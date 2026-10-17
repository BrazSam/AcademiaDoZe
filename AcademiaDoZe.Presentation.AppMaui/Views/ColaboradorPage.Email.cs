using System.Text.RegularExpressions;
namespace AcademiaDoZe.Presentation.AppMaui.Views;
// classe parcial para validação de e-mail - ColaboradorPage.Email.cs
public partial class ColaboradorPage : ContentPage
{
	 // Validação simples de e-mail ao perder foco
	private void OnEmailUnfocused(object? sender, FocusEventArgs e)
	 {
	 var email = EmailEntry?.Text?.Trim();
	 // se vazio, limpa a mensagem
	if (string.IsNullOrEmpty(email))
	 {
	 EmailErrorLabel.IsVisible = false;
	 return;
	 }
	 // regex simples: local@dominio.tld
	var re = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
	 if (!re.IsMatch(email))
	 {
	 EmailErrorLabel.IsVisible = true;
	 EmailErrorLabel.Text = "Formato de e-mail inválido. Use nome@dominio.com";
	 EmailEntry?.Focus();
	 }
	 else { EmailErrorLabel.IsVisible = false; }
	 }
}