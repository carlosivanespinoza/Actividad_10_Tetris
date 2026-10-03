using OpenTK.Mathematics;

namespace Tetris2D.UI
{
    /// <summary>
    /// Aqui deje los colores que use para que todo el juego tenga el mismo estilo.
    /// </summary>
    public static class TemaArcade
    {
        // Color del fondo principal.
        public static readonly Vector4 Fondo = new(0.04f, 0.05f, 0.12f, 1f);

        // Colores para piezas y detalles de la interfaz.
        public static readonly Vector4 Cian = new(0.00f, 0.90f, 1.00f, 1f);
        public static readonly Vector4 Magenta = new(0.95f, 0.25f, 0.85f, 1f);
        public static readonly Vector4 Amarillo = new(1.00f, 0.85f, 0.20f, 1f);
        public static readonly Vector4 Verde = new(0.25f, 1.00f, 0.55f, 1f);
        public static readonly Vector4 Rojo = new(1.00f, 0.25f, 0.25f, 1f);
        public static readonly Vector4 Azul = new(0.20f, 0.42f, 1.00f, 1f);
        public static readonly Vector4 Naranja = new(1.00f, 0.50f, 0.12f, 1f);

        // Colores para textos.
        public static readonly Vector4 Blanco = new(1f, 1f, 1f, 1f);
        public static readonly Vector4 TextoSuave = new(0.70f, 0.76f, 0.92f, 1f);

        // Color para botones desactivados.
        public static readonly Vector4 Gris = new(0.40f, 0.42f, 0.50f, 1f);

        // Color para paneles y cajas de texto.
        public static readonly Vector4 PanelOscuro = new(0.05f, 0.07f, 0.16f, 0.95f);
    }
}
