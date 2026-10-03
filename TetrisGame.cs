using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using Tetris2D.Graficos;
using Tetris2D.Pantallas;
using Tetris2D.UI;

namespace Tetris2D
{
    /// <summary>
    /// Esta es la ventana principal del juego. Aqui cargo las herramientas
    /// generales y mando los eventos a la pantalla que este activa.
    /// </summary>
    public class TetrisGame : GameWindow
    {
        private GestorShader _shaders = null!;
        private DibujadorCuadros _cuadros = null!;
        private RenderizadorTexto _texto = null!;
        private Pantalla _pantalla = null!;

        public TetrisGame(GameWindowSettings gws, NativeWindowSettings nws)
            : base(gws, nws)
        {
        }

        protected override void OnLoad()
        {
            base.OnLoad();

            // Active transparencias para que el texto y bordes se vean mejor.
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            // Creo una sola vez las herramientas de dibujo y texto.
            _shaders = new GestorShader();
            _cuadros = new DibujadorCuadros(_shaders);
            _texto = new RenderizadorTexto(_shaders, new GeneradorFuenteAtlas("Arial Black", 48f, true));

            // Primero muestro la pantalla de bienvenida.
            _pantalla = new PantallaBienvenida(_shaders, _cuadros, _texto);
            _pantalla.JugarSolicitado += nombre => CambiarPantalla(new PantallaJuego(_shaders, _cuadros, _texto, nombre));
            _pantalla.Cargar();
        }

        /// <summary>
        /// Cuando cambia el tamaño de la ventana, ajusto el area donde dibuja OpenGL.
        /// </summary>
        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);
            GL.Viewport(0, 0, FramebufferSize.X, FramebufferSize.Y);
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);
            _pantalla.Actualizar((float)e.Time, PuntoRaton());
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);

            GL.ClearColor(TemaArcade.Fondo.X, TemaArcade.Fondo.Y, TemaArcade.Fondo.Z, TemaArcade.Fondo.W);
            GL.Clear(ClearBufferMask.ColorBufferBit);

            // Uso una proyeccion en pixeles para dibujar mas facil.
            Matrix4 proyeccion = Matrix4.CreateOrthographicOffCenter(
                0, FramebufferSize.X, FramebufferSize.Y, 0, -1, 1);

            GL.UseProgram(_shaders.ProgramaColor);
            GL.UniformMatrix4(_shaders.LColorProyeccion, false, ref proyeccion);

            GL.UseProgram(_shaders.ProgramaTexto);
            GL.UniformMatrix4(_shaders.LTextoProyeccion, false, ref proyeccion);
            GL.UseProgram(0);

            _pantalla.Renderizar(FramebufferSize.X, FramebufferSize.Y);
            SwapBuffers();
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);
            _pantalla.AlClick(PuntoRaton(), e.Button);
        }

        protected override void OnTextInput(TextInputEventArgs e)
        {
            base.OnTextInput(e);
            _pantalla.AlTexto(e.AsString);
        }

        protected override void OnKeyDown(KeyboardKeyEventArgs e)
        {
            base.OnKeyDown(e);
            _pantalla.AlTecla(e.Key);
        }

        /// <summary>
        /// Convierto la posicion del mouse a pixeles reales de la ventana.
        /// </summary>
        private Vector2 PuntoRaton()
        {
            float sx = Size.X > 0 ? FramebufferSize.X / (float)Size.X : 1f;
            float sy = Size.Y > 0 ? FramebufferSize.Y / (float)Size.Y : 1f;
            return MousePosition * new Vector2(sx, sy);
        }

        private void CambiarPantalla(Pantalla nuevaPantalla)
        {
            _pantalla = nuevaPantalla;
            _pantalla.Cargar();
        }

        protected override void OnUnload()
        {
            _texto.Dispose();
            _cuadros.Dispose();
            _shaders.Dispose();
            base.OnUnload();
        }
    }
}
