# Tetris2D

Base del juego **Tetris 2D** hecha con **C# y OpenTK** (OpenGL 4) para la clase de
Graficación por Computadora.

## Requisitos

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- Windows (el atlas de texto usa `System.Drawing.Common`, que solo se soporta en Windows)

## Compilar y ejecutar

```bash
dotnet build
dotnet run
```

## Controles (Fase 1)

- Escribe tu nombre en la caja de texto.
- Presiona **Enter** o haz clic en **INICIAR JUEGO** (se habilita al escribir el nombre).
- Al iniciar se muestra un mensaje de confirmación con tu nombre.

## Estructura del proyecto

```
Program.cs                         Punto de entrada (crea la ventana y arranca el bucle)
TetrisGame.cs                      Ventana principal: loop, cámara y distribución de eventos
Graficos/GestorShader.cs           Compila los shaders GLSL (forma sólida y texto)
Graficos/DibujadorCuadros.cs       Dibuja rectángulos, bordes y líneas
UI/GeneradorFuenteAtlas.cs         Genera el atlas de letras con System.Drawing → textura GL
UI/RenderizadorTexto.cs            Dibuja texto en pantalla
UI/TemaArcade.cs                   Paleta de colores neón arcade
UI/Boton.cs                        Botón interactivo (hover y desactivado)
UI/CuadroTexto.cs                  Caja de entrada del nombre
Pantallas/Pantalla.cs              Clase base de las pantallas (gancho para la Fase 2)
Pantallas/PantallaBienvenida.cs    Pantalla de bienvenida arcade
```

## Cómo continuar (Fase 2)

Para agregar el tablero del Tetris:

1. Crea `Pantallas/PantallaJuego.cs` derivando de `Pantalla`.
2. Conéctalo en `TetrisGame.OnLoad`, suscribiéndote al evento `JugarSolicitado`
   de la pantalla de bienvenida (ahí hay un comentario de ejemplo).
3. Reutiliza `DibujadorCuadros` para pintar la cuadrícula 10×18 y `RenderizadorTexto`
   para los puntajes.