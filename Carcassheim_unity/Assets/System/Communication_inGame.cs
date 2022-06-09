using System;
using System.Net.Sockets;
using UnityEngine;
using ClassLibrary;
using System.Collections.Generic;
using Assert.system;
using Assets.System;
using System.Threading;
using System.Threading.Tasks;

namespace Assets.system
{
    public class Communication_inGame : CarcasheimBack
    {
        private ulong _id_partie;

        private ulong _mon_id;

        private int id_tile_init = 20;
        bool id_tile_init_received = true;

        private int _mode = 0; // 0 -> Classique | 1 -> Time-attack | 2 -> Score
        private int _nb_tuiles = -1;
        private int _score_max = -1;
        private int _timer_max_joueur = -1; // En secondes
        bool win_cond_received = false;

        private int _nb_joueur_max;
        private int _timer; // En secondes
        bool timer_tour_received = false;
        private int _meeples; // Nombre de meeples par joueur
        private int meeple_type = 1;

        private Dictionary<ulong, Tuile> dico_tuile;
        private Plateau lePlateau;

        private Player[] playerList;
        bool players_received = false;
        private ulong nextPlayer;

        private Position[] allposition;
        private Semaphore s_allposition;

        private Semaphore s_InGame;
        private bool testGameBegin = true;

        private bool turn_received = false;
        private bool first_turn_is_launch = false;

        private TurnPlayParam play_of_this_turn;

        private int nb_tile_for_turn = 0;

        private bool player_received = false;
        private bool playerlist_received = false;
        private bool gameStatus = true;

        private Semaphore s_WaitInit;
        private Semaphore s_WaitPlayerList;
        List<PlayerScoreParam> gains = new List<PlayerScoreParam>();
        List<Zone> zones = new List<Zone>();

        List<Tuple<int, int, ulong>> meeple_positions = new List<Tuple<int, int, ulong>>();

        // DISPLAY SYSTEM
        [SerializeField] DisplaySystem system_display = null;
        //====================================================================================================
        // COMM AFFICHAGE
        // => Display; indique que la fonction est utilis� pour communiquer avec l'affichage
        // => Serveur; indique qu'il faut communiqyer avec le serveur
        // UNE FOIS QUE LES DONNES DE DEBUT DE PARTIE SONT DISPONNIBLE :
        //      APPELER gameBegin()
        // UNE FOIS QUE getTile EST PRET 
        //      APPELER system_display.setNextState(
        //  SI CONSTRUIRE RIVIERE :  turnStart
        //  SINON SI SCORE CHANGE : scoreChange
        //  SINON SI FIN DE PARTIE : endOfGame
        //      )
        // UNE FOIS QUE LE Score A ETE AFFICHE
        //      APPELER system_display.setNextState(
        //  SINON SI NOUVEAU TOUR : turnStart
        //  SINON SI FIN DE PARTIE : endOfGame
        //      )
        // !!! le cas de l'explorateur n'est pas encore fait (apr�s getTile)

        //=======================================================
        // ATTRIBUTES
        private List<TileInitParam> tiles_drawed;

        //=======================================================
        // END TURN
        override public void sendTile(TurnPlayParam param)
        {
            Packet packet = new Packet();
            packet.IdMessage = Tools.IdMessage.EndTurn;
            packet.IdRoom = (int)_id_partie;
            packet.IdPlayer = _mon_id;

            packet.Data = new string[]
            {
                param.id_tile.ToString(),
                param.tile_pos.X.ToString(),
                param.tile_pos.Y.ToString(),
                param.tile_pos.Rotation.ToString(),
                param.id_meeple.ToString(),
                param.slot_pos.ToString()
            };

            Communication.Instance.SendAsync(packet);
        }

        override public void getTile(out TurnPlayParam param)
        {
            // PARTAGER LE COUP valid� PAR LE JOUEUR ELU => Display
            param = play_of_this_turn;
        }

        //=======================================================
        // START TURN

