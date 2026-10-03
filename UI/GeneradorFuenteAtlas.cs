using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Text;
using OpenTK.Graphics.OpenGL4;

namespace Tetris2D.UI
{
    /// <summary>
    /// Guardo la informacion de cada letra dentro de la textura de texto.
    /// </summary>
    public sealed class Glifo
    {
        public float U0, V0, U1, V1;   // region de la letra en la textura
        public float AnchoCelda;       // ancho de cada celda del atlas
        public float Ancho;            // espacio que avanza el texto
    }

    /// <summary>
    /// Esta clase genera una textura con letras para poder dibujar texto en OpenGL.
    /// Asi puedo mostrar titulos, nombres y puntajes dentro del juego.
    /// </summary>
    public class GeneradorFuenteAtlas : IDisposable
    {
        // Caracteres que soporta la textura de letras.
        public const string Caracteres =
            "ABCDEFGHIJKLMNÑOPQRSTUVWXYZ" +
            "abcdefghijklmnñopqrstuvwxyz" +
            "0123456789" +
            "ÁÉÍÓÚÜáéíóúü" +
            " .,:;!?¿¡-_'\"()[]{}<>/\\%&@#$*+=|~^`";

        public int Textura { get; private set; }
        public Dictionary<char, Glifo> Glifos { get; } = new();
        public float Altura { get; private set; }

        /// <summary>Fuentes que pruebo hasta encontrar una instalada.</summary>
        private static readonly string[] FuentesTentativas =
        {
            "Arial Black", "Arial", "Microsoft Sans Serif", "Segoe UI", "Consolas"
        };

        public GeneradorFuenteAtlas(string nombreFuentePreferida, float tamanoPixeles, bool negrita)
        {
            string[] candidatos = new[] { nombreFuentePreferida }.Concat(FuentesTentativas).ToArray();

            using FontFamily familia = BuscarFamilia(candidatos);
            FontStyle estilo = negrita ? FontStyle.Bold : FontStyle.Regular;
            using Font fuente = new(familia, tamanoPixeles, estilo, GraphicsUnit.Pixel);

            // Primero mido las letras para calcular el tamaño de la textura.
            using Bitmap medidor = new(1, 1);
            using Graphics gMedidor = Graphics.FromImage(medidor);
            gMedidor.PageUnit = GraphicsUnit.Pixel;
            gMedidor.TextRenderingHint = TextRenderingHint.AntiAlias;

            var anchos = new Dictionary<char, float>();
            float anchoMaximo = 0f;

            foreach (char c in Caracteres)
            {
                SizeF tam = gMedidor.MeasureString(c.ToString(), fuente,
                    new PointF(0, 0), StringFormat.GenericDefault);
                float ancho = MathF.Ceiling(tam.Width);
                anchos[c] = ancho;
                anchoMaximo = MathF.Max(anchoMaximo, ancho);
            }

            // Dejo cada letra en una celda para que no se mezclen al dibujarlas.
            float anchoCelda = anchoMaximo + 2f;
            Altura = MathF.Ceiling(fuente.Height) + 2f;

            int anchoAtlas = (int)MathF.Ceiling(anchoCelda * Caracteres.Length);
            int altoAtlas = (int)MathF.Ceiling(Altura);

            // Dibujo cada letra en blanco y con transparencia.
            using Bitmap bmp = new(anchoAtlas, altoAtlas, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            using Graphics g = Graphics.FromImage(bmp);
            g.Clear(Color.Transparent);   // dejo el fondo transparente
            g.PageUnit = GraphicsUnit.Pixel;
            g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;

            for (int i = 0; i < Caracteres.Length; i++)
            {
                char c = Caracteres[i];
                float x = 1f + i * anchoCelda;   // margen izquierdo de cada letra
                g.DrawString(c.ToString(), fuente, Brushes.White, x, 1f);

                // Guardo las coordenadas UV y el avance de la letra.
                Glifos[c] = new Glifo
                {
                    U0 = (i * anchoCelda) / anchoAtlas,
                    U1 = ((i + 1) * anchoCelda) / anchoAtlas,
                    V0 = 0f,
                    V1 = 1f,
                    AnchoCelda = anchoCelda,
                    Ancho = anchos[c] + 1f
                };
            }

            // Subo el bitmap como textura para OpenGL.
            Textura = GL.GenTexture();
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, Textura);

            BitmapData datos = bmp.LockBits(new Rectangle(0, 0, anchoAtlas, altoAtlas),
                ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba8,
                anchoAtlas, altoAtlas, 0, OpenTK.Graphics.OpenGL4.PixelFormat.Bgra, PixelType.UnsignedByte, datos.Scan0);

            bmp.UnlockBits(datos);

            // Uso filtro lineal para que las letras se vean mas suaves.
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter,
                (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter,
                (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS,
                (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT,
                (int)TextureWrapMode.ClampToEdge);
        }

        /// <summary>
        /// Busca una fuente instalada y si no encuentra, usa una generica.
        /// </summary>
        private static FontFamily BuscarFamilia(IEnumerable<string> nombres)
        {
            foreach (string nombre in nombres)
            {
                try
                {
                    return new FontFamily(nombre);
                }
                catch (ArgumentException)
                {
                    // Si no esta esa fuente, pruebo con otra.
                }
            }
            return FontFamily.GenericSansSerif;
        }

        public void Dispose()
        {
            GL.DeleteTexture(Textura);
        }
    }
}
