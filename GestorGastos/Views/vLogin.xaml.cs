using GestorGastos.ViewModels;

namespace GestorGastos.Views;

public partial class vLogin : ContentPage
{
    private readonly LoginViewModel _vm;
    private bool _huellaSolicitada;

    public vLogin(LoginViewModel vm)
    {
        InitializeComponent();
        BindingContext = _vm = vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        _vm.Actualizar();

        if (_vm.PuedeUsarHuella && !_huellaSolicitada)
        {
            _huellaSolicitada = true;
            await _vm.IngresarConHuellaCommand.ExecuteAsync(null);
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _huellaSolicitada = false;
    }
}
