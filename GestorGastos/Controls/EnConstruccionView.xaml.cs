namespace GestorGastos.Controls;

public partial class EnConstruccionView : ContentView
{
    public static readonly BindableProperty ParteProperty =
        BindableProperty.Create(nameof(Parte), typeof(string), typeof(EnConstruccionView), "Parte 2");

    public static readonly BindableProperty DescripcionProperty =
        BindableProperty.Create(nameof(Descripcion), typeof(string), typeof(EnConstruccionView), "Pantalla en construcción");

    public EnConstruccionView()
    {
        InitializeComponent();
    }

    public string Parte
    {
        get => (string)GetValue(ParteProperty);
        set => SetValue(ParteProperty, value);
    }

    public string Descripcion
    {
        get => (string)GetValue(DescripcionProperty);
        set => SetValue(DescripcionProperty, value);
    }
}
