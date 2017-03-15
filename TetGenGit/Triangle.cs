using System;
using System.Collections.Generic;

namespace TetGen
{
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
        /// <summary>
        /// Повертаю false якщо пряма не перетинає трикутник 
        /// </summary>
        /// <param name="rayForCrossing">промінь для перевірки чи він перетианає трикутник, у властивість
        /// запипується точка перетину</param>
        /// <returns></returns>
        public bool IsCrossTheTriangle(ref Ray rayForCrossing)
        {
            double[] xCoords = new double[4];
            double[] yCoords = new double[4];
            #region Спочатку перевіряється чи точка належить трикутнику
            switch (rayForCrossing.RayParallel)
            {
                case ParallelTo.X:
                    xCoords[0] = -rayForCrossing.Start.Z;
                    yCoords[0] = rayForCrossing.Start.Y;
                    xCoords[1] = -this[0].Z; xCoords[2] = -this[1].Z; xCoords[3] = -this[2].Z;
                    yCoords[1] = this[0].Y; yCoords[2] = this[1].Y; yCoords[3] = this[2].Y;
                    break;
                case ParallelTo.Y:
                    xCoords[0] = rayForCrossing.Start.X;
                    yCoords[0] = -rayForCrossing.Start.Z;
                    xCoords[1] = this[0].X; xCoords[2] = this[1].X; xCoords[3] = this[2].X;
                    yCoords[1] = -this[0].Z; yCoords[2] = -this[1].Z; yCoords[3] = -this[2].Z;
                    break;
                case ParallelTo.Z:
                    xCoords[0] = rayForCrossing.Start.X;
                    yCoords[0] = rayForCrossing.Start.Y;
                    xCoords[1] = this[0].X; xCoords[2] = this[1].X; xCoords[3] = this[2].X;
                    yCoords[1] = this[0].Y; yCoords[2] = this[2].Y; yCoords[3] = this[2].X;
                    break;
            }
            double a = (xCoords[1] - xCoords[0]) * (yCoords[2] - yCoords[1]) - (xCoords[2] - xCoords[1]) * (yCoords[1] - yCoords[0]);
            double b = (xCoords[2] - xCoords[0]) * (yCoords[3] - yCoords[2]) - (xCoords[3] - xCoords[2]) * (yCoords[2] - yCoords[0]);
            double c = (xCoords[3] - xCoords[0]) * (yCoords[1] - yCoords[3]) - (xCoords[1] - xCoords[3]) * (yCoords[3] - yCoords[0]);
            if ((a >= 0 && b >= 0 && c >= 0) || (a <= 0 && b <= 0 && c <= 0))
            {
                //   Console.WriteLine("Принадлежит треугольнику");
            }
            else
            {
                return false;
            }
            #endregion
            #region Визначаєм точку входження променя в трикутник
            // Робимо щось тіп того: http://www.reshebnik.ru/solutions/9/13/ 
            double m = rayForCrossing.End.X - rayForCrossing.Start.X;
            double n = rayForCrossing.End.Y - rayForCrossing.Start.Y;
            double p = rayForCrossing.End.Z - rayForCrossing.Start.Z;
            // Визанчав рівняння плоскості тута: https://www.youtube.com/watch?v=6FAXjzsZIR4
            double variableX = (this[1].Y - this[0].Y) * (this[2].Z - this[0].Z) - (this[2].Y - this[0].Y) * (this[1].Z - this[0].Z);
            double variableY = -((this[1].X - this[0].X) * (this[2].Z - this[0].Z) - (this[2].X - this[0].X) * (this[1].Z - this[0].Z));
            double variableZ = (this[1].X - this[0].X) * (this[2].Y - this[0].Y) - (this[2].X - this[0].X) * (this[1].Y - this[0].Y);
            double d = variableX * (-this[0].X) - variableY * (-this[0].Y) + variableZ * (-this[0].Z);
            #endregion
        }

        private static Triangle[] DivideCube(Vertex[] vertexesOfCube)
        {
            // отримуємо точку в центрі куба
            Vertex centerVertex;

            double maxLength = 0;
            // Вершини які при з`єднанні утворюють найбільшу діагональ
            Vertex verDiagonal1 = new Vertex();
            Vertex verDiagonal2 = new Vertex();
            for (int i = 0; i < vertexesOfCube.Length; i++)
            {
                for (int j = 0; j < vertexesOfCube.Length; j++)
                {
                    if (i == j)
                        continue;
                    double lengthOfLine = Math.Sqrt((vertexesOfCube[i].X - vertexesOfCube[j].X) * (vertexesOfCube[i].X - vertexesOfCube[j].X)
                        + (vertexesOfCube[i].Y - vertexesOfCube[j].Y) * (vertexesOfCube[i].Y - vertexesOfCube[j].Y)
                        + (vertexesOfCube[i].Z - vertexesOfCube[j].Z) * (vertexesOfCube[i].Z - vertexesOfCube[j].Z));
                    if (lengthOfLine > maxLength)
                    {
                        maxLength = lengthOfLine;
                        verDiagonal1 = vertexesOfCube[i];
                        verDiagonal2 = vertexesOfCube[j];
                    }

                }
            }
            centerVertex = new Vertex((verDiagonal1.X + verDiagonal2.X) / 2, (verDiagonal1.Y + verDiagonal2.Y) / 2, (verDiagonal1.Z + verDiagonal2.Z) / 2);

            return null;
        }
    }

}