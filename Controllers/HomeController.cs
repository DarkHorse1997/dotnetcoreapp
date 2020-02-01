using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using app.Models;
using Amazon.Rekognition.Model;
using Newtonsoft.Json;
using System.IO;


namespace app.Controllers
{
    public class HomeController : Controller
    {
    MemoryStream stream = new MemoryStream();
    public IActionResult Index()
        {
            return View();
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult ImageAnalyzer()
        {
          ViewData["Message"] = "Your Image page.";

          return View();
        }

        public IActionResult Video()
        {
          ViewData["Message"] = "Your Video page.";

          return View();
        }
        public IActionResult AnalyzeVideo(VideoModel model)
        {
          string jobId = string.Empty;
          ViewData["Message"] = "Your Video page.";
          ImageAnalyser.Program ps = new ImageAnalyser.Program();
          //jobId = "b5e4788917c49e768c29a6f9f242f2f1132419bc37fb54cc8f20d677fbeb9d9e";
          jobId = ps.DetectLabelsVideo(model.bucketName, model.fileName);
          
          List<LabelDetection> Labels = ps.GetDetectedLabelsVideo(jobId);
          model.Labels = Labels;
          return View("Video", model);
    }

        [HttpPost]
        public ActionResult AnalyzeImage(ImageModel model)
        {
          try
          {
            string faceMessage = String.Empty;
            string labelMessage = String.Empty;
            string textMessage = String.Empty;
            ImageAnalyser.Program ps = new ImageAnalyser.Program();
            stream = ps.Entry(model.Url);
            List<FaceDetail> FaceDetails = ps.DetectFaces(stream, model.Target, out faceMessage);
            model.FaceDetails = FaceDetails;
            List <Label> Labels = ps.DetectLabels(stream, model.Target, out labelMessage);
            model.Labels = Labels;
            List<TextDetection> TextDetections = ps.DetectText(stream, model.Target, out textMessage);
            model.TextDetections = TextDetections;
            int faceCounter = 1;
            if (FaceDetails != null && FaceDetails.Count > 0)
            {
              foreach (var faceDetail in model.FaceDetails)
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
                model.emotionConfidence = emotionConfidence;
                model.emotionName = emotionName;
                faceCounter++;
              }
            }
            model.Message = faceMessage == "" ? (labelMessage == "" ? (textMessage == "" ? model.Target + " does not found" : textMessage) : labelMessage) : faceMessage;
            FaceDetails = null;
            TextDetections = null;
            Labels = null;
            return View("ImageAnalyzer", model);
          }
          catch (Exception ex)
          {
            return View("~/Views/Shared/Error.cshtml");
          }
          
        }

     [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
