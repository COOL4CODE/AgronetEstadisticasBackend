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
using Microsoft.AnalysisServices.AdomdClient;

using AgronetEstadisticas.Models.parametersBinding;
using AgronetEstadisticas.Adapter;
using AgronetEstadisticas.Models;

namespace AgronetEstadisticas.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "post")]
    public class C6Controller : ApiController
    {
        [Route("api/Report/602")]
        public async Task<IHttpActionResult> postReport602(report602 parameters)
        {
            string sql = @"SELECT NON EMPTY 
--PARAMETRO 1 Año inicial
{[Periodo].[Anho].&[2000]:
-- PARAMETRO 2 Año final
[Periodo].[Anho].&[2005].[Description]}*
{[Measures].[Valor Millones Pesos]} ON 0,
TopCount({[Geografia].[Departamento].[Departamento]},10,[Measures].[Valor Millones Pesos]) ON 1
FROM
[Agronet Credito Web]
WHERE [Intermediario Financiero].[Intermediario Financiero].&[40];";
            var adapter = new SQLAnalysisAdaper();
            var results = adapter.GetTable(sql, "AgronetSQLAnalysisServicesCredito");
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