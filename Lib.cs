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
using app.Models;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;

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
               
            using (var webClient = new WebClient())
            {
              image = webClient.DownloadData(url);
            }
            stream = new MemoryStream(image);
            return stream;
          }
          catch (Exception ex)
          {
              Console.WriteLine(ex.Message);
            return stream;
          }
    
  }
      // For Image analysis
      public List<FaceDetail> DetectFaces(MemoryStream stream, string target, out string message)
      {
        string outMessage = "";
        var response = _client.DetectFacesAsync(new DetectFacesRequest
        {
            Attributes = { "ALL" },
            Image = new Image { Bytes = stream }
        }).Result;

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
          if (faceDetail.Gender.Value.ToString().ToLower() == target.ToLower())
          {
            outMessage = "The Object '" + target.ToUpper() + "' in your watchlist has been found in live stream with '" + Convert.ToInt32(faceDetail.Gender.Confidence) + "%' confidence.";

          }
          faceCounter++;
          }
        message = outMessage;
        LogResponse(GetIndentedJson(response), "DetectLabels");
        return response.FaceDetails;
      }

      // For Image analysis
      public List<Label> DetectLabels(MemoryStream stream, string target, out string message)
      {
          string outMessage = "";
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
          foreach (var label in response.Labels)
          {
              //Console.WriteLine($"{label.Name}                 {label.Confidence} %");
            if (label.Name.ToLower() == target.ToLower())
            {
              outMessage = "The Object '" + target.ToUpper() + "' in your watchlist has been found in live stream with '" + Convert.ToInt32(label.Confidence) + "%' confidence.";
            }
        
          }
          message = outMessage;
    
          LogResponse(GetIndentedJson(response), "DetectLabels");
          return response.Labels;
      }

    // For Image analysis
    public List<TextDetection> DetectText(MemoryStream stream, string target, out string message)
    {
      string outMessage = "";
      DetectTextRequest detectTextRequest = new DetectTextRequest()
      {
        Image = new Image()
        {
          Bytes = stream
        }
      };

      DetectTextResponse response = _client.DetectTextAsync(detectTextRequest).Result;
      //Console.WriteLine($"Texts Found: {response.TextDetections.Count}");
      //Console.WriteLine();

      foreach (TextDetection text in response.TextDetections)
      {
        //Console.WriteLine("text: " + text.DetectedText);
        //Console.WriteLine("Confidence: " + text.Confidence);
        //Console.WriteLine("Type: " + text.Type);
        //Console.WriteLine();
        if (text.DetectedText.ToLower() == target.ToLower() || text.Type.ToString().ToLower() == target.ToLower())
        {
          outMessage = "The Object '" + target.ToUpper() + "' in your watchlist has been found in live stream with '" + Convert.ToInt32(text.Confidence) + "%'  confidence";
        }

      }
      message = outMessage;
    
      LogResponse(JsonConvert.SerializeObject(response, Formatting.Indented), "DetectText");
      return response.TextDetections;
    }

    // For Video Analysis
    public string DetectLabelsVideo(string bucketName, string fileName)
        {
      _client = new AmazonRekognitionClient("AKIAJ6IHP3GJDFIV5YFA", "8FL7B+82iJ83nRfvxofKYLBA6XLAoYfvKU/EyamA", RegionEndpoint.USEast1);

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
    public List<LabelDetection> GetDetectedLabelsVideo(string jobId, string target, out string message)
    {
      string outMessage = String.Empty;
      var response = _client.GetLabelDetectionAsync(new GetLabelDetectionRequest
      {
          JobId = jobId
      }).Result;

      
      if (response.JobStatus.Value.ToUpper() == "IN_PROGRESS")
      {
        while (response.JobStatus.Value.ToUpper() != "SUCCEEDED")
        {
          Thread.Sleep(1000);
          response = _client.GetLabelDetectionAsync(new GetLabelDetectionRequest
          {
            JobId = jobId
          }).Result;
        }
        if (response.JobStatus.Value.ToUpper() == "SUCCEEDED")
        {
          response = _client.GetLabelDetectionAsync(new GetLabelDetectionRequest
          {
            JobId = jobId
          }).Result;
        }
      }

      foreach (var label in response.Labels)
      {
        //Console.WriteLine($"{label.Label.Name} at {label.Timestamp}        {label.Label.Confidence} %");
        if(label.Label.Name.Contains(target, StringComparison.InvariantCultureIgnoreCase))
        {
          outMessage = outMessage = "The Object '" + target.ToUpper() + "' in your watchlist has been found in live stream with '" + Convert.ToInt32(label.Label.Confidence) + "%' confidence.";
          break;
        }
       } 

      message = (outMessage == "" || outMessage == null)? "The Object '" + target.ToUpper() + "' in your watchlist has not been found in live stream." : outMessage;

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

      public async void SendSMS(string msg)
      {
        AmazonSimpleNotificationServiceClient snsClient = new AmazonSimpleNotificationServiceClient(Amazon.RegionEndpoint.USEast1);
        PublishRequest pubRequest = new PublishRequest();
        pubRequest.Message = msg;
        pubRequest.PhoneNumber = "+919888055959";
        // add optional messageattributes, for example:
        //pubRequest.messageattributes.add("aws.sns.sms.senderid", new messageattributevalue
        //{ stringvalue = "senderid", datatype = "string" });
        PublishResponse pubResponse = await snsClient.PublishAsync(pubRequest);
        Console.WriteLine(pubResponse.MessageId);
      //return msg;

      //var snsClient1 = new AmazonSimpleNotificationServiceClient(Amazon.RegionEndpoint.USEast2);

      // Publish a message to an Amazon SNS topic.
      msg = "The object in your watch list has been found in the live stream with ‘90%’ confidence.";
      var topicArn = "arn:aws:sns:us-east-1:735092621658:NotifyMe";

      PublishRequest publishRequest = new PublishRequest(topicArn, msg);
      PublishResponse publishResponse = await snsClient.PublishAsync(publishRequest);

      // Print the MessageId of the published message.
      Console.WriteLine("MessageId: " + publishResponse.MessageId);
    }
  }
}
