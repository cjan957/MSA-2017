using System;
using System.Collections.Generic;

namespace Tabs.Model
{
    public class FaceRectangle
    {
        public int top { get; set; }
        public int left { get; set; }
        public int width { get; set; }
        public int height { get; set; }
    }

    public class FaceAttributes
    {
        public double smile { get; set; }
        public string gender { get; set; }
        public double age { get; set; }
    }

    public class EvaluationModel
    {
        public string faceId { get; set; }
        public FaceRectangle faceRectangle { get; set; }
        public FaceAttributes faceAttributes { get; set; }
    }

}