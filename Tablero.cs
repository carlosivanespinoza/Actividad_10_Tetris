using OpenTK.Mathematics;
using Tetris2D.Graficos;
using Tetris2D.UI;

namespace Tetris2D
{
    public class Tablero
    {
        public const int Columnas = 10;
        public const int Filas = 18;

        private readonly int[,] _celdas = new int[Columnas, Filas];

        public bool HayColision(Pieza pieza, int posX, int posY)
        {
            foreach ((int bx, int by) in pieza.Bloques)
            {
                int x = posX + bx;
                int y = posY + by;

                if (x < 0 || x >= Columnas || y >= Filas)
                    return true;

                if (y >= 0 && _celdas[x, y] != 0)
                    return true;
            }

            return false;
        }

        public void Fijar(Pieza pieza, int posX, int posY)
        {
            foreach ((int bx, int by) in pieza.Bloques)
            {
                int x = posX + bx;
                int y = posY + by;
                if (x >= 0 && x < Columnas && y >= 0 && y < Filas)
                    _celdas[x, y] = (int)pieza.Tipo + 1;
            }
        }

        public int LimpiarLineas()
        {
            int lineas = 0;
            for (int y = Filas - 1; y >= 0; y--)
            {
                if (!FilaCompleta(y))
                    continue;

                EliminarFila(y);
                lineas++;
                y++;
            }

            return lineas;
        }

        public void Reiniciar()
        {
            Array.Clear(_celdas, 0, _celdas.Length);
        }

        public void Renderizar(DibujadorCuadros cuadros, float x0, float y0, float tamCelda)
        {
            float ancho = Columnas * tamCelda;
            float alto = Filas * tamCelda;

            cuadros.DibujarRectangulo(TemaArcade.PanelOscuro, x0, y0, x0 + ancho, y0 + alto);
            float grosorBorde = MathF.Max(2f, tamCelda * 0.10f);
            cuadros.DibujarBordeRectangulo(TemaArcade.Cian, x0, y0, x0 + ancho, y0 + alto, grosorBorde);

            Vector4 colorCuadricula = new(TemaArcade.TextoSuave.X, TemaArcade.TextoSuave.Y,
                TemaArcade.TextoSuave.Z, 0.10f);

            for (int x = 1; x < Columnas; x++)
            {
                float lx = x0 + x * tamCelda;
                cuadros.DibujarLinea(colorCuadricula, lx, y0, lx, y0 + alto, 1f);
            }

            for (int y = 1; y < Filas; y++)
            {
                float ly = y0 + y * tamCelda;
                cuadros.DibujarLinea(colorCuadricula, x0, ly, x0 + ancho, ly, 1f);
            }

            Vector4 bordeOscuro = new(0f, 0f, 0f, 0.35f);
            float grosorBisel = MathF.Max(1f, tamCelda * 0.06f);

            for (int x = 0; x < Columnas; x++)
            {
                for (int y = 0; y < Filas; y++)
                {
                    int valor = _celdas[x, y];
                    if (valor == 0)
                        continue;

                    Vector4 color = PiezaColor(valor);
                    float px = x0 + x * tamCelda;
                    float py = y0 + y * tamCelda;

                    cuadros.DibujarRectangulo(color, px, py, px + tamCelda, py + tamCelda);
                    cuadros.DibujarBordeRectangulo(bordeOscuro, px, py, px + tamCelda, py + tamCelda, grosorBisel);
                }
            }
        }

        private bool FilaCompleta(int fila)
        {
            for (int x = 0; x < Columnas; x++)
            {
                if (_celdas[x, fila] == 0)
                    return false;
            }

            return true;
        }

        private void EliminarFila(int fila)
        {
            for (int y = fila; y > 0; y--)
            {
                for (int x = 0; x < Columnas; x++)
                    _celdas[x, y] = _celdas[x, y - 1];
            }

            for (int x = 0; x < Columnas; x++)
                _celdas[x, 0] = 0;
        }

        private static Vector4 PiezaColor(int valorCelda)
        {
            TipoPieza tipo = (TipoPieza)(valorCelda - 1);
            return new Pieza(tipo).Color;
        }
    }
}
