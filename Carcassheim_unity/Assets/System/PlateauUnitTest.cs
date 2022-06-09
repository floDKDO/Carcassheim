namespace Assets.system
{
    public class PlateauUnitTest
    {
        Tuile _t1;
        Plateau _subject;
        public PlateauUnitTest()
        {
            int[][] lien = new int[][]
            {
            new int[] {1, 7},
            new int[] {2, 3},
            new int[] {4, 5, 6},
            new int[] {8, 9, 10},
            new int[] {11, 0}
            };
            TypeTerrain[] ter = new TypeTerrain[]
            {
            TypeTerrain.Route,
            TypeTerrain.Ville,
            TypeTerrain.Pre,
            TypeTerrain.Auberge,
            TypeTerrain.Abbaye
            };
            _t1 = new Tuile(0, 5, lien, ter);
        }

    }
}

