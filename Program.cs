using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;

namespace Tetris2D
{
    /// <summary>
    /// Aqui inicio el juego. Configure la ventana y despues arranco
    /// el ciclo principal de OpenTK.
    /// </summary>
    internal static class Program
    {
        private static void Main()
        {
            // Deje la configuracion normal del ciclo del juego.
            GameWindowSettings gameSettings = GameWindowSettings.Default;

            // Aqui defino el tamaño y titulo de la ventana.
            NativeWindowSettings nativeSettings = new()
            {
                ClientSize = new Vector2i(800, 600),
                Title = "Tetris2D — Graficación por Computadora"
            };

            using (TetrisGame juego = new(gameSettings, nativeSettings))
            {
                // Con Run empieza a cargar, actualizar y dibujar el juego.
                juego.Run();
            }
        }
    }
}
