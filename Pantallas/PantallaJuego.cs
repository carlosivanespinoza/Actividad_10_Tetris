using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using Tetris2D.Audio;
using Tetris2D.Graficos;
using Tetris2D.UI;

namespace Tetris2D.Pantallas
{
    public class PantallaJuego : Pantalla
    {
        private readonly string _nombreJugador;
        private readonly Tablero _tablero = new();

        private Pieza _piezaActual = null!;
        private Pieza _piezaSiguiente = null!;
        private int _posX;
        private int _posY;
        private int _puntaje;
        private int _lineas;
        private int _nivel = 1;
        private bool _gameOver;
        private bool _pausa;
        private float _acumuladorCaida;

        private float _tableroX;
        private float _tableroY;
        private float _tamCelda;
        private float _panelX;
        private float _panelY;
        private float _panelW;
        private float _panelH;

        public PantallaJuego(GestorShader shaders, DibujadorCuadros cuadros, RenderizadorTexto texto, string nombreJugador)
            : base(shaders, cuadros, texto)
        {
            _nombreJugador = string.IsNullOrWhiteSpace(nombreJugador) ? "JUGADOR" : nombreJugador.Trim();
        }

        public override void Cargar()
        {
            base.Cargar();
            Reiniciar();
        }

        public override void Actualizar(float dt, Vector2 raton)
        {
            base.Actualizar(dt, raton);
            if (_gameOver || _pausa)
                return;

            _acumuladorCaida += dt;
            if (_acumuladorCaida >= TiempoCaida())
            {
                _acumuladorCaida = 0f;
                if (!MoverPieza(0, 1))
                    FijarYGenerar();
            }
        }

        public override void AlTecla(Keys tecla)
        {
            base.AlTecla(tecla);

            if (_gameOver)
            {
                if (tecla == Keys.Enter)
                    Reiniciar();
                return;
            }

            if (tecla == Keys.Escape)
            {
                _pausa = !_pausa;
                return;
            }

            if (_pausa)
                return;

            switch (tecla)
            {
                case Keys.Left:
                    if (MoverPieza(-1, 0))
                        Sonido.Reproducir(TonoJuego.Mover);
                    break;

                case Keys.Right:
                    if (MoverPieza(1, 0))
                        Sonido.Reproducir(TonoJuego.Mover);
                    break;

                case Keys.Down:
                    if (MoverPieza(0, 1))
                    {
                        _puntaje += 1;
                        Sonido.Reproducir(TonoJuego.Mover);
                    }
                    else
                    {
                        FijarYGenerar();
                    }
                    break;

                case Keys.Up:
                    HardDrop();
                    break;

                case Keys.Space:
                    RotarConEsquinas();
                    break;
            }
        }

        public override void Renderizar(float ancho, float alto)
        {
            Cuadros.DibujarRectangulo(TemaArcade.Fondo, 0, 0, ancho, alto);
            CalcularDisposicion(ancho, alto);
            RenderizarEncabezado(ancho, alto);
            _tablero.Renderizar(Cuadros, _tableroX, _tableroY, _tamCelda);
            RenderizarPiezaViva();
            RenderizarPanelLateral(alto);

            if (_pausa)
                RenderizarOverlayPausa(ancho, alto);

            if (_gameOver)
                RenderizarGameOver(ancho, alto);
        }

        private void Reiniciar()
        {
            _tablero.Reiniciar();
            _puntaje = 0;
            _lineas = 0;
            _nivel = 1;
            _gameOver = false;
            _pausa = false;
            _acumuladorCaida = 0f;
            _piezaSiguiente = Pieza.Aleatoria();
            GenerarPieza();
        }

        private void GenerarPieza()
        {
            _piezaActual = _piezaSiguiente;
            _piezaSiguiente = Pieza.Aleatoria();
            _posX = (Tablero.Columnas - 4) / 2;
            _posY = 0;

            if (_tablero.HayColision(_piezaActual, _posX, _posY))
            {
                _gameOver = true;
                Sonido.Reproducir(TonoJuego.GameOver);
            }
        }

