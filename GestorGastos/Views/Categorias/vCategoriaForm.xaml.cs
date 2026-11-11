using GestorGastos.ViewModels;

namespace GestorGastos.Views;

public partial class vCategoriaForm : ContentPage
{
    private readonly CategoriaFormViewModel _vm;

    public vCategoriaForm(CategoriaFormViewModel vm)
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
