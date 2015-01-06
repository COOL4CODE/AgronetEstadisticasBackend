using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AgronetEstadisticas.Models.parametersBinding
{
    public class report501 : report
    {
        public string producto { get; set; }

        public string mercado { get; set; }

        public DateTime fecha_inicial { get; set; }

        public DateTime fecha_final { get; set; }

        public string periodo_corto { get; set; }

        public string periodo_largo { get; set; }

        public string numero_periodos_ifr { get; set; }
    }
}