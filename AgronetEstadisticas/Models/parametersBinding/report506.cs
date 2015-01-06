using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AgronetEstadisticas.Models.parametersBinding
{
    public class report506 : report
    {
        public DateTime fecha_inicial { get; set; }

        public DateTime fecha_final { get; set; }

        public string grupo { get; set; }

        public List<string> ciudad { get; set; }
    }
}