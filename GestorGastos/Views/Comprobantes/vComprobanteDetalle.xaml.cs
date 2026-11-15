using GestorGastos.ViewModels;

namespace GestorGastos.Views;

public partial class vComprobanteDetalle : ContentPage
{
    private readonly ComprobanteDetalleViewModel _vm;

    public vComprobanteDetalle(ComprobanteDetalleViewModel vm)
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
