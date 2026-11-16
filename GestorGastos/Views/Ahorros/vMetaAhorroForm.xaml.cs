using GestorGastos.ViewModels;

namespace GestorGastos.Views;

public partial class vMetaAhorroForm : ContentPage
{
    private readonly MetaAhorroFormViewModel _vm;

    public vMetaAhorroForm(MetaAhorroFormViewModel vm)
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
