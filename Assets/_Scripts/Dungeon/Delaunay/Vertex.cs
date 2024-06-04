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

        // Equals �������ϸ� GetHashCode�� ���� �������, Systems.Collections ���� HashCode�� �� ��ü�� �������� Ȯ���ϱ� ����

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