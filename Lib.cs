using Amazon;
using Amazon.Rekognition;
using Amazon.Rekognition.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Threading;
namespace ImageAnalyser
{
    public class Program
    {
        static AmazonRekognitionClient _client;

          public MemoryStream Entry(string url)
          {
            MemoryStream stream = new MemoryStream();
            try
            {
                //Replace YOUR-AWS-ACCESS-KEY, YOUR-AWS-SECRET-ACCESS-KEY and RegionEndpoint with your AWS Credentials and Region
                _client = new AmazonRekognitionClient("AKIAJ6IHP3GJDFIV5YFA", "8FL7B+82iJ83nRfvxofKYLBA6XLAoYfvKU/EyamA", RegionEndpoint.USEast1);
                //bool running = true;
                byte[] image;
               
                //while (running)
                //{
                    //Console.Clear();
                    //Console.WriteLine("Type the desired option's number:");
                    //Console.WriteLine("1 - Detect Faces");
                    //Console.WriteLine("2 - Detect Objects and Scenes");
                    //Console.WriteLine("3 - Detect Text");
                    //Console.WriteLine("4 - Start Detecting Objects and Scenes on video");
                    //Console.WriteLine("5 - Get Detected Objects and Scenes on video");
                    //var input = Console.ReadLine();

                    //if (input == "4")
                    //    DetectLabelsVideo();
                    //else if (input == "5")
                    //{
                    //    Console.WriteLine("Please provide JobId");
                    //    GetDetectedLabelsVideo(Console.ReadLine());
                    //}
                    //else
                    //{
                       // Console.WriteLine("Please provide picture's full path");
                        using (var webClient = new WebClient())
                        {
                          image = webClient.DownloadData(url);
                        }
               stream = new MemoryStream(image);
                       // byte[] image = File.ReadAllBytes(picPath);
                        //using (var stream = new MemoryStream(image))
                        //{
                        //    if (int.TryParse(input, out int result))
                        //    {
                        //        switch (result)
                        //        {
                        //            case 1:
                        //                DetectFaces(stream);
                        //                break;
                        //            case 2:
                        //                lb = DetectLabels(stream);
                        //                break;
                        //            case 3:
                        //                DetectText(stream);
                        //                break;
                        //            default:
                        //                Console.WriteLine("Invalid input, Bye");
                        //                break;
                        //        }
                        //    }
                        //    else
                        //        Console.WriteLine("Invalid input, Bye");
                        //}
                    //}
                    //Console.WriteLine();
                    //Console.WriteLine("Run again? Y/N");
                    //var input2 = Console.ReadLine();
                    //running = input2.ToLower() == "y" ? true : false;
                    
                //}
                return stream;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
             return stream;
            }
    
    }
        // For Image analysis
        public List<FaceDetail> DetectFaces(MemoryStream stream)
        {
            var response = _client.DetectFacesAsync(new DetectFacesRequest
            {
                Attributes = { "ALL" },
                Image = new Image { Bytes = stream }
            }).Result;

            Console.WriteLine($"Faces found: {response.FaceDetails.Count}");
            Console.WriteLine();
            int faceCounter = 1;
            foreach (var faceDetail in response.FaceDetails)
            {
                float emotionConfidence = 0;
                string emotionName = string.Empty;
                //Determines dominant emotion
                foreach (var emotion in faceDetail.Emotions)
                {
                    if (emotion.Confidence > emotionConfidence)
                    {
                        emotionConfidence = emotion.Confidence;
                        emotionName = emotion.Type;
                    }
                }

                Console.WriteLine($"Face {faceCounter}:");
                Console.WriteLine($"");
                Console.WriteLine($"Appears to be {faceDetail.Gender.Value}       {faceDetail.Gender.Confidence} %");
                if (faceDetail.Beard.Value)
                    Console.WriteLine($"Appears to have a beard                       {faceDetail.Beard.Confidence} %");
                if (faceDetail.Eyeglasses.Value)
                    Console.WriteLine($"Wearing glasses                               {faceDetail.Eyeglasses.Confidence} %");
                if (faceDetail.Sunglasses.Value)
                    Console.WriteLine($"Wearing Sunglasses                            {faceDetail.Sunglasses.Confidence} %");
                Console.WriteLine($"Age Range                                     {faceDetail.AgeRange.Low} - {faceDetail.AgeRange.High} old");
                Console.WriteLine($"Appears to be {emotionName}                   {emotionConfidence} %");

                Console.WriteLine();
                Console.WriteLine();

                faceCounter++;
            }

            LogResponse(GetIndentedJson(response), "DetectLabels");
      return response.FaceDetails;
        }

