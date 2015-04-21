using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AgronetEstadisticas.Models.parametersBinding
{
    public class report306 : report
    {
        public string fecha_inicial { get; set; }

        public string fecha_final { get; set; }
        
        public List<string> ciudad { get; set; }

        public string producto { get; set; }
    }
}