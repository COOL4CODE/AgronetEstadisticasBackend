using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AgronetEstadisticas.Models.parametersBinding
{
    public class report701 : report
    {
        public string fecha_inicial { get; set; }
        public string fecha_final { get; set; }
        public string especie { get; set; }
        public List<string> municipio { get; set; }
    }
}