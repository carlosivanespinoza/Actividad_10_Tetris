using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using Tetris2D.Graficos;
using Tetris2D.UI;

namespace Tetris2D.Pantallas
{
    /// <summary>
    /// Esta es la pantalla de bienvenida donde el jugador escribe su nombre
    /// antes de empezar la partida.
    /// </summary>
    public class PantallaBienvenida : Pantalla
    {
        private readonly Boton _botonIniciar;
        private readonly CuadroTexto _cuadroNombre;
        private bool _iniciado;

        // Guardo las estrellas con posicion relativa para que se acomoden a la ventana.
        private readonly Estrella[] _estrellas;

        private readonly struct Estrella
        {
            public readonly float Nx, Ny, Tam, Brillo;
            public Estrella(float nx, float ny, float tam, float brillo)
            {
                Nx = nx; Ny = ny; Tam = tam; Brillo = brillo;
            }
        }

        public PantallaBienvenida(GestorShader shaders, DibujadorCuadros cuadros, RenderizadorTexto texto)
            : base(shaders, cuadros, texto)
        {
            _botonIniciar = new Boton(cuadros, texto);
            _botonIniciar.Texto = "INICIAR JUEGO";
            _botonIniciar.AlPresionar += IniciarJuego;

            _cuadroNombre = new CuadroTexto(cuadros, texto);

            // Uso la misma semilla para que las estrellas salgan igual.
            Random rnd = new(2026);
            _estrellas = new Estrella[60];
            for (int i = 0; i < _estrellas.Length; i++)
            {
                _estrellas[i] = new Estrella(
                    (float)rnd.NextDouble(),
                    (float)rnd.NextDouble() * 0.85f,
                    0.5f + (float)rnd.NextDouble() * 2.0f,
                    0.12f + (float)rnd.NextDouble() * 0.6f);
            }
        }

        public override void Cargar()
        {
            base.Cargar();
            _cuadroNombre.Limpiar();     // inicio con el nombre vacio
        }

        public override void Actualizar(float dt, Vector2 raton)
        {
            base.Actualizar(dt, raton);
            _botonIniciar.Actualizar(raton);
            _cuadroNombre.Actualizar(dt);

            // El boton se activa cuando ya se escribio un nombre.
            _botonIniciar.Habilitado = !_iniciado && _cuadroNombre.Nombre.Length > 0;
        }

        public override void AlClick(Vector2 posicion, MouseButton boton)
        {
            base.AlClick(posicion, boton);
            if (boton == MouseButton.Left)
                _botonIniciar.AlClick(posicion);
        }

        public override void AlTecla(Keys tecla)
        {
            base.AlTecla(tecla);
            if (!_iniciado)
            {
                _cuadroNombre.AlTecla(tecla);
                if (tecla == Keys.Enter && _cuadroNombre.Nombre.Length > 0)
                    IniciarJuego();
            }
        }

        public override void AlTexto(string caracter)
        {
            base.AlTexto(caracter);
            if (!_iniciado)
                _cuadroNombre.AlTexto(caracter);
        }

        /// <summary>Cuando el jugador confirma, aviso que debe iniciar el juego.</summary>
        private void IniciarJuego()
        {
            if (_iniciado)
                return;
            _iniciado = true;
            // Aqui aviso que ya se puede abrir la pantalla del juego.
            SolicitarJuego(_cuadroNombre.Nombre);
        }

