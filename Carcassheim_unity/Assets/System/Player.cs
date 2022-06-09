using System.Net.Sockets;
using System.Threading;

namespace Assert.system
{
    /// <summary>
    ///     Information du joueur
    /// </summary>
    public class Player
    {
        /// <summary>
        ///     Id du joueur
        /// </summary>
        public ulong id { get; }

        /// <summary>
        ///     Nom du joueur
        /// </summary>
        public string name { get; set; }

        /// <summary>
        ///     Nombre de meeple du joueur
        /// </summary>
        public uint nbMeeples { get; set; }

        /// <summary>
        ///     score du joueur
        /// </summary>
        public uint score { get; set; }

        /// <summary>
        ///     status du joueur
        /// </summary>
        public bool status { get; set; }

        /// <summary>
        ///     récupération des nouvelles informations du joueur
        /// </summary>
        /// <param name="player_id">Id du joueur</param>
        /// <param name="player_name">Nom du joueur</param>
        /// <param name="player_nbMeeples">Nombre de meeples du joueur</param>
        /// <param name="player_score">Score du joueur</param>
        public Player(ulong player_id,string player_name,uint player_nbMeeples,uint player_score)
        {
            id = player_id;
            name = player_name;
            nbMeeples = player_nbMeeples;
            score = player_score;
            status = true;
        }

        /// <summary>
        ///     récupération des nouvelles informations du joueur
        /// </summary>
        /// <param name="player_id">Id du joueur</param>
        /// <param name="player_name">Nom du joueur</param>
        /// <param name="player_status">Statut du joueur</param>
        public Player(ulong player_id, string player_name, bool player_status)
        {
            id = player_id;
            name = player_name;
            nbMeeples = 0;
            score = 0;
            status = player_status;
        }
    }
}
