using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using ClassLibrary;
using Server;

namespace system
{
    internal class GestionnaireThreadCom
    {

        public List<Thread_communication> _lst_obj_threads_com { get; set; }
        public List<Thread> _lst_threads_com { get; set; }
        private List<int> _lst_localPort_dispo;
        private List<int> _lst_remotePort_dispo;
        private int _compteur_id_thread_com;
        private int _compteur_id_partie;
        private Semaphore _s_compteur_id_partie;

        private GestionnaireThreadCom() { }

        private static GestionnaireThreadCom _instance;

        // Lock pour l'accès en multithreaded
        private static readonly object _lock = new object();

        // ==========================
        // Récupération du singleton
        // ==========================


        /// <summary>
        /// Get an instance of the singleton
        /// </summary>
        /// <returns> An instance of the singleton </returns>
        public static GestionnaireThreadCom GetInstance()
        {

            if (_instance == null)
            {

                lock (_lock)
                {

                    if (_instance == null)
                    {
                        _instance = new GestionnaireThreadCom();
                        _instance._lst_obj_threads_com = new List<Thread_communication>();
                        _instance._lst_threads_com = new List<Thread>();
                        // int[] portsDispos = { 10001, 10002, 10003, 10004, 10005, 10006, 10007 };
                        _instance._lst_localPort_dispo = new List<int>();
                        _instance._lst_remotePort_dispo = new List<int>();
                        _instance._compteur_id_thread_com = 0;
                        _instance._compteur_id_partie = 1;
                        _instance._s_compteur_id_partie = new Semaphore(1, 1);

                        var error_value = Tools.Errors.None;
                        var test = ServerParameters.GetConfig(ref error_value);
                        if (error_value != Tools.Errors.None) // Checking for errors.
                        {
                            // Setting the error value.
                            // TODO : GetConfig error
                            return null;
                        }

                        for (var i = 1; i < test.MaxNbPorts; i++)
                        {
                            _instance._lst_localPort_dispo.Add(test.LocalPort + i);
                            _instance._lst_remotePort_dispo.Add(test.RemotePort + i);
                        }
                    }
                }
            }
            return _instance;
        }

        // ==========================================
        // Méthodes privées, pour utilisation interne
        // =========================================

        /// <summary>
        /// Generate a new communication thread manager
        /// </summary>
        /// <returns> The position of the thread manager in the list _lst_obj_threads_com </returns>
        private static int Creation_thread_com()
        {

            lock (_lock)
            {
                if (_instance._lst_localPort_dispo.Count != 0 && _instance._lst_remotePort_dispo.Count != 0)
                {
                    int localPort_choisi = _instance._lst_localPort_dispo[0];
                    int remotePort_choisi = _instance._lst_remotePort_dispo[0];

                    Thread_communication thread_com = new Thread_communication(localPort_choisi, remotePort_choisi, _instance._compteur_id_thread_com);
                    _instance._compteur_id_thread_com++;
                    Thread nouv_thread = new Thread(new ThreadStart(thread_com.Lancement_thread_com));

                    _instance._lst_obj_threads_com.Add(thread_com);
                    _instance._lst_threads_com.Add(nouv_thread);

                    _instance._lst_localPort_dispo.RemoveAt(0);
                    _instance._lst_remotePort_dispo.RemoveAt(0);

                    nouv_thread.Start();

                    return _instance._lst_obj_threads_com.Count()-1;
                }
                else
                {
                    // Erreur : plus de ports disponibles pour un nouveau thread de communication
                    return -1;
                }
            }

            

        }

        // ====================
        // Méthodes publiques
        // ====================

