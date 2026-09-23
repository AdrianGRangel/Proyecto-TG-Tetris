using OpenTK.Graphics.OpenGL4;

namespace Tetris2D.Graficos
{
    /// <summary>
    /// Compila y enlaza los shaders GLSL 330 que usa la interfaz.
    /// Expone dos programas listos para usarse:
    ///  - ProgramaColor: dibuja formas solidas (rectangulos, lineas, tablero).
    ///  - ProgramaTexto: dibuja texto texturizado con transparencia (alpha).
    ///
    /// Este helper es el unico lugar donde se trabaja con shaders, de modo que
    /// el resto del codigo solo pida "dibujar un rectangulo" o "dibujar texto"
    /// sin conocer los detalles de OpenGL.
    /// </summary>
    public class GestorShader : IDisposable
    {
        public int ProgramaColor { get; private set; }
        public int ProgramaTexto { get; private set; }

        // Ubicaciones de los uniforms del programa de COLOR.
        public int LColorProyeccion { get; private set; }
        public int LColorColor { get; private set; }

        // Ubicaciones de los uniforms del programa de TEXTO.
        public int LTextoProyeccion { get; private set; }
        public int LTextoColor { get; private set; }
        public int LTextoTextura { get; private set; }

        public GestorShader()
        {
            // ---------------------------------------------------------------
            // Shader de COLOR: cada vertice solo trae su posicion 2D.
            // La proyeccion ortografica convierte pixeles (x, y) en coordenadas
            // normalizadas de dispositivo (-1 .. 1) para el pipeline de OpenGL.
            // ---------------------------------------------------------------
            string vertexColor = @"#version 330 core
                layout(location = 0) in vec2 aPosicion;
                uniform mat4 proyeccion;
                void main()
                {
                    gl_Position = proyeccion * vec4(aPosicion, 0.0, 1.0);
                }";

            string fragmentColor = @"#version 330 core
                out vec4 colorSalida;
                uniform vec4 color;
                void main()
                {
                    colorSalida = color;
                }";

            ProgramaColor = CompilarEnlazar(vertexColor, fragmentColor);
            LColorProyeccion = GL.GetUniformLocation(ProgramaColor, "proyeccion");
            LColorColor = GL.GetUniformLocation(ProgramaColor, "color");

            // ---------------------------------------------------------------
            // Shader de TEXTO: cada vertice trae posicion 2D + coordenada de
            // textura (UV). El alpha guardado en la textura recorta la letra;
            // el color del texto se multiplica para pintarlo del tono deseado.
            // ---------------------------------------------------------------
            string vertexTexto = @"#version 330 core
                layout(location = 0) in vec2 aPosicion;
                layout(location = 1) in vec2 aCoordenadaTextura;
                out vec2 coordenadaTextura;
                uniform mat4 proyeccion;
                void main()
                {
                    coordenadaTextura = aCoordenadaTextura;
                    gl_Position = proyeccion * vec4(aPosicion, 0.0, 1.0);
                }";

            string fragmentTexto = @"#version 330 core
                in vec2 coordenadaTextura;
                out vec4 colorSalida;
                uniform sampler2D textura;
                uniform vec4 color;
                void main()
                {
                    vec4 texel = texture(textura, coordenadaTextura);
                    colorSalida = vec4(color.rgb * texel.a, color.a * texel.a);
                }";

            ProgramaTexto = CompilarEnlazar(vertexTexto, fragmentTexto);
            LTextoProyeccion = GL.GetUniformLocation(ProgramaTexto, "proyeccion");
            LTextoColor = GL.GetUniformLocation(ProgramaTexto, "color");
            LTextoTextura = GL.GetUniformLocation(ProgramaTexto, "textura");
        }

        /// <summary>
        /// Compila el vertex shader y el fragment shader y los enlaza en un
        /// programa listo para usarse. Lanza excepcion si algo falla, asi los
        /// errores de shader se ven de inmediato al iniciar el programa.
        /// </summary>
        private static int CompilarEnlazar(string vertexSrc, string fragmentSrc)
        {
            int vertex = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(vertex, vertexSrc);
            GL.CompileShader(vertex);
            VerificarCompilacion(vertex, "vertex");

            int fragment = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fragment, fragmentSrc);
            GL.CompileShader(fragment);
            VerificarCompilacion(fragment, "fragment");

            int programa = GL.CreateProgram();
            GL.AttachShader(programa, vertex);
            GL.AttachShader(programa, fragment);
            GL.LinkProgram(programa);
            VerificarEnlazado(programa);

            // Una vez enlazados, los shaders sueltos se pueden borrar.
            GL.DetachShader(programa, vertex);
            GL.DetachShader(programa, fragment);
            GL.DeleteShader(vertex);
            GL.DeleteShader(fragment);

            return programa;
        }

        private static void VerificarCompilacion(int shader, string tipo)
        {
            GL.GetShader(shader, ShaderParameter.CompileStatus, out int estado);
            if (estado == 0)
            {
                string log = GL.GetShaderInfoLog(shader);
                throw new InvalidOperationException($"Error al compilar el shader {tipo}: {log}");
            }
        }

        private static void VerificarEnlazado(int programa)
        {
            GL.GetProgram(programa, GetProgramParameterName.LinkStatus, out int estado);
            if (estado == 0)
            {
                string log = GL.GetProgramInfoLog(programa);
                throw new InvalidOperationException($"Error al enlazar el programa de shaders: {log}");
            }
        }

        public void Dispose()
        {
            GL.DeleteProgram(ProgramaColor);
            GL.DeleteProgram(ProgramaTexto);
        }
    }
}