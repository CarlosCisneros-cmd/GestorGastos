using GestorGastos.ViewModels;

namespace GestorGastos.Views;

public partial class vDeudaForm : ContentPage
{
    private readonly DeudaFormViewModel _vm;

    public vDeudaForm(DeudaFormViewModel vm)
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