        /// <summary>
        /// List all the rooms that exists
        /// </summary>
        /// <returns> A list of all rooms : first value is ID, then number of player and number of player max </returns>
        public string[] GetRoomList()
        {
            List<string> room_list = new List<string>();

            // Parcours des threads de communication pour lister ses rooms
            foreach (Thread_communication thread_com_iterateur in _instance._lst_obj_threads_com)
            {
                foreach (Thread_serveur_jeu thread_serv_ite in thread_com_iterateur.Get_list_server_thread())
                {
                    if (thread_serv_ite.Is_Private() == false && thread_serv_ite.Get_Status() != Tools.GameStatus.Stopped
                        && thread_serv_ite.Get_Status() != Tools.GameStatus.Running)
                    {
                        room_list.Add(thread_serv_ite.Get_ID().ToString());
                        room_list.Add(thread_serv_ite.Get_Moderateur().ToString());
                        room_list.Add(thread_serv_ite.NbJoueurs.ToString());
                        room_list.Add(thread_serv_ite.NbJoueursMax.ToString());
                        room_list.Add(((int)thread_serv_ite.Get_Mode()).ToString());
                    }            
                }
            }
            
            return room_list.ToArray();
        }

        /// <summary>
        /// Create a new room and return the port of the manager thread
        /// </summary>
        /// <param name="idPlayer"> Id of the player making this request </param>
        /// <param name="socket"> Socket of the player making this request </param>
        /// <returns> Returns a list containing the id then the port if all goes well, -1 otherwise </returns>
        public List<int> CreateRoom(ulong idPlayer, Socket? socket)
        {
            int portThreadCom = -1;
            int idNewRoom = -1;

            if (_lst_threads_com.Count == 0 && _lst_obj_threads_com.Count == 0)
            { // Aucun thread de comm n'existe

                int positionThreadCom = Creation_thread_com();

                if (positionThreadCom != -1)
                { // Seulement si un nouveau thread de com a pu être créé

                    // Demande de création d'une nouvelle partie dans le bon thread de com
                    _s_compteur_id_partie.WaitOne();
                    idNewRoom = _instance._lst_obj_threads_com[positionThreadCom].AddNewGame(idPlayer, socket, _compteur_id_partie);
                    _compteur_id_partie++;
                    _s_compteur_id_partie.Release();

                    if (idNewRoom != -1)
                    {
                        portThreadCom = _instance._lst_obj_threads_com[positionThreadCom].Get_remotePort();
                    }
                }            
            }
            else
            {
                bool thread_com_libre_trouve = false;

                // Parcours des différents threads de communication pour trouver un qui gère < 5 parties
                foreach (Thread_communication thread_com_iterateur in _lst_obj_threads_com)
                {
                    lock (thread_com_iterateur.Get_lock_nb_parties_gerees())
                    {
                        if (thread_com_iterateur.Get_nb_parties_gerees() < 5)
                        {
                            thread_com_libre_trouve = true;
                        }
                    }

                    if (thread_com_libre_trouve)
                    {
                        _s_compteur_id_partie.WaitOne();
                        idNewRoom = thread_com_iterateur.AddNewGame(idPlayer, socket, _compteur_id_partie);
                        _compteur_id_partie++;
                        _s_compteur_id_partie.Release();

                        if (idNewRoom != -1)
                        {
                            portThreadCom = thread_com_iterateur.Get_remotePort();
                        }

                        break; // Sort du foreach
                    }

                }

                // Si aucun des threads n'est libre pour héberger une partie de plus
                if (thread_com_libre_trouve == false)
                {

                    int positionThreadCom = Creation_thread_com();

                    if (positionThreadCom != -1)
                    { // Seulement si un nouveau thread de com a pu être créé

                        // Demande de création d'une nouvelle partie dans le bon thread de com
                        _s_compteur_id_partie.WaitOne();
                        idNewRoom = _instance._lst_obj_threads_com[positionThreadCom].AddNewGame(idPlayer, socket, _compteur_id_partie);
                        _compteur_id_partie++;
                        _s_compteur_id_partie.Release();

                        if (idNewRoom != -1)
                        {
                            portThreadCom = _instance._lst_obj_threads_com[positionThreadCom].Get_remotePort();
                        }
                    }

                }

            }

            List<int> listReturn = new List<int>{ idNewRoom, portThreadCom };

            return listReturn;
        }
        
        
        public void UpdateRoom(int idRoom, ulong idPlayer, string[] settings)
        {
            // Parcours des threads de communication pour trouver celui qui gère la partie cherchée
            foreach (Thread_communication thread_com_iterateur in _instance._lst_obj_threads_com)
            {
                foreach (Thread_serveur_jeu thread_serv_ite in thread_com_iterateur.Get_list_server_thread())
                {
                    if (idRoom != thread_serv_ite.Get_ID()) continue;
                    thread_serv_ite.Set_Settings(idPlayer, settings);
                    thread_com_iterateur.SendBroadcast(idRoom, Tools.IdMessage.RoomSettingsGet, idPlayer, thread_serv_ite.Get_Settings());
                    return;
                }
            }
        }
        
