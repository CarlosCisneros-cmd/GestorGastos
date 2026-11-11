using GestorGastos.ViewModels;

namespace GestorGastos.Views;

public partial class vCategorias : ContentPage
{
    private readonly CategoriasViewModel _vm;

    public vCategorias(CategoriasViewModel vm)
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
