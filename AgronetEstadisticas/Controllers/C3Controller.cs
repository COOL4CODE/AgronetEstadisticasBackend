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

using AgronetEstadisticas.Adapter;
using AgronetEstadisticas.Models;
using AgronetEstadisticas.Models.parametersBinding;

namespace AgronetEstadisticas.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "post")]
    public class C3Controller : ApiController
    {
        [Route("api/Report/301")]
        public IHttpActionResult postReport301(report301 parameters)
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
            var results = adapter.GetDatatable(sql);
            Object returnData = null;

            if (results != null)
            {
                if (parameters.tipo == "parametro")
                {
                    var queryParameters = from p in results.AsEnumerable()
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
                    var queryCharts = from r in results.AsEnumerable()
                                      group r by r["descripcion_VariableCalidad"] into chartGroup
                                      from seriesGroup in
                                          (from r in chartGroup
                                           group r by r["descripcion_Region"])
                                      group seriesGroup by chartGroup.Key;

                    int i = 1;
                    foreach (var outerGroup in queryCharts)
                    {
                        if (i == parameters.id)
                        {
                            Chart chart = new Chart { subtitle = outerGroup.Key.ToString(), series = new List<Series>() };
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
                }
                else if (parameters.tipo == "tabla")
                {
                    Table table = new Table { rows = adapter.GetDatatable(sql) };
                    returnData = (Table)table;
                }
            }
            else
            {
                return NotFound();
            }

            return Ok(returnData);
        }

        [Route("api/Report/302")]
        public IHttpActionResult postReport302(report302 parameters)
        {
            Object returnData = null;

            var adapter = new SQLAdapter();
            string sql = String.Format(@"create table  #SP_PRECIOS_LECHE_REGION (
	                                fecha date, 
	                                codigoRegion int,
	                                precio int,
	                                volumen int,
	                                variacionPrecio float,
	                                variacionVolumen float
                                )
                                insert into #SP_PRECIOS_LECHE_REGION EXEC [AgronetCadenas].[dbo].[SP_PRECIOS_LECHE_REGION]
		                                @Fecha_inicial = N'{0}',
		                                @Fecha_final = N'{1}'
                                SELECT 
	                                region.descripcion_Region, 
	                                #SP_PRECIOS_LECHE_REGION.fecha,
	                                #SP_PRECIOS_LECHE_REGION.precio,
	                                #SP_PRECIOS_LECHE_REGION.volumen,
	                                ISNULL(#SP_PRECIOS_LECHE_REGION.variacionPrecio,0) as variacionPrecio,
	                                ISNULL(#SP_PRECIOS_LECHE_REGION.variacionVolumen,0) as variacionVolumen
                                 FROM  AgronetCadenas.Leche.region region INNER JOIN #SP_PRECIOS_LECHE_REGION 
                                 ON #SP_PRECIOS_LECHE_REGION.codigoRegion = region.codigo_Region
                                 WHERE #SP_PRECIOS_LECHE_REGION.fecha between '{2}' and '{3}'

                                DROP TABLE #SP_PRECIOS_LECHE_REGION", parameters.fecha_inicial.ToString("yyyy-MM-dd"), parameters.fecha_final.ToString("yyyy-MM-dd"), parameters.fecha_inicial.ToString("yyyy-MM-dd"), parameters.fecha_final.ToString("yyyy-MM-dd"));

            DataTable result = adapter.GetDatatable(sql);

            if (parameters.tipo == "grafico")
            {                
                switch (parameters.id)
                {
                    case 1:
                        Chart chart1 = new Chart { subtitle = "Tendencia mensual al precio", series = new List<Series>() };

                        var queryCharts = from r in result.AsEnumerable()
                                          group r by r["descripcion_Region"] into seriesGroup
                                          select seriesGroup;

                        foreach (var outerGroup in queryCharts)
                        {
                            var serie = new Series { name = outerGroup.Key.ToString(), data = new List<Data>() };
                            foreach (var element in outerGroup)
                            {
                                var name = Convert.ToDateTime(element["fecha"]);
                                var y = Convert.ToDouble(element["precio"]);
                                var data = new Data { name = String.Format("{0:y}", name), y = y };
                                serie.data.Add(data);
                            }
                            chart1.series.Add(serie);
                        }

                        returnData = (Chart)chart1;
                        break;
                    case 2:
                        Chart chart2 = new Chart { subtitle = "Tendencia mensual del volumen", series = new List<Series>() };

                        var queryCharts1 = from r in result.AsEnumerable()
                                           group r by r["descripcion_Region"] into seriesGroup
                                           select seriesGroup;

                        foreach (var outerGroup in queryCharts1)
                        {
                            var serie = new Series { name = outerGroup.Key.ToString(), data = new List<Data>() };
                            foreach (var element in outerGroup)
                            {
                                var name = Convert.ToDateTime(element["fecha"]);
                                var y = Convert.ToDouble(element["volumen"]);
                                var data = new Data { name = String.Format("{0:y}", name), y = y };
                                serie.data.Add(data);
                            }
                            chart2.series.Add(serie);
                        }

                        returnData = (Chart)chart2;
                        break;
                }

            }
            else if (parameters.tipo == "tabla")
            {
                switch (parameters.id)
                {
                    case 1:
                        Table table = new Table { rows = result };
                        returnData = (Table)table;
                        break;
                }
            }

            if (returnData == null)
            {
                return NotFound();
            }

            return Ok(returnData);

        }

        [Route("api/Report/303")]
        public IHttpActionResult postReport303(report303 parameters)
        {
            Object returnData = null;
            if (returnData == null)
            {
                return NotFound();
            }

            return Ok(returnData);
        }

        [Route("api/Report/304")]
        public IHttpActionResult postReport304(report304 parameters)
        {
            Object returnData = null;
            if (returnData == null)
            {
                return NotFound();
            }

            return Ok(returnData);
        }

        [Route("api/Report/305")]
        public IHttpActionResult postReport305(report305 parameters)
        {
            Object returnData = null;
            if (returnData == null)
            {
                return NotFound();
            }

            return Ok(returnData);
        }

        [Route("api/Report/306")]
        public IHttpActionResult postReport306(report306 parameters)
        {
            Object returnData = null;
            if (returnData == null)
            {
                return NotFound();
            }

            return Ok(returnData);
        }

        [Route("api/Report/307")]
        public IHttpActionResult postReport307(report307 parameters)
        {
            Object returnData = null;
            if (returnData == null)
            {
                return NotFound();
            }

            return Ok(returnData);
        }

        [Route("api/Report/308")]
        public IHttpActionResult postReport308(report308 parameters)
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
