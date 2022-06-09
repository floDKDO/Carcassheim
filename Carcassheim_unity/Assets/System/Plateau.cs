using System.Collections.Generic;
using System;
using UnityEngine;

namespace Assets.system
{
    public struct Zone
    {
        public Zone(ulong[] id_players, Tuple<int, int, ulong>[] positions)
        {

            this.id_players = id_players;
            this.positions = positions;
        }

        public ulong[] id_players;
        public Tuple<int, int, ulong>[] positions;
    };

    public struct PlayerScoreParam
    {
        public PlayerScoreParam(ulong id_player, int points_gagnes)
        {
            this.id_player = id_player;
            this.points_gagnes = points_gagnes;
        }
        public ulong id_player;
        public int points_gagnes;
    };
    public class Plateau
    {
        private Tuile _tuileFantome;
        private static Tuile decoyTuile;
        /// <summary>
        /// le dernier virage de la riviere
        /// </summary>
        private int _lastRiverTurn = 0;

        /// <summary>
        /// le decalage des coordonnees en fonction d'une direction
        /// </summary>
        public static readonly int[,] PositionAdjacentes;

        /// <summary>
        /// les tuiles en fonction de leur id
        /// </summary>
        private Dictionary<ulong, Tuile> _dicoTuile;

        /// <summary>
        /// les tuiles en fonction de leur id
        /// </summary>
        public Dictionary<ulong, Tuile> DicoTuile => _dicoTuile;

        /// <summary>
        /// les tuiles posees sur le plateau
        /// </summary>
        private List<Tuile> _tuiles;

        /// <summary>
        /// les tuiles posees sur le plateau
        /// </summary>
        public List<Tuile> Tuiles
        {
            get { return _tuiles; }
            set { _tuiles = value; }
        }

        /// <summary>
        /// les tuiles posees sur le plateau
        /// </summary>
        public List<(int, int, ulong)> ChampsOuDesPionsOntEtePoses { get; private set; }

        public List<(int, int)> AbbayeIncomplete { get; private set; }

        /// <summary>
        /// instancie un plateau
        /// </summary>
        /// <param name="dicoTuiles">les tuiles que ce plateau pourra utiliser</param>
        public Plateau(Dictionary<ulong, Tuile> dicoTuiles)
        {
            _tuiles = new List<Tuile>();
            CompteurPoints.Init(this);
            _dicoTuile = dicoTuiles;
            ChampsOuDesPionsOntEtePoses = new List<(int, int, ulong)>();
            AbbayeIncomplete = new List<(int, int)>();
            decoyTuile = Tuile.Copy(_dicoTuile[0]);
        }

        /// <summary>
        /// initialise un plateau
        /// </summary>
        public Plateau()
        {
            _tuiles = new List<Tuile>();
            //CompteurPoints.Init(this);
            _dicoTuile = new Dictionary<ulong, Tuile>();
            ChampsOuDesPionsOntEtePoses = new List<(int, int, ulong)>();
            AbbayeIncomplete = new List<(int, int)>();
            decoyTuile = Tuile.Copy(_dicoTuile[0]);
        }

        /// <summary>
        /// initialise les champs static
        /// </summary>
        static Plateau()
        {
            PositionAdjacentes = new int[,]
            {
                { 0, -1 },
                { 1, 0 },
                { 0, 1 },
                { -1, 0 }
            };
        }

        /// <summary>
        /// recupere une tuile en fonction de ses coordonnees
        /// </summary>
        /// <param name="x">l'abscisse</param>
        /// <param name="y">l'ordonnee</param>
        /// <returns>la tuile aux coordonnees (x, y)</returns>
        public Tuile GetTuile(int x, int y)
        {
            //foreach (var item in _tuiles)
            //{
            //    if (item.X == x && item.Y == y)
            //        return item;
            //}
            //return null;

            decoyTuile.X = x;
            decoyTuile.Y = y;
            //Tuile.Compare<Tuile> tuileComparer = Tuile.CompareTuile;
            //IComparer<Tuile> tuileComparer = Tuile.CompareTuile;
            int i = _tuiles.BinarySearch(decoyTuile, decoyTuile);
            if (i < 0)
                return null;
            return _tuiles[i];
        }

        /// <summary>
        /// les tuiles posees
        /// </summary>
        public Tuile[] GetTuiles => _tuiles.ToArray();/*
    {
        return _tuiles.ToArray();
    }*/

        //public void GenererRiviere()
        //{
        //    List<Tuile> riviere = new List<Tuile>();

        //    foreach (var item in _dicoTuile.Values)
        //    {
        //        if (item.Riviere)
        //            riviere.Add(item);
        //    }

        //    Riviere.Init(this, riviere.ToArray());
        //}

        /// <summary>
        /// pose la toute premiere tuile
        /// </summary>
        /// <param name="idTuile">l'id de la tuile a poser</param>
        public void Poser1ereTuile(ulong idTuile)
        {
            var t = tuileDeModelId(idTuile);
            t.X = 0; t.Y = 0; t.Rotation = 0;
            _tuiles = new List<Tuile> { t };
        }

