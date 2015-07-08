using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AgronetEstadisticas.Models.parametersBinding
{
    public class report114 : report
    {
        public string producto { get; set; }
        public string anio_inicial { get; set; }
        public string anio_final { get; set; }
        public string departamento { get; set; }
        public List<string> municipio { get; set; }
    }
}