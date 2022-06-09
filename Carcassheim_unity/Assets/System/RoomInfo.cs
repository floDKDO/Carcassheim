using Assert.system;
using ClassLibrary;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace Assets.System
{

    public class RoomInfo : MonoBehaviour
    {
        private static RoomInfo _instance;
        public static RoomInfo Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = GameObject.FindObjectOfType<RoomInfo>();

                    if (_instance == null)
                    {
                        GameObject container = new GameObject("RoomInfo");
                        _instance = container.AddComponent<RoomInfo>();
                    }
                }

                return _instance;
            }
        }

        /* Les Informations de la Room */

        private int _idPartie;
        private ulong _idModerateur;
        private int _nbJoueur;
        private int _nbJoueurMax;
        private int _nbTuile;
        private int _idTileInitial;
        private int _meeples;

        private int _scoreMax;
        private bool _isPrivate;
        private Tools.Mode _mode;
        private int _timerJoueur;
        private int _timerPartie;

        bool _riverOn;
        bool _abbayeOn;

        public int idPartie { get; set; }

        public ulong idModerateur { get => _idModerateur; set { _idModerateur = value; SendModification(); } }
        public int nbJoueur { get => _nbJoueur; set { _nbJoueur = value; SendModification(); } }
        public int nbJoueurMax { get => _nbJoueurMax; set { _nbJoueurMax = value; SendModification(); } }
        public int nbTuile { get => _nbTuile; set { _nbTuile = value; SendModification(); } }
        public int idTileInit { get => _idTileInitial; set { _idTileInitial = value; SendModification(); } }
        public int meeples { get => _meeples; set { _meeples = value; SendModification(); } }
        public int scoreMax { get => _scoreMax; set { _scoreMax = value; SendModification(); } }
        public bool isPrivate { get => _isPrivate; set { _isPrivate = value; SendModification(); } }
        public Tools.Mode mode { get => _mode; set { _mode = value; SendModification(); } }
        public int timerJoueur { get => _timerJoueur; set { _timerJoueur = value; SendModification(); } }
        public int timerPartie { get => _timerPartie; set { _timerPartie = value; SendModification(); } }
        public bool riverOn { get => _riverOn; set { _riverOn = value; SendModification(); } }
        public bool abbayeOn { get => _abbayeOn; set { _abbayeOn = value; SendModification(); } }

        public RoomParameterRepre repre_parameter;

        private Dictionary<ulong, Player> Players;

        private Semaphore s_RoomInfo;

        private void Start()
        {
            _nbJoueur = 0;

            Players = new Dictionary<ulong, Player>();
            s_RoomInfo = new Semaphore(1, 1);
        }

        void Awake()
        {
            if (_instance != null)
            {
                Destroy(gameObject);
            }
            else
            {
                DontDestroyOnLoad(gameObject);
                _instance = this;
            }
        }

        public Player[] GetPlayers()
        {
            int taille = Players.Count;
            Player[] result = new Player[taille];
            Players.Values.CopyTo(result, 0);
            return result;
        }

        public void AddPlayer(ulong idPlayer, Player player)
        {
            if (!Players.ContainsKey(idPlayer))
            {
                Players.Add(idPlayer, player);
                nbJoueur++;
            }
        }
        public void RemovePlayer(ulong idPlayer)
        {
            if (Players.Remove(idPlayer))
                nbJoueur--;
        }

        public void setDefault(bool room_is_public, int room_player_max)
        {
            _isPrivate = !room_is_public;
            _nbJoueurMax = room_player_max;
            SendModification();
        }

        public void SetValues(string[] values)
        {
            try
            {
                s_RoomInfo.WaitOne();

                _idModerateur = ulong.Parse(values[0]);
                _nbJoueur = int.Parse(values[1]);
                _nbJoueurMax = int.Parse(values[2]);
                _isPrivate = bool.Parse(values[3]);
                _mode = (Tools.Mode)int.Parse(values[4]);
                _nbTuile = int.Parse(values[5]);
                _meeples = int.Parse(values[6]);
                _timerPartie = int.Parse(values[7]);
                _timerJoueur = int.Parse(values[8]);
                _scoreMax = int.Parse(values[9]);
                int flags_extension = int.Parse(values[10]);
                _riverOn = (flags_extension & (int)Tools.Extensions.Riviere) > 0;
                _abbayeOn = (flags_extension & (int)Tools.Extensions.Abbaye) > 0;

                s_RoomInfo.Release();

                if (repre_parameter != null && !repre_parameter.IsInititialized)
                    repre_parameter.to_initialize = true;

                repre_parameter?.addParameters(isPrivate, mode, timerJoueur, timerPartie, scoreMax, nbTuile, riverOn, abbayeOn);
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
            }
        }

        private void SendModification()
        {
            if (Communication.Instance.IdClient != idModerateur)
                return;

            Packet packet = new Packet();
            packet.IdPlayer = Communication.Instance.IdClient;
            packet.IdMessage = Tools.IdMessage.RoomSettingsSet;

            s_RoomInfo.WaitOne();

            packet.IdRoom = Communication.Instance.IdRoom;
            int flags_extension = (abbayeOn ? (int)Tools.Extensions.Abbaye : 0)
                                + (riverOn ? (int)Tools.Extensions.Riviere : 0);
            packet.Data = new string[]
            {
                nbJoueurMax.ToString(),
                isPrivate.ToString(),
                ((int) mode).ToString(),
                nbTuile.ToString(),
                meeples.ToString(),
                timerPartie.ToString(),
                timerJoueur.ToString(),
                scoreMax.ToString(),
                flags_extension.ToString()
            };

            s_RoomInfo.Release();

            Communication.Instance.SendAsync(packet);
        }

    }
}



