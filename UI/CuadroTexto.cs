using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using Tetris2D.Graficos;

namespace Tetris2D.UI
{
    /// <summary>
    /// Caja de texto que hice para que el jugador escriba su nombre.
    /// Tambien muestra un cursor parpadeando.
    /// </summary>
    public class CuadroTexto
    {
        public string Nombre { get; private set; } = "";
        public int MaxCaracteres { get; set; } = 12;
        public bool Habilitado { get; set; } = true;

        private readonly DibujadorCuadros _cuadros;
        private readonly RenderizadorTexto _texto;

        // Guardo el area actual de la caja.
        private float _x, _y, _ancho, _alto;

        private float _tiempoParpadeo;   // tiempo para parpadear el cursor

        public CuadroTexto(DibujadorCuadros cuadros, RenderizadorTexto texto)
        {
            _cuadros = cuadros;
            _texto = texto;
        }

        /// <summary>Agrega al nombre cada caracter que escribe el jugador.</summary>
        public void Limpiar()
        {
            Nombre = "";
        }

        public void AlTexto(string caracter)
        {
            if (Habilitado && caracter.Length == 1 && Nombre.Length < MaxCaracteres)
                Nombre += caracter;
        }

        /// <summary>Con Backspace borro la ultima letra del nombre.</summary>
        public void AlTecla(Keys tecla)
        {
            if (Habilitado && tecla == Keys.Backspace && Nombre.Length > 0)
                Nombre = Nombre[..^1];
        }

        /// <summary>Actualizo el tiempo del cursor parpadeante.</summary>
        public void Actualizar(float dt)
        {
            _tiempoParpadeo += dt;
        }

        public bool Contiene(Vector2 punto)
        {
            return punto.X >= _x && punto.X <= _x + _ancho && punto.Y >= _y && punto.Y <= _y + _alto;
        }

        /// <summary>
        /// Dibuja la caja con fondo, borde, nombre escrito y cursor.
        /// </summary>
        public void Renderizar(float x, float y, float ancho, float alto)
        {
            _x = x;
            _y = y;
            _ancho = ancho;
            _alto = alto;

            _cuadros.DibujarRectangulo(TemaArcade.PanelOscuro, x, y, x + ancho, y + alto);

            Vector4 borde = Habilitado ? TemaArcade.Cian : TemaArcade.TextoSuave;
            float grosorBorde = MathF.Max(1f, alto * 0.02f);
            _cuadros.DibujarBordeRectangulo(borde, x, y, x + ancho, y + alto, grosorBorde);

            // Dibujo el nombre y el cursor al final.
            bool cursorVisible = Habilitado && (_tiempoParpadeo % 1.0f) < 0.6f;
            string contenido = Habilitado && cursorVisible ? Nombre + "|" : Nombre;

            float altoTexto = alto * 0.62f;
            float textoX = x + ancho * 0.05f;
            float textoY = y + (alto - altoTexto) * 0.5f;
            _texto.DibujarTexto(contenido, textoX, textoY, altoTexto, TemaArcade.Blanco);
        }
    }
}
