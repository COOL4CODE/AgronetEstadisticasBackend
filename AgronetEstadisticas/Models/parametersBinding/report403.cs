using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AgronetEstadisticas.Models.parametersBinding
{
    public class report403 : report
    {
        public string anio_inicial { get; set; }
        public string anio_final { get; set; }
        public string producto { get; set; }
        public List<string> partida { get; set; }
    }
}