        public string[] SettingsRoom(int idRoom)
        {
            // Parcours des threads de communication pour trouver celui qui gère la partie cherchée
            foreach (Thread_communication thread_com_iterateur in _instance._lst_obj_threads_com)
            {
                foreach (Thread_serveur_jeu thread_serv_ite in thread_com_iterateur.Get_list_server_thread())
                {
                    if (idRoom == thread_serv_ite.Get_ID())
                        return thread_serv_ite.Get_Settings();
                }
            }

            return Array.Empty<string>();
        }

        public int CallAskPort(int idRoom)
        {
            // Parcours des threads de communication pour trouver celui qui gère la partie cherchée
            foreach (Thread_communication thread_com_iterateur in _instance._lst_obj_threads_com)
            {
                foreach (Thread_serveur_jeu thread_serv_ite in thread_com_iterateur.Get_list_server_thread())
                {
                    if (idRoom != thread_serv_ite.Get_ID()) continue;
                    return thread_com_iterateur.Get_remotePort();
                }
            }

            return -1;
        }

        public Tools.Errors JoinPlayer(int idRoom, ulong idPlayer, Socket? playerSocket)
        {
            List<string> dataPlayerName= new List<string>();
            var db = new Database();

            // Parcours des threads de communication pour trouver celui qui gère la partie cherchée
            foreach (Thread_communication thread_com_iterateur in _instance._lst_obj_threads_com)
            {
                foreach (Thread_serveur_jeu thread_serv_ite in thread_com_iterateur.Get_list_server_thread())
                {
                    if (idRoom != thread_serv_ite.Get_ID()) continue;
                    var playerStatus = thread_serv_ite.AddJoueur(idPlayer, playerSocket);                 
                    if (playerStatus == Tools.PlayerStatus.Full) // La room est pleine
                    {
                        return Tools.Errors.RoomJoin;
                    }
                    string playerName = db.GetPseudo((int)idPlayer);
                    thread_serv_ite.Get_Dico_Joueurs()[idPlayer].SetName(playerName);
                    dataPlayerName.Add(playerName);
                    thread_com_iterateur.SendBroadcast(idRoom, Tools.IdMessage.PlayerJoin, idPlayer, dataPlayerName.ToArray());

                    return Tools.Errors.None;

                }
            }

            return Tools.Errors.None;
        }

        public Tools.PlayerStatus RemovePlayer(int idRoom, ulong idPlayer)
        {
            // Parcours des threads de communication pour trouver celui qui gère la partie cherchée
            foreach (Thread_communication thread_com_iterateur in _instance._lst_obj_threads_com)
            {
                Tools.PlayerStatus playerStatus = thread_com_iterateur.PlayerLeave(idPlayer, idRoom);
                if(playerStatus == Tools.PlayerStatus.Success)
                    thread_com_iterateur.SendBroadcast(idRoom, Tools.IdMessage.PlayerLeave, idPlayer);
                return playerStatus;
            }

            return Tools.PlayerStatus.NotFound;
        }
        
