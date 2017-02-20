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
    struct Triangle
    {
        private Vertex[] Vertexes;
        public Vertex Normal { get; private set; }
        public Vertex this[int index]
        {
            get { return this.Vertexes[index]; }
        }
        public Triangle(IEnumerable<Vertex> Vertexes, Vertex Normal)
        {
            int count = 0;
            this.Vertexes = new Vertex[3];
            foreach (Vertex vertx in Vertexes)
            {
                this.Vertexes[count] = vertx;
                count++;
            }
            this.Normal = Normal;
        }
        /// <summary>
        /// Розділяє трикутник теперішнього екземпляру на декілька
        /// При cooficient = 2 розділяє 1 раз при 3 - 2 рази і так далі
        /// </summary>
        /// <param name="cooficient"></param>
        /// <returns></returns>
        public Triangle[] DivideTriangle(int cooficient)
        {
            Vertex firstSharedSide = new Vertex();
            Vertex secondSharedSize = new Vertex();
            Vertex another1 = new Vertex();
            Vertex another2 = new Vertex();
            List<Triangle> resultTriangles = new List<Triangle>();
            for (int i = 1; i < cooficient; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    another1 = new Vertex();
                    another2 = new Vertex();
                    switch (j)
                    {
                        case 0:
                            another1 = this.Vertexes[1]; another2 = this.Vertexes[2];
                            break;
                        case 1:
                            another1 = this.Vertexes[0]; another2 = this.Vertexes[2];
                            break;
                        case 2:
                            another1 = this.Vertexes[1]; another2 = this.Vertexes[0];
                            break;
                    }
                    if (this.Vertexes[j].X < another1.X && this.Vertexes[j].X > another2.X || this.Vertexes[j].X > another1.X && this.Vertexes[j].X < another2.X)
                    {
                        firstSharedSide = this.Vertexes[j];
                        break;
                    }
                    else if (this.Vertexes[j].Y < another1.Y && this.Vertexes[j].Y > another2.Y || this.Vertexes[j].Y > another1.Y && this.Vertexes[j].Y < another2.Y)
                    {
                        firstSharedSide = this.Vertexes[j];
                        break;
                    }
                    else if (j == 2)
                    {
                        int randVertex = new Random().Next(0, 3);
                        switch (randVertex)
                        {
                            case 0:
                                another1 = this.Vertexes[1]; another2 = this.Vertexes[2]; firstSharedSide = this.Vertexes[0];
                                break;
                            case 1:
                                another1 = this.Vertexes[0]; another2 = this.Vertexes[2]; firstSharedSide = this.Vertexes[1];
                                break;
                            case 2:
                                another1 = this.Vertexes[1]; another2 = this.Vertexes[0]; firstSharedSide = this.Vertexes[2];
                                break;
                        }
                    }
                }
                double coordX = (another1.X + another2.X) / 2;
                double coordY = (another1.Y + another2.Y) / 2;
                double coordZ = (another1.Z + another2.Z) / 2;
                secondSharedSize = new Vertex(coordX, coordY, coordZ);
                resultTriangles.AddRange(GetTriangles(new Vertex[] { firstSharedSide, secondSharedSize }, another1, another2));
            }
            return resultTriangles.ToArray();
        }/// <summary>
         /// Формуж нові екземплряри трикутників із вказаних вершин
         /// </summary>
         /// <param name="sharedSide"></param>
         /// <param name="another1"></param>
         /// <param name="another2"></param>
         /// <returns></returns>
        private Triangle[] GetTriangles(Vertex[] sharedSide, Vertex another1, Vertex another2)
        {
            try
            {
                Vertex[] vertexesForFirst = new Vertex[] { sharedSide[0], sharedSide[1], another1 };
                Vertex[] vertexesForSecond = new Vertex[] { sharedSide[0], sharedSide[1], another2 };
                Triangle first = new Triangle(vertexesForFirst, new Vertex());//Треба враховувати вертекс нормалі
                Triangle second = new Triangle(vertexesForSecond, new Vertex());//Треба враховувати вертекс нормалі
                return new Triangle[] { first, second };
            }
            catch (IndexOutOfRangeException e)
            {
                Console.WriteLine("Occured error when divided triangle " + e.Message);
                return null;
            }
        }
    }
    class Program
    {

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
                resultTriangles.AddRange(triang.DivideTriangle(2));
            }
            WriteASCIIFile(resultTriangles.ToArray(), args[1]);
            // foreach (Triangle tr in triangles)
            //  {
            //       Console.WriteLine(tr[0].X+" "+tr[0].Y+" "+tr[0].Z+"\t\t"+tr[1].X+" "+tr[1].Y+" "+tr[1].Z+"\t\t"+tr[2].X+" "+tr[2].Y+" "+tr[2].Z);
            //  }
            Console.ReadLine();
        }
    }
}
