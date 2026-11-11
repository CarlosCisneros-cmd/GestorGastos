using GestorGastos.ViewModels;

namespace GestorGastos.Views;

public partial class vAjustes : ContentPage
{
    private readonly AjustesViewModel _vm;

    public vAjustes(AjustesViewModel vm)
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