        private bool MoverPieza(int dx, int dy)
        {
            int nx = _posX + dx;
            int ny = _posY + dy;
            if (_tablero.HayColision(_piezaActual, nx, ny))
                return false;

            _posX = nx;
            _posY = ny;
            return true;
        }

        private void FijarYGenerar()
        {
            _tablero.Fijar(_piezaActual, _posX, _posY);
            Sonido.Reproducir(TonoJuego.Fijar);

            int eliminadas = _tablero.LimpiarLineas();
            if (eliminadas > 0)
            {
                _lineas += eliminadas;
                _puntaje += PuntosPorLineas(eliminadas);
                Sonido.Reproducir(TonoJuego.Linea);

                int nuevoNivel = 1 + _lineas / 10;
                if (nuevoNivel > _nivel)
                {
                    _nivel = nuevoNivel;
                    Sonido.Reproducir(TonoJuego.Nivel);
                }
            }

            GenerarPieza();
        }

        private void RotarConEsquinas()
        {
            List<(int X, int Y)> original = _piezaActual.Bloques.ToList();
            _piezaActual.Rotar();

            int[] desplazamientos = { 0, -1, 1, -2, 2 };
            foreach (int dx in desplazamientos)
            {
                if (!_tablero.HayColision(_piezaActual, _posX + dx, _posY))
                {
                    _posX += dx;
                    Sonido.Reproducir(TonoJuego.Rotar);
                    return;
                }
            }

            _piezaActual.EstablecerBloques(original);
        }

        private void HardDrop()
        {
            int distancia = 0;
            while (MoverPieza(0, 1))
                distancia++;

            _puntaje += distancia * 2;
            Sonido.Reproducir(TonoJuego.Mover);
            FijarYGenerar();
        }

        private float TiempoCaida()
        {
            return MathF.Max(0.12f, 0.80f - (_nivel - 1) * 0.07f);
        }

        private static int PuntosPorLineas(int lineas)
        {
            return lineas switch
            {
                1 => 100,
                2 => 300,
                3 => 500,
                >= 4 => 800,
                _ => 0
            };
        }

        private void CalcularDisposicion(float ancho, float alto)
        {
            float margen = alto * 0.05f;
            float altoEncabezado = alto * 0.14f;
            float altoUtil = alto - altoEncabezado - margen;
            _tamCelda = MathF.Floor(altoUtil / Tablero.Filas);

            float anchoTablero = Tablero.Columnas * _tamCelda;
            float altoTablero = Tablero.Filas * _tamCelda;
            float separacion = _tamCelda * 1.6f;
            float anchoPanel = MathF.Max(140f,
                MathF.Min(ancho * 0.32f, ancho - anchoTablero - separacion - margen * 2f));

            float zonaTotal = anchoTablero + separacion + anchoPanel;
            _tableroX = MathF.Max(margen, (ancho - zonaTotal) * 0.5f);
            _tableroY = altoEncabezado + MathF.Max(0f, (altoUtil - altoTablero) * 0.5f);
            _panelX = _tableroX + anchoTablero + separacion;
            _panelY = _tableroY;
            _panelW = anchoPanel;
            _panelH = altoTablero;
        }

        private void RenderizarPiezaViva()
        {
            if (_gameOver)
                return;

            Vector4 bordeOscuro = new(0f, 0f, 0f, 0.35f);
            float grosorBisel = MathF.Max(1f, _tamCelda * 0.06f);

            foreach ((int bx, int by) in _piezaActual.Bloques)
            {
                int gy = _posY + by;
                if (gy < 0)
                    continue;

                float px = _tableroX + (_posX + bx) * _tamCelda;
                float py = _tableroY + gy * _tamCelda;
                Cuadros.DibujarRectangulo(_piezaActual.Color, px, py, px + _tamCelda, py + _tamCelda);
                Cuadros.DibujarBordeRectangulo(bordeOscuro, px, py, px + _tamCelda, py + _tamCelda, grosorBisel);
            }
        }

