using OpenTK.Mathematics;

namespace Tetris2D.UI
{
    /// <summary>
    /// Paleta de colores "arcade" del juego: neones brillantes sobre un fondo
    /// azul-oscuro de sala de maquinas. Todas las pantallas usan estos colores
    /// para mantener una identidad visual consistente.
    /// </summary>
    public static class TemaArcade
    {
        // Fondo principal de las pantallas.
        public static readonly Vector4 Fondo = new(0.04f, 0.05f, 0.12f, 1f);

        // Colores neon representativos del estilo arcade.
        public static readonly Vector4 Cian = new(0.00f, 0.90f, 1.00f, 1f);
        public static readonly Vector4 Magenta = new(0.95f, 0.25f, 0.85f, 1f);
        public static readonly Vector4 Amarillo = new(1.00f, 0.85f, 0.20f, 1f);
        public static readonly Vector4 Verde = new(0.25f, 1.00f, 0.55f, 1f);
        public static readonly Vector4 Rojo = new(1.00f, 0.25f, 0.25f, 1f);
        // Colores nuevos para las piezas de Tetris (Fase 2)
        public static readonly Vector4 Azul = new(0.20f, 0.45f, 1.00f, 1f);
        public static readonly Vector4 Naranja = new(1.00f, 0.60f, 0.10f, 1f);

        // Texto y apoyos visuales.
        public static readonly Vector4 Blanco = new(1f, 1f, 1f, 1f);
        public static readonly Vector4 TextoSuave = new(0.70f, 0.76f, 0.92f, 1f);

        // Estados de botones.
        public static readonly Vector4 Gris = new(0.40f, 0.42f, 0.50f, 1f);

        // Paneles / cajas de texto.
        public static readonly Vector4 PanelOscuro = new(0.05f, 0.07f, 0.16f, 0.95f);
    }
}