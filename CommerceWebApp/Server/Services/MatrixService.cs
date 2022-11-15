using CommerceWebApp.Shared;
using Newtonsoft.Json;
using MongoDB.Driver;
using MathNet.Numerics.LinearAlgebra;

namespace CommerceWebApp.Server.Services
{
    public class MatrixService
    {
        public readonly static bool USE_PEARSON_PREDICTION = false;
        
        private Dictionary<string, MatrixInfo> matrices;

        public MatrixService()
        {
            this.matrices = new Dictionary<string, MatrixInfo>();
        }

        public void buildMatrix(string filename)
        {
            int matrixStart = 3;
            int amountOfUsers = 0;
            int amountOfProducts = 0;
            List<string> users = new();
            List<string> products = new();

            string test1Data = System.IO.File.ReadAllText("Data/" + filename + ".txt").Trim();
            string[] rows = test1Data.Split("\n");

            string[] firstRow = rows[0].Split(" ");
            amountOfUsers = Int32.Parse(firstRow[0]);
            amountOfProducts = Int32.Parse(firstRow[1]);

            users = rows[1].Split(" ").ToList();
            products = rows[2].Split(" ").ToList();

            Matrix<double> matrix = Matrix<double>.Build.Dense(amountOfUsers, amountOfProducts);
            for (int i = matrixStart; i < rows.Length; i++)
            {
                List<string> ratings = rows[i].Trim().Split(" ").ToList();
                for (int j = 0; j < ratings.Count(); j++)
                {
                    matrix[i - matrixStart, j] = Double.Parse(ratings[j]);
                }
            }

            matrices.Add(filename, new MatrixInfo()
            {
                AmountOfUsers = amountOfUsers,
                AmountOfProducts = amountOfProducts,
                Users = users,
                Products = products,
                Matrix = matrix,
                UserAverages = new()
            });

            computeUserAverages(filename);
            computeAdjustedMatrix(filename);
        }

        public MatrixInfo getMatrix(string fileName)
        {
            return matrices[fileName];
        }

        public void computeUserAverages(string filename) 
        {
            MatrixInfo m = matrices[filename];
            Matrix<double> matrix = m.Matrix!;
            for (int user = 0; user < matrix!.RowCount; user++) {
                double userAverage = matrix.Row(user).Where(x => x > 0).ToList().Average();
                m.UserAverages!.Add(user, userAverage);
            }
        }

        private void computeAdjustedMatrix(string filename)
        {
            MatrixInfo m = matrices[filename];
            Matrix<double> matrix = m.Matrix!;
            Matrix<double> adjustedMatrix = Matrix<double>.Build.Dense(matrix.RowCount, matrix.ColumnCount);

            for (int user = 0; user < matrix.RowCount; user++)
            {
                double userAverage = m.UserAverages![user];
                for (int product = 0; product < matrix.ColumnCount; product++)
                {
                    if (matrix[user, product] == 0)
                    {
                        adjustedMatrix[user, product] = double.NegativeInfinity;
                    }
                    else
                    {
                        adjustedMatrix[user, product] = matrix[user, product] - userAverage;
                    }
                }
            }

            matrices[filename].AdjustedMatrix = adjustedMatrix;
        }
    }
}
