using System;
using System.Collections.Generic;
using ClassLibrary;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Timers;

using Server;

namespace system
{
/// <summary>
///     Object used when a user create a game, on a new thread that will be the server of this game
/// </summary>

    public partial class Thread_serveur_jeu
    {

        /// <summary>
        ///     The concerned game id.
        /// </summary>
        private readonly int _id_partie;

        /// <summary>
        ///     The players dictionary, Keys are ids and Values are players.
        /// </summary>
        private Dictionary<ulong, Player> _dico_joueur;

        /// <summary>
        ///     semaphore added on the players dictionary.
        /// </summary>
        private Semaphore _s_dico_joueur;

        /// <summary>
        ///     actual players number.
        /// </summary>
        private uint _nombre_joueur;

        /// <summary>
        ///     maximum players number.
        /// </summary>
        private uint _nombre_joueur_max;

        /// <summary>
        ///     Admin's id.
        /// </summary>
        private ulong _id_moderateur;

        /// <summary>
        ///     Game status.
        /// </summary>
        private Tools.GameStatus _statut_partie;

        /// <summary>
        ///     Game mode, 0 when classic, 1 when Time-Attack and 2 when score.
        /// </summary>
        private Tools.Mode _mode; 

        /// <summary>
        ///     Tiles number.
        /// </summary>
        private int _nb_tuiles;

        /// <summary>
        ///     Score to get, in case of score mode.
        /// </summary>
        private int _score_max;

        /// <summary>
        ///     private game, if True, the game is private, else the game is public.
        /// </summary>
        private bool _privee;

        /// <summary>
        ///     Number of meeples per player.
        /// </summary>
        private Tools.Meeple _meeples; // Nombre de meeples par joueur
        
        /// <summary>
        ///     The game timer in case that the game mode is set to Time-attack.
        /// </summary>
        private DateTime _DateTime_game;
        private System.Timers.Timer _timer_game;
        private Tools.Timer _timer_game_value;

        /// <summary>
        ///     The round timer, for each round the player has to finish before timer expiration.
        /// </summary>
        public DateTime _DateTime_player { get; set; }
        public System.Timers.Timer _timer_player;
        private Tools.Timer _timer_player_value; // En secondes
        
        /// <summary>
        ///     The room timer, which is not activ anymore if it expires. 
        /// </summary>
        public DateTime _DateTime_room { get; set; }
        public System.Timers.Timer _timer_room;

        /// <summary>
        ///     Initial tile's id, either the path or the river, depends on the dlc
        /// </summary>
        private ulong _idTuileInit; 

        
        /// <summary>
        ///     settings lock.
        /// </summary>
        private readonly object _lock_settings;

       
        /// <summary>
        ///     Plateau (game board)
        /// </summary>
        private Plateau _plateau;

        /// <summary>
        ///     The actual player's offset.
        /// </summary>
        private uint _offsetActualPlayer; // The offset of the actual player in the _dico_joueur 

        /// <summary>
        ///     The game tiles list.
        /// </summary>
        private List<ulong> _tuilesGame; // Totalité des tuiles de la game

        /// <summary>
        ///     In case that the river extention is on, when river generated at the begginig, it's tiles is stored in this list.
        /// </summary>
        private List<ulong> _rivieresGame; // Totalité des tuiles de la game 

        /// <summary>
        ///     List containes the three tiles sent by server to client, at the bigging of each round.
        /// </summary>      
        private string[] _tuilesEnvoyees; 

        /// <summary>
        ///     Chosen tile's id.
        /// </summary>      
        private ulong _idTuileChoisie; // L'id de la tuile choisie par le client parmis les 3 envoyées
        
        /// <summary>
        ///     Temporary position of the tile of actual round.
        /// </summary>      
        private Position _posTuileTourActu;

        /// <summary>
        ///     Temporary position of the pion of actual round, to cast : {idPlayer, posTuile X, posTuile Y, idMeeple, idSlot}.
        /// </summary>      
        private string[] _posPionTourActu;

        /// <summary>
        ///    Extention chosen for this game.
        /// </summary>  
        private int _extensionsGame; 


        /// <summary>
        ///     Anti-chreat : The first valid tile's id, when three tiles sent to client, the id of the first valid one.
        /// </summary>  
        private ulong _AC_idFirstValidTile;

        /// <summary>
        ///     Anti-chreat : Semaphore on the first valid tile's id attribut, to protect access.
        /// </summary>  
        private Semaphore _s_AC_idFirstValidTile;

        /// <summary>
        ///     Anti-chreat : .
        /// </summary>  
        private bool _AC_drawedTilesValid;

        /// <summary>
        ///     Anti-chreat : Barrier used in waiting all users to verify the validity of one or many tiles.
        /// </summary>  
        private Barrier _AC_barrierAllVerifDone;

        /// <summary>
        ///     Anti-chreat : True if the barrier's up, false if not.
        /// </summary>  
        private bool _AC_barrierUp; 
        
        /// <summary>
        ///     Anti-chreat : Semaphore on _AC_barrierUp to protect acess.
        /// </summary>  
        private Semaphore _s_AC_barrierUp;

        /// <summary>
        ///     Anti-chreat : Semaphore on _s_nombre_joueur to protect acess.
        /// </summary>
        private Semaphore _s_nombre_joueur;

        /// <summary>
        ///     Anti-chreat : Semaphore on _s_plateau to protect acess.
        /// </summary>
        private Semaphore _s_plateau;

        /// <summary>
        ///     Anti-chreat : Semaphore on _s_offsetActualPlayer to protect acess.
        /// </summary>
        private Semaphore _s_offsetActualPlayer;

        /// <summary>
        ///     Anti-chreat : Semaphore on _s_tuilesGame to protect acess.
        /// </summary>
        private Semaphore _s_tuilesGame;

        /// <summary>
        ///     Semaphore des tuiles de rivière de la game
        /// </summary>
        private Semaphore _s_rivieresGame;

        /// <summary>
        ///     Anti-chreat : Semaphore on _s_tuilesEnvoyees to protect acess.
        /// </summary>
        private Semaphore _s_tuilesEnvoyees;

        /// <summary>
        ///     Anti-chreat : Semaphore on _s_posTuileTourActu to protect acess.
        /// </summary>
        private Semaphore _s_posTuileTourActu;

        /// <summary>
        ///     Anti-chreat : Semaphore on _s_posPionTourActu to protect acess.
        /// </summary>
        private Semaphore _s_posPionTourActu; 

        /// <summary>
        ///     Getter : Get the player dictionary.
        /// </summary>
        public Dictionary<ulong, Player> Get_Dico_Joueurs()
        {
            return this._dico_joueur;
        }

        /// <summary>
        ///     Getter : Get the game id.
        /// </summary>
        public int Get_ID()
        {
            return this._id_partie;
        }

        /// <summary>
        ///     Getter : Get the game mode.
        /// </summary>
        public Tools.Mode Get_Mode()
        {
            return this._mode;
        }