        //private void PoserTuile(Tuile tuile, Position pos)
        //{
        //    PoserTuile(tuile, pos.X, pos.Y, pos.ROT);
        //}

        //private void PoserTuile(ulong idTuile, Position pos)
        //{
        //    PoserTuile(tuileDeModelId(idTuile), pos.X, pos.Y, pos.ROT);
        //}

        //private void PoserTuile(ulong idTuile, int x, int y, int rot)
        //{
        //    PoserTuile(tuileDeModelId(idTuile), x, y, rot);
        //}

        /// <summary>
        /// Pose une tuile
        /// </summary>
        /// <param name="tuile">la tuile a poser</param>
        /// <param name="x">l'abscisse ou la poser</param>
        /// <param name="y">l'ordonnees ou la poser</param>
        /// <param name="rot">la rotation a lui appliquer (dans le sens direct)</param>
        private void PoserTuile(Tuile tuile, int x, int y, int rot)
        {
            tuile.X = x;
            tuile.Y = y;
            tuile.Rotation = rot;
            _tuiles.Add(tuile);
            _tuiles.Sort(tuile);

            if (tuile.Riviere)
            {
                int temp;
                CheckDirectionRiviere(tuile, x, y, rot, out temp);
                if (temp != 0)
                    _lastRiverTurn = temp;
            }

            bool abbaye = false;

            foreach (Slot slot in tuile.Slots)
            {
                if (slot.Terrain == TypeTerrain.Abbaye)
                {
                    abbaye = true;
                    break;
                }
            }

            if (abbaye && CompteurPoints.PointAbbaye(tuile) == 0)
            {
                AbbayeIncomplete.Add((x, y));
            }

            int c = 0;
            foreach (var item in _tuiles)
            {
                if (item.TuileFantome)
                {
                    // Debug.Log("tuile d'id " + item.Id + " est toujours fantome");
                    c++;
                }
            }
            // Debug.Log("il y a en tout : " + c + "tuiles fantomes");
        }

        /// <summary>
        /// quel virage prend une tuile riviere lorsqu'elle est dans une certaine position
        /// </summary>
        /// <param name="tuile">la tuile</param>
        /// <param name="x">l'abscisse la tuile est</param>
        /// <param name="y">l'ordonnee ou la tuile est</param>
        /// <param name="rot">la rotation qui lui est appliquee</param>
        /// <param name="turn">le virage qu'elle prend</param>
        public void CheckDirectionRiviere(Tuile tuile, int x, int y, int rot, out int turn)
        {
            turn = 0;
            int pos1, pos2, dir1 = -1, dir2 = -1;
            for (int i = 0; i < tuile.Slots.Length; i++)
            {
                if (tuile.Slots[i].Terrain == TypeTerrain.Riviere)
                {
                    var tab = tuile.LienSlotPosition[i];
                    if (tab.Length == 1)
                    {
                        turn = 0;
                        return;
                    }
                    pos1 = tab[0];
                    pos2 = tab[1];

                    dir1 = pos1 / 3;
                    dir2 = pos2 / 3;
                    break;
                }
            }
            if ((dir1 + dir2) % 2 == 0)
            {
                turn = 0;
                return;
            }

            int x1 = x + PositionAdjacentes[(dir1 + 4 - rot) % 4, 0];
            int y1 = y + PositionAdjacentes[(dir1 + 4 - rot) % 4, 1];
            if (GetTuile(x1, y1) == null)
                (dir1, dir2) = (dir2, dir1);

            turn = dir1 - dir2;
            if (turn == -3)
                turn = -1;
            else if (turn == 3)
                turn = 1;
        }

        //turn = 0;
        //int rot = tuile.Rotation;
        //int dirInit = -1, dirNext = -1;
        //for (int i = 0; i < 4; i++)
        //{
        //    int x1 = x + PositionAdjacentes[i, 0];
        //    int y1 = y + PositionAdjacentes[i, 1];

        //    if (GetTuile(x1, y1) != null)
        //    {
        //        dirInit = i + tuile.Rotation;
        //        Debug.Log("tuile trouve en (" + x1 + " : " + y1 + ") direction :" + i);
        //        Debug.LogWarning("x = " + x + " y = " + y);
        //        break;
        //    }
        //}
        //int j = 0;
        //foreach (var item in tuile.Slots)
        //{
        //    if (item.Terrain == TypeTerrain.Riviere)
        //    {
        //        if (tuile.LienSlotPosition.Length == 1)
        //            return;

        //        int direction = tuile.LienSlotPosition[j][0];
        //        direction = direction / 3;
        //        direction += rot;
        //        direction = direction % 4;

