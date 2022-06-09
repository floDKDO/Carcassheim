using System.Collections;
using System.Collections.Generic;


public enum DisplaySystemActionTypes
{
    tileSetCoord = 0,
    tileSelection = 1,
    meepleSetCoord = 2,
    meepleSelection = 3,
    StateSelection = 4
};


public abstract class DisplaySystemAction
{
    public DisplaySystemActionTypes action_type;
    public DisplaySystemState required_state = DisplaySystemState.noState;
}

public class DisplaySystemActionTileSetCoord : DisplaySystemAction
{
    public int tile_id;
    public PositionRepre new_pos;


    public DisplaySystemActionTileSetCoord(int tile_id, PositionRepre new_pos)
    {
        required_state = DisplaySystemState.tilePosing;
        action_type = DisplaySystemActionTypes.tileSetCoord;
        this.tile_id = tile_id;
        this.new_pos = new_pos;
    }
}

public class DisplaySystemActionTileSelection : DisplaySystemAction
{
    public int index_in_hand;
    public int tile_id;
    public DisplaySystemActionTileSelection(int tile_id, int index_in_hand)
    {
        action_type = DisplaySystemActionTypes.tileSelection;
        this.tile_id = tile_id;
        this.index_in_hand = index_in_hand;
    }
}

public class DisplaySystemActionMeepleSetCoord : DisplaySystemAction
{
    public int tile_id;
    public PositionRepre tile_pos;
    public int meeple_id;
    public int slot_pos;
    public DisplaySystemActionMeepleSetCoord(int tile_id, PositionRepre tile_pos, int meeple_id, int slot_pos)
    {
        action_type = DisplaySystemActionTypes.meepleSetCoord;
        required_state = DisplaySystemState.meeplePosing;
        this.tile_id = tile_id;
        this.tile_pos = tile_pos;
        this.meeple_id = meeple_id;
        this.slot_pos = slot_pos;
    }
}

public class DisplaySystemActionMeepleSelection : DisplaySystemAction
{
    public int index_in_hand;
    public int meeple_id;
    public DisplaySystemActionMeepleSelection(int meeple_id, int index_in_hand)
    {
        action_type = DisplaySystemActionTypes.meepleSelection;
        this.index_in_hand = index_in_hand;
        this.meeple_id = meeple_id;
    }
}

public class DisplaySystemActionStateSelection : DisplaySystemAction
{
    public DisplaySystemState new_state;

    public DisplaySystemActionStateSelection(DisplaySystemState new_state)
    {
        action_type = DisplaySystemActionTypes.StateSelection;
        this.new_state = new_state;
    }
}
