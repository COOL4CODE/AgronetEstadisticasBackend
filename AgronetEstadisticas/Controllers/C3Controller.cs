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
    public class C3Controller : ApiController
    {
        [Route("api/Report/301")]
        public async Task<IHttpActionResult> postReport301(report301 parameters)
        {
            string sql = String.Format(@"SELECT variableCalidad.descripcion_VariableCalidad,
                                            region.descripcion_Region,
                                            calidadRegional.fecha_CalidadRegional, 
                                            calidadRegional.valor_CalidadRegional, 
                                            calidadRegional.codigoVariableCalidad_CalidadRegional
                                            FROM (AgronetCadenas.compraLeche.calidadRegional calidadRegional INNER JOIN AgronetCadenas.Leche.region region ON calidadRegional.codigoRegion_CalidadRegional = region.codigo_Region)
                                            INNER JOIN AgronetCadenas.Leche.variableCalidad variableCalidad ON calidadRegional.codigoVariableCalidad_CalidadRegional=variableCalidad.codigo_VariableCalidad
                                            WHERE calidadRegional.fecha_CalidadRegional BETWEEN '{0}' AND '{1}'
                                            ORDER BY  variableCalidad.descripcion_VariableCalidad, 
                                            region.descripcion_Region,
                                            calidadRegional.fecha_CalidadRegional DESC", parameters.anio_inicial.ToString("yyyy-MM-dd"), parameters.anio_final.ToString("yyyy-MM-dd"));

            var adapter = new SQLAdapter();
            var results = adapter.GetResults(sql);
            Object returnData = null;

            if (results != null)
            {
                if (parameters.tipo == "parametro")
                {
                    var queryParameters = from p in results
                                          orderby p["fecha_CalidadRegional"]
                                          select p["fecha_CalidadRegional"];


                    ParameterData param1 = new ParameterData { name = "anio_inicial", value = String.Format("{0:yyyy-MM-dd}", queryParameters.Min()) };
                    ParameterData param2 = new ParameterData { name = "anio_final", value = String.Format("{0:yyyy-MM-dd}", queryParameters.Max()) };
                    Parameter parameter = new Parameter { name = "rango_fechas", data = new List<ParameterData>() };
                    parameter.data.Add(param1);
                    parameter.data.Add(param2);

                    returnData = (Parameter)parameter;
                }
                else if (parameters.tipo == "grafico")
                {
                    var queryCharts = from r in results
                                      group r by r["descripcion_VariableCalidad"] into chartGroup
                                      from seriesGroup in
                                          (from r in chartGroup
                                           group r by r["descripcion_Region"])
                                      group seriesGroup by chartGroup.Key;

                    int i = 1;
                    foreach (var outerGroup in queryCharts)
                    {
                        if (i == parameters.id) {
                            Chart chart = new Chart { name = outerGroup.Key.ToString(), series = new List<Series>() };
                            foreach (var innerGroup in outerGroup)
                            {
                                var serie = new Series { name = innerGroup.Key.ToString(), data = new List<Data>() };
                                foreach (var element in innerGroup)
                                {
                                    var name = Convert.ToDateTime(element["fecha_CalidadRegional"]);
                                    var y = Convert.ToDouble(element["valor_CalidadRegional"]);
                                    var data = new Data { name = String.Format("{0:y}", name), y = y };
                                    serie.data.Add(data);
                                }
                                chart.series.Add(serie);

                            }
                            returnData = (Chart)chart;
                            break;
                        }                        
                        i++;
                    }                   
                } else if (parameters.tipo == "tabla") {
                    /*Table table = new Table { name = "", rows = new List<Dictionary<string,object>>()};
                    table.rows = results;

                    returnData = (Table)table;*/
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
