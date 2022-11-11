using MathNet.Numerics.LinearAlgebra;

namespace CommerceWebApp.Server.Services
{
    public class RecommenderService
    {
        public readonly static int NEIGHBOURHOOD_SIZE = 2;

        // Calculates Prediction
        public static double CalculatePrediction(Matrix<double> matrix, int user, int product)
        {
            // Get the user's ratings
            var userRatings = matrix.Row(user);

            //Gets the user's average
            double userAverage = matrix.Row(user).Where(x => x > -1).ToList().Average();

            double numerator = 0;
            double denominator = 0;

            Dictionary<int, double> similarities = new();
            for (int i = 0; i < matrix.RowCount; i++)
            {
                //Skips current user
                if (i == user) continue;

                //Gets similarity between users
                double similarity = CalculateSimilarity(matrix, user, i);
                similarities.Add(i, similarity);
            }
            
            Dictionary<int, double> neighbours = similarities.OrderByDescending(x => x.Value).Take(NEIGHBOURHOOD_SIZE).ToDictionary(x => x.Key, x => x.Value);
            foreach (KeyValuePair<int, double> neighbour in neighbours)
            {
                double user2Average = matrix.Row(neighbour.Key).Where(x => x > -1).Average();

                //Gets similarity between users
                numerator += (neighbour.Value * (matrix[neighbour.Key, product] - user2Average));
                denominator += neighbour.Value;
            }

            // Calculate the prediction
            double prediction = userAverage + (numerator / denominator);

            return prediction;
        }

        // Calculates Similarity
        public static double CalculateSimilarity(Matrix<double> matrix, int user1, int user2)
        {
            Matrix<double> goodMatrix = Matrix<double>.Build.Dense(matrix.RowCount, matrix.ColumnCount);
            matrix.CopyTo(goodMatrix);

            //Remove products (columns) both users have not reviewed
            for (int product = matrix.ColumnCount - 1; product >= 0; product--)
            {
                if (matrix[user1, product] == -1 || matrix[user2, product] == -1)
                {
                    goodMatrix = goodMatrix.RemoveColumn(product);
                }
            }

            // Calculate the ratings with the bad columns removed
            var user1Ratings = goodMatrix.Row(user1);
            var user2Ratings = goodMatrix.Row(user2);

            // Calculate average
            var user1Average = matrix.Row(user1).Where(x => x > -1).ToList().Average();
            var user2Average = matrix.Row(user2).Where(x => x > -1).ToList().Average();

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

        // Create Recomendation Score
        public static Matrix<double> CalculatePredictedUserRatings(Matrix<double> matrix)
        {
            Matrix<double> predictedUserRatings = Matrix<double>.Build.Dense(matrix.RowCount, matrix.ColumnCount);
            for (int user = 0; user < matrix.RowCount; user++)
            {
                for (int product = 0; product < matrix.ColumnCount; product++)
                {
                    if (matrix[user, product] == -1)
                    {
                        predictedUserRatings[user, product] = Math.Round(CalculatePrediction(matrix, user, product), 2);
                    }
                    else
                    {
                        predictedUserRatings[user, product] = matrix[user, product];
                    }
                }
            }

            return predictedUserRatings;
        }
    }
}