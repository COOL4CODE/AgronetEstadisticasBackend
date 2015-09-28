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
    public class C7Controller : ApiController
    {
        [Route("api/Report/701")]
        public IHttpActionResult postReport701(report701 parameters)
        {
            Object returnData = null;
            SQLAdapter adapter = new SQLAdapter();

            switch (parameters.tipo)
            {
                case "parametro":
                    Parameter param = new Parameter { data = new List<ParameterData>() };
                    switch (parameters.id)
                    {
                        case 1:
                            param.name = "anio";
                            foreach (var d in (from p in adapter.GetDatatable(@"select DISTINCT YEAR(AgronetPesca.dbo.Volumen_Pesca.fechaMensual_VolumenPesca) as anho  
                                                                                from AgronetPesca.dbo.Volumen_Pesca 
                                                                                order by YEAR(AgronetPesca.dbo.Volumen_Pesca.fechaMensual_VolumenPesca);").AsEnumerable()
                                               select p))
                            {
                                ParameterData parameter = new ParameterData { name = Convert.ToString(d["anho"]), value = Convert.ToString(d["anho"]) };
                                param.data.Add(parameter);
                            }
                            returnData = (Parameter)param;
                            break;
                        case 2:
                            param.name = "especie";
                            foreach (var d in (from p in adapter.GetDatatable(@"SELECT DISTINCT AgronetPesca.dbo.Volumen_Pesca.codigoProducto_VolumenPesca as codigo, 
                                                                                (nombreComun_Producto + ' - ' + nombreCientifico_Producto) as descripcion 
                                                                                FROM  AgronetPesca.dbo.Volumen_Pesca 
                                                                                INNER JOIN AgronetPesca.dbo.Volumen_Productos 
                                                                                ON AgronetPesca.dbo.Volumen_Pesca.codigoProducto_VolumenPesca = AgronetPesca.dbo.Volumen_Productos.codigo_Producto 
                                                                                WHERE AgronetPesca.dbo.Volumen_Productos.nombreCientifico_Producto is not null
                                                                                GROUP BY AgronetPesca.dbo.Volumen_Pesca.codigoProducto_VolumenPesca, (nombreComun_Producto + ' - ' + nombreCientifico_Producto) 
                                                                                ORDER BY (nombreComun_Producto + ' - ' + nombreCientifico_Producto);").AsEnumerable()
                                               select p))
                            {
                                ParameterData parameter = new ParameterData { name = Convert.ToString(d["descripcion"]), value = Convert.ToString(d["codigo"]) };
                                param.data.Add(parameter);
                            }
                            returnData = (Parameter)param;
                            break;
                        case 3:
                            param.name = "municipio";
                            foreach (var d in (from p in adapter.GetDatatable(String.Format(@"SELECT DISTINCT
                                                                                AgronetPesca.dbo.Volumen_Pesca.codigoMunicipio_VolumenPesca as codigo,
                                                                                AgronetPesca.dbo.Divipola_5Digitos.nombreMunicipio + ' (' + AgronetPesca.dbo.Departamentos.nombreDepartamento + ')' AS descripcion
                                                                                FROM AgronetPesca.dbo.Volumen_Pesca 
                                                                                INNER JOIN 
                                                                                AgronetPesca.dbo.Divipola_5Digitos 
                                                                                ON AgronetPesca.dbo.Volumen_Pesca.codigoMunicipio_VolumenPesca = 
                                                                                AgronetPesca.dbo.Divipola_5Digitos.CodMunicipio 
                                                                                INNER JOIN 
                                                                                AgronetPesca.dbo.Departamentos 
                                                                                ON AgronetPesca.dbo.Divipola_5Digitos.codigoDepartamento = 
                                                                                AgronetPesca.dbo.Departamentos.codigoDepartamento 
                                                                                WHERE
                                                                                --PARAMETROS 
                                                                                AgronetPesca.dbo.Volumen_Pesca.fechaMensual_VolumenPesca >= '{0}-01-01' 
                                                                                AND AgronetPesca.dbo.Volumen_Pesca.fechaMensual_VolumenPesca <= '{1}-12-31'
                                                                                AND AgronetPesca.dbo.Volumen_Pesca.codigoProducto_VolumenPesca = {2}
                                                                                GROUP BY 
                                                                                AgronetPesca.dbo.Volumen_Pesca.fechaMensual_VolumenPesca,
                                                                                AgronetPesca.dbo.Volumen_Pesca.codigoMunicipio_VolumenPesca, 
                                                                                AgronetPesca.dbo.Divipola_5Digitos.nombreMunicipio 
                                                                                + ' (' + AgronetPesca.dbo.Departamentos.nombreDepartamento + ')'", parameters.fecha_inicial, parameters.fecha_final, parameters.especie)).AsEnumerable()
                                               select p))
                            {
                                ParameterData parameter = new ParameterData { name = Convert.ToString(d["descripcion"]), value = Convert.ToString(d["codigo"]) };
                                param.data.Add(parameter);
                            }
                            returnData = (Parameter)param;
                            break;
                    }
                    break;
                case "grafico":
                    DataTable results = adapter.GetDatatable(String.Format(@"CREATE TABLE #SP_VOLUMENPESCA_ESPECIE(
                                                                            fecha date,
                                                                            codigoCuenca int,
                                                                            codigoTipoPesca int,
                                                                            codigoMunicipio int,
                                                                            codigoProducto int,
                                                                            precio float,
                                                                            variacionMesPrecio float
                                                                            )
                                                                            insert into #SP_VOLUMENPESCA_ESPECIE EXEC AgronetPesca.dbo.SP_VOLUMENPESCA_ESPECIE
                                                                            @Fecha_inicial = N'{0}-01-01',
                                                                            @Fecha_final = N'{1}-12-31',
                                                                            @especie = {2}

                                                                            SELECT YEAR(#SP_VOLUMENPESCA_ESPECIE.fecha) fecha,
                                                                            AgronetPesca.dbo.Volumen_Productos.nombreComun_Producto,
                                                                            AgronetPesca.dbo.Divipola_5Digitos.nombreMunicipio,
                                                                            SUM(#SP_VOLUMENPESCA_ESPECIE.precio) precio
                                                                            FROM #SP_VOLUMENPESCA_ESPECIE 
                                                                            INNER JOIN AgronetPesca.dbo.Volumen_Productos ON AgronetPesca.dbo.Volumen_Productos.codigo_Producto = #SP_VOLUMENPESCA_ESPECIE.codigoProducto 
                                                                            INNER JOIN AgronetPesca.dbo.Divipola_5Digitos ON AgronetPesca.dbo.Divipola_5Digitos.CodMunicipio = #SP_VOLUMENPESCA_ESPECIE.codigoMunicipio
                                                                            WHERE #SP_VOLUMENPESCA_ESPECIE.codigoMunicipio IN (" + string.Join(",", parameters.municipio.Select(d => "'" + d + "'")) + @")
                                                                            GROUP BY YEAR(#SP_VOLUMENPESCA_ESPECIE.fecha),
                                                                            AgronetPesca.dbo.Volumen_Productos.nombreComun_Producto,
                                                                            AgronetPesca.dbo.Divipola_5Digitos.nombreMunicipio
                                                                            ORDER BY YEAR(#SP_VOLUMENPESCA_ESPECIE.fecha),
                                                                            AgronetPesca.dbo.Volumen_Productos.nombreComun_Producto,
                                                                            AgronetPesca.dbo.Divipola_5Digitos.nombreMunicipio
                                                                            DROP TABLE #SP_VOLUMENPESCA_ESPECIE;", parameters.fecha_inicial, parameters.fecha_final, parameters.especie));

                    Chart chart = new Chart { series = new List<Series>() };
                    switch (parameters.id)
                    {
                        case 1:
                            chart.subtitle = "";
                            foreach (var productGroup in (from r in results.AsEnumerable()
                                                          group r by r["nombreMunicipio"]))
                            {
                                var serie = new Series { name = productGroup.Key.ToString().Trim(), data = new List<Data>() };
                                foreach (var anioData in productGroup)
                                {
                                    var data = new Data { name = Convert.ToString(anioData["fecha"]), y = Convert.ToDouble(anioData["precio"]) };
                                    serie.data.Add(data);

                                }
                                chart.series.Add(serie);
                            }

                            returnData = (Chart)chart;
                            break;
                    }
                    break;
                case "tabla":
                    DataTable tableResults = adapter.GetDatatable(String.Format(@"CREATE TABLE #SP_VOLUMENPESCA_ESPECIE(
                                                                            fecha date,
                                                                            codigoCuenca int,
                                                                            codigoTipoPesca int,
                                                                            codigoMunicipio int,
                                                                            codigoProducto int,
                                                                            precio float,
                                                                            variacionMesPrecio float
                                                                            )
                                                                            insert into #SP_VOLUMENPESCA_ESPECIE EXEC AgronetPesca.dbo.SP_VOLUMENPESCA_ESPECIE
                                                                            @Fecha_inicial = N'{0}-01-01',
                                                                            @Fecha_final = N'{1}-12-31',
                                                                            @especie = {2}

                                                                            SELECT #SP_VOLUMENPESCA_ESPECIE.fecha,
                                                                            AgronetPesca.dbo.Volumen_Productos.nombreComun_Producto,
                                                                            AgronetPesca.dbo.Divipola_5Digitos.nombreMunicipio,
                                                                            AgronetPesca.dbo.Volumen_Cuenca.Descripcion_Cuenca,
                                                                            AgronetPesca.dbo.Volumen_TipoPesca.Descripcion_TipoPesca,
                                                                            #SP_VOLUMENPESCA_ESPECIE.precio
                                                                            FROM #SP_VOLUMENPESCA_ESPECIE 
                                                                            INNER JOIN AgronetPesca.dbo.Volumen_Productos ON AgronetPesca.dbo.Volumen_Productos.codigo_Producto = #SP_VOLUMENPESCA_ESPECIE.codigoProducto 
                                                                            INNER JOIN AgronetPesca.dbo.Divipola_5Digitos ON AgronetPesca.dbo.Divipola_5Digitos.CodMunicipio = #SP_VOLUMENPESCA_ESPECIE.codigoMunicipio
                                                                            INNER JOIN AgronetPesca.dbo.Volumen_Cuenca ON AgronetPesca.dbo.Volumen_Cuenca.codigo_Cuenca = #SP_VOLUMENPESCA_ESPECIE.codigoCuenca
                                                                            INNER JOIN AgronetPesca.dbo.Volumen_TipoPesca ON AgronetPesca.dbo.Volumen_TipoPesca.Codigo_TipoPesca = #SP_VOLUMENPESCA_ESPECIE.codigoTipoPesca
                                                                            WHERE #SP_VOLUMENPESCA_ESPECIE.codigoMunicipio IN (" + string.Join(",", parameters.municipio.Select(d => "'" + d + "'")) + @")
                                                                            ORDER BY #SP_VOLUMENPESCA_ESPECIE.fecha,
                                                                            AgronetPesca.dbo.Volumen_Productos.nombreComun_Producto,
                                                                            AgronetPesca.dbo.Divipola_5Digitos.nombreMunicipio,
                                                                            AgronetPesca.dbo.Volumen_TipoPesca.Descripcion_TipoPesca
                                                                            DROP TABLE #SP_VOLUMENPESCA_ESPECIE;", parameters.fecha_inicial, parameters.fecha_final, parameters.especie));

                    switch (parameters.id)
                    {
                        case 1:
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

        [Route("api/Report/702")]
        public IHttpActionResult postReport702(report702 parameters)
        {
            Object returnData = null;
            SQLAdapter adapter = new SQLAdapter();

            switch (parameters.tipo)
            {
                case "parametro":
                    Parameter param = new Parameter { data = new List<ParameterData>() };
                    switch (parameters.id)
                    {
                        case 1:
                            param.name = "anio";
                            foreach (var d in (from p in adapter.GetDatatable(@"select DISTINCT YEAR(AgronetPesca.dbo.Volumen_Pesca.fechaMensual_VolumenPesca) as anho  
                                                                                from AgronetPesca.dbo.Volumen_Pesca 
                                                                                order by YEAR(AgronetPesca.dbo.Volumen_Pesca.fechaMensual_VolumenPesca);").AsEnumerable()
                                               select p))
                            {
                                ParameterData parameter = new ParameterData { name = Convert.ToString(d["anho"]), value = Convert.ToString(d["anho"]) };
                                param.data.Add(parameter);
                            }
                            returnData = (Parameter)param;
                            break;                      
                        case 2:
                            param.name = "municipio";
                            foreach (var d in (from p in adapter.GetDatatable(String.Format(@"SELECT DISTINCT
                                                                                AgronetPesca.dbo.Volumen_Pesca.codigoMunicipio_VolumenPesca as codigo,
                                                                                AgronetPesca.dbo.Divipola_5Digitos.nombreMunicipio + ' (' + AgronetPesca.dbo.Departamentos.nombreDepartamento + ')' AS descripcion
                                                                                FROM AgronetPesca.dbo.Volumen_Pesca 
                                                                                INNER JOIN 
                                                                                AgronetPesca.dbo.Divipola_5Digitos 
                                                                                ON AgronetPesca.dbo.Volumen_Pesca.codigoMunicipio_VolumenPesca = 
                                                                                AgronetPesca.dbo.Divipola_5Digitos.CodMunicipio 
                                                                                INNER JOIN 
                                                                                AgronetPesca.dbo.Departamentos 
                                                                                ON AgronetPesca.dbo.Divipola_5Digitos.codigoDepartamento = 
                                                                                AgronetPesca.dbo.Departamentos.codigoDepartamento 
                                                                                WHERE
                                                                                --PARAMETROS 
                                                                                AgronetPesca.dbo.Volumen_Pesca.fechaMensual_VolumenPesca >= '{0}-01-01' 
                                                                                AND AgronetPesca.dbo.Volumen_Pesca.fechaMensual_VolumenPesca <= '{1}-12-31'
                                                                                GROUP BY 
                                                                                AgronetPesca.dbo.Volumen_Pesca.fechaMensual_VolumenPesca,
                                                                                AgronetPesca.dbo.Volumen_Pesca.codigoMunicipio_VolumenPesca, 
                                                                                AgronetPesca.dbo.Divipola_5Digitos.nombreMunicipio 
                                                                                + ' (' + AgronetPesca.dbo.Departamentos.nombreDepartamento + ')'", parameters.fecha_inicial, parameters.fecha_final)).AsEnumerable()
                                               select p))
                            {
                                ParameterData parameter = new ParameterData { name = Convert.ToString(d["descripcion"]), value = Convert.ToString(d["codigo"]) };
                                param.data.Add(parameter);
                            }
                            returnData = (Parameter)param;
                            break;
                        case 3:
                            param.name = "tipo_pesca";
                            foreach (var d in (from p in adapter.GetDatatable(@"SELECT [Codigo_TipoPesca] as codigo ,
                                                                                [Descripcion_TipoPesca] as descripcion 
                                                                                FROM [AgronetPesca].[dbo].[Volumen_TipoPesca];").AsEnumerable()
                                               select p))
                            {
                                ParameterData parameter = new ParameterData { name = Convert.ToString(d["descripcion"]), value = Convert.ToString(d["codigo"]) };
                                param.data.Add(parameter);
                            }
                            returnData = (Parameter)param;
                            break;
                        case 4:
                            param.name = "especie";
                            foreach (var d in (from p in adapter.GetDatatable(String.Format(@"USE AgronetPesca
                                                                                SELECT distinct dbo.Volumen_Pesca.codigoProducto_VolumenPesca as codigo, 
                                                                                dbo.Volumen_Productos.nombreComun_Producto as descripcion 
                                                                                FROM dbo.Volumen_Pesca 
                                                                                INNER JOIN dbo.Divipola_5Digitos 
                                                                                ON dbo.Volumen_Pesca.codigoMunicipio_VolumenPesca = dbo.Divipola_5Digitos.CodMunicipio 
                                                                                INNER JOIN   dbo.Departamentos 
                                                                                ON dbo.Divipola_5Digitos.codigoDepartamento = dbo.Departamentos.codigoDepartamento 
                                                                                INNER JOIN  dbo.Volumen_Productos 
                                                                                ON dbo.Volumen_Pesca.codigoProducto_VolumenPesca = dbo.Volumen_Productos.codigo_Producto 
                                                                                WHERE     
                                                                                --parametros
                                                                                dbo.Volumen_Pesca.fechaMensual_VolumenPesca >= '{0}-01-01' 
                                                                                AND  dbo.Volumen_Pesca.fechaMensual_VolumenPesca <= '{1}-12-31' 
                                                                                AND  dbo.Volumen_Pesca.codigoTipoPesca_VolumenPesca = {2} 
                                                                                AND dbo.Volumen_Pesca.codigoMunicipio_VolumenPesca = {3} 
                                                                                GROUP BY dbo.Volumen_Pesca.codigoProducto_VolumenPesca, dbo.Volumen_Productos.nombreComun_Producto 
                                                                                ORDER BY dbo.Volumen_Productos.nombreComun_Producto;", parameters.fecha_inicial, parameters.fecha_final, parameters.tipo_pesca, parameters.municipio)).AsEnumerable()
                                               select p))
                            {
                                ParameterData parameter = new ParameterData { name = Convert.ToString(d["descripcion"]), value = Convert.ToString(d["codigo"]) };
                                param.data.Add(parameter);
                            }
                            returnData = (Parameter)param;
                            break;
                    }
                    break;
                case "grafico":
                    DataTable results = adapter.GetDatatable(String.Format(@"create table  #SP_VOLUMENPESCA_MUNICIPIO(
	                                                                            fecha date,
	                                                                            codigoCuenca int,
	                                                                            codigoTipoPesca int,
	                                                                            codigoMunicipio int,
	                                                                            codigoProducto int,
	                                                                            peso float,
	                                                                            variacionMesPrecio float
                                                                            )
                                                                            insert into #SP_VOLUMENPESCA_MUNICIPIO EXEC [AgronetPesca].[dbo].[SP_VOLUMENPESCA_MUNICIPIO]
		                                                                            @Fecha_inicial = N'{0}-01-01',
		                                                                            @Fecha_final = N'{1}-12-31',
		                                                                            @municipio = {2}
                                                                            select 
                                                                            YEAR(#SP_VOLUMENPESCA_MUNICIPIO.fecha) fecha,
                                                                            AgronetPesca.dbo.Divipola_5Digitos.nombreMunicipio,
                                                                            AgronetPesca.dbo.Volumen_Productos.nombreComun_Producto,
                                                                            SUM(#SP_VOLUMENPESCA_MUNICIPIO.peso) peso
                                                                            from #SP_VOLUMENPESCA_MUNICIPIO 
                                                                            inner join AgronetPesca.dbo.Volumen_Productos ON AgronetPesca.dbo.Volumen_Productos.codigo_Producto = #SP_VOLUMENPESCA_MUNICIPIO.codigoProducto 
                                                                            INNER JOIN AgronetPesca.dbo.Divipola_5Digitos ON #SP_VOLUMENPESCA_MUNICIPIO.codigoMunicipio = AgronetPesca.dbo.Divipola_5Digitos.CodMunicipio 
                                                                            WHERE #SP_VOLUMENPESCA_MUNICIPIO.codigoProducto IN (" + string.Join(",", parameters.especie.Select(d => "'" + d + "'")) + @")
                                                                            AND #SP_VOLUMENPESCA_MUNICIPIO.codigoTipoPesca = {3}
                                                                            GROUP BY YEAR(#SP_VOLUMENPESCA_MUNICIPIO.fecha),
                                                                            AgronetPesca.dbo.Divipola_5Digitos.nombreMunicipio,
                                                                            AgronetPesca.dbo.Volumen_Productos.nombreComun_Producto
                                                                            ORDER BY YEAR(#SP_VOLUMENPESCA_MUNICIPIO.fecha),
                                                                            AgronetPesca.dbo.Divipola_5Digitos.nombreMunicipio,
                                                                            AgronetPesca.dbo.Volumen_Productos.nombreComun_Producto
                                                                            DROP TABLE #SP_VOLUMENPESCA_MUNICIPIO", parameters.fecha_inicial, parameters.fecha_final, parameters.municipio, parameters.tipo_pesca));

                    Chart chart = new Chart { series = new List<Series>() };
                    switch (parameters.id)
                    {
                        case 1:
                            chart.subtitle = "";
                            foreach (var productGroup in (from r in results.AsEnumerable()
                                                          group r by r["nombreComun_Producto"]))
                            {
                                var serie = new Series { name = productGroup.Key.ToString().Trim(), data = new List<Data>() };
                                foreach (var anioData in productGroup)
                                {
                                    var data = new Data { name = Convert.ToString(anioData["fecha"]), y = Convert.ToDouble(anioData["peso"]) };
                                    serie.data.Add(data);

                                }
                                chart.series.Add(serie);
                            }

                            returnData = (Chart)chart;
                            break;
                    }
                    break;
                case "tabla":
                    DataTable tableResults = adapter.GetDatatable(String.Format(@"create table  #SP_VOLUMENPESCA_MUNICIPIO(
	                                                                                fecha date,
	                                                                                codigoCuenca int,
	                                                                                codigoTipoPesca int,
	                                                                                codigoMunicipio int,
	                                                                                codigoProducto int,
	                                                                                peso float,
	                                                                                variacionMesPrecio float
                                                                                )
                                                                                insert into #SP_VOLUMENPESCA_MUNICIPIO EXEC [AgronetPesca].[dbo].[SP_VOLUMENPESCA_MUNICIPIO]
		                                                                                @Fecha_inicial = N'{0}-01-01',
		                                                                                @Fecha_final = N'{1}-12-31',
		                                                                                @municipio = {2}
                                                                                select 
                                                                                #SP_VOLUMENPESCA_MUNICIPIO.fecha,
                                                                                AgronetPesca.dbo.Divipola_5Digitos.nombreMunicipio,
                                                                                AgronetPesca.dbo.Volumen_Productos.nombreComun_Producto,
                                                                                AgronetPesca.dbo.Volumen_Cuenca.Descripcion_Cuenca,
                                                                                AgronetPesca.dbo.Volumen_TipoPesca.Descripcion_TipoPesca,
                                                                                #SP_VOLUMENPESCA_MUNICIPIO.peso
                                                                                from #SP_VOLUMENPESCA_MUNICIPIO 
                                                                                inner join AgronetPesca.dbo.Volumen_Productos ON AgronetPesca.dbo.Volumen_Productos.codigo_Producto = #SP_VOLUMENPESCA_MUNICIPIO.codigoProducto 
                                                                                INNER JOIN AgronetPesca.dbo.Divipola_5Digitos ON #SP_VOLUMENPESCA_MUNICIPIO.codigoMunicipio = AgronetPesca.dbo.Divipola_5Digitos.CodMunicipio 

                                                                                INNER JOIN AgronetPesca.dbo.Volumen_Cuenca ON AgronetPesca.dbo.Volumen_Cuenca.codigo_Cuenca = #SP_VOLUMENPESCA_MUNICIPIO.codigoCuenca
                                                                                INNER JOIN AgronetPesca.dbo.Volumen_TipoPesca ON AgronetPesca.dbo.Volumen_TipoPesca.Codigo_TipoPesca = #SP_VOLUMENPESCA_MUNICIPIO.codigoTipoPesca
                                                                                where #SP_VOLUMENPESCA_MUNICIPIO.codigoProducto IN (" + string.Join(",", parameters.especie.Select(d => "'" + d + "'")) + @")
                                                                                AND #SP_VOLUMENPESCA_MUNICIPIO.codigoTipoPesca = {3}
                                                                                ORDER BY #SP_VOLUMENPESCA_MUNICIPIO.fecha,
                                                                            AgronetPesca.dbo.Divipola_5Digitos.nombreMunicipio,
                                                                            AgronetPesca.dbo.Volumen_Productos.nombreComun_Producto
                                                                                DROP TABLE #SP_VOLUMENPESCA_MUNICIPIO;", parameters.fecha_inicial, parameters.fecha_final, parameters.municipio, parameters.tipo_pesca));

                    switch (parameters.id)
                    {
                        case 1:
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
    
        [Route("api/Report/708")]
        public IHttpActionResult postReport708(report708 parameters)
        {
            Object returnData = null;
            PostgresqlAdapter adapter = new PostgresqlAdapter();
            switch (parameters.tipo)
            {
                case "parametro":
                    Parameter param = new Parameter { data = new List<ParameterData>() };
                    switch (parameters.id)
                    {
                        case 1:
                            param.name = "departamento";
                            foreach (var d in (from p in adapter.GetDataTable(@"SELECT DISTINCT est.departamento FROM clima.estaciontexto est ORDER BY est.departamento").AsEnumerable()
                                               select p))
                            {
                                ParameterData parameter = new ParameterData { name = Convert.ToString(d["departamento"]), value = Convert.ToString(d["departamento"]) };
                                param.data.Add(parameter);
                            }
                            returnData = (Parameter)param;
                            break;
                        case 2:
                            param.name = "municipio";
                            foreach (var d in (from p in adapter.GetDataTable(@"SELECT DISTINCT est.municipio FROM clima.estaciontexto est
                                                                                WHERE est.departamento IN (" + string.Join(",", parameters.departamento.Select(d => "'" + d + "'")) + @") ORDER BY est.municipio").AsEnumerable()
                                               select p))
                            {
                                ParameterData parameter = new ParameterData { name = Convert.ToString(d["municipio"]), value = Convert.ToString(d["municipio"]) };
                                param.data.Add(parameter);
                            }
                            returnData = (Parameter)param;
                            break;
                       case 3:
                            param.name = "estacion";
                            foreach (var d in (from p in adapter.GetDataTable(@"SELECT DISTINCT est.codigo, est.nombre nombreestacion FROM clima.estaciontexto est
                                                                                WHERE est.municipio IN  (" + string.Join(",", parameters.municipio.Select(d => "'" + d + "'")) + @") ORDER BY est.nombre").AsEnumerable()
                                               select p))
                            {
                                ParameterData parameter = new ParameterData { name = Convert.ToString(d["nombreestacion"]), value = Convert.ToString(d["codigo"]) };
                                param.data.Add(parameter);
                            }
                            returnData = (Parameter)param;
                            break;
                    }
                    break;
                case "grafico":
                    Chart chart = new Chart { series = new List<Series>() };
                    switch (parameters.id)
                    {
                        case 1:
                            DataTable results = adapter.GetDataTable(@"SELECT est.departamento, est.municipio, est.nombre nombreestacion, pre.* FROM 
                                                                       clima.precipitacionperiodoreferenciaestacion pre
                                                                       INNER JOIN clima.estaciontexto est ON pre.codigo = est.codigo
                                                                       WHERE est.codigo IN (" + string.Join(",", parameters.estacion.Select(d => "'" + d + "'")) + @") ORDER BY est.nombre");

                            chart.subtitle = "";
                            foreach (var d in (from g in results.AsEnumerable() select g))
                            {
                                Series serie = new Series { name = Convert.ToString(d["nombreestacion"]) };

                                serie.data.Add(new Data { name = "Enero", y = Convert.ToDouble(d["ene"]) });
                                serie.data.Add(new Data { name = "Febrero", y = Convert.ToDouble(d["feb"]) });
                                serie.data.Add(new Data { name = "Marzo", y = Convert.ToDouble(d["mar"]) });
                                serie.data.Add(new Data { name = "Abril", y = Convert.ToDouble(d["abr"]) });
                                serie.data.Add(new Data { name = "Mayo", y = Convert.ToDouble(d["may"]) });
                                serie.data.Add(new Data { name = "Junio", y = Convert.ToDouble(d["jun"]) });
                                serie.data.Add(new Data { name = "Julio", y = Convert.ToDouble(d["jul"]) });
                                serie.data.Add(new Data { name = "Agosto", y = Convert.ToDouble(d["ago"]) });
                                serie.data.Add(new Data { name = "Septiembre", y = Convert.ToDouble(d["sep"]) });
                                serie.data.Add(new Data { name = "Octubre", y = Convert.ToDouble(d["oct"]) });
                                serie.data.Add(new Data { name = "Noviembre", y = Convert.ToDouble(d["nov"]) });
                                serie.data.Add(new Data { name = "Diciembre", y = Convert.ToDouble(d["dic"]) });
                                serie.data.Add(new Data { name = "Anual", y = Convert.ToDouble(d["anual"]) });

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
                            DataTable tableResults = adapter.GetDataTable(@"SELECT est.departamento, est.municipio, est.nombre nombreestacion, pre.* FROM 
                                                                       clima.precipitacionperiodoreferenciaestacion pre
                                                                       INNER JOIN clima.estaciontexto est ON pre.codigo = est.codigo
                                                                       WHERE est.codigo IN (" + string.Join(",", parameters.estacion.Select(d => "'" + d + "'")) + @") ORDER BY est.nombre");


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

        private long ToUnixTimestamp(DateTime target)
        {
            var date = new DateTime(1970, 1, 1, 0, 0, 0, target.Kind);
            var unixTimestamp = System.Convert.ToInt64((target - date).TotalSeconds);

            return unixTimestamp;
        }
    }
}
