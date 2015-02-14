using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AgronetEstadisticas.Models.parametersBinding
{
    public class report305 : report
    {
        public String fecha_inicial { get; set; }

        public String fecha_final { get; set; }

        public List<string> departamento { get; set; }

        public string tipo_producto { get; set; }
    }
}