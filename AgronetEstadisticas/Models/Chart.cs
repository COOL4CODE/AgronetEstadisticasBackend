using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AgronetEstadisticas.Models
{
    public class Chart
    {
        public string subtitle { get; set; }
        public List<Series> series { get; set; }
    }
}