        /// <summary>
        ///     Getter : Get the admin player id.
        /// </summary>
        public ulong Get_Moderateur()
        {
            return this._id_moderateur;
        }
        
        /// <summary>
        ///     Getter : Get the actual player id.
        /// </summary>
        public ulong Get_ActualPlayerId()
        {
            _s_dico_joueur.WaitOne();
            ulong[] idPlayer_array = _dico_joueur.Keys.ToArray();
            _s_dico_joueur.Release();
            Console.WriteLine("Get_ActualPlayerId : " + _offsetActualPlayer);
            return idPlayer_array[_offsetActualPlayer];
        }

        public ulong Get_PlayerMeeples(ulong idPlayer)
        {
            _s_dico_joueur.WaitOne();
            var playerMeeples = _dico_joueur[idPlayer]._nbMeeples;
            _s_dico_joueur.Release();
            return playerMeeples;
        }

        /// <summary>
        ///     Getter : Get the game status.
        /// </summary>
        public Tools.GameStatus Get_Status()
        {
            return this._statut_partie;
        }

        /// <summary>
        ///     Getter : Get the value of _privee to define if the game is privat or public.
        /// </summary>
        public bool Is_Private()
        {
            return this._privee;
        }

        /// <summary>
        ///     Getter : Get _AC_drawedTilesValid.
        /// </summary>
        public bool Get_AC_drawedTilesValid()
        {
            return _AC_drawedTilesValid;
        }

        /// <summary>
        ///     Setter : Pass the attribute indicating if a tile is valid to true and stores the id of the first valid tile if prior to the actual one
        /// </summary>
        /// <param name="idTuile"></param>
        public void SetValid_AC_drawedTilesValid(ulong idTuile)
        {
            Console.WriteLine("** DEBUG : MaJ idFirstValid -> new prop = " + idTuile.ToString() + " | actual = " + _AC_idFirstValidTile.ToString());
            if(_AC_drawedTilesValid != true)
            {
                _AC_drawedTilesValid = true;
            }

            _s_tuilesEnvoyees.WaitOne();
            // DEBUG affichage _tuilesEnvoyees
            Console.WriteLine("** DEBUG : MaJ idFirstValid _tuilesEnvoyees : [ ");
            foreach (var elem in _tuilesEnvoyees)
            {
                Console.Write(elem + ", ");
            }
            Console.Write("] \n");
            _s_tuilesEnvoyees.Release();


            _s_AC_idFirstValidTile.WaitOne();
            for(int i = 0; i<3; i++)
            {
                if(idTuile == UInt64.Parse(_tuilesEnvoyees[i])) // Si l'idTuile arrive avant l'idFirstValid déjà stocké
                {
                    _AC_idFirstValidTile = idTuile;
                    Console.WriteLine("*** DEBUG : MaJ SetValide_AC -> nouv idFirstValid = " + idTuile.ToString());
                    break;
                }
                if(_AC_idFirstValidTile == UInt64.Parse(_tuilesEnvoyees[i])) // Si l'idFirstValid arrive avant le nouveau idTuile
                {
                    Console.WriteLine("*** DEBUG : NO MaJ SetValide_AC -> idFirstValid = " + _tuilesEnvoyees[i] + " has been found first.");
                    break;
                }
            }
            _s_AC_idFirstValidTile.Release();
            
        }

        /// <summary>
        ///     Getter : Get the first valid tile's id (_AC_idFirstValidTile).
        /// </summary>
        public ulong Get_AC_idFirstValidTile()
        {
            return _AC_idFirstValidTile;
        }

        /// <summary>
        ///     Getter : Get value of (_AC_barrierUp).
        /// </summary>
        public bool Get_AC_barrierUp()
        {
            return _AC_barrierUp;
        }

        /// <summary>
        ///     Getter : Get actual position of the round's tile (_posTuileTourActu).
        /// </summary>
        public Position Get_posTuileTourActu()
        {
            return _posTuileTourActu;
        }

        /// <summary>
        ///     Getter : Get actual position of the round's pion.
        /// </summary>
        public string[] Get_posPionTourActu()
        {
            string[] returnString;
            if (_posPionTourActu.Length > 0)
            {
                returnString = new string[5];
                Array.Copy(_posPionTourActu, returnString, 5);
            }
            else
            {
                returnString = Array.Empty<string>();
            }
                
            return returnString;
        }

        /// <summary>
        ///     Setter : Set the chosen tile's id (_idTuileChoisie).
        /// </summary>
        ///<param name="idTuile">the id of the chosen tile.</param>
        public void Set_idTuileChoisie(ulong idTuile)
        {
            this._idTuileChoisie = idTuile;
        }


        
        /// <summary>
        ///     Setter : Set the three picked tiles list  (_tuilesEnvoyees).
        /// </summary>
        ///<param name="tuilesEnvoyees">the list of the three picked tiles .</param>
        public void Set_tuilesEnvoyees(string[] tuilesEnvoyees)
        {
            _s_tuilesEnvoyees.WaitOne();
            _tuilesEnvoyees = new string[tuilesEnvoyees.Length];
            Array.Copy(tuilesEnvoyees, _tuilesEnvoyees, tuilesEnvoyees.Length);
            _s_tuilesEnvoyees.Release();
        }


        /// <summary>
        ///     Getter : Get id of the chosen tile (_idTuileChoisie).
        /// </summary>
        public ulong Get_idTuileChoisie()
        {
            return _idTuileChoisie;
        }


        // <summary>
        ///     Getter : Get winner id, max of player.Value._score from the _s_dico_joueur
        /// </summary>
        public ulong GetWinner()
        {
            uint scoreMax = 0;
            ulong winner = _id_moderateur;

            _s_dico_joueur.WaitOne();
            foreach(var player in _dico_joueur)
            {
                if(player.Value._score > scoreMax)
                {
                    winner = player.Key;
                    scoreMax = player.Value._score;
                }
            }
            _s_dico_joueur.Release();

            return winner;
        }


        // <summary>
        ///     Getter : Get id of initial tile (_idTuileInit).
        /// </summary>
        public ulong Get_idTuileInit()
        {
            return this._idTuileInit;
        }

        public List<ulong> Get_rivieresGame()
        {
            List<ulong> listReturn = _rivieresGame;
            return listReturn;
        }

        // <summary>
        ///     Getter : Get number of players.
        /// </summary>
        public uint NbJoueurs
        {
            get { return this._nombre_joueur; }
        }


        // <summary>
        ///     Getter : Get max number of players.
        /// </summary>
        public uint NbJoueursMax
        {
            get { return this._nombre_joueur_max; }
        }

