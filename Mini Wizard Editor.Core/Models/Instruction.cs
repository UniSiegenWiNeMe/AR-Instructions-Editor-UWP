using System;
using System.Collections.Generic;
using System.Text;

namespace ARInstructionsEditor.Core.Models
{
    public class Instruction
    {
        public List<Step> Steps { get; set; }
        public String Name { get; set; }
        public DateTime DateCreated { get; set; }
    }
}
