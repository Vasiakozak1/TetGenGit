using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TetGen
{
    static class ListExtension
    {
        public static bool IsHaveThePlane(this List<Vertex[]> VertexesList, Vertex[] VertexesForCheck)
        {
            foreach (Vertex[] currentPlane in VertexesList)
            {
                int checkCount = 0;
                foreach (Vertex vrtx in currentPlane)
                {
                    foreach (Vertex vrtxForCheck in VertexesForCheck)
                    {
                        if (vrtx == vrtxForCheck)
                        {
                            checkCount++;
                            continue;
                        }
                    }
                }
                if (checkCount == VertexesForCheck.Length)
                    return true;
            }
            return false;
        }
    }
}
