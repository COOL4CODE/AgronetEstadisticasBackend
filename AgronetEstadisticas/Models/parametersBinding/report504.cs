using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AgronetEstadisticas.Models.parametersBinding
{
    public class report504 : report
    {
        public string producto { get; set; }

        public List<string> mercado { get; set; }
        public DateTime fecha_inicial { get; set; }

        public DateTime fecha_final { get; set; }
    }
}