        public Tools.PlayerStatus KickPlayer(int idRoom, ulong idModo, ulong idPlayer)
        {
            // Parcours des threads de communication pour trouver celui qui gère la partie cherchée
            foreach (Thread_communication thread_com_iterateur in _instance._lst_obj_threads_com)
            {
                foreach (Thread_serveur_jeu thread_serv_ite in thread_com_iterateur.Get_list_server_thread())
                {
                    if (idRoom != thread_serv_ite.Get_ID()) continue;
                    if (idModo != thread_serv_ite.Get_Moderateur())
                        return Tools.PlayerStatus.Permissions;
                    return thread_serv_ite.RemoveJoueur(idPlayer);
                }
            }

            return Tools.PlayerStatus.NotFound;
        }
        
        public Tools.PlayerStatus ReadyPlayer(int idRoom, ulong idPlayer)
        {
            // Parcours des threads de communication pour trouver celui qui gère la partie cherchée
            foreach (Thread_communication thread_com_iterateur in _instance._lst_obj_threads_com)
            {
                foreach (Thread_serveur_jeu thread_serv_ite in thread_com_iterateur.Get_list_server_thread())
                {
                    if (idRoom != thread_serv_ite.Get_ID()) continue;
                    
                    var before = thread_serv_ite.EveryoneIsReady();
                    Tools.PlayerStatus playerStatusReturn = thread_serv_ite.SetPlayerStatus(idPlayer);
                    var after = thread_serv_ite.EveryoneIsReady();
                    
                    if(playerStatusReturn == Tools.PlayerStatus.Success)
                    {
                        if (after == false)
                        {
                            // Envoie de quoi afficher le player ready à tt le mondes
                            thread_com_iterateur.SendBroadcast(idRoom, Tools.IdMessage.PlayerReady, idPlayer);
                        }

                        if (before == false && after == true)
                        {
                            playerStatusReturn = Tools.PlayerStatus.LastPlayerReady;
                        }
                            
                        // Si tout le monde est prêt, inutile de broadcast le ready : la game va se lancer presque immédiatement
                        
                    }
                    return playerStatusReturn;
                }
            }

            return Tools.PlayerStatus.NotFound;
        }

        public string[] CallPlayerList(int idRoom)
        {
            List<string> listPlayerAndName = new List<string>();

            // Parcours des threads de communication pour trouver celui qui gère la partie cherchée
            foreach (Thread_communication thread_com_iterateur in _instance._lst_obj_threads_com)
            {
                foreach (Thread_serveur_jeu thread_serv_ite in thread_com_iterateur.Get_list_server_thread())
                {
                    if (idRoom != thread_serv_ite.Get_ID()) continue;
                    foreach (var joueur in thread_serv_ite.Get_Dico_Joueurs())
                    {
                        string playerName = joueur.Value.GetName();
                        listPlayerAndName.Add(joueur.Key.ToString());
                        listPlayerAndName.Add(playerName);
                    }

                    return listPlayerAndName.ToArray();
                }
            }

            return listPlayerAndName.ToArray();
        }
        
        public string[] CallPlayersStatus(int idRoom)
        {
            List<string> listPlayerAndNameAndStatus = new List<string>();

            // Parcours des threads de communication pour trouver celui qui gère la partie cherchée
            foreach (Thread_communication thread_com_iterateur in _instance._lst_obj_threads_com)
            {
                foreach (Thread_serveur_jeu thread_serv_ite in thread_com_iterateur.Get_list_server_thread())
                {
                    if (idRoom != thread_serv_ite.Get_ID()) continue;
                    foreach (var joueur in thread_serv_ite.Get_Dico_Joueurs())
                    {
                        string playerName = joueur.Value.GetName();
                        string playerStatus = joueur.Value.GetReady().ToString();
                        listPlayerAndNameAndStatus.Add(joueur.Key.ToString());
                        listPlayerAndNameAndStatus.Add(playerName);
                        listPlayerAndNameAndStatus.Add(playerStatus);
                    }

                    return listPlayerAndNameAndStatus.ToArray();
                }
            }

            return listPlayerAndNameAndStatus.ToArray();
        }

