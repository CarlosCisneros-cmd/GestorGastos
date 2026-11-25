using System.ComponentModel;
using GestorGastos.Models;
using GestorGastos.ViewModels;

namespace GestorGastos.Views;

public partial class vCalendario : ContentPage
{
    private readonly CalendarioViewModel _vm;
    private readonly List<CeldaDia> _celdas = new();

    private readonly Color _primario = Recurso("Primario", Colors.MediumPurple);
    private readonly Color _ingreso = Recurso("Ingreso", Colors.Green);
    private readonly Color _gasto = Recurso("Gasto", Colors.Red);
    private readonly Color _pago = Recurso("Advertencia", Colors.Orange);

    public vCalendario(CalendarioViewModel vm)
    {
        InitializeComponent();
        BindingContext = _vm = vm;
        CrearCeldas();
        _vm.PropertyChanged += Vm_PropertyChanged;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _vm.CargarAsync();
    }

    // Se crean las 42 celdas una sola vez (6 semanas x 7 días)
    private void CrearCeldas()
    {
        for (var i = 0; i < 42; i++)
        {
            var numero = new Label { FontSize = 14, HorizontalOptions = LayoutOptions.Center };
            var puntos = new Label { FontSize = 9, HorizontalOptions = LayoutOptions.Center, FormattedText = new FormattedString() };
            var fondo = new Border
            {
                StrokeThickness = 0,
                StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 12 },
                BackgroundColor = Colors.Transparent,
                Margin = 2,
                Content = new VerticalStackLayout
                {
                    VerticalOptions = LayoutOptions.Center,
                    Spacing = 0,
                    Children = { numero, puntos }
                }
            };

            var celda = new CeldaDia(fondo, numero, puntos);
            var toque = new TapGestureRecognizer();
            toque.Tapped += (_, _) => SeleccionarCelda(celda);
            fondo.GestureRecognizers.Add(toque);

            Grid.SetRow(fondo, i / 7);
            Grid.SetColumn(fondo, i % 7);
            GridDias.Children.Add(fondo);
            _celdas.Add(celda);
        }
    }

    private void Vm_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(CalendarioViewModel.Dias))
        {
            ActualizarCeldas();
        }
        else if (e.PropertyName == nameof(CalendarioViewModel.DiaSeleccionado))
        {
            ActualizarSeleccion();
        }
    }

    // Solo cambia textos y colores; no vuelve a crear vistas
    private void ActualizarCeldas()
    {
        var dias = _vm.Dias;
        for (var i = 0; i < _celdas.Count; i++)
        {
            var celda = _celdas[i];
            var dia = i < dias.Count ? dias[i] : null;
            celda.Dia = dia;

            if (dia is null || !dia.EsDelMes)
            {
                celda.Numero.Text = string.Empty;
                celda.Puntos.FormattedText = new FormattedString();
                celda.Fondo.Stroke = Colors.Transparent;
                continue;
            }

            celda.Numero.Text = dia.Fecha.Day.ToString();
            celda.Fondo.Stroke = dia.EsHoy ? _primario : Colors.Transparent;
            celda.Fondo.StrokeThickness = dia.EsHoy ? 1.5 : 0;

            var texto = new FormattedString();
            if (dia.TieneIngresos) texto.Spans.Add(new Span { Text = "●", TextColor = _ingreso });
            if (dia.TieneGastos) texto.Spans.Add(new Span { Text = "●", TextColor = _gasto });
            if (dia.TienePagos) texto.Spans.Add(new Span { Text = "●", TextColor = _pago });
            if (texto.Spans.Count == 0) texto.Spans.Add(new Span { Text = " " });
            celda.Puntos.FormattedText = texto;
        }

        ActualizarSeleccion();
    }

    private void ActualizarSeleccion()
    {
        var seleccionado = _vm.DiaSeleccionado;
        var colorSeleccion = _primario.WithAlpha(0.30f);
        foreach (var celda in _celdas)
        {
            var activo = celda.Dia is not null && seleccionado is not null && celda.Dia.EsDelMes && celda.Dia.Fecha == seleccionado.Fecha;
            celda.Fondo.BackgroundColor = activo ? colorSeleccion : Colors.Transparent;
        }
    }

    private void SeleccionarCelda(CeldaDia celda)
    {
        if (celda.Dia is not null && celda.Dia.EsDelMes)
        {
            _vm.SeleccionarDiaCommand.Execute(celda.Dia);
        }
    }

    private async void ListaDia_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is TransaccionDetalle movimiento)
        {
            ListaDia.SelectedItem = null;
            await _vm.AbrirMovimientoCommand.ExecuteAsync(movimiento);
        }
    }

    private static Color Recurso(string clave, Color porDefecto)
    {
        return Application.Current?.Resources.TryGetValue(clave, out var valor) == true && valor is Color color
            ? color
            : porDefecto;
    }

    private sealed class CeldaDia
    {
        public CeldaDia(Border fondo, Label numero, Label puntos)
        {
            Fondo = fondo;
            Numero = numero;
            Puntos = puntos;
        }

        public Border Fondo { get; }
        public Label Numero { get; }
        public Label Puntos { get; }
        public DiaCalendario? Dia { get; set; }
    }
}
