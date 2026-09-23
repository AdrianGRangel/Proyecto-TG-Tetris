using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Text;
using OpenTK.Graphics.OpenGL4;

namespace Tetris2D.UI
{
    /// <summary>
    /// Informacion de un caracter dentro del atlas de texto:
    /// las coordenadas UV (donde vive la letra en la textura) y su ancho de
    /// avance (cuanto se recorre el cursor al escribirla).
    /// </summary>
    public sealed class Glifo
    {
        public float U0, V0, U1, V1;   // region de la letra en la textura (0..1)
        public float AnchoCelda;       // ancho de la celda en pixeles del atlas
        public float Ancho;            // ancho de avance (espaciado proporcional)
    }

    /// <summary>
    /// Genera en tiempo de ejecucion la "textura de letras" del juego.
    ///
    /// Pasos:
    ///  1. Con System.Drawing dibujamos cada caracter de la fuente sobre un bitmap.
    ///  2. Ese bitmap se sube como textura de OpenGL (BGRA).
    ///  3. Guardamos por letra sus coordenadas UV y su ancho para que
    ///     RenderizadorTexto la dibuje como un cuadro texturizado.
    ///
    /// Asi cualquier texto (titulos, nombres, numeros, ñ o acentos) se ve bien
    /// sin instalar tipos de letra externos.
    /// </summary>
    public class GeneradorFuenteAtlas : IDisposable
    {
        // Conjunto de caracteres soportados por el atlas (incluye espanol).
        public const string Caracteres =
            "ABCDEFGHIJKLMNÑOPQRSTUVWXYZ" +
            "abcdefghijklmnñopqrstuvwxyz" +
            "0123456789" +
            "ÁÉÍÓÚÜáéíóúü" +
            " .,:;!?¿¡-_'\"()[]{}<>/\\%&@#$*+=|~^`";

        public int Textura { get; private set; }
        public Dictionary<char, Glifo> Glifos { get; } = new();
        public float Altura { get; private set; }

        /// <summary>Nombres de fuente probados en orden hasta encontrar uno instalado.</summary>
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

            // --- Medimos las letras para deducir el tamano de la textura ---
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

            // Cada letra vive en una celda del mismo tamano (celda uniforme), con
            // 1 pixel de margen a cada lado para que no se mezclen letras vecinas.
            float anchoCelda = anchoMaximo + 2f;
            Altura = MathF.Ceiling(fuente.Height) + 2f;

            int anchoAtlas = (int)MathF.Ceiling(anchoCelda * Caracteres.Length);
            int altoAtlas = (int)MathF.Ceiling(Altura);

            // --- Dibujamos cada letra en su celda (color blanco + alpha) ---
            using Bitmap bmp = new(anchoAtlas, altoAtlas, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            using Graphics g = Graphics.FromImage(bmp);
            g.Clear(Color.Transparent);   // el fondo queda transparente
            g.PageUnit = GraphicsUnit.Pixel;
            g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;

            for (int i = 0; i < Caracteres.Length; i++)
            {
                char c = Caracteres[i];
                float x = 1f + i * anchoCelda;   // margen izquierdo de la celda
                g.DrawString(c.ToString(), fuente, Brushes.White, x, 1f);

                // UV de toda la celda + ancho de avance proporcional.
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

            // --- Subimos el bitmap como textura de OpenGL (BGRA) ---
            Textura = GL.GenTexture();
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, Textura);

            BitmapData datos = bmp.LockBits(new Rectangle(0, 0, anchoAtlas, altoAtlas),
                ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba8,
                anchoAtlas, altoAtlas, 0, OpenTK.Graphics.OpenGL4.PixelFormat.Bgra, PixelType.UnsignedByte, datos.Scan0);

            bmp.UnlockBits(datos);

            // Filtro lineal = letras suaves al escalarlas; repeticion desactivada.
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
        /// Devuelve la primera familia tipografica instalada de la lista.
        /// Si ninguna existe, cae en la fuente generica del sistema.
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
                    // Fuente no instalada: probamos con la siguiente.
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