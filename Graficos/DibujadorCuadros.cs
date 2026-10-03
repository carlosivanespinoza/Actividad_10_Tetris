using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace Tetris2D.Graficos
{
    /// <summary>
    /// Esta clase la use para dibujar rectangulos, bordes y lineas en 2D.
    /// Me sirve para pintar la interfaz y tambien las casillas del tablero.
    /// </summary>
    public class DibujadorCuadros : IDisposable
    {
        private readonly GestorShader _shaders;
        private readonly int _vao;
        private readonly int _vbo;

        public DibujadorCuadros(GestorShader shaders)
        {
            _shaders = shaders;

            // Un rectangulo se dibuja con 2 triangulos, por eso preparo 6 vertices.
            _vao = GL.GenVertexArray();
            _vbo = GL.GenBuffer();

            GL.BindVertexArray(_vao);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, 6 * 2 * sizeof(float) * 4, IntPtr.Zero, BufferUsageHint.StreamDraw);

            GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 2 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);
        }

        /// <summary>
        /// Dibuja un rectangulo relleno con el color que se mande.
        /// </summary>
        public void DibujarRectangulo(Vector4 color, float x0, float y0, float x1, float y1)
        {
            // Aqui armo los dos triangulos que forman el rectangulo.
            float[] vertices =
            {
                x0, y0,  x1, y0,  x1, y1,
                x0, y0,  x1, y1,  x0, y1
            };

            GL.BindVertexArray(_vao);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
            GL.BufferSubData(BufferTarget.ArrayBuffer, 0, vertices.Length * sizeof(float), vertices);

            GL.UseProgram(_shaders.ProgramaColor);
            GL.Uniform4(_shaders.LColorColor, color);
            GL.DrawArrays(PrimitiveType.Triangles, 0, 6);
        }

        /// <summary>
        /// Dibuja el borde de un rectangulo usando cuatro rectangulos delgados.
        /// </summary>
        public void DibujarBordeRectangulo(Vector4 color, float x0, float y0, float x1, float y1, float grosor)
        {
            DibujarRectangulo(color, x0, y0, x1, y0 + grosor);                          // borde de arriba
            DibujarRectangulo(color, x0, y1 - grosor, x1, y1);                          // borde de abajo
            DibujarRectangulo(color, x0, y0 + grosor, x0 + grosor, y1 - grosor);        // borde izquierdo
            DibujarRectangulo(color, x1 - grosor, y0 + grosor, x1, y1 - grosor);        // borde derecho
        }

        /// <summary>
        /// Dibuja una linea con grosor entre dos puntos.
        /// </summary>
        public void DibujarLinea(Vector4 color, float x0, float y0, float x1, float y1, float grosor)
        {
            Vector2 p0 = new(x0, y0);
            Vector2 p1 = new(x1, y1);
            Vector2 direccion = p1 - p0;
            if (direccion.LengthSquared < 0.0001f)
                return;

            direccion.Normalize();
            Vector2 normal = new Vector2(-direccion.Y, direccion.X) * (grosor * 0.5f);

            Vector2 a = p0 - normal;
            Vector2 b = p0 + normal;
            Vector2 c = p1 + normal;
            Vector2 d = p1 - normal;

            float[] vertices =
            {
                a.X, a.Y,  b.X, b.Y,  c.X, c.Y,
                a.X, a.Y,  c.X, c.Y,  d.X, d.Y
            };

            GL.BindVertexArray(_vao);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
            GL.BufferSubData(BufferTarget.ArrayBuffer, 0, vertices.Length * sizeof(float), vertices);

            GL.UseProgram(_shaders.ProgramaColor);
            GL.Uniform4(_shaders.LColorColor, color);
            GL.DrawArrays(PrimitiveType.Triangles, 0, 6);
        }

        public void Dispose()
        {
            GL.DeleteVertexArray(_vao);
            GL.DeleteBuffer(_vbo);
        }
    }
}
