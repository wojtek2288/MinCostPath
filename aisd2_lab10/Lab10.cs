using System;
using System.Collections.Generic;
using ASD.Graphs;

namespace Lab10
{
    public class Lab10 : MarshalByRefObject
    {
        private readonly (double?, Edge[]) _emptySolutionResult = (null, new Edge[0]);
        private double? minSum = double.MaxValue;
        private List<Edge> list = new List<Edge>();
        /// <summary>
        /// Calculate minimal path between all locations that are still in use
        /// </summary>
        /// <param name="g">Graph representing mountain village with all locations and paths between locations </param>
        /// <param name="terminalVertices">identified locations that are still in use</param>
        /// <returns>
        ///     "sumOfEdgesWeight"- represents minimal path length between all terminal locations
        ///     "path" - represents list of all edges used to build connection between all terminal locations
        ///         NOT VERIFIED IN VERSION1 
        /// </returns>
        public (double? sumOfEdgesWeight, Edge[] path) Version1(Graph g, int[] terminalVertices)
        {
            bool[] visited = new bool[g.VerticesCount];
            List<int> ter = new List<int>();
            double pLimit = 1;
            double Limit = double.MaxValue;
            int idx = 0;

            for(int i = 0; i < terminalVertices.Length; i++)
            {
                g.DijkstraShortestPaths(terminalVertices[i], out PathsInfo[] p);
                pLimit = 1;
                foreach (var j in terminalVertices)
                {
                    if (double.IsNaN(p[j].Dist))
                        return (null, new Edge[0]);
                    if (pLimit > Limit)
                        break;
                    pLimit += p[j].Dist;
                }
                if (pLimit < Limit)
                {
                    Limit = pLimit;
                    idx = i;
                }
            }

            ter.Add(terminalVertices[idx]);
            visited[terminalVertices[idx]] = true;

            minSum = Limit;
            list.Clear();

            Version1Rec(g, ter, new List<Edge>(), terminalVertices, 0, visited);
            if(minSum == double.MaxValue)
                return (null, new Edge[0]);

            return (minSum, list.ToArray());
        }

        public (double? sumOfEdgesWeight, List<Edge> path) Version1Rec(Graph g, List<int> Vert, List<Edge> path, int[] terminalVertices, double Sum, bool[] visited)
        {
            bool flag = true;
            foreach(var i in terminalVertices)
            {
                if(!visited[i])
                {
                    flag = false;
                    break;
                }
            }
            if(flag)
            {
                return (Sum, path);
            }

            for(int i = 0; i < Vert.Count; i++)
            {
                if (Sum + 1 >= minSum)
                    break;

                foreach(var edge in g.OutEdges(Vert[i]))
                {
                    if (visited[edge.To] || Sum + edge.Weight >= minSum)
                        continue;

                    Vert.Add(edge.To);
                    visited[edge.To] = true;
                    path.Add(edge);

                    var (res, p) = Version1Rec(g, Vert, path, terminalVertices, Sum + edge.Weight, visited);

                    if(res != null && res < minSum)
                    {
                        minSum = res;
                        list.Clear();
                        foreach (var j in p)
                            list.Add(j);
                    }

                    visited[edge.To] = false;
                    path.RemoveAt(path.Count - 1);
                    Vert.RemoveAt(Vert.Count - 1);
                }
            }
            return (minSum, list);
        }

        /// <summary>
        /// Calculate minimal path between all locations that are still in use
        /// </summary>
        /// <param name="g">Graph representing mountain village with all locations and paths between locations </param>
        /// <param name="terminalVertices">identified locations that are still in use</param>
        /// <returns>
        ///     "sumOfEdgesWeight"- represents minimal path length between all terminal locations
        ///     "path" - represents list of all edges used to build connection between all terminal locations
        /// </returns>
        public (double? sumOfEdgesWeight, Edge[] path) Version2(Graph g, int[] terminalVertices)
        {
            return Version1(g, terminalVertices);
        }
    }
}