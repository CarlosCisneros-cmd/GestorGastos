using GestorGastos.ViewModels;

namespace GestorGastos.Views;

public partial class vMas : ContentPage
{
    private readonly MasViewModel _vm;

    public vMas(MasViewModel vm)
    {
        InitializeComponent();
        BindingContext = _vm = vm;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _vm.Cargar();
    }
}