        //        if (direction == dirInit)
        //        {
        //            direction = tuile.LienSlotPosition[j][1];
        //            direction = direction / 3;
        //            direction += rot;
        //            direction = direction % 4;
        //        }
        //        dirNext = direction;
        //    }
        //    j++;
        //}

        //switch ((4 + dirInit - dirNext) % 4)
        //{
        //    case GAUCHE:
        //        turn = GAUCHE;
        //        break;
        //    case DROITE:
        //        turn = DROITE;
        //        break;
        //}
        //Debug.Log("Direction intiale :" + dirInit + "      prochaine direction " + dirNext);
        //}

        /// <summary>
        /// Pose une tuile ephemere, supprime les autres tuiles ephemeres
        /// </summary>
        /// <param name="idTuile">l'id de la tuile en question</param>
        /// <param name="pos">la position dans laquelle la tuile est posee</param>
        public void PoserTuileFantome(ulong idTuile, Position pos)
        {
            PoserTuileFantome(idTuile, pos.X, pos.Y, pos.ROT);
        }

        /// <summary>
        /// Pose une tuile ephemere, supprime les autres tuiles ephemeres
        /// </summary>
        /// <param name="idTuile">l'id de la tuile en question</param>
        /// <param name="x">l'abscisse ou la placer</param>
        /// <param name="y">l'ordonnee ou la placer</param>
        /// <param name="rot">la rotation a appliquer a la tuile</param>
        public void PoserTuileFantome(ulong idTuile, int x, int y, int rot)
        {
            var t = tuileDeModelId(idTuile);
            _tuileFantome = t;
            t.TuileFantome = true;
            _tuiles.Remove(FindTuileFantome);
            PoserTuile(t, x, y, rot);

        }

        /// <summary>
        /// Les tuiles fantome deviennent des vrais tuiles
        /// </summary>
        public void ValiderTour()
        {
            Tuile tuile = FindTuileFantome;
            if (tuile != null)
            {
                tuile.TuileFantome = false;
                _tuileFantome = null;
            }
        }

        /// <summary>
        /// La tuile fantome du plateau si il y en a une
        /// </summary>
        private Tuile FindTuileFantome
        {
            get
            {
                foreach (var item in _tuiles)
                {
                    if (item.TuileFantome)
                        return item;
                }
                return null;
                //return _tuileFantome;
            }
        }

        //private void RemoveTuilesFantomes()
        //{
        //    int toRemove = int.MaxValue;
        //    bool founded = false;
        //    for (int i = 0; i < _tuiles.Count; i++)
        //    {
        //        var current = _tuiles[i];
        //        if (current.TuileFantome)
        //        {
        //            if (founded)
        //                throw new Exception("shouldn't be more than 1 TuileFantome");
        //            founded = true;
        //            toRemove = i;
        //        }
        //    }
        //    if (founded)
        //        _tuiles.RemoveAt(toRemove);
        //}

        /// <summary>
        /// instancie une tuile a partir d'un modele
        /// </summary>
        /// <param name="id_tuile">l'id du modele</param>
        /// <returns>la tuile instanciee</returns>
        public Tuile tuileDeModelId(ulong id_tuile)
        {
            return Tuile.Copy(_dicoTuile[id_tuile]);
        }

        /// <summary>
        /// parcoure tous les emplacements libres du plateau, verfie si chacun est libre, dans chaque rotation
        /// </summary>
        /// <param name="idTuile">l'id de la tuile</param>
        /// <returns>un tableau de toutes les positions ou une tuile est posable</returns>
        public Position[] PositionsPlacementPossible(ulong idTuile)
        {
            return PositionsPlacementPossible(tuileDeModelId(idTuile));
        }