        override public void askMeeplePosition(MeeplePosParam mp, List<int> slot_pos)
        {
            if (mp.pos_tile == null)
            {
                Debug.LogError("Asking meeple for null position");
                return;
            }
            lePlateau.PoserTuileFantome((ulong)mp.id_tile, mp.pos_tile.X, mp.pos_tile.Y, mp.pos_tile.Rotation);
            slot_pos.AddRange(lePlateau.EmplacementPionPossible(mp.pos_tile.X, mp.pos_tile.Y, (ulong)mp.id_meeple));
        }
        override public int getNextPlayer()
        {
            //PARTAGER L'ID DU JOUEUR ELU => Display
            return (int)nextPlayer;
        }

        override public int askTilesInit(List<TileInitParam> tiles)
        {
            // PARTAGER LA LISTE DES TUILES TIRES => Display
            // Id tuile,
            // FLAG => true : garder la tuile; false: jeter la tuile
            // RETURN nombre de tuile dans la main au final

            for (int i = 0; i < nb_tile_for_turn; i++)
                tiles.Add(tiles_drawed[i]);
            tiles_drawed.Clear();
            nb_tile_for_turn = 0;

            return 1;
        }

        override public void askMeeplesInit(List<MeepleInitParam> meeples)
        {
            // PARTAGER LA LISTE DES ID DES MEEPLES POSABLES ET LE NOMBRE DISP�NIBLE => Display
            // Id meeple, nb de meeple d'id Id disponible
            int taille = playerList.Length;
            for (int j = 0; j < taille; j++)
            {
                if (playerList[j].id == nextPlayer && playerList[j].nbMeeples != 0)
                    meeples.Add(new MeepleInitParam((int)Tools.MeepleType.Default, (int)playerList[j].nbMeeples));
            }
        }

        override public void getTilePossibilities(int tile_id, List<PositionRepre> positions)
        {
            // PARTAGER LA LISTE DES POSITIONS OU LA TUILE D'ID tile_id PEUT ETRE POSE => Display
            // tile_id est un argument 
            // ajouter avec new Position,(X, Y, Rotation) avec rotation = (0: Nord, 1: Est, 2: Sud, 3: Ouest)
            // mettre une position par rotation
            s_allposition.WaitOne();
            int i, taille = allposition.Length;
            for (i = 0; i < taille; i++)
            {
                positions.Add(new PositionRepre(
                    allposition[i].X,
                    allposition[i].Y,
                    allposition[i].ROT
                    ));
            }
            s_allposition.Release();
        }

        public override void askMeepleRetired(List<Tuple<int, int, ulong>> positions)
        {
            positions.AddRange(meeple_positions);
            meeple_positions.Clear();
        }

        //=======================================================
        // SCORE
        override public void askScores(List<PlayerScoreParam> players_scores, List<Zone> zones)
        {
            // PARTAGER LES NOUVEAUX SCORES POUR CHAQUE JOUEUR => Display
            // Id du joueur dont le score change, Nouveau score, Zone provoquant le changement de score
            // Zone: array/list de (id tuile, position de la tuile sur le plateau, id du slot)
            int taille = playerList.Length;
            for (int j = 0; j < taille; j++)
            {
                players_scores.Add(new PlayerScoreParam(
                    (ulong)playerList[j].id,
                    (int)playerList[j].score));
            }
        }

        //=======================================================
        // GAME BEGIN
        override public void askPlayersInit(List<PlayerInitParam> players)
        {
            //TODO PARTAGER ID JOUEUR, NOM JOUEUR, NB DE MEEPLE => Display
            // Debug.Log("JOUEUR DEMANDE");
            int taille = playerList.Length;
            for (int j = 0; j < taille; j++)
            {
                players.Add(new PlayerInitParam(
                    (int)playerList[j].id,
                    (int)playerList[j].nbMeeples,
                    playerList[j].name
                    ));
            }
        }
        override public int askIdTileInitial()
        {
            // PARTAGER ID DE LA TUILE INITIAL EN POSITION (0, 0) => Display
            return id_tile_init;
        }

