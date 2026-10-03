using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using Tetris2D.Graficos;

namespace Tetris2D.UI
{
    /// <summary>
    /// Con esta clase dibujo los textos del juego usando el atlas de letras.
    /// La uso para titulos, puntaje, nivel y mensajes.
    /// </summary>
    public class RenderizadorTexto : IDisposable
    {
        private readonly GestorShader _shaders;
        private readonly GeneradorFuenteAtlas _atlas;
        private readonly int _vao;
        private readonly int _vbo;

        public RenderizadorTexto(GestorShader shaders, GeneradorFuenteAtlas atlas)
        {
            _shaders = shaders;
            _atlas = atlas;

            // Cada vertice guarda posicion y coordenada de textura.
            _vao = GL.GenVertexArray();
            _vbo = GL.GenBuffer();

            GL.BindVertexArray(_vao);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, 6 * 4 * sizeof(float) * 4, IntPtr.Zero, BufferUsageHint.StreamDraw);

            GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float), 2 * sizeof(float));
            GL.EnableVertexAttribArray(1);
        }

        /// <summary>
        /// Calculo el ancho del texto para poder centrarlo.
        /// </summary>
        public float MedirTexto(string texto, float altoPixeles)
        {
            if (string.IsNullOrEmpty(texto))
                return 0f;

            float escala = altoPixeles / _atlas.Altura;
            float ancho = 0f;
            foreach (char c in texto)
            {
                if (_atlas.Glifos.TryGetValue(c, out Glifo? g))
                    ancho += g.Ancho * escala;
            }
            return ancho;
        }

        /// <summary>
        /// Dibuja una linea de texto en la posicion indicada.
        /// </summary>
        public void DibujarTexto(string texto, float x, float y, float altoPixeles, Vector4 color)
        {
            if (string.IsNullOrEmpty(texto))
                return;

            float escala = altoPixeles / _atlas.Altura;
            float altoLetra = _atlas.Altura * escala;

            // Cada letra se dibuja como un rectangulo de dos triangulos.
            var vertices = new List<float>(texto.Length * 24);
            float cursorX = x;

            foreach (char c in texto)
            {
                if (!_atlas.Glifos.TryGetValue(c, out Glifo? g))
                    continue;   // si el caracter no existe, lo ignoro

                float anchoLetra = g.AnchoCelda * escala;
                float u0 = g.U0, v0 = g.V0, u1 = g.U1, v1 = g.V1;

                vertices.AddRange(new float[]
                {
                    cursorX, y,         u0, v0,          // esquina de arriba izquierda
                    cursorX + anchoLetra, y, u1, v0,     // esquina de arriba derecha
                    cursorX + anchoLetra, y + altoLetra, u1, v1,  // esquina de abajo derecha

                    cursorX, y,         u0, v0,          // segundo triangulo
                    cursorX + anchoLetra, y + altoLetra, u1, v1,
                    cursorX, y + altoLetra, u0, v1       // esquina de abajo izquierda
                });

                cursorX += g.Ancho * escala;   // avanzo a la siguiente letra
            }

            if (vertices.Count == 0)
                return;

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, _atlas.Textura);

            GL.BindVertexArray(_vao);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Count * sizeof(float), vertices.ToArray(), BufferUsageHint.StreamDraw);

            GL.UseProgram(_shaders.ProgramaTexto);
            GL.Uniform1(_shaders.LTextoTextura, 0);     // uso la textura 0
            GL.Uniform4(_shaders.LTextoColor, color);
            GL.DrawArrays(PrimitiveType.Triangles, 0, vertices.Count / 4);
        }

        public void Dispose()
        {
            GL.DeleteVertexArray(_vao);
            GL.DeleteBuffer(_vbo);
            _atlas.Dispose();
        }
    }
}
