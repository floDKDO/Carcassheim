using System;
using System.Threading;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using ClassLibrary;
using Server;


namespace system
{

    public class Thread_communication
    {
        // Attributs

        private int _id_thread_com;
        private int _numero_localPort;
        private int _numero_remotePort;
        private int _nb_parties_gerees;

        private List<int> _id_parties_gerees;
        private List<Thread_serveur_jeu> _lst_serveur_jeu;

        // Locks

        private readonly object _lock_nb_parties_gerees;
        private readonly object _lock_id_parties_gerees;

        // =============
        // Constructeur
        // =============

        public Thread_communication(int num_localPort, int num_remotePort, int id)
        {
            _numero_localPort = num_localPort;
            _numero_remotePort = num_remotePort;
            _nb_parties_gerees = 0;
            _id_parties_gerees = new List<int>();
            _id_thread_com = id;
            _lst_serveur_jeu = new List<Thread_serveur_jeu>();
            _lock_nb_parties_gerees = new object();
            _lock_id_parties_gerees = new object();
        }

        // ==================
        // Getters et setters
        // ==================

        public int Get_nb_parties_gerees()
        {
            return _nb_parties_gerees;
        }

        public List<int> Get_id_parties_gerees()
        {
            return _id_parties_gerees;
        }

        public object Get_lock_nb_parties_gerees()
        {
            return _lock_nb_parties_gerees;
        }

        public object Get_lock_id_parties_gerees()
        {
            return _lock_id_parties_gerees;
        }

        public int Get_localPort()
        {
            return _numero_localPort;
        }
        
        public int Get_remotePort()
        {
            return _numero_remotePort;
        }

        public List<Thread_serveur_jeu> Get_list_server_thread()
        {
            return this._lst_serveur_jeu;
        }

        // ========================
        // Méthodes communication
        // ========================

        public void SendUnicast(int idRoom, Tools.IdMessage idMessage, Socket? playerSocket, ulong idPlayer, string[] data)
        {
            // Generate packet
            Packet packet = new Packet();
            packet.IdMessage = idMessage;
            packet.IdPlayer = idPlayer;
            packet.IdRoom = idRoom;
            packet.Data = data;

            // Send to the client
            Server.Server.SendToSpecificClient(playerSocket, packet);

        }

        /// <summary>
        /// Broadcasts a message to all except the player initiating the request
        /// </summary>
        /// <param name="roomId"></param>
        /// <param name="idMessage"></param>
        /// <param name="idPlayer"> Player originating the broadcast </param>
        /// <param name="data"></param>
        public void SendBroadcast(int roomId, Tools.IdMessage idMessage, ulong idPlayer, string[] data)
        {
            // Generate packet
            Packet packet = new Packet();
            packet.IdMessage = idMessage;
            packet.IdRoom = roomId;
            packet.IdPlayer = idPlayer;

            packet.Data = data;

            foreach (Thread_serveur_jeu thread_serv_ite in Get_list_server_thread())
            {
                if (thread_serv_ite.Get_ID() == roomId)
                {
                    // Envoi à chaque joueur
                    foreach (var joueur in thread_serv_ite.Get_Dico_Joueurs())
                    {
                        // On envoie le display à tous sauf au joueur dont c'est l'action (si tuileDrawn on envoit à tous)
                        if (joueur.Key != idPlayer || idMessage is Tools.IdMessage.RoomSettingsGet or Tools.IdMessage.TuileDraw 
                            or Tools.IdMessage.PionPlacement or Tools.IdMessage.PlayerKick) 
                        {
                            Console.WriteLine("SendBroadcast : to " + joueur.Value._id_player + "!");
                            Server.Server.SendToSpecificClient(joueur.Value._socket_of_player, packet);
                        }
                    }
                }
            }
        }

        // -----------------------------------------
        // Surcharges de la méthode de communication
        // -----------------------------------------

        public void SendBroadcast(int roomId, Tools.IdMessage idMessage)
        {
            SendBroadcast(roomId, idMessage, 0, Array.Empty<string>());
        }

        public void SendBroadcast(int roomId, Tools.IdMessage idMessage, ulong idPlayer)
        {
            SendBroadcast(roomId, idMessage, idPlayer, Array.Empty<string>());
        }

        public void SendBroadcast(int roomId, Tools.IdMessage idMessage, string[] data)
        {
            SendBroadcast(roomId, idMessage, 0, data);
        }

        // =================
        // Méthodes moteur
        // =================

