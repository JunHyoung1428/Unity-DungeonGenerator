using System;

namespace Delaunay
{
    public class Vertex
    {
        public readonly int x;
        public readonly int y;

        public Vertex( int x, int y )
        {
            this.x = x;
            this.y = y;
        }


        /*****************************************
         *             Override Methods
         *****************************************/
        #region
        public override string ToString()
        {
            return $"{x},{y}";
        }

        public override bool Equals( object obj )
        {
            return obj is Vertex v && v.x == x && v.y == y;
        }

        // Equals 재정의하면 GetHashCode도 같이 해줘야함, Systems.Collections 에서 HashCode로 두 객체가 동일한지 확인하기 때문

        public override int GetHashCode()
        {
            return HashCode.Combine(x, y); 
        }
        #endregion
        /*****************************************
        *                Operator
        *****************************************/
        #region >,<
        public static bool operator <( Vertex lv, Vertex rv )
        {
            return ( lv.x < rv.x ) || ( ( lv.x == rv.x ) && ( lv.y < rv.y ) );
        }

        public static bool operator >( Vertex lv, Vertex rv )
        {
            return ( lv.x > rv.x ) || ( ( lv.x == rv.x ) && ( lv.y > rv.y ) );
        }
        #endregion
    }

}