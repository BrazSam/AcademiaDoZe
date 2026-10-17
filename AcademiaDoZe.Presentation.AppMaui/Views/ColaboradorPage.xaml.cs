using AcademiaDoZe.Presentation.AppMaui.ViewModels;
using Microsoft.Maui.Controls;
namespace AcademiaDoZe.Presentation.AppMaui.Views;

public partial class ColaboradorPage : ContentPage
{
    private bool _initialized;
    public ColaboradorPage(ColaboradorViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (!_initialized && BindingContext is ColaboradorViewModel viewModel)
        {
            _initialized = true;
            await viewModel.InitializeAsync();
        }
    }

    private void OnShowPasswordToggled(object? sender, ToggledEventArgs e)
    {
        if (SenhaEntry is not null)
        {
            // Switch.IsToggled == true -> mostrar senha -> IsPassword = false
            SenhaEntry.IsPassword = !e.Value;
        }
    }

}
