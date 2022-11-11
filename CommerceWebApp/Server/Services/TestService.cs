using CommerceWebApp.Shared;
using Newtonsoft.Json;
using MongoDB.Driver;
using MathNet.Numerics.LinearAlgebra;

namespace CommerceWebApp.Server.Services
{
    public class TestService
    {
        private readonly List<string> fileNames;
        private Dictionary<string, MatrixInfo> matrices;

        public TestService()
        {
            this.fileNames = new List<string>{
                "test",
                "test2",
                "test3"
            };

            this.matrices = new Dictionary<string, MatrixInfo>();
            
            foreach (string fileName in fileNames)
            {
                buildMatrix(fileName);
            }
        

            foreach (KeyValuePair<string, MatrixInfo> entry in this.matrices)
            {
                Console.WriteLine(RecommenderService.CalculatePredictedUserRatings(entry.Value.Matrix!));
            }
        }

        public void buildMatrix(string filename)
        {
            string test1Data = System.IO.File.ReadAllText("Data/" + filename + ".txt").Trim();
            string[] rows = test1Data.Split("\n");
            
            string[] firstRow = rows[0].Split(" ");
            int amountOfUsers =  Int32.Parse(firstRow[0]);
            int amountOfProducts = Int32.Parse(firstRow[1]);
            
            string[] users = rows[1].Split(" ");
            string[] products = rows[2].Split(" ");
            

            Matrix<double> matrix = Matrix<double>.Build.Dense(amountOfUsers, amountOfProducts);
            for(int i = 3; i < rows.Length; i++) {
                var ratings = rows[i].Trim().Split(" ").ToList();
                for(int j = 0; j < ratings.Count(); j++) {
                    matrix[i-3, j] = Double.Parse(ratings[j]);
                }
            }

            matrices.Add(filename, new MatrixInfo(){
                AmountOfUsers = amountOfUsers,
                AmountOfProducts = amountOfProducts,
                Users = users,
                Products = products,
                Matrix = matrix
            });
        }

        public MatrixInfo getMatrix(string fileName)
        {
            return matrices[fileName];
        }
    }
}