        public override void Renderizar(float ancho, float alto)
        {
            // Fondo y estrellas decorativas.
            Cuadros.DibujarRectangulo(TemaArcade.Fondo, 0, 0, ancho, alto);
            foreach (Estrella e in _estrellas)
            {
                float tam = e.Tam * alto * 0.004f;
                Vector4 blanco = new(TemaArcade.Blanco.X, TemaArcade.Blanco.Y,
                    TemaArcade.Blanco.Z, e.Brillo);
                Cuadros.DibujarRectangulo(blanco, e.Nx * ancho, e.Ny * alto,
                    e.Nx * ancho + tam, e.Ny * alto + tam);
            }

            float cx = ancho * 0.5f;
            string titulo = "TETRIS2D";

            // Titulo con brillo tipo neon.
            // Para el brillo dibujo el titulo varias veces con transparencia.
            float tituloAlto = alto * 0.10f;
            float tituloY = alto * 0.16f;
            float resplandor = alto * 0.008f;

            Vector4 cianSuave = new(TemaArcade.Cian.X, TemaArcade.Cian.Y,
                TemaArcade.Cian.Z, 0.16f);
            Vector2[] direcciones =
            {
                new(-resplandor, 0), new(resplandor, 0),
                new(0, -resplandor), new(0, resplandor),
                new(-resplandor, -resplandor), new(resplandor, resplandor)
            };

            foreach (Vector2 d in direcciones)
            {
                float anchoTitulo = Texto.MedirTexto(titulo, tituloAlto);
                float x = cx - anchoTitulo * 0.5f + d.X;
                Texto.DibujarTexto(titulo, x, tituloY + d.Y, tituloAlto, cianSuave);
            }

            float anchoTotal = Texto.MedirTexto(titulo, tituloAlto);
            Texto.DibujarTexto(titulo, cx - anchoTotal * 0.5f, tituloY, tituloAlto, TemaArcade.Cian);

            // Subtitulo de la pantalla.
            string subtitulo = "— GRAFICACIÓN POR COMPUTADORA —";
            float subAlto = alto * 0.028f;
            float subAncho = Texto.MedirTexto(subtitulo, subAlto);
            Texto.DibujarTexto(subtitulo, cx - subAncho * 0.5f, alto * 0.30f, subAlto, TemaArcade.TextoSuave);

            // Etiqueta y caja para capturar el nombre.
            string etiqueta = "INGRESA TU NOMBRE:";
            float etqAlto = alto * 0.025f;
            float etqAncho = Texto.MedirTexto(etiqueta, etqAlto);
            Texto.DibujarTexto(etiqueta, cx - etqAncho * 0.5f, alto * 0.42f, etqAlto, TemaArcade.Magenta);

            float cajaAncho = ancho * 0.44f;
            float cajaAlto = alto * 0.075f;
            float cajaY = alto * 0.48f;
            _cuadroNombre.Renderizar(cx - cajaAncho * 0.5f, cajaY, cajaAncho, cajaAlto);

            // Boton para iniciar el juego.
            float botonAncho = ancho * 0.36f;
            float botonAlto = alto * 0.085f;
            _botonIniciar.X0 = cx - botonAncho * 0.5f;
            _botonIniciar.Y0 = cajaY + cajaAlto + alto * 0.05f;
            _botonIniciar.X1 = _botonIniciar.X0 + botonAncho;
            _botonIniciar.Y1 = _botonIniciar.Y0 + botonAlto;
            _botonIniciar.Renderizar(alto * 0.042f);

            // Texto pequeno de la parte inferior.
            string pie = "TETRIS2D v1.0 — actividad completada";
            float pieAlto = alto * 0.018f;
            float pieAncho = Texto.MedirTexto(pie, pieAlto);
            Texto.DibujarTexto(pie, cx - pieAncho * 0.5f, alto - pieAlto * 2.2f, pieAlto, TemaArcade.TextoSuave);

            // Confirmacion despues de iniciar.
            if (_iniciado)
                RenderizarConfirmacion(ancho, alto);
        }

        /// <summary>
        /// Dibuja el mensaje de confirmacion cuando el nombre ya fue recibido.
        /// </summary>
        private void RenderizarConfirmacion(float ancho, float alto)
        {
            // Oscurezco el fondo para que el mensaje resalte.
            Cuadros.DibujarRectangulo(new Vector4(0f, 0f, 0f, 0.55f), 0, 0, ancho, alto);

            float cx = ancho * 0.5f;
            float panelAncho = ancho * 0.74f;
            float panelAlto = alto * 0.30f;
            float panelX = cx - panelAncho * 0.5f;
            float panelY = alto * 0.34f;

            Cuadros.DibujarRectangulo(TemaArcade.PanelOscuro, panelX, panelY, panelX + panelAncho, panelY + panelAlto);
            Cuadros.DibujarBordeRectangulo(TemaArcade.Cian, panelX, panelY, panelX + panelAncho, panelY + panelAlto, MathF.Max(1f, alto * 0.006f));

            string mensaje = $"¡BUENA SUERTE, {_cuadroNombre.Nombre}!";
            float mAlto = alto * 0.055f;
            float mAncho = Texto.MedirTexto(mensaje, mAlto);
            Texto.DibujarTexto(mensaje, cx - mAncho * 0.5f, panelY + panelAlto * 0.24f, mAlto, TemaArcade.Amarillo);

            string nota = "El tablero y las piezas ya estan listos para jugar.";
            float nAlto = alto * 0.03f;
            float nAncho = Texto.MedirTexto(nota, nAlto);
            Texto.DibujarTexto(nota, cx - nAncho * 0.5f, panelY + panelAlto * 0.52f, nAlto, TemaArcade.TextoSuave);

            string cerrar = "Cierra la ventana para salir.";
            float cAlto = alto * 0.022f;
            float cAncho = Texto.MedirTexto(cerrar, cAlto);
            Texto.DibujarTexto(cerrar, cx - cAncho * 0.5f, panelY + panelAlto * 0.74f, cAlto, TemaArcade.TextoSuave);
        }
    }
}
