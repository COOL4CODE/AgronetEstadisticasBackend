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

using AgronetEstadisticas.Adapter;
using AgronetEstadisticas.Models;
using AgronetEstadisticas.Models.parametersBinding;

namespace AgronetEstadisticas.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "post")]
    public class C1Controller : ApiController
    {
        [Route("api/Report/101")]
        public async Task<IHttpActionResult> postReport101(report101 parameters)
        {
            //string sql = String.Format(@"SELECT ", parameters.anio_inicial.ToString("yyyy-MM-dd"), parameters.anio_final.ToString("yyyy-MM-dd"));

            string sql = @"SELECT codigoagronetproducto_eva, codigomunicipio_eva, anho_eva, areasembrada_eva, 
                              areacosechada_eva, produccion_eva
                              FROM eva_mpal.evamunicipalanual;";
            var adapter = new PostgresqlAdapter();
            var results = adapter.GetResults(sql);
            Object returnData = null;

            if (results != null)
            {
                if (parameters.tipo == "parametro")
                {

                }
                else if (parameters.tipo == "grafico")
                {
                    var queryCharts = from r in results
                                      select r;

                    Series series = new Series { name = "test", data = new List<Data>() };
                    foreach (var d in queryCharts) {
                        Data data = new Data { name = Convert.ToString(d["codigomunicipio_eva"]), y = Convert.ToDouble(d["areasembrada_eva"]) };
                        series.data.Add(data);
                    }

                    returnData = (Series)series;
                   
                }
                else if (parameters.tipo == "tabla")
                {
                   
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
