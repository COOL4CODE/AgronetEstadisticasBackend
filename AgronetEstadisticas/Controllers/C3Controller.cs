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
                                            WHERE calidadRegional.fecha_CalidadRegional BETWEEN '{0}-01-01' AND '{1}-12-31'
                                            ORDER BY calidadRegional.fecha_CalidadRegional, variableCalidad.descripcion_VariableCalidad, 
                                            region.descripcion_Region DESC", parameters.anio_inicial, parameters.anio_final);

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
                                            WHERE calidadRegional.fecha_CalidadRegional BETWEEN '{0}-01-01' AND '{1}-12-31'
                                            ORDER BY  calidadRegional.fecha_CalidadRegional, variableCalidad.descripcion_VariableCalidad, 
                                            region.descripcion_Region DESC", parameters.anio_inicial, parameters.anio_final);

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
                            string sql = @"SELECT DISTINCT YEAR(fecha_PrecioRegional) anios FROM AgronetCadenas.compraLeche.precioRegional
ORDER BY YEAR(fecha_PrecioRegional)";

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
		                                                        @Fecha_final = N'{1}-12-31'

                                                        SELECT 
	                                                        region.descripcion_Region, 
	                                                        #SP_PRECIOS_LECHE_REGION.fecha,
	                                                        #SP_PRECIOS_LECHE_REGION.precio,
	                                                        #SP_PRECIOS_LECHE_REGION.volumen,
	                                                        ISNULL(#SP_PRECIOS_LECHE_REGION.variacionPrecio,0) as variacionPrecio,
	                                                        ISNULL(#SP_PRECIOS_LECHE_REGION.variacionVolumen,0) as variacionVolumen
                                                         FROM   AgronetCadenas.Leche.region region INNER JOIN #SP_PRECIOS_LECHE_REGION 
                                                         ON #SP_PRECIOS_LECHE_REGION.codigoRegion = region.codigo_Region
                                                         WHERE #SP_PRECIOS_LECHE_REGION.fecha between '{2}-01-01' and '{3}-12-31'

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
		                                                        @Fecha_final = N'{1}-12-31'

                                                        SELECT 
	                                                        region.descripcion_Region, 
	                                                        #SP_PRECIOS_LECHE_REGION.fecha,
	                                                        #SP_PRECIOS_LECHE_REGION.precio,
	                                                        #SP_PRECIOS_LECHE_REGION.volumen,
	                                                        ISNULL(#SP_PRECIOS_LECHE_REGION.variacionPrecio,0) as variacionPrecio,
	                                                        ISNULL(#SP_PRECIOS_LECHE_REGION.variacionVolumen,0) as variacionVolumen
                                                         FROM   AgronetCadenas.Leche.region region INNER JOIN #SP_PRECIOS_LECHE_REGION 
                                                         ON #SP_PRECIOS_LECHE_REGION.codigoRegion = region.codigo_Region
                                                         WHERE #SP_PRECIOS_LECHE_REGION.fecha between '{2}-01-01' and '{3}-12-31'

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
                            string sql = @"SELECT DISTINCT YEAR(fecha_PrecioRegional) anios FROM AgronetCadenas.compraLeche.precioRegional
ORDER BY YEAR(fecha_PrecioRegional)";

                            DataTable data = adapter.GetDatatable(sql);
                            Parameter param = new Parameter { name = "anios" , data = new List<ParameterData>() };
                            foreach (var d in (from p in data.AsEnumerable() select p[@"anios"])){
                                ParameterData parameter = new ParameterData { name = Convert.ToString(d), value = Convert.ToString(d) };
                                param.data.Add(parameter);
                            }

                            returnData = (Parameter)param;

                            break;
                        case 2:

                            string sqlp2 = @"create table  #SP_PRECIOS_LECHE_DEPARTAMENTO(
	                                        fecha date,
	                                        codigoDepartamento int,
	                                        precio int,
	                                        volumen int,
	                                        variacionPrecio float,
	                                        variacionVolumen float
                                        )
                                        insert into #SP_PRECIOS_LECHE_DEPARTAMENTO EXEC [AgronetCadenas].[dbo].[SP_PRECIOS_LECHE_DEPARTAMENTO]
		                                         @Fecha_inicial = N'" + parameters.fecha_inicial + @"-01-01', @Fecha_final = N'" + parameters.fecha_final + @"-12-31'

                                        SELECT DISTINCT
	                                        regionDepartamento.descripcionDepartamento_RegionDepartamento as deptos
                                         FROM   AgronetCadenas.Leche.regionDepartamento regionDepartamento INNER JOIN #SP_PRECIOS_LECHE_DEPARTAMENTO 
                                         ON #SP_PRECIOS_LECHE_DEPARTAMENTO.codigoDepartamento = regionDepartamento.codigoDepartamento_RegionDepartamento
                                         WHERE #SP_PRECIOS_LECHE_DEPARTAMENTO.fecha between '" + parameters.fecha_inicial + @"-01-01' and '"
                                                                                             + parameters.fecha_final + @"-12-31'
                                                                                              
                                       DROP TABLE #SP_PRECIOS_LECHE_DEPARTAMENTO";

                            DataTable datap2 = adapter.GetDatatable(sqlp2);
                            Parameter paramp2 = new Parameter { name = "departamentos" , data = new List<ParameterData>() };
                            foreach (var d in (from p in datap2.AsEnumerable() select p[@"deptos"])){
                                ParameterData parameter = new ParameterData { name = Convert.ToString(d), value = Convert.ToString(d) };
                                paramp2.data.Add(parameter);
                            }

                            returnData = (Parameter)paramp2;

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
                                        and regionDepartamento.descripcionDepartamento_RegionDepartamento IN (" + string.Join(",", parameters.departamento.Select(d => "'" + d + "'")) + @")
                                        DROP TABLE #SP_PRECIOS_LECHE_DEPARTAMENTO";
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
            SQLAdapter adapter = new SQLAdapter();
            switch (parameters.tipo)
            {
                case "parametro":
                    switch (parameters.id)
                    {
                        case 1:
                            string sql = @"select DISTINCT YEAR(fecha_PrecioDepartamental) as anios 
                                        from AgronetCadenas.ventaLeche.PrecioDepartamental
                                        ORDER BY YEAR(fecha_PrecioDepartamental)
                                        ";

                            DataTable data = adapter.GetDatatable(sql);
                            Parameter param = new Parameter { name = "anios" , data = new List<ParameterData>() };
                            foreach (var d in (from p in data.AsEnumerable() select p[@"anios"])){
                                ParameterData parameter = new ParameterData { name = Convert.ToString(d), value = Convert.ToString(d) };
                                param.data.Add(parameter);
                            }

                            returnData = (Parameter)param;
                            break;
                        case 2:
                            string sqlp2 = @"SELECT TOP (100) PERCENT AgronetCadenas.ventaLeche.PrecioDepartamental.codigoDepartamento_PrecioDepartamental, AgronetCadenas.dbo.Departamentos.nombreDepartamento
FROM AgronetCadenas.dbo.Departamentos INNER JOIN AgronetCadenas.ventaLeche.PrecioDepartamental
ON  AgronetCadenas.dbo.Departamentos.codigoDepartamento = AgronetCadenas.ventaLeche.PrecioDepartamental.codigoDepartamento_PrecioDepartamental 
GROUP BY AgronetCadenas.dbo.Departamentos.nombreDepartamento, AgronetCadenas.ventaLeche.PrecioDepartamental.codigoDepartamento_PrecioDepartamental 
ORDER BY AgronetCadenas.dbo.Departamentos.nombreDepartamento";

                             DataTable datap2 = adapter.GetDatatable(sqlp2);
                            Parameter paramp2 = new Parameter { name = "departamentos" , data = new List<ParameterData>() };
                            foreach (var d in datap2.AsEnumerable()){
                                ParameterData parameter = new ParameterData { name = Convert.ToString(d["nombreDepartamento"]), value = Convert.ToString(d["codigoDepartamento_PrecioDepartamental"]) };
                                paramp2.data.Add(parameter);
                            }

                            returnData = (Parameter)paramp2;        

                            break;
                    }
                    break;
                case "grafico":
                    switch (parameters.id)
                    {
                        case 1:
                            break;
                    }
                    break;
                case "tabla":
                    switch (parameters.id)
                    {
                        case 1:
                            DataTable datatable = adapter.GetDatatable(String.Format(@"create table  #SP_PRECIOS_VENTALECHE_DEPARTAMENTO(
	                                                                                    fecha date,
	                                                                                    codigoDepartamento int,
	                                                                                    codigoProducto int,
	                                                                                    codigoTipoProducto int,
	                                                                                    producto text,
	                                                                                    precio float,
	                                                                                    unidadPrecio text,
	                                                                                    volumen float,
	                                                                                    unidadVolumen text,
	                                                                                    variacionPrecio float,
	                                                                                    variacionVolumen float
                                                                                    )
                                                                                    insert into #SP_PRECIOS_VENTALECHE_DEPARTAMENTO EXEC [AgronetCadenas].[dbo].[SP_PRECIOS_VENTALECHE_DEPARTAMENTO]
		                                                                                    @Fecha_inicial = N'{0}-01-01',
		                                                                                    @Fecha_final = N'{1}-12-31'

                                                                                     SELECT 
	                                                                                    regionDepartamento.nombreDepartamento, 
	                                                                                    producto.descripcion_Producto,
	                                                                                    productoTipo.descripcion_productoTipo,
	                                                                                    #SP_PRECIOS_VENTALECHE_DEPARTAMENTO.fecha,
	                                                                                    #SP_PRECIOS_VENTALECHE_DEPARTAMENTO.precio,
	                                                                                    #SP_PRECIOS_VENTALECHE_DEPARTAMENTO.volumen,
	                                                                                    ISNULL(#SP_PRECIOS_VENTALECHE_DEPARTAMENTO.variacionPrecio,0) as variacionPrecio,
	                                                                                    ISNULL(#SP_PRECIOS_VENTALECHE_DEPARTAMENTO.variacionVolumen,0) as variacionVolumen
                                                                                        FROM   
                                                                                        AgronetCadenas.dbo.Departamentos regionDepartamento 
                                                                                        INNER JOIN #SP_PRECIOS_VENTALECHE_DEPARTAMENTO 
	                                                                                    ON #SP_PRECIOS_VENTALECHE_DEPARTAMENTO.codigoDepartamento = regionDepartamento.codigoDepartamento
                                                                                        INNER JOIN  AgronetCadenas.ventaLeche.producto producto 
	                                                                                    ON producto.codigo_Producto = #SP_PRECIOS_VENTALECHE_DEPARTAMENTO.codigoProducto
	                                                                                    INNER JOIN AgronetCadenas.ventaLeche.productoTipo productoTipo
	                                                                                    ON producto.codigo_Producto = productoTipo.codigoProducto_productoTipo
                                                                                        WHERE regionDepartamento.codigoDepartamento = {2}
	                                                                                    ORDER BY producto.descripcion_Producto, productoTipo.descripcion_productoTipo, #SP_PRECIOS_VENTALECHE_DEPARTAMENTO.fecha
                                                                                    DROP TABLE #SP_PRECIOS_VENTALECHE_DEPARTAMENTO", parameters.fecha_inicial, parameters.fecha_final, parameters.departamento));
                            Table table = new Table { rows = datatable };
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

        [Route("api/Report/305")]
        public IHttpActionResult postReport305(report305 parameters)
        {
            Object returnData = null;
            SQLAdapter adapter = new SQLAdapter();
            switch (parameters.tipo)
            {
                case "parametro":
                    switch (parameters.id)
                    {
                        case 1:

                            string sqlp1 = @"select DISTINCT YEAR(fecha_PrecioDepartamental) as anios
                                        from AgronetCadenas.ventaLeche.PrecioDepartamental
                                        ORDER BY YEAR(fecha_PrecioDepartamental)
                                        ";

                            DataTable datap1 = adapter.GetDatatable(sqlp1);
                            Parameter param1 = new Parameter { name = "anios" , data = new List<ParameterData>() };
                            foreach (var d in (from p in datap1.AsEnumerable() select p[@"anios"])){
                                ParameterData parameter = new ParameterData { name = Convert.ToString(d), value = Convert.ToString(d) };
                                param1.data.Add(parameter);
                            }

                            returnData = (Parameter)param1;
                            break;
                        case 2:

                            String sqlp2 = @"SELECT  AgronetCadenas.ventaLeche.PrecioDepartamental.codigoDepartamento_PrecioDepartamental, AgronetCadenas.dbo.Departamentos.nombreDepartamento 
FROM AgronetCadenas.dbo.Departamentos INNER JOIN AgronetCadenas.ventaLeche.PrecioDepartamental ON AgronetCadenas.dbo.Departamentos.codigoDepartamento = AgronetCadenas.ventaLeche.PrecioDepartamental.codigoDepartamento_PrecioDepartamental
GROUP BY AgronetCadenas.dbo.Departamentos.nombreDepartamento, AgronetCadenas.ventaLeche.PrecioDepartamental.codigoDepartamento_PrecioDepartamental
ORDER BY AgronetCadenas.dbo.Departamentos.nombreDepartamento";

                            DataTable datap2 = adapter.GetDatatable(sqlp2);
                            Parameter param2 = new Parameter { name = "departamentos" , data = new List<ParameterData>() };
                            foreach (var d in datap2.AsEnumerable()){
                                ParameterData parameter = new ParameterData { name = Convert.ToString(d["nombreDepartamento"]), value = Convert.ToString(d["codigoDepartamento_PrecioDepartamental"]) };
                                param2.data.Add(parameter);
                            }
                            returnData = (Parameter)param2;
                            break;
                        case 3:

                            DataTable datap3 = adapter.GetDatatable(@"select id_productoTipo, descripcion_productoTipo from AgronetCadenas.ventaLeche.productoTipo");
                            Parameter param3 = new Parameter { name = "productos" , data = new List<ParameterData>() };
                            foreach (var d in datap3.AsEnumerable()){
                                ParameterData parameter = new ParameterData { name = Convert.ToString(d["descripcion_productoTipo"]), value = Convert.ToString(d["id_productoTipo"]) };
                                param3.data.Add(parameter);
                            }
                            returnData = (Parameter)param3;


                            break;
                    }
                    break;
                case "grafico":

                    DataTable datatable = adapter.GetDatatable(String.Format(@"CREATE TABLE  #SP_PRECIOS_VENTALECHE_DEPARTAMENTO(
	                                                                            fecha date,
	                                                                            codigoDepartamento int,
	                                                                            codigoProducto int,
	                                                                            codigoTipoProducto int,
	                                                                            producto text,
	                                                                            precio float,
	                                                                            unidadPrecio text,
	                                                                            volumen float,
	                                                                            unidadVolumen text,
	                                                                            variacionPrecio float,
	                                                                            variacionVolumen float
                                                                            )
                                                                            INSERT INTO #SP_PRECIOS_VENTALECHE_DEPARTAMENTO EXEC [AgronetCadenas].[dbo].[SP_PRECIOS_VENTALECHE_DEPARTAMENTO]
		                                                                            @Fecha_inicial = N'{0}-01-01',
		                                                                            @Fecha_final = N'{1}-12-31'

                                                                            SELECT regionDepartamento.descripcionDepartamento_RegionDepartamento as departamento,
	                                                                            #SP_PRECIOS_VENTALECHE_DEPARTAMENTO.producto,
	                                                                            #SP_PRECIOS_VENTALECHE_DEPARTAMENTO.fecha,
	                                                                            #SP_PRECIOS_VENTALECHE_DEPARTAMENTO.precio,
	                                                                            #SP_PRECIOS_VENTALECHE_DEPARTAMENTO.volumen                                                                            FROM   AgronetCadenas.Leche.regionDepartamento regionDepartamento
                                                                            INNER JOIN #SP_PRECIOS_VENTALECHE_DEPARTAMENTO ON #SP_PRECIOS_VENTALECHE_DEPARTAMENTO.codigoDepartamento = regionDepartamento.codigoDepartamento_RegionDepartamento
                                                                            WHERE regionDepartamento.codigoDepartamento_RegionDepartamento IN (" + string.Join(",", parameters.departamento.Select(d => d)) + @")
                                                                            AND #SP_PRECIOS_VENTALECHE_DEPARTAMENTO.codigoTipoProducto = {2}
                                                                            ORDER BY #SP_PRECIOS_VENTALECHE_DEPARTAMENTO.fecha, regionDepartamento.descripcionDepartamento_RegionDepartamento

                                                                            DROP TABLE #SP_PRECIOS_VENTALECHE_DEPARTAMENTO", parameters.fecha_inicial, parameters.fecha_final, parameters.tipo_producto));
                    var dataGroups = from r in datatable.AsEnumerable()
                                    group r by r["departamento"] into seriesGroup
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

                    String sqlTable = @"create table  #SP_PRECIOS_VENTALECHE_DEPARTAMENTO(
	                                        fecha date,
	                                        codigoDepartamento int,
	                                        codigoProducto int,
	                                        codigoTipoProducto int,
	                                        producto text,
	                                        unidadPrecio text,
	                                        precio int,
	                                        volumen int,
	                                        unidadVolumen text,
	                                        variacionPrecio float,
	                                        variacionVolumen float
                                        )
                                        insert into #SP_PRECIOS_VENTALECHE_DEPARTAMENTO EXEC [AgronetCadenas].[dbo].[SP_PRECIOS_VENTALECHE_DEPARTAMENTO]
		                                        @Fecha_inicial = N'" + parameters.fecha_inicial + @"-01-01',
		                                        @Fecha_final = N'" + parameters.fecha_final + @"-01-01'

                                        SELECT 
	                                        regionDepartamento.descripcionDepartamento_RegionDepartamento as departamento, 
	                                        regionDepartamento.codigoDepartamento_RegionDepartamento as codigoDepartamento,
	                                        producto.descripcion_Producto as producto,
	                                        #SP_PRECIOS_VENTALECHE_DEPARTAMENTO.fecha as fecha,
	                                        #SP_PRECIOS_VENTALECHE_DEPARTAMENTO.precio as precio,
	                                        #SP_PRECIOS_VENTALECHE_DEPARTAMENTO.volumen as volumen,
	                                        ISNULL(#SP_PRECIOS_VENTALECHE_DEPARTAMENTO.variacionPrecio,0) as variacionPrecio,
	                                        ISNULL(#SP_PRECIOS_VENTALECHE_DEPARTAMENTO.variacionVolumen,0) as variacionVolumen
                                         FROM   AgronetCadenas.Leche.regionDepartamento regionDepartamento INNER JOIN #SP_PRECIOS_VENTALECHE_DEPARTAMENTO 
                                         ON #SP_PRECIOS_VENTALECHE_DEPARTAMENTO.codigoDepartamento = regionDepartamento.codigoDepartamento_RegionDepartamento
                                         INNER JOIN  AgronetCadenas.ventaLeche.producto producto ON producto.codigo_Producto = #SP_PRECIOS_VENTALECHE_DEPARTAMENTO.codigoProducto
                                         WHERE #SP_PRECIOS_VENTALECHE_DEPARTAMENTO.fecha between '" + parameters.fecha_inicial + @"-01-01' and '" + parameters.fecha_final + @"-12-31'
                                         and regionDepartamento.descripcionDepartamento_RegionDepartamento  IN (" + string.Join(",", parameters.departamento.Select(d => "'" + d + "'")) + @") 
                                         and producto.descripcion_Producto = '" + parameters.tipo_producto + @"'

                                        DROP TABLE #SP_PRECIOS_VENTALECHE_DEPARTAMENTO";

                    switch (parameters.id)
                    {
                        case 1:

                            DataTable datatable2 = adapter.GetDatatable(String.Format(@"CREATE TABLE  #SP_PRECIOS_VENTALECHE_DEPARTAMENTO(
	                                                                            fecha date,
	                                                                            codigoDepartamento int,
	                                                                            codigoProducto int,
	                                                                            codigoTipoProducto int,
	                                                                            producto text,
	                                                                            precio float,
	                                                                            unidadPrecio text,
	                                                                            volumen float,
	                                                                            unidadVolumen text,
	                                                                            variacionPrecio float,
	                                                                            variacionVolumen float
                                                                            )
                                                                            INSERT INTO #SP_PRECIOS_VENTALECHE_DEPARTAMENTO EXEC [AgronetCadenas].[dbo].[SP_PRECIOS_VENTALECHE_DEPARTAMENTO]
		                                                                            @Fecha_inicial = N'{0}-01-01',
		                                                                            @Fecha_final = N'{1}-12-31'

                                                                            SELECT regionDepartamento.descripcionDepartamento_RegionDepartamento as departamento,
	                                                                            #SP_PRECIOS_VENTALECHE_DEPARTAMENTO.producto,
	                                                                            #SP_PRECIOS_VENTALECHE_DEPARTAMENTO.fecha,
	                                                                            #SP_PRECIOS_VENTALECHE_DEPARTAMENTO.precio,
	                                                                            #SP_PRECIOS_VENTALECHE_DEPARTAMENTO.volumen,    
                                                                                ISNULL(#SP_PRECIOS_VENTALECHE_DEPARTAMENTO.variacionPrecio,0) as variacionPrecio,
	                                                                            ISNULL(#SP_PRECIOS_VENTALECHE_DEPARTAMENTO.variacionVolumen,0) as variacionVolumen,
                                                                                #SP_PRECIOS_VENTALECHE_DEPARTAMENTO.unidadVolumen,
                                                                                #SP_PRECIOS_VENTALECHE_DEPARTAMENTO.unidadPrecio
                                                                            FROM   AgronetCadenas.Leche.regionDepartamento regionDepartamento
                                                                            INNER JOIN #SP_PRECIOS_VENTALECHE_DEPARTAMENTO ON #SP_PRECIOS_VENTALECHE_DEPARTAMENTO.codigoDepartamento = regionDepartamento.codigoDepartamento_RegionDepartamento
                                                                            WHERE regionDepartamento.codigoDepartamento_RegionDepartamento IN (" + string.Join(",", parameters.departamento.Select(d => d)) + @")
                                                                            AND #SP_PRECIOS_VENTALECHE_DEPARTAMENTO.codigoTipoProducto = {2}
                                                                            ORDER BY #SP_PRECIOS_VENTALECHE_DEPARTAMENTO.fecha, regionDepartamento.descripcionDepartamento_RegionDepartamento

                                                                            DROP TABLE #SP_PRECIOS_VENTALECHE_DEPARTAMENTO", parameters.fecha_inicial, parameters.fecha_final, parameters.tipo_producto));
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

        [Route("api/Report/306")]
        public IHttpActionResult postReport306(report306 parameters)
        {
            Object returnData = null;
            SQLAdapter adapter = new SQLAdapter();
            switch (parameters.tipo)
            {
                case "parametro":
                    switch (parameters.id)
                    {
                        case 1:

                            String sqlp1 = @"select DISTINCT YEAR(fecha_PreciosConsumidor) anios from [AgronetCadenas].consumidor.PreciosConsumidor
order by YEAR(fecha_PreciosConsumidor);";

                            DataTable datap1 = adapter.GetDatatable(sqlp1);
                            Parameter param1 = new Parameter { name = "anios" , data = new List<ParameterData>() };
                            foreach (var d in (from p in datap1.AsEnumerable() select p[@"anios"])){
                                ParameterData parameter = new ParameterData { name = Convert.ToString(d), value = Convert.ToString(d) };
                                param1.data.Add(parameter);
                            }

                            returnData = (Parameter)param1;
                            

                            break;
                        case 2:

                            String sqlp2 = @"SELECT     TOP (100) PERCENT [AgronetCadenas].consumidor.PreciosConsumidor.codigoCiudad_PreciosConsumidor, [AgronetCadenas].Leche.Divipola.nombreMunicipio
FROM         [AgronetCadenas].consumidor.PreciosConsumidor
INNER JOIN   [AgronetCadenas].Leche.Divipola ON [AgronetCadenas].consumidor.PreciosConsumidor.codigoCiudad_PreciosConsumidor = [AgronetCadenas].Leche.Divipola.codigoMunicipio 
GROUP BY [AgronetCadenas].consumidor.PreciosConsumidor.codigoCiudad_PreciosConsumidor, [AgronetCadenas].Leche.Divipola.nombreMunicipio 
ORDER BY [AgronetCadenas].Leche.Divipola.nombreMunicipio";

                            DataTable datap2 = adapter.GetDatatable(sqlp2);
                            Parameter param2 = new Parameter { name = "municipios" , data = new List<ParameterData>() };
                            foreach (var d in (from p in datap2.AsEnumerable() select p)){
                                ParameterData parameter = new ParameterData { name = Convert.ToString(d["nombreMunicipio"]), value = Convert.ToString(d["codigoCiudad_PreciosConsumidor"]) };
                                param2.data.Add(parameter);
                            }

                            returnData = (Parameter)param2;

                            break;
                        case 3:
                            String sqlp3 = @"USE AgronetCadenas;
SELECT     consumidor.PreciosConsumidor.codigoProducto_PreciosConsumidor, consumidor.producto.descripcion_Producto 
FROM         consumidor.PreciosConsumidor
INNER JOIN    consumidor.producto ON consumidor.PreciosConsumidor.codigoProducto_PreciosConsumidor = consumidor.producto.codigo_Producto
GROUP BY consumidor.PreciosConsumidor.codigoProducto_PreciosConsumidor, consumidor.producto.descripcion_Producto
HAVING      (consumidor.producto.descripcion_Producto LIKE N'leche%')";

                            DataTable datap3 = adapter.GetDatatable(sqlp3);
                            Parameter param3 = new Parameter { name = "productos" , data = new List<ParameterData>() };
                            foreach (var d in (from p in datap3.AsEnumerable() select p)){
                                ParameterData parameter = new ParameterData { name = Convert.ToString(d["descripcion_Producto"]), value = Convert.ToString(d["codigoProducto_PreciosConsumidor"]) };
                                param3.data.Add(parameter);
                            }

                            returnData = (Parameter)param3;

                            break;
                    }
                    break;
                case "grafico":
                    switch (parameters.id)
                    {
                        case 1:
                            DataTable results = adapter.GetDatatable(String.Format(@"SELECT producto.descripcion_Producto, PreciosConsumidor.fecha_PreciosConsumidor, Divipola.nombreMunicipio, PreciosConsumidor.precio_PreciosConsumidor
FROM   (AgronetCadenas.consumidor.PreciosConsumidor PreciosConsumidor
 INNER JOIN AgronetCadenas.Leche.Divipola Divipola ON PreciosConsumidor.codigoCiudad_PreciosConsumidor=Divipola.codigoMunicipio) 
 INNER JOIN AgronetCadenas.consumidor.producto producto ON PreciosConsumidor.codigoProducto_PreciosConsumidor=producto.codigo_Producto
WHERE PreciosConsumidor.fecha_PreciosConsumidor BETWEEN '{0}-01-01' AND '{1}-12-31'
AND  PreciosConsumidor.codigoProducto_PreciosConsumidor = {2}
AND PreciosConsumidor.codigoCiudad_PreciosConsumidor IN (" + string.Join(",", parameters.ciudad.Select(d => "'" + d + "'")) + @")
ORDER BY Divipola.nombreMunicipio, PreciosConsumidor.fecha_PreciosConsumidor", parameters.fecha_inicial, parameters.fecha_final, parameters.producto)); 
                            
                            Chart chart = new Chart { subtitle = "", series = new List<Series>() };
                            foreach (var innerGroup in (from d in results.AsEnumerable()
                                                        group d by d["nombreMunicipio"]))
                            {
                                var serie = new Series { name = innerGroup.Key.ToString(), data = new List<Data>() };
                                foreach (var element in innerGroup)
                                {
                                    var name = Convert.ToDateTime(element["fecha_PreciosConsumidor"]);
                                    var y = Convert.ToDouble(element["precio_PreciosConsumidor"]);
                                    var data = new Data { name = String.Format("{0:y}", name), y = y };
                                    serie.data.Add(data);
                                }
                                chart.series.Add(serie);

                            }
                            returnData = (Chart)chart;
                            break;
                    }
                    break;
                case "tabla":
                    switch (parameters.id)
                    {
                        case 1:
                            DataTable tableResults = adapter.GetDatatable(String.Format(@"SELECT producto.descripcion_Producto, PreciosConsumidor.fecha_PreciosConsumidor, Divipola.nombreMunicipio, PreciosConsumidor.precio_PreciosConsumidor
FROM   (AgronetCadenas.consumidor.PreciosConsumidor PreciosConsumidor
 INNER JOIN AgronetCadenas.Leche.Divipola Divipola ON PreciosConsumidor.codigoCiudad_PreciosConsumidor=Divipola.codigoMunicipio) 
 INNER JOIN AgronetCadenas.consumidor.producto producto ON PreciosConsumidor.codigoProducto_PreciosConsumidor=producto.codigo_Producto
WHERE PreciosConsumidor.fecha_PreciosConsumidor BETWEEN '{0}-01-01' AND '{1}-12-31'
AND  PreciosConsumidor.codigoProducto_PreciosConsumidor = {2}
AND PreciosConsumidor.codigoCiudad_PreciosConsumidor IN (" + string.Join(",", parameters.ciudad.Select(d => "'" + d + "'")) + @")
ORDER BY Divipola.nombreMunicipio, PreciosConsumidor.fecha_PreciosConsumidor", parameters.fecha_inicial, parameters.fecha_final, parameters.producto)); 

                            Table table = new Table { rows = tableResults };
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

        [Route("api/Report/307")]
        public IHttpActionResult postReport307(report307 parameters)
        {
            Object returnData = null;
            SQLAdapter adapter = new SQLAdapter();
            switch (parameters.tipo)
            {
                case "parametro":
                    switch (parameters.id)
                    {
                        case 1:

                            String sqlp1 = @"select 
                                            DISTINCT YEAR(fecha_PrecioGanadero) as anios
                                            from AgronetCadenas.compraLeche.precioGanaderoDepto 
                                            ORDER BY YEAR(fecha_PrecioGanadero)";

                            DataTable datap1 = adapter.GetDatatable(sqlp1);
                            Parameter param1 = new Parameter { name = "anios", data = new List<ParameterData>() };
                            foreach (var d in (from p in datap1.AsEnumerable() select p[@"anios"]))
                            {
                                ParameterData parameter = new ParameterData { name = Convert.ToString(d), value = Convert.ToString(d) };
                                param1.data.Add(parameter);
                            }

                            returnData = (Parameter)param1;


                            break;
                        case 2:

                            String sqlp2 = @"USE AgronetCadenas;
SELECT compraLeche.precioDepartamental.codigoDepartamento_PrecioDepartamental, Leche.regionDepartamento.descripcionDepartamento_RegionDepartamento 
FROM         compraLeche.precioDepartamental 
INNER JOIN   Leche.regionDepartamento ON compraLeche.precioDepartamental.codigoDepartamento_PrecioDepartamental = Leche.regionDepartamento.codigoDepartamento_RegionDepartamento 
GROUP BY compraLeche.precioDepartamental.codigoDepartamento_PrecioDepartamental, Leche.regionDepartamento.descripcionDepartamento_RegionDepartamento";

                            DataTable datap2 = adapter.GetDatatable(sqlp2);
                            Parameter param2 = new Parameter { name = "departamentos", data = new List<ParameterData>() };
                            foreach (var d in (from p in datap2.AsEnumerable() select p))
                            {
                                ParameterData parameter = new ParameterData { name = Convert.ToString(d["descripcionDepartamento_RegionDepartamento"]), value = Convert.ToString(d["codigoDepartamento_PrecioDepartamental"]) };
                                param2.data.Add(parameter);
                            }

                            returnData = (Parameter)param2;

                            break;

                    }
                    break;
                case "grafico":

                    String sqlGrafico1 = @"create table  #SP_PRECIOS_LECHE_GANADERO_DEPTO(
	                                fecha date,
	                                codigoDepartamento int,
	                                precio int,
	                                VariacionMesPrecio float,
	                                VariacionAnualPrecio float
                                )
                                insert into #SP_PRECIOS_LECHE_GANADERO_DEPTO EXEC [AgronetCadenas].[dbo].[SP_PRECIOS_LECHE_GANADERO_DEPTO]
		                                @Fecha_inicial = N'" + parameters.fecha_inicial + @"-01-01',
		                                @Fecha_final = N'" + parameters.fecha_final + @"-12-31'

                                SELECT 
                                regionDepartamento.descripcionDepartamento_RegionDepartamento as departamento, 
                                regionDepartamento.codigoDepartamento_RegionDepartamento as codigoDepartamento,
                                #SP_PRECIOS_LECHE_GANADERO_DEPTO.fecha as fecha,
                                ISNULL(#SP_PRECIOS_LECHE_GANADERO_DEPTO.precio,0) as precio,
                                ISNULL(#SP_PRECIOS_LECHE_GANADERO_DEPTO.VariacionMesPrecio,0) as variacionPrecio,
                                ISNULL(#SP_PRECIOS_LECHE_GANADERO_DEPTO.VariacionAnualPrecio,0) as variacionVolumen
                                FROM   AgronetCadenas.Leche.regionDepartamento regionDepartamento INNER JOIN #SP_PRECIOS_LECHE_GANADERO_DEPTO 
                                ON #SP_PRECIOS_LECHE_GANADERO_DEPTO.codigoDepartamento = regionDepartamento.codigoDepartamento_RegionDepartamento 
                                WHERE #SP_PRECIOS_LECHE_GANADERO_DEPTO.fecha between '" + parameters.fecha_inicial + @"-01-01' and '" + parameters.fecha_final + @"-12-31'
                                 and  #SP_PRECIOS_LECHE_GANADERO_DEPTO.codigoDepartamento in (" + string.Join(",", parameters.departamento.Select(d => "'" + d + "'")) + @")
                                DROP TABLE #SP_PRECIOS_LECHE_GANADERO_DEPTO";


                    DataTable datatable = adapter.GetDatatable(sqlGrafico1);
                    var dataGroups = from r in datatable.AsEnumerable()
                                     group r by r["departamento"] into seriesGroup
                                     select seriesGroup;

                    switch (parameters.id)
                    {
                        case 1:

                            Chart chart1 = new Chart
                            {
                                subtitle = @"Precio de compra de leche cruda al productor con ‎bonificaciones voluntarias por departamento",
                                series = new List<Series>()
                            };

                            foreach (var dataGroup in dataGroups)
                            {
                                var serie = new Series { name = dataGroup.Key.ToString(), data = new List<Data>() };

                                foreach (var seriesData in dataGroup)
                                {
                                    var name = Convert.ToDateTime(seriesData["fecha"]);
                                    var y = Convert.ToDouble(seriesData["precio"]);
                                    var data = new Data { name = String.Format("{0:y}", name), y = y };
                                    serie.data.Add(data);
                                }
                                chart1.series.Add(serie);
                            }

                            returnData = (Chart)chart1;

                            break;

                    }
                    break;
                case "tabla":



                    switch (parameters.id)
                    {
                        case 1:

                            String sqlTabla1 = @"create table  #SP_PRECIOS_LECHE_GANADERO_DEPTO(
	                                                fecha date,
	                                                codigoDepartamento int,
	                                                precio int,
	                                                VariacionMesPrecio float,
	                                                VariacionAnualPrecio float
                                                )
                                                insert into #SP_PRECIOS_LECHE_GANADERO_DEPTO EXEC [AgronetCadenas].[dbo].[SP_PRECIOS_LECHE_GANADERO_DEPTO]
		                                                @Fecha_inicial = N'" + parameters.fecha_inicial + @"-01-01',
		                                                @Fecha_final = N'" + parameters.fecha_final + @"-12-31'

                                                SELECT 
                                                regionDepartamento.descripcionDepartamento_RegionDepartamento as departamento, 
                                                regionDepartamento.codigoDepartamento_RegionDepartamento as codigoDepartamento,
                                                #SP_PRECIOS_LECHE_GANADERO_DEPTO.fecha as fecha,
                                                ISNULL(#SP_PRECIOS_LECHE_GANADERO_DEPTO.precio,0) as precio,
                                                ISNULL(#SP_PRECIOS_LECHE_GANADERO_DEPTO.VariacionMesPrecio,0) as variacionPrecio,
                                                ISNULL(#SP_PRECIOS_LECHE_GANADERO_DEPTO.VariacionAnualPrecio,0) as variacionVolumen
                                                FROM   AgronetCadenas.Leche.regionDepartamento regionDepartamento INNER JOIN #SP_PRECIOS_LECHE_GANADERO_DEPTO 
                                                ON #SP_PRECIOS_LECHE_GANADERO_DEPTO.codigoDepartamento = regionDepartamento.codigoDepartamento_RegionDepartamento 
                                                WHERE #SP_PRECIOS_LECHE_GANADERO_DEPTO.fecha between '" + parameters.fecha_inicial + @"-01-01' and '" + parameters.fecha_final + @"-12-31'
                                                    and #SP_PRECIOS_LECHE_GANADERO_DEPTO.codigoDepartamento in (" + string.Join(",", parameters.departamento.Select(d => "'" + d + "'")) + @")
                                                DROP TABLE #SP_PRECIOS_LECHE_GANADERO_DEPTO";

                            DataTable data = adapter.GetDatatable(sqlTabla1);
                            Table table = new Table { rows = data };
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

        [Route("api/Report/308")]
        public IHttpActionResult postReport308(report308 parameters)
        {
            Object returnData = null;
            SQLAdapter adapter = new SQLAdapter();
            switch (parameters.tipo)
            {
                case "parametro":
                    switch (parameters.id)
                    {
                        case 1:

                            String sqlp1 = @"select 
                                            DISTINCT YEAR(fecha_PrecioGanadero) as anios
                                            from AgronetCadenas.compraLeche.precioGanaderoDepto 
                                            ORDER BY YEAR(fecha_PrecioGanadero)";

                            DataTable datap1 = adapter.GetDatatable(sqlp1);
                            Parameter param1 = new Parameter { name = "anios", data = new List<ParameterData>() };
                            foreach (var d in (from p in datap1.AsEnumerable() select p[@"anios"]))
                            {
                                ParameterData parameter = new ParameterData { name = Convert.ToString(d), value = Convert.ToString(d) };
                                param1.data.Add(parameter);
                            }

                            returnData = (Parameter)param1;

                            break;

                    }
                    break;
                case "grafico":

                    String sqlGrafico1 = @"create table  #SP_PRECIOS_LECHE_GANADERO_REGION(
	fecha date,
	codigoRegion int,
	precio int,
	variacionPrecio float,
	variacionVolumen float
)
insert into #SP_PRECIOS_LECHE_GANADERO_REGION EXEC [AgronetCadenas].[dbo].[SP_PRECIOS_LECHE_GANADERO_REGION]
		@Fecha_inicial = N'"+parameters.fecha_inicial+@"-01-01',
		@Fecha_final = N'"+parameters.fecha_final+@"-12-31'

SELECT region.descripcion_Region as region,
	#SP_PRECIOS_LECHE_GANADERO_REGION.fecha as fecha,
	#SP_PRECIOS_LECHE_GANADERO_REGION.precio as precio,
	ISNULL(#SP_PRECIOS_LECHE_GANADERO_REGION.variacionPrecio,0) as variacionPrecio,
	ISNULL(#SP_PRECIOS_LECHE_GANADERO_REGION.variacionVolumen,0) as variacionVolumen
 FROM AgronetCadenas.Leche.region region INNER JOIN #SP_PRECIOS_LECHE_GANADERO_REGION 
 ON #SP_PRECIOS_LECHE_GANADERO_REGION.codigoRegion = region.codigo_Region
 WHERE #SP_PRECIOS_LECHE_GANADERO_REGION.fecha between '" + parameters.fecha_inicial + @"-01-01' and '" + parameters.fecha_final + @"-12-31'

DROP TABLE #SP_PRECIOS_LECHE_GANADERO_REGION";


                    DataTable datatable = adapter.GetDatatable(sqlGrafico1);
                    var dataGroups = from r in datatable.AsEnumerable()
                                     group r by r["region"] into seriesGroup
                                     select seriesGroup;

                    switch (parameters.id)
                    {
                        case 1:

                            Chart chart1 = new Chart
                            {
                                subtitle = @"Precio de compra de leche cruda al productor con ‎bonificaciones voluntarias por región‎",
                                series = new List<Series>()
                            };

                            foreach (var dataGroup in dataGroups)
                            {
                                var serie = new Series { name = dataGroup.Key.ToString(), data = new List<Data>() };

                                foreach (var seriesData in dataGroup)
                                {
                                    var name = Convert.ToDateTime(seriesData["fecha"]);
                                    var y = Convert.ToDouble(seriesData["precio"]);
                                    var data = new Data { name = String.Format("{0:y}", name), y = y };
                                    serie.data.Add(data);
                                }
                                chart1.series.Add(serie);
                            }

                            returnData = (Chart)chart1;

                            break;

                    }
                    break;
                case "tabla":



                    switch (parameters.id)
                    {
                        case 1:

                            String sqlTabla1 = @"create table  #SP_PRECIOS_LECHE_GANADERO_REGION(
	                                        fecha date,
	                                        codigoRegion int,
	                                        precio int,
	                                        variacionPrecio float,
	                                        variacionVolumen float
                                        )
                                        insert into #SP_PRECIOS_LECHE_GANADERO_REGION EXEC [AgronetCadenas].[dbo].[SP_PRECIOS_LECHE_GANADERO_REGION]
		                                        @Fecha_inicial = N'" + parameters.fecha_inicial + @"-01-01',
		                                        @Fecha_final = N'" + parameters.fecha_final + @"-12-31'

                                        SELECT region.descripcion_Region,
	                                        #SP_PRECIOS_LECHE_GANADERO_REGION.fecha,
	                                        #SP_PRECIOS_LECHE_GANADERO_REGION.precio,
	                                        ISNULL(#SP_PRECIOS_LECHE_GANADERO_REGION.variacionPrecio,0) as variacionPrecio,
	                                        ISNULL(#SP_PRECIOS_LECHE_GANADERO_REGION.variacionVolumen,0) as variacionVolumen
                                         FROM AgronetCadenas.Leche.region region INNER JOIN #SP_PRECIOS_LECHE_GANADERO_REGION 
                                         ON #SP_PRECIOS_LECHE_GANADERO_REGION.codigoRegion = region.codigo_Region
                                         WHERE #SP_PRECIOS_LECHE_GANADERO_REGION.fecha between '" + parameters.fecha_inicial + @"-01-01' 
                                        and '" + parameters.fecha_final + @"-12-31'

                                        DROP TABLE #SP_PRECIOS_LECHE_GANADERO_REGION";


                            DataTable data = adapter.GetDatatable(sqlTabla1);
                            Table table = new Table { rows = data };
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

    }
}
