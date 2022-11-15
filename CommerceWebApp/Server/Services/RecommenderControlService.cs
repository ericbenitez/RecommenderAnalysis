using CommerceWebApp.Shared;

namespace CommerceWebApp.Server.Services
{
    public class RecommenderControlService
    {
        private MatrixService matrixService;

        public RecommenderControlService(MatrixService matrixService)
        {
            Console.WriteLine("asdas");
            this.matrixService = matrixService;

            string verificationMatrixFilename = "parsed-data-trimmed";
            List<string> fileNames = new List<string>{
                "test",
                "test2",
                "test3",
                "testa",
                //"user-test",
                "parsed-data-trimmed"
            };

            foreach (string fileName in fileNames)
            {
                matrixService.buildMatrix(fileName);
            }

            foreach (string filename in fileNames)
            {
                MatrixInfo matrixInfo = matrixService.getMatrix(filename);
                //Console.WriteLine(RecommenderService.CalculatePredictedCosineRatingComplete(matrixInfo));
                //Console.WriteLine(matrixInfo.AdjustedMatrix!);
            }

            RecommendationValidationService verifier = new RecommendationValidationService(matrixService);
            foreach (string filename in fileNames) {
                Console.WriteLine(verifier.CalculateMeanAbsoluteError(matrixService.getMatrix(filename)));
            }
        }

    }
}
