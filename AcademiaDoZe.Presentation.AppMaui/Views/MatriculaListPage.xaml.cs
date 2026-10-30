using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Presentation.AppMaui.ViewModels;
namespace AcademiaDoZe.Presentation.AppMaui.Views;

public partial class MatriculaListPage : ContentPage
{
    public MatriculaListPage(MatriculaListViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is MatriculaListViewModel viewModel)
        {
            await viewModel.LoadMatriculasCommand.ExecuteAsync(null);
        }
    }
    private async void OnEditButtonClicked(object? sender, EventArgs e)
    {
        try
        {
            if (sender is Button button && button.BindingContext is MatriculaDto matricula && BindingContext is MatriculaListViewModel viewModel)
            {
                await viewModel.EditMatriculaCommand.ExecuteAsync(matricula);
            }
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Erro", $"Erro ao editar matricula: {ex.Message}", "OK");
        }
    }
    private async void OnDeleteButtonClicked(object? sender, EventArgs e)
    {
        try
        {
            if (sender is Button button && button.BindingContext is MatriculaDto matricula && BindingContext is MatriculaListViewModel viewModel)
            {
                await viewModel.DeleteMatriculaCommand.ExecuteAsync(matricula);
            }
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Erro", $"Erro ao excluir matricula: {ex.Message}", "OK");
        }
    }
    private async void OnMoreClicked(object? sender, EventArgs e)
    {
        if (sender is not Button btn ||
        btn.BindingContext is not MatriculaDto dto ||
        BindingContext is not MatriculaListViewModel vm)
            return;
        var choice = await DisplayActionSheetAsync("Acoes", "Cancelar", null, "Editar", "Excluir");
        switch (choice)
        {
            case "Editar":
                await vm.EditMatriculaCommand.ExecuteAsync(dto);
                break;
            case "Excluir":
                await vm.DeleteMatriculaCommand.ExecuteAsync(dto);
                break;
            default:
                break;
        }
    }
}