        /// <summary>
        /// Settings of that game
        /// </summary>
        /// <returns> 
        /// Returns a string[] in that order: { id_moderator, player_number, player_number_max,
        /// private, mode, number_tiles, number_pawn, timer, timer_max_player }.
        /// </returns>
        public string[] Get_Settings()
        {
            List<string> settingsList = new List<string>();

            settingsList.Add(_id_moderateur.ToString());
            settingsList.Add(_nombre_joueur.ToString());
            settingsList.Add(_nombre_joueur_max.ToString());
            settingsList.Add(_privee.ToString());
            settingsList.Add(((int)_mode).ToString());
            settingsList.Add(_nb_tuiles.ToString());
            settingsList.Add(((int)_meeples).ToString());
            settingsList.Add(((int)_timer_game_value).ToString());
            settingsList.Add(((int)_timer_player_value).ToString());
            settingsList.Add(_score_max.ToString());
            settingsList.Add(_extensionsGame.ToString());

            return settingsList.ToArray();
        }

        /// <summary>
        /// Sets the settings of the game
        /// </summary>
        /// <param name="idPlayer"> Id of the player making the request </param>
        /// <param name="settings"> The new set of settings (in the same order than Get_settings, without the 2 firsts) </param>
        public void Set_Settings(ulong idPlayer, string[] settings)
        {
            if (idPlayer == Get_Moderateur() && _statut_partie == Tools.GameStatus.Room)
            {
                lock (_lock_settings)
                {
                    try
                    {
                        _nombre_joueur_max = Convert.ToUInt32(settings[0]);
                        _privee = bool.Parse(settings[1]);

                        switch (Int32.Parse(settings[2]))
                        {
                            case 0:
                                _mode = Tools.Mode.Default;
                                break;
                            case 1:
                                _mode = Tools.Mode.TimeAttack;
                                break;
                            case 2:
                                _mode = Tools.Mode.Score;
                                break;
                            default:
                                _mode = Tools.Mode.Default;
                                break;
                        }

                        _nb_tuiles = int.Parse(settings[3]);

                        switch (int.Parse(settings[4]))
                        {
                            case 4:
                                _meeples = Tools.Meeple.Quatre;
                                break;
                            case 8:
                                _meeples = Tools.Meeple.Huit;
                                break;
                            case 10:
                                _meeples = Tools.Meeple.Dix;
                                break;
                            default:
                                _meeples = Tools.Meeple.Huit;
                                break;
                        }

                        switch (int.Parse(settings[5]))
                        {
                            case 60:
                                _timer_game_value = Tools.Timer.Minute;
                                break;
                            case 1800:
                                _timer_game_value = Tools.Timer.DemiHeure;
                                break;
                            case 3600:
                                _timer_game_value = Tools.Timer.Heure;
                                break;
                            default:
                                _timer_game_value = Tools.Timer.DemiHeure;
                                break;

                        }

                        switch (int.Parse(settings[6]))
                        {
                            case 10:
                                _timer_player_value = Tools.Timer.DixSecondes;
                                break;
                            case 30:
                                _timer_player_value = Tools.Timer.DemiMinute;
                                break;
                            case 60:
                                _timer_player_value = Tools.Timer.Minute;
                                break;
                            default:
                                _timer_player_value = Tools.Timer.DemiMinute;
                                break;

                        }

                        _score_max = int.Parse(settings[7]);
                        _extensionsGame = int.Parse(settings[8]);

                        if (IsRiverExtensionOn())
                        {
                            _idTuileInit = 24;
                        }
                        else
                        {
                            _idTuileInit = 22;
                        }
                        
                        resetRoomTimer();

                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("ERROR: Set_settings : " + ex);
                        return;
                    }
                }
            }
        }

        /// <summary>
        /// Pass to the next player
        /// </summary>
        /// <returns> Returns the new actual player's id </returns>
        public ulong PassToAndGet_NextPlayer()
        {
            // Transforme le dico en array pour récupérer le n-ième joueur
            _s_dico_joueur.WaitOne();
            ulong[] idPlayer_array = _dico_joueur.Keys.ToArray();
            _s_dico_joueur.Release();

            // Incrémente le numéro du joueur actuel (modulo nb_joueurs) et récupère l'idPlayer du nouveau joueur
            _s_offsetActualPlayer.WaitOne();
            _s_nombre_joueur.WaitOne();
            _offsetActualPlayer = (_offsetActualPlayer + 1) % _nombre_joueur;
            _s_nombre_joueur.Release();
            ulong nextPlayer = idPlayer_array[_offsetActualPlayer];
            _s_offsetActualPlayer.Release();

            return nextPlayer;
        }

        /// <summary>
        /// Switch moderator to the next player
        /// </summary>
        public void SwitchModerateur()
        {
            _s_dico_joueur.WaitOne();
            ulong newModerator = Get_Dico_Joueurs().First().Key;
            _s_dico_joueur.Release();
            _id_moderateur = newModerator;
            
        }

        /// <summary>
        ///     Constructor : initialisation of parameters.
        /// </summary>
        ///<param name="id_partie">id of the game .</param>
        ///<param name="id_joueur_createur">id of the admin player.</param>
        ///<param name="playerSocket">socket.</param>
        public Thread_serveur_jeu(int id_partie, ulong id_joueur_createur, Socket? playerSocket)
        {
            _id_partie = id_partie;

            /* Zone  du Dictionary Score */
            _dico_joueur = new Dictionary<ulong, Player>();
            _s_dico_joueur = new Semaphore(1, 1);
            _nombre_joueur = 0;
            _s_nombre_joueur = new Semaphore(1, 1);
            _nombre_joueur_max = 8;
            _id_moderateur = id_joueur_createur;

            _statut_partie = Tools.GameStatus.Room;

            // Initialisation des valeurs par défaut
            _mode = Tools.Mode.Default;
            _nb_tuiles = 60;
            _score_max = 50;
            _privee = false; // Une partie est par défaut privée
            _timer_game_value = Tools.Timer.Heure; // Une heure par défaut
            _timer_player_value = Tools.Timer.Minute;
            _meeples = Tools.Meeple.Huit;


            // Initialisation des attributs moteurs
            _idTuileInit = 22; // Initialise sans dlc rivière
            _tuilesEnvoyees = new string[3];

            // Initialisation attributs anticheat
            _posPionTourActu = Array.Empty<string>();
            _AC_idFirstValidTile = ulong.MaxValue;
            _AC_barrierUp = false;
            _AC_drawedTilesValid = false;

            // Init des locks
            _lock_settings = new object();

            // Initialisation des semaphores d'attributs moteurs
            _s_offsetActualPlayer = new Semaphore(1, 1);
            _tuilesGame = new List<ulong>();
            _s_tuilesGame = new Semaphore(1, 1);
            _s_tuilesEnvoyees = new Semaphore(1, 1);
            _s_rivieresGame = new Semaphore(1, 1);

            _s_plateau = new Semaphore(1, 1);
            _s_posPionTourActu = new Semaphore(1, 1);
            _s_posTuileTourActu = new Semaphore(1, 1);
            
            _s_AC_barrierUp = new Semaphore(1, 1);
            _s_AC_idFirstValidTile = new Semaphore(1, 1);

        }

        
        /// <summary>
        ///     Method : adding a player to the game.
        /// </summary>
        ///<param name="id_joueur">id of the player entering the game (to add).</param>
        ///<param name="playerSocket">socket.</param>
        public Tools.PlayerStatus AddJoueur(ulong id_joueur, Socket? playerSocket)
        {
            _s_dico_joueur.WaitOne();

            if (_dico_joueur.ContainsKey(id_joueur))
            {
                _s_dico_joueur.Release();
                return Tools.PlayerStatus.Found;
            }

            _s_nombre_joueur.WaitOne();
            if (_nombre_joueur >= _nombre_joueur_max)
            {
                _s_dico_joueur.Release();
                return Tools.PlayerStatus.Full;
            }
            _s_nombre_joueur.Release();

            

            _dico_joueur.Add(id_joueur, new Player(id_joueur, playerSocket));
            _s_nombre_joueur.WaitOne();
            _nombre_joueur++;
            _s_nombre_joueur.Release();
            _s_dico_joueur.Release();
            
            resetRoomTimer();

            return Tools.PlayerStatus.Success;
        }


