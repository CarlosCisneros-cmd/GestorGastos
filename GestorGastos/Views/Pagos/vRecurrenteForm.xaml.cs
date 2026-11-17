using GestorGastos.ViewModels;

namespace GestorGastos.Views;

public partial class vRecurrenteForm : ContentPage
{
    private readonly RecurrenteFormViewModel _vm;

    public vRecurrenteForm(RecurrenteFormViewModel vm)
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