        /// <summary>
        /// Add a new game (room)
        /// </summary>
        /// <param name="playerId"> Id of the moderator </param>
        /// <param name="socket"> Socket of the moderator (first player) </param>
        /// <param name="idParty"> Id of the party (first player) </param>
        /// <returns> The id of the game (-1 if error occurs) </returns>
        public int AddNewGame(ulong playerId, Socket? playerSocket, int idParty)
        {

            int id_nouv_partie = -1;

            lock (this._lock_nb_parties_gerees)
            {
                if (_nb_parties_gerees < 5)
                {
                    id_nouv_partie = idParty;

                    lock (this._lock_id_parties_gerees)
                    {
                        _id_parties_gerees.Add(idParty);
                    }

                    _nb_parties_gerees++;
                }
            }

            if (id_nouv_partie != -1) // Si la partie a pu être crée
            {
                Thread_serveur_jeu thread_serveur_jeu = new Thread_serveur_jeu(id_nouv_partie, playerId, playerSocket);

                _lst_serveur_jeu.Add(thread_serveur_jeu);
                thread_serveur_jeu.startRoomTimer();

                return id_nouv_partie;

            }
            else // La partie n'a pas pu être créée
            {
                return id_nouv_partie;
            }


        }

        public void DeleteGame(int roomId)
        {
            int indexOfRoom = 0;
            foreach (Thread_serveur_jeu thread_serv_ite in Get_list_server_thread())
            {
                if (thread_serv_ite.Get_ID() == roomId)
                {
                    lock (_lock_nb_parties_gerees)
                    {
                        _nb_parties_gerees--;
                    }
                    lock (_lock_id_parties_gerees)
                    {
                        var idToRemove = _id_parties_gerees.Single(id => id.Equals(roomId));
                        thread_serv_ite.stopRoomTimer();
                        _id_parties_gerees.Remove(idToRemove);
                    }
                    break;
                }

                indexOfRoom++;
            }

            _lst_serveur_jeu.RemoveAt(indexOfRoom);
        }

        public void TileVerifAntiCheat(ulong idPlayer, Socket? playerSocket, Tools.Errors errors, int idRoom, ulong idTuile, Position posTuile)
        {
            foreach (Thread_serveur_jeu threadJeu in Get_list_server_thread())
            {
                if(threadJeu.Get_ID() == idRoom)
                {
                    if (errors == Tools.Errors.Data) // Aucune tuile n'est pas posable selon ce client
                    {
                        threadJeu.SetACBarrier(); // Sets the barrier if inexistant


                        // On attend que tous les joueurs aient exécuté leur rôle d'arbitre
                        threadJeu.WaitACBarrier();

                        if (idPlayer == threadJeu.Get_ActualPlayerId()) // Joueur actuel
                        {
                            // Vérification de la validité des tuiles piochées
                            if (threadJeu.Get_AC_drawedTilesValid())
                            {
                                // Les tuiles s'avèrent valides, on a affaire à un tricheur
                                PlayerCheated(idPlayer, playerSocket, idRoom);
                                // On renvoie les 3 mêmes tuiles
                                SendUnicast(idRoom, Tools.IdMessage.TuileDraw, playerSocket, idPlayer, threadJeu.GetThreeLastTiles());
                            }
                            else
                            {
                                // En effet aucune tuile n'est valide, nous renvoyons trois nouvelles tuiles
                                threadJeu.ShuffleTilesGame();
                                threadJeu.Set_tuilesEnvoyees(threadJeu.GetThreeLastTiles());
                                SendUnicast(idRoom, Tools.IdMessage.TuileDraw, playerSocket, idPlayer, threadJeu.GetThreeLastTiles());
                            }

                            // Destruction de la barrière par le joueur actu
                            threadJeu.DisposeACBarrier();
                        }                   

                    }
                    else if (errors == Tools.Errors.None) // Une tuile est posable selon ce client
                    {

                        threadJeu.SetACBarrier(); // Sets the barrier if inexistant

                        if (idPlayer != threadJeu.Get_ActualPlayerId()) // Réponse des autres joueurs (sert de vérif)
                        {
                            bool isLegal = threadJeu.isTilePlacementLegal(idTuile, posTuile.X, posTuile.Y, posTuile.ROT);
                            if (isLegal) // S'il s'avère que le coup est valide, on passe l'attribut à true
                            {
                                threadJeu.SetValid_AC_drawedTilesValid(idTuile);
                            }
                            Console.WriteLine("* VerifAC : idPlayer=" + idPlayer.ToString() + " currentPlayer=" + 
                                threadJeu.Get_ActualPlayerId().ToString() + " verified tile of id=" + idTuile.ToString() + 
                                " and has decreted it as " + isLegal.ToString());
                            Console.WriteLine("* VerifAC_suite : position tried = " + posTuile.ToString());
                        }

                        // Signale que le rôle d'arbitre de ce joueur a été joué
                        threadJeu.WaitACBarrier();

                        if (idPlayer == threadJeu.Get_ActualPlayerId())  // Réponse du joueur actuel
                        {
                            // Vérification de la validité des tuiles piochées ET de l'id de la première tuile valide
                            if (threadJeu.Get_AC_drawedTilesValid() && idTuile == threadJeu.Get_AC_idFirstValidTile())
                            {
                                // Tout est bon, définition de la tuile choisie
                                ChooseIdTile(idRoom, idPlayer, idTuile, posTuile, playerSocket);
                            }
                            else
                            {
                                Console.WriteLine("** AC_Verif - TRICHEUR : idPremière tuile valide = " + threadJeu.Get_AC_idFirstValidTile() + " | idTuile = "
                                    + idTuile);
                                // Les tuiles s'avèrent non-valide OU l'id de la tuile choisie n'est pas la première à être valide
                                PlayerCheated(idPlayer, playerSocket, idRoom);
                                // On renvoie les 3 mêmes tuiles
                                SendUnicast(idRoom, Tools.IdMessage.TuileDraw, playerSocket, idPlayer, threadJeu.GetThreeLastTiles());

                            }

                            // Destruction de la barrière par le joueur actu
                            threadJeu.DisposeACBarrier();
                        }

                        
                    }

                    break;
                }
            }
        }


