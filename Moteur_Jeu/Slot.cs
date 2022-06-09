public class Slot
{
    public TypeTerrain Terrain { get; set; }
    public int IdJoueur { get; set; }

    public Slot(TypeTerrain terrain)
    {
        Terrain = terrain;
        IdJoueur = 0;
    }
}