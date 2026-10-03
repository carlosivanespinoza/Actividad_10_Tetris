using System.Media;

namespace Tetris2D.Audio
{
    public enum TonoJuego
    {
        Mover,
        Rotar,
        Fijar,
        Linea,
        Nivel,
        GameOver
    }

    public static class Sonido
    {
        private const int FrecuenciaMuestreo = 22050;

        public static void Reproducir(TonoJuego tono)
        {
            try
            {
                byte[] wav = tono switch
                {
                    TonoJuego.Mover => CrearWav(440, 520, 0.045, 0.22),
                    TonoJuego.Rotar => CrearWav(620, 760, 0.060, 0.24),
                    TonoJuego.Fijar => CrearWav(230, 160, 0.090, 0.28),
                    TonoJuego.Linea => CrearWav(520, 920, 0.150, 0.30),
                    TonoJuego.Nivel => CrearWav(600, 1200, 0.220, 0.32),
                    TonoJuego.GameOver => CrearWav(260, 90, 0.450, 0.35),
                    _ => CrearWav(440, 440, 0.080, 0.20)
                };

                SoundPlayer player = new(new MemoryStream(wav));
                player.Play();
            }
            catch
            {
                // Si falla el sonido, no quiero que se cierre el juego.
            }
        }

        private static byte[] CrearWav(double frecuenciaInicial, double frecuenciaFinal, double segundos, double volumen)
        {
            short[] muestras = CrearMuestras(frecuenciaInicial, frecuenciaFinal, segundos, volumen);
            int datosBytes = muestras.Length * sizeof(short);

            using MemoryStream ms = new();
            using BinaryWriter bw = new(ms);

            bw.Write("RIFF"u8.ToArray());
            bw.Write(36 + datosBytes);
            bw.Write("WAVE"u8.ToArray());
            bw.Write("fmt "u8.ToArray());
            bw.Write(16);
            bw.Write((short)1);
            bw.Write((short)1);
            bw.Write(FrecuenciaMuestreo);
            bw.Write(FrecuenciaMuestreo * sizeof(short));
            bw.Write((short)sizeof(short));
            bw.Write((short)16);
            bw.Write("data"u8.ToArray());
            bw.Write(datosBytes);

            foreach (short muestra in muestras)
                bw.Write(muestra);

            return ms.ToArray();
        }

        private static short[] CrearMuestras(double frecuenciaInicial, double frecuenciaFinal, double segundos, double volumen)
        {
            int total = Math.Max(1, (int)(FrecuenciaMuestreo * segundos));
            short[] muestras = new short[total];

            double fase = 0;
            for (int i = 0; i < total; i++)
            {
                double t = i / (double)Math.Max(1, total - 1);
                double frecuencia = frecuenciaInicial + (frecuenciaFinal - frecuenciaInicial) * t;
                fase += 2.0 * Math.PI * frecuencia / FrecuenciaMuestreo;

                double envolvente = Math.Sin(Math.PI * t);
                double valor = Math.Sin(fase) * volumen * envolvente;
                muestras[i] = (short)(valor * short.MaxValue);
            }

            return muestras;
        }
    }
}
