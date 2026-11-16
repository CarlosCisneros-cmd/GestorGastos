using GestorGastos.ViewModels;

namespace GestorGastos.Views;

public partial class vPresupuestoForm : ContentPage
{
    private readonly PresupuestoFormViewModel _vm;

    public vPresupuestoForm(PresupuestoFormViewModel vm)
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
