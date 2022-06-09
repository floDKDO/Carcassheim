using Slot;

public class Tuile
{
    Slot[] _slots;
    int _nombreSlot;
    int[][] _lienSlotPosition;

    public Slot[] Slots => _slots;
    public int[][] LienSlotPosition => _lienSlotPosition;
    public int NombreSlot => _nombreSlot;

    public int X { get; set; }
    public int Y { get; set; }
    public Rotation Rotation { get; set; }

    public Tuile()
    {
        _slots = new Slot[_nombreSlot];
        _lienSlotPosition = new int[_nombreSlot][12];

    }

    public int IdSlotFromPositionInterne(int pos)
    {
        for (int i = 0; i < _nombreSlot; i++)
        {
            foreach (int p in _lienSlotPosition[i])
            {
                if (p == pos)
                    return i;
            }
        }
    }

    public TypeTerrain[] TerrainSurFace(Rotation rot)
    {
        TypeTerrain[] resultat = new TypeTerrain[3];

        int positionInterneRecherchee = rot * 3, compteur = 0;
        
        for (int i = 0; i < _nombreSlot; i++)
        {
            foreach (int position in _lienSlotPosition[i])
            {
                if (position == positionInterneRecherchee)
                {
                    resultat[compteur++] = _slots[i].Terrain;
                    positionInterneRecherchee++;
                }
            }
        }

        return resultat;
    }
}
