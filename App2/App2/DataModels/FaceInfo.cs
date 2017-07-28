using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App2.DataModels
{
    public class FaceInfo
    {
        [JsonProperty(PropertyName = "Id")]
        public string ID { get; set; }

        [JsonProperty(PropertyName = "timestamp")]
        public DateTime timeStamp { get; set; }

        [JsonProperty(PropertyName = "FaceId1")]
        public string FaceId1 { get; set; }

        [JsonProperty(PropertyName = "FaceId2")]
        public string FaceId2 { get; set; }

        [JsonProperty(PropertyName = "Age1")]
        public double Age1 { get; set; }

        [JsonProperty(PropertyName = "Age2")]
        public double Age2 { get; set; }

        //[JsonProperty(PropertyName = "Gender")]
        //public string Gender { get; set; }
    }
}
