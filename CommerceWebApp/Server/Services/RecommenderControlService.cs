using CommerceWebApp.Shared;

namespace CommerceWebApp.Server.Services
{
    public class RecommenderControlService
    {
        private MatrixService matrixService;

        public RecommenderControlService(MatrixService matrixService)
        {
            this.matrixService = matrixService;

            List<string> fileNames = new List<string>{
                "test",
                "test2",
                "test3",
                "testa",
                "parsed-data-trimmed",
                "assignment2-data",
                //"user-test",
                //"sparse_test",
                //"sparse_test2",
                //"sparse_test3",
                //"sparse_test4",
                //"sparse_test5"
            };

            foreach (string fileName in fileNames)
            {
                matrixService.BuildMatrix(fileName);
            }

            
            RecommendationValidationService verifier = new RecommendationValidationService(matrixService);

            File.WriteAllText("Data/Experiment_Output.txt", String.Empty);

            foreach (string filename in fileNames)
            {
                verifier.ExperimentTime(matrixService.GetMatrix(filename));
            }

            //foreach (string fileName in fileNames) 
            //{
            //    Console.WriteLine($"{fileName}: {verifier.CalculateMeanAbsoluteErrorCosine(matrixService.GetMatrix(fileName))}");
            //}
            //foreach (string fileName in fileNames)
            //{
            //    Console.WriteLine($"{fileName}: {verifier.CalculateMeanAbsoluteErrorPearson(matrixService.GetMatrix(fileName))}");
            //}

            //foreach (string filename in fileNames)
            //{
            //    Console.WriteLine("-----------------------");
            //    Console.WriteLine(filename);
            //    MatrixInfo matrixInfo = matrixService.getMatrix(filename);
            //    IEnumerable<KeyValuePair<int, int>> counts = RecommenderService.CountPaths(matrixInfo, matrixInfo.Users!.IndexOf("User1")).OrderByDescending(x => x.Value);
            //    foreach (KeyValuePair<int, int> entry in counts) 
            //    {
            //        Console.WriteLine($"{matrixInfo.Products![entry.Key]}: ({entry.Value})");
            //    }
            //}
        }

    }
}