        override public void askTimerTour(out int min, out int sec)
        {
            // PARTAGER LE TEMPS DISPONIBLE PAR TOUR => Display
            min = _timer / 60;
            sec = _timer % 60;
        }

        override public void askWinCondition(ref WinCondition win_cond, List<int> parameters)
        {
            // PARTAGER CONDITION DE VICTOIRE => Display
            // 0 TUILE => nb de tuile
            // 1 TEMPS => nb de min, puis nb de sec
            // 2 SCORE => score à atteindre

            switch (_mode)
            {
                case 1:
                    win_cond = WinCondition.WinByTime;
                    parameters.Add(_timer_max_joueur / 60);
                    parameters.Add(_timer_max_joueur % 60);
                    break;
                case 2:
                    win_cond = WinCondition.WinByPoint;
                    parameters.Add(_score_max);
                    break;
                default:
                    win_cond = WinCondition.WinByTile;
                    parameters.Add(_nb_tuiles);
                    break;
            }
        }

        override public int getMyPlayer()
        {
            // PARTAGER ID DU JOUEUR SUR CE CLIENT => Display
            return (int)_mon_id;
        }

        override public void askPlayerOrder(List<int> player_ids)
        {
            // PARTAGER LES IDS DES JOUEURS DANS L'ORDRE DU TOUR => Display
            int taille = playerList.Length;
            for (int j = 0; j < taille; j++)
            {
                player_ids.Add((int)playerList[j].id);
            }
        }

        //=======================================================
        // END GAME

        override public void askFinalScore(List<PlayerScoreParam> playerScores, List<Zone> zones)
        {
            // Pareil que askScore
            int taille = playerList.Length;
            for (int j = 0; j < taille; j++)
            {
                playerScores.Add(new PlayerScoreParam(
                    (ulong)playerList[j].id,
                    (int)playerList[j].score));
            }
        }

        //=======================================================
        // ACTION IN GAME
        override public void sendAction(DisplaySystemAction action)
        {
            // ENVOYER AU!!SERVEUR L'ACTION: PARTAGE DIRECT => Serveur
            switch (action.action_type)
            {
                case DisplaySystemActionTypes.tileSetCoord:
                    DisplaySystemActionTileSetCoord action_tsc = (DisplaySystemActionTileSetCoord)action;
                    if (action_tsc.new_pos != null)
                        SendPosition(action_tsc.tile_id,
                            action_tsc.new_pos.X, action_tsc.new_pos.Y, action_tsc.new_pos.Rotation
                            , Tools.IdMessage.TuilePlacement);
                    else
                        SendPosition(action_tsc.tile_id,
                            0, 0, 0
                            , Tools.IdMessage.TuilePlacement);
                    break;

                case DisplaySystemActionTypes.tileSelection:
                    DisplaySystemActionTileSelection action_ts = (DisplaySystemActionTileSelection)action;
                    break;

                case DisplaySystemActionTypes.meepleSetCoord:
                    DisplaySystemActionMeepleSetCoord action_msc = (DisplaySystemActionMeepleSetCoord)action;
                    SendMeepple(action_msc.tile_id, action_msc.tile_pos.X, action_msc.tile_pos.Y, action_msc.tile_pos.Rotation, action_msc.meeple_id, action_msc.slot_pos);
                    break;

                case DisplaySystemActionTypes.meepleSelection:
                    DisplaySystemActionMeepleSelection action_ms = (DisplaySystemActionMeepleSelection)action;
                    break;

                case DisplaySystemActionTypes.StateSelection:
                    DisplaySystemActionStateSelection action_ss = (DisplaySystemActionStateSelection)action;
                    break;
            }

            return;
        }

