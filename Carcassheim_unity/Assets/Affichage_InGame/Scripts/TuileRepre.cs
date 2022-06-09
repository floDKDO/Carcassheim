using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TuileRepre : MonoBehaviour
{

    // * LOOK *************************************************
    public Transform pivotPoint;
    public GameObject model;

    [SerializeField] private Collider body_collider;

    [SerializeField] private TMPro.TMP_Text _id_repre;
    [SerializeField] private List<SlotIndic> slots;
    [SerializeField] private Transform rep_O, rep_u, rep_v;

    Dictionary<int, SlotIndic> slots_mapping;

    int _index = -1;
    public int Index { set => _index = value; get => _index; }

    public int MaxSlot { set; get; }

    // * STAT *************************************************
    private int _id = 0;

    public int Id
    {
        set { _id = value; _id_repre.text = value.ToString(); if (value < 0) _id_repre.gameObject.SetActive(false); }
        get { return _id; }
    }

    // * POSITION *********************************************
    private PositionRepre _pos;
    public PositionRepre Pos
    {
        set
        {
            // Debug.Log("Setting position " + (_pos == null ? "nothing" : _pos.ToString()) + " to " + (value == null ? "nothing" : value.ToString()));
            _pos = value;
            body_collider.enabled = _pos != null;
            int rotation = _pos != null ? _pos.Rotation : 0;
            transform.localRotation = Quaternion.Euler(0, 0, rotation * 90);
            setIndexFromPos();
        }
        get => _pos;
    }
    public List<PositionRepre> possibilitiesPosition = new List<PositionRepre>();

    void Awake()
    {
        slots_mapping = new Dictionary<int, SlotIndic>();
        foreach (SlotIndic slot in slots)
        {
            slots_mapping.Add(slot.Id, slot);
        }
        MaxSlot = slots_mapping.Count;
    }

    public void showPossibilities(PlayerRepre player)
    {
        foreach (SlotIndic slot in slots)
        {
            slot.show(player);
        }
    }
    public void showPossibilities(PlayerRepre player, List<int> slot_pos)
    {
        hidePossibilities();
        foreach (int index in slot_pos)
        {
            slots[index].show(player);
        }
    }

    public void hidePossibilities()
    {
        foreach (SlotIndic slot in slots)
        {
            slot.hide();
        }
    }

    public void highlightFace(PlayerRepre player, int id)
    {
        slots_mapping[id].highlightFace(player);
    }

    public void unlightFace(int id)
    {
        slots_mapping[id].unlightFace();
    }

    public PositionRepre isPossible(PositionRepre pos)
    {
        if (pos == null)
            return null;
        foreach (PositionRepre true_pos in possibilitiesPosition)
        {
            if (true_pos.X == pos.X && true_pos.Y == pos.Y && (pos.Rotation == -1 || true_pos.Rotation == pos.Rotation))
                return true_pos;
        }
        return null;
    }

    public void setIndexFromPos()
    {
        if (Pos == null)
            _index = -1;
        for (int idx = 0; idx < possibilitiesPosition.Count; idx++)
        {
            if (possibilitiesPosition[idx] == Pos)
                _index = idx;
        }
    }


    public bool nextRotation()
    {
        if (Pos == null)
            return false;
        int x = Pos.X;
        int y = Pos.Y;
        int rotation = Pos.Rotation;
        bool found = false;
        for (int i = 0; i < 3; i++)
        {
            rotation = (rotation + 1) % 4;
            if (isPossible(new PositionRepre(x, y, rotation)) != null)
            {
                found = true;
                break;
            }
        }
        if (found)
        {
            Pos = new PositionRepre(x, y, rotation);
        }
        return found;
    }

    public bool nextRotation(out PositionRepre npos)
    {
        npos = null;
        if (Pos == null)
            return false;
        int x = Pos.X;
        int y = Pos.Y;
        int rotation = Pos.Rotation;
        bool found = false;
        for (int i = 0; i < 3; i++)
        {
            rotation = (rotation + 1) % 4;
            if (isPossible(new PositionRepre(x, y, rotation)) != null)
            {
                found = true;
                break;
            }
        }
        if (found)
        {
            npos = new PositionRepre(x, y, rotation);
        }
        return found;
    }

    public void addSlot(SlotIndic slot)
    {
        slot.transform.parent = pivotPoint;
        slot.front.transform.parent = pivotPoint;
        Vector3 whynot = new Vector3(0, 0.0772999972f, -0.00100000005f);
        slot.front.transform.localPosition = whynot;
        slot.transform.localPosition = rep_O.localPosition + (rep_u.localPosition - rep_O.localPosition) * slot.Xf + (rep_v.localPosition - rep_O.localPosition) * slot.Yf;
        slots.Add(slot);
    }

    public SlotIndic getSlotAt(int id_slot)
    {
        if (slots_mapping.ContainsKey(id_slot))
            return slots_mapping[id_slot];
        return null;
    }

    public bool setMeeplePos(MeepleRepre meeple, int id_slot)
    {
        if (id_slot == -1)
        {
            TuileRepre old_parent = meeple.ParentTile;
            meeple.ParentTile = null;
            meeple.SlotPos = -1;
            return old_parent == null;

        }
        SlotIndic slot_indic;
        if (slots_mapping.TryGetValue(id_slot, out slot_indic))
        {
            return setMeeplePos(meeple, slot_indic);
        }
        else
            return false;
    }
    public bool setMeeplePos(MeepleRepre meeple, SlotIndic slot_indic)
    {
        if (meeple == null)
        {
            return false;
        }
        TuileRepre old_parent = meeple.ParentTile;
        meeple.ParentTile = this;
        meeple.SlotPos = slot_indic.Id;
        meeple.transform.parent = slot_indic.transform;
        meeple.transform.localPosition = new Vector3(0, 0, 0);
        slot_indic.meeple_at = meeple;
        return old_parent == null;
    }
}