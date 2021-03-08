using System;
using System.Collections.Generic;
using System.Text;

namespace RotaryWheel.Models
{
    public class RotaryModel
    {
        public List<RotarySector> Sectors { get; set; }

    }
    public class RotarySector
    {
        public int Price { get; set; }
        public string Color { get; set; }
    }
}
