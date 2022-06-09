using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BillboardMode
{
    BillboardFront,
    BillboardFacing
};

public class billboard_stable : MonoBehaviour
{
    public int orientation = -1;
    public BillboardMode billboard_mode = BillboardMode.BillboardFront;
    // Update is called once per frame
    void Start()
    {
        Face();
    }

    void Face()
    {
        if (billboard_mode == BillboardMode.BillboardFront)
            transform.forward = orientation * Camera.main.transform.forward;
        else
            transform.forward = orientation * (Camera.main.transform.position - transform.position).normalized;
    }
}
