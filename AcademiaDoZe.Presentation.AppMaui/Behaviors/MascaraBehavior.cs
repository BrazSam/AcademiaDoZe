using AcademiaDoZe.Presentation.AppMaui.Converters; //Samuel Braz dos Santos
namespace AcademiaDoZe.Presentation.AppMaui.Behaviors;

// Formata o Entry enquanto o usuário digita
public class MascaraBehavior : Behavior<Entry>
{
    public static readonly BindableProperty MascaraProperty =
        BindableProperty.Create(nameof(Mascara), typeof(string), typeof(MascaraBehavior), string.Empty);
    public string Mascara
    {
        get => (string)GetValue(MascaraProperty);
        set => SetValue(MascaraProperty, value);
    }

    protected override void OnAttachedTo(Entry entry)
    {
        base.OnAttachedTo(entry);
        entry.TextChanged += OnTextChanged;
    }
    protected override void OnDetachingFrom(Entry entry)
    {
        entry.TextChanged -= OnTextChanged;
        base.OnDetachingFrom(entry);
    }
    private void OnTextChanged(object? sender, TextChangedEventArgs e)
    {
        if (sender is not Entry entry || string.IsNullOrEmpty(Mascara))
            return;
        var formatado = MascaraFormatador.Aplicar(e.NewTextValue, Mascara);
        if (entry.Text != formatado)
        {
            entry.Text = formatado;                  // dispara TextChanged de novo, mas aí já está igual e para
            entry.CursorPosition = formatado.Length; // cursor no fim, para continuar digitando
        }
    }
}