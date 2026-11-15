using GestorGastos.ViewModels;

namespace GestorGastos.Views;

public partial class vCalendario : ContentPage
{
    private readonly CalendarioViewModel _vm;

    public vCalendario(CalendarioViewModel vm)
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
