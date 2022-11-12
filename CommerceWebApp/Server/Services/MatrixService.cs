using CommerceWebApp.Shared;
using Newtonsoft.Json;
using MongoDB.Driver;
using MathNet.Numerics.LinearAlgebra;

namespace CommerceWebApp.Server.Services
{
    public class MatrixService
    {
        public readonly static bool USE_PEARSON_PREDICTION = false;
        
        private readonly List<string> fileNames;
        private Dictionary<string, MatrixInfo> matrices;

        public MatrixService()
        {
            this.fileNames = new List<string>{
                "test",
                "test2",
                "test3",
                "testa"
            };

            this.matrices = new Dictionary<string, MatrixInfo>();

            foreach (string fileName in fileNames)
            {
                buildMatrix(fileName);
            }

            // foreach (KeyValuePair<string, MatrixInfo> entry in this.matrices)
            // {
            //     Console.WriteLine(entry.Value.AdjustedMatrix!);
            // }

            foreach (KeyValuePair<string, MatrixInfo> entry in this.matrices)
            {
                Console.WriteLine(RecommenderService.CalculatePredictedCosineRatingComplete(entry.Value.Matrix!, entry.Value.AdjustedMatrix!));
            }
        }

        public void buildMatrix(string filename)
        {
            string test1Data = System.IO.File.ReadAllText("Data/" + filename + ".txt").Trim();
            string[] rows = test1Data.Split("\n");

            string[] firstRow = rows[0].Split(" ");
            int amountOfUsers = Int32.Parse(firstRow[0]);
            int amountOfProducts = Int32.Parse(firstRow[1]);

            string[] users = rows[1].Split(" ");
            string[] products = rows[2].Split(" ");


            Matrix<double> matrix = Matrix<double>.Build.Dense(amountOfUsers, amountOfProducts);
            for (int i = 3; i < rows.Length; i++)
            {
                var ratings = rows[i].Trim().Split(" ").ToList();
                for (int j = 0; j < ratings.Count(); j++)
                {
                    matrix[i - 3, j] = Double.Parse(ratings[j]);
                }
            }

            matrices.Add(filename, new MatrixInfo()
            {
                AmountOfUsers = amountOfUsers,
                AmountOfProducts = amountOfProducts,
                Users = users,
                Products = products,
                Matrix = matrix
            });

            computeAdjustedMatrix(filename);
        }

        public MatrixInfo getMatrix(string fileName)
        {
            return matrices[fileName];
        }

        private void computeAdjustedMatrix(String filename)
        {
            Matrix<double> matrix = matrices[filename].Matrix!;
            Matrix<double> adjustedMatrix = Matrix<double>.Build.Dense(matrix.RowCount, matrix.ColumnCount);

            for (int user = 0; user < matrix.RowCount; user++)
            {
                double userAverage = matrix.Row(user).Where(x => x > -1).ToList().Average();
                for (int product = 0; product < matrix.ColumnCount; product++)
                {
                    if (matrix[user, product] == -1)
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
