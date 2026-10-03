using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using Tetris2D.Graficos;
using Tetris2D.UI;

namespace Tetris2D.Pantallas
{
    /// <summary>
    /// Esta clase base me sirve para que todas las pantallas tengan los mismos metodos.
    /// Asi puedo cambiar entre bienvenida y juego sin repetir tanto codigo.
    /// </summary>
    public abstract class Pantalla
    {
        // Estas herramientas las usan todas las pantallas.
        protected readonly GestorShader Shaders;
        protected readonly DibujadorCuadros Cuadros;
        protected readonly RenderizadorTexto Texto;

        /// <summary>
        /// Este evento lo uso cuando la pantalla de bienvenida pide empezar la partida.
        /// </summary>
        public event Action<string>? JugarSolicitado;

        protected Pantalla(GestorShader shaders, DibujadorCuadros cuadros, RenderizadorTexto texto)
        {
            Shaders = shaders;
            Cuadros = cuadros;
            Texto = texto;
        }

        /// <summary>Aqui cargo lo que necesite la pantalla.</summary>
        public virtual void Cargar() { }

        /// <summary>Aqui actualizo la logica de la pantalla en cada frame.</summary>
        public virtual void Actualizar(float dt, Vector2 raton) { }

        /// <summary>Aqui dibujo la pantalla completa.</summary>
        public abstract void Renderizar(float ancho, float alto);

        /// <summary>Aqui recibo los clics del mouse.</summary>
        public virtual void AlClick(Vector2 posicion, MouseButton boton) { }

        /// <summary>Aqui recibo las teclas presionadas.</summary>
        public virtual void AlTecla(Keys tecla) { }

        /// <summary>Aqui recibo texto escrito por el usuario.</summary>
        public virtual void AlTexto(string caracter) { }

        /// <summary>Aviso a la ventana principal que ya debe abrir el juego.</summary>
        protected void SolicitarJuego(string nombreJugador)
        {
            JugarSolicitado?.Invoke(nombreJugador);
        }
    }
}
