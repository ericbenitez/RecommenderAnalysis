using MathNet.Numerics.LinearAlgebra;

namespace CommerceWebApp.Shared
{
    public class MatrixInfo
    {
        public int AmountOfUsers { get; set; }
        public int AmountOfProducts { get; set; }
        public string[]? Users { get; set; }
        public string[]? Products { get; set; }
        public Matrix<double>? Matrix { get; set; }
    }
}