        public List<ulong> CallPlayerCurrent(int idRoom, ulong idPlayer)
        {
            var currentPlayer = new List<ulong>();
            currentPlayer.Add(0);
            currentPlayer.Add(0);

            // Parcours des threads de communication pour trouver celui qui gère la partie cherchée
            foreach (Thread_communication thread_com_iterateur in _instance._lst_obj_threads_com)
            {
                foreach (Thread_serveur_jeu thread_serv_ite in thread_com_iterateur.Get_list_server_thread())
                {
                    if (idRoom != thread_serv_ite.Get_ID() || thread_serv_ite.Get_Status() != Tools.GameStatus.Running ||
                        thread_serv_ite.Get_Dico_Joueurs().ContainsKey(idPlayer) == false) continue;
                    currentPlayer[0] = thread_serv_ite.Get_ActualPlayerId();
                    if(currentPlayer[0] != 0)
                    {
                        if (thread_serv_ite.IsRiverExtensionOn() && thread_serv_ite.Get_rivieresGame().Count > 0)
                        {
                            currentPlayer[1] = 0;
                        }
                        else
                        {
                            currentPlayer[1] = thread_serv_ite.Get_PlayerMeeples(currentPlayer[0]);
                        }
                    }                                              
                    return currentPlayer;
                }
            }
            
            return currentPlayer;
        }

        public void CallPlayerKick(int idRoom, ulong idPlayer, string[] data)
        {
            foreach (Thread_communication thread_com_iterateur in _instance._lst_obj_threads_com)
            {
                if (thread_com_iterateur.Get_id_parties_gerees().Contains(idRoom))
                {
                    thread_com_iterateur.PlayerKick(idRoom, idPlayer, data);
                }
                break;
           
            }
        }

        public Tools.PlayerStatus LogoutPlayer(ulong idPlayer)
        {
            Tools.PlayerStatus playerStatus = Tools.PlayerStatus.Default;
            // Parcours des threads de communication pour trouver ceux qui contiennent le joueur.
            foreach (Thread_communication thread_com_iterateur in _instance._lst_obj_threads_com)
            {
                foreach (Thread_serveur_jeu thread_serv_ite in thread_com_iterateur.Get_list_server_thread())
                {
                    if(thread_serv_ite.Get_Dico_Joueurs().ContainsKey(idPlayer) == false) continue;
                    if (playerStatus == Tools.PlayerStatus.Success)
                        thread_serv_ite.RemoveJoueur(idPlayer);
                    else
                        playerStatus = thread_serv_ite.RemoveJoueur(idPlayer);

                }
            }
            return playerStatus;
        }
        
        /// <summary>
        ///     Called everytime someone clicks on "ready" and starts the game if everyone is ready
        /// </summary>
        /// <param name="idRoom"></param>
        /// <param name="playerSocket"></param>
        /// <returns></returns>
        public Tools.Errors StartGame(int idRoom)
        {
            Tools.Errors errors = Tools.Errors.NotFound;

            // Parcours des threads de communication pour trouver celui qui gère la partie cherchée
            foreach (Thread_communication thread_com_iterateur in _instance._lst_obj_threads_com)
            {
                foreach (Thread_serveur_jeu thread_serv_ite in thread_com_iterateur.Get_list_server_thread())
                {
                    if (idRoom != thread_serv_ite.Get_ID()) continue;
                    if (thread_serv_ite.NbJoueurs < 2)
                        return Tools.Errors.NbPlayers;
                    if (thread_serv_ite.EveryoneIsReady())
                    {
                        // Lancement de la game
                        ulong idTuileInit = thread_serv_ite.StartGame();
                        string[] dataStartGame = new string[] { idTuileInit.ToString() };
                        // Préviens tous les joueurs (broadcast start)
                        thread_com_iterateur.SendBroadcast(idRoom, Tools.IdMessage.StartGame, dataStartGame);
                        // Stockage des 3 tuiles qui seront envoyées lors de la demande de tuileDraw
                        thread_serv_ite.Set_tuilesEnvoyees(thread_serv_ite.GetThreeLastTiles());
                        return Tools.Errors.None; // return valeur correcte
                    }
                    else // Des joueurs ne sont pas prêts
                    {
                        return Tools.Errors.PlayerReady;
                    }
                }
            }

            return errors;
        }

