using System;
using System.Collections.Generic;

namespace TetGen
{
    struct Parallelepiped
    {
        /// <summary>
        /// 
        /// </summary>
        public bool IsOrdered { get; private set; }
        public List<Vertex> VertexesOfFigure { get; private set; }

        public Vertex this[int index]
        {
            get { return VertexesOfFigure[index]; }
        }
        /// <summary>
        /// Ініціалізує паралелепіпед, він впорядкований якщо наперед відомі точки фігури
        /// Вважається що порядок буде такий: a1,b1,c1,d1,a2,b2,c2,d2
        /// </summary>
        /// <param name="IsOrdered"></param>
        /// <param name="Vertexes"></param>
        public Parallelepiped(bool IsOrdered, params Vertex[] Vertexes)
        {
            this.IsOrdered = IsOrdered;
            VertexesOfFigure = new List<Vertex>(8);

            foreach (Vertex vrtx in Vertexes)
            {
                VertexesOfFigure.Add(vrtx);

            }
            if (VertexesOfFigure.Count != 8)
                throw new Exception("Count of parallelepiped vertexes must be 8!");
        }
        /// <summary>
        ///  З точок перетину променів які всередині фігури утворюються паралелепіпеди, які потім перетворюються в тетраедри
        /// </summary>
        /// <param name="VertexesOfCrosingXY"></param>
        /// <param name="RaysOfParallelZ"></param>
        /// <returns></returns>
        public static Parallelepiped[] GetParallelepipedsInsideFigure(Vertex[] VrtxsOfCrossingXYRays
            , List<Ray> RaysOfParallelZ, Ray[][] RaysParallelX, Ray[][] RaysParallelY)
        {
            List<Parallelepiped> resultList = new List<Parallelepiped>();
            List<double> zCoordinates = new List<double>();
            foreach (Ray[] vrtxs in RaysParallelY)
            {
                foreach (Ray ray in vrtxs)
                {
                    if (!zCoordinates.Contains(ray.Start.Z))
                        zCoordinates.Add(ray.Start.Z);
                }

            }
            List<double> yCoordinates = new List<double>();
            foreach (Ray r in RaysOfParallelZ)
            {
                if (!yCoordinates.Contains(r.Start.Y))
                    yCoordinates.Add(r.Start.Y);
            }
            List<double> xCoordinates = new List<double>();
            foreach (Ray r in RaysOfParallelZ)
            {
                if (!xCoordinates.Contains(r.Start.X))
                    xCoordinates.Add(r.Start.X);
            }

            xCoordinates.Sort();
            yCoordinates.Sort();
            zCoordinates.Sort();

            for (int i = 0; i < zCoordinates.Count - 1; i++)
            {
                for (int j = 0; j < yCoordinates.Count - 1; j++)
                {
                    for (int k = 0; k < xCoordinates.Count - 1; k++)
                    {
                        #region test


                        #endregion
                        Vertex a1 = new Vertex(xCoordinates[k], yCoordinates[j], zCoordinates[i]);
                        Vertex b1 = new Vertex(xCoordinates[k], yCoordinates[j], zCoordinates[i + 1]);
                        Vertex c1 = new Vertex(xCoordinates[k + 1], yCoordinates[i], zCoordinates[i + 1]);
                        Vertex d1 = new Vertex(xCoordinates[k + 1], yCoordinates[i], zCoordinates[i]);

                        Vertex a2 = new Vertex(xCoordinates[k], yCoordinates[j + 1], zCoordinates[i]);
                        Vertex b2 = new Vertex(xCoordinates[k], yCoordinates[j + 1], zCoordinates[i + 1]);
                        Vertex c2 = new Vertex(xCoordinates[k + 1], yCoordinates[j + 1], zCoordinates[i + 1]);
                        Vertex d2 = new Vertex(xCoordinates[k + 1], yCoordinates[j + 1], zCoordinates[i]);
                        Vertex[] vertexesOfParPpd = new Vertex[] { a1, b1, c1, d1, a2, b2, c2, d2 };


                        // Перевіряємо чи підходять ці точки
                        bool checkFlag = true;

                        foreach (Vertex vrtx in vertexesOfParPpd)
                        {


                            //Промені які проходять через цю точку
                            Ray rayX = default(Ray);
                            Ray rayY = default(Ray);
                            Ray rayZ = default(Ray);
                            double misstep = 1e-8;// похибка
                            foreach (Ray[] raysParX in RaysParallelX)
                            {
                                bool endLoopFlag = false;
                                foreach (Ray rX in raysParX)
                                {
                                    if (vrtx.Y + misstep >= rX.End.Y && vrtx.Y - misstep <= rX.End.Y
                                     && vrtx.Z + misstep >= rX.End.Z && vrtx.Z - misstep <= rX.End.Z)
                                    {
                                        // Найшли промінь який проходить через точку
                                        rayX = rX;
                                        endLoopFlag = true;
                                        break;
                                    }
                                }
                                if (endLoopFlag)
                                    break;
                            }

                            foreach (Ray[] raysParY in RaysParallelY)
                            {
                                bool endLoopFlag = false;
                                foreach (Ray rY in raysParY)
                                {
                                    if (vrtx.X + misstep >= rY.End.X && vrtx.X - misstep <= rY.End.X
                                      && vrtx.Z + misstep >= rY.End.Z && vrtx.Z - misstep <= rY.End.Z)
                                    {
                                        // Найшли промінь який проходить через точку
                                        rayY = rY;
                                        endLoopFlag = true;
                                        break;
                                    }
                                }
                                if (endLoopFlag)
                                    break;
                            }

                            foreach (Ray rZ in RaysOfParallelZ)
                            {

                                if (vrtx.X + misstep >= rZ.End.X && vrtx.X - misstep <= rZ.End.X
                                  && vrtx.Y + misstep >= rZ.End.Y && vrtx.Y - misstep <= rZ.End.Y)
                                {
                                    // Найшли промінь який проходить через точку
                                    rayZ = rZ;
                                    break;
                                }
                            }

                            if (rayX == default(Ray) || rayY == default(Ray) || rayZ == default(Ray))
                            {
                                checkFlag = false;
                                break;
                            }

                            // Перевіряємо чи точки розташовані між точками входу променів у тіло
                            bool flagX = false;
                            for (int a = 0; a < rayX.VertexesOfEntry.Count - 1; a++)
                            {
                                if (vrtx.X >= rayX.VertexesOfEntry[a].X && vrtx.X <= rayX.VertexesOfEntry[a + 1].X ||
                                    vrtx.X <= rayX.VertexesOfEntry[a].X && vrtx.X >= rayX.VertexesOfEntry[a + 1].X)
                                    flagX = true;
                            }
                            bool flagY = false;
                            for (int a = 0; a < rayY.VertexesOfEntry.Count - 1; a++)
                            {
                                if (vrtx.Y >= rayY.VertexesOfEntry[a].Y && vrtx.Y <= rayY.VertexesOfEntry[a + 1].Y ||
                                    vrtx.Y <= rayY.VertexesOfEntry[a].Y && vrtx.Y >= rayY.VertexesOfEntry[a + 1].Y)
                                    flagY = true;
                            }
                            bool flagZ = false;
                            for (int a = 0; a < rayZ.VertexesOfEntry.Count - 1; a++)
                            {
                                if (vrtx.Z >= rayZ.VertexesOfEntry[a].Z && vrtx.Z <= rayZ.VertexesOfEntry[a + 1].Z ||
                                   vrtx.Z <= rayZ.VertexesOfEntry[a].Z && vrtx.Z >= rayZ.VertexesOfEntry[a + 1].Z)
                                    flagZ = true;
                            }

                            if (!flagX || !flagY || !flagZ)
                            {
                                checkFlag = false;
                                break;
                            }

                        }
                        if (checkFlag)
                        {
                            resultList.Add(new Parallelepiped(true, vertexesOfParPpd));
                        }
                    }
                }
            }

            return resultList.ToArray();
        }

