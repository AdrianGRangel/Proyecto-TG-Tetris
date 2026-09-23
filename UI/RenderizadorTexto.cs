using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using Tetris2D.Graficos;

namespace Tetris2D.UI
{
    /// <summary>
    /// Dibuja texto en la ventana usando el atlas generado por GeneradorFuenteAtlas.
    ///
    /// Cada letra es un pequeño rectangulo (quad) con la coordenada UV apuntando
    /// a la celda de esa letra dentro de la textura. El fragment shader usa el
    /// canal alpha de la textura para recortar la forma de la letra.
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

            // Vertice = posicion(vec2) + coordenada de textura(vec2) = 4 floats.
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
        /// Ancho en pixeles que ocuparia el texto a la altura indicada.
        /// Sirve para centrar las frases en la pantalla.
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
        /// Dibuja una linea de texto. (x, y) es la esquina superior izquierda,
        /// altoPixeles es la altura a la que se dibuja la letra mas alta.
        /// </summary>
        public void DibujarTexto(string texto, float x, float y, float altoPixeles, Vector4 color)
        {
            if (string.IsNullOrEmpty(texto))
                return;

            float escala = altoPixeles / _atlas.Altura;
            float altoLetra = _atlas.Altura * escala;

            // Hasta 6 vertices (2 triangulos) por caracter con 4 floats cada uno.
            var vertices = new List<float>(texto.Length * 24);
            float cursorX = x;

            foreach (char c in texto)
            {
                if (!_atlas.Glifos.TryGetValue(c, out Glifo? g))
                    continue;   // caracter no disponible en el atlas: lo saltamos

                float anchoLetra = g.AnchoCelda * escala;
                float u0 = g.U0, v0 = g.V0, u1 = g.U1, v1 = g.V1;

                vertices.AddRange(new float[]
                {
                    cursorX, y,         u0, v0,          // esquina superior izquierda
                    cursorX + anchoLetra, y, u1, v0,     // superior derecha
                    cursorX + anchoLetra, y + altoLetra, u1, v1,  // inferior derecha

                    cursorX, y,         u0, v0,          // repite el primer triangulo
                    cursorX + anchoLetra, y + altoLetra, u1, v1,
                    cursorX, y + altoLetra, u0, v1       // inferior izquierda
                });

                cursorX += g.Ancho * escala;   // avanzamos el cursor una letra
            }

            if (vertices.Count == 0)
                return;

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, _atlas.Textura);

            GL.BindVertexArray(_vao);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Count * sizeof(float), vertices.ToArray(), BufferUsageHint.StreamDraw);

            GL.UseProgram(_shaders.ProgramaTexto);
            GL.Uniform1(_shaders.LTextoTextura, 0);     // sampler = unidad 0
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