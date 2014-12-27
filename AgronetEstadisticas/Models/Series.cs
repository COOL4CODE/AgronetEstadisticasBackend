using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AgronetEstadisticas.Models
{
    public class Series
    {
        public string name { get; set; }
        public List<Data> data { get; set; }

    }
}