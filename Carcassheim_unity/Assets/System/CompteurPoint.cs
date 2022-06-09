using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
namespace Assets.system
{
    internal class CompteurPoints
    {
        /// <summary>
        /// le plateau
        /// </summary>
        Plateau _plateau;

        /// <summary>
        /// le singleton
        /// </summary>
        static CompteurPoints instance;

        /// <summary>
        /// instancie le singleton
        /// </summary>
        /// <param name="plateau">quel plateau le singleton va traiter</param>
        private CompteurPoints(Plateau plateau)
        {
            _plateau = plateau;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="plateau"></param>
        public static void Init(Plateau plateau)
        {
            if (instance == null)
                instance = new CompteurPoints(plateau);
            else
                instance._plateau = plateau;
        }

        /// <summary>
        /// compte les points de tout les champs du plateau
        /// </summary>
        /// <returns>quels joueurs gagnent combien de points</returns>
        public static Dictionary<ulong, int> CompterPointDesChampsEnFinDePartie()
        {
            ulong[] idJoueurTemp;
            var result = new Dictionary<ulong, int>();

            foreach (var item in instance._plateau.ChampsOuDesPionsOntEtePoses)
            {
                int x, y; ulong idSlot;
                (x, y, idSlot) = item;
                int points = CompterZoneFerme(x, y, (int)idSlot, out idJoueurTemp, true);
                foreach (var joueur in idJoueurTemp)
                {
                    if (result.ContainsKey(joueur))
                    {
                        result[joueur] += points;
                    }
                    else
                        result.Add(joueur, points);
                }
            }

            return result;
        }

        public static Dictionary<ulong, int> CompterPointFinPartie()
        {
            var temp = CompterPointDesChampsEnFinDePartie();
            var result = new Dictionary<ulong, int>();

            foreach (var item in temp)
            {
                result.Add(item.Key, item.Value);
            }

            foreach (var item in instance._plateau.AbbayeIncomplete)
            {
                int x, y;
                (x, y) = item;
                Tuile tuile = instance._plateau.GetTuile(x, y);
                int pts = PointAbbaye(tuile, true);

                ulong joueur = tuile.Slots[0].IdJoueur;

                if (joueur == ulong.MaxValue)
                    continue;

                if (result.ContainsKey(joueur))
                {
                    result[joueur] += pts;
                }
                else
                    result.Add(joueur, pts);
            }

            return result;
        }

        /// <summary>
        /// evalue les points qui vaut une zone
        /// </summary>
        /// <param name="x">l'abscisse de la tuile definissant la zone</param>
        /// <param name="y">l'ordonnee de la tuile definissant la zone</param>
        /// <param name="idSlot">le slot definissant la zone</param>
        /// <param name="idJoueur">les joueurs gagnants les points</param>
        /// <param name="compterChamps">laisser a false si il s'agit du comptage des points en milieu de partie du a la fermeture d'une zone</param>
        /// <returns>les points de la zone</returns>
        /// <exception cref="ArgumentException">si idSlot est trop grand</exception>
        public static int CompterZoneFerme(int x, int y, int idSlot, out ulong[] idJoueur, bool compterChamps = false)
        {
            Tuile tuile = instance._plateau.GetTuile(x, y);
            if (tuile.NombreSlot <= idSlot)
                throw new ArgumentException("idSlot trop grand");

            if (tuile.Slots[idSlot].Terrain == TypeTerrain.Pre && !compterChamps)
            {
                idJoueur = new ulong[0];
                return 0;
            }

            //idJoueur = tuile.Slots[idSlot].IdJoueur;

            List<(Tuile, ulong)> parcourue = new List<(Tuile, ulong)>();
            int result = 0;
            Dictionary<ulong, int> pionParJoueur = new Dictionary<ulong, int>();
            instance.PointsZone(tuile, idSlot, parcourue, ref result, pionParJoueur);

            // Debug.Log("Pions Par Joueur : " + pionParJoueur.ToString());
            // Debug.Log("POINTS : " + result);

            ulong playerWithMostPawn = ulong.MaxValue;
            int mostPawn = -1;
            List<ulong> playerGainingPoints = new List<ulong>();
            foreach (var item in pionParJoueur)
            {
                //Debug.Log(item);
                if (item.Value > mostPawn)
                {
                    mostPawn = item.Value;
                    playerWithMostPawn = item.Key;
                }
            }
            // Debug.Log("PION " + mostPawn);
            foreach (var item in pionParJoueur)
            {
                if (item.Value == mostPawn)
                {
                    playerGainingPoints.Add(item.Key);
                    // Debug.Log("JOUEUR " + item.Key);
                }
            }
            idJoueur = playerGainingPoints.ToArray();

            return result;
        }

        /// <summary>
        /// fonction recursive qui evalue les points qui vaut une zone
        /// </summary>
        /// <param name="tuile">la tuile courrante</param>
        /// <param name="idSlot">le slot courrant</param>
        /// <param name="parcourue">la liste des couples (tuiles; slots) deja parcourus</param>
        /// <param name="result">les points que vaut la zone</param>
        /// <param name="pionParJoueur"></param>
        /// <param name="compterChamps">laisser a false si il s'agit du comptage des points en milieu de partie du a la fermeture d'une zone</param>
        private void PointsZone(Tuile tuile, int idSlot,
            List<(Tuile, ulong)> parcourue, ref int result, Dictionary<ulong, int> pionParJoueur, bool compterChamps = false)
        {
            //bool vide, resultat = true;
            int[] positionsInternesProchainesTuiles;
            Tuile[] adj = TuilesAdjacentesAuSlot(tuile, idSlot, out positionsInternesProchainesTuiles);

            if (adj.Length == 0)
                return;

            int c = 0;
            foreach (var item in adj)
            {
                if (item == null)
                    continue;

                int pos = positionsInternesProchainesTuiles[c++];
                ulong nextSlot = item.IdSlotFromPositionInterne(pos);

                if (item == null || parcourue.Contains((item, nextSlot)))
                    continue;
                parcourue.Add((item, nextSlot));

                ulong idJ = item.Slots[nextSlot].IdJoueur;
                if (compterChamps)
                {
                    item.Slots[nextSlot].IdJoueur = ulong.MaxValue;
                }

                if (idJ != ulong.MaxValue)
                {
                    if (pionParJoueur.ContainsKey(idJ))
                        pionParJoueur[idJ]++;
                    else
                        pionParJoueur.Add(idJ, 1);
                }

                result += PointTerrain(item, nextSlot);
                PointsZone(item, (int)nextSlot, parcourue, ref result, pionParJoueur);
            }
        }

        /// <summary>
        /// calcul combien de point vaut un slot
        /// </summary>
        /// <param name="tuile">la tuile</param>
        /// <param name="idSlot">le slot representant le terrain</param>
        /// <returns>les points que vaut ce slot</returns>
        private static int PointTerrain(Tuile tuile, ulong idSlot)
        {
            int result = 1;
            TypeTerrain terrain = tuile.Slots[idSlot].Terrain;
            switch (terrain)
            {
                case TypeTerrain.Ville:
                    break;
                case TypeTerrain.VilleBlason:
                    result = 2;
                    break;
                case TypeTerrain.Pre:
                    result = PointChamps(tuile, idSlot);
                    break;
                case TypeTerrain.Abbaye:
                    result = PointAbbaye(tuile);
                    break;
                case TypeTerrain.Auberge:
                    break;
                case TypeTerrain.Cathedrale:
                    break;
                case TypeTerrain.Riviere:
                    break;
                case TypeTerrain.Route:
                    break;
                default:
                    break;
            }
            return result;
        }

        /// <summary>
        /// calcul combien de point vaut 1 slot champs
        /// </summary>
        /// <param name="tuile">tuile de l'abbaye</param>
        /// <returns>le nombre de tuile adjascente au celle-ci</returns>
        public static int PointAbbaye(Tuile tuile, bool endGame = false)
        {
            int[,] positionAdjacentes = new int[,]
            {
                { -1, -1 },
                { 0, -1 },
                { 1, -1 },
                { -1, 0 },
                { 1, 0 },
                { -1, 1 },
                { 1, 1 },
                { 0, 1 }
            };
            int nombreTuileAdjascentes = 0;

            for (int i = 0; i < 8; i++)
            {
                int x = positionAdjacentes[i, 0] + tuile.X;
                int y = positionAdjacentes[i, 1] + tuile.Y;
                if (instance._plateau.GetTuile(x, y) != null)
                    nombreTuileAdjascentes++;
            }

            if (endGame)
                return nombreTuileAdjascentes;
            return nombreTuileAdjascentes == 8 ? 9 : 0;
        }

        /// <summary>
        /// calcul combien de point vaut 1 slot champs
        /// </summary>
        /// <param name="tuile">la tuile surlaquelle est le champs</param>
        /// <param name="idSlot">le slot representant le champs</param>
        /// <returns>1 si le champs est adjascent a une ville, 0 sinon</returns>
        private static int PointChamps(Tuile tuile, ulong idSlot)
        {
            Slot[] slots = tuile.Slots;
            foreach (var item in slots[idSlot].LinkOtherSlots)
            {
                var t = slots[item].Terrain;
                if (t == TypeTerrain.VilleBlason || t == TypeTerrain.Pre)
                    return 1;
            }
            return 0;
        }

        /// <summary>
        /// permet d'acceder aux tuiles qui sont directement liees a un certain slot
        /// </summary>
        /// <param name="tuile">la tuile a partir de laquelle on cherche celle qui lui sont adjacentes</param>
        /// <param name="idSlot">le slot en question</param>
        /// <param name="positionsInternesProchainesTuiles"> les positions internes a partir desquelles on a accede aux tuiles retournees</param>
        /// <returns>un tableau des tuiles adjacentes</returns>
        private Tuile[] TuilesAdjacentesAuSlot(Tuile tuile, int idSlot, out int[] positionsInternesProchainesTuiles)
        {
            //emplacementVide = false;

            int[] positionsInternes = tuile.LienSlotPosition[idSlot];
            List<int> positionsInternesProchainesTuilesTemp = new List<int>();
            List<Tuile> resultat = new List<Tuile>();
            int x = tuile.X, y = tuile.Y;

            int direction;
            foreach (int position in positionsInternes)
            {
                //direction = (position + (3 * tuile.Rotation)) / 3;
                direction = (4 + (position / 3) - tuile.Rotation) % 4;

                Tuile elem = _plateau.GetTuile(x + Plateau.PositionAdjacentes[direction, 0],
                                      y + Plateau.PositionAdjacentes[direction, 1]);

                if (elem == null)
                {
                    //emplacementVide = true;
                }

                else if (!resultat.Contains(elem))
                {
                    resultat.Add(elem);
                    var trucComplique = ((position - 3 * tuile.Rotation) + 18 + 3 * elem.Rotation) % 12;
                    switch (trucComplique % 3)
                    {
                        case 0:
                            trucComplique = (trucComplique + 2);
                            break;
                        case 2:
                            trucComplique = (trucComplique - 2);
                            break;
                        default:
                            break;
                    }
                    positionsInternesProchainesTuilesTemp.Add(trucComplique);
                }
            }
            positionsInternesProchainesTuiles = positionsInternesProchainesTuilesTemp.ToArray();

            return resultat.ToArray();
        }

        /*
        public int[] EmplacementPionPossible(Tuile tuile, int idJoueur)
        {
            List<int> resultat = new List<int>();
            List<Tuile> parcourues = new List<Tuile>();

            for (int i = 0; i < tuile.NombreSlot; i++)
            {
                if (ZoneAppartientAutreJoueur(tuile, i, idJoueur, parcourues))
                    resultat.Add(i);
                parcourues.Clear();
            }

            return resultat.ToArray();
        }

        private bool ZoneAppartientAutreJoueur(Tuile tuile, int idSlot, int idJoueur, List<Tuile> parcourues)
        {
            bool vide, resultat = true;
            int[] positionsInternesProchainesTuiles;
            Tuile[] adj = TuilesAdjacentesAuSlot(tuile, idSlot, out vide, out positionsInternesProchainesTuiles);

            if (adj.Length == 0)
                return false;

            int c = 0;
            foreach (var t in adj)
            {
                if (t == null || parcourues.Contains(t))
                    continue;
                parcourues.Add(t);

                int pos = positionsInternesProchainesTuiles[c++];
                int nextSlot = t.IdSlotFromPositionInterne(pos);
                int idJ = t.Slots[nextSlot].IdJoueur;
                if (idJ != 0 && idJ != idJoueur)
                    return false;
                resultat = resultat && ZoneAppartientAutreJoueur(t, nextSlot, idJoueur, parcourues);
            }

            return resultat;
        }*/
    }
}
