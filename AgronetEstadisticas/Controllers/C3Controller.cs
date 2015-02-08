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

            Object returnData = null;
            SQLAdapter adapter = new SQLAdapter();
            switch (parameters.tipo)
            {
                case "parametro":
                    switch (parameters.id)
                    {
                        case 1:

                            String SQLquery1 = @"SELECT DISTINCT
                                YEAR(calidadRegional.fecha_CalidadRegional) as anios
                                    FROM   
                                (AgronetCadenas.compraLeche.calidadRegional calidadRegional 
                                INNER JOIN AgronetCadenas.Leche.region region ON calidadRegional.codigoRegion_CalidadRegional=region.codigo_Region) 
                                INNER JOIN AgronetCadenas.Leche.variableCalidad variableCalidad ON calidadRegional.codigoVariableCalidad_CalidadRegional=variableCalidad.codigo_VariableCalidad
                                ORDER BY
                                YEAR(calidadRegional.fecha_CalidadRegional)";

                            DataTable data = adapter.GetDatatable(SQLquery1);
                            Parameter param = new Parameter { name = "anios" , data = new List<ParameterData>() };
                            foreach (var d in (from p in data.AsEnumerable() select p[@"anios"])){
                                ParameterData parameter = new ParameterData { name = Convert.ToString(d), value = Convert.ToString(d) };
                                param.data.Add(parameter);
                            }

                            returnData = (Parameter)param;

                            break;
                      
                    }
                    break;
                case "grafico":

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
                                            calidadRegional.fecha_CalidadRegional DESC", parameters.anio_inicial, parameters.anio_final);

                    DataTable results = adapter.GetDatatable(sql);

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

                    break;
                case "tabla":
                    switch (parameters.id)
                    {
                        case 1:

                             string sqlTable= String.Format(@"SELECT variableCalidad.descripcion_VariableCalidad,
                                            region.descripcion_Region,
                                            calidadRegional.fecha_CalidadRegional, 
                                            calidadRegional.valor_CalidadRegional, 
                                            calidadRegional.codigoVariableCalidad_CalidadRegional
                                            FROM (AgronetCadenas.compraLeche.calidadRegional calidadRegional INNER JOIN AgronetCadenas.Leche.region region ON calidadRegional.codigoRegion_CalidadRegional = region.codigo_Region)
                                            INNER JOIN AgronetCadenas.Leche.variableCalidad variableCalidad ON calidadRegional.codigoVariableCalidad_CalidadRegional=variableCalidad.codigo_VariableCalidad
                                            WHERE calidadRegional.fecha_CalidadRegional BETWEEN '{0}' AND '{1}'
                                            ORDER BY  variableCalidad.descripcion_VariableCalidad, 
                                            region.descripcion_Region,
                                            calidadRegional.fecha_CalidadRegional DESC", parameters.anio_inicial, parameters.anio_final);

                    DataTable tableResults = adapter.GetDatatable(sqlTable);
                    Table table = new Table { rows = tableResults};
                    returnData = (Table)table;

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

        [Route("api/Report/302")]
        public IHttpActionResult postReport302(report302 parameters)
        {
            Object returnData = null;
            SQLAdapter adapter = new SQLAdapter();
            switch (parameters.tipo)
            {
                case "parametro":
                    switch (parameters.id)
                    {
                        case 1:
                            string sql = @"SELECT DISTINCT
                                YEAR(calidadRegional.fecha_CalidadRegional) as anios
                                FROM   
                                AgronetCadenas.compraLeche.calidadRegional calidadRegional
                                ORDER BY YEAR(calidadRegional.fecha_CalidadRegional)";

                            DataTable data = adapter.GetDatatable(sql);
                            Parameter param = new Parameter { name = "anios" , data = new List<ParameterData>() };
                            foreach (var d in (from p in data.AsEnumerable() select p[@"anios"])){
                                ParameterData parameter = new ParameterData { name = Convert.ToString(d), value = Convert.ToString(d) };
                                param.data.Add(parameter);
                            }

                            returnData = (Parameter)param;

                            break;
                    }
                    break;
                case "grafico":

                    string sqlChart = String.Format(@"create table  #SP_PRECIOS_LECHE_REGION (
	                                                        fecha date, 
	                                                        codigoRegion int,
	                                                        precio int,
	                                                        volumen int,
	                                                        variacionPrecio float,
	                                                        variacionVolumen float
                                                        )
                                                        insert into #SP_PRECIOS_LECHE_REGION EXEC [AgronetCadenas].[dbo].[SP_PRECIOS_LECHE_REGION]
		                                                        @Fecha_inicial = N'{0}-01-01',
		                                                        @Fecha_final = N'{1}-01-01'

                                                        SELECT 
	                                                        region.descripcion_Region, 
	                                                        #SP_PRECIOS_LECHE_REGION.fecha,
	                                                        #SP_PRECIOS_LECHE_REGION.precio,
	                                                        #SP_PRECIOS_LECHE_REGION.volumen,
	                                                        ISNULL(#SP_PRECIOS_LECHE_REGION.variacionPrecio,0) as variacionPrecio,
	                                                        ISNULL(#SP_PRECIOS_LECHE_REGION.variacionVolumen,0) as variacionVolumen
                                                         FROM   AgronetCadenas.Leche.region region INNER JOIN #SP_PRECIOS_LECHE_REGION 
                                                         ON #SP_PRECIOS_LECHE_REGION.codigoRegion = region.codigo_Region
                                                         WHERE #SP_PRECIOS_LECHE_REGION.fecha between '{2}-01-01' and '{3}-01-01'

                                                        DROP TABLE #SP_PRECIOS_LECHE_REGION",
                                                        parameters.fecha_inicial,parameters.fecha_final,
                                                        parameters.fecha_inicial,parameters.fecha_final
                                                        );
                            DataTable datatable = adapter.GetDatatable(sqlChart);
                            var dataGroups = from r in datatable.AsEnumerable()
                                             group r by r["descripcion_Region"] into seriesGroup
                                             select seriesGroup;

                    switch (parameters.id)
                    {
                        case 1:
                            
                            Chart chart1 = new Chart { subtitle = @"Tendencia mensual del precio", series = new List<Series>() };

                            foreach(var dataGroup in dataGroups){
                                var serie = new Series { name = dataGroup.Key.ToString(), data = new List<Data>() };

                                foreach(var seriesData in dataGroup){
                                    var name = Convert.ToDateTime(seriesData["fecha"]);
                                    var y = Convert.ToDouble(seriesData["precio"]);
                                    var data = new Data { name = String.Format("{0:y}", name), y = y };
                                    serie.data.Add(data);
                                }
                                chart1.series.Add(serie);
                            }

                            returnData = (Chart)chart1;

                            break;
                        case 2:

                             Chart chart2 = new Chart { subtitle = @"Tendencia mensual del volumen", series = new List<Series>() };

                            foreach(var dataGroup in dataGroups){
                                var serie = new Series { name = dataGroup.Key.ToString(), data = new List<Data>() };

                                foreach(var seriesData in dataGroup){
                                    var name = Convert.ToDateTime(seriesData["fecha"]);
                                    var y = Convert.ToDouble(seriesData["volumen"]);
                                    var data = new Data { name = String.Format("{0:y}", name), y = y };
                                    serie.data.Add(data);
                                }
                                chart2.series.Add(serie);
                            }

                            returnData = (Chart)chart2;

                            break;

                    }
                    break;
                case "tabla":

                    string sqlTable = String.Format(@"create table  #SP_PRECIOS_LECHE_REGION (
	                                                        fecha date, 
	                                                        codigoRegion int,
	                                                        precio int,
	                                                        volumen int,
	                                                        variacionPrecio float,
	                                                        variacionVolumen float
                                                        )
                                                        insert into #SP_PRECIOS_LECHE_REGION EXEC [AgronetCadenas].[dbo].[SP_PRECIOS_LECHE_REGION]
		                                                        @Fecha_inicial = N'{0}-01-01',
		                                                        @Fecha_final = N'{1}-01-01'

                                                        SELECT 
	                                                        region.descripcion_Region, 
	                                                        #SP_PRECIOS_LECHE_REGION.fecha,
	                                                        #SP_PRECIOS_LECHE_REGION.precio,
	                                                        #SP_PRECIOS_LECHE_REGION.volumen,
	                                                        ISNULL(#SP_PRECIOS_LECHE_REGION.variacionPrecio,0) as variacionPrecio,
	                                                        ISNULL(#SP_PRECIOS_LECHE_REGION.variacionVolumen,0) as variacionVolumen
                                                         FROM   AgronetCadenas.Leche.region region INNER JOIN #SP_PRECIOS_LECHE_REGION 
                                                         ON #SP_PRECIOS_LECHE_REGION.codigoRegion = region.codigo_Region
                                                         WHERE #SP_PRECIOS_LECHE_REGION.fecha between '{2}-01-01' and '{3}-01-01'

                                                        DROP TABLE #SP_PRECIOS_LECHE_REGION",
                                                        parameters.fecha_inicial,parameters.fecha_final,
                                                        parameters.fecha_inicial,parameters.fecha_final
                                                        );
                            DataTable datatable2 = adapter.GetDatatable(sqlTable);

                    switch (parameters.id)
                    {
                        case 1:

                            Table table = new Table { rows = datatable2 };
                            returnData = (Table)table;

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

        [Route("api/Report/303")]
        public IHttpActionResult postReport303(report303 parameters)
        {
            
            Object returnData = null;
            SQLAdapter adapter = new SQLAdapter();
            switch (parameters.tipo)
            {
                case "parametro":
                    switch (parameters.id)
                    {
                        case 1:
                            string sql = @"SELECT DISTINCT
                                YEAR(calidadRegional.fecha_CalidadRegional) as anios
                                FROM   
                                AgronetCadenas.compraLeche.calidadRegional calidadRegional
                                ORDER BY YEAR(calidadRegional.fecha_CalidadRegional)";

                            DataTable data = adapter.GetDatatable(sql);
                            Parameter param = new Parameter { name = "anios" , data = new List<ParameterData>() };
                            foreach (var d in (from p in data.AsEnumerable() select p[@"anios"])){
                                ParameterData parameter = new ParameterData { name = Convert.ToString(d), value = Convert.ToString(d) };
                                param.data.Add(parameter);
                            }

                            returnData = (Parameter)param;

                            break;
                    }
                    break;
                case "grafico":

                    string sqlChart = @"create table  #SP_PRECIOS_LECHE_DEPARTAMENTO(
	                                    fecha date,
	                                    codigoDepartamento int,
	                                    precio int,
	                                    volumen int,
	                                    variacionPrecio float,
	                                    variacionVolumen float
                                        )
                                        insert into #SP_PRECIOS_LECHE_DEPARTAMENTO EXEC [AgronetCadenas].[dbo].[SP_PRECIOS_LECHE_DEPARTAMENTO]
		                                        @Fecha_inicial = N'"+parameters.fecha_inicial+@"-01-01', @Fecha_final = N'"+parameters.fecha_final+@"-12-31' 
                                        SELECT 
	                                    regionDepartamento.descripcionDepartamento_RegionDepartamento as descDep, 
	                                    regionDepartamento.codigoDepartamento_RegionDepartamento as codigoDep,
	                                    #SP_PRECIOS_LECHE_DEPARTAMENTO.fecha,
	                                    #SP_PRECIOS_LECHE_DEPARTAMENTO.precio,
	                                    #SP_PRECIOS_LECHE_DEPARTAMENTO.volumen,
	                                    ISNULL(#SP_PRECIOS_LECHE_DEPARTAMENTO.variacionPrecio,0) as variacionPrecio,
	                                    ISNULL(#SP_PRECIOS_LECHE_DEPARTAMENTO.variacionVolumen,0) as variacionVolumen
                                        FROM   AgronetCadenas.Leche.regionDepartamento regionDepartamento INNER JOIN #SP_PRECIOS_LECHE_DEPARTAMENTO 
                                        ON #SP_PRECIOS_LECHE_DEPARTAMENTO.codigoDepartamento = regionDepartamento.codigoDepartamento_RegionDepartamento
                                        WHERE #SP_PRECIOS_LECHE_DEPARTAMENTO.fecha between '"+parameters.fecha_inicial+@"-01-01' and '"
                                                                                             +parameters.fecha_final+@"-12-31'
                                                                                             and regionDepartamento.descripcionDepartamento_RegionDepartamento IN ("+string.Join(",",parameters.departamento.Select(d => "'"+d+"'")) +@") 
                                       DROP TABLE #SP_PRECIOS_LECHE_DEPARTAMENTO";
                                                        
                        DataTable datatable = adapter.GetDatatable(sqlChart);
                        var dataGroups = from r in datatable.AsEnumerable()
                        group r by r["descDep"] into seriesGroup
                        select seriesGroup;

                    switch (parameters.id)
                    {
                        case 1:
                            
                            Chart chart1 = new Chart { subtitle = @"Tendencia mensual del precio", series = new List<Series>() };

                            foreach(var dataGroup in dataGroups){
                                var serie = new Series { name = dataGroup.Key.ToString(), data = new List<Data>() };

                                foreach(var seriesData in dataGroup){
                                    var name = Convert.ToDateTime(seriesData["fecha"]);
                                    var y = Convert.ToDouble(seriesData["precio"]);
                                    var data = new Data { name = String.Format("{0:y}", name), y = y };
                                    serie.data.Add(data);
                                }
                                chart1.series.Add(serie);
                            }

                            returnData = (Chart)chart1;

                            break;
                        case 2:

                            Chart chart2 = new Chart { subtitle = @"Tendencia mensual del volumen", series = new List<Series>() };

                            foreach(var dataGroup in dataGroups){
                                var serie = new Series { name = dataGroup.Key.ToString(), data = new List<Data>() };

                                foreach(var seriesData in dataGroup){
                                    var name = Convert.ToDateTime(seriesData["fecha"]);
                                    var y = Convert.ToDouble(seriesData["volumen"]);
                                    var data = new Data { name = String.Format("{0:y}", name), y = y };
                                    serie.data.Add(data);
                                }
                                chart2.series.Add(serie);
                            }

                            returnData = (Chart)chart2;

                            break;

                    }
                    break;
                case "tabla":

                    string sqlTable = @"create table  #SP_PRECIOS_LECHE_DEPARTAMENTO(
	                                    fecha date,
	                                    codigoDepartamento int,
	                                    precio int,
	                                    volumen int,
	                                    variacionPrecio float,
	                                    variacionVolumen float
                                        )
                                        insert into #SP_PRECIOS_LECHE_DEPARTAMENTO EXEC [AgronetCadenas].[dbo].[SP_PRECIOS_LECHE_DEPARTAMENTO]
		                                        @Fecha_inicial = N'" + parameters.fecha_inicial + @"-01-01', @Fecha_final = N'" + parameters.fecha_final + @"-12-31' 
                                        SELECT 
	                                    regionDepartamento.descripcionDepartamento_RegionDepartamento as descDep, 
	                                    regionDepartamento.codigoDepartamento_RegionDepartamento as codigoDep,
	                                    #SP_PRECIOS_LECHE_DEPARTAMENTO.fecha,
	                                    #SP_PRECIOS_LECHE_DEPARTAMENTO.precio,
	                                    #SP_PRECIOS_LECHE_DEPARTAMENTO.volumen,
	                                    ISNULL(#SP_PRECIOS_LECHE_DEPARTAMENTO.variacionPrecio,0) as variacionPrecio,
	                                    ISNULL(#SP_PRECIOS_LECHE_DEPARTAMENTO.variacionVolumen,0) as variacionVolumen
                                        FROM   AgronetCadenas.Leche.regionDepartamento regionDepartamento INNER JOIN #SP_PRECIOS_LECHE_DEPARTAMENTO 
                                        ON #SP_PRECIOS_LECHE_DEPARTAMENTO.codigoDepartamento = regionDepartamento.codigoDepartamento_RegionDepartamento
                                        WHERE #SP_PRECIOS_LECHE_DEPARTAMENTO.fecha between '" + parameters.fecha_inicial + @"-01-01' and '"
                                                                                             + parameters.fecha_final + @"-12-31'
                                        and regionDepartamento.descripcionDepartamento_RegionDepartamento = IN (" + string.Join(",", parameters.departamento.Select(d => "'" + d + "'")) + ") DROP TABLE #SP_PRECIOS_LECHE_DEPARTAMENTO";
                            DataTable datatable2 = adapter.GetDatatable(sqlTable);

                    switch (parameters.id)
                    {
                        case 1:

                            Table table = new Table { rows = datatable2 };
                            returnData = (Table)table;

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
