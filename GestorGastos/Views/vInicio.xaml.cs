using GestorGastos.ViewModels;

namespace GestorGastos.Views;

public partial class vInicio : ContentPage
{
    private readonly InicioViewModel _vm;

    public vInicio(InicioViewModel vm)
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
