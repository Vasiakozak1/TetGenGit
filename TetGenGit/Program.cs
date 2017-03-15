using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
namespace TetGen
{
    delegate string ReplaceCharInStr(string line);
    struct Vertex
    {
        public double X { get; private set; }
        public double Y { get; private set; }
        public double Z { get; private set; }
        public Vertex(double X, double Y, double Z)
        {
            this.X = X;
            this.Y = Y;
            this.Z = Z;
        }
    }


    struct Ray
    {
        public Vertex Start { get; private set; }
        public Vertex End { get; private set; }

        public ParallelTo RayParallel { get; private set; }

        public List<Vertex> VertexesOfEntry { get; private set; }

        public static Ray GetRay(Vertex start, Vertex end, ParallelTo rayParallel, params Vertex[] vertexesOfEntry)
        {
            Ray result = new Ray();
            result.Start = start;
            result.End = end;
            result.RayParallel = rayParallel;
            result.VertexesOfEntry = new List<Vertex>();
            if (vertexesOfEntry.Length != 0)
                result.VertexesOfEntry.AddRange(vertexesOfEntry);
            return result;
        }
    }
    enum ParallelTo
    {
        X,
        Y,
        Z
    }

    class Program
    {
        #region Зчитування - запис  
        private static Triangle[] ReadASCIIFile(string fileName)
        {
            ReplaceCharInStr ReplaceDotToComma = (string line) =>
            {
                int dotIndex = line.IndexOf('.');
                StringBuilder tempStr = new StringBuilder();
                tempStr.Append(line.Substring(0, dotIndex));
                tempStr.Append(',');
                tempStr.Append(line.Substring(dotIndex + 1));
                return tempStr.ToString();
            };

            List<Vertex> values = new List<Vertex>();
            List<Triangle> resultTriangles = new List<Triangle>();
            Vertex normal = new Vertex();
            try
            {
                StreamReader readFile = new StreamReader(fileName, Encoding.ASCII);
                using (readFile)
                {
                    while (!readFile.EndOfStream)
                    {
                        try
                        {
                            string nextLine = readFile.ReadLine();
                            string[] line = nextLine.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            if (line[0].Equals("vertex"))
                            {
                                //Крапку перероблюємо у кому бо double.Parse її не розпізнає
                                for (int i = 1; i < 4; i++)
                                    line[i] = ReplaceDotToComma(line[i]);

                                double x = double.Parse(line[1]);
                                double y = double.Parse(line[2]);
                                double z = double.Parse(line[3]);
                                values.Add(new Vertex(x, y, z));
                            }
                            else if (line[0].Equals("facet"))
                            {
                                for (int i = 2; i < 5; i++)
                                    line[i] = ReplaceDotToComma(line[i]);
                                double xNormal = double.Parse(line[2]);
                                double yNormal = double.Parse(line[3]);
                                double zNormal = double.Parse(line[4]);
                                normal = new Vertex(xNormal, yNormal, zNormal);
                            }

                            if (values.Count == 3)
                            {
                                resultTriangles.Add(new Triangle(values, normal));
                                values.Clear();
                            }
                        }
                        catch (FormatException e)
                        {
                            Console.WriteLine("Occured error while reading the file:" + e.Message);
                        }
                    }
                }
            }
            catch (IOException e)
            {
                Console.WriteLine("Виникли проблеми при зчитуванні файлу: " + e.Message);
            }
            catch (IndexOutOfRangeException e)
            {
                Console.WriteLine("Вершина містить недостатньо координат:" + e.Message);
            }
            return resultTriangles.ToArray();
        }

        private static Triangle[] ReadBinaryFile(string fileName)
        {
            List<Vertex> values;
            Stream baseStream = File.Open(fileName, FileMode.Open);
            BinaryReader binRead = new BinaryReader(baseStream);
            List<Triangle> triangles = new List<Triangle>();
            Vertex normal = new Vertex();
            using (binRead)
            {
                binRead.ReadBytes(80);
                uint numTris = binRead.ReadUInt32();
                for (uint i = 0; i < numTris; i++)
                {
                    values = new List<Vertex>();
                    float nx = binRead.ReadSingle();
                    float ny = binRead.ReadSingle();
                    float nz = binRead.ReadSingle();
                    normal = new Vertex(nx, ny, nz);

                    float v1x = binRead.ReadSingle();
                    float v1y = binRead.ReadSingle();
                    float v1z = binRead.ReadSingle();
                    values.Add(new Vertex(v1x, v1y, v1z));

                    float v2x = binRead.ReadSingle();
                    float v2y = binRead.ReadSingle();
                    float v2z = binRead.ReadSingle();
                    values.Add(new Vertex(v2x, v2y, v2z));

                    float v3x = binRead.ReadSingle();
                    float v3y = binRead.ReadSingle();
                    float v3z = binRead.ReadSingle();
                    values.Add(new Vertex(v3x, v3y, v3z));
                    triangles.Add(new Triangle(values, normal));

                    ushort numAttr = binRead.ReadUInt16();
                }
            }
            return triangles.ToArray();
        }

