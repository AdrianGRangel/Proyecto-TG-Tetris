using System.Media;
using System.Text;

namespace Tetris2D.Audio
{
    public enum TonoJuego
    {
        Mover, Rotar, Fijar, Linea, Nivel, GameOver
    }

    public static class Sonido
    {
        private const int Tasa = 22050;

        public static void Reproducir(TonoJuego tono)
        {
            byte[] wav = ConstruirWav(tono);
            if (wav.Length == 0)
                return;

            try
            {
                using var ms = new MemoryStream(wav);
                using var jugador = new SoundPlayer(ms);
                jugador.Play();
            }
            catch
            {
                // Ignorar errores si no hay dispositivo de audio disponible
            }
        }

        private static byte[] ConstruirWav(TonoJuego tono)
        {
            return tono switch
            {
                // (frecuencia, segundos, volumen)
                TonoJuego.Mover => CrearBarrido(980, 980, 0.05, 0.30),
                TonoJuego.Rotar => CrearBarrido(740, 740, 0.05, 0.30),
                TonoJuego.Fijar => CrearBarrido(330, 300, 0.10, 0.50),
                TonoJuego.Linea => CrearBarrido(660, 990, 0.18, 0.60),
                TonoJuego.Nivel => CrearBarrido(440, 880, 0.28, 0.60),
                TonoJuego.GameOver => CrearBarrido(420, 90, 0.70, 0.65),
                _ => Array.Empty<byte>()
            };
        }

        private static byte[] CrearBarrido(double fInicio, double fFin, double segundos, double volumen)
        {
            int n = (int)(Tasa * segundos);
            var muestras = new byte[n * 2];

            for (int i = 0; i < n; i++)
            {
                double t = i / (double)Tasa;
                double progreso = i / (double)Math.Max(1, n - 1);
                double frecuencia = fInicio + (fFin - fInicio) * progreso;
                double seno = Math.Sin(2 * Math.PI * frecuencia * t) * volumen;
                short pcm = (short)Math.Clamp((double)(seno * short.MaxValue), short.MinValue, short.MaxValue);
                muestras[i * 2] = (byte)(pcm & 0xFF);
                muestras[i * 2 + 1] = (byte)((pcm >> 8) & 0xFF);
            }

            return EnvolverEnWav(muestras);
        }

        private static byte[] EnvolverEnWav(byte[] pcm)
        {
            var wav = new byte[44 + pcm.Length];

            Array.Copy(Encoding.ASCII.GetBytes("RIFF"), 0, wav, 0, 4);
            BitConverter.GetBytes(36 + pcm.Length).CopyTo(wav, 4);
            Array.Copy(Encoding.ASCII.GetBytes("WAVE"), 0, wav, 8, 4);
            Array.Copy(Encoding.ASCII.GetBytes("fmt "), 0, wav, 12, 4);
            BitConverter.GetBytes(16).CopyTo(wav, 16); // tamano subchunk de formato
            BitConverter.GetBytes((short)1).CopyTo(wav, 20); // formato PCM
            BitConverter.GetBytes((short)1).CopyTo(wav, 22); // numero de canales (mono)
            BitConverter.GetBytes(Tasa).CopyTo(wav, 24); // frecuencia de muestreo
            BitConverter.GetBytes(Tasa * 2).CopyTo(wav, 28); // bytes por segundo
            BitConverter.GetBytes((short)2).CopyTo(wav, 32); // alineamiento de bloque
            BitConverter.GetBytes((short)16).CopyTo(wav, 34); // bits por muestra
            Array.Copy(Encoding.ASCII.GetBytes("data"), 0, wav, 36, 4);
            BitConverter.GetBytes(pcm.Length).CopyTo(wav, 40); // tamano de los datos
            Array.Copy(pcm, 0, wav, 44, pcm.Length);

            return wav;
        }
    }
}
