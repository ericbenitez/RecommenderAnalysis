using CommerceWebApp.Shared;

namespace CommerceWebApp.Server.Services
{
    public class RecommenderControlService
    {

        public RecommenderControlService()
        {
            List<string> fileNames = new List<string>{
                "test",
                "test2",
                "test3",
                "testa"
            };

            MatrixService matrices = new MatrixService();

            foreach (string fileName in fileNames)
            {
                matrices.buildMatrix(fileName);
            }

            foreach (string filename in fileNames)
            {
                MatrixInfo matrixInfo = matrices.getMatrix(filename);
                Console.WriteLine(RecommenderService.CalculatePredictedCosineRatingComplete(matrixInfo.Matrix!, matrixInfo.AdjustedMatrix!));
            }
        }

    }
}
