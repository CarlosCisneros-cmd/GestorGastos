using GestorGastos.ViewModels;

namespace GestorGastos.Views;

public partial class vDeudas : ContentPage
{
    private readonly DeudasViewModel _vm;

    public vDeudas(DeudasViewModel vm)
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
