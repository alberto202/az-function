using Microsoft.Azure.CognitiveServices.Vision.Face;
using Microsoft.Azure.CognitiveServices.Vision.Face.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Fn_Storage_Example
{
    public class FnStorageExample
    {
        [FunctionName("FnStorageExample")]
        public async Task RunAsync([BlobTrigger("fun-storage/{name}", Connection = "ConnectionStringStorage")] Stream myBlob, string name, ILogger log)
        {
            log.LogInformation($"C# Blob trigger function Processed blob\n Name:{name} \n Size: {myBlob.Length} Bytes");

            // From your Face subscription in the Azure portal, get your subscription key and endpoint.
            string SUBSCRIPTION_KEY = Environment.GetEnvironmentVariable("FACE_SUBSCRIPTION_KEY", EnvironmentVariableTarget.Process);
            string ENDPOINT = Environment.GetEnvironmentVariable("FACE_SUBSCRIPTION_ENDPOINT", EnvironmentVariableTarget.Process);

            // Authenticate.
            IFaceClient client = new FaceClient(new ApiKeyServiceClientCredentials(SUBSCRIPTION_KEY)) { Endpoint = ENDPOINT };

            try
            {

                // Identify - recognize a face(s) in a person group (a person group is created in this example).
                var response = await client.Face.DetectWithStreamAsync(
                                            myBlob,
                                            recognitionModel: RecognitionModel.Recognition02,
                                            detectionModel: DetectionModel.Detection01,
                                            returnFaceAttributes: new List<FaceAttributeType> {
                                                FaceAttributeType.Emotion,
                                                FaceAttributeType.Age,
                                                FaceAttributeType.Gender,
                                                FaceAttributeType.Hair
                                            });

                Console.WriteLine(response[0].FaceAttributes.ToString());

                List<DetectedFace> sufficientQualityFaces = new List<DetectedFace>();
                foreach (DetectedFace detectedFace in response)
                {
                    var faceQualityForRecognition = detectedFace.FaceAttributes.QualityForRecognition;
                    if (faceQualityForRecognition.HasValue && (faceQualityForRecognition.Value >= QualityForRecognition.Medium))
                    {
                        sufficientQualityFaces.Add(detectedFace);
                    }
                }

                Console.WriteLine("End of quickstart.");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