        /// <summary>
        ///     Method : Removing player from the game.
        /// </summary>
        ///<param name="id_joueur">id of the player to remove from game .</param>
        ///<param name="playerSocket">socket.</param>
        public Tools.PlayerStatus RemoveJoueur(ulong id_joueur)
        {

            _s_dico_joueur.WaitOne();
            bool res = _dico_joueur.Remove(id_joueur);
            if (res)
            {
                _s_nombre_joueur.WaitOne();
                _nombre_joueur--;
                _s_nombre_joueur.Release();
                if (id_joueur == _id_moderateur)
                {
                    if (_dico_joueur.Count > 1)
                    {
                        _id_moderateur = _dico_joueur.First().Key;
                    }
                    else
                    {/* Il n'y a plus qu'un seul joueur ou moins dans la room */
                        _statut_partie = Tools.GameStatus.Stopped;
                    }
                }

                _s_dico_joueur.Release();
                

                return Tools.PlayerStatus.Success;
            }

            _s_dico_joueur.Release();
            return Tools.PlayerStatus.NotFound;
        }



        /// <summary>
        ///     Methode : incrementing the cheat value of the player, and testing if it's his second time then RemoveJoueur .
        /// </summary>
        ///<param name="id_joueur">id of the concerned player .</param>
        public Tools.PlayerStatus SetPlayerTriche(ulong id_joueur)
        {
            _s_dico_joueur.WaitOne();
            if (!_dico_joueur.ContainsKey(id_joueur))
            {
                _s_dico_joueur.Release();
                return Tools.PlayerStatus.NotFound;
            }

            Player player = _dico_joueur[id_joueur];
            player._s_player.WaitOne();
            player._triche++;
            if (player._triche == (uint)Tools.PlayerStatus.Kicked)
            {
                player._s_player.Release();
                _s_dico_joueur.Release();
                RemoveJoueur(id_joueur);
                return Tools.PlayerStatus.Kicked;
            }
            player._s_player.Release();
            _s_dico_joueur.Release();

            return Tools.PlayerStatus.Success;
        }


        /// <summary>
        ///     Methode : Changing the player status, if not ready then ready else not ready.
        /// </summary>
        ///<param name="id_joueur">id of the concerned player .</param>
        public Tools.PlayerStatus SetPlayerStatus(ulong id_joueur)
        {
            _s_dico_joueur.WaitOne();
            if (!_dico_joueur.ContainsKey(id_joueur))
            {
                _s_dico_joueur.Release();
                return Tools.PlayerStatus.NotFound;
            }

            Player player = _dico_joueur[id_joueur];
            player._s_player.WaitOne();
            player._is_ready = !player._is_ready;
            player._s_player.Release();
            _s_dico_joueur.Release();
            
            resetRoomTimer();

            return Tools.PlayerStatus.Success;
        }


        /// <summary>
        ///     Methode : incrementong a player score.
        /// </summary>
        ///<param name="id_joueur">id of the concerned player .</param>
        ///<param name="score">score to add to the concerned player score.</param>
        public Tools.PlayerStatus SetPlayerPoint(ulong id_joueur, uint score)
        {
            _s_dico_joueur.WaitOne();
            if (!_dico_joueur.ContainsKey(id_joueur))
            {
                _s_dico_joueur.Release();
                return Tools.PlayerStatus.NotFound;
            }

            Player player = _dico_joueur[id_joueur];
            player._s_player.WaitOne();
            player._score += score;
            player._s_player.Release();
            _s_dico_joueur.Release();

            return Tools.PlayerStatus.Success;
        }


        /// <summary>
        ///     Methode : Setting barrier.
        /// </summary>
        public void SetACBarrier()
        {
            _s_nombre_joueur.WaitOne();
            _s_AC_barrierUp.WaitOne();
            if(_AC_barrierUp == false)
            {
                _AC_barrierAllVerifDone = new Barrier((int)_nombre_joueur);
                _AC_barrierUp = true;
            }
            _s_AC_barrierUp.Release();
            _s_nombre_joueur.Release();
        }

        /// <summary>
        ///     Methode : wait barrier.
        /// </summary>
        public void WaitACBarrier()
        {
            if (_AC_barrierUp)
            {
                _AC_barrierAllVerifDone.SignalAndWait(5000);
            }
            
        }

        /// <summary>
        ///     Methode : Disposing the barrier then getting it down again.
        /// </summary>
        public void DisposeACBarrier()
        {
            _s_AC_barrierUp.WaitOne();
            _AC_barrierAllVerifDone.Dispose();
            _AC_barrierUp = false;
            _s_AC_barrierUp.Release();
        }
        

        
        /// <summary>
        ///     Methode : is everyone ready?.
        /// </summary>
        /// <returns> Returns True or False </returns>
        public bool EveryoneIsReady()
        {
            bool result = true;

            _s_dico_joueur.WaitOne();
            foreach(var player in _dico_joueur)
            {
                // Vérifie que tous les joueurs
                if(player.Value._is_ready == false)
                {
                    result = false;
                    break;
                }
            }
            _s_dico_joueur.Release();

            return result;
        }

        /// <summary>
        ///     Methode : Initialization of player's meeples from the value of the parametre _meeples.
        /// </summary>
        public void InitializePlayerMeeples()
        {
            _s_dico_joueur.WaitOne();
            foreach(var player in _dico_joueur)
            {
                player.Value._nbMeeples = ((ulong)_meeples);
            }
            _s_dico_joueur.Release();
        }

        // =======================
        // Méthodes moteur de jeu
        // =======================

