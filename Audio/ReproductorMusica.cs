using NAudio.Wave;

namespace Tetris2D.Audio
{
    public enum PistaMusica
    {
        Menu,
        JuegoA,
        JuegoB,
        GameOver
    }

    /// <summary>
    /// Stream envoltorio para reproducir pistas de audio en bucle continuo y sin cortes.
    /// </summary>
    public class LoopStream : WaveStream
    {
        private readonly WaveStream _sourceStream;
        public bool EnableLooping { get; set; } = true;

        public LoopStream(WaveStream sourceStream)
        {
            _sourceStream = sourceStream;
        }

        public override WaveFormat WaveFormat => _sourceStream.WaveFormat;
        public override long Length => _sourceStream.Length;

        public override long Position
        {
            get => _sourceStream.Position;
            set => _sourceStream.Position = value;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int totalBytesRead = 0;
            while (totalBytesRead < count)
            {
                int bytesRead = _sourceStream.Read(buffer, offset + totalBytesRead, count - totalBytesRead);
                if (bytesRead == 0)
                {
                    if (_sourceStream.Position == 0 || !EnableLooping)
                        break;
                    _sourceStream.Position = 0;
                }
                totalBytesRead += bytesRead;
            }
            return totalBytesRead;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _sourceStream.Dispose();
            }
            base.Dispose(disposing);
        }
    }

    /// <summary>
    /// Gestor y reproductor de música de fondo (BGM) y pistas temáticas del juego
    /// utilizando NAudio y Windows Media Foundation para compatibilidad directa con archivos .m4a y .wav.
    /// </summary>
    public static class ReproductorMusica
    {
        private static WaveOutEvent? _salida;
        private static LoopStream? _loopStream;
        private static WaveStream? _lector;
        private static readonly object _bloqueo = new();

        private static float _volumen = 0.35f;
        private static bool _silenciado = false;
        private static PistaMusica? _pistaActual = null;
        private static bool _estaPausado = false;

        public static bool EsSilenciado => _silenciado;
        public static bool EstaPausado => _estaPausado;
        public static PistaMusica? PistaActual => _pistaActual;

        public static float Volumen
        {
            get => _volumen;
            set
            {
                _volumen = Math.Clamp(value, 0f, 1f);
                lock (_bloqueo)
                {
                    if (_salida != null)
                        _salida.Volume = _silenciado ? 0f : _volumen;
                }
            }
        }

        public static void AlternarSilencio()
        {
            _silenciado = !_silenciado;
            lock (_bloqueo)
            {
                if (_salida != null)
                    _salida.Volume = _silenciado ? 0f : _volumen;
            }
        }

        /// <summary>
        /// Inicia la reproducción de una pista temática de forma asíncrona.
        /// </summary>
        public static void Reproducir(PistaMusica pista, bool enBucle = true, float? volumen = null)
        {
            if (volumen.HasValue)
                _volumen = Math.Clamp(volumen.Value, 0f, 1f);

            Task.Run(() =>
            {
                lock (_bloqueo)
                {
                    if (_pistaActual == pista && _salida?.PlaybackState == PlaybackState.Playing)
                        return;

                    DetenerInterno();

                    string? ruta = ResolverRuta(pista);
                    if (string.IsNullOrEmpty(ruta) || !File.Exists(ruta))
                        return;

                    try
                    {
                        WaveStream stream;
                        if (ruta.EndsWith(".wav", StringComparison.OrdinalIgnoreCase))
                        {
                            stream = new AudioFileReader(ruta);
                        }
                        else
                        {
                            stream = new MediaFoundationReader(ruta);
                        }

                        _lector = stream;
                        _loopStream = new LoopStream(_lector) { EnableLooping = enBucle };
                        _salida = new WaveOutEvent();
                        _salida.Init(_loopStream);
                        _salida.Volume = _silenciado ? 0f : _volumen;
                        _salida.Play();

                        _pistaActual = pista;
                        _estaPausado = false;
                    }
                    catch
                    {
                        DetenerInterno();
                    }
                }
            });
        }

        public static void Pausar()
        {
            lock (_bloqueo)
            {
                try
                {
                    if (_salida?.PlaybackState == PlaybackState.Playing)
                    {
                        _salida.Pause();
                        _estaPausado = true;
                    }
                }
                catch { }
            }
        }

        public static void Reanudar()
        {
            lock (_bloqueo)
            {
                try
                {
                    if (_salida?.PlaybackState == PlaybackState.Paused)
                    {
                        _salida.Play();
                        _estaPausado = false;
                    }
                }
                catch { }
            }
        }

        public static void Detener()
        {
            lock (_bloqueo)
            {
                DetenerInterno();
                _pistaActual = null;
                _estaPausado = false;
            }
        }

        private static void DetenerInterno()
        {
            try
            {
                if (_salida != null)
                {
                    _salida.Stop();
                    _salida.Dispose();
                    _salida = null;
                }
                if (_loopStream != null)
                {
                    _loopStream.Dispose();
                    _loopStream = null;
                }
                if (_lector != null)
                {
                    _lector.Dispose();
                    _lector = null;
                }
            }
            catch { }
        }

        private static string? ResolverRuta(PistaMusica pista)
        {
            string nombreBase = pista switch
            {
                PistaMusica.Menu => "videoplayback (1)",
                PistaMusica.JuegoA => "videoplayback (2)",
                PistaMusica.JuegoB => "videoplayback (1)",
                PistaMusica.GameOver => "videoplayback",
                _ => "videoplayback"
            };

            string[] extensiones = { ".m4a", ".wav" };
            string[] carpetas = {
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Audio"),
                Path.Combine(Directory.GetCurrentDirectory(), "Audio"),
                "Audio"
            };

            foreach (var carpeta in carpetas)
            {
                foreach (var ext in extensiones)
                {
                    string ruta = Path.Combine(carpeta, nombreBase + ext);
                    if (File.Exists(ruta))
                        return Path.GetFullPath(ruta);
                }
            }

            return null;
        }
    }
}
