using GestorGastos.ViewModels;

namespace GestorGastos.Views;

public partial class vProximosPagos : ContentPage
{
    private readonly ProximosPagosViewModel _vm;

    public vProximosPagos(ProximosPagosViewModel vm)
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