        public void ChooseIdTile(int idRoom, ulong idPlayer, ulong idTuile, Position exemplePos, Socket? playerSocket)
        {
            foreach (Thread_serveur_jeu threadJeu in Get_list_server_thread())
            {
                if (threadJeu.Get_ID() == idRoom)
                {
                    // Vérifie que l'exemple de position est bon
                    bool isLegal = threadJeu.isTilePlacementLegal(idTuile, exemplePos.X, exemplePos.Y, exemplePos.ROT);
                    if (isLegal) // Si tuile en effet posable
                    {
                        // Sauvegarde de l'id de la tuile choisie
                        threadJeu.Set_idTuileChoisie(idTuile);
                    }
                    else
                    {
                        Console.WriteLine("** ChooseIdTile - TRICHEUR : placement suivant illégal -> " + idTuile.ToString() + " " + exemplePos.ToString());
                        // La tuile n'est pas vraiment posable
                        PlayerCheated(idPlayer, playerSocket, idRoom);

                        // Renvoie les 3 tuiles
                        string[] tuilesAEnvoyer = threadJeu.GetThreeLastTiles();
                        SendUnicast(idRoom, Tools.IdMessage.TuileDraw, playerSocket, idPlayer, threadJeu.GetThreeLastTiles());
                    }

                    break;
                }
            }
        }

