using GestorGastos.ViewModels;

namespace GestorGastos.Views;

public partial class vPresupuestos : ContentPage
{
    private readonly PresupuestosViewModel _vm;

    public vPresupuestos(PresupuestosViewModel vm)
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
