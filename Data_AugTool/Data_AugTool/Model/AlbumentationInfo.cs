using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
        public double Value { get; set; } = 0;

        public AlbumentationInfo()
        {
            TypeName = "unknown";
            IsChecked = false;
            ValueMin = 0;
            ValueMax = 0;
            IsUseValueMin = false;
            IsUseValueMax = false;
        }
        public AlbumentationInfo(string typeName, double valueMin, double valueMax, bool isUseValueMin = true, bool isUseValueMax = true)
        {
            TypeName = typeName;
            ValueMin = valueMin;
            ValueMax = valueMax;
            IsUseValueMin = isUseValueMin;
            IsUseValueMax = isUseValueMax;
            IsChecked = false;
        }
        public AlbumentationInfo(string typeName)
        {
            TypeName = typeName;
            ValueMin = 0.0;
            ValueMax = 0.0;
            IsUseValueMin = false;
            IsUseValueMax = false;
            IsChecked = false;
        }
    }
}
