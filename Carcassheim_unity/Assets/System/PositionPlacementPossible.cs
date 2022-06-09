using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.system
{
    internal class PositionPlacementPossible
    {
        private Plateau _subject;
        static readonly int[,] PositionAdjacentes;

        public PositionPlacementPossible(Plateau plateau)
        {
            _subject = plateau;
        }
        static PositionPlacementPossible()
        {
            PositionAdjacentes = Plateau.PositionAdjacentes;
        }
        public Position[] Main(ulong idTuile)
        {
            return Main(Tuile.DicoTuiles[idTuile]);
        }

        public Position[] Main(Tuile tuile)
        {
            List<Position> resultat = new List<Position>();

            int x, y, rot;
            List<int> checkedX = new List<int>(), checkedY = new List<int>();
            foreach (var t in _subject.GetTuiles)
            {
                for (int i = 0; i < 4; i++)
                {
                    x = t.X + PositionAdjacentes[i, 0];
                    y = t.Y + PositionAdjacentes[i, 1];

                    if (checkedX.Contains(x) && checkedY.Contains(y))
                        continue;

                    checkedX.Add(x);
                    checkedY.Add(y);

                    for (rot = 0; rot < 4; rot++)
                    {
                        if (PlacementLegal(tuile, x, y, rot))
                        {
                            var p = new Position(x, y, rot);
                            //Debug.Log(p.ToString());
                            resultat.Add(p);
                        }
                    }
                }
            }

            return resultat.ToArray();
        }

        public bool PlacementLegal(ulong idTuile, int x, int y, int rotation)
        {
            return PlacementLegal(Tuile.DicoTuiles[idTuile], x, y, rotation);
        }

        public bool PlacementLegal(Tuile tuile, int x, int y, int rotation)
        {
            if (_subject.GetTuile(x, y) != null)
            {
                return false;
            }

            Tuile[] tuilesAdjacentes = TuilesAdjacentes(x, y);

            bool auMoinsUne = false;
            for (int i = 0; i < 4; i++)
            {
                Tuile t = tuilesAdjacentes[i];

                if (t == null)
                {
                    continue;
                }
                auMoinsUne = true;

                TypeTerrain[] faceTuile1 = tuile.TerrainSurFace((rotation + i) % 4);
                TypeTerrain[] faceTuile2 = t.TerrainSurFace((t.Rotation + i + 2) % 4);

                if (!CorrespondanceTerrains(faceTuile1, faceTuile2))
                    return false;
                else
                {
                    //Debug.Log("Correspondance : " + ((t.Rotation + i + 2) % 4));
                }
                //Debug.Log("hello " + i + x + y + rotation);
            }

            return auMoinsUne;
        }

        private bool CorrespondanceTerrains(TypeTerrain[] t1, TypeTerrain[] t2)
        {
            for (int i = 0; i < 3; i++)
            {
                if (t1[i] != t2[2 - i])
                    return false;
            }
            return true;
        }
        private Tuile[] TuilesAdjacentes(int x, int y)
        {
            Tuile[] resultat = new Tuile[4];

            var tab = PositionAdjacentes;

            for (int i = 0; i < 4; i++)
            {
                resultat[i] = _subject.GetTuile(x + tab[i, 0], y + tab[i, 1]);
            }
            return resultat;
        }

        private Tuile[] TuilesAdjacentes(Tuile t)
        {
            return TuilesAdjacentes(t.X, t.Y);
        }
    }
}
