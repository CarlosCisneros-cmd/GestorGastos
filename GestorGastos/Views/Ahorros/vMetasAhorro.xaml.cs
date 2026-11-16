using GestorGastos.ViewModels;

namespace GestorGastos.Views;

public partial class vMetasAhorro : ContentPage
{
    private readonly MetasAhorroViewModel _vm;

    public vMetasAhorro(MetasAhorroViewModel vm)
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
