using CommerceWebApp.Shared;
using MathNet.Numerics.LinearAlgebra;
using System.Runtime.Intrinsics.X86;

namespace CommerceWebApp.Server.Services
{
    public class RecommenderService
    {
        public readonly static int NEIGHBOURHOOD_SIZE = 5;

        // Calculates Prediction
        public static double CalculatePearsonPrediction(MatrixInfo matrixInfo, int user, int product)
        {
            Matrix<double> matrix = matrixInfo.Matrix!;

            //Get the user's ratings 
            var userRatings = matrix.Row(user);

            //Gets the user's average
            var userRow = matrix.Row(user).Where(x => x > 0);
            double userAverage = userRow.Count() > 0 ? userRow.ToList().Average() : 1;

            double numerator = 0;
            double denominator = 0;

            Dictionary<int, double> similarities = new();
            for (int i = 0; i < matrix.RowCount; i++)
            {
                //Skips current user
                if (i == user) continue;

                //Gets similarity between users
                double similarity = CalculatePearsonSimilarity(matrixInfo, user, i);
                similarities.Add(i, similarity);
            }

            Dictionary<int, double> neighbours = similarities.Where(x => x.Value > 0).OrderByDescending(x => x.Value).Take(NEIGHBOURHOOD_SIZE).ToDictionary(x => x.Key, x => x.Value);
            foreach (KeyValuePair<int, double> neighbour in neighbours)
            {
                userRow = matrix.Row(neighbour.Key).Where(x => x > 0);
                double user2Average = userRow.Count() > 0 ? userRow.ToList().Average() : 0;

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
            for (int i = 0; i < matrix.ColumnCount; i++)
            {
                //Skips current product
                if (i == product) continue;

                //Gets similarity between products
                if (!double.IsNegativeInfinity(matrix[user, i]))
                {
                    double similarity = CalculateCosineSimilarity(matrixInfo, product, i);
                    similarities.Add(i, similarity);
                }
            }

            // GET NEIGHBOURS
            Dictionary<int, double> neighbours = similarities.Where(x => x.Value > 0).OrderByDescending(x => x.Value).Take(NEIGHBOURHOOD_SIZE).ToDictionary(x => x.Key, x => x.Value);

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
            Matrix<double> matrix = matrixInfo.Matrix!;

            List<double> user1Array = new List<double>();
            List<double> user2Array = new List<double>();

            //Remove products (columns) both users have not reviewed
            for (int product = matrix.ColumnCount - 1; product >= 0; product--)
            {
                var user1Value = matrix[user1, product];
                var user2Value = matrix[user2, product];

                if (user1Value != 0 && user2Value != 0)
                {
                    user1Array.Add(user1Value);
                    user2Array.Add(user2Value);
                }
            }

            // Calculate the ratings with the bad columns removed
            var user1Ratings = Vector<double>.Build.DenseOfEnumerable(user1Array);
            var user2Ratings = Vector<double>.Build.DenseOfEnumerable(user2Array); ;

            // Calculate average
            var user1Average = user1Array.Count() > 0 ? user1Array.ToList().Average() : 1;
            var user2Average = user2Array.Count() > 0 ? user2Array.ToList().Average() : 1; ;

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

            var asda = dotProduct / (denominator1 * denominator2);
            
            return asda; // nan
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