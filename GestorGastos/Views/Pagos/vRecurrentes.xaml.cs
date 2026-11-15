using GestorGastos.ViewModels;

namespace GestorGastos.Views;

public partial class vRecurrentes : ContentPage
{
    private readonly RecurrentesViewModel _vm;

    public vRecurrentes(RecurrentesViewModel vm)
    {
        InitializeComponent();
        BindingContext = _vm = vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _vm.CargarAsync();
    }
}
