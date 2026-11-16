namespace GestorGastos.Controls;

public class AnilloProgreso : GraphicsView
{
    public static readonly BindableProperty ProgresoProperty =
        BindableProperty.Create(nameof(Progreso), typeof(double), typeof(AnilloProgreso), 0.0,
            propertyChanged: (vista, anterior, nuevo) => ((AnilloProgreso)vista).Invalidate());

    public static readonly BindableProperty ColorAnilloProperty =
        BindableProperty.Create(nameof(ColorAnillo), typeof(Color), typeof(AnilloProgreso), Color.FromArgb("#7C5CDB"),
            propertyChanged: (vista, anterior, nuevo) => ((AnilloProgreso)vista).Invalidate());

    public AnilloProgreso()
    {
        Drawable = new Dibujo(this);
    }

    public double Progreso
    {
        get => (double)GetValue(ProgresoProperty);
        set => SetValue(ProgresoProperty, value);
    }

    public Color ColorAnillo
    {
        get => (Color)GetValue(ColorAnilloProperty);
        set => SetValue(ColorAnilloProperty, value);
    }

    private class Dibujo : IDrawable
    {
        private readonly AnilloProgreso _anillo;

        public Dibujo(AnilloProgreso anillo)
        {
            _anillo = anillo;
        }

        public void Draw(ICanvas canvas, RectF rect)
        {
            const float grosor = 8f;
            var tamano = Math.Min(rect.Width, rect.Height) - grosor;
            if (tamano <= 0)
            {
                return;
            }

            var x = rect.Center.X - tamano / 2;
            var y = rect.Center.Y - tamano / 2;

            canvas.StrokeSize = grosor;
            canvas.StrokeColor = Color.FromArgb("#EEE8F7");
            canvas.DrawEllipse(x, y, tamano, tamano);

            var progreso = Math.Clamp(_anillo.Progreso, 0, 1);
            if (progreso <= 0)
            {
                return;
            }

            canvas.StrokeColor = _anillo.ColorAnillo;
            canvas.StrokeLineCap = LineCap.Round;

            if (progreso >= 0.999)
            {
                canvas.DrawEllipse(x, y, tamano, tamano);
                return;
            }

            var barrido = (float)(progreso * 360);
            canvas.DrawArc(x, y, tamano, tamano, 90, 90 - barrido, true, false);
        }
    }
}
