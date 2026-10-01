using AcademiaDoZe.Presentation.AppMaui.ViewModels;
namespace AcademiaDoZe.Presentation.AppMaui.Views;

public partial class LogradouroPage : ContentPage
{
    public LogradouroPage(LogradouroViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
    protected override async void OnAppearing()
    {
        /* implementar depois */
    }
}