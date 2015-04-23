using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AgronetEstadisticas.Models.parametersBinding
{
    public class report808 : report
    {
        public string nivel { get; set; }

        public string grupo { get; set; }
        public string tipo_variacion { get; set; }
        public string fecha_inicial { get; set; }

        public string fecha_final { get; set; }
    }
}