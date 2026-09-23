using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using Tetris2D.Graficos;
using Tetris2D.UI;

namespace Tetris2D.Pantallas
{
    /// <summary>
    /// Pantalla de bienvenida estilo arcade de la Fase 1:
    ///  - Fondo oscuro con estrellas ("cielo" de sala de maquinas).
    ///  - Titulo "TETRIS2D" con resplandor neon.
    ///  - Caja para escribir el nombre del jugador.
    ///  - Boton INICIAR JUEGO (se habilita al escribir el nombre).
    ///
    /// Al presionar INICIAR se muestra un mensaje de confirmacion con el nombre
    /// y se dispara el evento JugarSolicitado (que en la Fase 2 abrira el tablero).
    /// </summary>
    public class PantallaBienvenida : Pantalla
    {
        private readonly Boton _botonIniciar;
        private readonly CuadroTexto _cuadroNombre;
        private bool _iniciado;

        // Estrellas decorativas: posicion en fracciones de pantalla (0..1),
        // tamano y brillo fijos para que no cambien de lugar entre frames.
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

            // Semilla fija: mismas estrellas en cada ejecucion.
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
            _cuadroNombre.Limpiar();     // empezamos con el nombre vacio
        }

        public override void Actualizar(float dt, Vector2 raton)
        {
            base.Actualizar(dt, raton);
            _botonIniciar.Actualizar(raton);
            _cuadroNombre.Actualizar(dt);

            // El boton solo funciona cuando hay nombre escrito.
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

        /// <summary>El jugador confirmo su nombre: mostramos la confirmacion.</summary>
        private void IniciarJuego()
        {
            if (_iniciado)
                return;
            _iniciado = true;
            // Gancho para la Fase 2: aqui TetrisGame cambiara a PantallaJuego.
            SolicitarJuego(_cuadroNombre.Nombre);
        }

        public override void Renderizar(float ancho, float alto)
        {
            // --- Fondo + estrellas decorativas -------------------------------
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

            // --- Titulo con resplandor neon ----------------------------------
            // El "glow" se logra dibujando el mismo texto varias veces, muy
            // translucido, desplazado unos pixeles en cada direccion.
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

            // --- Subtitulo ---------------------------------------------------
            string subtitulo = "— GRAFICACIÓN POR COMPUTADORA —";
            float subAlto = alto * 0.028f;
            float subAncho = Texto.MedirTexto(subtitulo, subAlto);
            Texto.DibujarTexto(subtitulo, cx - subAncho * 0.5f, alto * 0.30f, subAlto, TemaArcade.TextoSuave);

            // --- Etiqueta + caja del nombre ----------------------------------
            string etiqueta = "INGRESA TU NOMBRE:";
            float etqAlto = alto * 0.025f;
            float etqAncho = Texto.MedirTexto(etiqueta, etqAlto);
            Texto.DibujarTexto(etiqueta, cx - etqAncho * 0.5f, alto * 0.42f, etqAlto, TemaArcade.Magenta);

            float cajaAncho = ancho * 0.44f;
            float cajaAlto = alto * 0.075f;
            float cajaY = alto * 0.48f;
            _cuadroNombre.Renderizar(cx - cajaAncho * 0.5f, cajaY, cajaAncho, cajaAlto);

            // --- Boton INICIAR JUEGO -----------------------------------------
            float botonAncho = ancho * 0.36f;
            float botonAlto = alto * 0.085f;
            _botonIniciar.X0 = cx - botonAncho * 0.5f;
            _botonIniciar.Y0 = cajaY + cajaAlto + alto * 0.05f;
            _botonIniciar.X1 = _botonIniciar.X0 + botonAncho;
            _botonIniciar.Y1 = _botonIniciar.Y0 + botonAlto;
            _botonIniciar.Renderizar(alto * 0.042f);

            // --- Pie de pantalla ---------------------------------------------
            string pie = "TETRIS2D v1.0 — base lista para la Fase 2 (tablero)";
            float pieAlto = alto * 0.018f;
            float pieAncho = Texto.MedirTexto(pie, pieAlto);
            Texto.DibujarTexto(pie, cx - pieAncho * 0.5f, alto - pieAlto * 2.2f, pieAlto, TemaArcade.TextoSuave);

            // --- Confirmacion al iniciar -------------------------------------
            if (_iniciado)
                RenderizarConfirmacion(ancho, alto);
        }

        /// <summary>
        /// Mensaje final de la Fase 1: confirma que el nombre se recibio bien.
        /// En la Fase 2, este lugar sera una transicion hacia el tablero.
        /// </summary>
        private void RenderizarConfirmacion(float ancho, float alto)
        {
            // Oscurecemos toda la pantalla detrás del panel.
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

            string nota = "La Fase 2 agregará aquí el tablero y las piezas.";
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