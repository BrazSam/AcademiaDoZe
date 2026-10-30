using AcademiaDoZe.Presentation.AppMaui.ViewModels;
using AcademiaDoZe.Application.Enums;
namespace AcademiaDoZe.Presentation.AppMaui.Views;

public partial class MatriculaPage : ContentPage
{
    public MatriculaPage(MatriculaViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is MatriculaViewModel viewModel)
        {
            await viewModel.InitializeAsync();
            // ensure Fim is calculated when loading existing data
            CalculateFimFromPlanoAndInicio();
        }
    }
    private void PlanoPicker_SelectedIndexChanged(object? sender, EventArgs e)
    {
        CalculateFimFromPlanoAndInicio();
    }
    private void InicioDatePicker_DateSelected(object? sender, DateChangedEventArgs e)
    {
        CalculateFimFromPlanoAndInicio();
    }
    private void CalculateFimFromPlanoAndInicio()
    {
        if (BindingContext is not MatriculaViewModel vm) return;
        var plano = vm.Matricula.Plano;
        var inicio = vm.Matricula.DataInicio;
        DateOnly fim = inicio;
        fim = plano switch
        {
            AppMatriculaPlano.Mensal => inicio.AddMonths(1),
            AppMatriculaPlano.Trimestral => inicio.AddMonths(3),
            AppMatriculaPlano.Semestral => inicio.AddMonths(6),
            AppMatriculaPlano.Anual => inicio.AddYears(1),
            _ => inicio,
        };
        // update viewmodel and picker control
        vm.Matricula.DataFim = fim;
        //vm.OnPropertyChanged(nameof(vm.Matricula));
        // update readonly DatePicker
        try
        {
            FimDatePicker.Date = fim.ToDateTime(new TimeOnly(0, 0));
        }
        catch { }
    }
}