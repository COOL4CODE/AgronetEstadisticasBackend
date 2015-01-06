using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AgronetEstadisticas.Models.parametersBinding
{
    public class report701 : report
    {
        public DateTime fecha_inicial { get; set; }
        public DateTime fecha_final { get; set; }
        public string especie { get; set; }
        public List<string> municipio { get; set; }
    }
}