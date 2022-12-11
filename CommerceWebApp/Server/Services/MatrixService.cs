using CommerceWebApp.Shared;
using Newtonsoft.Json;
using MongoDB.Driver;
using MathNet.Numerics.LinearAlgebra;
using System.Runtime.Intrinsics.X86;

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

        public void BuildMatrix(string filename)
        {
            int matrixStart = 3;
            int amountOfUsers;
            int amountOfProducts;
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
                Name = filename,
                AmountOfUsers = amountOfUsers,
                AmountOfProducts = amountOfProducts,
                Users = users,
                Products = products,
                Matrix = matrix,
                UserAverages = new(),
                CommonUserItems = new(),
                ProductToUserMap = new()
            });

            ComputeUserAverages(filename);
            ComputeAdjustedMatrix(filename);
            ComputeAllUserCombinations(filename);
            ComputeProductMap(filename);
        }

        public MatrixInfo GetMatrix(string fileName)
        {
            return matrices[fileName];
        }

        public void ComputeUserAverages(string filename) 
        {
            MatrixInfo m = matrices[filename];
            Matrix<double> matrix = m.Matrix!;
            for (int user = 0; user < matrix!.RowCount; user++) {
                CalculateUserAverage(m, user);
            }
        }

        public void CalculateUserAverage(MatrixInfo m, int user)
        {
            Matrix<double> matrix = m.Matrix!;
            List<double> userRow = matrix.Row(user).Where(x => x > 0).ToList();
            double userAverage = userRow.Count() > 0 ? userRow.Average() : 1;
            if (!m.UserAverages!.ContainsKey(user))
            {
                m.UserAverages!.Add(user, userAverage);
            }
            else
            {
                m.UserAverages![user] = userAverage;
            }
        }

        public void ComputeAdjustedMatrix(string filename)
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

        public void ComputeAdjustedRow(MatrixInfo m, Matrix<double> matrix, Matrix<double> adjustedMatrix, int user) 
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

        public void ComputeAllUserCombinations(string filename)
        {
            MatrixInfo m = matrices[filename];
            Matrix<double> matrix = m.Matrix!;
            for (int user = 0; user < matrix!.RowCount; user++)
            {
                CalculateUserCombinations(m, user);
            }
        }

        public void CalculateUserCombinations(MatrixInfo matrixInfo, int user) 
        {
            Matrix<double> matrix = matrixInfo.Matrix!;

            for (int user2 = 0; user2 < matrix.RowCount; user2++)
            {
                if (user2 == user) continue;

                Dictionary<int, double> userMap = new Dictionary<int, double>();

                //Remove products (columns) both users have not reviewed
                for (int product = matrix.ColumnCount - 1; product >= 0; product--)
                {
                    var user1Value = matrix[user, product];
                    var user2Value = matrix[user2, product];

                    if (user1Value != 0 && user2Value != 0)
                    {
                        userMap.Add(product, user1Value);
                    }
                }

                if (!matrixInfo.CommonUserItems!.ContainsKey(user))
                {
                    matrixInfo.CommonUserItems!.Add(user, new Dictionary<int, Dictionary<int, double>>());
                }
                if (!matrixInfo.CommonUserItems![user].ContainsKey(user2))
                {
                    matrixInfo.CommonUserItems![user].Add(user2, new Dictionary<int, double>());
                }
                
                matrixInfo.CommonUserItems![user][user2] = userMap;
            }

        }

        public void ComputeProductMap(string filename) 
        {
            MatrixInfo matrixInfo = matrices[filename];
            Matrix<double> matrix = matrixInfo.Matrix!;
            for (int product = 0; product < matrix.ColumnCount; product++) 
            {
                HashSet<int> productMap = new();

                for (int user = 0; user < matrix.RowCount; user++) 
                {
                    if (matrix[user, product] > 0)
                    {
                        productMap.Add(user);
                    }
                }
                
                matrixInfo.ProductToUserMap!.Add(product, productMap);

            }
        }

        public void RemoveRatingUser(MatrixInfo matrixInfo, int user, int product)
        {
            var productToUserMapping = matrixInfo.ProductToUserMap!;
            productToUserMapping[product].Remove(user);

            //Iterate over this products hashset
            foreach (int productUser in matrixInfo.ProductToUserMap![product])
            {
                matrixInfo.CommonUserItems![user][productUser].Remove(product);
                if (matrixInfo.CommonUserItems[user][productUser].Count == 0)
                {
                    matrixInfo.CommonUserItems[user].Remove(productUser);
                }

                matrixInfo.CommonUserItems![productUser][user].Remove(product);
                if (matrixInfo.CommonUserItems[productUser][user].Count == 0)
                {
                    matrixInfo.CommonUserItems[productUser].Remove(user);
                }

            }
        }

        public void AddRatingUser(MatrixInfo matrixInfo, int user, int product)
        { 
            var productToUserMapping = matrixInfo.ProductToUserMap!;
            productToUserMapping[product].Add(user);

            foreach (int productUser in matrixInfo.ProductToUserMap![product])
            {
                if (productUser == user) continue;

                if (!matrixInfo.CommonUserItems![user].ContainsKey(productUser))
                {
                    matrixInfo.CommonUserItems[user].Add(productUser, new Dictionary<int, double>());
                }
                matrixInfo.CommonUserItems[user][productUser].Add(product, matrixInfo.Matrix![user, product]);

                if (!matrixInfo.CommonUserItems![productUser].ContainsKey(user)) 
                {
                    matrixInfo.CommonUserItems[productUser].Add(user, new Dictionary<int, double>());
                }
                matrixInfo.CommonUserItems[productUser][user].Add(product, matrixInfo.Matrix![productUser, product]);
            }
        }

        public void ComputeAllProductCombinations(string filename)
        {
            MatrixInfo m = matrices[filename];
            Matrix<double> matrix = m.Matrix!;
            for (int product = 0; product < matrix!.ColumnCount; product++)
            {
                CalculateProductCombinations(m, product);
            }
        }

        public void CalculateProductCombinations(MatrixInfo matrixInfo, int product)
        {
            Matrix<double> matrix = matrixInfo.Matrix!;

            for (int product2 = 0; product2 < matrix.ColumnCount; product2++)
            {
                if (product2 == product) continue;

                Dictionary<int, double> productMap = new Dictionary<int, double>();

                //Remove products (columns) both users have not reviewed
                for (int user = matrix.RowCount - 1; user >= 0; user--)
                {
                    var product1Value = matrix[user, product];
                    var product2Value = matrix[user, product2];

                    if (product1Value != 0 && product2Value != 0)
                    {
                        productMap.Add(user, product1Value);
                    }
                }

                if (!matrixInfo.CommonItemUsers!.ContainsKey(product))
                {
                    matrixInfo.CommonItemUsers!.Add(product, new Dictionary<int, Dictionary<int, double>>());
                }
                if (!matrixInfo.CommonItemUsers![product].ContainsKey(product2))
                {
                    matrixInfo.CommonItemUsers![product].Add(product2, new Dictionary<int, double>());
                }

                matrixInfo.CommonItemUsers![product][product2] = productMap;
            }
        }

        public void RemoveRatingProduct(MatrixInfo matrixInfo, int user, int product)
        {
            var userToProductMapping = matrixInfo.UserToProductMap!;
            userToProductMapping[user].Remove(product);

            //Iterate over this products hashset
            foreach (int productUser in matrixInfo.UserToProductMap![user])
            {
                matrixInfo.CommonItemUsers![product][productUser].Remove(user);
                if (matrixInfo.CommonItemUsers[product][productUser].Count == 0)
                {
                    matrixInfo.CommonItemUsers[product].Remove(productUser);
                }

                matrixInfo.CommonItemUsers![productUser][product].Remove(user);
                if (matrixInfo.CommonItemUsers[productUser][product].Count == 0)
                {
                    matrixInfo.CommonItemUsers[productUser].Remove(product);
                }

            }
        }

        public void AddRatingProduct(MatrixInfo matrixInfo, int user, int product)
        {
            var userToProductMapping = matrixInfo.UserToProductMap!;
            userToProductMapping[product].Add(user);

            foreach (int productUser in matrixInfo.UserToProductMap![user])
            {
                if (productUser == product) continue;

                if (!matrixInfo.CommonItemUsers![product].ContainsKey(productUser))
                {
                    matrixInfo.CommonItemUsers[product].Add(productUser, new Dictionary<int, double>());
                }
                matrixInfo.CommonItemUsers[product][productUser].Add(user, matrixInfo.Matrix![user, product]);

                if (!matrixInfo.CommonItemUsers![productUser].ContainsKey(user))
                {
                    matrixInfo.CommonItemUsers[productUser].Add(product, new Dictionary<int, double>());
                }
                matrixInfo.CommonItemUsers[productUser][product].Add(user, matrixInfo.Matrix![user, productUser]);
            }
        }

    }
}
