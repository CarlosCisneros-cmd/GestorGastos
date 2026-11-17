using GestorGastos.ViewModels;

namespace GestorGastos.Views;

public partial class vRegistrarMovimiento : ContentPage
{
    private readonly RegistrarMovimientoViewModel _vm;

    public vRegistrarMovimiento(RegistrarMovimientoViewModel vm)
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
