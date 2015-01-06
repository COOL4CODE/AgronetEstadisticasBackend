using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AgronetEstadisticas.Models.parametersBinding
{
    public class report508 : report
    {
        public string producto { get; set; }

        public string unidad { get; set; }
        public string region { get; set; }

        public DateTime fecha_inicial { get; set; }

        public DateTime fecha_final { get; set; }
    }
}