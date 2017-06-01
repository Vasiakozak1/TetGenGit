using System;
using System.Collections.Generic;
using System.Linq;
namespace TetGen
{
    partial class Program
    {

        /// <summary>
        /// Сканує фігуру променями які паралельні осі x,y, повертає хз покищо
        /// </summary>
        /// <param name="FigureTriangles"></param>
        /// <param name="SizeOfCubes"></param>
        /// <returns></returns>
        private static Triangle[] ScanFigure(Triangle[] FigureTriangles, double SizeOfCubes)
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

                minY = minYOfTriangle < minY ? minYOfTriangle : minY;
                maxY = maxYOfTriangle > maxY ? maxYOfTriangle : maxY;

                minZ = minZOfTriangle < minZ ? minZOfTriangle : minZ;
                maxZ = maxZOfTriangle > maxZ ? maxZOfTriangle : maxZ;
            }
            // Знайшли крайні координати

            m1: int sizeOfFirstDimention = (int)((maxX - minX) / SizeOfCubes);
            int sizeOfSecondDimention = (int)((maxY - minY) / SizeOfCubes);
            int sizeOfThirdDimention = (int)((maxZ - minZ) / SizeOfCubes);
            if (sizeOfFirstDimention == 0 || sizeOfSecondDimention == 0 || sizeOfThirdDimention == 0)
            {
                SizeOfCubes = SizeOfCubes / 2;
                goto m1;
            }
            // Проводимо промені паралельні x
            Ray[,] parallelRaysToX = GetParallelRays(ParallelTo.X, SizeOfCubes, minX, maxX, minZ, maxZ, minY, maxY);
            // Проводимо промені паралельні y
            Ray[,] parallelRaysToY = GetParallelRays(ParallelTo.Y, SizeOfCubes, minY, maxY, minX, maxX, minZ, maxZ);
            // В цих масивах розміщуються тільки промені які перетинають фігуру
            Ray[][] steppedArrOfRaysX;
            Ray[][] steppedArrOfRaysY;

            steppedArrOfRaysX = GetSteppedArrFromRays(parallelRaysToX, FigureTriangles);
            steppedArrOfRaysY = GetSteppedArrFromRays(parallelRaysToY, FigureTriangles);

            // Точки перетину променів 
            Vertex[] vertexesOfCrossing = GetPointsOfCrossing(steppedArrOfRaysX, steppedArrOfRaysY);
            // Промені паралельні осі Z, утворюються з точок перетину променів паралеьних Y, X
            List<Ray> arrOfRaysZ = new List<Ray>();
            vertexesOfCrossing = SortVertexesByCoords(vertexesOfCrossing).ToArray();
            foreach (Vertex tempVertex in vertexesOfCrossing)
            {
                Vertex startVertex = new Vertex(tempVertex.X, tempVertex.Y, minZ);
                Vertex endVertex = new Vertex(tempVertex.X, tempVertex.Y, maxZ);
                Ray tempRay = Ray.GetRay(startVertex, endVertex, ParallelTo.Z);
                foreach (Triangle triangle in FigureTriangles)
                {
                    triangle.IsCrossTheTriangle(ref tempRay);

                }
                if (tempRay.VertexesOfEntry.Count > 0)
                    arrOfRaysZ.Add(tempRay);

            }

            List<Triangle> resultTriangles = new List<Triangle>();
            Parallelepiped[] divideFigureByParpds = Parallelepiped.GetParallelepipedsInsideFigure(vertexesOfCrossing, arrOfRaysZ, steppedArrOfRaysX, steppedArrOfRaysY);
            foreach (Parallelepiped parppd in divideFigureByParpds)
            {
                resultTriangles.AddRange(parppd.DivideParallelepiped());
            }