        // For Image analysis
        public List<Label> DetectLabels(MemoryStream stream)
        {
            Console.WriteLine("Minimum confidence level? (0 - 100)");
            var minConfidence = 70;//float.Parse(Console.ReadLine());

            Stopwatch watch = new Stopwatch();
            watch.Start();
            var response = _client.DetectLabelsAsync(new DetectLabelsRequest
            {
                MinConfidence = minConfidence,
                MaxLabels = 100,
                Image = new Image
                {
                    Bytes = stream
                }
            }).Result;
            watch.Stop();
            Console.WriteLine($"Elapsed Time: { watch.Elapsed }");
            Console.WriteLine($"Objects and Scenes Found: {response.Labels.Count}:");
            Console.WriteLine();

            foreach (var label in response.Labels)
            {
                Console.WriteLine($"{label.Name}                 {label.Confidence} %");
            }

            LogResponse(GetIndentedJson(response), "DetectLabels");
            return response.Labels;
        }

    // For Image analysis
    public List<TextDetection> DetectText(MemoryStream stream)
    {
      DetectTextRequest detectTextRequest = new DetectTextRequest()
      {
        Image = new Image()
        {
          Bytes = stream
        }
      };

      DetectTextResponse response = _client.DetectTextAsync(detectTextRequest).Result;
      Console.WriteLine($"Texts Found: {response.TextDetections.Count}");
      Console.WriteLine();

      foreach (TextDetection text in response.TextDetections)
      {
        Console.WriteLine("text: " + text.DetectedText);
        Console.WriteLine("Confidence: " + text.Confidence);
        Console.WriteLine("Type: " + text.Type);
        Console.WriteLine();
      }

      Console.WriteLine();
      Console.WriteLine("JSON response:");
      //Console.WriteLine();
      LogResponse(JsonConvert.SerializeObject(response, Formatting.Indented), "DetectText");
      return response.TextDetections;
    }

    // For Video Analysis
    public string DetectLabelsVideo(string bucketName, string fileName)
        {
           // Console.WriteLine("Minimum confidence level? (0 - 100)");

      var minConfidence = 70;// float.Parse(Console.ReadLine());

            //Console.WriteLine("Bucket name:");
            //var bucketName = Console.ReadLine();

            //Console.WriteLine("File name:");
            //var fileName = Console.ReadLine();

            var response = _client.StartLabelDetectionAsync(new StartLabelDetectionRequest
            {
                MinConfidence = minConfidence,
                Video = new Video
                {
                    S3Object = new S3Object
                    {
                        Bucket = bucketName,
                        Name = fileName
                    }
                }
            }).Result;

            Console.WriteLine($"JobId: {response.JobId}");

            LogResponse(GetIndentedJson(response), "DetectLabelsVideo");
      return response.JobId;
        }

        // For Video Analysis
        public List<LabelDetection> GetDetectedLabelsVideo(string jobId)
        {
            var response = _client.GetLabelDetectionAsync(new GetLabelDetectionRequest
            {
                JobId = jobId
            }).Result;

            Console.WriteLine($"Job Status: {response.JobStatus}");
            Console.WriteLine($"Objects and Scenes Found: {response.Labels.Count}");
            Console.WriteLine();
          if(response.JobStatus.Value.ToUpper()== "IN_PROGRESS")
          { Thread.Sleep(10000);
         response = _client.GetLabelDetectionAsync(new GetLabelDetectionRequest
        {
          JobId = jobId
        }).Result;
      }
      
      foreach (var label in response.Labels)
            {
                Console.WriteLine($"{label.Label.Name} at {label.Timestamp}        {label.Label.Confidence} %");
            }

            LogResponse(GetIndentedJson(response), "GetDetectedLabelsVideo");
            return response.Labels;
        }

        
        static void LogResponse(string text, string method)
        {
            string path = $@"..\..\..\..\{method}.txt";

            StreamWriter sw;

            if (!File.Exists(path))
                sw = File.CreateText(path);
            else
                sw = new StreamWriter(path);
            
            using (sw)
            {
                sw.WriteLine(text);
            }
        }

        static string GetIndentedJson(object response)
        {
            return JsonConvert.SerializeObject(response, Formatting.Indented);
        }
    }
}