        /// <summary>
        ///     Methode : Starting game.
        /// </summary>
        /// <returns> Returns the id of the initial tile  </returns>
        public ulong StartGame()
        {
            stopRoomTimer();
            
            _statut_partie = Tools.GameStatus.Running;

            // Génération du dicoTuile de la classe tuile
            Dictionary<ulong, Tuile> dicoTuiles = LireXML2.Read("system/config_back.xml");

            // Génération du plateau
            _plateau = new Plateau(dicoTuiles);

            // Récupération de la liste de joueurs
            _s_dico_joueur.WaitOne();
            ulong[] idPlayer_array = _dico_joueur.Keys.ToArray();
            _s_dico_joueur.Release();

            // Indication du joueur actuel (commence toujours par le modérateur)
            _s_offsetActualPlayer.WaitOne();
            _offsetActualPlayer = 0;
            _s_offsetActualPlayer.Release();


            // Réduction du nombre de tuile de 1, pour la tuile de départ
            if (IsRiverExtensionOn() == false)
            {
                _nb_tuiles--;
            }

            // Génération des tuiles de la game
            _s_tuilesGame.WaitOne();
            _tuilesGame = Random_sort_tuiles(_nb_tuiles);
            
            if (IsRiverExtensionOn())
            {
                Console.WriteLine("extension RIVIERE is ON");
                _rivieresGame = Random_sort_rivieres();
            }
            _s_tuilesGame.Release();

            // Génération des attributs d'anti cheat
            _AC_drawedTilesValid = false;
            Set_tuilesEnvoyees(GetThreeLastTiles());

            // Initialise la tuile placée de ce tour inexistante
            _posTuileTourActu = new Position();
            _posTuileTourActu.SetNonExistent();

            // Pose la première tuile de la partie
            _s_plateau.WaitOne();
            _plateau.Poser1ereTuile(_idTuileInit);
            _s_plateau.Release();

            // Initialise les meeples de tt le monde
            InitializePlayerMeeples();

            if (_mode == Tools.Mode.TimeAttack)
            {
                _timer_game = new System.Timers.Timer();
                _timer_game.Interval = 1000;
                _timer_game.Elapsed += OnTimedEventGame;
                _DateTime_game = DateTime.Now;
                _timer_game.AutoReset = true;
                _timer_game.Enabled = true;
            }


            _timer_player = new System.Timers.Timer();
            _timer_player.Interval = 1000;
            _timer_player.Elapsed += OnTimedEventPlayer;
            _DateTime_player = DateTime.Now;
            _timer_player.AutoReset = true;
            _timer_player.Enabled = true;

// TODO :
            // synchronisation de la methode
            // genere les tuiles
            // envoie 3 tuiles au player 1
            // start timer
            // return valeur d'erreur pour la méthode parent
            return _idTuileInit;
        }
        
        
        /// <summary>
        ///     Methode : Event when game timer expires (EndGame()) .
        /// </summary>
        private void OnTimedEventGame(Object source, System.Timers.ElapsedEventArgs e)
        {
            var diff = DateTime.Now.Subtract(_DateTime_game).Hours;
            if (diff < (int) _timer_game_value / 3600) return;

            var idPlayer = Get_ActualPlayerId();
            Console.WriteLine("Game was raised at {0}. EndGame() is called + {1}", e.SignalTime, idPlayer);
            _timer_game.Stop();
            
            string[] dataPlayToSend = ParseStoredPlayToData();
            GestionnaireThreadCom gestionnaire = GestionnaireThreadCom.GetInstance();
            // Force the end of the game
            gestionnaire.CallForceEndTurn(idPlayer, _id_partie, dataPlayToSend);
        }
        

        /// <summary>
        ///     Methode : Event when player's round timer expires, forcing the end of the round .
        /// </summary>
        private void OnTimedEventPlayer(Object source, System.Timers.ElapsedEventArgs e)
        {
            var diff = DateTime.Now.Subtract(_DateTime_player).Minutes;
            if (diff < (int) _timer_player_value / 60) return;
            
            var idPlayer = Get_ActualPlayerId();
            Console.WriteLine("Player was raised at {0}. EndTurn({1}) is called", e.SignalTime, idPlayer);
            _timer_player.Stop();

            GestionnaireThreadCom gestionnaire = GestionnaireThreadCom.GetInstance();

            _s_dico_joueur.WaitOne();
            _dico_joueur[idPlayer]._timer++;
            _s_dico_joueur.Release();

            string[] dataPlayToSend = ParseStoredPlayToData();
            

            // Kick après la fin du tour (pour éviter les erreurs)
            if (_dico_joueur[idPlayer]._timer > 2)
            {         
                gestionnaire.CallPlayerKick(_id_partie, idPlayer, dataPlayToSend);

            }
            else // Sinon fin de tour forcée classique
            {
                // Force the end of the turn
                gestionnaire.CallForceEndTurn(idPlayer, _id_partie, dataPlayToSend);
            }
        }

        public void startRoomTimer()
        {
            _timer_room = new System.Timers.Timer();
            _timer_room.Interval = 1000;
            _timer_room.Elapsed += OnTimedEventRoom;
            _DateTime_room = DateTime.Now;
            _timer_room.AutoReset = true;
            _timer_room.Enabled = true;
        }

        public void resetRoomTimer()
        {
            _DateTime_room = DateTime.Now;
        }
        
        public void stopRoomTimer()
        {
            _timer_room.Stop();
        }
        
        /// <summary>
        ///     Methode : Event when room timer expires (CloseRoom()) .
        /// </summary>
        private void OnTimedEventRoom(Object source, System.Timers.ElapsedEventArgs e)
        {
            var diff = DateTime.Now.Subtract(_DateTime_room).Minutes;
            if (diff < 300 / 60) return; // 300 -> 5 minutes
            
            Console.WriteLine("Room was raised at {0}. CloseRoom() is called", e.SignalTime);
            _timer_room.Stop();
            _statut_partie = Tools.GameStatus.Stopped;
            
            GestionnaireThreadCom gestionnaire = GestionnaireThreadCom.GetInstance();
            // Force the end of the game
            gestionnaire.CallForceCloseRoom(_id_partie);
        }
        
        /// <summary>
        ///     Method : Get three tiles' id from the game's list of tile
        /// </summary>
        /// <returns> A array of 3 tiles' id </returns>
        public string[] GetThreeLastTiles()
        {
            List<string> tuilesTirees;

            if (IsRiverExtensionOn() && _rivieresGame.Count() != 0)
            {
                // Génère les 3 tuiles à envoyer
                tuilesTirees = tirageTroisTuiles(_rivieresGame);
            }
            else
            {
                // Génère les 3 tuiles à envoyer
                tuilesTirees = tirageTroisTuiles(_tuilesGame);
            }
           

            // Met à jour le stockage des 3 tuiles envoyées
            _s_tuilesEnvoyees.WaitOne();
            _tuilesEnvoyees = new string[tuilesTirees.Count];
            Array.Copy(tuilesTirees.ToArray(), _tuilesEnvoyees, tuilesTirees.Count);
            _s_tuilesEnvoyees.Release();
            
            return tuilesTirees.ToArray();
        }

        /// <summary>
        ///     Method : Shuffles the list of game's tiles
        ///     Draw new set of tile if not in default mode
        /// </summary>
        public void ShuffleTilesGame()
        {
            _s_tuilesGame.WaitOne();

            List<ulong> tuilesGame_resultat = new List<ulong>();

            if (_tuilesGame.Count() != 0)
            {             
                var rnd = new System.Random();
                var randomedList = _tuilesGame.OrderBy(item => rnd.Next());
                foreach (var value in randomedList)
                {
                    tuilesGame_resultat.Add(value);
                }
            }
            else{
                if(_mode != Tools.Mode.Default)
                {
                    tuilesGame_resultat = Random_sort_tuiles(_nb_tuiles);
                }
            }
            
            
            _tuilesGame = tuilesGame_resultat;
            _s_tuilesGame.Release();

        }
        
