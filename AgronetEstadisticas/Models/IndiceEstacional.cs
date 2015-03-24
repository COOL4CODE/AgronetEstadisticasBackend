using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AgronetEstadisticas.Models
{
    public class IndiceEstacional
    {
        public DateTime fechaSemanal { get; set; }
        public Double precioSobrePromedioMovil { get; set; }
    }
}