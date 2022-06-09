namespace Assets.system
{
    /// <summary>
    /// definie la position d'une tuile posee sur le plateau
    /// </summary>
    public struct Position
    {
        /// <summary>
        /// definie la position
        /// </summary>
        int _x, _y, _rot;

        /// <summary>
        /// l'abscisse de la tuile sur le plateau
        /// </summary>
        public int X => _x;

        /// <summary>
        /// l'ordonnee de la tuile sur le plateau
        /// </summary>
        public int Y => _y;

        /// <summary>
        /// la rotation de la tuile
        /// </summary>
        public int ROT => _rot;

        /// <summary>
        /// instancie une position
        /// </summary>
        /// <param name="x">abscisse</param>
        /// <param name="y">ordonnee</param>
        /// <param name="rot">rotation</param>
        public Position(int x, int y, int rot)
        {
            _x = x;
            _y = y;
            _rot = rot;
        }

        /// <summary>
        /// override pour faciliter le deboggage
        /// </summary>
        /// <returns>une chaine de la forme (x, y) [point cardinal correspondant a la rotation]</returns>
        public override string ToString()
        {
            string r = "";
            switch (ROT)
            {
                case 0:
                    r = "nord";
                    break;
                case 1:
                    r = "est";
                    break;
                case 2:
                    r = "sud";
                    break;
                case 3:
                    r = "ouest";
                    break;
                default:
                    r = "lol";
                    break;

            }
            return "("+ _x.ToString()+", " + _y.ToString()+", " + r + ")";
        }

        /*public static explicit operator(PositionRepr) (Position p)
        {
            return new PositionRepr(p._x, p._y, p._rot);
        }

        public static explicit operator(Position) (PositionRepr p)
        {
            return new Position(p._x, p._y, p._rot);
        }*/
    }
}
