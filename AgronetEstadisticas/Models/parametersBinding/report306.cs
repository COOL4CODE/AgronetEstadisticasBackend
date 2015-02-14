using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AgronetEstadisticas.Models.parametersBinding
{
    public class report306 : report
    {
        public String fecha_inicial { get; set; }

        public String fecha_final { get; set; }
        public List<string> ciudad { get; set; }

        public string producto { get; set; }
    }
}