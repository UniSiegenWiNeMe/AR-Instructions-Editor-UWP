using System;
using System.Collections.Generic;

namespace ARInstructionsEditor.Core.Models
{
    public class Step
    {
        public int StepNumber { get; set; }
        public List<Item> Items { get; set; }
        public List<MediaFile> MediaFiles { get; set; }
        public String Text { get; set; }
    }
}