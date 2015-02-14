using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Web.Http.Description;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Data;
using MdxClient;

using AgronetEstadisticas.Models.parametersBinding;
using AgronetEstadisticas.Adapter;
using AgronetEstadisticas.Models;

namespace AgronetEstadisticas.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "post")]
    public class C5Controller : ApiController
    {
        [Route("api/Report/501")]
        public IHttpActionResult postReport501(report501 parameters)
        {
            Object returnData = null;
            SQLAdapter adapter = new SQLAdapter();
            switch (parameters.tipo)
            {
                case "parametro":
                    switch (parameters.id)
                    {
                        case 1:
                            break;
                        case 2:
                            break;
                        case 3:
                            break;
                    }
                    break;
                case "grafico":
                    switch (parameters.id)
                    {
                        case 1:
                            break;
                        case 2:
                            break;
                        case 3:
                            break;
                    }
                    break;
                case "tabla":
                    switch (parameters.id)
                    {
                        case 1:
                            break;
                        case 2:
                            break;
                        case 3:
                            break;
                    }
                    break;
            }

            if (returnData == null)
            {
                return NotFound();
            }

            return Ok(returnData);
        }

        [Route("api/Report/502")]
        public IHttpActionResult postReport502(report502 parameters)
        {
            Object returnData = null;
            SQLAdapter adapter = new SQLAdapter();
            switch (parameters.tipo)
            {
                case "parametro":
                    switch (parameters.id)
                    {
                        case 1:
                            break;
                        case 2:
                            break;
                        case 3:
                            break;
                    }
                    break;
                case "grafico":
                    switch (parameters.id)
                    {
                        case 1:
                            break;
                        case 2:
                            break;
                        case 3:
                            break;
                    }
                    break;
                case "tabla":
                    switch (parameters.id)
                    {
                        case 1:
                            break;
                        case 2:
                            break;
                        case 3:
                            break;
                    }
                    break;
            }

            if (returnData == null)
            {
                return NotFound();
            }

            return Ok(returnData);
        }

        [Route("api/Report/503")]
        public IHttpActionResult postReport503(report503 parameters)
        {
            Object returnData = null;
            SQLAdapter adapter = new SQLAdapter();
            switch (parameters.tipo)
            {
                case "parametro":
                    switch (parameters.id)
                    {
                        case 1:
                            break;
                        case 2:
                            break;
                        case 3:
                            break;
                    }
                    break;
                case "grafico":
                    switch (parameters.id)
                    {
                        case 1:
                            break;
                        case 2:
                            break;
                        case 3:
                            break;
                    }
                    break;
                case "tabla":
                    switch (parameters.id)
                    {
                        case 1:
                            break;
                        case 2:
                            break;
                        case 3:
                            break;
                    }
                    break;
            }

            if (returnData == null)
            {
                return NotFound();
            }

            return Ok(returnData);
        }

        [Route("api/Report/504")]
        public IHttpActionResult postReport504(report504 parameters)
        {
            Object returnData = null;
            SQLAdapter adapter = new SQLAdapter();
            switch (parameters.tipo)
            {
                case "parametro":
                    switch (parameters.id)
                    {
                        case 1:
                            break;
                        case 2:
                            break;
                        case 3:
                            break;
                    }
                    break;
                case "grafico":
                    switch (parameters.id)
                    {
                        case 1:
                            break;
                        case 2:
                            break;
                        case 3:
                            break;
                    }
                    break;
                case "tabla":
                    switch (parameters.id)
                    {
                        case 1:
                            break;
                        case 2:
                            break;
                        case 3:
                            break;
                    }
                    break;
            }

            if (returnData == null)
            {
                return NotFound();
            }

            return Ok(returnData);
        }

        [Route("api/Report/505")]
        public IHttpActionResult postReport505(report505 parameters)
        {
            Object returnData = null;
            SQLAdapter adapter = new SQLAdapter();
            switch (parameters.tipo)
            {
                case "parametro":
                    switch (parameters.id)
                    {
                        case 1:
                            String sql1 = @"SELECT AgronetSIPSA.dbo.SipsaDiario.codigoProducto_SipsaDiario as codigo, AgronetSIPSA.dbo.Sipsa_ProductosDiarios.nombreProducto_ProductosDiarios as descripcion
                                            FROM   AgronetSIPSA.dbo.Sipsa_ProductosDiarios 
                                            INNER JOIN AgronetSIPSA.dbo.SipsaDiario ON AgronetSIPSA.dbo.Sipsa_ProductosDiarios.codigoProducto_ProductosDiarios = AgronetSIPSA.dbo.SipsaDiario.codigoProducto_SipsaDiario
                                            GROUP BY AgronetSIPSA.dbo.Sipsa_ProductosDiarios.nombreProducto_ProductosDiarios, AgronetSIPSA.dbo.SipsaDiario.codigoProducto_SipsaDiario
                                            ORDER BY AgronetSIPSA.dbo.Sipsa_ProductosDiarios.nombreProducto_ProductosDiarios;";
                            DataTable data = adapter.GetDatatable(sql1);
                            Parameter param = new Parameter { name = "productos", data = new List<ParameterData>() };
                            foreach (var d in (from p in data.AsEnumerable() select p))
                            {
                                ParameterData parameter = new ParameterData { name = Convert.ToString(d["descripcion"]), value = Convert.ToString(d["codigo"]) };
                                param.data.Add(parameter);
                            }

                            returnData = (Parameter)param;
                            break;
                    }
                    break;
                case "grafico":
                    switch (parameters.id)
                    {
                        case 1:
                            String sql1 = @" SELECT Sipsa_ProductosDiarios.nombreProducto_ProductosDiarios, 
                                             VW_PrecioPequeñosProductores.fecha, 
                                             VW_PrecioPequeñosProductores.nombreMercado_MercadosDia, 
                                             VW_PrecioPequeñosProductores.PrecioSemanaAnt, 
                                             VW_PrecioPequeñosProductores.PrecioSemanaAct, 
                                             VW_PrecioPequeñosProductores.Variacion, 
                                             VW_PrecioPequeñosProductores.codigoProducto_SipsaDiario, 
                                             VW_PrecioPequeñosProductores.unidad_SipsaDiario, 
                                             Sipsa_MercadosDiarios.nombreMercado_MercadosDia
                                             FROM   (AgronetSIPSA.dbo.VW_PrecioPequeñosProductores VW_PrecioPequeñosProductores 
                                             INNER JOIN AgronetSIPSA.dbo.Sipsa_MercadosDiarios Sipsa_MercadosDiarios ON VW_PrecioPequeñosProductores.codMercado_MercadosDia=Sipsa_MercadosDiarios.codMercado_MercadosDia) 
                                             INNER JOIN AgronetSIPSA.dbo.Sipsa_ProductosDiarios Sipsa_ProductosDiarios ON VW_PrecioPequeñosProductores.nombreProducto_ProductosDiarios=Sipsa_ProductosDiarios.nombreProducto_ProductosDiarios
                                             WHERE Sipsa_ProductosDiarios.codigoProducto_ProductosDiarios = 90
                                             ORDER BY VW_PrecioPequeñosProductores.nombreMercado_MercadosDia";

                            Chart chart1 = new Chart { subtitle = "", series = new List<Series>() };

                            Series serie1 = new Series { name = "Producción", data = new List<Data>() };
                            Series serie2 = new Series { name = "Área", data = new List<Data>() };

                            chart1.series.Add(serie1);
                            chart1.series.Add(serie2);
                        
                            foreach (var d1 in (from d in result.AsEnumerable()
                                                select d))
                            {
                                Data data1 = new Data { name = Convert.ToString(d1["anho_eva"]), y = Convert.ToDouble(d1["produccion_eva"]) };
                                Data data2 = new Data { name = Convert.ToString(d1["anho_eva"]), y = Convert.ToDouble(d1["area_eva"]) };

                                serie1.data.Add(data1);
                                serie2.data.Add(data2);
                            }

                            returnData = (Chart)chart1;


                            break;
                    }
                    break;
                case "tabla":
                    switch (parameters.id)
                    {
                        case 1:

                            break;
                    }
                    break;
            }

            if (returnData == null)
            {
                return NotFound();
            }

            return Ok(returnData);
        }

        [Route("api/Report/506")]
        public IHttpActionResult postReport506(report506 parameters)
        {
            Object returnData = null;
            SQLAdapter adapter = new SQLAdapter();
            switch (parameters.tipo)
            {
                case "parametro":
                    switch (parameters.id)
                    {
                        case 1:
                            break;
                        case 2:
                            break;
                        case 3:
                            break;
                    }
                    break;
                case "grafico":
                    switch (parameters.id)
                    {
                        case 1:
                            break;
                        case 2:
                            break;
                        case 3:
                            break;
                    }
                    break;
                case "tabla":
                    switch (parameters.id)
                    {
                        case 1:
                            break;
                        case 2:
                            break;
                        case 3:
                            break;
                    }
                    break;
            }

            if (returnData == null)
            {
                return NotFound();
            }

            return Ok(returnData);
        }

        [Route("api/Report/507")]
        public IHttpActionResult postReport507(report507 parameters)
        {
            Object returnData = null;
            SQLAdapter adapter = new SQLAdapter();
            switch (parameters.tipo)
            {
                case "parametro":
                    switch (parameters.id)
                    {
                        case 1:
                            break;
                        case 2:
                            break;
                        case 3:
                            break;
                    }
                    break;
                case "grafico":
                    switch (parameters.id)
                    {
                        case 1:
                            break;
                        case 2:
                            break;
                        case 3:
                            break;
                    }
                    break;
                case "tabla":
                    switch (parameters.id)
                    {
                        case 1:
                            break;
                        case 2:
                            break;
                        case 3:
                            break;
                    }
                    break;
            }

            if (returnData == null)
            {
                return NotFound();
            }

            return Ok(returnData);
        }

        [Route("api/Report/508")]
        public IHttpActionResult postReport508(report508 parameters)
        {
            Object returnData = null;
            SQLAdapter adapter = new SQLAdapter();
            switch (parameters.tipo)
            {
                case "parametro":
                    switch (parameters.id)
                    {
                        case 1:
                            break;
                        case 2:
                            break;
                        case 3:
                            break;
                    }
                    break;
                case "grafico":
                    switch (parameters.id)
                    {
                        case 1:
                            break;
                        case 2:
                            break;
                        case 3:
                            break;
                    }
                    break;
                case "tabla":
                    switch (parameters.id)
                    {
                        case 1:
                            break;
                        case 2:
                            break;
                        case 3:
                            break;
                    }
                    break;
            }

            if (returnData == null)
            {
                return NotFound();
            }

            return Ok(returnData);
        }

        [Route("api/Report/509")]
        public IHttpActionResult postReport509(report509 parameters)
        {
            Object returnData = null;
            SQLAdapter adapter = new SQLAdapter();
            switch (parameters.tipo)
            {
                case "parametro":
                    switch (parameters.id)
                    {
                        case 1:
                            break;
                        case 2:
                            break;
                        case 3:
                            break;
                    }
                    break;
                case "grafico":
                    switch (parameters.id)
                    {
                        case 1:
                            break;
                        case 2:
                            break;
                        case 3:
                            break;
                    }
                    break;
                case "tabla":
                    switch (parameters.id)
                    {
                        case 1:
                            break;
                        case 2:
                            break;
                        case 3:
                            break;
                    }
                    break;
            }

            if (returnData == null)
            {
                return NotFound();
            }

            return Ok(returnData);
        }

        [Route("api/Report/510")]
        public IHttpActionResult postReport510(report510 parameters)
        {
            Object returnData = null;
            SQLAdapter adapter = new SQLAdapter();
            switch (parameters.tipo)
            {
                case "parametro":
                    switch (parameters.id)
                    {
                        case 1:
                            break;
                        case 2:
                            break;
                        case 3:
                            break;
                    }
                    break;
                case "grafico":
                    switch (parameters.id)
                    {
                        case 1:
                            break;
                        case 2:
                            break;
                        case 3:
                            break;
                    }
                    break;
                case "tabla":
                    switch (parameters.id)
                    {
                        case 1:
                            break;
                        case 2:
                            break;
                        case 3:
                            break;
                    }
                    break;
            }

            if (returnData == null)
            {
                return NotFound();
            }

            return Ok(returnData);
        }

        [Route("api/Report/511")]
        public IHttpActionResult postReport511(report511 parameters)
        {
            Object returnData = null;
            SQLAdapter adapter = new SQLAdapter();
            switch (parameters.tipo)
            {
                case "parametro":
                    switch (parameters.id)
                    {
                        case 1:
                            break;
                        case 2:
                            break;
                        case 3:
                            break;
                    }
                    break;
                case "grafico":
                    switch (parameters.id)
                    {
                        case 1:
                            break;
                        case 2:
                            break;
                        case 3:
                            break;
                    }
                    break;
                case "tabla":
                    switch (parameters.id)
                    {
                        case 1:
                            break;
                        case 2:
                            break;
                        case 3:
                            break;
                    }
                    break;
            }

            if (returnData == null)
            {
                return NotFound();
            }

            return Ok(returnData);
        }

        [Route("api/Report/515")]
        public IHttpActionResult postReport515(report515 parameters)
        {
            Object returnData = null;
            SQLAdapter adapter = new SQLAdapter();
            switch (parameters.tipo)
            {
                case "parametro":
                    switch (parameters.id)
                    {
                        case 1:
                            break;
                        case 2:
                            break;
                        case 3:
                            break;
                    }
                    break;
                case "grafico":
                    switch (parameters.id)
                    {
                        case 1:
                            break;
                        case 2:
                            break;
                        case 3:
                            break;
                    }
                    break;
                case "tabla":
                    switch (parameters.id)
                    {
                        case 1:
                            break;
                        case 2:
                            break;
                        case 3:
                            break;
                    }
                    break;
            }

            if (returnData == null)
            {
                return NotFound();
            }

            return Ok(returnData);
        }
    }
}
