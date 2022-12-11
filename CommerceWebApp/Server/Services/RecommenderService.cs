using CommerceWebApp.Shared;
using MathNet.Numerics.LinearAlgebra;
using System.Runtime.Intrinsics.X86;
using System.Diagnostics;

namespace CommerceWebApp.Server.Services
{
    public class RecommenderService
    {
        public static int neighbourhoodSize = 5;
        public static double neighbourhoodThreshold = 0.6;
        public static int neighbourhoodMethod = 1; //1 = take top k, 2 = threshold based, 3 = first k above threshold
        public static bool includeNegatives = false;
        private static Func<KeyValuePair<int, double>, bool> negativeCondition = (x => x.Value > 0);
        private static Func<KeyValuePair<int, double>, bool> thresholdCondition = (x => true);

        public static void SetNeighbourhoodSize(int size) 
        {
            neighbourhoodSize = size;
        }
        public static void SetNeighbourhoodThreshold(double threshold)
        {
            neighbourhoodThreshold = threshold;
            thresholdCondition = (x => neighbourhoodMethod > 1 ? x.Value >= neighbourhoodThreshold : true);
        }

        public static void SetNeighbourhoodMethod(int method)
        {
            neighbourhoodMethod = method;
            thresholdCondition = (x => neighbourhoodMethod > 1 ? x.Value >= neighbourhoodThreshold : true);
    }

        public static void SetIncludeNegatives(bool include)
        {
            includeNegatives = include;
            negativeCondition = (x => includeNegatives ? true : x.Value > 0);
        }

        public static void ResetValues()
        {
            neighbourhoodSize = 5;
            neighbourhoodThreshold = 0.6;
            neighbourhoodMethod = 1;
            includeNegatives = false;
            negativeCondition = negativeCondition = (x => x.Value > 0);
            thresholdCondition = (x => neighbourhoodMethod > 1 ? x.Value >= neighbourhoodThreshold : true);
        }

        // Calculates Prediction
        public static double CalculatePearsonPrediction(MatrixInfo matrixInfo, int user, int product)
        {
            Matrix<double> matrix = matrixInfo.Matrix!;

            //Get the user's ratings 
            var userRatings = matrix.Row(user);

            //Gets the user's average
            var userRow = matrix.Row(user).Where(x => x > 0);
            double userAverage = matrixInfo.UserAverages![user];

            double numerator = 0;
            double denominator = 0;

            Dictionary<int, double> similarities = new();
            int neighbourCount = 0;
            for (int i = 0; i < matrix.RowCount; i++)
            {
                //Skips current user
                if (i == user) continue;

                //if we are using method 3 we are done finding neighbours once we find the right amount
                if (neighbourhoodMethod == 3 && neighbourCount == neighbourhoodSize) break;

                //Gets similarity between users
                if (matrix[i, product] > 0)
                {
                    double similarity = CalculatePearsonSimilarity(matrixInfo, user, i);
                    if (double.IsNaN(similarity)) continue; //Skip if there was nothing in common

                    if ((neighbourhoodMethod == 3 && similarity >= neighbourhoodThreshold) || neighbourhoodMethod != 3)
                    {
                        similarities.Add(i, similarity);
                        neighbourCount++;
                    }
                }
            }

            //Critical step
            //Uses previous settings to determine our neighbours
            var sortNeighbours = similarities.Where(negativeCondition).Where(thresholdCondition).OrderByDescending(x => x.Value);
            Dictionary<int, double> neighbours = sortNeighbours.Take(neighbourhoodMethod == 1 ? neighbourhoodSize : sortNeighbours.Count()).ToDictionary(x => x.Key, x => x.Value);

            foreach (KeyValuePair<int, double> neighbour in neighbours)
            {
                userRow = matrix.Row(neighbour.Key).Where(x => x > 0);
                double user2Average = matrixInfo.UserAverages![neighbour.Key];

                //Gets similarity between users
                numerator += (neighbour.Value * (matrix[neighbour.Key, product] - user2Average));
                denominator += neighbour.Value;
            }

            // Calculate the prediction
            double prediction = userAverage + (numerator / denominator);

            if (double.IsNaN(prediction) || double.IsInfinity(prediction))
            {
                var list = matrix.Row(user).ToList().Where(x => x > 0).ToList();
                double average = list.Count() > 0 ? list.Average() : 1;
                prediction = average;
            }

            prediction = prediction > 5 ? 5 : prediction;
            prediction = prediction < 1 ? 1 : prediction;

            return prediction;
        }

