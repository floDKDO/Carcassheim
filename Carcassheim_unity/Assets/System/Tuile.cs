using System.Collections.Generic;
using UnityEngine;
using System;

namespace Assets.system
{
    public partial class Tuile : Comparer<Tuile>
    {
        /// <summary>
        /// Les slots constituants la tuile
        /// </summary>
        private readonly Slot[] _slots;

        /// <summary>
        /// le nombre de slot qu'a la tuile
        /// </summary>
        private readonly int _nombreSlot;

        /// <summary>
        /// tableau qui lie chacun des slots a leur positions internes
        /// </summary>
        private readonly int[][] _lienSlotPosition;

        /// <summary>
        /// l'id de la tuile
        /// </summary>
        private readonly ulong _id;

        /// <summary>
        /// 
        /// </summary>
        private readonly int[,] _lienEntreSlots;

        /// <summary>
        /// une tuile fantome est une tuile ephemere, qui n'intervient pas lors du calcul de placement legal
        /// </summary>
        public bool TuileFantome { get; set; } = false;

        /// <summary>
        /// vaut true si la tuile est une tuile utilisee uniquement pour former la riviere en debut de partie
        /// (si l'option riviere est active)
        /// </summary>
        public bool Riviere
        {
            get
            {
                foreach (var item in _slots)
                {
                    if (item.Terrain == TypeTerrain.Riviere)
                        return true;
                }
                return false;
            }
        }

        /// <summary>
        /// l'id de la tuile
        /// </summary>
        public ulong Id => _id;

        /// <summary>
        /// les slots de la tuile
        /// </summary>
        public Slot[] Slots => _slots;

        /// <summary>
        /// 
        /// </summary>
        public int[][] LienSlotPosition => _lienSlotPosition;

        /// <summary>
        /// le nombre de slot qu'a une tuile
        /// </summary>
        public int NombreSlot => _nombreSlot;

        /// <summary>
        /// la probabilite qu'une tuile a d'etre piochee
        /// </summary>
        private readonly int _proba;

        /// <summary>
        /// la probabilite qu'une tuile a d'etre piochee
        /// </summary>
        public int Pobabilite => _proba;

        /// <summary>
        /// l'abscisse de la tuile une fois qu'elle a ete posee
        /// </summary>
        public int X { get; set; }

        /// <summary>
        /// l'ordonnee de la tuile une fois qu'elle a ete posee
        /// </summary>
        public int Y { get; set; }

        /// <summary>
        /// quelle face de la tuile pointe vers le nord
        /// </summary>
        public int Rotation { get; set; }

        /// <summary>
        /// les clefs de ce dictionnaire sont les 12 points cardinales, les valeurs sont l'id de la position interne correspondante
        /// </summary>
        public static Dictionary<string, int> PointsCardPos = new Dictionary<string, int>()
        {
            {"NNO", 0},
            {"N", 1},
            {"NNE", 2},
            {"NEE", 3},
            {"E", 4},
            {"SEE", 5},
            {"SSE", 6},
            {"S", 7},
            {"SSO", 8},
            {"SOO", 9},
            {"O", 10},
            {"NOO", 11}
        };

        /// <summary>
        /// permet d'acceder a une tuile en fonction de son id
        /// </summary>
        public static Dictionary<ulong, Tuile> DicoTuiles { get; set; }

        /// <summary>
        /// instancie une tuile
        /// </summary>
        /// <param name="id">l'id de la tuile</param>
        /// <param name="nombreSlot">le nombre de slot de la tuile</param>
        /// <param name="lien">les liens entre slots et positions internes</param>
        /// <param name="terrains">les terrains des differents slots</param>
        public Tuile(ulong id, int nombreSlot, int[][] lien, TypeTerrain[] terrains)
        {
            _nombreSlot = nombreSlot;
            _slots = new Slot[nombreSlot];

            int s = 0;
            for (int i = 0; i < nombreSlot; i++)
            {
                _slots[i] = new Slot(terrains[i], new ulong[0]);
                s += lien[i].Length;
            }
            if (nombreSlot != terrains.Length || lien.Length != nombreSlot || s != 12)
                Debug.Log("Erreur tuile d'id: " + id);


            _lienSlotPosition = lien;
        }

        /// <summary>
        /// instancie une tuile
        /// </summary>
        /// <param name="id">l'id de la tuile</param>
        /// <param name="lien">les liens entre slots et positions internes</param>
        /// <param name="lienEntreSlots">quels terrains sont adjacents a quels autres</param>
        /// <param name="slots">les slots de la tuile</param>
        public Tuile(ulong id, Slot[] slots, int[][] lien, int[,] lienEntreSlots = null) : this(id, slots, lien)
        {
            if (lienEntreSlots != null)
                _lienEntreSlots = lienEntreSlots;
        }

