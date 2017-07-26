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

        [JsonProperty(PropertyName = "FaceID")]
        public string FaceID { get; set; }

        [JsonProperty(PropertyName = "Happiness")]
        public float Happiness { get; set; }

        [JsonProperty(PropertyName = "Gender")]
        public string Gender { get; set; }
    }
}
