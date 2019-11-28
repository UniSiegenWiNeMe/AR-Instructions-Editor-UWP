using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ARInstructionsEditor.ViewModels
{
    class InstructionViewModel
    {
        public class Instruction
        {
            public List<StepViewModel> Steps { get; set; }
            public String Name { get; set; }
            public DateTime DateCreated { get; set; }
        }
    }
}
