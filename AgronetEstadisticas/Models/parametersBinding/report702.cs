using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AgronetEstadisticas.Models.parametersBinding
{
    public class report702 : report
    {
        public DateTime fecha_inicial { get; set; }
        public DateTime fecha_final { get; set; }
        public string municipio { get; set; }
        public string tipo_pesca { get; set; }
        public List<string> especie { get; set; }
    }
}