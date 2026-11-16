using GestorGastos.ViewModels;

namespace GestorGastos.Views;

public partial class vDeudaDetalle : ContentPage
{
    private readonly DeudaDetalleViewModel _vm;

    public vDeudaDetalle(DeudaDetalleViewModel vm)
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
