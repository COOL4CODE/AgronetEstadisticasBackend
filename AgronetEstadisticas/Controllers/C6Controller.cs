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
        [Route("api/Report/601")]
        public IHttpActionResult postReport601(report601 parameters)
        {
            Object returnData = null;
            if (returnData == null)
            {
                return NotFound();
            }

            return Ok(returnData);
        }

        [Route("api/Report/602")]
        public IHttpActionResult postReport602(report602 parameters)
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

        [Route("api/Report/603")]
        public IHttpActionResult postReport603(report603 parameters)
        {
            Object returnData = null;
            if (returnData == null)
            {
                return NotFound();
            }

            return Ok(returnData);
        }

        [Route("api/Report/604")]
        public IHttpActionResult postReport604(report604 parameters)
        {
            Object returnData = null;
            if (returnData == null)
            {
                return NotFound();
            }

            return Ok(returnData);
        }

        [Route("api/Report/605")]
        public IHttpActionResult postReport605(report605 parameters)
        {
            Object returnData = null;
            if (returnData == null)
            {
                return NotFound();
            }

            return Ok(returnData);
        }

        [Route("api/Report/606")]
        public IHttpActionResult postReport606(report606 parameters)
        {
            Object returnData = null;
            if (returnData == null)
            {
                return NotFound();
            }

            return Ok(returnData);
        }

        [Route("api/Report/607")]
        public IHttpActionResult postReport607(report607 parameters)
        {
            Object returnData = null;
            if (returnData == null)
            {
                return NotFound();
            }

            return Ok(returnData);
        }

        [Route("api/Report/608")]
        public IHttpActionResult postReport608(report608 parameters)
        {
            Object returnData = null;
            if (returnData == null)
            {
                return NotFound();
            }

            return Ok(returnData);
        }

        [Route("api/Report/609")]
        public IHttpActionResult postReport609(report609 parameters)
        {
            Object returnData = null;
            if (returnData == null)
            {
                return NotFound();
            }

            return Ok(returnData);
        }
    }
}