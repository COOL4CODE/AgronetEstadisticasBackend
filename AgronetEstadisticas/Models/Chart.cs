using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AgronetEstadisticas.Models
{
    public class Chart
    {
        public string name { get; set; }
        public List<Series> series { get; set; }
    }
}