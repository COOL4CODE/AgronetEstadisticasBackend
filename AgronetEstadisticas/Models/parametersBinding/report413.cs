﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AgronetEstadisticas.Models.parametersBinding
{
    public class report413 : report
    {
        public string anio_inicial { get; set; }

        public string anio_final { get; set; }
        public string pais { get; set; }

        public List<string> producto { get; set; }
    }
}