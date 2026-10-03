using OpenTK.Mathematics;
using Tetris2D.UI;

namespace Tetris2D
{
    public enum TipoPieza
    {
        I,
        O,
        T,
        S,
        Z,
        J,
        L
    }

    public class Pieza
    {
        private static readonly Random Azar = new();

        private static readonly Dictionary<TipoPieza, (int X, int Y)[]> FormasBase = new()
        {
            [TipoPieza.I] = new[] { (0, 1), (1, 1), (2, 1), (3, 1) },
            [TipoPieza.O] = new[] { (1, 0), (2, 0), (1, 1), (2, 1) },
            [TipoPieza.T] = new[] { (1, 0), (0, 1), (1, 1), (2, 1) },
            [TipoPieza.S] = new[] { (1, 0), (2, 0), (0, 1), (1, 1) },
            [TipoPieza.Z] = new[] { (0, 0), (1, 0), (1, 1), (2, 1) },
            [TipoPieza.J] = new[] { (0, 0), (0, 1), (1, 1), (2, 1) },
            [TipoPieza.L] = new[] { (2, 0), (0, 1), (1, 1), (2, 1) }
        };

        private static readonly Dictionary<TipoPieza, Vector4> Colores = new()
        {
            [TipoPieza.I] = TemaArcade.Cian,
            [TipoPieza.O] = TemaArcade.Amarillo,
            [TipoPieza.T] = TemaArcade.Magenta,
            [TipoPieza.S] = TemaArcade.Verde,
            [TipoPieza.Z] = TemaArcade.Rojo,
            [TipoPieza.J] = TemaArcade.Azul,
            [TipoPieza.L] = TemaArcade.Naranja
        };

        public TipoPieza Tipo { get; }
        public List<(int X, int Y)> Bloques { get; private set; }
        public Vector4 Color => Colores[Tipo];

        public Pieza(TipoPieza tipo)
        {
            Tipo = tipo;
            Bloques = FormasBase[tipo].ToList();
        }

        public List<(int X, int Y)> CalcularRotacion()
        {
            if (Tipo == TipoPieza.O)
                return Bloques.ToList();

            return Bloques
                .Select(b => (X: 3 - b.Y, Y: b.X))
                .ToList();
        }

        public void Rotar()
        {
            Bloques = CalcularRotacion();
        }

        public void EstablecerBloques(IEnumerable<(int X, int Y)> bloques)
        {
            Bloques = bloques.ToList();
        }

        public static Pieza Aleatoria()
        {
            Array valores = Enum.GetValues(typeof(TipoPieza));
            return new Pieza((TipoPieza)valores.GetValue(Azar.Next(valores.Length))!);
        }
    }
}
