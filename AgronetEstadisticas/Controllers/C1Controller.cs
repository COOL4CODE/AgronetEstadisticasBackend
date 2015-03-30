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
    public class C1Controller : ApiController
    {
        [Route("api/Report/101")]
        public IHttpActionResult postReport101(report101 parameters)
        {
            Object returnData = null;

            var adapter = new PostgresqlAdapter();
            if (parameters.tipo == "parametro")
            {
                Parameter parameter = new Parameter { data = new List<ParameterData>() };

                switch (parameters.id)
                {
                    case 1:
                        parameter.name = "departamento";
                        string sql1 = @"SELECT DISTINCT
	                                    base.v_departamento.codigo departamentocod, 
	                                    base.v_departamento.nombre departamento
                                    FROM eva_mpal.v_evadepartamental INNER JOIN eva_mpal.v_productodetalle ON eva_mpal.v_evadepartamental.codigoagronetproducto_eva = eva_mpal.v_productodetalle.codigoagronetproducto, 
	                                    base.v_departamento
                                    ORDER BY base.v_departamento.nombre ASC";
                        DataTable data1 = adapter.GetDataTable(sql1);
                        foreach (var p in (from p in data1.AsEnumerable()
                                           select p))
                        {
                            ParameterData param = new ParameterData { name = Convert.ToString(p["departamento"]), value = Convert.ToString(p["departamentocod"]) };
                            parameter.data.Add(param);
                        }
                        break;
                    
                    
                    case 2:
                        parameter.name = "anio";
                        string sql2 = @"SELECT DISTINCT
	eva_mpal.v_evadepartamental.anho_eva as anio
FROM eva_mpal.v_evadepartamental INNER JOIN base.v_departamento ON eva_mpal.v_evadepartamental.codigodepartamento_eva = base.v_departamento.codigo::VARCHAR
	INNER JOIN eva_mpal.v_productodetalle ON eva_mpal.v_evadepartamental.codigoagronetproducto_eva = eva_mpal.v_productodetalle.codigoagronetproducto
ORDER BY eva_mpal.v_evadepartamental.anho_eva;";
                        DataTable data2 = adapter.GetDataTable(sql2);
                        foreach (var p in (from p in data2.AsEnumerable()
                                           select p["anio"]))
                        {
                            ParameterData param = new ParameterData { name = Convert.ToString(p), value = Convert.ToString(p) };
                            parameter.data.Add(param);
                        }
                        break;
                    case 3:
                        parameter.name = "producto";
                        string sql3 = @"SELECT DISTINCT
	eva_mpal.v_productodetalle.codigoagronetproducto as productocod, 
	eva_mpal.v_productodetalle.nombrecomun as producto
FROM eva_mpal.v_evadepartamental INNER JOIN base.v_departamento ON eva_mpal.v_evadepartamental.codigodepartamento_eva = base.v_departamento.codigo::VARCHAR
	INNER JOIN eva_mpal.v_productodetalle ON eva_mpal.v_evadepartamental.codigoagronetproducto_eva = eva_mpal.v_productodetalle.codigoagronetproducto

ORDER BY eva_mpal.v_productodetalle.nombrecomun
";
                        DataTable data3 = adapter.GetDataTable(sql3);
                        foreach (var p in (from p in data3.AsEnumerable()
                                           select p))
                        {
                            ParameterData param = new ParameterData { name = Convert.ToString(p["producto"]).Trim(), value = Convert.ToString(p["productocod"]).Trim() };
                            parameter.data.Add(param);
                        }
                        break;
                }
                returnData = (Parameter)parameter;
            }
            else if (parameters.tipo == "grafico")
            {
                string sqlString1 = @"SELECT 
	base.v_departamento.nombre, 
	eva_mpal.v_productodetalle.codigogrupo, 
	eva_mpal.v_productodetalle.grupo, 
	eva_mpal.v_productodetalle.codigoagronetproducto, 
	eva_mpal.v_productodetalle.nombrecomun, 
	eva_mpal.v_productodetalle.descripcion, 
	eva_mpal.v_evadepartamental.anho_eva as anho_eva, 
	eva_mpal.v_evadepartamental.areacosechada_eva as area_eva, 
	eva_mpal.v_evadepartamental.produccion_eva as produccion_eva, 
	eva_mpal.v_evadepartamental.rendimiento_eva as rendimiento,

    /*area_cosechada = area del producto / area total nacional por año y producto*/
    SUM(eva_mpal.v_evadepartamental.areacosechada_eva /
	(SELECT 
		SUM(ve.areacosechada_eva)
	FROM eva_mpal.v_evadepartamental ve INNER JOIN eva_mpal.v_productodetalle vp ON ve.codigoagronetproducto_eva = vp.codigoagronetproducto
	WHERE ve.anho_eva = eva_mpal.v_evadepartamental.anho_eva 
		AND vp.codigoagronetproducto = eva_mpal.v_productodetalle.codigoagronetproducto 
	GROUP BY ve.anho_eva
	ORDER BY ve.anho_eva ASC)*100) as area_total_nacional,

	/*produccion_nacional =  produccion producto / produccion total nacional por año y producto*/
	SUM(eva_mpal.v_evadepartamental.produccion_eva /
	(SELECT 
		SUM(ve.produccion_eva)
	FROM eva_mpal.v_evadepartamental ve INNER JOIN eva_mpal.v_productodetalle vp ON ve.codigoagronetproducto_eva = vp.codigoagronetproducto
	WHERE ve.anho_eva = eva_mpal.v_evadepartamental.anho_eva  
		AND vp.codigoagronetproducto = eva_mpal.v_productodetalle.codigoagronetproducto 
	GROUP BY ve.anho_eva
	ORDER BY ve.anho_eva ASC)*100) as produccion_total_nacional,
	
	/*participacion del area del producto con respecto al grupo*/
	SUM(eva_mpal.v_evadepartamental.produccion_eva /
	(SELECT
		SUM(ve.areacosechada_eva)
	FROM eva_mpal.v_evadepartamental ve INNER JOIN eva_mpal.v_productodetalle vp ON ve.codigoagronetproducto_eva = vp.codigoagronetproducto
	WHERE ve.anho_eva = eva_mpal.v_evadepartamental.anho_eva
		AND vp.codigogrupo = eva_mpal.v_productodetalle.codigogrupo
	GROUP BY vp.codigogrupo
	ORDER BY vp.codigogrupo ASC)*100) as participacion_area_cosechada,
	
	/*participacion produccion transitorios*/
	SUM(eva_mpal.v_evadepartamental.produccion_eva /
	(SELECT
		SUM(transi.produccion_eva)
	FROM eva_mpal.v_evadepartamentalsemestral transi
	WHERE transi.codigoagronetproducto_eva  = eva_mpal.v_productodetalle.codigoagronetproducto AND transi.anho_eva =  eva_mpal.v_evadepartamental.anho_eva
	GROUP BY transi.codigoagronetproducto_eva, transi.anho_eva
	)*100) as participacion_prod_transitorios
	
	
FROM eva_mpal.v_evadepartamental INNER JOIN base.v_departamento ON eva_mpal.v_evadepartamental.codigodepartamento_eva = base.v_departamento.codigo::VARCHAR
	INNER JOIN eva_mpal.v_productodetalle ON eva_mpal.v_evadepartamental.codigoagronetproducto_eva = eva_mpal.v_productodetalle.codigoagronetproducto
WHERE eva_mpal.v_evadepartamental.anho_eva >= " + parameters.anio_inicial+@" 
	AND eva_mpal.v_evadepartamental.anho_eva <= "+parameters.anio_final+@"
	AND eva_mpal.v_productodetalle.codigoagronetproducto = "+parameters.producto+@"
	AND base.v_departamento.codigo = "+parameters.departamento+@"

GROUP BY eva_mpal.v_evadepartamental.anho_eva, 
		 base.v_departamento.nombre, 
		 eva_mpal.v_productodetalle.nombrecomun, 
		 eva_mpal.v_productodetalle.codigoagronetproducto, 
	     eva_mpal.v_evadepartamental.areacosechada_eva,
		 eva_mpal.v_evadepartamental.produccion_eva,
		 eva_mpal.v_evadepartamental.rendimiento_eva,
		 eva_mpal.v_productodetalle.descripcion,
		 eva_mpal.v_productodetalle.grupo,
		 eva_mpal.v_productodetalle.codigogrupo

ORDER BY eva_mpal.v_evadepartamental.anho_eva";  
                DataTable result = adapter.GetDataTable(sqlString1);
                switch (parameters.id)
                {
                    case 1:
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
                    case 2:
                        Chart chart2 = new Chart { subtitle = "", series = new List<Series>() };

                        Series serie3 = new Series { name = "Rendimiento", data = new List<Data>() };
                        chart2.series.Add(serie3);
                        
                        foreach (var d1 in (from d in result.AsEnumerable()
                                            select d))
                        {
                            Data data1 = new Data { name = Convert.ToString(d1["anho_eva"]), y = Convert.ToDouble(d1["rendimiento"]) };
                            serie3.data.Add(data1);
                        }

                        returnData = (Chart)chart2;
                        break;
                    case 3:
                        Chart chart3 = new Chart { subtitle = "", series = new List<Series>() };

                        Series serie4 = new Series { name = "Producción", data = new List<Data>() };
                        Series serie5 = new Series { name = "Área", data = new List<Data>() };

                        chart3.series.Add(serie4);
                        chart3.series.Add(serie5);

                        foreach (var d1 in (from d in result.AsEnumerable()
                                            select d))
                        {
                            Data data1 = new Data { name = Convert.ToString(d1["anho_eva"]), y = Convert.ToDouble(d1["produccion_total_nacional"]) };
                            Data data2 = new Data { name = Convert.ToString(d1["anho_eva"]), y = Convert.ToDouble(d1["area_total_nacional"]) };

                            serie4.data.Add(data1);
                            serie5.data.Add(data2);
                        }

                        returnData = (Chart)chart3;
                        break;
                    case 4:
                        Chart chart4 = new Chart { subtitle = "", series = new List<Series>() };
                        
                        Series serie6 = new Series { name = "Producción", data = new List<Data>() };
                        Series serie7 = new Series { name = "Área", data = new List<Data>() };

                        chart4.series.Add(serie6);
                        chart4.series.Add(serie7);

                        foreach (var d1 in (from d in result.AsEnumerable()
                                            select d))
                        {
                            Data data1 = new Data { name = Convert.ToString(d1["anho_eva"]), y = Convert.ToDouble(d1["participacion_transi_produccion"]) };
                            Data data2 = new Data { name = Convert.ToString(d1["anho_eva"]), y = Convert.ToDouble(d1["participacion_transi_area"]) };

                            serie6.data.Add(data1);
                            serie7.data.Add(data2);
                        }

                        returnData = (Chart)chart4;
                        break;
                }

            }
            else if (parameters.tipo == "tabla")
            {
                string sqlString2 = @"SELECT 
	base.v_departamento.nombre, 
	eva_mpal.v_productodetalle.codigogrupo, 
	eva_mpal.v_productodetalle.grupo, 
	eva_mpal.v_productodetalle.codigoagronetproducto, 
	eva_mpal.v_productodetalle.nombrecomun, 
	eva_mpal.v_productodetalle.descripcion, 
	eva_mpal.v_evadepartamental.anho_eva as anho_eva, 
	eva_mpal.v_evadepartamental.areacosechada_eva as area_eva, 
	eva_mpal.v_evadepartamental.produccion_eva as produccion_eva, 
	eva_mpal.v_evadepartamental.rendimiento_eva as rendimiento,

    /*area_cosechada = area del producto / area total nacional por año y producto*/
    SUM(eva_mpal.v_evadepartamental.areacosechada_eva /
	(SELECT 
		SUM(ve.areacosechada_eva)
	FROM eva_mpal.v_evadepartamental ve INNER JOIN eva_mpal.v_productodetalle vp ON ve.codigoagronetproducto_eva = vp.codigoagronetproducto
	WHERE ve.anho_eva = eva_mpal.v_evadepartamental.anho_eva 
		AND vp.codigoagronetproducto = eva_mpal.v_productodetalle.codigoagronetproducto 
	GROUP BY ve.anho_eva
	ORDER BY ve.anho_eva ASC)*100) as area_total_nacional,

	/*produccion_nacional =  produccion producto / produccion total nacional por año y producto*/
	SUM(eva_mpal.v_evadepartamental.produccion_eva /
	(SELECT 
		SUM(ve.produccion_eva)
	FROM eva_mpal.v_evadepartamental ve INNER JOIN eva_mpal.v_productodetalle vp ON ve.codigoagronetproducto_eva = vp.codigoagronetproducto
	WHERE ve.anho_eva = eva_mpal.v_evadepartamental.anho_eva  
		AND vp.codigoagronetproducto = eva_mpal.v_productodetalle.codigoagronetproducto 
	GROUP BY ve.anho_eva
	ORDER BY ve.anho_eva ASC)*100) as produccion_total_nacional,
	
	/*participacion del area del producto con respecto al grupo*/
	SUM(eva_mpal.v_evadepartamental.produccion_eva /
	(SELECT
		SUM(ve.areacosechada_eva)
	FROM eva_mpal.v_evadepartamental ve INNER JOIN eva_mpal.v_productodetalle vp ON ve.codigoagronetproducto_eva = vp.codigoagronetproducto
	WHERE ve.anho_eva = eva_mpal.v_evadepartamental.anho_eva
		AND vp.codigogrupo = eva_mpal.v_productodetalle.codigogrupo
	GROUP BY vp.codigogrupo
	ORDER BY vp.codigogrupo ASC)*100) as participacion_area_cosechada,
	
	/*participacion produccion transitorios*/
	SUM(eva_mpal.v_evadepartamental.produccion_eva /
	(SELECT
		SUM(transi.produccion_eva)
	FROM eva_mpal.v_evadepartamentalsemestral transi
	WHERE transi.codigoagronetproducto_eva  = eva_mpal.v_productodetalle.codigoagronetproducto AND transi.anho_eva =  eva_mpal.v_evadepartamental.anho_eva
	GROUP BY transi.codigoagronetproducto_eva, transi.anho_eva
	)*100) as participacion_prod_transitorios
	
	
FROM eva_mpal.v_evadepartamental INNER JOIN base.v_departamento ON eva_mpal.v_evadepartamental.codigodepartamento_eva = base.v_departamento.codigo::VARCHAR
	INNER JOIN eva_mpal.v_productodetalle ON eva_mpal.v_evadepartamental.codigoagronetproducto_eva = eva_mpal.v_productodetalle.codigoagronetproducto
WHERE eva_mpal.v_evadepartamental.anho_eva >= " + parameters.anio_inicial + @" 
	AND eva_mpal.v_evadepartamental.anho_eva <= " + parameters.anio_final + @"
	AND eva_mpal.v_productodetalle.codigoagronetproducto = " + parameters.producto + @"
	AND base.v_departamento.codigo = " + parameters.departamento + @"

GROUP BY eva_mpal.v_evadepartamental.anho_eva, 
		 base.v_departamento.nombre, 
		 eva_mpal.v_productodetalle.nombrecomun, 
		 eva_mpal.v_productodetalle.codigoagronetproducto, 
	     eva_mpal.v_evadepartamental.areacosechada_eva,
		 eva_mpal.v_evadepartamental.produccion_eva,
		 eva_mpal.v_evadepartamental.rendimiento_eva,
		 eva_mpal.v_productodetalle.descripcion,
		 eva_mpal.v_productodetalle.grupo,
		 eva_mpal.v_productodetalle.codigogrupo

ORDER BY eva_mpal.v_evadepartamental.anho_eva";

               switch (parameters.id)
               {
                   case 1:
                       Table table = new Table { rows = adapter.GetDataTable(sqlString2) };
                       returnData = (Table)table;
                       break;
               }
            }

            if (returnData == null) {
                return NotFound();
            }

            return Ok(returnData);
        }

        [Route("api/Report/102")]
        public IHttpActionResult postReport102(report102 parameters)
        {
            Object returnData = null;
            
            var adapter = new PostgresqlAdapter();           
            switch (parameters.tipo)
            {
                case "parametro":

                    switch (parameters.id)
                    {
                        case 1:
                            var sql1 = @"SELECT DISTINCT
	                            eva_mpal.v_evadepartamental.anho_eva as anho_eva
                            FROM eva_mpal.v_evadepartamental INNER JOIN base.v_departamento ON eva_mpal.v_evadepartamental.codigodepartamento_eva = base.v_departamento.codigo::VARCHAR
	                            INNER JOIN eva_mpal.v_productodetalle ON eva_mpal.v_evadepartamental.codigoagronetproducto_eva = eva_mpal.v_productodetalle.codigoagronetproducto
                            WHERE 
	                            eva_mpal.v_productodetalle.codigoagronetproducto = "+parameters.producto+@"
                            ORDER BY eva_mpal.v_evadepartamental.anho_eva";
                            Parameter parameter1 = new Parameter { name = "anho_eva", data = new List<ParameterData>() };
                            DataTable data1 = adapter.GetDataTable(sql1);
                            foreach (var p in (from p in data1.AsEnumerable()
                                               select p["anho_eva"]))
                            {
                                ParameterData param = new ParameterData { name = Convert.ToString(p).Trim(), value = Convert.ToString(p).Trim() };
                                parameter1.data.Add(param);
                            }
                            returnData = (Parameter)parameter1;
                            break;
                        case 2:
                            var sql2 = @"SELECT DISTINCT
	                            eva_mpal.v_productodetalle.codigoagronetproducto as productocod, 
	                            eva_mpal.v_productodetalle.nombrecomun as producto
                            FROM eva_mpal.v_evadepartamental INNER JOIN base.v_departamento ON eva_mpal.v_evadepartamental.codigodepartamento_eva = base.v_departamento.codigo::VARCHAR
	                            INNER JOIN eva_mpal.v_productodetalle ON eva_mpal.v_evadepartamental.codigoagronetproducto_eva = eva_mpal.v_productodetalle.codigoagronetproducto

                            ORDER BY eva_mpal.v_productodetalle.nombrecomun
                            ";
                            Parameter parameter2 = new Parameter { name = "producto", data = new List<ParameterData>() };
                            DataTable data2 = adapter.GetDataTable(sql2);
                            foreach (var p in (from p in data2.AsEnumerable()
                                               select p))
                            {
                                ParameterData param = new ParameterData { name = Convert.ToString(p["producto"]), value = Convert.ToString(p["productocod"]) };
                                parameter2.data.Add(param);
                            }
                            returnData = (Parameter)parameter2;
                            break;
                    }

                    break;
                case "grafico":

                    string sqlString1 = @"SELECT
	                                    eva_mpal.v_productodetalle.codigoagronetproducto as productocod,
	                                    eva_mpal.v_productodetalle.nombrecomun as producto,
	                                    eva_mpal.v_evadepartamental.anho_eva as anho_eva,
	                                    SUM(eva_mpal.v_evadepartamental.areacosechada_eva) as nacional_area_eva,
	                                    SUM(eva_mpal.v_evadepartamental.produccion_eva) as nacional_produccion_eva,
	                                    SUM(eva_mpal.v_evadepartamental.rendimiento_eva) as nacional_rendimiento
    
                                    FROM eva_mpal.v_evadepartamental INNER JOIN base.v_departamento ON v_evadepartamental.codigodepartamento_eva = base.v_departamento.codigo::VARCHAR
	                                    INNER JOIN eva_mpal.v_productodetalle ON eva_mpal.v_evadepartamental.codigoagronetproducto_eva = eva_mpal.v_productodetalle.codigoagronetproducto
                                    WHERE eva_mpal.v_evadepartamental.anho_eva >= "+parameters.anio_inicial+@"
	                                    AND eva_mpal.v_evadepartamental.anho_eva <= "+parameters.anio_final+@"
	                                    AND eva_mpal.v_productodetalle.codigoagronetproducto = "+parameters.producto+@"

                                    GROUP BY
	                                    eva_mpal.v_evadepartamental.anho_eva,
	                                    eva_mpal.v_productodetalle.codigoagronetproducto,
	                                    eva_mpal.v_productodetalle.nombrecomun,
	                                    eva_mpal.v_productodetalle.descripcion

                                    ORDER BY eva_mpal.v_evadepartamental.anho_eva
                                    ";
                    
                    DataTable results = adapter.GetDataTable(sqlString1);

                    switch (parameters.id)
                    {
                        case 1:
                            Chart chart1 = new Chart { subtitle = "", series = new List<Series>() };

                            Series serie1 = new Series { name = "Producción", data = new List<Data>() };
                            Series serie2 = new Series { name = "Área", data = new List<Data>() };

                            chart1.series.Add(serie1);
                            chart1.series.Add(serie2);
                        
                            foreach (var d1 in (from d in results.AsEnumerable()
                                                select d))
                            {
                                Data data1 = new Data { name = Convert.ToString(d1["anho_eva"]), y = Convert.ToDouble(d1["nacional_produccion_eva"]) };
                                Data data2 = new Data { name = Convert.ToString(d1["anho_eva"]), y = Convert.ToDouble(d1["nacional_area_eva"]) };

                                serie1.data.Add(data1);
                                serie2.data.Add(data2);
                            }

                            returnData = (Chart)chart1;
                            break;
                        case 2:
                            Chart chart2 = new Chart { subtitle = "", series = new List<Series>() };

                            Series serie3 = new Series { name = "Rendimiento", data = new List<Data>() };
                           
                            chart2.series.Add(serie3);
                       
                        
                            foreach (var d1 in (from d in results.AsEnumerable()
                                                select d))
                            {
                                Data data1 = new Data { name = Convert.ToString(d1["anho_eva"]), y = Convert.ToDouble(d1["nacional_rendimiento"]) };
                                

                                serie3.data.Add(data1);
                                
                            }

                            returnData = (Chart)chart2;
                            break;
                        case 3:

                            String sqlChart3 = @"SELECT
	                                            eva_mpal.v_evadepartamental.anho_eva as anho_eva,
	                                            eva_mpal.v_productodetalle.codigoagronetproducto as productocod,
    
	                                            /*participacion del area del producto con respecto al grupo*/
	                                            SUM(eva_mpal.v_evadepartamental.produccion_eva /
	                                            (SELECT
    	                                            SUM(ve.areacosechada_eva)
	                                            FROM eva_mpal.v_evadepartamental ve INNER JOIN eva_mpal.v_productodetalle vp ON ve.codigoagronetproducto_eva = vp.codigoagronetproducto
	                                            WHERE ve.anho_eva = eva_mpal.v_evadepartamental.anho_eva
    	                                            AND vp.codigogrupo = eva_mpal.v_productodetalle.codigogrupo
	                                            GROUP BY vp.codigogrupo
	                                            ORDER BY vp.codigogrupo ASC)) as participacion_area_cosechada,
    
	                                            /*participacion produccion transitorios*/
	                                            SUM(eva_mpal.v_evadepartamental.produccion_eva /
	                                            (SELECT
    	                                            SUM(transi.produccion_eva)
	                                            FROM eva_mpal.v_evadepartamentalsemestral transi
	                                            WHERE transi.codigoagronetproducto_eva  = eva_mpal.v_productodetalle.codigoagronetproducto AND transi.anho_eva =  eva_mpal.v_evadepartamental.anho_eva
	                                            GROUP BY transi.codigoagronetproducto_eva, transi.anho_eva
	                                            )) as participacion_prod_transitorios
    
                                            FROM eva_mpal.v_evadepartamental
	                                            INNER JOIN eva_mpal.v_productodetalle ON eva_mpal.v_evadepartamental.codigoagronetproducto_eva = eva_mpal.v_productodetalle.codigoagronetproducto
                                            WHERE eva_mpal.v_evadepartamental.anho_eva >= "+parameters.anio_inicial+@"
	                                            AND eva_mpal.v_evadepartamental.anho_eva <= "+parameters.anio_final+@"
	                                            AND eva_mpal.v_productodetalle.codigoagronetproducto = "+parameters.producto+@"

                                            GROUP BY
	                                            eva_mpal.v_evadepartamental.anho_eva,
	                                            eva_mpal.v_productodetalle.codigoagronetproducto

                                            ORDER BY eva_mpal.v_evadepartamental.anho_eva

                                            ";

                            DataTable results3 = adapter.GetDataTable(sqlChart3);

                             Chart chart3 = new Chart { subtitle = "", series = new List<Series>() };

                            Series serie3_1 = new Series { name = "Producción", data = new List<Data>() };
                            Series serie3_2 = new Series { name = "Área", data = new List<Data>() };

                            chart3.series.Add(serie3_1);
                            chart3.series.Add(serie3_2);
                        
                            foreach (var d1 in (from d in results3.AsEnumerable()
                                                select d))
                            {
                                Data data1 = new Data { name = Convert.ToString(d1["anho_eva"]), y = Convert.ToDouble(d1["participacion_prod_transitorios"]) };
                                Data data2 = new Data { name = Convert.ToString(d1["anho_eva"]), y = Convert.ToDouble(d1["participacion_area_cosechada"]) };

                                serie3_1.data.Add(data1);
                                serie3_2.data.Add(data2);
                            }

                            returnData = (Chart)chart3;
                            break;
                    }

                    break;
                case "tabla":

                    String sqlTable = @"SELECT
	                                            eva_mpal.v_evadepartamental.anho_eva as anho_eva,
	                                            eva_mpal.v_productodetalle.codigoagronetproducto as productocod,
    
	                                            /*participacion del area del producto con respecto al grupo*/
	                                            SUM(eva_mpal.v_evadepartamental.produccion_eva /
	                                            (SELECT
    	                                            SUM(ve.areacosechada_eva)
	                                            FROM eva_mpal.v_evadepartamental ve INNER JOIN eva_mpal.v_productodetalle vp ON ve.codigoagronetproducto_eva = vp.codigoagronetproducto
	                                            WHERE ve.anho_eva = eva_mpal.v_evadepartamental.anho_eva
    	                                            AND vp.codigogrupo = eva_mpal.v_productodetalle.codigogrupo
	                                            GROUP BY vp.codigogrupo
	                                            ORDER BY vp.codigogrupo ASC)) as participacion_area_cosechada,
    
	                                            /*participacion produccion transitorios*/
	                                            SUM(eva_mpal.v_evadepartamental.produccion_eva /
	                                            (SELECT
    	                                            SUM(transi.produccion_eva)
	                                            FROM eva_mpal.v_evadepartamentalsemestral transi
	                                            WHERE transi.codigoagronetproducto_eva  = eva_mpal.v_productodetalle.codigoagronetproducto AND transi.anho_eva =  eva_mpal.v_evadepartamental.anho_eva
	                                            GROUP BY transi.codigoagronetproducto_eva, transi.anho_eva
	                                            )) as participacion_prod_transitorios
    
                                            FROM eva_mpal.v_evadepartamental
	                                            INNER JOIN eva_mpal.v_productodetalle ON eva_mpal.v_evadepartamental.codigoagronetproducto_eva = eva_mpal.v_productodetalle.codigoagronetproducto
                                            WHERE eva_mpal.v_evadepartamental.anho_eva >= "+parameters.anio_inicial+@"
	                                            AND eva_mpal.v_evadepartamental.anho_eva <= "+parameters.anio_final+@"
	                                            AND eva_mpal.v_productodetalle.codigoagronetproducto = "+parameters.producto+@"

                                            GROUP BY
	                                            eva_mpal.v_evadepartamental.anho_eva,
	                                            eva_mpal.v_productodetalle.codigoagronetproducto

                                            ORDER BY eva_mpal.v_evadepartamental.anho_eva

                                            ";

                    
                    switch (parameters.id)
                    {
                        case 1:
                            Table table = new Table { rows = adapter.GetDataTable(sqlTable) };
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

        [Route("api/Report/103")]
        public IHttpActionResult postReport103(report103 parameters)
        {
            Object returnData = null;
            PostgresqlAdapter adapter = new PostgresqlAdapter();

            switch (parameters.tipo)
            {
                case "parametro":

                    switch (parameters.id)
                    {
                        case 1:
                                 string sql1 = @"SELECT DISTINCT
                                  ev.codigoagronetproducto_eva as productocod,
                                  ep.descripcion as producto
 
                                FROM
                                  eva_mpal.v_evadepartamental ev,
                                  base.departamento b,
                                  eva_mpal.producto ep
                                WHERE
                                  b.codigo::VARCHAR = ev.codigodepartamento_eva AND
                                  ep.codigoagronetcultivo = ev.codigoagronetproducto_eva
    
                                ORDER BY ep.descripcion
                                ";
                            DataTable data1 = adapter.GetDataTable(sql1);
                            Parameter parameter1 = new Parameter { name = "producto", data = new List<ParameterData>() };
                            foreach (var p in (from p in data1.AsEnumerable()
                                               select p))
                            {
                                ParameterData param = new ParameterData { name = Convert.ToString(p["producto"]).Trim(), value = Convert.ToString(p["productocod"]).Trim() };
                                parameter1.data.Add(param);
                            }
                            returnData = (Parameter)parameter1;
                            break;
                        case 2:
                            string sql2 = @"SELECT DISTINCT
                                          ev.codigodepartamento_eva as departamentocod,
                                          b.nombre as departamento
 
                                        FROM
                                          eva_mpal.v_evadepartamental ev,
                                          base.departamento b,
                                          eva_mpal.producto ep
                                        WHERE
                                          b.codigo::VARCHAR = ev.codigodepartamento_eva AND
                                          ep.codigoagronetcultivo = ev.codigoagronetproducto_eva
                                          /*PARAMETROS*/
                                          AND ev.codigoagronetproducto_eva = "+parameters.producto+@"
";
                            DataTable data2 = adapter.GetDataTable(sql2);
                            Parameter parameter2 = new Parameter { name = "departamento", data = new List<ParameterData>() };
                            foreach (var p in (from p in data2.AsEnumerable()
                                               select p))
                            {
                                ParameterData param = new ParameterData { name = Convert.ToString(p["departamento"]).Trim(), value = Convert.ToString(p["departamento"]).Trim() };
                                parameter2.data.Add(param);
                            }
                            returnData = (Parameter)parameter2;
                            break;
                        case 3:
                            string sql3 = @"SELECT DISTINCT
                                              ev.anho_eva
 
                                            FROM
                                              eva_mpal.v_evadepartamental ev,
                                              base.departamento b,
                                              eva_mpal.producto ep
                                            WHERE
                                              b.codigo::VARCHAR = ev.codigodepartamento_eva AND
                                              ep.codigoagronetcultivo = ev.codigoagronetproducto_eva
                                              /*PARAMETROS*/
                                              AND ev.codigoagronetproducto_eva = "+parameters.producto+@"
                                            ORDER BY ev.anho_eva
                                            ";
                            DataTable data3 = adapter.GetDataTable(sql3);
                            Parameter parameter3 = new Parameter { name = "anio", data = new List<ParameterData>() };
                            foreach (var p in (from p in data3.AsEnumerable()
                                               select p["anho_eva"]))
                            {
                                ParameterData param = new ParameterData { name = Convert.ToString(p), value = Convert.ToString(p) };
                                parameter3.data.Add(param);
                            }
                            returnData = (Parameter)parameter3;
                            break;
                    }

                    break;
                case "grafico":

                    string sqlString = @"SELECT
  ev.anho_eva as anho_eva,
  ev.codigoagronetproducto_eva as productocod,
  ep.descripcion as producto,
  ev.codigodepartamento_eva as departamentocod,
  b.nombre as departamento,
  ev.areacosechada_eva as area,
  ev.produccion_eva as produccion,
  ev.rendimiento_eva as rendimiento
 
FROM
  eva_mpal.v_evadepartamental ev,
  base.departamento b,
  eva_mpal.producto ep
WHERE
  b.codigo::VARCHAR = ev.codigodepartamento_eva AND
  ep.codigoagronetcultivo = ev.codigoagronetproducto_eva
  /*PARAMETROS*/
  AND ev.anho_eva >= "+parameters.anio_inicial+@" AND ev.anho_eva <= "+parameters.anio_final+@"
  AND ev.codigoagronetproducto_eva = "+parameters.producto+@" AND b.codigo IN (" + string.Join(",", parameters.departamento.Select(d => "'" + d + "'")) + @")
GROUP BY
    ev.anho_eva, ev.codigoagronetproducto_eva,
    ep.descripcion,
    ev.codigodepartamento_eva,
    b.nombre,
    ev.areacosechada_eva,
    ev.produccion_eva,
    ev.rendimiento_eva    
ORDER BY ev.produccion_eva desc, ev.areacosechada_eva desc, ev.rendimiento_eva desc";

                    DataTable results = adapter.GetDataTable(sqlString);

          string sqlString4 = @"SELECT
  ev.anho_eva,
  ev.codigoagronetproducto_eva,
  ep.descripcion,
  ev.codigodepartamento_eva,
  b.nombre,
  (
    SELECT
      SUM(a.produccion_eva)
    FROM
      eva_mpal.v_evadepartamental a,
      base.departamento b,
      eva_mpal.producto c
    WHERE
      b.codigo::VARCHAR = a.codigodepartamento_eva AND
      c.codigoagronetcultivo = a.codigoagronetproducto_eva
   	 /*PARAMS*/
   	 AND c.codigoagronetcultivo = ev.codigoagronetproducto_eva
   	 AND a.anho_eva = ev.anho_eva
    GROUP BY a.anho_eva
    ORDER BY a.anho_eva ASC
  ) as total_nacional_producto,
  ev.areacosechada_eva as area,
  ev.produccion_eva as produccion,
  /* area / total area nacional*/  
  (SUM(ev.areacosechada_eva)/(
    SELECT
      SUM(a.areacosechada_eva)
    FROM
      eva_mpal.v_evadepartamental a,
      base.departamento b,
      eva_mpal.producto c
    WHERE
      b.codigo::VARCHAR = a.codigodepartamento_eva AND
      c.codigoagronetcultivo = a.codigoagronetproducto_eva
   	 /*PARAMS*/
   	 AND c.codigoagronetcultivo = ev.codigoagronetproducto_eva
   	 AND a.anho_eva = ev.anho_eva
    GROUP BY a.anho_eva
    ORDER BY a.anho_eva ASC
  )) as participacion_area_nacional,
  /* prod depto / total prod nacional*/
  (SUM(ev.produccion_eva)/(
    SELECT
      SUM(a.produccion_eva)
    FROM
      eva_mpal.v_evadepartamental a,
      base.departamento b,
      eva_mpal.producto c
    WHERE
      b.codigo::VARCHAR = a.codigodepartamento_eva AND
      c.codigoagronetcultivo = a.codigoagronetproducto_eva
   	 /*PARAMS*/
   	 AND c.codigoagronetcultivo = ev.codigoagronetproducto_eva
   	 AND a.anho_eva = ev.anho_eva
    GROUP BY a.anho_eva
    ORDER BY a.anho_eva ASC
  )) as participacion_prod_nacional
 
FROM
  eva_mpal.v_evadepartamental ev,
  base.departamento b,
  eva_mpal.producto ep
WHERE
  b.codigo::VARCHAR = ev.codigodepartamento_eva AND
  ep.codigoagronetcultivo = ev.codigoagronetproducto_eva
  /*PARAMETROS*/
  AND ev.anho_eva >= "+parameters.anio_inicial+@" AND ev.anho_eva <= "+parameters.anio_final+@"
  AND ev.codigoagronetproducto_eva = "+parameters.anio_final+@" AND b.codigo IN (" + string.Join(",", parameters.departamento.Select(d => "'" + d + "'")) + @")
GROUP BY
    ev.anho_eva, ev.codigoagronetproducto_eva,
    ep.descripcion,
    ev.codigodepartamento_eva,
    b.nombre,
    ev.areacosechada_eva,
    ev.produccion_eva,
    ev.rendimiento_eva    
ORDER BY ev.anho_eva, ev.produccion_eva desc, ev.areacosechada_eva desc, ev.rendimiento_eva desc";

                    DataTable results4 = adapter.GetDataTable(sqlString4);





                    switch (parameters.id)
                    {
                        case 1:
                            Chart chart1 = new Chart { subtitle = "", series = new List<Series>() };

                            var query1 = from r in results.AsEnumerable()
                                         group r by r["departamento"];

                            foreach (var deptosGroup in query1)
                            {
                                var serie1 = new Series { name = deptosGroup.Key.ToString(), data = new List<Data>() };
                                foreach (var el1 in deptosGroup)
                                {
                                    var data = new Data { name = Convert.ToString(el1["anho_eva"]), y = Convert.ToDouble(el1["area"]) };
                                    serie1.data.Add(data);

                                }
                                chart1.series.Add(serie1);
                            }

                            returnData = (Chart)chart1;
                            break;
                        case 2:
                            Chart chart2 = new Chart { subtitle = "", series = new List<Series>() };

                            var query2 = from r in results.AsEnumerable()
                                         group r by r["departamento"];

                            foreach (var deptosGroup in query2)
                            {
                                var serie1 = new Series { name = deptosGroup.Key.ToString(), data = new List<Data>() };
                                foreach (var el1 in deptosGroup)
                                {
                                    var data = new Data { name = Convert.ToString(el1["anho_eva"]), y = Convert.ToDouble(el1["produccion"]) };
                                    serie1.data.Add(data);

                                }
                                chart2.series.Add(serie1);
                            }

                            returnData = (Chart)chart2;
                            break;
                        case 3:
                            Chart chart3 = new Chart { subtitle = "", series = new List<Series>() };

                            var query3 = from r in results.AsEnumerable()
                                         group r by r["departamento"];

                            foreach (var deptosGroup in query3)
                            {
                                var serie1 = new Series { name = deptosGroup.Key.ToString(), data = new List<Data>() };
                                foreach (var el1 in deptosGroup)
                                {
                                    var data = new Data { name = Convert.ToString(el1["anho_eva"]), y = Convert.ToDouble(el1["rendimiento"]) };
                                    serie1.data.Add(data);

                                }
                                chart3.series.Add(serie1);
                            }

                            returnData = (Chart)chart3;
                            break;
                        case 4:


                            Chart chart4 = new Chart { subtitle = "", series = new List<Series>() };

                            var query4 = from r in results4.AsEnumerable()
                                         group r by r["nombre"];

                            foreach (var deptosGroup in query4)
                            {
                                var serie1 = new Series { name = deptosGroup.Key.ToString(), data = new List<Data>() };
                                foreach (var el1 in deptosGroup)
                                {
                                    var data = new Data { name = Convert.ToString(el1["anho_eva"]), y = Convert.ToDouble(el1["area"]) };
                                    serie1.data.Add(data);

                                }
                                chart4.series.Add(serie1);
                            }

                            returnData = (Chart)chart4;
                            break;
                        case 5:
                            Chart chart5 = new Chart { subtitle = "", series = new List<Series>() };

                            var query5 = from r in results4.AsEnumerable()
                                         group r by r["nombre"];

                            foreach (var deptosGroup in query5)
                            {
                                var serie1 = new Series { name = deptosGroup.Key.ToString(), data = new List<Data>() };
                                foreach (var el1 in deptosGroup)
                                {
                                    var data = new Data { name = Convert.ToString(el1["anho_eva"]), y = Convert.ToDouble(el1["produccion"]) };
                                    serie1.data.Add(data);

                                }
                                chart5.series.Add(serie1);
                            }

                            returnData = (Chart)chart5;
                            break;
                        case 6:
                            Chart chart6 = new Chart { subtitle = "", series = new List<Series>() };

                            var query6 = from r in results4.AsEnumerable()
                                         group r by r["nombre"];

                            foreach (var deptosGroup in query6)
                            {
                                var serie1 = new Series { name = deptosGroup.Key.ToString(), data = new List<Data>() };
                                foreach (var el1 in deptosGroup)
                                {
                                    var data = new Data { name = Convert.ToString(el1["anho_eva"]), y = Convert.ToDouble(el1["total_nacional_producto"]) };
                                    serie1.data.Add(data);

                                }
                                chart6.series.Add(serie1);
                            }

                            returnData = (Chart)chart6;
                            break;
                        
                        case 7:
                            Chart chart7 = new Chart { subtitle = "", series = new List<Series>() };

                            var query7 = from r in results4.AsEnumerable()
                                         group r by r["nombre"];

                            foreach (var deptosGroup in query7)
                            {
                                var serie1 = new Series { name = deptosGroup.Key.ToString(), data = new List<Data>() };
                                foreach (var el1 in deptosGroup)
                                {
                                    var data = new Data { name = Convert.ToString(el1["anho_eva"]), y = Convert.ToDouble(el1["participacion_area_nacional"]) };
                                    serie1.data.Add(data);

                                }
                                chart7.series.Add(serie1);
                            }

                            returnData = (Chart)chart7;
                            break;
                    }

                    break;
                case "tabla":
                    string sqlStringTable = @"SELECT 	base.departamento.codigo,
                                    base.departamento.nombre,
                                    COALESCE(eva_mpal.productos.grupo, 0) as grupo,
                                    eva_mpal.productos.codigoagronetcultivo,
                                    eva_mpal.productos.nombredescriptorcultivo,
                                    COALESCE(eva_mpal.evadepartamentalanual.anho_eva, 0) as anho_eva,
                                    COALESCE(eva_mpal.evadepartamentalanual.area_eva, 0) as area_eva,
                                    COALESCE(eva_mpal.evadepartamentalanual.produccion_eva, 0) as produccion_eva,
                                    COALESCE((eva_mpal.evadepartamentalanual.produccion_eva/eva_mpal.evadepartamentalanual.area_eva), 0) AS rendimiento,
                                    COALESCE(eva_mpal.evadepartamentalanual.area_eva / ( SELECT SUM(e.area_eva) total_nacion_area FROM eva_mpal.productos p INNER JOIN eva_mpal.evadepartamentalanual e ON p.codigoagronetcultivo = e.codigoagronetproducto_eva WHERE e.anho_eva = eva_mpal.evadepartamentalanual.anho_eva AND p.nombredescriptorcultivo = eva_mpal.productos.nombredescriptorcultivo )*100, 0 )AS area_total_nacional,
                                    COALESCE(eva_mpal.evadepartamentalanual.produccion_eva / ( SELECT SUM(e.produccion_eva) AS total_nacion_produccion FROM eva_mpal.productos p INNER JOIN eva_mpal.evadepartamentalanual e ON p.codigoagronetcultivo = e.codigoagronetproducto_eva WHERE e.anho_eva = eva_mpal.evadepartamentalanual.anho_eva AND p.nombredescriptorcultivo = eva_mpal.productos.nombredescriptorcultivo )*100, 0) AS produccion_total_nacional,
                                    COALESCE((SELECT SUM(e.area_eva) FROM eva_mpal.productos p INNER JOIN eva_mpal.evadepartamentalsemestral e ON p.codigoagronetcultivo = e.codigoagronetproducto_eva WHERE e.anho_eva = eva_mpal.evadepartamentalanual.anho_eva AND p.nombredescriptorcultivo = eva_mpal.productos.nombredescriptorcultivo ) / (SELECT SUM(e.area_eva) FROM eva_mpal.productos p INNER JOIN eva_mpal.evadepartamentalsemestral e ON p.codigoagronetcultivo = e.codigoagronetproducto_eva WHERE e.anho_eva = eva_mpal.evadepartamentalanual.anho_eva AND p.grupo = eva_mpal.productos.grupo)*100 , 0) AS participacion_transi_area,
                                    COALESCE((SELECT SUM(e.produccion_eva) FROM eva_mpal.productos p INNER JOIN eva_mpal.evadepartamentalsemestral e ON p.codigoagronetcultivo = e.codigoagronetproducto_eva WHERE e.anho_eva = eva_mpal.evadepartamentalanual.anho_eva AND p.nombredescriptorcultivo = eva_mpal.productos.nombredescriptorcultivo ) / (SELECT SUM(e.produccion_eva) FROM eva_mpal.productos p INNER JOIN eva_mpal.evadepartamentalsemestral e ON p.codigoagronetcultivo = e.codigoagronetproducto_eva WHERE e.anho_eva = eva_mpal.evadepartamentalanual.anho_eva AND p.grupo = eva_mpal.productos.grupo)*100, 0) AS participacion_transi_produccion,
                                    ((SELECT COALESCE(SUM(e.area_eva),0) FROM eva_mpal.productos p INNER JOIN eva_mpal.evadepartamentalanual e ON p.codigoagronetcultivo = e.codigoagronetproducto_eva WHERE e.anho_eva = eva_mpal.evadepartamentalanual.anho_eva AND p.nombredescriptorcultivo = eva_mpal.productos.nombredescriptorcultivo) / eva_mpal.evadepartamentalanual.area_eva) AS participacion_area_nacional,
                                    ((SELECT COALESCE(SUM(e.produccion_eva),0) FROM eva_mpal.productos p INNER JOIN eva_mpal.evadepartamentalanual e ON p.codigoagronetcultivo = e.codigoagronetproducto_eva WHERE e.anho_eva = eva_mpal.evadepartamentalanual.anho_eva AND p.nombredescriptorcultivo = eva_mpal.productos.nombredescriptorcultivo ) / eva_mpal.evadepartamentalanual.produccion_eva) AS participacion_produccion_nacional
                                    FROM eva_mpal.productos INNER JOIN eva_mpal.evadepartamentalanual ON eva_mpal.productos.codigoagronetcultivo = eva_mpal.evadepartamentalanual.codigoagronetproducto_eva
                                    INNER JOIN base.departamento ON base.departamento.codigo::VARCHAR = eva_mpal.evadepartamentalanual.codigodepartamento_eva
                                    WHERE eva_mpal.evadepartamentalanual.anho_eva >= " + parameters.anio_inicial + " AND eva_mpal.evadepartamentalanual.anho_eva <= " + parameters.anio_final + " AND eva_mpal.productos.nombredescriptorcultivo = '" + parameters.producto + "' AND base.departamento.nombre IN (" + string.Join(",", parameters.departamento.Select(d => "'" + d + "'")) + ");";

                    switch (parameters.id)
                    {
                        case 1:
                            Table table = new Table { rows = adapter.GetDataTable(sqlStringTable) };
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

        [Route("api/Report/104")]
        public IHttpActionResult postReport104(report104 parameters)
        {
            Object returnData = null;
            PostgresqlAdapter adapter = new PostgresqlAdapter();

            string sqlString = @"SELECT 
                                  ev.anho_eva, 
                                  ev.codigoagronetproducto_eva, 
                                  ep.descripcion as nombredescriptorcultivo, 
                                  ev.codigodepartamento_eva, 
                                  b.nombre as nombre, 
                                  ev.areacosechada_eva as area_eva,
                                  /* area / total area nacional*/  
                                  (SUM(ev.produccion_eva)/(
	                                SELECT 
		                                SUM(eva_mpal.v_evadepartamental.areacosechada_eva)
	                                FROM eva_mpal.v_evadepartamental INNER JOIN eva_mpal.v_productodetalle ON eva_mpal.v_evadepartamental.codigoagronetproducto_eva = eva_mpal.v_productodetalle.codigoagronetproducto
	                                WHERE eva_mpal.v_evadepartamental.anho_eva = eva_mpal.v_evadepartamental.anho_eva 
		                                AND eva_mpal.v_productodetalle.codigoagronetproducto = ev.codigoagronetproducto_eva AND eva_mpal.v_evadepartamental.anho_eva = ev.anho_eva
	                                GROUP BY eva_mpal.v_evadepartamental.anho_eva
	                                ORDER BY eva_mpal.v_evadepartamental.anho_eva ASC
                                  )) as participacion_area_nacional, 
                                  ev.produccion_eva as produccion_eva,
                                  /* prod depto / total prod nacional*/ 
                                  (SUM(ev.produccion_eva)/(
	                                SELECT 
		                                SUM(eva_mpal.v_evadepartamental.produccion_eva)
	                                FROM eva_mpal.v_evadepartamental INNER JOIN eva_mpal.v_productodetalle ON eva_mpal.v_evadepartamental.codigoagronetproducto_eva = eva_mpal.v_productodetalle.codigoagronetproducto
	                                WHERE eva_mpal.v_evadepartamental.anho_eva = eva_mpal.v_evadepartamental.anho_eva 
		                                AND eva_mpal.v_productodetalle.codigoagronetproducto = ev.codigoagronetproducto_eva AND eva_mpal.v_evadepartamental.anho_eva = ev.anho_eva
	                                GROUP BY eva_mpal.v_evadepartamental.anho_eva
	                                ORDER BY eva_mpal.v_evadepartamental.anho_eva ASC
                                  )) as participacion_prod_nacional, 
                                  ev.rendimiento_eva as rendimiento_eva
  
                                FROM 
                                  eva_mpal.v_evadepartamental ev, 
                                  base.departamento b, 
                                  eva_mpal.producto ep
                                WHERE 
                                  b.codigo::VARCHAR = ev.codigodepartamento_eva AND
                                  ep.codigoagronetcultivo = ev.codigoagronetproducto_eva
                                  /*PARAMETROS*/
                                  AND ev.anho_eva = 2003
                                  AND ev.codigoagronetproducto_eva = 111030920256
                                GROUP BY
	                                ev.anho_eva, ev.codigoagronetproducto_eva,
	                                ep.descripcion, 
	                                ev.codigodepartamento_eva, 
	                                b.nombre,
	                                ev.areacosechada_eva,
	                                ev.produccion_eva,
	                                ev.rendimiento_eva	
                                ORDER BY ev.produccion_eva desc, ev.areacosechada_eva desc, ev.rendimiento_eva desc";

            switch (parameters.tipo)
            {
                case "parametro":

                    switch (parameters.id)
                    {
                        case 1:
                            string sql1 = @"SELECT DISTINCT
                                          ev.anho_eva 
                                        FROM 
                                          eva_mpal.v_evadepartamental ev, 
                                          base.departamento b, 
                                          eva_mpal.producto ep
                                        WHERE 
                                          b.codigo::VARCHAR = ev.codigodepartamento_eva AND
                                          ep.codigoagronetcultivo = ev.codigoagronetproducto_eva
                                          /*PARAMETROS*/
                                          AND ev.codigoagronetproducto_eva = "+parameters.producto+@"
                                        ORDER BY ev.anho_eva ";
                            DataTable data1 = adapter.GetDataTable(sql1);
                            Parameter parameter1 = new Parameter { name = "anio", data = new List<ParameterData>() };
                            foreach (var p in (from p in data1.AsEnumerable()
                                               select p["anho_eva"]))
                            {
                                ParameterData param = new ParameterData { name = Convert.ToString(p), value = Convert.ToString(p) };
                                parameter1.data.Add(param);
                            }
                            returnData = (Parameter)parameter1;
                            break;
                        case 2:
                            string sql2 = @"SELECT DISTINCT
                                          ev.codigoagronetproducto_eva as productocod, 
                                          ep.descripcion as producto
  
                                        FROM 
                                          eva_mpal.v_evadepartamental ev, 
                                          base.departamento b, 
                                          eva_mpal.producto ep
                                        WHERE 
                                          b.codigo::VARCHAR = ev.codigodepartamento_eva AND
                                          ep.codigoagronetcultivo = ev.codigoagronetproducto_eva
                                        ORDER BY  ep.descripcion";
                            DataTable data2 = adapter.GetDataTable(sql2);
                            Parameter parameter2 = new Parameter { name = "producto", data = new List<ParameterData>() };
                            foreach (var p in (from p in data2.AsEnumerable()
                                               select p))
                            {
                                ParameterData param = new ParameterData { name = Convert.ToString(p["producto"]).Trim(), value = Convert.ToString(p["productocod"]).Trim() };
                                parameter2.data.Add(param);
                            }
                            returnData = (Parameter)parameter2;
                            break;
                        case 3:
                            Parameter parameter3 = new Parameter { name = "ordenamiento", data = new List<ParameterData>() };
                            ParameterData pdata1 = new ParameterData { name = "Área", value = "area_eva" };
                            ParameterData pdata2 = new ParameterData { name = "Producción", value = "produccion_eva" };
                            ParameterData pdata3 = new ParameterData { name = "Rendimiento", value = "rendimiento_eva" };
                            parameter3.data.Add(pdata1);
                            parameter3.data.Add(pdata2);
                            parameter3.data.Add(pdata3);
                            returnData = (Parameter)parameter3;
                            break;
                    }

                    break;
                case "grafico":

                    DataTable results = adapter.GetDataTable(sqlString);
                    switch (parameters.id)
                    {
                        case 1:
                            Chart chart1 = new Chart { subtitle = "", series = new List<Series>() };

                            var query1 = from r in results.AsEnumerable()
                                         group r by r["nombredescriptorcultivo"];

                            foreach (var productoGroup in query1)
                            {
                                var serie1 = new Series { name = productoGroup.Key.ToString(), data = new List<Data>() };
                                foreach (var el1 in productoGroup)
                                {
                                    var data = new Data { name = Convert.ToString(el1["nombre"]), y = Convert.ToDouble(el1[parameters.ordenamiento]) };
                                    serie1.data.Add(data);

                                }
                                chart1.series.Add(serie1);
                            }

                            returnData = (Chart)chart1;
                            break;
                        case 2:
                            Chart chart2 = new Chart { subtitle = "", series = new List<Series>() };

                            var query2 = from r in results.AsEnumerable()
                                         group r by r["nombredescriptorcultivo"];

                            foreach (var productoGroup in query2)
                            {
                                var serie1 = new Series { name = productoGroup.Key.ToString(), data = new List<Data>() };
                                foreach (var el1 in productoGroup)
                                {
                                    var data = new Data { name = Convert.ToString(el1["nombre"]), y = Convert.ToDouble(el1[parameters.ordenamiento]) };
                                    serie1.data.Add(data);

                                }
                                chart2.series.Add(serie1);
                            }

                            returnData = (Chart)chart2;
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

        [Route("api/Report/105")]
        public IHttpActionResult postReport105(report105 parameters)
        {
            Object returnData = null;
            PostgresqlAdapter adapter = new PostgresqlAdapter();

            string sqlString = @" SELECT 
                                  v_evadepartamental.anho_eva, 
                                  v_evadepartamental.codigoagronetproducto_eva, 
                                  producto.descripcion as producto, 
                                  v_evadepartamental.codigodepartamento_eva, 
                                  departamento.nombre as departamento, 
                                  v_evadepartamental.areacosechada_eva as area_eva, 
                                  v_evadepartamental.produccion_eva as produccion_eva, 
                                  v_evadepartamental.rendimiento_eva as rendimiento_eva
  
                                FROM 
                                  eva_mpal.v_evadepartamental, 
                                  base.departamento, 
                                  eva_mpal.producto
                                WHERE 
                                  departamento.codigo::VARCHAR = v_evadepartamental.codigodepartamento_eva AND
                                  producto.codigoagronetcultivo = v_evadepartamental.codigoagronetproducto_eva
                                  /*PARAMETROS*/
                                  AND v_evadepartamental.anho_eva >= "+parameters.anio_inicial+@" 
                                  AND v_evadepartamental.anho_eva <= "+parameters.anio_final+@" 
                                  AND departamento.codigo = "+parameters.departamento+@"
                                ORDER BY v_evadepartamental.produccion_eva, v_evadepartamental.areacosechada_eva, v_evadepartamental.rendimiento_eva ";
            switch (parameters.tipo)
            {
                case "parametro":

                    switch (parameters.id)
                    {
                        case 1:
                            string sql1 = @"SELECT DISTINCT
                                          v_evadepartamental.anho_eva as anho_eva
  
                                        FROM 
                                          eva_mpal.v_evadepartamental, 
                                          base.departamento, 
                                          eva_mpal.producto
                                        WHERE 
                                          departamento.codigo::VARCHAR = v_evadepartamental.codigodepartamento_eva AND
                                          producto.codigoagronetcultivo = v_evadepartamental.codigoagronetproducto_eva
                                          /*PARAMETROS*/
                                          AND departamento.codigo = "+parameters.departamento+@"
                                        ORDER BY v_evadepartamental.anho_eva";
                            DataTable data1 = adapter.GetDataTable(sql1);
                            Parameter parameter1 = new Parameter { name = "anio", data = new List<ParameterData>() };
                            foreach (var p in (from p in data1.AsEnumerable()
                                               select p["anho_eva"]))
                            {
                                ParameterData param = new ParameterData { name = Convert.ToString(p), value = Convert.ToString(p) };
                                parameter1.data.Add(param);
                            }
                            returnData = (Parameter)parameter1;
                            break;
                        case 2:
                            string sql2 = @"SELECT DISTINCT
                                          v_evadepartamental.codigodepartamento_eva as departamentocod, 
                                          departamento.nombre as departamento
  
                                        FROM 
                                          eva_mpal.v_evadepartamental, 
                                          base.departamento, 
                                          eva_mpal.producto
                                        WHERE 
                                          departamento.codigo::VARCHAR = v_evadepartamental.codigodepartamento_eva AND
                                          producto.codigoagronetcultivo = v_evadepartamental.codigoagronetproducto_eva

                                        ORDER BY departamento.nombre";
                            DataTable data2 = adapter.GetDataTable(sql2);
                            Parameter parameter2 = new Parameter { name = "departamento", data = new List<ParameterData>() };
                            foreach (var p in (from p in data2.AsEnumerable()
                                               select p))
                            {
                                ParameterData param = new ParameterData { name = Convert.ToString(p["departamento"]).Trim(), value = Convert.ToString(p["departamentocod"]).Trim() };
                                parameter2.data.Add(param);
                            }
                            returnData = (Parameter)parameter2;
                            break;
                    }

                    break;
                case "grafico":

                    DataTable results = adapter.GetDataTable(sqlString);
                    switch (parameters.id)
                    {
                        case 1:
                            Chart chart1 = new Chart { subtitle = "", series = new List<Series>() };

                            var query1 = from r in results.AsEnumerable()
                                         group r by r["departamento"] into deptoGroup
                                         from productGroup in (from d in deptoGroup
                                                               group d by d["producto"])
                                         group productGroup by deptoGroup.Key;

                            foreach (var deptoGroup in query1)
                            {
                                var serie1 = new Series { name = deptoGroup.Key.ToString(), data = new List<Data>() };
                                foreach (var productGroup in deptoGroup)
                                {
                                    double y = productGroup.Sum(d => Convert.ToDouble(d["area_eva"]));
                                    var data = new Data { name = Convert.ToString(productGroup.Key.ToString()), y = y };
                                    serie1.data.Add(data);
                                }
                                chart1.series.Add(serie1);
                            }

                            returnData = (Chart)chart1;
                            break;
                        case 2:
                            Chart chart2 = new Chart { subtitle = "", series = new List<Series>() };

                            var query2 = from r in results.AsEnumerable()
                                         group r by r["departamento"] into deptoGroup
                                         from productGroup in (from d in deptoGroup
                                                               group d by d["producto"])
                                         group productGroup by deptoGroup.Key;

                            foreach (var deptoGroup in query2)
                            {
                                var serie1 = new Series { name = deptoGroup.Key.ToString(), data = new List<Data>() };
                                foreach (var productGroup in deptoGroup)
                                {
                                    double y = productGroup.Sum(d => Convert.ToDouble(d["produccion_eva"]));
                                    var data = new Data { name = Convert.ToString(productGroup.Key.ToString()), y = y };
                                    serie1.data.Add(data);
                                }
                                chart2.series.Add(serie1);
                            }

                            returnData = (Chart)chart2;
                            break;
                    }

                    break;
                case "tabla":

                    switch (parameters.id)
                    {
                        case 1:
                            Table table = new Table { rows = adapter.GetDataTable(sqlString) };
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

        [Route("api/Report/106")]
        public IHttpActionResult postReport106(report106 parameters)
        {
            Object returnData = null;
            PostgresqlAdapter adapter = new PostgresqlAdapter();
            switch (parameters.tipo)
            {
                case "parametro":

                    switch (parameters.id)
                    {
                        case 1:
                            string sql1 = @"SELECT DISTINCT
                                              pi.anho as anho
                                            FROM 
                                              pecuario.inventariobovinogrupo pi, 
                                              base.departamento b, 
                                              pecuario.orientacionbovino po
                                            WHERE 
                                              b.codigo = pi.codigodepto AND
                                              po.codigo = pi.codigoedadtipobovino AND
                                              pi.codigodepto in  (" + string.Join(",", parameters.departamento.Select(d => "'" + d + "'")) + @")	
                                            ORDER BY pi.anho";
                            DataTable data1 = adapter.GetDataTable(sql1);
                            Parameter parameter1 = new Parameter { name = "anio", data = new List<ParameterData>() };
                            foreach (var p in (from p in data1.AsEnumerable()
                                               select p["anho"]))
                            {
                                ParameterData param = new ParameterData { name = Convert.ToString(p), value = Convert.ToString(p) };
                                parameter1.data.Add(param);
                            }
                            returnData = (Parameter)parameter1;
                            break;
                        case 2:
                            string sql2 = @"SELECT DISTINCT
                                          pi.codigodepto as departamentocod,
                                          b.nombre as departamento
                                        FROM 
                                          pecuario.inventariobovinogrupo pi, 
                                          base.departamento b, 
                                          pecuario.orientacionbovino po
                                        WHERE 
                                          b.codigo = pi.codigodepto AND
                                          po.codigo = pi.codigoedadtipobovino
                                        ORDER BY pi.codigodepto, b.nombre
                                        ";
                            DataTable data2 = adapter.GetDataTable(sql2);
                            Parameter parameter2 = new Parameter { name = "departamento", data = new List<ParameterData>() };
                            foreach (var p in (from p in data2.AsEnumerable()
                                               select p))
                            {
                                ParameterData param = new ParameterData { name = Convert.ToString(p["departamento"]).Trim(), value = Convert.ToString(p["departamentocod"]).Trim() };
                                parameter2.data.Add(param);
                            }
                            returnData = (Parameter)parameter2;
                            break;
                    }

                    break;
                case "grafico":
                    switch (parameters.id)
                    {
                        case 1:
                            string sql3 = @"SELECT 
                                          inventariobovinogrupo.anho, 
                                          (SELECT 
	                                          SUM(p.totalmachos) machos_nal
	                                        FROM 
	                                          pecuario.inventariobovinogrupo p, 
	                                          pecuario.orientacionbovino po
	                                        WHERE 
	                                          po.codigo = p.codigoedadtipobovino
	                                          AND p.anho = inventariobovinogrupo.anho

	                                        GROUP BY p.anho) as total_machos_nal,
                                          (SELECT 
	                                          SUM(p.totalhembras) hembras_nal
	                                        FROM 
	                                          pecuario.inventariobovinogrupo p, 
	                                          pecuario.orientacionbovino po
	                                        WHERE 
	                                          po.codigo = p.codigoedadtipobovino
	                                          AND p.anho = inventariobovinogrupo.anho

	                                        GROUP BY p.anho) as total_hembras_nal
                                        FROM 
                                          pecuario.inventariobovinogrupo, 
                                          base.departamento, 
                                          pecuario.orientacionbovino
                                        WHERE 
                                          departamento.codigo = inventariobovinogrupo.codigodepto AND
                                          orientacionbovino.codigo = inventariobovinogrupo.codigoedadtipobovino
                                          /*PARAMETROS*/
                                          AND inventariobovinogrupo.anho >= '" + parameters.anio_inicial + @"' AND inventariobovinogrupo.anho <= '" + parameters.anio_final + @"'
                                        GROUP BY inventariobovinogrupo.anho
                                        ORDER BY inventariobovinogrupo.anho";
                    
                            DataTable results1 = adapter.GetDataTable(sql3);                    

                            Chart chart1 = new Chart { subtitle = "", series = new List<Series>() };

                            Series serie1 = new Series { name = "Machos", data = new List<Data>() };
                            Series serie2 = new Series { name = "Hembras", data = new List<Data>() };

                            chart1.series.Add(serie1);
                            chart1.series.Add(serie2);
                        
                            foreach (var d1 in (from d in results1.AsEnumerable()
                                                select d))
                            {
                                Data data1 = new Data { name = Convert.ToString(d1["anho"]), y = Convert.ToDouble(d1["total_machos_nal"]) };
                                Data data2 = new Data { name = Convert.ToString(d1["anho"]), y = Convert.ToDouble(d1["total_hembras_nal"]) };

                                serie1.data.Add(data1);
                                serie2.data.Add(data2);
                            }

                            returnData = (Chart)chart1;
                            break;
                        case 2:
                            string sql4 = @"SELECT 
                                          pi.anho as anio, 
                                          pi.codigodepto as departamentocod, 
                                          b.nombre as departamento, 
                                          pi.codigoedadtipobovino, 
                                          po.descripcion, 
                                          SUM(pi.totalmachos) AS total_machos_depto,  
                                          SUM(pi.totalhembras) AS total_hembras_depto
                                        FROM 
                                          pecuario.inventariobovinogrupo pi, 
                                          base.departamento b, 
                                          pecuario.orientacionbovino po
                                        WHERE 
                                          b.codigo = pi.codigodepto AND
                                          po.codigo = pi.codigoedadtipobovino
                                          /*PARAMETROS*/
                                          AND pi.anho >= "+parameters.anio_inicial+@" AND pi.anho <= "+parameters.anio_final+@"
                                          AND pi.codigodepto in (" + string.Join(",", parameters.departamento.Select(d => "'" + d + "'")) + @")
                                        GROUP BY pi.anho, pi.codigodepto, b.nombre, pi.codigoedadtipobovino, 
                                          po.descripcion
                                        ORDER BY pi.anho, pi.codigoedadtipobovino";

                            DataTable results2 = adapter.GetDataTable(sql4);
                            Chart chart2 = new Chart { subtitle = "", series = new List<Series>() };

                            var query2 = from r in results2.AsEnumerable()
                                         group r by r["departamento"] into deptoGroup
                                         from anioGroup in (from d in deptoGroup
                                                               group d by d["anio"])
                                         group anioGroup by deptoGroup.Key;

                            foreach (var deptoGroup in query2)
                            {
                                var serie = new Series { name = deptoGroup.Key.ToString(), data = new List<Data>() };
                                foreach (var anioGroup in deptoGroup)
                                {
                                    double y = anioGroup.Sum(d => Convert.ToDouble(d["total_machos_depto"]));
                                    var data = new Data { name = Convert.ToString(anioGroup.Key.ToString()), y = y };
                                    serie.data.Add(data);
                                }
                                chart2.series.Add(serie);
                            }

                            returnData = (Chart)chart2;

                            break;
                        case 3:
                            string sql5 = @"SELECT 
                                          pi.anho as anio, 
                                          pi.codigodepto as departamentocod, 
                                          b.nombre as departamento, 
                                          pi.codigoedadtipobovino, 
                                          po.descripcion, 
                                          SUM(pi.totalmachos) AS total_machos_depto,  
                                          SUM(pi.totalhembras) AS total_hembras_depto
                                        FROM 
                                          pecuario.inventariobovinogrupo pi, 
                                          base.departamento b, 
                                          pecuario.orientacionbovino po
                                        WHERE 
                                          b.codigo = pi.codigodepto AND
                                          po.codigo = pi.codigoedadtipobovino
                                          /*PARAMETROS*/
                                          AND pi.anho >= " + parameters.anio_inicial + @" AND pi.anho <= " + parameters.anio_final + @"
                                          AND pi.codigodepto in (" + string.Join(",", parameters.departamento.Select(d => "'" + d + "'")) + @")
                                        GROUP BY pi.anho, pi.codigodepto, b.nombre, pi.codigoedadtipobovino, 
                                          po.descripcion
                                        ORDER BY pi.anho, pi.codigoedadtipobovino";

                            DataTable results3 = adapter.GetDataTable(sql5);
                            Chart chart3 = new Chart { subtitle = "", series = new List<Series>() };

                            var query3 = from r in results3.AsEnumerable()
                                         group r by r["departamento"] into deptoGroup
                                         from anioGroup in (from d in deptoGroup
                                                               group d by d["anio"])
                                         group anioGroup by deptoGroup.Key;

                            foreach (var deptoGroup in query3)
                            {
                                var serie = new Series { name = deptoGroup.Key.ToString(), data = new List<Data>() };
                                foreach (var anioGroup in deptoGroup)
                                {
                                    double y = anioGroup.Sum(d => Convert.ToDouble(d["total_hembras_depto"]));
                                    var data = new Data { name = Convert.ToString(anioGroup.Key.ToString()), y = y };
                                    serie.data.Add(data);
                                }
                                chart3.series.Add(serie);
                            }

                            returnData = (Chart)chart3;
                            break;

                        case 4:
                            string sql6 = @"
                                            SELECT 
                                              pi.anho as anio, 
                                              pi.codigoorientacion as orientacioncod, 
                                              po.descripcion as orientacion, 
                                              SUM(pi.total) as total_animales
                                            FROM 
                                              pecuario.inventariobovinoorientacion pi, 
                                              pecuario.orientacionbovino po 
                                            WHERE 
                                              po.codigo = pi.codigoorientacion AND pi.anho >= "+parameters.anio_inicial+@" AND pi.anho <= "+parameters.anio_final+@"
                                            GROUP BY pi.anho, pi.codigoorientacion, po.descripcion
                                            ORDER BY pi.anho, 
                                              pi.codigoorientacion
                                            ";
                            
                            DataTable results4 = adapter.GetDataTable(sql6);
                            Chart chart4 = new Chart { subtitle = "", series = new List<Series>() };

                            var query4 = from r in results4.AsEnumerable()
                                         group r by r["orientacion"] into orientacionGroup
                                         from anioGroup in (from d in orientacionGroup
                                                               group d by d["anio"])
                                         group anioGroup by orientacionGroup.Key;

                            foreach (var orientacionGroup in query4)
                            {
                                var serie = new Series { name = orientacionGroup.Key.ToString(), data = new List<Data>() };
                                foreach (var anioGroup in orientacionGroup)
                                {
                                    double y = anioGroup.Sum(d => Convert.ToDouble(d["total_animales"]));
                                    var data = new Data { name = Convert.ToString(anioGroup.Key.ToString()), y = y };
                                    serie.data.Add(data);
                                }
                                chart4.series.Add(serie);
                            }

                            returnData = (Chart)chart4;
                            break;
                    }

                    break;
                case "tabla":

                    string sql7 = @"SELECT 
                                    pi.anho as anio, 
                                    pi.codigodepto as departamentocod, 
                                    b.nombre as departamento, 
                                    pi.codigoedadtipobovino as codigoedadbovino, 
                                    po.descripcion as , 
                                    SUM(pi.totalmachos) AS total_machos_depto,  
                                    SUM(pi.totalhembras) AS total_hembras_depto
                                FROM 
                                    pecuario.inventariobovinogrupo pi, 
                                    base.departamento b, 
                                    pecuario.orientacionbovino po
                                WHERE 
                                    b.codigo = pi.codigodepto AND
                                    po.codigo = pi.codigoedadtipobovino
                                    /*PARAMETROS*/
                                    AND pi.anho >= "+parameters.anio_inicial+@" AND pi.anho <= "+parameters.anio_final+@"
                                    AND pi.codigodepto in " + string.Join(",", parameters.departamento.Select(d => "'" + d + "'")) + @"
                                GROUP BY pi.anho, pi.codigodepto, b.nombre, pi.codigoedadtipobovino, 
                                    po.descripcion
                                ORDER BY pi.anho, pi.codigoedadtipobovino"; 

                    switch (parameters.id)
                    {
                        case 1:
                            Table table = new Table { rows = adapter.GetDataTable(sql7) };
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

        [Route("api/Report/107")]
        public IHttpActionResult postReport107(report107 parameters)
        {
            Object returnData = null;

            string sqlString = @"SELECT 
                              pp.producto, 	
                              p.nombre, 
                              pp.periodo as periodo, 
                              pp.departamento as departamentocod, 
                              b.nombre as departamento_nombre,
                              pp.produccion as produccion_pecuaria, 
                              pp.unidad 

                            FROM 
                              pecuario.produccionpecuaria pp, 
                              pecuario.producto p, 
                              base.departamento b
                            WHERE 
                              p.codigo = pp.producto AND
                              b.codigo = pp.departamento
                              /*PARAMETROS*/	
                              AND pp.producto = "+parameters.tipo_pecuario+@" AND pp.periodo >= "+parameters.anio_inicial+@" 
                              AND pp.periodo <= "+parameters.anio_final+@"
                              AND pp.departamento IN (" + string.Join(",", parameters.departamento.Select(d => "'" + d + "'")) +") ";

            PostgresqlAdapter adapter = new PostgresqlAdapter();
            switch (parameters.tipo)
            {
                case "parametro":

                    switch (parameters.id)
                    {
                        case 1:
                            string sql1 = @"SELECT DISTINCT
                                          pp.periodo

                                        FROM 
                                          pecuario.produccionpecuaria pp, 
                                          pecuario.producto p, 
                                          base.departamento b
                                        WHERE 
                                          p.codigo = pp.producto AND
                                          b.codigo = pp.departamento AND 
                                          /*PARAMETROS*/	
                                          pp.departamento IN (" + string.Join(",", parameters.departamento.Select(d => "'" + d + "'")) +@") ORDER BY pp.periodo";
                                        

                            DataTable data1 = adapter.GetDataTable(sql1);
                            Parameter parameter1 = new Parameter { name = "anio", data = new List<ParameterData>() };
                            foreach (var p in (from p in data1.AsEnumerable()
                                               select p["periodo"]))
                            {
                                ParameterData param = new ParameterData { name = Convert.ToString(p), value = Convert.ToString(p) };
                                parameter1.data.Add(param);
                            }
                            returnData = (Parameter)parameter1;
                            break;
                        case 2:
                            string sql2 = @"SELECT DISTINCT 
                                          pp.departamento as departamentocod, 
                                          b.nombre as departamento
                                        FROM 
                                          pecuario.produccionpecuaria pp, 
                                          pecuario.producto p, 
                                          base.departamento b
                                        WHERE 
                                          p.codigo = pp.producto AND
                                          b.codigo = pp.departamento AND 
                                          /*PARAMETROS*/	
                                          pp.producto = "+parameters.tipo_pecuario+@"
                                        ORDER BY b.nombre
";
                            DataTable data2 = adapter.GetDataTable(sql2);
                            Parameter parameter2 = new Parameter { name = "departamento", data = new List<ParameterData>() };
                            foreach (var p in (from p in data2.AsEnumerable()
                                               select p))
                            {
                                ParameterData param = new ParameterData { name = Convert.ToString(p["departamento"]).Trim(), value = Convert.ToString(p["departamentocod"]).Trim() };
                                parameter2.data.Add(param);
                            }
                            returnData = (Parameter)parameter2;
                            break;
                        case 3:
                            string sql3 = @"
                                            SELECT DISTINCT
                                              pp.producto  as codigo, 	
                                              p.nombre as nombre

                                            FROM 
                                              pecuario.produccionpecuaria pp, 
                                              pecuario.producto p, 
                                              base.departamento b
                                            WHERE 
                                              p.codigo = pp.producto AND
                                              b.codigo = pp.departamento
                                            ORDER BY p.nombre
";
                            DataTable data3 = adapter.GetDataTable(sql3);
                            Parameter parameter3 = new Parameter { name = "tipo_pecuario", data = new List<ParameterData>() };
                            foreach (var p in (from p in data3.AsEnumerable()
                                               select p))
                            {
                                ParameterData param = new ParameterData { name = Convert.ToString(p["nombre"]), value = Convert.ToString(p["codigo"]) };
                                parameter3.data.Add(param);
                            }
                            returnData = (Parameter)parameter3;
                            break;
                    }

                    break;
                case "grafico":

                    switch (parameters.id)
                    {
                        case 1:
                            Chart chart1 = new Chart { subtitle = "", series = new List<Series>() };

                            var query1 = from r in adapter.GetDataTable(sqlString).AsEnumerable()
                                         group r by r["departamento_nombre"] into deptoGroup
                                         from anioGroup in (from d in deptoGroup
                                                               group d by d["periodo"])
                                         group anioGroup by deptoGroup.Key;

                            foreach (var deptoGroup in query1)
                            {
                                var serie = new Series { name = deptoGroup.Key.ToString(), data = new List<Data>() };
                                foreach (var anioGroup in deptoGroup)
                                {
                                    double y = anioGroup.Sum(d => Convert.ToDouble(d["produccion_pecuaria"]));
                                    var data = new Data { name = Convert.ToString(anioGroup.Key.ToString()), y = y };
                                    serie.data.Add(data);
                                }
                                chart1.series.Add(serie);
                            }

                            returnData = (Chart)chart1;

                            break;
                    }

                    break;
                case "tabla":

                    String sqlTable = @"SELECT 
                                              p.nombre, 
                                              pp.periodo, 
                                              b.nombre,
                                              pp.produccion as produccionToneladas

                                            FROM 
                                              pecuario.produccionpecuaria pp, 
                                              pecuario.producto p, 
                                              base.departamento b
                                            WHERE 
                                              p.codigo = pp.producto AND
                                              b.codigo = pp.departamento AND 
                                              /*PARAMETROS*/	
                                              pp.departamento IN (" + string.Join(",", parameters.departamento.Select(d => "'" + d + "'")) +@") 
                                              AND pp.producto = " + parameters.tipo_pecuario + @" 
                                              AND pp.periodo >= " + parameters.anio_inicial + @" AND pp.periodo <= " + parameters.anio_final + "";
                    switch (parameters.id)
                    {

                        case 1:
                            Table table = new Table { rows = adapter.GetDataTable(sqlTable) };
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
