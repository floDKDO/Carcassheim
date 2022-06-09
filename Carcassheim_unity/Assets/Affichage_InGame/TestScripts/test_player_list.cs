using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class test_player_list : MonoBehaviour
{
    public PlayerList pl_list;
    public Banner banner;
    bool treated = false;
    float cooldown = 0;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void OnGUI()
    {
        if (!treated)
        {
            treated = true;
            cooldown = 0;
            if (Input.GetKeyUp("p"))
            {
                PlayerRepre pl = new PlayerRepre();
                pl_list.addPlayer(pl);
                if (pl_list.nbPlayer() == 1)
                {
                    banner.setPlayer(pl);
                }
                // Debug.Log("Player nb : " + ((uint)pl_list.nbPlayer()).ToString());
                banner.setPlayerNumber(pl_list.nbPlayer());
            }
            else if (Input.GetKeyUp("n"))
            {
                pl_list.nextPlayer(null);
            }
            else if (Input.GetKeyUp("m"))
            {
                PlayerRepre pl = pl_list.getActPlayer();
                if (pl != null)
                    pl.NbMeeple += 1;
            }
            else if (Input.GetKeyUp("s"))
            {
                PlayerRepre pl = pl_list.getActPlayer();
                if (pl != null)
                    pl.Score += 10;
            }
            else if (Input.GetKeyUp("d"))
            {
                PlayerRepre pl = pl_list.getActPlayer();
                if (pl != null)
                {
                    if (pl_list.nbPlayer() == 1)
                    {
                        banner.setPlayer(null);
                    }
                    pl_list.playerDisconnected(pl);
                    banner.playerDisconnected(pl);
                }
            }
            else
            {
                treated = false;
            }
        }
    }

    void Update()
    {
        cooldown -= Time.deltaTime;
        if (cooldown <= 0)
            treated = false;
    }
}