        // Start is called before the first frame update
        void Start()
        {
            s_WaitInit = new Semaphore(1, 1);
            s_WaitInit.WaitOne();
            s_WaitPlayerList = new Semaphore(0, 1);

            dico_tuile = LireXML2.Read("config_back.xml");
            lePlateau = new Plateau(dico_tuile);

            Communication.Instance.StartListening(OnPacketReceived);
            s_InGame = new Semaphore(1, 1);
            s_allposition = new Semaphore(1, 1);

            tiles_drawed = new List<TileInitParam>();

            id_tile_init = RoomInfo.Instance.idTileInit;
            lePlateau.Poser1ereTuile((ulong)id_tile_init);

            s_InGame.WaitOne();
            id_tile_init_received = true;
            s_InGame.Release();

            _id_partie = (ulong)Communication.Instance.IdRoom;
            _mon_id = Communication.Instance.IdClient;

            _mode = (int)RoomInfo.Instance.mode; // 0 -> Classique | 1 -> Time-attack | 2 -> Score

            _nb_tuiles = RoomInfo.Instance.nbTuile;
            _score_max = RoomInfo.Instance.scoreMax;
            _timer_max_joueur = (int)RoomInfo.Instance.timerJoueur; // En secondes

            s_InGame.WaitOne();
            win_cond_received = true;
            s_InGame.Release();

            _nb_joueur_max = RoomInfo.Instance.nbJoueurMax;

            if (_mode == 1)
                _timer = (int)RoomInfo.Instance.timerPartie; // En secondes
            else
                _timer = (int)RoomInfo.Instance.timerJoueur;

            s_InGame.WaitOne();
            timer_tour_received = true;
            s_InGame.Release();

            _meeples = RoomInfo.Instance.meeples; // Nombre de meeples par joueur

            Action playerlist = () =>
            {
                Packet packet = new Packet();
                packet.IdPlayer = Communication.Instance.IdClient;
                packet.IdRoom = Communication.Instance.IdRoom;
                packet.IdMessage = Tools.IdMessage.PlayerList;
                packet.Data = Array.Empty<string>();

                Communication.Instance.SendAsync(packet);
            };
            Task.Run(playerlist);

            /* Demmande Current et TuileDraw */
            OnEndTurnReceive(null);

            s_WaitInit.Release();
        }

        public void Disconnection(Socket socket)
        {
            socket.Shutdown(SocketShutdown.Both);
            socket.Close();

            Communication.Instance.IsInRoom = 0;
        }

        public void OnPacketReceived(object sender, Packet packet)
        {
            /* Permet d'attendre la fin de l'initialisation de la class */
            s_WaitInit.WaitOne();
            s_WaitInit.Release();

            if (packet.IdMessage == Tools.IdMessage.TuileDraw)
            {
                if ((packet.Error == Tools.Errors.Unknown) && !gameStatus)
                {
                    Debug.LogError("[ERREUR] : TuileDraw : " + packet.Error);
                    return;
                }
                OnTuileReceived(packet);
            }
            else if (packet.IdMessage == Tools.IdMessage.PlayerCurrent)
            {
                if ((packet.Error == Tools.Errors.Unknown) && !gameStatus)
                {
                    Debug.LogError("[ERREUR] : PlayerCurrent : " + packet.Error);
                    return;
                }
                OnPlayerCurrentReceive(packet);
            }
            else if (packet.IdMessage == Tools.IdMessage.PlayerList)
            {
                OnPlayerListReceive(packet);
            }
            else if (packet.IdMessage == Tools.IdMessage.TuilePlacement)
            {
                if (packet.Error == Tools.Errors.None)
                {
                    DisplaySystemAction dsa = new DisplaySystemActionTileSetCoord(int.Parse(packet.Data[0]),
                    new PositionRepre(int.Parse(packet.Data[1]), int.Parse(packet.Data[2]), int.Parse(packet.Data[3])));
                    system_display.execDirtyAction(dsa);
                }
            }
            else if (packet.IdMessage == Tools.IdMessage.EndTurn)
            {
                gameStatus = false;
                OnEndTurnReceive(packet);
            }
            else if (packet.IdMessage == Tools.IdMessage.TuileVerification)
            {
                if (packet.Error == Tools.Errors.None)
                {
                    if (first_turn_is_launch && !turn_received)
                    {
                        turn_received = true;
                        checkTurnStart();
                    }
                    else if (!turn_received)
                    {
                        turn_received = true;
                        checkGameBegin();
                    }
                }
            }
            else if (packet.IdMessage == Tools.IdMessage.EndGame)
            {
                system_display.setNextState(DisplaySystemState.endOfGame);
            }
            else if (packet.IdMessage == Tools.IdMessage.TimerPlayer)
            {
                OnEndTurnReceive(packet, DisplaySystemState.timeOutIdleState);
            }
            else if (packet.IdMessage == Tools.IdMessage.PionPlacement)
            {
                if (packet.Error == Tools.Errors.None)
                {
                    DisplaySystemAction dsa = new DisplaySystemActionMeepleSetCoord(int.Parse(packet.Data[0]),
                    new PositionRepre(int.Parse(packet.Data[1]), int.Parse(packet.Data[2]), int.Parse(packet.Data[3])),
                    int.Parse(packet.Data[4]), int.Parse(packet.Data[5]));
                    system_display.execDirtyAction(dsa);
                }
            }
        }

