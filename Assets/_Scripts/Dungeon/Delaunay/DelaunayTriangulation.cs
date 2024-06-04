using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Delaunay
{
    public class DelaunayTriangulation
    {
       public static HashSet<Triangle> Triangulate( IEnumerable<Vertex> vertices )
        {
            // 슈퍼삼각형 생성 및 슈퍼삼각형을 포함하는 HashSet 생성
            var superTriangle = CalcSuperTriangle( vertices );
            var triangulation = new HashSet<Triangle> { superTriangle };

            //각 Vertex에 대해 수행
            foreach ( var vertex in vertices )
            {
                // 정점이 삼각형 안에 있는 경우(badTriangles)를 담기 위한 HashSet
                HashSet<Triangle> badTriangles = new HashSet<Triangle>();
                foreach ( var triangle in triangulation )
                {
                    if ( triangle.IsInCircumCircle(vertex) )
                        badTriangles.Add(triangle);
                }

                HashSet<Edge> polygon = new HashSet<Edge>();

                foreach ( var badTriangle in badTriangles )
                {
                    foreach ( var edge in badTriangle.edges )
                    {
                        bool isShared = false;
                        foreach ( var otherTriangle in badTriangles )
                        {
                            if ( badTriangle == otherTriangle )
                                continue;
                            if ( otherTriangle.HasEdge(edge) )
                            {
                                isShared = true;
                            }
                        }

                        // edge가 겹치지 않은 경우에 추가
                        if ( !isShared )
                            polygon.Add(edge);
                    }
                }
                // HashSet에서 badTriangles 제거
                triangulation.ExceptWith(badTriangles);

                foreach ( var edge in polygon )
                {
                    triangulation.Add(new Triangle(vertex, edge.a, edge.b));
                }
            }

            // 슈퍼삼각형(과 같은 정점을 공유하는 삼각형) 제거
            triangulation.RemoveWhere(( Triangle t ) => t.HasSameVertex(superTriangle));

            return triangulation;
        }

        private static Triangle CalcSuperTriangle( IEnumerable<Vertex> vertices )
        {
            int minX = int.MaxValue;
            int maxX = int.MinValue;
            int minY = int.MaxValue;
            int maxY = int.MinValue;

            foreach ( var v in vertices )
            {
                minX = Mathf.Min(minX, v.x);
                maxX = Mathf.Max(maxX, v.x);
                minY = Mathf.Min(minY, v.y);
                maxY = Mathf.Max(maxY, v.y);
            }

            int dx = ( maxX - minX + 1 );

            Vertex v1 = new Vertex(( minX - dx - 1 ) * 2, minY - 1 - ( maxY + ( maxY - minY ) + 1 ));
            Vertex v2 = new Vertex(( minX + maxX ) / 2, ( maxY + ( maxY - minY ) + 1 ) * 2);
            Vertex v3 = new Vertex(( maxX + dx + 1 ) * 2, minY - 1 - ( maxY + ( maxY - minY ) + 1 ));

            return new Triangle(v1, v2, v3);
        }

    }
}