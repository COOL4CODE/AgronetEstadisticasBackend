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

        public string fecha_inicial { get; set; }

        public string fecha_final { get; set; }

        public int periodo_corto { get; set; }

        public int periodo_largo { get; set; }

        public int numero_periodos_ifr { get; set; }
    }
}