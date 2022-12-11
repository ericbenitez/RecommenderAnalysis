using CommerceWebApp.Shared;
using MathNet.Numerics.LinearAlgebra;
using System.Diagnostics;
using MicroLibrary;

namespace CommerceWebApp.Server.Services
{
    public class RecommendationValidationService
    {
        private MatrixService matrixService;
        public RecommendationValidationService(MatrixService ms) 
        {
            this.matrixService = ms;
        }
        public double CalculateMeanAbsoluteErrorCosine(MatrixInfo matrixInfo)
        {
            Matrix<double> matrix = matrixInfo.Matrix!;
            double numeratorSum = 0;
            int testSetSize = 0;
            for (int user = 0; user < matrix.RowCount; user++)
            {
                for (int product = 0; product < matrix.ColumnCount; product++)
                {
                    if (matrix[user, product] != 0)
                    {
                        double rating = matrix[user, product];
                        matrix[user, product] = 0;
                        matrixService.RemoveRatingProduct(matrixInfo, user, product);

                        if (product == 0 && user != 0) 
                        {
                            matrixService.CalculateUserAverage(matrixInfo, user-1);
                            matrixService.ComputeAdjustedRow(matrixInfo, matrix, matrixInfo.AdjustedMatrix!, user - 1);
                        }

                        matrixService.CalculateUserAverage(matrixInfo, user);
                        matrixService.ComputeAdjustedRow(matrixInfo, matrix, matrixInfo.AdjustedMatrix!, user);
                        double userAverage = matrixInfo.UserAverages![user];

                        double prediction = RecommenderService.CalculateCosinePrediction(matrixInfo, user, product);
                        matrix[user, product] = rating;
                        matrixService.AddRatingProduct(matrixInfo, user, product);

                        double guess = prediction + userAverage;
                        guess = guess < 1 ? 1 : guess;
                        guess = guess > 5 ? 5 : guess;

                        numeratorSum += Math.Abs(guess - rating);
                        testSetSize++;
                    }
                }
            }

            return numeratorSum / testSetSize;
        }

        public double CalculateMeanAbsoluteErrorPearson(MatrixInfo matrixInfo)
        {
            Matrix<double> matrix = matrixInfo.Matrix!;
            double numeratorSum = 0;
            int testSetSize = 0;
            for (int user = 0; user < matrix.RowCount; user++)
            {
                for (int product = 0; product < matrix.ColumnCount; product++)
                {
                    if (matrix[user, product] != 0)
                    {
                        double rating = matrix[user, product];
                        matrix[user, product] = 0;
                        matrixService.RemoveRatingUser(matrixInfo, user, product);

                        if (product == 0 && user != 0)
                        {
                            matrixService.CalculateUserAverage(matrixInfo, user - 1);
                        }

                        matrixService.CalculateUserAverage(matrixInfo, user);
                        double userAverage = matrixInfo.UserAverages![user];

                        double prediction = RecommenderService.CalculatePearsonPrediction(matrixInfo, user, product);
                        matrix[user, product] = rating;
                        matrixService.AddRatingUser(matrixInfo, user, product);

                        double guess = prediction;
                        guess = guess < 1 ? 1 : guess;
                        guess = guess > 5 ? 5 : guess;

                        numeratorSum += Math.Abs(guess - rating);
                        testSetSize++;
                    }
                }
            }

            return numeratorSum / testSetSize;
        }

        public void ExperimentTime(MatrixInfo matrixInfo) 
        {
            double mae;
            List<int> sizes = new List<int>(){2, 5, 10, 20};
            for (int algorithm = 1; algorithm < 3; algorithm++) 
            {
                foreach (int size in sizes)
                {
                    for (int method = 1; method < 4; method++)
                    {
                        for (int negatives = 1; negatives < 3; negatives++)
                        {
                            if ((method == 2 && size == 2) || method != 2)
                            {
                                RecommenderService.SetNeighbourhoodSize(size);
                                RecommenderService.SetNeighbourhoodMethod(method);

                                bool setNegative = true;
                                if (negatives == 2)
                                {
                                    setNegative = false;
                                }

                                RecommenderService.SetIncludeNegatives(setNegative);

                                MicroStopwatch stopwatch = new MicroStopwatch();
                                long time;
                                if (method > 1)
                                {
                                    for (double threshold = -1; threshold <= 1; threshold += 0.2)
                                    {
                                        RecommenderService.SetNeighbourhoodThreshold(threshold);

                                        stopwatch.Reset();
                                        stopwatch.Start();

                                        //Do the work here
                                        if (algorithm == 1)
                                        {
                                            mae = CalculateMeanAbsoluteErrorCosine(matrixInfo);
                                        }
                                        else
                                        {
                                            mae = CalculateMeanAbsoluteErrorPearson(matrixInfo);
                                        }

                                        stopwatch.Stop();
                                        time = stopwatch.ElapsedMicroseconds;

                                        Console.WriteLine($"Name: {matrixInfo.Name}");
                                        Console.WriteLine($"Negatives: {setNegative}");
                                        Console.WriteLine($"Alg: {algorithm}");
                                        Console.WriteLine($"Size: {size}");
                                        Console.WriteLine($"Method: {method}");
                                        Console.WriteLine($"Threshold: {threshold}");
                                        Console.WriteLine($"Microseconds: {time}");
                                        Console.WriteLine($"MAE: {mae}");
                                        Console.WriteLine("----------------------------");
                                    }
                                }
                                else
                                {
                                    stopwatch.Start();

                                    //Do the work here
                                    if (algorithm == 1)
                                    {
                                        mae = CalculateMeanAbsoluteErrorCosine(matrixInfo);
                                    }
                                    else
                                    {
                                        mae = CalculateMeanAbsoluteErrorPearson(matrixInfo);
                                    }

                                    stopwatch.Stop();
                                    time = stopwatch.ElapsedMicroseconds;

                                    Console.WriteLine($"Name: {matrixInfo.Name}");
                                    Console.WriteLine($"Negatives: {setNegative}");
                                    Console.WriteLine($"Alg: {algorithm}");
                                    Console.WriteLine($"Size: {size}");
                                    Console.WriteLine($"Method: {method}");
                                    Console.WriteLine($"Threshold: 0");
                                    Console.WriteLine($"Microseconds: {time}");
                                    Console.WriteLine($"MAE: {mae}");
                                    Console.WriteLine("----------------------------");

                                }
                                RecommenderService.ResetValues();
                            }
                        }
                    }
                }
            }
        }

    }
}