        public string[] CallDrawTile(ulong idPlayer, int idRoom, Socket? playerSocket)
        {
            string[] stringReturnEmpty = Array.Empty<string>();

            foreach (Thread_communication thread_com_iterateur in _instance._lst_obj_threads_com)
            {
                foreach (Thread_serveur_jeu threadJeu in thread_com_iterateur.Get_list_server_thread())
                {
                    if (idRoom != threadJeu.Get_ID() || threadJeu.Get_Dico_Joueurs().ContainsKey(idPlayer) == false) continue;
                    return threadJeu.GetThreeLastTiles();
                }                  
            }

            return stringReturnEmpty;
        }

        public void CallTileVerif(ulong idPlayer, Socket? playerSocket, Tools.Errors errors, int idRoom, ulong idTuile, Position posTuile)
        {
            // Parcours des threads de communication pour trouver celui qui gère la partie cherchée
            foreach (Thread_communication thread_com_iterateur in _instance._lst_obj_threads_com)
            {
                // Thread de com gérant la partie trouvé
                if (thread_com_iterateur.Get_id_parties_gerees().Contains(idRoom))
                {
                    thread_com_iterateur.TileVerifAntiCheat(idPlayer, playerSocket, errors, idRoom, idTuile, posTuile);
                }
            }
        }

        /// <summary>
        /// Verify the placement of the tile
        /// </summary>
        /// <param name="idPlayer"></param>
        /// <param name="playerSocket"></param>
        /// <param name="idRoom"></param>
        /// <param name="idTuile"></param>
        /// <param name="posX"></param>
        /// <param name="posY"></param>
        /// <param name="rotat"></param>
        /// <returns> Errors.Permission if it is not the actual player, IllegalPlay if incorrect place or None if all goes well </returns>
        public Tools.Errors CallVerifyTilePlacement(ulong idPlayer, Socket? playerSocket, int idRoom, string idTuile, string posX, string posY, string rotat)
        {
            // Si la demande ne trouve pas de partie ou qu'elle ne provient pas d'un joueur à qui c'est le tour : permission error
            Tools.Errors errors = Tools.Errors.Permission;

            Console.WriteLine("DEBUG_TuilePlacement : Entrée dans GestionnaireCom");

            // Parcours des threads de communication pour trouver celui qui gère la partie cherchée
            foreach (Thread_communication thread_com_iterateur in _instance._lst_obj_threads_com)
            {
                // Thread de com gérant la partie trouvé
                if (thread_com_iterateur.Get_id_parties_gerees().Contains(idRoom))
                {
                    Console.WriteLine("CallVerifyTilePlacement : Room was found");
                    errors = thread_com_iterateur.VerifyTilePlacement(idPlayer, playerSocket, idRoom, idTuile, posX, posY, rotat);
                    break;
                }

            }
            return errors; // return valeur correcte
        }

        public Tools.Errors CallVerifyPionPlacement(ulong idPlayer, Socket? playerSocket, int idRoom, string[] data)
        {
            // Si la demande ne trouve pas de partie ou qu'elle ne provient pas d'un joueur à qui c'est le tour : permission error
            Tools.Errors errors = Tools.Errors.Permission;

            // Parcours des threads de communication pour trouver celui qui gère la partie cherchée
            foreach (Thread_communication thread_com_iterateur in _instance._lst_obj_threads_com)
            {
                // Thread de com gérant la partie trouvé
                if (thread_com_iterateur.Get_id_parties_gerees().Contains(idRoom))
                {
                    errors = thread_com_iterateur.VerifyPionPlacement(idPlayer, playerSocket, idRoom, data);
                    break;
                }

            }
            return errors; // return valeur correcte
        }

