using CommerceWebApp.Shared;
using MathNet.Numerics.LinearAlgebra;


namespace CommerceWebApp.Server.Services
{
    public class RecommendationValidationService
    {

        public static double CalculateMeanAbsoluteError(MatrixInfo matrixInfo)
        {
            Matrix<double> matrix = matrixInfo.Matrix!;
            double numeratorSum = 0;
            int testSetSize = 0;
            for (int user = 0; user < matrix.RowCount; user++)
            {
                double userAverage = matrixInfo.UserAverages![user];
                for (int product = 0; product < matrix.ColumnCount; product++)
                {
                    if (matrix[user, product] != 0)
                    {
                        double rating = matrix[user, product];
                        matrix[user, product] = 0;
                        double prediction = RecommenderService.CalculateCosinePrediction(matrixInfo, user, product);
                        matrix[user, product] = rating;
                        
                        double guess = prediction + userAverage;
                        numeratorSum += Math.Abs(guess - rating);
                        testSetSize++;
                    }
                }
            }

            return numeratorSum / testSetSize;
        }

    }
}