        /// <summary>
        /// Розбиваємо паралелепіпед на тетраедри
        /// </summary>
        /// <param name="vertexesOfCube"></param>
        /// <returns></returns>
        public Triangle[] DivideParallelepiped()
        {

            // отримуємо точку в центрі куба
            Vertex centerVertex;

            double maxLength = 0;
            // Вершини які при з`єднанні утворюють найбільшу діагональ
            Vertex verDiagonal1 = new Vertex();
            Vertex verDiagonal2 = new Vertex();
            for (int i = 0; i < VertexesOfFigure.Count; i++)
            {
                for (int j = 0; j < VertexesOfFigure.Count; j++)
                {
                    if (i == j)
                        continue;
                    double lengthOfLine = Math.Sqrt((this[i].X - this[j].X) * (this[i].X - this[j].X)
                        + (this[i].Y - this[j].Y) * (this[i].Y - this[j].Y)
                        + (this[i].Z - this[j].Z) * (this[i].Z - this[j].Z));
                    if (lengthOfLine > maxLength)
                    {
                        maxLength = lengthOfLine;
                        verDiagonal1 = this[i];
                        verDiagonal2 = this[j];
                    }

                }
            }
            // Точка в центрі куба
            centerVertex = new Vertex((verDiagonal1.X + verDiagonal2.X) / 2, (verDiagonal1.Y + verDiagonal2.Y) / 2, (verDiagonal1.Z + verDiagonal2.Z) / 2);


            List<Vertex[]> listOfPlanes = new List<Vertex[]>(6); // Колекція із координатами вершин плоскостей

            switch (this.IsOrdered)
            {
                // див. Image of cube.png
                case true:
                    listOfPlanes.Add(new Vertex[] { this[0], this[1], this[4], this[5] });// Ліва грань
                    listOfPlanes.Add(new Vertex[] { this[0], this[4], this[7], this[3] });// Передня грань
                    listOfPlanes.Add(new Vertex[] { this[4], this[5], this[6], this[7] });// Верхня грань

                    listOfPlanes.Add(new Vertex[] { this[2], this[3], this[7], this[6] });// Права грань 
                    listOfPlanes.Add(new Vertex[] { this[2], this[1], this[5], this[6] });// Задня грань
                    listOfPlanes.Add(new Vertex[] { this[0], this[1], this[2], this[3] });// Нижня грань
                    break;
                case false:
                    Random rnd = new Random();
                    Vertex diagonalVertex = verDiagonal1;
                    // Покищо пошук 4 точок на одній плоскості реалізовано не дуже...
                    for (int i = 0; i < 2; i++)
                    {
                        int indexDiagonal = this.VertexesOfFigure.IndexOf(diagonalVertex);
                        int countOfPlanes = 0;
                        while (countOfPlanes < 3)
                        {
                            int indexSecond = rnd.Next(0, 8);
                            int indexThird = rnd.Next(0, 8);
                            int indexFourth = rnd.Next(0, 8);

                            while (indexSecond == indexThird || indexSecond == indexFourth || indexSecond == indexDiagonal
                                || indexThird == indexFourth || indexThird == indexDiagonal || indexFourth == indexDiagonal)
                            {
                                indexSecond = rnd.Next(0, 8);
                                indexThird = rnd.Next(0, 8);
                                indexFourth = rnd.Next(0, 8);
                            }
                            Vertex secondVert = VertexesOfFigure[indexSecond];
                            Vertex thrirdVert = VertexesOfFigure[indexThird];
                            Vertex fourthVert = VertexesOfFigure[indexFourth];

                            Vertex[] fourVertexesForCheck = new Vertex[] { diagonalVertex, secondVert, thrirdVert, fourthVert };
                            bool isOnSamePlane = IsOnTheSamePlane(fourVertexesForCheck);
                            if (!isOnSamePlane)
                                continue;
                            bool isHavePlane = listOfPlanes.IsHaveThePlane(fourVertexesForCheck);


                            if (!isHavePlane && isOnSamePlane)
                            {
                                countOfPlanes++;
                                listOfPlanes.Add(fourVertexesForCheck);
                            }

                        }
                        diagonalVertex = verDiagonal2;

                    }
                    break;
            }




            List<Triangle> resultTriangles = new List<Triangle>();
            foreach (Vertex[] plane in listOfPlanes)
            {
                maxLength = 0;
                for (int i = 0; i < plane.Length; i++)
                {
                    for (int j = 1; j < plane.Length; j++)
                    {
                        if (i == j)
                            continue;
                        double tempLength = Math.Sqrt((plane[i].X - plane[j].X) * (plane[i].X - plane[j].X)
                        + (plane[i].Y - plane[j].Y) * (plane[i].Y - plane[j].Y)
                        + (plane[i].Z - plane[j].Z) * (plane[i].Z - plane[j].Z));
                        if (tempLength > maxLength)
                        {
                            verDiagonal1 = plane[i];
                            verDiagonal2 = plane[j];
                            maxLength = tempLength;
                        }
                    }
                }

                Vertex anotherVertex1 = new Vertex();
                Vertex anotherVertex2 = new Vertex();
                foreach (Vertex vrtx in plane)
                {
                    if (vrtx == verDiagonal1 || vrtx == verDiagonal2)
                        continue;
                    if (anotherVertex1 == new Vertex())
                        anotherVertex1 = vrtx;
                    else
                        anotherVertex2 = vrtx;
                }
                //Утворюються 7 трикутників з отриманих вершин
                resultTriangles.Add(new Triangle(verDiagonal1, verDiagonal2, anotherVertex1));
                resultTriangles.Add(new Triangle(anotherVertex1, verDiagonal1, centerVertex));
                resultTriangles.Add(new Triangle(anotherVertex1, verDiagonal2, centerVertex));

                resultTriangles.Add(new Triangle(verDiagonal1, verDiagonal2, centerVertex));

                resultTriangles.Add(new Triangle(verDiagonal1, verDiagonal2, anotherVertex2));
                resultTriangles.Add(new Triangle(verDiagonal1, anotherVertex2, centerVertex));
                resultTriangles.Add(new Triangle(verDiagonal2, anotherVertex2, centerVertex));
            }

            return resultTriangles.ToArray();
        }
        /// <summary>
        /// Перевірка яи точки находяться на одній площині
        /// </summary>
        /// <param name="vertexesOfPlane">Масив із чотирма точками</param>
        /// <returns></returns>
        private static bool IsOnTheSamePlane(Vertex[] vertexesOfPlane)
        {
            Vertex vectorAB =
                new Vertex(vertexesOfPlane[1].X - vertexesOfPlane[0].X, vertexesOfPlane[1].Y - vertexesOfPlane[0].Y, vertexesOfPlane[1].Z - vertexesOfPlane[0].Z);
            Vertex vectorAC =
                new Vertex(vertexesOfPlane[2].X - vertexesOfPlane[0].X, vertexesOfPlane[2].Y - vertexesOfPlane[0].Y, vertexesOfPlane[2].Z - vertexesOfPlane[0].Z);
            Vertex vectorAD =
                new Vertex(vertexesOfPlane[3].X - vertexesOfPlane[0].X, vertexesOfPlane[3].Y - vertexesOfPlane[0].Y, vertexesOfPlane[3].Z - vertexesOfPlane[0].Z);

            // Находимо змішаний добуток векторів
            double firstPart = (vectorAB.X * vectorAC.Y * vectorAD.Z) + (vectorAC.X * vectorAB.Z * vectorAD.Y) + (vectorAB.Y * vectorAC.Z * vectorAD.X);
            double secondPart = -(vectorAB.Z * vectorAC.Y * vectorAD.X) - (vectorAB.X * vectorAC.Z * vectorAD.Y) - (vectorAB.Y * vectorAC.X * vectorAD.Z);
            if (firstPart - secondPart == 0)
                return true;
            return false;
        }

        /// <summary>
        /// Перевіряє чи паралелепіпед перетинає якусь сторону фігури чи ні
        /// </summary>
        /// <param name="VertexesOfFigure">Координати фігури</param>
        /// <param name="TrianglesOfFigure">Трикутники фігури</param>
        /// <returns></returns>
        private static bool IsInsideFigure(Vertex[] VertexesOfFigure, Triangle[] TrianglesOfFigure)
        {
            foreach (Vertex vrtx1 in VertexesOfFigure)
            {
                foreach (Vertex vrtx2 in VertexesOfFigure)
                {
                    if (vrtx1 == vrtx2)
                        continue;
                    Ray rayForCheck = Ray.GetRay(vrtx1, vrtx2, ParallelTo.None);
                    foreach (Triangle triangle in TrianglesOfFigure)
                    {
                        if (triangle.IsCrossTheTriangle(rayForCheck))
                            return false;
                    }
                }
            }
            return true;
        }
    }
}