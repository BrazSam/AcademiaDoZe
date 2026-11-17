using AcademiaDoZe.Presentation.AppMaui.ViewModels; //Samuel Braz dos Santos
namespace AcademiaDoZe.Presentation.AppMaui.Views;

public partial class AcessoPage : ContentPage
{
    public AcessoPage(AcessoViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is AcessoViewModel viewModel)
            viewModel.Iniciar();   // liga o relógio
        CpfEntry.Focus();          // já abre pronto para digitar
    }
    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        if (BindingContext is AcessoViewModel viewModel)
            viewModel.Parar();     // desliga o relógio ao sair da tela
    }
}