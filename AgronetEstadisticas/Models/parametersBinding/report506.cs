using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AgronetEstadisticas.Models.parametersBinding
{
    public class report506 : report
    {
        public String fecha_inicial { get; set; }

        public String fecha_final { get; set; }

        public string grupo { get; set; }

        public List<string> ciudad { get; set; }
    }
}