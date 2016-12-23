using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
namespace CIITLAB_UltraSonic2016
{
    [Serializable]
    public class Config
    {
        public string applicationPath { get; set; }
        public bool useWifi { get; set; }
        public ComPortSettings comPortSettings { get; set; }
        public string serverAddress { get; set; }
        public int minDistance { get; set; }
        public int maxDistance { get; set; }
        public int numberOfSamples { get; set; }
        public int samplingTime { get; set; }
        public string logoPath { get; set; }
        public string backgroundColor { get; set; }
        public bool debug { get; set; }
    }
    [Serializable]
    public class ComPortSettings
    {
        public string port { get; set; }
        public int baudRate { get; set; }
    }
}
