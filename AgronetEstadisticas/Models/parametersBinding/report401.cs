﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AgronetEstadisticas.Models.parametersBinding
{
    public class report401 : report
    {

        public DateTime fecha_inicial { get; set; }
        public DateTime fecha_final { get; set; }
        public string cadena { get; set; }

        public List<string> partida { get; set; }

    }
}