        /// <summary>
        /// parcoure tous les emplacements libres du plateau, verfie si chacun est libre, dans chaque rotation
        /// </summary>
        /// <param name="tuile">la tuile</param>
        /// <returns>un tableau de toutes les positions ou une tuile est posable</returns>
        public Position[] PositionsPlacementPossible(Tuile tuile)
        {
            var listTuiles = new List<Tuile>();

            foreach (var item in _tuiles)
            {
                if (item.TuileFantome)
                    continue;
                listTuiles.Add(item);
            }

            if (listTuiles.Count == 0)
                return new Position[] { new Position(0, 0, 0) };

            List<Position> resultat = new List<Position>();

            int x, y, rot;

            List<Position> checked_pos = new List<Position>();
            foreach (var t in listTuiles)
            {
                for (int i = 0; i < 4; i++)
                {
                    x = t.X + PositionAdjacentes[i, 0];
                    y = t.Y + PositionAdjacentes[i, 1];

                    if (checked_pos.Contains(new Position(x, y, 0)))
                        continue;

                    checked_pos.Add(new Position(x, y, 0));

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

        /// <summary>
        /// verifie que les faces d'une tuile s'assemblent avec les faces des tuiles adjacentes a un emplacement
        /// </summary>
        /// <param name="idTuile">l'id de cette tuile</param>
        /// <param name="x">l'abscisse de l'emplacement</param>
        /// <param name="y">l'ordonnee de l'emplacement</param>
        /// <param name="rotation">la rotation appliquee a la tuile</param>
        /// <returns>true si la tuile est placable a ces coordonnees avec cette rotation, false sinon</returns>
        public bool PlacementLegal(ulong idTuile, int x, int y, int rotation)
        {
            return PlacementLegal(tuileDeModelId(idTuile), x, y, rotation);
        }

        /// <summary>
        /// verifie que les faces d'une tuile s'assemblent avec les faces des tuiles adjacentes a un emplacement
        /// </summary>
        /// <param name="tuile">la tuile</param>
        /// <param name="x">l'abscisse de l'emplacement</param>
        /// <param name="y">l'ordonnee de l'emplacement</param>
        /// <param name="rotation">la rotation appliquee a la tuile</param>
        /// <returns>true si la tuile est placable a ces coordonnees avec cette rotation, false sinon</returns>
        public bool PlacementLegal(Tuile tuile, int x, int y, int rotation)
        {
            bool riviere = tuile.Riviere;

            Tuile tl = GetTuile(x, y);
            // if (tl != null)
            // {
            //     Debug.Log("tuile non nulle en (" + x + " : " + y + ") de fantomite " + tl.TuileFantome);
            // }
            if (tl != null && !tl.TuileFantome)
            {
                return false;
            }

            Tuile[] tuilesAdjacentes = TuilesAdjacentes(x, y);

            bool auMoinsUne = true;
            for (int i = 0; i < 4; i++)
            {
                Tuile t = tuilesAdjacentes[i];

                if (t == null)
                {
                    continue;
                }

                TypeTerrain[] faceTuile1 = tuile.TerrainSurFace((rotation + i) % 4);
                TypeTerrain[] faceTuile2 = t.TerrainSurFace((t.Rotation + i + 2) % 4);

                if (!CorrespondanceTerrains(faceTuile1, faceTuile2))
                    return false;
                if (riviere && !RiviereDansFace(faceTuile2))
                    return false;

                if (riviere)
                {
                    int currentTurn;
                    CheckDirectionRiviere(tuile, x, y, rotation, out currentTurn);
                    if (currentTurn == _lastRiverTurn && currentTurn != 0)
                    {
                        // Debug.Log("CURRENT TURN  = " + currentTurn);
                        return false;
                    }
                    else
                    {
                        // Debug.Log("PAS DE probleme avec le U de la riviere\ncurrentTurn : " + currentTurn + "\n_lastTurn" + _lastRiverTurn);
                    }
                }//Debug.Log("Correspondance : " + ((t.Rotation + i + 2) % 4));

                //Debug.Log("hello " + i + x + y + rotation);
            }
            return auMoinsUne;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="face">les 3 elements terrains d'une face</param>
        /// <returns>true si une riviere touche cette face, false sinon</returns>
        private bool RiviereDansFace(TypeTerrain[] face)
        {
            for (int i = 0; i < face.Length; i++)
            {
                if (face[i] == TypeTerrain.Riviere)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// verifie que 2 faces peuvent s'assembler
        /// </summary>
        /// <param name="t1">la face de la premiere tuile</param>
        /// <param name="t2">la face de la deuxieme tuile</param>
        /// <returns>true si les faces correspondent, false sinon</returns>
        private bool CorrespondanceTerrains(TypeTerrain[] t1, TypeTerrain[] t2)
        {
            for (int i = 0; i < 3; i++)
            {
                if (!(TerrainCompatible(t1[i], t2[2 - i])))
                    return false;
            }
            return true;
        }

        /// <summary>
        /// verfie si 2 terrains peuvent etre places cote-a-cote
        /// </summary>
        /// <param name="t1">un terrain</param>
        /// <param name="t2">l'autre terrain</param>
        /// <returns>true si les terrains sont compatible, false sinon</returns>
        private bool TerrainCompatible(TypeTerrain t1, TypeTerrain t2)
        {
            if (t1 == t2)
                return true;
            if (t1 == TypeTerrain.VilleBlason && t2 == TypeTerrain.Ville)
                return true;
            if (t1 == TypeTerrain.Ville && t2 == TypeTerrain.VilleBlason)
                return true;
            return false;
        }

        /// <summary>
        /// renvoie les tuiles adjacentes a des coordonnees
        /// </summary>
        /// <param name="x">l'abscisse de la position</param>
        /// <param name="y">l'ordonnee de la position</param>
        /// <returns>un tableau contenant 4 elements : les 4 tuiles adjacentes a l'emplacement, potentiellement nulles</returns>
        private Tuile[] TuilesAdjacentes(int x, int y)
        {
            Tuile[] resultat = new Tuile[4];

            var tab = PositionAdjacentes;

            for (int i = 0; i < 4; i++)
            {
                resultat[i] = GetTuile(x + tab[i, 0], y + tab[i, 1]);
            }
            return resultat;
        }

        //private Tuile[] TuilesAdjacentes(Tuile t)
        //{
        //    return TuilesAdjacentes(t.X, t.Y);
        //}

        /// <summary>
        /// verifie si une zone est fermee
        /// </summary>
        /// <param name="x">l'abscisse de la tuile a partir de laquelle on part</param>
        /// <param name="y">l'ordonnee de la tuile a partir de laquelle on part</param>
        /// <param name="gain">les points que chaque joueur gagne du a la fermeture de cette zone</param>
        /// <param name="zones">les couples (tuiles; idSlot) formant la zone</param>
        /// <returns>true si au moins un joueur a gagne des points, false sinon</returns>
        public bool VerifZoneFermeeTuile(int x, int y, List<PlayerScoreParam> gain, List<Zone> zones)
        {
            Tuile tl = GetTuile(x, y);
            bool point_change = false;
            for (ulong i = 0; i < (ulong)tl.NombreSlot; i++)
            {
                if (ZoneFermeeForSlot(x, y, i))
                {
                    ulong[] gagnants;
                    int point = CompteurPoints.CompterZoneFerme(x, y, (int)i, out gagnants);
                    foreach (ulong id_joueur in gagnants)
                    {
                        gain.Add(new PlayerScoreParam(id_joueur, point));
                        point_change = true;
                    }
                    Zone z = new Zone();
                    z.id_players = gagnants;
                    z.positions = new Tuple<int, int, ulong>[1];
                    z.positions[0] = new Tuple<int, int, ulong>(x, y, i);
                    zones.Add(z);
                }

            }
            return point_change;
        }

        /// <summary>
        /// verifie si une zone est ferme
        /// </summary>
        /// <param name="x">l'abscisse de la tuile de depart</param>
        /// <param name="y">l'ordonnee de la tuile de depart</param>
        /// <param name="idSlot">l'id du slot qui definie la zone</param>
        /// <returns>true si la zone est fermee, false sinon</returns>
        public bool ZoneFermeeForSlot(int x, int y, ulong idSlot)
        {
            return ZoneFermeeForSlot(GetTuile(x, y), idSlot);
        }

        /// <summary>
        /// verifie si une zone est ferme
        /// </summary>
        /// <param name="tuile">la tuile qui definie la zone</param>
        /// <param name="idSlot">l'id du slot qui definie la zone</param>
        /// <returns>true si la zone est fermee, false sinon</returns>
        public bool ZoneFermeeForSlot(Tuile tuile, ulong idSlot)
        {
            if (!_tuiles.Contains(tuile)) // ERROR
                return false;

            List<Tuile> tuilesFormantZone = new List<Tuile>();

            return ZoneFermeeAux(tuile, idSlot, tuilesFormantZone);
        }

        /// <summary>
        /// verifie recursivement si une zone est fermee
        /// </summary>
        /// <param name="tuile">la tuile courrante</param>
        /// <param name="idSlot">le slot de la tuile courrante appartenant a la zone</param>
        /// <param name="tuilesFormantZone">la liste des tuiles deja parcourue</param>
        /// <returns>true si la zone est fermee, false sinon</returns>
        private bool ZoneFermeeAux(Tuile tuile, ulong idSlot, List<Tuile> tuilesFormantZone)
        {
            bool ferme = true, emplacementVide;
            int[] positionsInternes = new int[4];
            Tuile[] tuilesAdjSlot = TuilesAdjacentesAuSlot(tuile, idSlot, out emplacementVide, out positionsInternes);

            if (emplacementVide)
            {
                return false;
            }

            int c = 0;
            foreach (var item in tuilesAdjSlot)
            {
                if (!tuilesFormantZone.Contains(item))
                {
                    tuilesFormantZone.Add(item);

                    ulong idSlotProchaineTuile = item.IdSlotFromPositionInterne(positionsInternes[c]);
                    ferme = ferme && ZoneFermeeAux(item, idSlotProchaineTuile, tuilesFormantZone);
                }
                c++;
            }

            return ferme;
        }

        /// <summary>
        /// permet d'acceder aux tuiles qui sont directement liees a un certain slot
        /// </summary>
        /// <param name="tuile">la tuile a partir de laquelle on cherche celle qui lui sont adjacentes</param>
        /// <param name="idSlot">le slot en question</param>
        /// <param name="emplacementVide"> si il le slot mene a une un emplacement vide, c'est-a-dire a une zone ouverte</param>
        /// <param name="positionsInternesProchainesTuiles"> les positions internes a partir desquelles on a accede aux tuiles retournees</param>
        /// <returns>un tableau des tuiles adjacentes</returns>
        /// <exception cref="Exception">si l'id du slot est invalide</exception>
        private Tuile[] TuilesAdjacentesAuSlot(Tuile tuile, ulong idSlot,
            out bool emplacementVide, out int[] positionsInternesProchainesTuiles)
        {
            //Debug.Log("Verif tuile " + tuile.Id.ToString() + " for slot " + idSlot.ToString());
            emplacementVide = false;
            int[] positionsInternes;
            try
            {
                positionsInternes = tuile.LienSlotPosition[idSlot];
            }
            catch (Exception e)
            {
                throw new Exception(e.Message + " (probablement la faute de Justin)");
            }
            List<int> positionsInternesProchainesTuilesTemp = new List<int>();
            List<Tuile> resultat = new List<Tuile>();
            int x = tuile.X, y = tuile.Y;

            int direction;
            foreach (int position in positionsInternes)
            {
                //direction = (position + (3 * tuile.Rotation)) / 3;
                direction = (4 + position / 3 - tuile.Rotation) % 4;

                //Debug.Log("Direction vers la prochaine Tuile de la zone = " + direction);

                Tuile elem = GetTuile(x + PositionAdjacentes[direction, 0],
                                      y + PositionAdjacentes[direction, 1]);

                if (elem == null)
                {
                    if (tuile.Id == 12)
                    {
                        //Debug.LogWarning("elem null en direction : " + direction);
                        //Debug.Log("T12 de coordonnees : " + tuile.X + "; " + tuile.Y);
                        //Debug.Log("La tuile a pour rotation : " + tuile.Rotation + ". la position pointant vers une tuile nulle est " + position);
                        //Debug.Log("le slot est le numero " + idSlot + ". Il a pour terrain : " + tuile.Slots[idSlot].Terrain);
                    }
                    emplacementVide = true;
                }

                else if (!resultat.Contains(elem))
                {
                    resultat.Add(elem);
                    var trucComplique = ((position - 3 * tuile.Rotation) + 18 + 3 * elem.Rotation) % 12;
                    switch (trucComplique % 3)
                    {
                        case 0:
                            trucComplique = (trucComplique + 2) % 12;
                            break;
                        case 2:
                            trucComplique = (trucComplique + 10) % 12;
                            break;
                        default:
                            break;
                    }
                    positionsInternesProchainesTuilesTemp.Add(trucComplique);
                    //positionsInternesProchainesTuilesTemp.Add((((direction + 2) % 4) * 3 + (position % 3) + 3 * elem.Rotation) % 12);
                }
            }
            positionsInternesProchainesTuiles = positionsInternesProchainesTuilesTemp.ToArray();

            return resultat.ToArray();
        }

        /// <summary>
        /// pose un pion sur une tuile sur un slot
        /// </summary>
        /// <param name="idJoueur">le joueur posant ce pion</param>
        /// <param name="x">l'abscisse de la tuile ou le pion est pose</param>
        /// <param name="y">l'ordonnee de la tuile ou le pion est pose</param>
        /// <param name="idSlot">l'id du slot sur lequel le pion est pose</param>
        public void PoserPion(ulong idJoueur, int x, int y, ulong idSlot)
        {
            PoserPion(idJoueur, GetTuile(x, y), idSlot);
        }

        /// <summary>
        /// pose un pion sur une tuile sur un slot
        /// </summary>
        /// <param name="idJoueur">le joueur posant ce pion</param>
        /// <param name="tuile">la tuile sur laquelle le pion est pose</param>
        /// <param name="idSlot">l'id du slot sur lequel le pion est pose</param>
        public void PoserPion(ulong idJoueur, Tuile tuile, ulong idSlot)
        {
            Debug.Log(idSlot);
            tuile.Slots[idSlot].IdJoueur = idJoueur;
            if (tuile.Slots[idSlot].Terrain == TypeTerrain.Pre)
                ChampsOuDesPionsOntEtePoses.Add((tuile.X, tuile.Y, idSlot));
        }

        /// <summary>
        /// verifie sur quels slots d'une tuile un pion est posable
        /// </summary>
        /// <param name="x">l'abscisse de la tuile en question</param>
        /// <param name="y">l'ordonnee de la tuile en question</param>
        /// <param name="id_meeple">l'id du type de meeple</param>
        /// <returns>un tableau contenant les id des slots ou un pion est posable</returns>
        public int[] EmplacementPionPossible(int x, int y/*, ulong idJoueur*/, ulong id_meeple)
        {
            return EmplacementPionPossible(x, y/*, idJoueur*/);
        }

        /// <summary>
        /// verifie sur quels slots d'une tuile un pion est posable
        /// </summary>
        /// <param name="x">l'abscisse de la tuile en question</param>
        /// <param name="y">l'ordonnee de la tuile en question</param>
        /// <returns>un tableau contenant les id des slots ou un pion est posable</returns>
        public int[] EmplacementPionPossible(int x, int y/*, ulong idJoueur*/)
        {
            //Debug.Log("Debut fonction EmplacementPionPossible avec X = " + x + " Y = " + y);
            Tuile tuile = GetTuile(x, y);
            List<int> resultat = new List<int>();
            List<(Tuile, ulong)> parcourus = new List<(Tuile, ulong)>();
            //Debug.Log("NombreSlot = " + tuile.NombreSlot);
            for (int i = 0; i < tuile.NombreSlot; i++)
            {
                //Debug.Log("LOOKING SLOT " + i);
                if (!ZoneAppartientAutreJoueur(x, y, (ulong)i, /*idJoueur,*/ parcourus))
                    resultat.Add(i);
                parcourus.Clear();
            }

            return resultat.ToArray();
        }

        /// <summary>
        /// verifie si une zone est deja occupee
        /// </summary>
        /// <param name="x">l'abscisse de la tuile ou commence la zone</param>
        /// <param name="y">l'ordonnee de la tuile ou commence la zone</param>
        /// <param name="idSlot">l'id du slot ou commence la zone</param>
        /// <param name="parcourus">liste des tuples (tuile, slot) deja parcourus/traites</param>
        /// <returns>true si la zone est deja occupee</returns>
        private bool ZoneAppartientAutreJoueur(int x, int y, ulong idSlot, /*ulong idJoueur,*/ List<(Tuile, ulong)> parcourus)
        {
            //Debug.Log("debut methode ZoneAppartientAutreJoueur avec x=" + x + " y=" + y + " idslot=" + idSlot + " idJoueur=" + idJoueur
            //+ " liste des tuiles parcourues de longeur: " + parcourus.Count);
            Tuile tl_ref = GetTuile(x, y);
            //Debug.Log("READING (" + tl_ref.Id + ") " + x + ", " + y + ", " + tl_ref.Rotation + " :" + idSlot + " : " + tl_ref.Slots[idSlot].Terrain);
            bool vide, resultat = false;
            int[] positionsInternesProchainesTuiles;
            Tuile[] adj = TuilesAdjacentesAuSlot(tl_ref, idSlot, out vide, out positionsInternesProchainesTuiles);

            //Debug.Log("methode TuilesAdjacentesAuSlot appelee, adj de longueur: " + adj.Length);
            foreach (var item in adj)
            {
                //Debug.Log("Tuile dans adj :" + item.ToString());
            }

            if (adj.Length == 0)
                return false;
            int c = -1;
            foreach (var t in adj)
            {
                c++;
                if (t == null || parcourus.Contains((t, idSlot)))
                {
                    continue;
                }
                parcourus.Add((t, idSlot));

                int pos = positionsInternesProchainesTuiles[c];
                ulong nextSlot = t.IdSlotFromPositionInterne(pos);
                ulong idJ = t.Slots[nextSlot].IdJoueur;

                //Debug.Log("Verification sur " + t.ToString() + ". idSlot : " + nextSlot + " " + t.Slots.ToString());

                if (idJ != ulong.MaxValue)
                {
                    //Debug.Log("Zone " + x + ", " + y + ", " + idSlot + " appartient à " + idJ);
                    return true;
                }
                //Debug.Log("FROM " + x + ", " + y + ", " + idSlot + " to " + t.X + ", " + t.Y + ", " + nextSlot);
                resultat = resultat || ZoneAppartientAutreJoueur(t.X, t.Y, nextSlot, /*idJoueur,*/ parcourus);
                if (resultat)
                    return resultat;
            }

            return resultat;
        }

        /// <summary>
        /// verifie qu'un joueur a le droit de poser un meeple sur un certain slot
        /// </summary>
        /// <param name="x">l'abscisse de la tuile sur laquelle le joueur veut poser un meeple</param>
        /// <param name="y">l'ordonnee de la tuile sur laquelle le joueur veut poser un meeple</param>
        /// <param name="idSlot">l'id du slot sur lequel le joueur veut poser un meeple</param>
        /// <param name="idJoueur">l'id du joueur</param>
        /// <param name="idMeeple">l'id du type de meeple</param>
        /// <returns>true si le pion est posable, false sinon</returns>
        public bool PionPosable(int x, int y, ulong idSlot, ulong idJoueur, ulong idMeeple)
        {
            if (idSlot > 12)
                return false;
            Tuile tuile = GetTuile(x, y);

            // Debug.Log("LE PION EST IL POSABLE SUR LA TUILE " + tuile.ToString() + " SLOT :" + idSlot + " ?");

            if (tuile == null || (ulong)tuile.NombreSlot < idSlot)
                return false;

            int[] tab = EmplacementPionPossible(x, y/*, idJoueur*/);
            for (int i = 0; i < tab.Length; i++)
            {
                if ((ulong)tab[i] == idSlot)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Enleve les pions d'une zone du plateau et les rend aux joueurs a qui ils appartiennent
        /// </summary>
        /// <param name="x">l'abscisse ou la tuile a partir de laquelle commence la zone se trouve</param>
        /// <param name="y">l'ordonnee ou la tuile a partir de laquelle commence la zone se trouve</param>
        /// <param name="idSlot">le slot de la tuile ou la zone commence</param>
        /// <returns>en clefs l'id des joueurs a qui rendre les pions. En valeur, combien de slot leur rendre</returns>
        public Dictionary<ulong, int> RemoveAllPawnInZone(int x, int y, ulong idSlot)
        {
            List<(Tuile, ulong)> parcourues = new List<(Tuile, ulong)>();
            var tuile = GetTuile(x, y);
            var result = new Dictionary<ulong, int>();
            if (tuile.Slots[idSlot].Terrain == TypeTerrain.Pre)
                return result;
            RemoveAllPawnInZoneAux(tuile, idSlot, parcourues, ref result);
            return result;
        }

        /// <summary>
        /// Enleve les pions d'une zone du plateau et les rend aux joueurs a qui ils appartiennent
        /// </summary>
        /// <param name="tuile">la tuile sur laquelle la zone commence</param>
        /// <param name="idSlot">le slot de la tuile ou la zone commence</param>
        /// <param name="parcourues">la list des tuples (tuiles, slot) etant deja traites</param>
        /// <param name="result">en clefs l'id des joueurs a qui rendre les pions. En valeur, combien de slot leur rendre</param>
        /// <returns>en clefs l'id des joueurs a qui rendre les pions. En valeur, combien de slot leur rendre</returns>
        private Dictionary<ulong, int> RemoveAllPawnInZoneAux(Tuile tuile, ulong idSlot,
            List<(Tuile, ulong)> parcourues, ref Dictionary<ulong, int> result)
        {
            bool vide;
            int[] positionsInternesProchainesTuiles;
            parcourues.Add((tuile, idSlot));
            Tuile[] adj = TuilesAdjacentesAuSlot(tuile, idSlot, out vide, out positionsInternesProchainesTuiles);

            ulong idCurrentJoueur = tuile.Slots[idSlot].IdJoueur;

            if (idCurrentJoueur != ulong.MaxValue)
            {
                if (result.ContainsKey(idCurrentJoueur))
                    result[idCurrentJoueur] += 1;
                else
                    result.Add(idCurrentJoueur, 1);
            }

            tuile.Slots[idSlot].IdJoueur = ulong.MaxValue;
            for (int i = 0; i < adj.Length; i++)
            {
                Tuile currentTuile = adj[i];
                if (currentTuile == null)
                    continue;
                ulong nextSlot = currentTuile.IdSlotFromPositionInterne(positionsInternesProchainesTuiles[i]);
                if (parcourues.Contains((currentTuile, nextSlot)))
                    continue;
                RemoveAllPawnInZoneAux(currentTuile,
                    nextSlot, parcourues, ref result);
            }
            return result;
        }



        private void RemoveAllPawnInTuileAux(Tuile tuile, ulong idSlot,
            List<(Tuile, ulong)> parcourues, List<Tuple<int, int, ulong>> results)
        {
            bool vide;
            int[] positionsInternesProchainesTuiles;
            parcourues.Add((tuile, idSlot));
            Tuile[] adj = TuilesAdjacentesAuSlot(tuile, idSlot, out vide, out positionsInternesProchainesTuiles);

            ulong idCurrentJoueur = tuile.Slots[idSlot].IdJoueur;

            if (idCurrentJoueur != ulong.MaxValue)
            {
                results.Add(new Tuple<int, int, ulong>(tuile.X, tuile.Y, idSlot));
            }
            for (int i = 0; i < adj.Length; i++)
            {
                Tuile currentTuile = adj[i];
                if (currentTuile == null)
                    continue;
                ulong nextSlot = currentTuile.IdSlotFromPositionInterne(positionsInternesProchainesTuiles[i]);
                if (parcourues.Contains((currentTuile, nextSlot)))
                    continue;
                RemoveAllPawnInTuileAux(currentTuile,
                    nextSlot, parcourues, results);
            }
        }

        public Dictionary<ulong, int> RemoveAllPawnInTile(int x, int y, List<Tuple<int, int, ulong>> positions)
        {
            Dictionary<ulong, int> result = new Dictionary<ulong, int>();
            List<(Tuile, ulong)> parcourues = new List<(Tuile, ulong)>();
            var tuile = GetTuile(x, y);
            for (int slot = 0; slot < tuile.NombreSlot; slot++)
            {
                if (tuile.Slots[slot].Terrain == TypeTerrain.Pre || !ZoneFermeeForSlot(x, y, (ulong)slot) ||
                    (CompteurPoints.PointAbbaye(tuile) == 0 && tuile.Slots[slot].Terrain == TypeTerrain.Abbaye))
                {
                    continue;
                }
                parcourues.Clear();
                RemoveAllPawnInTuileAux(tuile, (ulong)slot, parcourues, positions);
                parcourues.Clear();
                RemoveAllPawnInZoneAux(tuile, (ulong)slot, parcourues, ref result);
            }
            return result;
        }
    }
}