        private void RenderizarPanelLateral(float alto)
        {
            if (_panelW < 120f || _panelH < 120f)
                return;

            Cuadros.DibujarRectangulo(TemaArcade.PanelOscuro, _panelX, _panelY, _panelX + _panelW, _panelY + _panelH);
            float grosorBorde = MathF.Max(1f, _tamCelda * 0.06f);
            Cuadros.DibujarBordeRectangulo(TemaArcade.Cian, _panelX, _panelY, _panelX + _panelW, _panelY + _panelH, grosorBorde);

            float cx = _panelX + _panelW * 0.5f;
            float y = _panelY + _tamCelda * 0.6f;
            float avance = _tamCelda * 2.1f;

            TituloPanel("JUGADOR", cx, y);
            TextoCentrado(_nombreJugador.ToUpperInvariant(), cx, y + avance * 0.32f, _tamCelda * 0.75f, TemaArcade.Amarillo);

            float yPuntos = y + avance * 0.92f;
            TituloPanel("PUNTAJE", cx, yPuntos);
            TextoCentrado(_puntaje.ToString(), cx, yPuntos + avance * 0.30f, _tamCelda * 1.25f, TemaArcade.Amarillo);

            float yLineas = yPuntos + avance * 0.86f;
            TituloPanel("LINEAS", cx, yLineas);
            TextoCentrado(_lineas.ToString(), cx, yLineas + avance * 0.30f, _tamCelda * 1.25f, TemaArcade.Verde);

            float yNivel = yLineas + avance * 0.86f;
            TituloPanel("NIVEL", cx, yNivel);
            TextoCentrado(_nivel.ToString(), cx, yNivel + avance * 0.28f, _tamCelda * 1.1f, TemaArcade.Rojo);
            TextoCentrado($"{_lineas % 10}/10 PARA SUBIR", cx, yNivel + avance * 0.82f, _tamCelda * 0.48f, TemaArcade.TextoSuave);

            float ySig = yNivel + avance * 1.32f;
            TituloPanel("SIGUIENTE", cx, ySig);
            float altoCaja = _tamCelda * 3.0f;
            float anchoCaja = MathF.Min(_panelW * 0.86f, _tamCelda * 4.2f);
            float cajaX = _panelX + (_panelW - anchoCaja) * 0.5f;
            float cajaY = ySig + avance * 0.30f;
            Cuadros.DibujarRectangulo(TemaArcade.PanelOscuro, cajaX, cajaY, cajaX + anchoCaja, cajaY + altoCaja);
            Cuadros.DibujarBordeRectangulo(TemaArcade.Cian, cajaX, cajaY, cajaX + anchoCaja, cajaY + altoCaja, grosorBorde);
            RenderizarPreview(_piezaSiguiente, cajaX, cajaY, anchoCaja, altoCaja);

            float altoCtrl = MathF.Max(9f, _tamCelda * 0.46f);
            float yCtrl = _panelY + _panelH - _tamCelda * 2.7f;
            TextoCentrado("FLECHAS MOVER", cx, yCtrl, altoCtrl, TemaArcade.TextoSuave);
            TextoCentrado("ESPACIO ROTAR", cx, yCtrl + _tamCelda * 0.65f, altoCtrl, TemaArcade.TextoSuave);
            TextoCentrado("ESC PAUSAR", cx, yCtrl + _tamCelda * 1.30f, altoCtrl, TemaArcade.TextoSuave);
        }

