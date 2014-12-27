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

using AgronetEstadisticas.Models.parametersBinding;
using AgronetEstadisticas.Adapter;
using AgronetEstadisticas.Models;

namespace AgronetEstadisticas.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "post")]
    public class C4Controller : ApiController
    {

        [Route("api/Report/401")]
        public async Task<IHttpActionResult> postReport401(report401 parameters)
        {
            string sql = @"SELECT
                            NON EMPTY {[Measures].[Ton Netas Expo],[Measures].[Valor Expo Miles FOB Dol]} ON 0,
                            NON EMPTY {[Pais].[Pais].[Pais]}
                            *
                            {[Producto].[Producto General].&[Café]}
                            *
                            {
                            [Producto].[Cadena-Partida10].[Descripcion Partida10 Dig Union].&[0901110000 - Café sin tostar, sin descafeinar.],
                            [Producto].[Cadena-Partida10].[Descripcion Partida10 Dig Union].&[0901111000 - Café sin tostar, sin descafeinar, para la siembra.],
                            [Producto].[Cadena-Partida10].[Descripcion Partida10 Dig Union].&[0901119000 - Los demás cafés sin tostar, sin descafeinar.],
                            [Producto].[Cadena-Partida10].[Descripcion Partida10 Dig Union].&[0901120000 - Café sin tostar, descafeinado.]
                            }
                            *
                            {[Periodo].[anho].&[1.991E3]:[Periodo].[anho].&[1.994E3]}
                            *
                            {[Periodo].[Mes].[Mes]}
                            ON 1
                            FROM
                            [Agronet Comercio];";
            var adapter = new SQLAnalysisAdaper();
            var results = adapter.GetTable(sql, "AgronetSQLAnalysisServicesComercio");
            Object returnData = null;

            if (results != null)
            {
                if (parameters.tipo == "parametro")
                {

                }
                else if (parameters.tipo == "grafico")
                {
 
                }
                else if (parameters.tipo == "tabla")
                {
                    returnData = (Table)results;
                }
            }
            else
            {
                return NotFound();
            }

            return Ok(returnData);
        }
    }
}
