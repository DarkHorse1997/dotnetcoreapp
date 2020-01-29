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
            ImageAnalyser.Program ps = new ImageAnalyser.Program();
            stream = ps.Entry(model.Url);
            List<FaceDetail> FaceDetails = ps.DetectFaces(stream);
            model.FaceDetails = FaceDetails;
            List<Label> Labels = ps.DetectLabels(stream);
            model.Labels = Labels;
            List<TextDetection> TextDetections = ps.DetectText(stream);
            model.TextDetections = TextDetections;
            return View("ImageAnalyzer", model);
          }
          catch (Exception ex)
          {
        return View("~/Views/Shared/Error.cshtml");
      }
          
        }

      [HttpPost]
      public ActionResult DetectImage(ImageModel model, string submitButton)
      {
       
        if (ModelState.IsValid)
        {
          //TODO: SubscribeUser(model.Email);
          //  model.isAnalyse = true;
        }
      ImageAnalyser.Program ps = new ImageAnalyser.Program();
      stream = ps.Entry(model.Url);
      switch (submitButton)
      {
        case "Detect Faces":
         
          List<FaceDetail> FaceDetails = ps.DetectFaces(stream);
          model.FaceDetails = FaceDetails;
          model.Labels = null;
          model.TextDetections = null;
          break;
        case "Detect Objects and Scenes":
          ViewBag.Message = "The operation was cancelled!";
          List<Label> Labels = ps.DetectLabels(stream);
          model.Labels = Labels;
          model.FaceDetails = null;
          model.TextDetections = null;
          break;
        case "Detect Text":
          ViewBag.Message = "Customer saved successfully!";
          List<TextDetection> TextDetections = ps.DetectText(stream);
          model.TextDetections = TextDetections;
          model.FaceDetails = null;
          model.Labels = null;
          break;
      }
     
        
       
        return View("DetectImage", model);
      }
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