        private void RenderizarPreview(Pieza pieza, float cajaX, float cajaY, float anchoCaja, float altoCaja)
        {
            int minX = pieza.Bloques.Min(b => b.X);
            int maxX = pieza.Bloques.Max(b => b.X);
            int minY = pieza.Bloques.Min(b => b.Y);
            int maxY = pieza.Bloques.Max(b => b.Y);

            float tamMini = MathF.Min(anchoCaja, altoCaja) * 0.30f;
            float anchoPieza = (maxX - minX + 1) * tamMini;
            float altoPieza = (maxY - minY + 1) * tamMini;
            float px0 = cajaX + (anchoCaja - anchoPieza) * 0.5f;
            float py0 = cajaY + (altoCaja - altoPieza) * 0.5f;

            Vector4 bordeOscuro = new(0f, 0f, 0f, 0.35f);
            float grosorBisel = MathF.Max(1f, tamMini * 0.08f);

            foreach ((int bx, int by) in pieza.Bloques)
            {
                float px = px0 + (bx - minX) * tamMini;
                float py = py0 + (by - minY) * tamMini;
                Cuadros.DibujarRectangulo(pieza.Color, px, py, px + tamMini, py + tamMini);
                Cuadros.DibujarBordeRectangulo(bordeOscuro, px, py, px + tamMini, py + tamMini, grosorBisel);
            }
        }

        private void RenderizarOverlayPausa(float ancho, float alto)
        {
            Cuadros.DibujarRectangulo(new Vector4(0f, 0f, 0f, 0.60f), 0, 0, ancho, alto);
            float panelAncho = ancho * 0.70f;
            float panelAlto = alto * 0.32f;
            float panelX = (ancho - panelAncho) * 0.5f;
            float panelY = alto * 0.34f;

            Cuadros.DibujarRectangulo(TemaArcade.PanelOscuro, panelX, panelY, panelX + panelAncho, panelY + panelAlto);
            Cuadros.DibujarBordeRectangulo(TemaArcade.Amarillo, panelX, panelY, panelX + panelAncho, panelY + panelAlto, MathF.Max(2f, alto * 0.008f));
            TextoCentrado("P A U S A", ancho * 0.5f, panelY + panelAlto * 0.18f, alto * 0.07f, TemaArcade.Amarillo);
            TextoCentrado("PRESIONA ESC PARA CONTINUAR", ancho * 0.5f, panelY + panelAlto * 0.60f, alto * 0.03f, TemaArcade.TextoSuave);
        }

        private void RenderizarGameOver(float ancho, float alto)
        {
            Cuadros.DibujarRectangulo(new Vector4(0f, 0f, 0f, 0.68f), 0, 0, ancho, alto);
            float panelAncho = ancho * 0.76f;
            float panelAlto = alto * 0.38f;
            float panelX = (ancho - panelAncho) * 0.5f;
            float panelY = alto * 0.31f;

            Cuadros.DibujarRectangulo(TemaArcade.PanelOscuro, panelX, panelY, panelX + panelAncho, panelY + panelAlto);
            Cuadros.DibujarBordeRectangulo(TemaArcade.Rojo, panelX, panelY, panelX + panelAncho, panelY + panelAlto, MathF.Max(2f, alto * 0.008f));
            TextoCentrado("GAME OVER", ancho * 0.5f, panelY + panelAlto * 0.16f, alto * 0.075f, TemaArcade.Rojo);
            TextoCentrado($"PUNTAJE FINAL: {_puntaje}", ancho * 0.5f, panelY + panelAlto * 0.47f, alto * 0.035f, TemaArcade.Amarillo);
            TextoCentrado("ENTER PARA REINICIAR", ancho * 0.5f, panelY + panelAlto * 0.68f, alto * 0.03f, TemaArcade.TextoSuave);
        }

        private void RenderizarEncabezado(float ancho, float alto)
        {
            TextoCentrado("TETRIS - 2D", ancho * 0.5f, alto * 0.025f, alto * 0.045f, TemaArcade.Cian);
        }

        private void TituloPanel(string texto, float cx, float y)
        {
            TextoCentrado(texto, cx, y, _tamCelda * 0.75f, TemaArcade.Magenta);
        }

        private void TextoCentrado(string texto, float cx, float y, float alto, Vector4 color)
        {
            float ancho = Texto.MedirTexto(texto, alto);
            Texto.DibujarTexto(texto, cx - ancho * 0.5f, y, alto, color);
        }
    }
}
