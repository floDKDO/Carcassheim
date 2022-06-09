using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlotIndic : MonoBehaviour
{
    public Renderer model_renderer;

    public TileFace front;

    [SerializeField] private float _xf;
    [SerializeField] private float _yf;
    [SerializeField] private int _id;
    public float Xf { get => _xf; set => _xf = value; }
    public float Yf { get => _yf; set => _yf = value; }
    public int Id { get => _id; set => _id = value; }

    [SerializeField] Collider meeple_collider;

    [SerializeField] private float alpha_ref = 0.2f;
    [SerializeField] private float alpha_front_ref = 0.7f;
    public MeepleRepre meeple_at = null;

    public string Maskname
    {
        set
        {
            mmaskname = value;
        }
        get
        {
            string res = mmaskname;
            mmaskname = null;
            return res;
        }
    }
    private string mmaskname = null;


    public void show(PlayerRepre player)
    {
        Color col = player.color;
        col.a = alpha_ref;
        model_renderer.material.SetColor("_Color", col);
        model_renderer.enabled = true;
        meeple_collider.enabled = true;
    }

    public void hide()
    {
        model_renderer.enabled = false;
        meeple_collider.enabled = false;
    }

    public void clean()
    {
        if (meeple_at != null)
            Destroy(meeple_at);
    }

    public void highlightFace(PlayerRepre player)
    {
        Color col = player.color;
        col.a = alpha_front_ref;
        front.model_renderer.material.SetColor("_Color", col);
        front.gameObject.SetActive(true);
    }

    public void unlightFace()
    {
        front.gameObject.SetActive(false);
    }
    public void setFace(TileFace face)
    {
        front = face;
    }
}
