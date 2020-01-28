using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Amazon;
using Amazon.Rekognition;
using Amazon.Rekognition.Model;
using Newtonsoft.Json;

namespace app.Models
{

  public class SubscribeModel
  {
    [Required]
    public string Url { get; set; }

    public bool isAnalyse { get; set; }
    public List<Label> Labels { get; set; }
  }
}