        private bool OnTuileReceived(Packet packet)
        {
            int id_tuile;
            Tuile tuile;
            int i;
            bool tuile_ok = false;
            TileInitParam tileParam;
            Position[] positions;

            int taille = packet.Data.Length;

            for (i = 0; i < taille; i++)
            {
                id_tuile = Convert.ToInt32(packet.Data[i]);
                positions = lePlateau.PositionsPlacementPossible((ulong)id_tuile);

                tileParam = new TileInitParam
                {
                    tile_flags = false,
                    id_tile = id_tuile
                };

                if (positions != null)
                {
                    if (positions.Length > 0)
                    {
                        SendPosition(id_tuile, positions[0].X, positions[0].Y, positions[0].ROT, Tools.IdMessage.TuileVerification);
                        tileParam.tile_flags = true;

                        s_allposition.WaitOne();
                        allposition = positions;
                        s_allposition.Release();

                        tuile_ok = true;
                    }
                }

                nb_tile_for_turn += 1;
                tiles_drawed.Add(tileParam);
                if (tuile_ok)
                    return tuile_ok;
            }

            packet.IdMessage = Tools.IdMessage.TuileVerification;
            packet.Error = Tools.Errors.Data;
            Communication.Instance.SendAsync(packet);
            return tuile_ok;
        }

        public void OnPlayerListReceive(Packet packet)
        {
            int i, compteur = 0, taille_data = packet.Data.Length;
            playerList = new Player[taille_data / 2];

            for (i = 0; i < taille_data; i += 2)
            {
                playerList[compteur] = new Player(ulong.Parse(packet.Data[i]), packet.Data[i + 1], (uint)RoomInfo.Instance.meeples, 0);
                compteur++;
            }

            s_InGame.WaitOne();
            players_received = true;
            playerlist_received = true;
            s_InGame.Release();
            s_WaitPlayerList.Release();
        }

        public void OnPlayerCurrentReceive(Packet packet)
        {
            bool isOK = false;
            s_InGame.WaitOne();
            if (playerlist_received)
                isOK = true;
            s_InGame.Release();

            if (!isOK)
                s_WaitPlayerList.WaitOne();

            nextPlayer = packet.IdPlayer;
            player_received = true;

            int taille = playerList.Length;
            for (int j = 0; j < taille; j++)
                if (playerList[j].id == nextPlayer)
                {
                    playerList[j].nbMeeples = uint.Parse(packet.Data[0]);
                    break;
                }
        }

