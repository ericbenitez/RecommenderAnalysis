using MathNet.Numerics.LinearAlgebra;

namespace CommerceWebApp.Shared
{
    public class MatrixInfo
    {
        public int AmountOfUsers { get; set; }
        public int AmountOfProducts { get; set; }
        public List<string>? Users { get; set; }
        public List<string>? Products { get; set; }
        public Matrix<double>? Matrix { get; set; }
        public Matrix<double>? AdjustedMatrix {get; set;}

        public Dictionary<int, double> UserAverages { get; set; }
    }
}