        public static double CalculateCosinePrediction(MatrixInfo matrixInfo, int user, int product)
        {
            Matrix<double> matrix = matrixInfo.AdjustedMatrix!;

            // GET SIMILARITY BETWEEN PRODUCTS
            Dictionary<int, double> similarities = new();
            int neighbourCount = 0;
            for (int i = 0; i < matrix.ColumnCount; i++)
            {
                //Skips current product
                if (i == product) continue;

                //if we are using method 3 we are done finding neighbours once we find the right amount
                if (neighbourhoodMethod == 3 && neighbourCount == neighbourhoodSize) break;

                //Gets similarity between products
                if (!double.IsNegativeInfinity(matrix[user, i]))
                {
                    double similarity = CalculateCosineSimilarity(matrixInfo, product, i);
                    if (double.IsNaN(similarity)) continue;

                    if ((neighbourhoodMethod == 3 && similarity >= neighbourhoodThreshold) || neighbourhoodMethod != 3)
                    {
                        similarities.Add(i, similarity);
                        neighbourCount++;
                    }
                }
            }

            //Critical step
            //Uses previous settings to determine our neighbours
            var sortNeighbours = similarities.Where(negativeCondition).Where(thresholdCondition).OrderByDescending(x => x.Value);
            var value = neighbourhoodMethod == 1 ? neighbourhoodSize : sortNeighbours.Count();
            Dictionary<int, double> neighbours = sortNeighbours.Take(neighbourhoodMethod == 1 ? neighbourhoodSize : sortNeighbours.Count()).ToDictionary(x => x.Key, x => x.Value);

            // CALCULATE PREDICTION
            double numerator = 0;
            double denominator = 0;
            foreach (KeyValuePair<int, double> neighbour in neighbours)
            {
                var ok = matrix[user, neighbour.Key];

                numerator += (neighbour.Value * matrix[user, neighbour.Key]);
                denominator += neighbour.Value;
            }

            double prediction = numerator / denominator;

            if (double.IsNaN(prediction) || double.IsInfinity(prediction)) {
                var list = matrix.Row(user).ToList().Where(x => x > 0).ToList();
                double average = list.Count() > 0 ? list.Average() : 1;
                prediction = average;
            }

            //prediction = prediction > 5 ? 5 : prediction;
            //prediction = prediction < 1 ? 1 : prediction;

            return prediction;
        }

        // Calculates Similarity
        public static double CalculatePearsonSimilarity(MatrixInfo matrixInfo, int user1, int user2)
        {
            List<double> user1Array = matrixInfo.CommonUserItems![user1].ContainsKey(user2) ? matrixInfo.CommonUserItems![user1][user2].Values.ToList() : new();
            List<double> user2Array = matrixInfo.CommonUserItems![user2].ContainsKey(user1) ? matrixInfo.CommonUserItems![user2][user1].Values.ToList() : new();
        
            // Calculate the ratings with the bad columns removed
            var user1Ratings = Vector<double>.Build.DenseOfEnumerable(user1Array);
            var user2Ratings = Vector<double>.Build.DenseOfEnumerable(user2Array); ;

            // Calculate average
            var user1Average = user1Array.Count() > 0 ? user1Array.ToList().Average() : 1;
            var user2Average = user2Array.Count() > 0 ? user2Array.ToList().Average() : 1;

            // Substract average from ratings
            var user1Adjusted = user1Ratings.Subtract(user1Average);
            var user2Adjusted = user2Ratings.Subtract(user2Average);

            // Dot product of numerator
            var dotProduct = user1Adjusted.DotProduct(user2Adjusted);

            // Calculate the two sections of the denominator
            var denominator1 = user1Adjusted.L2Norm();
            var denominator2 = user2Adjusted.L2Norm();

            return dotProduct / (denominator1 * denominator2);
        }

