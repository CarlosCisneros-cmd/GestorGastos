using GestorGastos.ViewModels;

namespace GestorGastos.Views;

public partial class vGaleriaComprobantes : ContentPage
{
    private readonly GaleriaComprobantesViewModel _vm;

    public vGaleriaComprobantes(GaleriaComprobantesViewModel vm)
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
