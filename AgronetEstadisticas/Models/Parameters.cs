using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AgronetEstadisticas.Models
{
    public class Parameter
    {
        public string name { get; set; }
        public List<ParameterData> data { get; set; }
    }

    public class ParameterData
    {
        public string name { get; set; }
        public string value { get; set; }

    }
}