        private void OnEndTurnReceive(Packet packet, DisplaySystemState next_state = DisplaySystemState.idleState)
        {
            Action playercurrent = () =>
            {
                Packet packet = new Packet();
                packet.IdPlayer = Communication.Instance.IdClient;
                packet.IdRoom = Communication.Instance.IdRoom;
                packet.IdMessage = Tools.IdMessage.PlayerCurrent;
                packet.Data = Array.Empty<string>();

                Communication.Instance.SendAsync(packet);
            };
            Task.Run(playercurrent);

            Action tuiledraw = () =>
            {
                Packet packet = new Packet();
                packet.IdPlayer = Communication.Instance.IdClient;
                packet.IdRoom = Communication.Instance.IdRoom;
                packet.IdMessage = Tools.IdMessage.TuileDraw;
                packet.Data = Array.Empty<string>();

                Communication.Instance.SendAsync(packet);
            };
            Task.Run(tuiledraw);

            if (packet != null)
            {
                if (packet.Data.Length < 6)
                {
                    Debug.LogError("[ERREUR] : OnEndTurnReceive packet size < 6 : " + packet.Data.Length);
                    return;
                }

                int id_tile = int.Parse(packet.Data[0]);
                int x_tile = int.Parse(packet.Data[1]);
                int y_tile = int.Parse(packet.Data[2]);
                int r_tile = int.Parse(packet.Data[3]);
                int id_meeple = int.Parse(packet.Data[4]);
                int pos_meeple = int.Parse(packet.Data[5]);
                play_of_this_turn = new TurnPlayParam(id_tile, new PositionRepre(x_tile, y_tile, r_tile), id_meeple, pos_meeple);
                if (id_tile != -1)
                    lePlateau.PoserTuileFantome((ulong)id_tile, new Position(x_tile, y_tile, r_tile));
                if (id_meeple != -1 && id_meeple != -1)
                    lePlateau.PoserPion(nextPlayer, x_tile, y_tile, (ulong)pos_meeple);

                if (id_tile != -1)
                {
                    lePlateau.ValiderTour();

                    if (lePlateau.VerifZoneFermeeTuile(x_tile, y_tile, gains, zones))
                    {
                        lePlateau.RemoveAllPawnInTile(x_tile, y_tile, meeple_positions);
                    }

                    gains.Clear();
                    zones.Clear();
                }

                int taille = playerList.Length;
                for (int j = 0; j < taille; j++)
                {
                    playerList[j].score = uint.Parse(packet.Data[j + 6]);
                }
                system_display.setNextState(next_state);
                system_display.setNextState(DisplaySystemState.scoreChange);
            }

        }

        public void SendPosition(int id_tuile, int X, int Y, int ROT, Tools.IdMessage idMessage)
        {
            Packet packet = new Packet();
            packet.IdMessage = idMessage;
            packet.IdRoom = (int)_id_partie;
            packet.IdPlayer = _mon_id;

            packet.Data = new string[]
            {
                id_tuile.ToString(),
                X.ToString(),
                Y.ToString(),
                ROT.ToString()
            };

            Communication.Instance.SendAsync(packet);
        }

        public void SendMeepple(int id_tuile, int X, int Y, int ROT, int id_meeple, int slot_pos)
        {
            Packet packet = new Packet();
            packet.IdMessage = Tools.IdMessage.PionPlacement;
            packet.IdRoom = (int)_id_partie;
            packet.IdPlayer = _mon_id;

            packet.Data = new string[]
            {
                id_tuile.ToString(),
                X.ToString(),
                Y.ToString(),
                ROT.ToString(),
                id_meeple.ToString(),
                slot_pos.ToString()
            };

            Communication.Instance.SendAsync(packet);
        }

        public void checkTurnStart()
        {
            if (turn_received && player_received)
            {
                player_received = false;
                turn_received = false;
                system_display.setNextState(DisplaySystemState.turnStart);
            }
        }

        public void checkGameBegin()
        {
            s_InGame.WaitOne();
            if (players_received && win_cond_received && id_tile_init_received && timer_tour_received && testGameBegin && turn_received && player_received)
            {
                s_InGame.Release();

                testGameBegin = false;
                players_received = false;
                player_received = false;
                turn_received = false;
                system_display.setNextState(DisplaySystemState.gameStart);
                first_turn_is_launch = true;

                return;
            }
            s_InGame.Release();
        }
    }
}