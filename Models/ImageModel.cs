using System;
using System.Collections.Generic;
using Amazon.Rekognition.Model;
using Newtonsoft.Json;

namespace app.Models
{
  public class ImageModel
  {
    public string Url { get; set; }

    public bool isAnalyse { get; set; }
    public List<Label> Labels { get; set; }
    public List<FaceDetail> FaceDetails { get; set; }
    public List<TextDetection> TextDetections { get; set; }
  }
}
