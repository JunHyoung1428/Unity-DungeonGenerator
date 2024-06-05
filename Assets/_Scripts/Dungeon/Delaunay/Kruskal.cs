using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Delaunay
{
    /// <summary>
    /// Find MST(Minimum Spanning Tree) With Kruskal's Algorithm. cost : 𝑂(𝐸𝑙𝑜𝑔𝑉)
    /// </summary>
    public class Kruskal
    {
        /// <summary>
        /// Minimum Spanning Tree
        /// </summary>
        /// <param name="graph"></param>
        /// <returns></returns>
        public static List<Edge> MST( IEnumerable<Edge> graph, int unionRate=25)
        {
            //반환할 최소 신장 트리의 간선들
            List<Edge> mst = new List<Edge>();    
            List<Edge> edges = new List<Edge>(graph);
            edges.Sort(Edge.LengthCompare);

            HashSet<Vertex> points = new HashSet<Vertex>();
            //각 간선들의 정점을 HashSet에 추가
            foreach(Edge edge in edges)
            {
                points.Add(edge.a);
                points.Add(edge.b);
            }

            Dictionary<Vertex, Vertex> parents = new Dictionary<Vertex, Vertex>();
            foreach ( var point in points )
                parents [point] = point;


            // 정점의 부모를 찾는 로컬 메서드
            Vertex Find(Vertex x )
            {
                if ( parents [x] == x ) return x;
                parents [x] = Find(parents [x]);

                return parents [x];
            }

            // 간선의 부모를 합치는 로컬 메서드, 합친 간선은 MST에 추가
            void Union( Edge edge )
            {
                var x_par = Find(edge.a);
                var y_par = Find(edge.b);

                // 만약 같은 경우에는 랜덤하게 일부 간선을 선택하여 연결
                if ( x_par == y_par )
                {
                    // 1/6 의 확률, 필요시 조정
                    if ( Random.Range(0, 100) < unionRate )
                    {
                        mst.Add(edge);
                    }
                    return;
                }

                //간선을 최소 신장 트리에 추가
                mst.Add(edge);

                // 부모를 합치는 과정
                if ( x_par < y_par ) parents [y_par] = x_par;
                else parents [x_par] = y_par;
            }


            //모든 간선을 순회하며 MST 구성
            foreach ( Edge edge in edges )
                Union(edge);

            return mst;
        }
    }

}
