using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace system
{
    internal class Riviere
    {
        /// <summary>
        /// le plateau sur lequel il faut generer une riviere
        /// </summary>
        private Plateau _plateau;

        /// <summary>
        /// instancie un generateur de riviere
        /// </summary>
        /// <param name="plateau">le plateau</param>
        private Riviere(Plateau plateau)
        {
            _plateau = plateau;
        }

        /// <summary>
        /// cree la riviere sur le plateau
        /// </summary>
        /// <param name="tuilesRiviere">le 1er elem du tableau doit etre le debut de la riviere, le dernier doit en etre la fin</param>
        /// <exception cref="Exception">si le tableau des tuiles rivieres n'est pas conforme</exception>
        public static void Init(Plateau plateau, Tuile[] tuilesRiviere)
        {
            var obj = new Riviere(plateau);
            int length = tuilesRiviere.Length;
            int i1 = -1, i2 = -1;
            // faire en sorte que l'amont et l'avale soit aux extremites du tab
            for (int i = 0; i < length; i++)
            {
                if (RiviereExtreme(tuilesRiviere[i]))
                {
                    if (i1 == -1)
                        i1 = i;
                    else i2 = i;
                }
            }

            if (i2 == -1)
                throw new Exception("tuiles riviere extremes introuvable");

            (tuilesRiviere[0], tuilesRiviere[i1]) = (tuilesRiviere[i1], tuilesRiviere[0]);
            (tuilesRiviere[length - 1], tuilesRiviere[i2]) = (tuilesRiviere[i2], tuilesRiviere[length - 1]);
            obj.InitialiserRiviere(tuilesRiviere);
        }

        /// <summary>
        /// cree la riviere
        /// </summary>
        /// <param name="tuilesRiviere">les tuiles qui forment la riviere</param>
        private void InitialiserRiviere(Tuile[] tuilesRiviere)
        {
            _plateau.Poser1ereTuile(tuilesRiviere[0].Id);

            var rand = new Random();
            int x, y;
            Tuile current;

            int lastDirection = DirectionRiviereExtreme(tuilesRiviere[0]);

            for (int i = 1; i < tuilesRiviere.Length; i++)
            {
                current = tuilesRiviere[i];
                int slotR = SlotRiviere(current);
                x = Plateau.PositionAdjacentes[lastDirection, 0] + tuilesRiviere[i - 1].X;
                y = Plateau.PositionAdjacentes[lastDirection, 0] + tuilesRiviere[i - 1].Y;

                int randI;
                if (RiviereExtreme(current))
                {
                    randI = 0;
                }
                else
                    randI = rand.Next(1);
                int rot = tuilesRiviere[i].LienSlotPosition[slotR][randI] / 3;
                rot += ((lastDirection + 2) % 4);

                /*
                Position = _plateau.PlacementLegal(current)[randI];
                
                _plateau.PoserTuile(current, Position);
                */
                _plateau.PoserTuileFantome(current.Id, x, y, rot);
                lastDirection = 1 - current.LienSlotPosition[slotR][1 - randI] / 3;
            }
        }

        /// <summary>
        /// retourne le slot riviere d'une tuile
        /// </summary>
        /// <param name="tuile">la tuile en question</param>
        /// <returns>l'id du slot representant la riviere</returns>
        private static int SlotRiviere(Tuile tuile)
        {
            for (int i = 0; i < tuile.Slots.Length; i++)
            {
                if (tuile.Slots[i].Terrain == TypeTerrain.Riviere)
                    return i;
            }

            return -1;
        }

        /// <summary>
        /// calcul la direction vers laquelle une riviere pointe
        /// </summary>
        /// <param name="tuile">la tuile</param>
        /// <returns>la direction</returns>
        /// <exception cref="Exception">si la tuile n'est pas bien formatee</exception>
        private static int DirectionRiviereExtreme(Tuile tuile)
        {
            int slot = SlotRiviere(tuile);
            if (tuile.LienSlotPosition[slot].Length != 1)
                throw new Exception("tuile riviere incoherente");

            int posInterne = tuile.LienSlotPosition[slot][0];

            if (posInterne % 3 != 1)
                throw new Exception("tuile riviere incoherente");

            return tuile.Rotation + posInterne / 3;
        }

        /// <summary>
        /// calcul si une tuile a une seule face riviere
        /// </summary>
        /// <param name="tuile">la tuile</param>
        /// <returns>true si la tuile n'a qu'une seule face riviere, false sinon</returns>
        private static bool RiviereExtreme(Tuile tuile)
        {
            int slot = SlotRiviere(tuile);
            if (tuile.LienSlotPosition[slot].Length == 1)
                return true;
            return false;
        }
    }
}