        public Tools.Errors VerifyTilePlacement(ulong idPlayer, Socket? playerSocket, int idRoom, string idTuile, string posX, string posY, string rotat)
        {
            // Si la demande ne trouve pas de partie ou qu'elle ne provient pas d'un joueur à qui c'est le tour : permission error
            Tools.Errors errors = Tools.Errors.Permission;

            Console.WriteLine("DEBUG_TuilePlacement : Entrée dans Thread_comm");

            // Parcours des threads de jeu pour trouver celui qui gère la partie cherchée

            foreach (Thread_serveur_jeu thread_serv_ite in _lst_serveur_jeu)
            {
                if (idRoom != thread_serv_ite.Get_ID()) continue;
                Console.WriteLine("VerifyTilePlacement : ID was found");
                if (idPlayer == thread_serv_ite.Get_ActualPlayerId())
                {
                    Console.WriteLine("VerifyTilePlacement : current player");
                    if (thread_serv_ite.Get_idTuileChoisie() != UInt64.Parse(idTuile)) // Vérifie qu'il s'agit de la même qu'il essaie de poser
                    {
                        Console.WriteLine("VerifyTilePlacement : CHEAT");
                        // Coup illégal : tentative de pose d'une autre tuile que celle choisie
                        errors = Tools.Errors.IllegalPlay;
                        break;
                    }
                    else
                    {
                        // Vérification du placement
                        errors = thread_serv_ite.TilePlacement(idPlayer, UInt64.Parse(idTuile), Int32.Parse(posX), Int32.Parse(posY), Int32.Parse(rotat));
                        Console.WriteLine("DEBUG_TuilePlacement : error = " + errors);
                        
                        if (errors == Tools.Errors.None) // Si coup légal
                        {
                            Console.WriteLine("DEBUG_TuilePlacement : Broadcast (avant)");

                            // Envoi de l'information à tous pour l'affichage
                            string[] dataToSend = new string[] { idTuile, posX, posY, rotat };
                            SendBroadcast(idRoom, Tools.IdMessage.TuilePlacement, idPlayer, dataToSend);

                            Console.WriteLine("DEBUG_TuilePlacement : Broadcast (après)");
                        }
                        else
                        {
                            Console.WriteLine("VerifyTilePlacement : TilePlacement ERROR");
                        }
                            
                        break;
                    }

                     
                }
                else
                {
                    Console.WriteLine("VerifyTilePlacement : not current player " + thread_serv_ite.Get_ActualPlayerId());
                }

            }
            
            if(errors == Tools.Errors.IllegalPlay)
            {
                PlayerCheated(idPlayer, playerSocket, idRoom);
            }

            return errors; // return valeur correcte
        }

        public Tools.Errors VerifyPionPlacement(ulong idPlayer, Socket? playerSocket, int idRoom, string[] data)
        {
            // Si la demande ne trouve pas de partie ou qu'elle ne provient pas d'un joueur à qui c'est le tour : permission error
            Tools.Errors errors = Tools.Errors.Permission;

            // Parcours des threads de jeu pour trouver celui qui gère la partie cherchée

            foreach (Thread_serveur_jeu thread_serv_ite in _lst_serveur_jeu)
            {
                if (idRoom != thread_serv_ite.Get_ID()) continue;
                if (idPlayer == thread_serv_ite.Get_ActualPlayerId())
                {
                    // Vérification qu'une tuile a bien été placée auparavant ET qu'aucun pion n'est pas déjà placé
                    if(thread_serv_ite.Get_posTuileTourActu().IsExisting() == true && 
                        thread_serv_ite.Get_posPionTourActu().Length == 0)
                    {
                        // Vérification qu'il lui reste un pion
                        if(thread_serv_ite.Get_Dico_Joueurs()[idPlayer]._nbMeeples < 1)
                        {
                            return Tools.Errors.Permission;
                        }

                        // Vérification du placement
                        var posTuile = new Position(int.Parse(data[1]), int.Parse(data[2]), int.Parse(data[3]));
                        var idMeeple = ulong.Parse(data[4]);
                        var slotPos = int.Parse(data[5]);
                        
                        errors = thread_serv_ite.PionPlacement(idPlayer, posTuile, idMeeple, slotPos);

                        if(errors == Tools.Errors.None) // Si placement légal
                        {
                            // Envoi de l'information à tous pour l'affichage 
                            SendBroadcast(idRoom, Tools.IdMessage.PionPlacement, idPlayer, data);
                        }
                        
                        break;
                    }

                    // Dans le cas où aucun pion n'a pas été placée auparavant ou qu'un pion est déjà placé,
                    // on renvoie une erreur Permission                 
                }

            }

            if (errors == Tools.Errors.IllegalPlay)
            {
                PlayerCheated(idPlayer, playerSocket, idRoom);
            }

            return errors; // return valeur correcte
        }

        public Tools.Errors CancelTuilePlacement(ulong idPlayer, Socket? playerSocket, int idRoom)
        {
            // Si la demande ne trouve pas de partie ou qu'elle ne provient pas d'un joueur à qui c'est le tour : permission error
            Tools.Errors errors = Tools.Errors.Permission;

            // Parcours des threads de jeu pour trouver celui qui gère la partie cherchée

            foreach (Thread_serveur_jeu thread_serv_ite in _lst_serveur_jeu)
            {
                if (idRoom != thread_serv_ite.Get_ID()) continue;
                if (idPlayer == thread_serv_ite.Get_ActualPlayerId())
                {
                    // Vérification qu'une tuile a bien été placée auparavant ET qu'un pion n'y est pas déjà placé
                    if (thread_serv_ite.Get_posTuileTourActu().IsExisting() == true &&
                        thread_serv_ite.Get_posPionTourActu().Length == 0)
                    {
                        // Retrait de la position de tuile
                        thread_serv_ite.RetirerTuileTourActu();

                        // Envoie l'information display
                        SendBroadcast(idRoom, Tools.IdMessage.CancelTuilePlacement, idPlayer);

                        errors = Tools.Errors.None;
                        break;
                    }

                    // Dans le cas où aucune tuile n'a été placée auparavant ou qu'un pion est toujours placé,
                    // on renvoie une erreur Permission                 
                }

            }

            return errors; 
        }

