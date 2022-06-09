using System.Collections.Generic;

namespace Assets.system
{
    public class Slot
    {
        /// <summary>
        /// quel terrain represente ce slot
        /// </summary>
        private readonly TypeTerrain _terrain;

        /// <summary>
        /// a quels autres slots est connectes ce slots
        /// </summary>
        private readonly ulong[] _linkOtherSlots;

        /// <summary>
        /// a quels autres slots est connectes ce slots
        /// </summary>
        public ulong[] LinkOtherSlots => _linkOtherSlots;

        /// <summary>
        /// quel terrain represente ce slot
        /// </summary>
        public TypeTerrain Terrain => _terrain;

        /// <summary>
        /// quel joueur occupe ce slot
        /// </summary>
        public ulong IdJoueur { get; set; }

        /// <summary>
        /// Type de terrain en fonction de leur id
        /// </summary>
        private static Dictionary<ulong, TypeTerrain> TerrainFromId;

        /// <summary>
        /// instancie un slot
        /// </summary>
        /// <param name="terrain">quel terrain pour ce slot</param>
        /// <param name="link">slots adjacents</param>
        public Slot(TypeTerrain terrain, ulong[] link)
        {
            _linkOtherSlots = link;
            _terrain = terrain;
            IdJoueur = ulong.MaxValue;
        }

        /// <summary>
        /// instancie un slot
        /// </summary>
        /// <param name="idTerrain">terrain du slot</param>
        /// <param name="link">slots adjacents</param>
        public Slot(ulong idTerrain, ulong[] link)
        {
            _linkOtherSlots = link;
            _terrain = TerrainFromId[idTerrain];
            IdJoueur = ulong.MaxValue;
        }

        /// <summary>
        /// instancie un slot
        /// </summary>
        /// <param name="idTerrain">terrain du slot</param>
        public Slot(ulong idTerrain) { }

        /// <summary>
        /// override pour facilite le deboggage
        /// </summary>
        /// <returns>"Slot appartenant au joueur: " + IdJoueur</returns>
        public override string ToString()
        {
            return "Slot appartenant au joueur: " + IdJoueur;
        }

        /// <summary>
        /// instancie un tableau de slot a partir d'un modele
        /// </summary>
        /// <param name="src">modele</param>
        /// <returns>le tableau instancie</returns>
        public static Slot[] CoypArray(Slot[] src)
        {
            Slot[] result = new Slot[src.Length];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = new Slot(src[i].Terrain, src[i].LinkOtherSlots);
            }
            return result;
        }

        /// <summary>
        /// instancie TerrainFromId
        /// </summary>
        static Slot()
        {
            TerrainFromId = new Dictionary<ulong, TypeTerrain>
            {
                { 0, TypeTerrain.Ville },
                { 1, TypeTerrain.Route },
                { 2, TypeTerrain.Pre },
                { 3, TypeTerrain.Abbaye },
                { 4, TypeTerrain.Auberge },
                { 5, TypeTerrain.Cathedrale }
            };
        }
    }
}
