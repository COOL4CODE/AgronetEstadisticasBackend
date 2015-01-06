using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AgronetEstadisticas.Models.parametersBinding
{
    public class report502 : report
    {
        public string producto { get; set; }

        public string mercado1 { get; set; }

        public string mercado2 { get; set; }

        public DateTime fecha_inicial { get; set; }

        public DateTime fecha_final { get; set; }
    }
}