using GestorGastos.ViewModels;

namespace GestorGastos.Views;

public partial class vCuentaForm : ContentPage
{
    private readonly CuentaFormViewModel _vm;

    public vCuentaForm(CuentaFormViewModel vm)
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
