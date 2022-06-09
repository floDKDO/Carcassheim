using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerList : MonoBehaviour
{

    [SerializeField] private Transform waiting_players;
    [SerializeField] private GameObject player_repre_model;
    [SerializeField] private DisplaySystem master;

    private LinkedList<PlayerRepre> _players;
    private LinkedList<GameObject> _players_repre;
    private bool _ignore_first = false;

    // Start is called before the first frame update
    void Awake()
    {
        _players = new LinkedList<PlayerRepre>();
        _players_repre = new LinkedList<GameObject>();
    }

    void OnEnable()
    {
        master.OnPlayerDisconnected += playerDisconnected;
    }

    void OnDisable()
    {
        master.OnPlayerDisconnected -= playerDisconnected;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public bool addPlayer(PlayerRepre player)
    {
        foreach (PlayerRepre pl in _players)
        {
            if (pl.Id == player.Id)
            {
                return false;
            }
        }

        GameObject repre = Instantiate<GameObject>(player_repre_model);
        PlayerIcon ic = repre.GetComponent<PlayerIcon>();
        ic.setPlayer(player);
        if (_players.Count == 0)
        {
            _players.AddLast(player);
            _players_repre.AddLast(repre);
            repre.transform.SetParent(transform, false);
            repre.transform.SetAsFirstSibling();
        }
        else
        {
            _players.AddBefore(_players.Last, player);
            _players_repre.AddBefore(_players_repre.Last, repre);
            repre.transform.SetParent(waiting_players, false);
        }
        return true;
    }

    public void playerDisconnected(PlayerRepre player)
    {
        LinkedListNode<PlayerRepre> pl = _players.First;
        LinkedListNode<GameObject> pl_repre = _players_repre.First;
        while (pl != null)
        {
            if (pl.Value.Id == player.Id)
            {
                if (pl == _players.Last && pl != _players.First)
                {
                    _ignore_first = true;
                }
                _players.Remove(pl);
                _players_repre.Remove(pl_repre);
                Destroy(pl_repre.Value);
            }
            pl = pl.Next;
            pl_repre = pl_repre.Next;
        };
        if (_players.Count == 0)
        {
            _ignore_first = false;
        }
    }

    public PlayerRepre getActPlayer()
    {
        LinkedListNode<PlayerRepre> pl_node = _players.Last;
        if (pl_node == null)
            return null;
        return pl_node.Value;
    }

    public PlayerRepre getNextPlayer()
    {
        LinkedListNode<PlayerRepre> pl_node = _players.First;
        if (pl_node == null)
            return null;
        return pl_node.Value;
    }

    public int nbPlayer()
    {
        return _players.Count;
    }

    public bool nextPlayer(PlayerRepre player)
    {
        if (_players.Count < 2)
        {
            Debug.LogWarning("No player to cycle trough");
            return false;
        }

        LinkedListNode<PlayerRepre> pl_node = _players.First;
        LinkedListNode<GameObject> repre_node = _players_repre.First;
        _players.RemoveFirst();
        _players_repre.RemoveFirst();

        PlayerRepre pl = pl_node.Value;
        GameObject repre = repre_node.Value;
        /*if (pl.Id != player.Id)
        {
            master.askPlayerOrder(m_players);
            //TODO treat correctly the case where the order change
        }*/

        repre.transform.SetParent(transform, false);
        repre.transform.SetAsFirstSibling();
        if (!_ignore_first)
        {
            LinkedListNode<GameObject> prev_repre_node = _players_repre.Last;
            GameObject prev_repre = prev_repre_node.Value;
            prev_repre.transform.SetParent(waiting_players, false);
        }
        else
        {
            _ignore_first = false;
        }
        _players.AddLast(pl_node);
        _players_repre.AddLast(repre_node);
        return true;
    }
}
