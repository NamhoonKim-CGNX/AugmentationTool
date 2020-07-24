using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data_AugTool.Model
{
    public class AlbumentationInfo
    {
        public string TypeName { get; set; }
        public bool IsChecked { get; set; }
        public double ValueMin { get; set; }
        public double ValueMax { get; set; }
        public bool IsUseValueMin { get; set; }
        public bool IsUseValueMax { get; set; }
    }
}
