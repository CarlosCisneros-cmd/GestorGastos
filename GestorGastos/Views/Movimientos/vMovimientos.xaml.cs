using GestorGastos.ViewModels;

namespace GestorGastos.Views;

public partial class vMovimientos : ContentPage
{
    private readonly MovimientosViewModel _vm;

    public vMovimientos(MovimientosViewModel vm)
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
