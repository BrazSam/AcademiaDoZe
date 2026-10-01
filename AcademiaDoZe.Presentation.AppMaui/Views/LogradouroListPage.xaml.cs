using AcademiaDoZe.Presentation.AppMaui.ViewModels;
using AcademiaDoZe.Application.DTOs;
namespace AcademiaDoZe.Presentation.AppMaui.Views;

public partial class LogradouroListPage : ContentPage
{
    public LogradouroListPage(LogradouroListViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
    protected override async void OnAppearing()
    {
        /* implementar depois */
    }
    private async void OnEditButtonClicked(object sender, EventArgs e)
    {
        /* implementar depois */
    }
    private async void OnDeleteButtonClicked(object sender, EventArgs e)
    {
        /* implementar depois */
    }
}