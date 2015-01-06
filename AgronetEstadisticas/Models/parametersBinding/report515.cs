using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AgronetEstadisticas.Models.parametersBinding
{
    public class report515 : report
    {
        public DateTime fecha_inicial { get; set; }

        public DateTime fecha_final { get; set; }
        public string producto { get; set; }
    }
}