        public Tools.Errors CallCancelTuilePlacement(ulong idPlayer, Socket? playerSocket, int idRoom)
        {
            // Si la demande ne trouve pas de partie ou qu'elle ne provient pas d'un joueur à qui c'est le tour : permission error
            Tools.Errors errors = Tools.Errors.Permission;

            // Parcours des threads de communication pour trouver celui qui gère la partie cherchée
            foreach (Thread_communication thread_com_iterateur in _instance._lst_obj_threads_com)
            {
                // Thread de com gérant la partie trouvé
                if (thread_com_iterateur.Get_id_parties_gerees().Contains(idRoom))
                {
                    errors = thread_com_iterateur.CancelTuilePlacement(idPlayer, playerSocket, idRoom);
                    break;
                }

            }
            return errors; // return valeur correcte
        }

        public Tools.Errors CallCancelPionPlacement(ulong idPlayer, Socket? playerSocket, int idRoom)
        {
            // Si la demande ne trouve pas de partie ou qu'elle ne provient pas d'un joueur à qui c'est le tour : permission error
            Tools.Errors errors = Tools.Errors.Permission;

            // Parcours des threads de communication pour trouver celui qui gère la partie cherchée
            foreach (Thread_communication thread_com_iterateur in _instance._lst_obj_threads_com)
            {
                // Thread de com gérant la partie trouvé
                if (thread_com_iterateur.Get_id_parties_gerees().Contains(idRoom))
                {
                    errors = thread_com_iterateur.CancelPionPlacement(idPlayer, playerSocket, idRoom);
                    break;
                }

            }
            return errors; // return valeur correcte
        }

        public Tools.Errors CallEndTurn(ulong idPlayer, int idRoom, Socket? playerSocket, string[] data)
        {
            // Si la demande ne trouve pas de partie ou qu'elle ne provient pas d'un joueur à qui c'est le tour : permission error
            Tools.Errors errors = Tools.Errors.Permission;

            // Parcours des threads de communication pour trouver celui qui gère la partie cherchée
            foreach (Thread_communication thread_com_iterateur in _instance._lst_obj_threads_com)
            {
                // Thread de com gérant la partie trouvé
                if (thread_com_iterateur.Get_id_parties_gerees().Contains(idRoom))
                {
                    Console.WriteLine("CallEndTurn : idRoom was found !");
                    errors = thread_com_iterateur.Com_EndTurn(idPlayer, idRoom, playerSocket, data);
                    break;
                }

            }
            return errors; // return valeur correcte
        }

        public void CallForceEndTurn(ulong idPlayer, int idRoom, string[] data)
        {
            // Parcours des threads de communication pour trouver celui qui gère la partie cherchée
            foreach (Thread_communication thread_com_iterateur in _instance._lst_obj_threads_com)
            {
                // Thread de com gérant la partie trouvé
                if (thread_com_iterateur.Get_id_parties_gerees().Contains(idRoom))
                {
                    Console.WriteLine("CallForceEndTurn : idRoom was found !");
                    thread_com_iterateur.ForceEndTurn(idPlayer, idRoom, data);
                    break;
                }

            }
        }
        
        public void CallForceCloseRoom(int idRoom)
        {
            // Parcours des threads de communication pour trouver celui qui gère la partie cherchée
            foreach (Thread_communication thread_com_iterateur in _instance._lst_obj_threads_com)
            {
                // Thread de com gérant la partie trouvé
                if (thread_com_iterateur.Get_id_parties_gerees().Contains(idRoom))
                {
                    Console.WriteLine("CallForceCloseRoom : idRoom was found !");
                    thread_com_iterateur.ForceCloseRoom(idRoom);
                    break;
                }
            }
        }
        
        
        public void CallChooseIdTile(ulong idPlayer, int idRoom, ulong idTuile, Position pos, Socket? playerSocket)
        {
            // Parcours des threads de communication pour trouver celui qui gère la partie cherchée
            foreach (Thread_communication thread_com_iterateur in _instance._lst_obj_threads_com)
            {
                // Thread de com gérant la partie trouvé
                if (thread_com_iterateur.Get_id_parties_gerees().Contains(idRoom))
                {
                    thread_com_iterateur.ChooseIdTile(idRoom, idPlayer, idTuile, pos, playerSocket);
                    break;
                }

            }
        }
    }
}
