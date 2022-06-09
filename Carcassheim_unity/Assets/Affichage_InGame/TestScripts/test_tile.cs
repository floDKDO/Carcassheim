using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class test_tile : MonoBehaviour
{
    public int id_read = 0;

    public TuileRepre act_tuile = null;
    public PlayerRepre act_player = null;
    private bool hidden = true;
    int PlateauLayer;

    // Start is called before the first frame update
    void Start()
    {
        act_player = new PlayerRepre();
        PlateauLayer = LayerMask.NameToLayer("Board");
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.T))
        {
            if (act_tuile != null)
                Destroy(act_tuile.gameObject);
            act_tuile = Resources.Load<TuileRepre>("tile" + id_read.ToString());
            act_tuile = Instantiate<TuileRepre>(act_tuile, transform);

            act_tuile.possibilitiesPosition.Clear();
            int rotation = Random.Range(0, 4);
            int x = Random.Range(-100, 101), y = Random.Range(-100, 101);
            for (int i = 1; i <= 3; i++)
            {
                if (Random.Range(0f, 1f) < 0.4)
                {
                    if (Random.Range(0f, 1f) < 0.2)
                        x += 1;
                    PositionRepre pos = new PositionRepre(x, y, (rotation + i) % 4);
                    act_tuile.possibilitiesPosition.Add(pos);
                    Debug.Log("Position possible : " + pos.ToString());
                }
            }
            act_tuile.Pos = new PositionRepre(x, y, rotation);
            act_tuile.possibilitiesPosition.Add(act_tuile.Pos);
            Debug.Log("Position " + act_tuile.Pos);
        }
        else if (Input.GetKeyUp(KeyCode.A) && act_tuile != null)
        {
            if (hidden)
            {
                act_tuile.showPossibilities(act_player);
            }
            else
            {
                act_tuile.hidePossibilities();
            }
            hidden = !hidden;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            if (act_tuile == null)
                return;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, (1 << PlateauLayer)))
            {
                SlotIndic slot;
                TuileRepre tile;
                switch (hit.transform.tag)
                {
                    case "SlotCollider":
                        slot = hit.transform.parent.GetComponent<SlotIndic>();
                        if (hit.transform.parent.parent != act_tuile.pivotPoint)
                            Debug.Log("Wrong parent " + hit.transform.parent.parent.name + " instead of " + act_tuile.pivotPoint.name);
                        else
                        {
                            if (!slot.front.gameObject.activeSelf)
                                slot.highlightFace(act_player);
                            else
                                slot.unlightFace();
                        }
                        break;
                    case "TileBodyCollider":
                        if (act_tuile != null)
                        {
                            if (hit.transform.parent != act_tuile.pivotPoint)
                                Debug.Log("Wrong parent " + hit.transform.parent.parent.name + " instead of " + act_tuile.pivotPoint.name);
                            else
                            {
                                tile = hit.transform.parent.parent.GetComponent<TuileRepre>();
                                if (tile.nextRotation())
                                {
                                    Debug.Log("Rotated  to " + tile.Pos);
                                }
                                else
                                {
                                    Debug.Log("No rotation done");
                                }
                            }
                        }
                        break;
                    default:
                        Debug.Log("Hit " + hit.transform.tag + " collider");
                        break;
                }
            }
        }
    }
}
