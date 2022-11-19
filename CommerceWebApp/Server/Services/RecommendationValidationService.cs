using CommerceWebApp.Shared;
using MathNet.Numerics.LinearAlgebra;


namespace CommerceWebApp.Server.Services
{
    public class RecommendationValidationService
    {
        private MatrixService matrixService;
        public RecommendationValidationService(MatrixService ms) 
        {
            this.matrixService = ms;
        }
        public double CalculateMeanAbsoluteError(MatrixInfo matrixInfo)
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

                        if (product == 0 && user != 0) {
                            List<double> previousUser = matrix.Row(user-1).Where(x => x > 0).ToList();
                            matrixInfo.UserAverages![user-1] = previousUser.Count() > 0 ? previousUser.Average() : 1;
                            matrixService.ComputeAdjustedRow(matrixInfo, matrix, matrixInfo.AdjustedMatrix!, user - 1);
                        }

                        List<double> userRow = matrix.Row(user).Where(x => x > 0).ToList();
                        matrixInfo.UserAverages![user] = userRow.Count() > 0 ? userRow.Average() : 1;
                        matrixService.ComputeAdjustedRow(matrixInfo, matrix, matrixInfo.AdjustedMatrix!, user);
                        double userAverage = matrixInfo.UserAverages[user];

                        double prediction = RecommenderService.CalculateCosinePrediction(matrixInfo, user, product);
                        matrix[user, product] = rating;
                        
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

    }
}
