using GestorGastos.ViewModels;

namespace GestorGastos.Views;

public partial class vCuentas : ContentPage
{
    private readonly CuentasViewModel _vm;

    public vCuentas(CuentasViewModel vm)
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
