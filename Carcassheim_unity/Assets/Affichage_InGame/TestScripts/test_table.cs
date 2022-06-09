using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class test_table : MonoBehaviour
{
    public List<TuileRepre> tuiles;
    public List<MeepleRepre> meeples;
    public List<Transform> tile_colliders;
    public List<Transform> meeple_colliders;

    public GameObject tuile_collider_model;
    public GameObject meeple_collider_model;
    public GameObject tuile_model;
    public GameObject meeple_model;
    public Transform tuile_zone;
    public Transform meeple_zone;
    public Transform balise1;
    public Transform balise2;
    public Indicator indic;
    private int index_t = -1;
    private int index_m = -1;
    private int mode = 1;
    private int card_nb = 110;

    void Start()
    {
        // if (mode == 0)
        // {
        //     meeple_zone.gameObject.SetActive(false);
        //     tuile_zone.gameObject.SetActive(true);
        // }
        // else
        // {
        //     tuile_zone.gameObject.SetActive(false);
        //     meeple_zone.gameObject.SetActive(true);
        // }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.KeypadPlus))
        {
            if (mode == 0)
            {
                GameObject ntuile = Instantiate<GameObject>(tuile_model);
                tuiles.Add(ntuile.GetComponent<TuileRepre>());
                tuiles[tuiles.Count - 1].model.layer = LayerMask.NameToLayer("Table");
                tuiles[tuiles.Count - 1].transform.parent = tuile_zone;

                GameObject ntuile_collide = Instantiate<GameObject>(tuile_collider_model);
                ntuile_collide.GetComponent<ColliderStat>().Index = tile_colliders.Count;
                tile_colliders.Add(ntuile_collide.transform);
                ntuile_collide.transform.parent = tuile_zone;

                Vector3 pas = (balise2.position - balise1.position) / tuiles.Count;
                Vector3 origin = balise1.position + pas / 2;
                for (int i = 0; i < tuiles.Count; i++)
                {
                    tuiles[i].transform.position = pas * i + origin;
                    tile_colliders[i].position = pas * i + origin;
                    tuiles[i].pivotPoint.rotation = Quaternion.AngleAxis(90, balise2.position - balise1.position);
                }
                if (index_t == -1)
                    index_t = 0;
                tuiles[index_t].pivotPoint.rotation = balise1.rotation;
            }
            else
            {
                GameObject ntuile = Instantiate<GameObject>(meeple_model);
                meeples.Add(ntuile.GetComponent<MeepleRepre>());
                meeples[meeples.Count - 1].model.layer = LayerMask.NameToLayer("Table");
                meeples[meeples.Count - 1].transform.parent = meeple_zone;

                GameObject nmeeple_collide = Instantiate<GameObject>(meeple_collider_model);
                nmeeple_collide.GetComponent<ColliderStat>().Index = meeple_colliders.Count;
                meeple_colliders.Add(nmeeple_collide.transform);
                nmeeple_collide.transform.parent = meeple_zone;

                Vector3 pas = (balise2.position - balise1.position) / meeples.Count;
                Vector3 origin = balise1.position + pas / 2;
                for (int i = 0; i < meeples.Count; i++)
                {
                    meeples[i].transform.position = pas * i + origin;
                    meeple_colliders[i].transform.position = pas * i + origin;
                    meeples[i].pivotPoint.rotation = Quaternion.AngleAxis(90, balise2.position - balise1.position);
                }
                if (index_m == -1)
                    index_m = 0;
                meeples[index_m].pivotPoint.rotation = balise1.rotation;
            }
        }
        else if (Input.GetKeyUp("b"))
        {
            // mode = (mode + 1) % 2;
            // if (mode == 0)
            // {
            //     meeple_zone.gameObject.SetActive(false);
            //     tuile_zone.gameObject.SetActive(true);
            // }
            // else
            // {
            //     tuile_zone.gameObject.SetActive(false);
            //     meeple_zone.gameObject.SetActive(true);
            // }
        }
        else if (Input.GetKeyUp("c"))
        {
            indic.Value = card_nb--;
        }
        else if (Input.GetMouseButtonUp(1))
        {
            // RaycastHit hit;
            // Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            // Debug.Log("Collider test");
            // if (Physics.Raycast(ray, out hit))
            // {
            //     int index = hit.transform.gameObject.GetComponent<ColliderStat>().Index;
            //     if (mode == 0)
            //     {
            //         if (tuiles.Count > 0)
            //         {
            //             int nindex = index;
            //             if (index_t >= 0)
            //             {
            //                 tuiles[index_t].pivotPoint.rotation = Quaternion.AngleAxis(90, balise2.position - balise1.position);
            //             }
            //             tuiles[nindex].pivotPoint.rotation = balise1.rotation;
            //             index_t = nindex;
            //         }
            //     }
            //     else
            //     {
            //         if (meeples.Count > 0)
            //         {
            //             int nindex = index;
            //             if (index_m >= 0)
            //             {
            //                 meeples[index_m].pivotPoint.rotation = Quaternion.AngleAxis(90, balise2.position - balise1.position);
            //             }
            //             meeples[nindex].pivotPoint.rotation = balise1.rotation;
            //             index_m = nindex;
            //         }
            //      }
        }
    }
}
