using MathNet.Numerics.LinearAlgebra;

namespace CommerceWebApp.Shared
{
    public class MatrixInfo
    {
        public string? Name { get; set; }
        public int AmountOfUsers { get; set; }
        public int AmountOfProducts { get; set; }
        public List<string>? Users { get; set; }
        public List<string>? Products { get; set; }
        public Matrix<double>? Matrix { get; set; }
        public Matrix<double>? AdjustedMatrix {get; set;}

        public Dictionary<int, double>? UserAverages { get; set; }

        public Dictionary<int, Dictionary<int, Dictionary<int, double>>>? CommonUserItems { get; set; } // user1 -> user2 -> product -> rating
        // userA = 0 4 0 3 0 2
        // userB = 1 0 4 2 0 5
        // userA's common Map with User B = {{3, 3}, {5, 2}}
        // userB's common = {{3, 2}, {5, 5}}

        public Dictionary<int, HashSet<int>>? ProductToUserMap { get; set; } // product -> user -> 1

        public Dictionary<int, Dictionary<int, Dictionary<int, double>>>? CommonItemUsers { get; set; } // product1 -> product2 -> user -> rating
        // productA = 0 4 0 3 0 2
        // productB = 1 0 4 2 0 5
        // productA's common Map with User B = {{3, 3}, {5, 2}}
        // productB's common = {{3, 2}, {5, 5}}

        public Dictionary<int, HashSet<int>>? UserToProductMap { get; set; } // user -> product -> 1
    }
}