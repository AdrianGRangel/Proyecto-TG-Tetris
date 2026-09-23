using OpenTK.Mathematics;
using Tetris2D.Graficos;

namespace Tetris2D.UI
{
    /// <summary>
    /// Boton de la interfaz arcade: un rectangulo que reacciona al mouse.
    ///  - Habilitado: se pinta con efecto "hover" cuando el mouse esta encima.
    ///  - Deshabilitado: se muestra gris y no responde a los clics.
    /// </summary>
    public class Boton
    {
        /// <summary>Se dispara cuando el usuario hace clic sobre el boton.</summary>
        public event Action? AlPresionar;

        public float X0, Y0, X1, Y1;   // rectangulo del boton en pixeles
        public string Texto = "";
        public bool Habilitado = true;

        private bool _sobre;   // el mouse esta encima del boton en este frame

        public Boton(DibujadorCuadros cuadros, RenderizadorTexto texto)
        {
            _cuadros = cuadros;
            _texto = texto;
        }

        private readonly DibujadorCuadros _cuadros;
        private readonly RenderizadorTexto _texto;

        /// <summary>Actualiza el estado "hover" segun la posicion actual del mouse.</summary>
        public void Actualizar(Vector2 raton)
        {
            _sobre = Habilitado && Contiene(raton);
        }

        public void AlClick(Vector2 raton)
        {
            if (Habilitado && Contiene(raton))
                AlPresionar?.Invoke();
        }

        /// <summary>True si el punto (en pixeles) esta dentro del boton.</summary>
        public bool Contiene(Vector2 punto)
        {
            return punto.X >= X0 && punto.X <= X1 && punto.Y >= Y0 && punto.Y <= Y1;
        }

        /// <summary>
        /// Dibuja el boton con su color segun el estado (deshabilitado / normal /
        /// hover), un borde blanco y el texto centrado.
        /// </summary>
        public void Renderizar(float altoTexto)
        {
            Vector4 relleno = !Habilitado ? TemaArcade.Gris
                             : _sobre ? TemaArcade.Magenta
                             : TemaArcade.Cian;

            _cuadros.DibujarRectangulo(relleno, X0, Y0, X1, Y1);
            float grosorBorde = MathF.Max(1f, (Y1 - Y0) * 0.035f);
            _cuadros.DibujarBordeRectangulo(TemaArcade.Blanco, X0, Y0, X1, Y1, grosorBorde);

            // Texto centrado horizontal y verticalmente.
            float anchoTexto = _texto.MedirTexto(Texto, altoTexto);
            float centrarX = (X0 + X1 - anchoTexto) * 0.5f;
            float centrarY = (Y0 + Y1 - altoTexto) * 0.5f;
            _texto.DibujarTexto(Texto, centrarX, centrarY, altoTexto, TemaArcade.Blanco);
        }
    }
}