        public Tools.Errors CancelPionPlacement(ulong idPlayer, Socket? playerSocket, int idRoom)
        {
            // Si la demande ne trouve pas de partie ou qu'elle ne provient pas d'un joueur à qui c'est le tour : permission error
            Tools.Errors errors = Tools.Errors.Permission;

            // Parcours des threads de jeu pour trouver celui qui gère la partie cherchée

            foreach (Thread_serveur_jeu thread_serv_ite in _lst_serveur_jeu)
            {
                if (idRoom != thread_serv_ite.Get_ID()) continue;
                if (idPlayer == thread_serv_ite.Get_ActualPlayerId())
                {
                    // Vérification qu'un pion a bien été placée auparavant
                    if (thread_serv_ite.Get_posPionTourActu().Length != 0)
                    {
                        // Retrait de la position du pion
                        thread_serv_ite.RetirerPionTourActu();

                        // Envoie l'information display
                        SendBroadcast(idRoom, Tools.IdMessage.CancelPionPlacement, idPlayer);

                        errors = Tools.Errors.None;
                        break;
                    }

                    // Dans le cas où aucun pion n'a été placée auparavant ou qu'un pion est toujours placé,
                    // on renvoie une erreur Permission                 
                }

            }

            return errors;
        }

        public Tools.Errors Com_EndTurn(ulong idPlayer, int idRoom, Socket? playerSocket, string[] data)
        {
            // Si la demande ne trouve pas de partie ou qu'elle ne provient pas d'un joueur à qui c'est le tour : permission error
            Tools.Errors errors = Tools.Errors.Permission;

            foreach (Thread_serveur_jeu thread_serv_ite in _lst_serveur_jeu)
            {
                if (idRoom != thread_serv_ite.Get_ID()) continue;
                Console.WriteLine("Com_EndTurn : idRoom was found !");
                if (idPlayer == thread_serv_ite.Get_ActualPlayerId())
                {
                    Console.WriteLine("Com_EndTurn : actual player !");
                    
                    // Vérifie qu'il a au moins placé une tuile validée
                    if(thread_serv_ite.Get_posTuileTourActu().IsExisting())
                    {
                        Console.WriteLine("Com_EndTurn : tuile valid !");

                        // Check if sended play is coherent to our stored move (tile and pawn placement) OR valid nonetheless
                        Tools.Errors errorsEquivalence = CheckEquivalenceData(data, thread_serv_ite, idPlayer, playerSocket, idRoom);

                        if (errorsEquivalence != Tools.Errors.None)
                            return errorsEquivalence;

                        // Fin du tour actuel
                        Socket? nextPlayerSocket = thread_serv_ite.EndTurn(idPlayer, true);
                        // Mise à jour du status de la game
                        Tools.GameStatus statusGame = thread_serv_ite.UpdateGameStatus();
                        Console.WriteLine(thread_serv_ite.Get_Status());

                        // Génération du nouveau tableau data+scores
                        string[] allScores = thread_serv_ite.GetAllPlayersScore();
                        string[] dataWithScores = new string[allScores.Length + data.Length];

                        data.CopyTo(dataWithScores, 0);
                        allScores.CopyTo(dataWithScores, data.Length);

                        if (statusGame == Tools.GameStatus.Stopped) // Si la partie est terminée
                        {
                            Console.WriteLine("Com_EndTurn : game stopped !");

                            SendBroadcast(idRoom, Tools.IdMessage.EndTurn, dataWithScores);
                            ulong idPlayerWinner = thread_serv_ite.GetWinner();
                            string[] dataToSend = new string[] { idPlayerWinner.ToString() };
                            SendBroadcast(idRoom, Tools.IdMessage.EndGame, dataToSend);
                            DeleteGame(idRoom);
                        }
                        else // Si la partie n'est pas terminée
                        {
                            Console.WriteLine("Com_EndTurn : game still running !");
                            
                            // Mélange des tuiles pour le prochain tirage
                            thread_serv_ite.ShuffleTilesGame();
                            thread_serv_ite.Set_tuilesEnvoyees(thread_serv_ite.GetThreeLastTiles());

                            Console.WriteLine("Com_EndTurn : before broadcast !");

                            // Envoi de l'information du endturn
                            SendBroadcast(idRoom, Tools.IdMessage.EndTurn, dataWithScores);
                        }

                        return Tools.Errors.None;
                    }
                    else
                    {
                        Console.WriteLine("Com_EndTurn : tuile not valid !");
                    }

                    // S'il n'a pas posé de tuile : erreur Permission

                    
                }
                else
                {
                    Console.WriteLine("Com_EndTurn : not the actual player !");
                }

            }

            return errors;
        }

