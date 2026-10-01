using AcademiaDoZe.Presentation.AppMaui.ViewModels;
namespace AcademiaDoZe.Presentation.AppMaui.Views;

public partial class DashboardListPage : ContentPage
{
    public DashboardListPage(DashboardListViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is DashboardListViewModel viewModel)
        {
            await viewModel.LoadDashboardDataCommand.ExecuteAsync(null);
        }
    }
}