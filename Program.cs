using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;

namespace Tetris2D
{
    /// <summary>
    /// Punto de entrada del juego Tetris2D.
    /// Aqui solo se configuran las opciones de la ventana y se arranca el
    /// bucle principal de OpenTK (Run). 
    /// </summary>
    internal static class Program
    {
        private static void Main()
        {
            // Configuracion del bucle de juego (valores por defecto de OpenTK).
            GameWindowSettings gameSettings = GameWindowSettings.Default;

            // Configuracion de la ventana nativa creada por GLFW.
            NativeWindowSettings nativeSettings = new()
            {
                ClientSize = new Vector2i(800, 600),
                Title = "Tetris2D — Graficación por Computadora"
            };

            using (TetrisGame juego = new(gameSettings, nativeSettings))
            {
                // Run() inicia el ciclo: OnLoad -> (Update/Render)* -> OnUnload.
                juego.Run();
            }
        }
    }
}