        public static double CalculateCosineSimilarity(MatrixInfo matrixInfo, int product1, int product2)
        {
            Matrix<double> matrix = matrixInfo.AdjustedMatrix!;

            List<double> product1Array = new List<double>();
            List<double> product2Array = new List<double>();

            for (int user = 0; user < matrix.RowCount; user++)
            {
                var product1Value = matrix[user, product1];
                var product2Value = matrix[user, product2];

                if (!double.IsNegativeInfinity(product1Value) && !double.IsNegativeInfinity(product2Value))
                {
                    product1Array.Add(product1Value);
                    product2Array.Add(product2Value);
                }
            }

            var product1Ratings = Vector<double>.Build.DenseOfEnumerable(product1Array);
            var product2Ratings = Vector<double>.Build.DenseOfEnumerable(product2Array);

            var dotProduct = product1Ratings.DotProduct(product2Ratings);

            var denominator1 = product1Ratings.L2Norm();
            var denominator2 = product2Ratings.L2Norm();

            return dotProduct / (denominator1 * denominator2);
        }

        // Create Recomendation Score
        public static Matrix<double> CalculatePredictedUserRatings(MatrixInfo matrixInfo, bool isPearsonPrediction)
        {
            Matrix<double> matrix = matrixInfo.Matrix!;
            Matrix<double> predictedUserRatings = Matrix<double>.Build.Dense(matrix.RowCount, matrix.ColumnCount);

            for (int user = 0; user < matrix.RowCount; user++)
            {
                Vector<double> currentRow = matrix.Row(user);
                for (int product = 0; product < matrix.ColumnCount; product++)
                {
                    if (isPearsonPrediction && matrix[user, product] == 0)
                    {
                        predictedUserRatings[user, product] = Math.Round(CalculatePearsonPrediction(matrixInfo, user, product), 2);
                    }
                    else if (!isPearsonPrediction && matrix[user, product] == 0)
                    {
                        predictedUserRatings[user, product] = Math.Round(CalculateCosinePrediction(matrixInfo, user, product), 2);
                    }
                    else
                    {
                        predictedUserRatings[user, product] = matrix[user, product];
                    }
                }
            }
            
            return predictedUserRatings;
        }

        public static Matrix<double> CalculatePredictedCosineRatingComplete(MatrixInfo matrixInfo)
        {
            Matrix<double> matrix = matrixInfo.Matrix!;
            Matrix<double> adjustedMatrix = CalculatePredictedUserRatings(matrixInfo, false);

            Matrix<double> goodMatrix = Matrix<double>.Build.Dense(matrix.RowCount, matrix.ColumnCount);
            matrix.CopyTo(goodMatrix);

            for (int user = 0; user < matrix.RowCount; user++)
            {
                double userAverage = matrixInfo.UserAverages![user];
                for (int product = 0; product < matrix.ColumnCount; product++)
                {
                    if (matrix[user, product] == 0)
                    {
                        goodMatrix[user, product] = adjustedMatrix[user, product] + userAverage;
                    }
                }
            }

            return goodMatrix;
        }

        public static Dictionary<int, int> CountPaths(MatrixInfo matrixInfo, int user)
        {
            Dictionary<int, int> productCount = new Dictionary<int, int>();
            int PATH_LENGTH = 3;

            void recurseUser(int currentUser, int currentItem, int pathLength)
            {
                if (currentItem == matrixInfo.Matrix!.ColumnCount)
                {
                    return;
                }

                if (matrixInfo.Matrix![currentUser, currentItem] == 1)
                {
                    recurseItem(0, currentItem, pathLength + 1);
                }

                recurseUser(currentUser, currentItem + 1, pathLength);
            }

            void recurseItem(int currentUser, int currentItem, int pathLength) 
            {
                if (pathLength == PATH_LENGTH)
                {
                    if (productCount.ContainsKey(currentItem))
                    {
                        productCount[currentItem]++;
                    }
                    else if (matrixInfo.Matrix[user, currentItem] != 1)
                    {
                        productCount[currentItem] = 1;
                    }
                    return;
                }
                else if (currentUser == matrixInfo.Matrix.RowCount)
                {
                    return;
                }
                else if (currentUser != user &&  matrixInfo.Matrix[currentUser, currentItem] == 1) 
                {
                    recurseUser(currentUser, 0, pathLength + 1);
                }

                recurseItem(currentUser + 1, currentItem, pathLength);
            }



            recurseUser(user, 0, 0);
            return productCount;
        }
    }
}