        /// <summary>
        /// instancie une tuile
        /// </summary>
        /// <param name="id">l'id de la tuile</param>
        /// <param name="slots">les slots de la tuile</param>
        /// <param name="lien">les liens entre slots et positions internes</param>
        public Tuile(ulong id, Slot[] slots, int[][] lien)
        {
            _nombreSlot = slots.Length;
            _id = id;
            _slots = slots;

            int[][] actualLink = new int[_nombreSlot][];

            for (int i = 0; i < actualLink.Length; i++)
            {
                var temp = new List<int>();
                int[] tab = lien[i];

                for (int j = 0; j < tab.Length; j++)
                {
                    if (tab[j] == -1)
                    {
                        break;
                    }
                    temp.Add(tab[j]);
                }
                actualLink[i] = temp.ToArray();
            }

            _lienSlotPosition = actualLink;
        }

        /// <summary>
        /// instancie une nouvelle tuile a partir d'un modele ayant une differente addresse en memoire que son modele
        /// </summary>
        /// <param name="tuile">modele</param>
        /// <returns>la nouvelle tuile</returns>
        public static Tuile Copy(Tuile tuile)
        {

            Tuile result = new Tuile(tuile._id, Slot.CoypArray(tuile._slots), tuile._lienSlotPosition, tuile._lienEntreSlots);
            return result;
        }

        /// <summary>
        /// permet de recuperer un slot en fonction d'une position interne
        /// </summary>
        /// <param name="pos">la position interne a partir de laquelle l'on cherche le slot (compris entre 0 et 11)</param>
        /// <returns>l'id du slot</returns>
        public ulong IdSlotFromPositionInterne(int pos)
        {
            for (ulong i = 0; i < (ulong)_nombreSlot; i++)
            {
                foreach (int p in _lienSlotPosition[i])
                {
                    if (p == pos)
                        return i;
                }
            }
            return 0;
        }
        /*
            public TypeTerrain[] TerrainSurFace(int rot)
            {
                TypeTerrain[] resultat = new TypeTerrain[3];

                int[] positionInterneRecherchee = new int[3];
                for (int i = 0; i < 3; i++)
                {
                    positionInterneRecherchee[i] = rot * 3 + i;
                }

                int compteur = 0;
                for (int i = 0; i < _nombreSlot; i++)
                {
                    foreach (int position in _lienSlotPosition[i])
                    {
                        if (position == positionInterneRecherchee[0] ||
                            position == positionInterneRecherchee[1] ||
                            position == positionInterneRecherchee[2])
                        {
                            resultat[compteur++] = _slots[i].Terrain;

                            if (X == Y)
                                Debug.Log(position);
                        }
                    }
                }

                return resultat;
            }*/

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rot">Direction de la tuile</param>
        /// <returns>les 3 terrains des positions internes ordonnes dans le sens horraire</returns>
        public TypeTerrain[] TerrainSurFace(int rot)
        {
            TypeTerrain[] resultat = new TypeTerrain[3];

            int positionInterneRecherchee;
            for (int i = 0; i < 3; i++)
            {
                positionInterneRecherchee = rot * 3 + i;

                ulong idSlot = IdSlotFromPositionInterne(positionInterneRecherchee);
                resultat[i] = _slots[idSlot].Terrain;
            }

            return resultat;
        }

        static Tuile()
        {
            DicoTuiles = new Dictionary<ulong, Tuile>();
        }

        //public static implicit operator Tuile(ulong id) => DicoTuiles[id];

        /// <summary>
        /// informations utiles sur la tuile utilisees lors du deboggage
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "Tuile d'id : " + _id + " de position : (" + X + ", " + Y + ") R : " + Rotation;
        }

        /// <summary>
        /// vaut true si la tuile est une tuile utilisee uniquement pour former la riviere en debut de partie
        /// (si l'option riviere est active)
        /// </summary>
        /// <returns></returns>
        public bool isARiver()
        {
            foreach (Slot s in _slots)
            {
                if (s.Terrain == TypeTerrain.Riviere)
                    return true;
            }
            return false;
        }

        public override int Compare(Tuile t1, Tuile t2)
        {
            if (t1.X < t2.X)
                return -1;
            if (t1.X > t2.X)
                return 1;

            if (t1.Y < t2.Y)
                return -1;
            if (t1.Y > t2.Y)
                return 1;

            return 0;
        }
    }

}