        /// <summary>
        ///     Methode : Tile placement, this method test if the tile placement asked by the player is legal, if yes, the tile is placed, if not illegal + cheat avertissement
        /// </summary>
        ///<param name="idPlayer">id of the concerned player .</param>
        ///<param name="idTuile">id of the concerned tile .</param>
        ///<param name="posX"> postion x .</param>
        ///<param name="posY">position y .</param>
        ///<param name="rotat">rotation .</param>
        public Tools.Errors TilePlacement(ulong idPlayer, ulong idTuile, int posX, int posY, int rotat)
        {
            Console.WriteLine("DEBUG_TuilePlacement : Entrée dans Serveur_jeu");
            
            _s_plateau.WaitOne();
            // Si placement légal, on le sauvegarde
            if (isTilePlacementLegal(idTuile, posX, posY, rotat)){

                Console.WriteLine("TilePlacement : isTilePlacementLegal OK");

                _s_posTuileTourActu.WaitOne();
                _posTuileTourActu = new Position(posX, posY, rotat);
                _s_posTuileTourActu.Release();

                // Pose tuile fantôme
                _plateau.PoserTuileFantome(_idTuileChoisie, new Position(posX, posY, rotat));

                _s_plateau.Release();
                return Tools.Errors.None;
            }
            else // Si non, renvoie l'erreur illegalPlay + envoi message cheat
            {
                // Le thread de com s'occupera d'appeller le setplayerstatus pour indiquer la triche
                Console.WriteLine("TilePlacement : isTilePlacementLegal IllegalPlay");
                _s_plateau.Release();
                return Tools.Errors.IllegalPlay;
            }
        }


        /// <summary>
        ///     Methode : Pion placement, this method test if the pion placement asked by the player is legal, if yes, the pion is placed, if not illegal + cheat avertissement
        /// </summary>
        ///<param name="idPlayer">id of the concerned player .</param>
        ///<param name="idTuile">id of the concerned tile .</param>
        ///<param name="idMeeple"> id of the concerned meeple x .</param>
        ///<param name="slotPos">the position on the slot .</param>
        public Tools.Errors PionPlacement(ulong idPlayer, Position posTuile, ulong idMeeple, int slotPos)
        {
            _s_plateau.WaitOne();
            // Si placement légal, on le sauvegarde
            if (isPionPlacementLegal(posTuile, slotPos, idPlayer, idMeeple))
            {
                string[] posPion = new string[] { idPlayer.ToString(), posTuile.X.ToString(), posTuile.Y.ToString(), idMeeple.ToString(), slotPos.ToString() };
                _s_posPionTourActu.WaitOne();
                _posPionTourActu = new string[posPion.Length];
                Array.Copy(posPion, _posPionTourActu, posPion.Length);
                _s_posPionTourActu.Release();

                _s_plateau.Release();
                return Tools.Errors.None;
            }
            else // Si non, renvoie l'erreur illegalPlay + envoi message cheat
            {
                // Le thread de com s'occupera d'appeller le setplayerstatus pour indiquer la triche

                _s_plateau.Release();
                return Tools.Errors.IllegalPlay;
            }
        }
        
        /// <summary>
        ///     Methode : this method test if a tile placement is legal.
        /// </summary>
        ///<param name="idTuile">id of the concerned tile .</param>
        ///<param name="idMeeple"> id of the concerned meeple x .</param>
        ///<param name="posX"> postion x .</param>
        ///<param name="posY">position y .</param>
        ///<param name="rotat">rotation .</param>
        /// <returns> Returns True or False </returns>
        public bool isTilePlacementLegal(ulong idTuile, int posX, int posY, int rotat)
        {
            return _plateau.PlacementLegal(idTuile, posX, posY, rotat);
        }
        

        /// <summary>
        ///     Methode : this method test if a pion placement is legal.
        /// </summary>
        ///<param name="posTuile">The position of the concerned tile .</param>
        ///<param name="idSlot">id if the concerned slot .</param>
        ///<param name="idPlayer">id of the concerned player .</param>
        ///<param name="idMeeple"> id of the concerned meeple x .</param>
        /// <returns> Returns True or False </returns>
        public bool isPionPlacementLegal(Position posTuile, int idSlot, ulong idPlayer, ulong idMeeple)
        {
            Console.WriteLine("X=" + posTuile.X + " | Y=" + posTuile.Y);
            return _plateau.PionPosable(posTuile.X, posTuile.Y, (ulong)idSlot, idPlayer, idMeeple);
        }
        
        /// <summary>
        ///     Methode : Remove a tile when trying to be placed on a not possible position.
        /// </summary>
        public void RetirerTuileTourActu()
        {
            _s_posTuileTourActu.WaitOne();
            _posTuileTourActu.SetNonExistent();
            _s_posTuileTourActu.Release();
        }


        /// <summary>
        ///     Methode : Remove a pion.
        /// </summary>
        public void RetirerPionTourActu()
        {
            _s_posPionTourActu.WaitOne();
            _posPionTourActu = Array.Empty<string>();
            _s_posPionTourActu.Release();
        }
        

        /// <summary>
        ///     Methode : Updating the game status.
        /// </summary>
        public Tools.GameStatus UpdateGameStatus()
        {
            Tools.GameStatus statutGame = _statut_partie;

            // Update suivant le mode de jeu
            switch (_mode)
            {
                case Tools.Mode.Default:
                    if(_tuilesGame.Count == 0)
                    {
                        statutGame = Tools.GameStatus.Stopped;
                    }
                    break;
                case Tools.Mode.TimeAttack:
                    var diff = DateTime.Now.Subtract(_DateTime_game).Hours;
                    if (diff >= (int)_timer_game_value / 3600)
                        statutGame = Tools.GameStatus.Stopped;
                    //TODO

                    break;
                case Tools.Mode.Score:
                    // Vérifie si un joueur a atteint le score maximal
                    string[] allScores = GetAllPlayersScore();
                    foreach(string score in allScores)
                    {
                        if(Int32.Parse(score) >= _score_max)
                        {
                            statutGame = Tools.GameStatus.Stopped;
                            break;
                        }
                    }

                    break;
            }

            _s_nombre_joueur.WaitOne();
            if(_nombre_joueur <= 0)
            {
                statutGame = Tools.GameStatus.Stopped;

            }
            _s_nombre_joueur.Release();

            _statut_partie = statutGame;

            return statutGame;
        }


