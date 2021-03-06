using System.Collections.Generic;
using Amazon.Rekognition.Model;

namespace app.Models
{
  public class VideoModel
  {
    public string bucketName { get; set; }
    public string fileName { get; set; }
    public string Target { get; set; }
    public string Message { get; set; }
    public List<LabelDetection> Labels { get; set; }
  }
}