        /// <summary>
        /// Forces the end of a turn. 
        /// Doesn't care if the play is valid (tile and pawn pos), because it has already been checked if it is stored.
        /// If no play stored, -1 -1 broadcasted and no placement is played.
        /// </summary>
        /// <param name="idPlayer"></param>
        /// <param name="idRoom"></param>
        /// <param name="data"></param>
        public void ForceEndTurn(ulong idPlayer, int idRoom, string[] data)
        {
            foreach (Thread_serveur_jeu thread_serv_ite in _lst_serveur_jeu)
            {
                if (idRoom != thread_serv_ite.Get_ID()) continue;
                Console.WriteLine("Force_EndTurn : room found !");
                if (idPlayer == thread_serv_ite.Get_ActualPlayerId())
                {
                    // Fin du tour actuel
                    Socket? nextPlayerSocket = thread_serv_ite.EndTurn(idPlayer, false);
                    // Mise à jour du status de la game
                    Tools.GameStatus statusGame = thread_serv_ite.UpdateGameStatus();

                    // Génération du nouveau tableau data+scores
                    string[] allScores = thread_serv_ite.GetAllPlayersScore();
                    string[] dataWithScores = new string[allScores.Length + data.Length];

                    data.CopyTo(dataWithScores, 0);
                    allScores.CopyTo(dataWithScores, data.Length);

                    if (statusGame == Tools.GameStatus.Stopped) // Si la partie est terminée
                    {
                        Console.WriteLine("Force_EndTurn : game stopped !");

                        SendBroadcast(idRoom, Tools.IdMessage.TimerPlayer, dataWithScores);
                        
                        ulong idPlayerWinner = thread_serv_ite.GetWinner();
                        string[] dataToSend = new string[] { idPlayerWinner.ToString() };
                        SendBroadcast(idRoom, Tools.IdMessage.EndGame, dataToSend);
                        DeleteGame(idRoom);
                    }
                    else // Si la partie n'est pas terminée
                    {
                        Console.WriteLine("Force_EndTurn : game still running !");

                        // Mélange des tuiles pour le prochain tirage
                        thread_serv_ite.ShuffleTilesGame();
                        thread_serv_ite.Set_tuilesEnvoyees(thread_serv_ite.GetThreeLastTiles());

                        Console.WriteLine("Force_EndTurn : before broadcast !");

                        // Envoi de l'information du endturn
                        SendBroadcast(idRoom, Tools.IdMessage.TimerPlayer, dataWithScores);
                    }
                }
            }             
        }

        public void ForceCloseRoom(int idRoom)
        {
            Console.WriteLine("ForceCloseRoom");
            foreach (Thread_serveur_jeu thread_serv_ite in _lst_serveur_jeu)
            {
                if (thread_serv_ite.Get_ID() != idRoom) continue;
                
                Console.WriteLine("ForceCloseRoom : idRoom was found !");

                foreach (var player in thread_serv_ite.Get_Dico_Joueurs())
                {
                    Console.WriteLine("ForceCloseRoom : remove idPlayer " + player.Value._id_player);
                    thread_serv_ite.RemoveJoueur(player.Value._id_player);
                    SendUnicast(idRoom, Tools.IdMessage.PlayerKick, player.Value._socket_of_player, player.Value._id_player, Array.Empty<string>());
                }
                DeleteGame(idRoom);
            }
        }
        
