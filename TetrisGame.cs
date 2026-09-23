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
    /// Ventana principal del juego (hereda de GameWindow de OpenTK).
    ///
    /// Responsabilidades:
    ///  - Crear las herramientas de dibujo (shaders, dibujador y texto) una vez.
    ///  - Mantener la camara ortografica en pixeles del framebuffer.
    ///  - Repartir los eventos (mouse, teclado y texto) hacia la pantalla activa.
    ///  - Al cambiar de pantalla, solo se sustituye _pantalla.
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

            // Transparencias (los bordes suavizados del texto necesitan blending).
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            // Se crean las herramientas de dibujo una sola vez y se comparten
            // entre todas las pantallas.
            _shaders = new GestorShader();
            _cuadros = new DibujadorCuadros(_shaders);
            _texto = new RenderizadorTexto(_shaders, new GeneradorFuenteAtlas("Arial Black", 48f, true));

            // Pantalla inicial: la bienvenida arcade.
            _pantalla = new PantallaBienvenida(_shaders, _cuadros, _texto);
            _pantalla.Cargar();

            // FASE 2: aqui se conecta el cambio hacia el tablero. Ejemplo:
            // _pantalla.JugarSolicitado += nombre => CambiarPantalla(new PantallaJuego(...));
        }

        /// <summary>
        /// Cada vez que cambia el tamaño de la ventana ajustamos el viewport de
        /// OpenGL para que coincida con el framebuffer real (importante en
        /// pantallas con alta densidad de pixeles / HiDPI).
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

            // Camara ortografica: 1 unidad = 1 pixel del framebuffer, con el eje
            // Y hacia abajo (así coincide con las coordenadas del mouse). Ambas
            // pantallas (solidos y texto) usan esta misma proyeccion.
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
        /// Convierte la posicion del mouse (relativa al contenido de la ventana)
        /// al espacio en pixeles del framebuffer. Con HiDPI ambos espacios
        /// pueden diferir, asi que se aplica la proporcion framebuffer/ventana.
        /// </summary>
        private Vector2 PuntoRaton()
        {
            float sx = Size.X > 0 ? FramebufferSize.X / (float)Size.X : 1f;
            float sy = Size.Y > 0 ? FramebufferSize.Y / (float)Size.Y : 1f;
            return MousePosition * new Vector2(sx, sy);
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