        /// <summary>
        ///     Methode : Remove all game tiles.
        /// </summary>
        public void RetirerTuileGame(ulong idTuile)
        {
            
            int indexOfTile = -1;
            if(IsRiverExtensionOn() && _rivieresGame.Count() > 0) // Cas de l'extension rivière et toutes les tuiles n'ont pas été tirées
            {
                _s_rivieresGame.WaitOne();
                for (int i = 0; i < _rivieresGame.Count; i++)
                {
                    if (_rivieresGame[i] == idTuile)
                    {
                        indexOfTile = i;
                    }
                }
                Console.WriteLine("* RetirerTuileGame _rivieresGame.Count = " + _rivieresGame.Count);
                Console.WriteLine("* RetirerTuileGame tuile d'index : " + indexOfTile + " de _rivieresGame");
                _rivieresGame.RemoveAt(indexOfTile);
                _s_rivieresGame.Release();
            }
            else
            {
                _s_tuilesGame.WaitOne();
                for (int i = 0; i < _tuilesGame.Count; i++)
                {
                    if (_tuilesGame[i] == idTuile)
                    {
                        indexOfTile = i;
                    }
                }
                Console.WriteLine("* RetirerTuileGame _tuilesGame.Count = " + _tuilesGame.Count);
                Console.WriteLine("* RetirerTuileGame tuile d'index : " + indexOfTile + " de _tuilesGame");
                _tuilesGame.RemoveAt(indexOfTile);
                _s_tuilesGame.Release();
            }
           
        }


        /// <summary>
        ///     Methode : Cancelling a turn.
        /// </summary>
        public Socket? CancelTurn()
        {
            // Remise à inexistant la tuilePosActu et pionPosActu
            _s_posTuileTourActu.WaitOne();
            _posTuileTourActu.SetNonExistent();
            _s_posTuileTourActu.Release();

            _s_posPionTourActu.WaitOne();
            _posPionTourActu = Array.Empty<string>();
            _s_posPionTourActu.Release();

            // Passe au joueur suivant
            ulong nextPlayer = PassToAndGet_NextPlayer();

            _s_dico_joueur.WaitOne();
            Socket? nextPlayerSocket = _dico_joueur[nextPlayer]._socket_of_player;
            _s_dico_joueur.Release();


            return nextPlayerSocket;
        }

        /// <summary>
        ///     Methode : End the turn
        /// </summary>
        /// <returns> The socket of the next player to play </returns>
        public Socket? EndTurn(ulong idPlayer, bool timer)
        {
            _timer_player.Stop();

            bool aJoue = false;

            if (timer)
                _dico_joueur[idPlayer]._timer = 0;
            
            // Prise en compte du placement de la tuile et du pion (mise à jour structure de données)
            _s_plateau.WaitOne();
            
            try
            {
                // Si une tuile a été posée durant le tour (pour prendre en compte le cas de forçage de fin de tour)
                if (Get_posTuileTourActu().IsExisting())
                {
                    _plateau.PoserTuileFantome(_idTuileChoisie, _posTuileTourActu);

                    if (_posPionTourActu.Length != 0) // Si un pion a été posé durant le tour
                    {
                        foreach (var i in _posPionTourActu)
                        {
                            Console.WriteLine("i" + i);
                        }
                        _plateau.PoserPion(idPlayer, _posTuileTourActu.X, _posTuileTourActu.Y, UInt64.Parse(_posPionTourActu[4]));

                        // Retrait d'un pion au joueur
                        _s_dico_joueur.WaitOne();
                        _dico_joueur[idPlayer]._nbMeeples = _dico_joueur[idPlayer]._nbMeeples - 1;
                        _s_dico_joueur.Release();
                    }

                    aJoue = true;
                }           
      
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR: On PoserTuile : " + ex);
            }          
            _s_plateau.Release();

            // Validation de la position

            if(aJoue)
                _plateau.ValiderTour();

            List<Zone> lstZone = new List<Zone>();

            // Vérification des fermetures de chemins et mise à jour des points
            List<PlayerScoreParam> lstPlayerScoreGain = new List<PlayerScoreParam>();
            if(aJoue && _plateau.VerifZoneFermeeTuile(_posTuileTourActu.X, _posTuileTourActu.Y, lstPlayerScoreGain, lstZone))
            {
                // Mise à jour des points 
                _s_dico_joueur.WaitOne();

                foreach(var score in lstPlayerScoreGain)
                {
                    _dico_joueur[score.id_player].AddPoints((uint)score.points_gagnes);
                }

                _s_dico_joueur.Release();

                List<Tuple<int, int, ulong>> positions = new List<Tuple<int, int, ulong>>();

                Console.WriteLine("DBG - Avant d'entrer dans removeAllPawnInTile");

                Dictionary<ulong, int> dico = _plateau.RemoveAllPawnInTile(_posTuileTourActu.X, _posTuileTourActu.Y, positions);
                Console.WriteLine("DBG - dico de longueur : " + dico.Count);
                foreach (ulong id_player in dico.Keys)
                {
                    Console.WriteLine("DBG - Parcours des joueurs de dico.Keys");
                    _s_dico_joueur.WaitOne();
                    Player joueur = _dico_joueur[id_player];
                    _s_dico_joueur.Release();
                 
                    int meeplesRendus = dico[id_player];
                    Console.WriteLine("DBG - Meeples à rendre : " + meeplesRendus);

                    joueur.AddMeeple((uint)meeplesRendus);
                }

            }


            if(aJoue)
                RetirerTuileGame(_idTuileChoisie);


            // Remise à inexistant la tuilePosActu et pionPosActu
            _s_posTuileTourActu.WaitOne();
            _posTuileTourActu.SetNonExistent();
            _s_posTuileTourActu.Release();

            _s_posPionTourActu.WaitOne();
            _posPionTourActu = Array.Empty<string>();
            _s_posPionTourActu.Release();

            // Passe au joueur suivant
            ulong nextPlayer = PassToAndGet_NextPlayer();

            _s_dico_joueur.WaitOne();
            Socket? nextPlayerSocket = _dico_joueur[nextPlayer]._socket_of_player;
            _s_dico_joueur.Release();
            
            // reset player timer
            _DateTime_player = DateTime.Now;
            _timer_player.Start();

            return nextPlayerSocket;
        }


        /// <summary>
        ///     Methode : Ending a game.
        /// </summary>
        public void EndGame()
        {
            _statut_partie = Tools.GameStatus.Stopped;

            // TODO :
            // synchronisation de la methode
            // close timer
            // return valeur d'erreur pour la méthode parent
        }

        public void TimerPlayer(ulong idPlayer)
        {
            // TODO :
            // inc player timer count + check limit ?
        }

        // =================================
        // Méthodes supplémentaires moteur
        // =================================

        /// <summary>
        ///     Method : Get 3 tiles id in string format from a tile list
        /// </summary>
        /// <param name="tuiles"> Tiles list </param>
        /// <returns> List of 3 tiles id in string format </returns>
        public static List<string> tirageTroisTuiles(List<ulong> tuiles)
        {
            List<string> list = new List<string>();
            if(tuiles.Count > 2)
            {
                for (int i = tuiles.Count - 3; i < tuiles.Count; i++)
                    list.Insert(0, tuiles[i].ToString());
            }
            else
            {
                foreach(ulong tileId in tuiles)
                {
                    list.Insert(0, tileId.ToString());
                }
            }
            
            return list;
        }