        /// <summary>
        ///     Checks if the data is equivalent to the last play stored. If not, check the validity of it.
        /// </summary>
        /// <param name="dataReceived"></param>
        /// <param name="threadJeu"></param>
        /// <param name="idPlayer"></param>
        /// <param name="playerSocket"></param>
        /// <param name="idRoom"></param>
        /// <returns> None if equivalent OR valid nonetheless, illegalPlay else </returns>
        public Tools.Errors CheckEquivalenceData(string[] dataReceived, Thread_serveur_jeu threadJeu, ulong idPlayer, Socket? playerSocket, int idRoom)
        {
            Tools.Errors errors = Tools.Errors.None;

            // Parses the data
            ulong idTuile = UInt64.Parse(dataReceived[0]);
            Position posTuile = new Position(Int32.Parse(dataReceived[1]), Int32.Parse(dataReceived[2]), Int32.Parse(dataReceived[3]));
            int idMeeple = Int32.Parse(dataReceived[4]);
            int slotPos = Int32.Parse(dataReceived[5]);

            // Checks if it's the good id tile (the same that has been drawed)
            if(idTuile == threadJeu.Get_idTuileChoisie())
            {
                Position lastTilePosServer = threadJeu.Get_posTuileTourActu();
                // Checks if the pos of the tile is the same that the stored one
                if (posTuile.X == lastTilePosServer.X && posTuile.Y == lastTilePosServer.Y && posTuile.ROT == lastTilePosServer.ROT)
                {
                    string[] lastPawnPosServer = threadJeu.Get_posPionTourActu();


                    // Checks if the pawn is the same that the stored one
                    if(lastPawnPosServer.Length != 0 && idMeeple == Int32.Parse(lastPawnPosServer[3]) && slotPos == Int32.Parse(lastPawnPosServer[4]))
                    {
                        // All is good, same play than the stored one
                        return Tools.Errors.None;
                    }
                    else // ? New ? pawn
                    {
                        // Check if pawn is set or not (-1 -1) then verify placement
                        if(idMeeple != -1 && slotPos != -1)
                            errors = threadJeu.PionPlacement(idPlayer, posTuile, (ulong)idMeeple, slotPos);
                    }
                }
                else
                {
                    // Checks if valid nonetheless
                    errors = threadJeu.TilePlacement(idPlayer, idTuile, posTuile.X, posTuile.Y, posTuile.ROT);

                    if(errors == Tools.Errors.None)
                    {
                        // Check if pawn is set or not (-1 -1) then verify placement
                        if (idMeeple != -1 && slotPos != -1)
                            errors = threadJeu.PionPlacement(idPlayer, posTuile, (ulong)idMeeple, slotPos);
                    }
                }
            }
            else
            {
                errors = Tools.Errors.IllegalPlay;
            }


            if (errors == Tools.Errors.IllegalPlay)
            {
                PlayerCheated(idPlayer, playerSocket, idRoom);
            }


            return errors;
        }

        public void PlayerCheated(ulong idPlayer, Socket? playerSocket, int idRoom)
        {
            Packet packet = new Packet();


            // Recherche de la partie
            foreach (Thread_serveur_jeu threadJeu in _lst_serveur_jeu)
            {
                if(threadJeu.Get_ID() == idRoom)
                {
                    // Indique au serveur la triche
                    Tools.PlayerStatus playerStatus = threadJeu.SetPlayerStatus(idPlayer);


                    if (playerStatus == Tools.PlayerStatus.Kicked) // Deuxième triche -> kick
                    {
                        packet.IdMessage = Tools.IdMessage.PlayerKick;
                        PlayerKick(idRoom, idPlayer, Array.Empty<string>());
                    }
                    else // Première triche -> avertissement
                    {
                        packet.IdMessage = Tools.IdMessage.PlayerCheat;
                        Server.Server.SendToSpecificClient(playerSocket, packet);
                    }
                    break;
                }
            }

            
        }

        public Tools.PlayerStatus PlayerLeave(ulong idPlayer, int idRoom)
        {
            // Recherche de la partie
            foreach (Thread_serveur_jeu threadJeu in _lst_serveur_jeu)
            {
                if (threadJeu.Get_ID() == idRoom)
                {
                    // Retrait du joueur de la partie
                    Tools.PlayerStatus playerStatus = threadJeu.RemoveJoueur(idPlayer);

                    // Vérification du status de la partie (si le dernier joueur quitte -> fin de partie)
                    if (threadJeu.Get_Status() == Tools.GameStatus.Stopped)
                    {
                        DeleteGame(idRoom);
                        return playerStatus;
                    }

                    if (threadJeu.Get_ActualPlayerId() == idPlayer) // Si le joueur quitte durant son tour
                    {
                        // On abandonne les informations du tour actuel
                        Socket? nextPlayerSock = threadJeu.CancelTurn();
                        // On termine le tour de manière forcée
                        ForceEndTurn(idPlayer, idRoom, Array.Empty<string>());
                    }

                    // Si le joueur était le modérateur
                    if(threadJeu.Get_Moderateur() == idPlayer)
                    {
                        threadJeu.SwitchModerateur();
                    }

                    

                    return playerStatus;

                }
            }

            return Tools.PlayerStatus.NotFound;
        }

