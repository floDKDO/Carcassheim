using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TileIndicatorState
{
    TilePossibilitie,
    TilePosed,
    LastTile
};

public class TileIndicator : MonoBehaviour
{
    [SerializeField] Collider tile_collider;

    private TileIndicatorState _state = TileIndicatorState.TilePossibilitie;
    public TileIndicatorState state
    {
        set
        {
            _state = value;
            if (value == TileIndicatorState.TilePossibilitie)
                tile_collider.enabled = true;
            else
                tile_collider.enabled = false;
            if (value == TileIndicatorState.LastTile)
                model_renderer.enabled = true;
        }
        get => _state;
    }
    public PositionRepre position = new PositionRepre();
    public PlayerRepre player;
    public Renderer model_renderer;

    [SerializeField] private float alpha_ref = 1f;

    public void setAttributes(PlayerRepre player, PositionRepre pos)
    {
        this.position = pos;
        this.player = player;
        Color col = player.color;
        col.a = alpha_ref;
        model_renderer.material.SetColor("_Color", col);
    }

    public void display()
    {
        model_renderer.enabled = true;
        if (state == TileIndicatorState.TilePossibilitie)
            tile_collider.enabled = true;
    }


    public void hide()
    {
        model_renderer.enabled = false;
        if (state == TileIndicatorState.TilePossibilitie)
            tile_collider.enabled = false;
    }
}
