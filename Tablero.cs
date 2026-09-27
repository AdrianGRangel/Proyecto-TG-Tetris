using OpenTK.Mathematics;
using Tetris2D.Graficos;
using Tetris2D.UI;

public class Tablero
{
    public const int Columnas = 10;
    public const int Filas = 18;

    private readonly int[,] _celdas = new int[Columnas, Filas];

    public int ObtenerCelda(int x, int y)
    {
        return _celdas[x, y];
    }

    public void Limpiar()
    {
        for (int x = 0; x < Columnas; x++)
            for (int y = 0; y < Filas; y++)
                _celdas[x, y] = 0;
    }

    public bool Colisiona(IReadOnlyList<(int X, int Y)> bloques, int posX, int posY)
    {
        foreach ((int bx, int by) in bloques)
        {
            int x = posX + bx;
            int y = posY + by;

            if (x < 0 || x >= Columnas || y < 0 || y >= Filas)
                return true;
            if (_celdas[x, y] != 0)
                return true;
        }
        return false;
    }

    public void FijarPieza(Pieza pieza, int posX, int posY)
    {
        int valorColor = (int)pieza.Tipo + 1;
        foreach ((int bx, int by) in pieza.Bloques)
        {
            int x = posX + bx;
            int y = posY + by;
            if (x >= 0 && x < Columnas && y >= 0 && y < Filas)
                _celdas[x, y] = valorColor;
        }
    }

    public int LimpiarLineasCompletas()
    {
        int lineasBorradas = 0;

        for (int y = Filas - 1; y >= 0; y--)
        {
            if (FilaCompleta(y))
            {
                lineasBorradas++;
                BajarFilasSuperiores(y);
                y++;
            }
        }

        return lineasBorradas;
    }

    private bool FilaCompleta(int y)
    {
        for (int x = 0; x < Columnas; x++)
        {
            if (_celdas[x, y] == 0)
                return false;
        }
        return true;
    }

    private void BajarFilasSuperiores(int filaInicio)
    {
        for (int y = filaInicio; y > 0; y--)
        {
            for (int x = 0; x < Columnas; x++)
                _celdas[x, y] = _celdas[x, y - 1];
        }

        for (int x = 0; x < Columnas; x++)
            _celdas[x, 0] = 0;
    }

    public void Renderizar(DibujadorCuadros cuadros, float x0, float y0, float tamCelda)
    {
        float ancho = Columnas * tamCelda;
        float alto = Filas * tamCelda;

        // Fondo oscuro del area de juego + borde neon.
        cuadros.DibujarRectangulo(TemaArcade.PanelOscuro, x0, y0, x0 + ancho, y0 + alto);
        float grosorBorde = MathF.Max(2f, tamCelda * 0.10f);
        cuadros.DibujarBordeRectangulo(TemaArcade.Cian, x0, y0, x0 + ancho, y0 + alto, grosorBorde);

        // Cuadricula lineas verticales y horizontales cada tamCelda).
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

        // Cada bloque fijado: rectangulo de su color con un borde interior
        // oscuro para dar volumen ("bisel").
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

    private static Vector4 PiezaColor(int valorCelda)
    {
        TipoPieza tipo = (TipoPieza)(valorCelda - 1);
        return new Pieza(tipo).Color;
    }
}