            return resultTriangles.ToArray();
        }
        /// <summary>
        /// Допоміжний метод, переробляє простий масив із променів у "кривий" масив.
        /// У результуючий масив записуються тільки ті промені, які пересікаються з фігурою
        /// </summary>
        /// <param name="ArrayOfRays"></param>
        /// <param name="FigureTriangles"></param>
        /// <returns></returns>
        private static Ray[][] GetSteppedArrFromRays(Ray[,] ArrayOfRays, Triangle[] FigureTriangles)
        {
            // Цей масив зберігає в собі кількість променів у кожному рядку
            int[] numbersOfRaysInRow = new int[ArrayOfRays.GetLength(0)];
            int numbersCounter = 0;
            // Отримужмо координати точок входу - виходу із тіла
            for (int i = 0; i < ArrayOfRays.GetLength(0); i++)
            {
                for (int j = 0; j < ArrayOfRays.GetLength(1); j++)
                {
                    Ray tempRay = ArrayOfRays[i, j];
                    bool flag = true;
                    foreach (Triangle triangle in FigureTriangles)
                    {
                        if (triangle.IsCrossTheTriangle(ref tempRay))
                        {
                            if (flag)
                            {
                                numbersOfRaysInRow[numbersCounter] += 1;
                                flag = false;
                            }
                        }

                    }
                    ArrayOfRays[i, j] = tempRay;
                }
                numbersCounter++;
            }
            // Щоб не було комірки із нульовим значенням
            int counter = 0;
            foreach (int a in numbersOfRaysInRow)
            {
                if (a == 0)
                    continue;
                counter++;
            }
            int[] tempArr = new int[counter];
            counter = 0;
            foreach (int a in numbersOfRaysInRow)
            {
                if (a == 0)
                    continue;
                tempArr[counter] = a;
                counter++;
            }
            numbersOfRaysInRow = tempArr;

            Ray[][] resultingArrOfRays = new Ray[numbersOfRaysInRow.Length][];
            for (int i = 0; i < resultingArrOfRays.Length; i++)
                resultingArrOfRays[i] = new Ray[numbersOfRaysInRow[i]];

            int SecdDimArrCount = 0;

            // Розмірності вимірів ArrayOfRays масиву 
            int firstDim = ArrayOfRays.GetLength(0);
            int secDim = ArrayOfRays.GetLength(1);
            for (int i = 0; i < firstDim; i++)
            {
                int FirDimArrCount = 0;
                //   int currLen = resultingArrOfRays[i].Length;
                for (int j = 0; j < secDim; j++)
                {
                    if (ArrayOfRays[i, j].VertexesOfEntry == null)
                        continue;
                    if (ArrayOfRays[i, j].VertexesOfEntry.Count > 0)
                    {
                        resultingArrOfRays[SecdDimArrCount][FirDimArrCount] = ArrayOfRays[i, j];
                        FirDimArrCount++;
                    }
                }
                if (FirDimArrCount > 0)
                    SecdDimArrCount++;
            }

            return resultingArrOfRays;
        }
        // Утворює промені парарельні x або y. Промені паралельні осі Z будуть утворюватися іншим чином
        private static Ray[,] GetParallelRays(ParallelTo RayParallelism, double SizeOfCubes, double startValue, double endValue,
            double MinOfFirstDim, double MaxOfFirstDim, double MinOfSecondDim, double MaxOfSecondDim)
        {
            int countOfLines = (int)((MaxOfSecondDim - MinOfSecondDim) / SizeOfCubes);
            int countOfColumns = (int)((MaxOfFirstDim - MinOfFirstDim) / SizeOfCubes);

            countOfColumns++;
            countOfLines++;

            Ray[,] resultingArrayOfRays = new Ray[countOfLines, countOfColumns];

            int secondDimCount = 0;
            int firstDimCount = 0;
            switch (RayParallelism)
            {
                case ParallelTo.X:
                    secondDimCount = 0;
                    for (double y = MinOfSecondDim; y < MaxOfSecondDim; y += SizeOfCubes)
                    {
                        firstDimCount = 0;
                        for (double z = MinOfFirstDim; z < MaxOfFirstDim; z += SizeOfCubes)

                        {

                            Vertex startVertex = new Vertex(startValue, y, z);
                            Vertex endVertex = new Vertex(endValue, y, z);
                            Ray currentRay = Ray.GetRay(startVertex, endVertex, ParallelTo.X);
                            resultingArrayOfRays[secondDimCount, firstDimCount] = currentRay;
                            firstDimCount++;
                        }
                        secondDimCount++;
                    }
                    break;
                case ParallelTo.Y:
                    secondDimCount = 0;
                    for (double z = MinOfSecondDim; z < MaxOfSecondDim; z += SizeOfCubes)
                    {
                        firstDimCount = 0;
                        for (double x = MinOfFirstDim; x < MaxOfFirstDim; x += SizeOfCubes)
                        {
                            Vertex startVertex = new Vertex(x, startValue, z);
                            Vertex endVertex = new Vertex(x, endValue, z);
                            Ray currentRay = Ray.GetRay(startVertex, endVertex, ParallelTo.Y);
                            resultingArrayOfRays[secondDimCount, firstDimCount] = currentRay;
                            firstDimCount++;
                        }
                        secondDimCount++;
                    }
                    break;
                default: throw new Exception("Rays must be only parallel to x or y in this method");
            }
            return resultingArrayOfRays;
        }

        /// <summary>
        /// Отримуємо точки перетину променів які зберігаються в ступінчастих масивах
        /// </summary>
        /// <param name="FirstArrayOfRays"></param>
        /// <param name="SecondArrayOfRays"></param>
        /// <returns></returns>
        private static Vertex[] GetPointsOfCrossing(Ray[][] FirstArrayOfRays, Ray[][] SecondArrayOfRays)
        {
            // Роблю по цьому методу
            // http://mathprofi.ru/zadachi_s_pryamoi_v_prostranstve.html
            // Приклад 14       
            List<Vertex> vertexesOfCrossing = new List<Vertex>();
            for (int a = 0; a < FirstArrayOfRays.Length; a++)
            {
                for (int b = 0; b < FirstArrayOfRays[a].Length; b++)
                {
                    Ray ray = FirstArrayOfRays[a][b];
                    // Параметричне рівняння для 1 - ої прямої
                    double xcountOfT = ray.End.X - ray.Start.X;
                    double ycountOfT = ray.End.Y - ray.Start.Y;
                    double zcountOfT = ray.End.Z - ray.Start.Z;
                    for (int c = 0; c < SecondArrayOfRays.Length; c++)
                    {
                        for (int d = 0; d < SecondArrayOfRays[c].Length; d++)
                        {
                            Ray anotherRay = SecondArrayOfRays[c][d];
                            // Параметричне рівняння для 2 - ої прямої
                            // Мінуси ставлю бо у рівнянні змінні переносяться за знак =
                            double xCountOfS = -(anotherRay.End.X - anotherRay.Start.X);
                            double yCountOfS = -(anotherRay.End.Y - anotherRay.Start.Y);
                            double zCountOfS = -(anotherRay.End.Z - anotherRay.Start.Z);

                            double firstFreeVar = ray.Start.X + (-anotherRay.Start.X);
                            double secondFreeVar = ray.Start.Y + (-anotherRay.Start.Y);
                            double thirdFreeVar = ray.Start.Z + (-anotherRay.Start.Z);

                            #region Не знаю точно чи правильно
                            double valueOfS = 0;
                            double valueOfT = 0;

                            if (xcountOfT != 0 && xCountOfS == 0)
                                valueOfT = -firstFreeVar / xcountOfT;

                            else if (ycountOfT != 0 && yCountOfS == 0)
                                valueOfT = -secondFreeVar / ycountOfT;

                            else if (zcountOfT != 0 && zCountOfS == 0)
                                valueOfT = -thirdFreeVar / zcountOfT;

                            else if (xcountOfT == 0 && xCountOfS != 0)
                            {
                                valueOfS = -firstFreeVar / xCountOfS;
                                if (yCountOfS != 0 && ycountOfT != 0)
                                    valueOfT = -secondFreeVar / (valueOfS * yCountOfS);
                                else
                                    valueOfT = -thirdFreeVar / (valueOfS * zCountOfS);
                            }
                            else if (ycountOfT == 0 && yCountOfS != 0)
                            {
                                valueOfS = -secondFreeVar / yCountOfS;
                                if (xCountOfS != 0 && xcountOfT != 0)
                                    valueOfT = -firstFreeVar / (valueOfS * xCountOfS);
                                else
                                    valueOfT = -thirdFreeVar / (valueOfS * zCountOfS);
                            }
                            else if (zcountOfT == 0 && zCountOfS != 0)
                            {
                                valueOfS = -thirdFreeVar / zCountOfS;
                                if (xCountOfS != 0 && xcountOfT != 0)
                                    valueOfT = -firstFreeVar / (valueOfS * xCountOfS);
                                else
                                    valueOfT = -secondFreeVar / (valueOfS * yCountOfS);
                            }
                            else
                            {
                                valueOfS = ((-xCountOfS) / xcountOfT + yCountOfS) / ((-firstFreeVar) / xcountOfT + secondFreeVar);

                                valueOfT = -(xCountOfS * valueOfS + firstFreeVar);
                            }

                            #endregion

                            double x = xcountOfT * valueOfT + ray.Start.X;
                            double y = ycountOfT * valueOfT + ray.Start.Y;
                            double z = zcountOfT * valueOfT + ray.Start.Z;
                            Vertex result = new Vertex(x, y, z);
                            if (!vertexesOfCrossing.Contains(result))
                                vertexesOfCrossing.Add(result);
                        }
                    }

                }
            }
            List<Vertex> resultVertexes = new List<Vertex>();
            foreach (Vertex tempVrtx in vertexesOfCrossing)
            {
                if (double.IsNaN(tempVrtx.X) || double.IsNaN(tempVrtx.Y) || double.IsNaN(tempVrtx.Z))
                    continue;
                resultVertexes.Add(tempVrtx);
            }
            return resultVertexes.ToArray();
        }


        private static IEnumerable<Vertex> SortVertexesByCoords(Vertex[] VertexesToSort)
        {
            var sortedArr = from vertexes in VertexesToSort
                            orderby vertexes.X ascending
                            orderby vertexes.X ascending
                            orderby vertexes.Z ascending
                            select vertexes;

            return sortedArr.ToArray();
        }
        private static IEnumerable<Ray> SortRaysByCoords(List<Ray> RaysToSort)
        {
            // Маємо масив променів які паралельні до однієї і тієї ж осі
            IOrderedEnumerable<Ray> resultSortedArr;
            switch (RaysToSort[0].RayParallel)
            {
                case ParallelTo.None:
                    resultSortedArr = from rays in RaysToSort
                                      orderby rays.Start.X
                                      orderby rays.Start.Y
                                      orderby rays.Start.Z
                                      select rays;
                    return resultSortedArr.ToArray();
                case ParallelTo.X:
                    resultSortedArr = from rays in RaysToSort
                                      orderby rays.Start.Z
                                      orderby rays.Start.Y
                                      select rays;
                    return resultSortedArr.ToArray();
                case ParallelTo.Y:
                    resultSortedArr = from rays in RaysToSort
                                      orderby rays.Start.Z
                                      orderby rays.Start.X
                                      select rays;
                    return resultSortedArr.ToArray();
                case ParallelTo.Z:
                    resultSortedArr = from rays in RaysToSort
                                      orderby rays.Start.X
                                      orderby rays.Start.Y
                                      select rays;
                    return resultSortedArr.ToArray();
            }
            return null;
        }
    }

}