        /// <summary>
        ///     Methode : Removing the chosen tile's id from the game tiles list.
        /// </summary>
        ///<param name="tuiles">game tiles list .</param>
        ///<param name="idTuile">id of the concerned tile .</param>
        /// <returns> New list after removing the tile </returns>
        public static List<ulong> suppTuileChoisie(List<ulong> tuiles, ulong idTuile)
        {
            int i = 0;
            for (i = tuiles.Count - 1; i >= 0 && tuiles[i] != idTuile; i--) ;
            tuiles.Remove(tuiles[i]);

            return tuiles;
        }



        /// <summary>
        ///     Methode : select a tile to pick up while randomly generating the game tiles list .
        /// </summary>
        ///<param name="id">tile's id .</param>
        ///<param name="x">random number (it must be between 0 and the sum of all tiles probabilities) .</param>
        ///<param name="dico">tiles dictionary (id,probability) .</param>
        /// <returns> Selected tile to pick up </returns>
        public static ulong tuile_a_tirer(ulong id, int x, Dictionary<ulong, ulong> dico)
        {
            ulong sum = 0;
            foreach (var item in dico)          //Parcourir dico:
            {
                if ((int)(sum + item.Value) < x)
                {
                    sum += item.Value;          //chercher l'id correspondant
                                                //Tant que sum+la proba de tuile actuele > id de tuile --> avance
                }

                else
                {
                    id = item.Key;              //id retrouvé
                    break;
                }

            }

            return id;
        }



        /// <summary>
        ///     Methode : Randomly generat a river to begin (river extention).
        /// </summary>
        /// <returns> list of generated river's tiles </returns>
        public static List<ulong> Random_sort_rivieres()
        {
            var random = new Random();
            var rivieres = new List<ulong>();
            var rivieresRaw = new List<ulong>();

            var db = new Database();
            db.RemplirRivieres(rivieresRaw);

            Console.WriteLine("/!\\ DBG - Random_sort_rivi : head is " + rivieresRaw[0]);
            rivieresRaw.RemoveAt(0); // Retrait de la première tuile (init, posée auto)
            var tail = rivieresRaw.Last(); // Mise à part de la dernière (aval)
            Console.WriteLine("/!\\ DBG - Random_sort_rivi : tail is " + tail.ToString());
            rivieresRaw.RemoveAt(rivieresRaw.Count-1);

            // DEBUG
            Console.WriteLine("/!\\ DBG - rivieresRaw = [");
            foreach(ulong idTuile in rivieresRaw){
                Console.Write("" + idTuile.ToString() + ", ");
            }
            Console.Write("]");

            while (rivieresRaw.Count > 0)
            {
                var index = random.Next(0, rivieresRaw.Count);
                rivieres.Add(rivieresRaw[index]);
                rivieresRaw.RemoveAt(index);
            }

            rivieres.Insert(0, tail);

            // DEBUG
            Console.WriteLine("/!\\ DBG - rivieres = [");
            foreach (ulong idTuile in rivieres)
            {
                Console.Write("" + idTuile.ToString() + ", ");
            }
            Console.Write("]");

            //Retourner la liste 
            return rivieres;
        }

        /// <summary>
        ///     Method : 'Randomly' generate the game tiles list.  
        /// </summary>
        /// <param name="nbTuiles"> Number of tiles to have in the list </param>
        /// <returns> List of game tiles </returns>
        public static List<ulong> Random_sort_tuiles(int nbTuiles)
        {
            List<ulong> list = null;
            list = new List<ulong>();
            System.Random MyRand = new System.Random();
            int x = 0;
            ulong idTuile = 0, sumDesProbas = 0;

            //Recuperer les id des tuiles et leurs probas depuis la bdd

            //Dictionary<int, int> map = new Dictionary<int, int>();
            //La section suivante est à remplacer par une methode de l'équipe BDD qui retourne 
            //un dico des ids de tuile avec leurs probas
            /*************************/
            Dictionary<ulong, ulong> map = new Dictionary<ulong, ulong>();

            var db = new Database();
            db.RemplirTuiles(map);


            //Parcourir le dictionnaire resultat pour calculer la somme des probabilités des tuiles:
            foreach (var item in map)
            {
                sumDesProbas += item.Value;

            }
            int tmp = (int)(sumDesProbas - sumDesProbas % 1.0);
            //Tirage aléatoire des tuiles
            for (int i = 0; i < nbTuiles; i++)
            {
                x = MyRand.Next(tmp);
                idTuile = tuile_a_tirer(idTuile, x, map);
                list.Add(idTuile);

            }
            //Retourner la liste 
            return list;

        }


        /// <summary>
        ///     Methode : Testing if the river extention is activated.
        /// </summary>
        /// <returns> True or False </returns>
        public bool IsRiverExtensionOn()
        {
            bool riverOn = (_extensionsGame & (int)Tools.Extensions.Riviere) > 0;
            return riverOn;
        }

        public bool IsAbbayeExtensionOn()
        {
            bool abbayeOn = (_extensionsGame & (int)Tools.Extensions.Abbaye) > 0;
            return abbayeOn;
        }

        public string[] ParseStoredPlayToData()
        {
            string[] dataToReturn = new string[6];

            dataToReturn[0] = Get_idTuileChoisie().ToString(); // idTuile

            _s_posTuileTourActu.WaitOne();

            if (Get_posTuileTourActu().IsExisting()) // Tile play stored
            {
                dataToReturn[1] = Get_posTuileTourActu().X.ToString(); // Pos X
                dataToReturn[2] = Get_posTuileTourActu().Y.ToString(); // Pos Y
                dataToReturn[3] = Get_posTuileTourActu().ROT.ToString(); // Pos ROT
            }
            else // No tile play stored
            {
                dataToReturn[0] = (-1).ToString(); // Set the id to -1 because there is no play
                dataToReturn[1] = (-1).ToString(); // Pos X
                dataToReturn[2] = (-1).ToString(); // Pos Y
                dataToReturn[3] = (-1).ToString(); // Pos ROT
            }

            _s_posTuileTourActu.Release();


            _s_posPionTourActu.WaitOne();

            if (Get_posPionTourActu().Length != 0) // Pawn play stored
            {
                dataToReturn[4] = Get_posPionTourActu()[4]; // idMeeple
                dataToReturn[5] = Get_posPionTourActu()[5]; // slotPos
            }
            else // No pawn play stored
            {
                dataToReturn[4] = (-1).ToString(); // idMeeple
                dataToReturn[5] = (-1).ToString(); // slotPos
            }

            _s_posPionTourActu.Release();

            return dataToReturn;

        }

        public string[] GetAllPlayersScore()
        {
            List<string> allPlayersScore = new List<string>();

            _s_dico_joueur.WaitOne();
            foreach (var player in _dico_joueur)
            {
                allPlayersScore.Add(player.Value._score.ToString());

            }
            _s_dico_joueur.Release();

            return allPlayersScore.ToArray();
        }
        
    }
}