        private static void WriteASCIIFile(Triangle[] triangles, string fileName)
        {
            ReplaceCharInStr ReplaceCommaToDot = (string line) =>
            {
                int commaIndex = line.IndexOf(',');
                if (commaIndex == -1)
                    return line;
                StringBuilder tempStr = new StringBuilder();
                tempStr.Append(line.Substring(0, commaIndex));
                tempStr.Append('.');
                tempStr.Append(line.Substring(commaIndex + 1));
                return tempStr.ToString();
            };

            Stream baseStream = new FileStream(fileName, FileMode.Create);
            StreamWriter writeFile = new StreamWriter(baseStream);
            using (writeFile)
            {
                writeFile.WriteLine("solid\t" + fileName);
                foreach (Triangle triangl in triangles)
                {
                    string normalXStr = ReplaceCommaToDot(triangl.Normal.X.ToString());
                    string normalYStr = ReplaceCommaToDot(triangl.Normal.Y.ToString());
                    string normalZStr = ReplaceCommaToDot(triangl.Normal.Z.ToString());
                    writeFile.WriteLine("facet normal {0} {1} {2}", normalXStr, normalYStr, normalZStr);
                    writeFile.WriteLine("outer loop");

                    for (int i = 0; i < 3; i++)
                    {
                        string vertexXStr = ReplaceCommaToDot(triangl[i].X.ToString());
                        string vertexYStr = ReplaceCommaToDot(triangl[i].Y.ToString());
                        string vertexZStr = ReplaceCommaToDot(triangl[i].Z.ToString());
                        writeFile.WriteLine("vertex {0} {1} {2}", vertexXStr, vertexYStr, vertexZStr);
                    }

                    writeFile.WriteLine("endloop");
                    writeFile.WriteLine("endfacet");
                }
                writeFile.Write("endfacet");
            }
        }

        #endregion

        private static Vertex[] ScanFigure(Triangle[] FigureTriangles, double SizeOfCubes)
        {
            //Крайні координати фігури
            double minX = FigureTriangles[0][0].X;
            double maxX = FigureTriangles[0][0].X;
            double minY = FigureTriangles[0][0].Y;
            double maxY = FigureTriangles[0][0].Y;
            double minZ = FigureTriangles[0][0].Z;
            double maxZ = FigureTriangles[0][0].Z;
            foreach (Triangle triang in FigureTriangles)
            {
                double minXOfTriangle = new double[] { triang[0].X, triang[1].X, triang[2].X }.Min();
                double maxXOfTriangle = new double[] { triang[0].X, triang[1].X, triang[2].X }.Max();

                double minYOfTriangle = new double[] { triang[0].Y, triang[1].Y, triang[2].Y }.Min();
                double maxYOfTriangle = new double[] { triang[0].Y, triang[1].Y, triang[2].Y }.Max();

                double minZOfTriangle = new double[] { triang[0].Z, triang[1].Z, triang[2].Z }.Min();
                double maxZOfTriangle = new double[] { triang[0].Z, triang[1].Z, triang[2].Z }.Max();

                minX = minXOfTriangle < minX ? minXOfTriangle : minX;
                maxX = maxXOfTriangle > maxX ? maxXOfTriangle : maxX;

                minY = minYOfTriangle < minY ? minXOfTriangle : minY;
                maxY = maxYOfTriangle > maxY ? maxYOfTriangle : maxY;

                minZ = minZOfTriangle < minZ ? minZOfTriangle : minZ;
                maxZ = maxZOfTriangle > maxZ ? maxZOfTriangle : maxZ;
            }
            // Знайшли крайні координати

            ParallelTo RayParallelism = ParallelTo.X; // Робимо промені паралельні x

            m1: int sizeOfFirstDimention = (int)((maxX - minX) / SizeOfCubes);
            int sizeOfSecondDimention = (int)((maxY - minY) / SizeOfCubes);
            int sizeOfThirdDimention = (int)((maxZ - minZ) / SizeOfCubes);
            if (sizeOfFirstDimention == 0 || sizeOfSecondDimention == 0 || sizeOfThirdDimention == 0)
            {
                SizeOfCubes = SizeOfCubes / 2;
                goto m1;
            }
            Ray[,,] arrayOfRaysX = new Ray[sizeOfThirdDimention, sizeOfSecondDimention, sizeOfFirstDimention];

            // Проводимо промені паралельні x

            for (double y = minY; y <= maxY; y += SizeOfCubes)
            {
                for (double z = minZ; z <= maxZ; z += SizeOfCubes)
                {
                    Vertex start = new Vertex(minX, y, z);
                    Vertex end = new Vertex(minX + maxX, y, z);
                    Ray tempRay = Ray.GetRay(start, end, RayParallelism);
                }
            }

            return null;
        }



        static void Main(string[] args)
        {
            List<Triangle> resultTriangles = new List<Triangle>();
            args = new string[] { "sphere.stl", "CreatedFile.stl" };
            if (args.Length == 0)
            {
                Console.WriteLine("Програму запущено без параметрів");
                Console.ReadLine();
                return;
            }
            Triangle[] triangles = ReadASCIIFile(args[0]);
            foreach (Triangle triang in triangles)
            {
                resultTriangles.AddRange(triang.DivideTriangle(6));
            }
            WriteASCIIFile(resultTriangles.ToArray(), args[1]);


            Console.ReadLine();
        }
    }
}