        public void PlayerKick(int idRoom, ulong idPlayer, string[] data)
        {
            foreach (Thread_serveur_jeu thread_serv_ite in Get_list_server_thread())
            {
                if (thread_serv_ite.Get_Dico_Joueurs().ContainsKey(idPlayer) == false) continue;               
                // Informe tous le monde du kick
                SendBroadcast(idRoom, Tools.IdMessage.PlayerKick, idPlayer);

                //Debug kick
                Console.WriteLine("/!\\ DEBUG - Playerkicked : " + idPlayer);

                // Retrait du joueur de la game
                thread_serv_ite.RemoveJoueur(idPlayer);
                // Cancel tour actuel
                Socket? nextPlayerSock = thread_serv_ite.CancelTurn();

                // Mise à jour du status de la game
                Tools.GameStatus statusGame = thread_serv_ite.UpdateGameStatus();

                // Génération du nouveau tableau data+scores
                string[] allScores = thread_serv_ite.GetAllPlayersScore();
                string[] dataWithScores = new string[allScores.Length + data.Length];

                data.CopyTo(dataWithScores, 0);
                allScores.CopyTo(dataWithScores, data.Length);

                if (statusGame == Tools.GameStatus.Stopped) // Si la partie est terminée
                {
                    Console.WriteLine("Playerkicked : game stopped !");

                    SendBroadcast(idRoom, Tools.IdMessage.EndTurn, dataWithScores);
                    ulong idPlayerWinner = thread_serv_ite.GetWinner();
                    string[] dataToSend = new string[] { idPlayerWinner.ToString() };
                    SendBroadcast(idRoom, Tools.IdMessage.EndGame, dataToSend);
                    DeleteGame(idRoom);
                }
                else // Si la partie n'est pas terminée
                {
                    Console.WriteLine("Playerkicked : game still running !");

                    // Mélange des tuiles pour le prochain tirage
                    thread_serv_ite.ShuffleTilesGame();
                    thread_serv_ite.Set_tuilesEnvoyees(thread_serv_ite.GetThreeLastTiles());

                    Console.WriteLine("Playerkicked : before broadcast !");

                    // Envoi de l'information du endturn
                    SendBroadcast(idRoom, Tools.IdMessage.TimerPlayer, dataWithScores);
                }

                // reset player timer               
                thread_serv_ite._DateTime_player = DateTime.Now;
                thread_serv_ite._timer_player.Start();

                break;

            }
        }

        // ===============================
        // Fonction principale (threadée)
        // ===============================

        public void Lancement_thread_com()
        {

            // Informations du thread

            Console.WriteLine(string.Format("[{0}] Je suis un thread !", _id_thread_com));
            Console.WriteLine(string.Format("[{0}] J'officie sur le port numéro {1} !", _id_thread_com, _numero_localPort));
            Console.WriteLine(string.Format("[{0}] Je gère actuellement {1} parties!", _id_thread_com, _nb_parties_gerees));
            foreach (int id_ite in _id_parties_gerees)
            {
                Console.WriteLine(string.Format("[{0}] Je gère la partie d'ID {1}", _id_thread_com, id_ite));
            }

            //Debug.Log(string.Format("Compteur d'id de strings : {0}", _compteur_id_thread_com));


            
            // Lancement du serveur d'écoute du thread de com
            Server.Server.StartListening(_id_thread_com + 1);


            


            /*
            TextAsset contents = Resources.Load<TextAsset>("network/config");
            Parameters parameters = JsonConvert.DeserializeObject<Parameters>(contents.ToString());
            parameters.ServerPort = Convert.ToInt32(packet.Data[0]);
            _mon_id = packet.IdPlayer;

            ClientAsync.Connection(socket, parameters);
            ClientAsync.connectDone.WaitOne();

            ClientAsync.OnPacketReceived += OnPacketReceived;
            ClientAsync.Receive(socket);
            
            */

        }
    }
}
