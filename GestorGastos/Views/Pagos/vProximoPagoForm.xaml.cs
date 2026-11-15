using GestorGastos.ViewModels;

namespace GestorGastos.Views;

public partial class vProximoPagoForm : ContentPage
{
    private readonly ProximoPagoFormViewModel _vm;

    public vProximoPagoForm(ProximoPagoFormViewModel vm)
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
