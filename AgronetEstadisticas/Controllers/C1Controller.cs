﻿using System;
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
                        parameter.name = "anio";
                        string sql1 = @"SELECT DISTINCT
                                        eva_mpal.evadepartamentalanual.anho_eva anio
                                        FROM eva_mpal.productos INNER JOIN eva_mpal.evadepartamentalanual ON eva_mpal.productos.codigoagronetcultivo = eva_mpal.evadepartamentalanual.codigoagronetproducto_eva
                                        INNER JOIN base.departamento ON base.departamento.codigo::VARCHAR = eva_mpal.evadepartamentalanual.codigodepartamento_eva
                                        ORDER BY eva_mpal.evadepartamentalanual.anho_eva;";
                        DataTable data1 = adapter.GetDataTable(sql1);
                        foreach (var p in (from p in data1.AsEnumerable()
                                           select p["anio"]))
                        {
                            ParameterData param = new ParameterData { name = Convert.ToString(p), value = Convert.ToString(p) };
                            parameter.data.Add(param);
                        }
                        break;
                    case 2:
                        parameter.name = "departamento";
                        string sql2 = @"SELECT DISTINCT
                                        base.departamento.nombre departamento
                                        FROM eva_mpal.productos INNER JOIN eva_mpal.evadepartamentalanual ON eva_mpal.productos.codigoagronetcultivo = eva_mpal.evadepartamentalanual.codigoagronetproducto_eva
                                        INNER JOIN base.departamento ON base.departamento.codigo::VARCHAR = eva_mpal.evadepartamentalanual.codigodepartamento_eva;";
                        DataTable data2 = adapter.GetDataTable(sql2);
                        foreach (var p in (from p in data2.AsEnumerable()
                                           select p["departamento"]))
                        {
                            ParameterData param = new ParameterData { name = Convert.ToString(p), value = Convert.ToString(p) };
                            parameter.data.Add(param);
                        }
                        break;
                    case 3:
                        parameter.name = "producto";
                        string sql3 = @"SELECT DISTINCT
                                        eva_mpal.productos.descripcion producto
                                        FROM eva_mpal.productos INNER JOIN eva_mpal.evadepartamentalanual ON eva_mpal.productos.codigoagronetcultivo = eva_mpal.evadepartamentalanual.codigoagronetproducto_eva
                                        INNER JOIN base.departamento ON base.departamento.codigo::VARCHAR = eva_mpal.evadepartamentalanual.codigodepartamento_eva
                                        ORDER BY eva_mpal.productos.descripcion;";
                        DataTable data3 = adapter.GetDataTable(sql3);
                        foreach (var p in (from p in data3.AsEnumerable()
                                           select p["producto"]))
                        {
                            ParameterData param = new ParameterData { name = Convert.ToString(p), value = Convert.ToString(p) };
                            parameter.data.Add(param);
                        }
                        break;
                }
                returnData = (Parameter)parameter;
            }
            else if (parameters.tipo == "grafico")
            {
                string sqlString1 = @"SELECT base.departamento.codigo,
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
                                WHERE eva_mpal.evadepartamentalanual.anho_eva >= " + parameters.anio_inicial + @" AND eva_mpal.evadepartamentalanual.anho_eva <= " + parameters.anio_final + @" AND eva_mpal.productos.nombredescriptorcultivo = '" + parameters.producto + @"' AND base.departamento.nombre = '" + parameters.departamento + "'";  
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
                string sqlString2 = @"SELECT base.departamento.codigo,
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
                                WHERE eva_mpal.evadepartamentalanual.anho_eva >= " + parameters.anio_inicial + @" AND eva_mpal.evadepartamentalanual.anho_eva <= " + parameters.anio_final + @" AND eva_mpal.productos.nombredescriptorcultivo = '" + parameters.producto + @"' AND base.departamento.nombre = '" + parameters.departamento + "'";

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
                                        eva_mpal.productos.nombredescriptorcultivo
                                        FROM eva_mpal.productos INNER JOIN eva_mpal.evadepartamentalanual ON eva_mpal.productos.codigoagronetcultivo = eva_mpal.evadepartamentalanual.codigoagronetproducto_eva
                                        INNER JOIN base.departamento ON base.departamento.codigo::VARCHAR = eva_mpal.evadepartamentalanual.codigodepartamento_eva
                                        ORDER BY nombredescriptorcultivo";
                            Parameter parameter1 = new Parameter { name = "productos", data = new List<ParameterData>() };
                            DataTable data1 = adapter.GetDataTable(sql1);
                            foreach (var p in (from p in data1.AsEnumerable()
                                               select p["nombredescriptorcultivo"]))
                            {
                                ParameterData param = new ParameterData { name = Convert.ToString(p), value = Convert.ToString(p) };
                                parameter1.data.Add(param);
                            }
                            returnData = (Parameter)parameter1;
                            break;
                        case 2:
                            var sql2 = @"SELECT DISTINCT
                                        COALESCE(eva_mpal.evadepartamentalanual.anho_eva, 0) as anho_eva
                                        FROM eva_mpal.productos INNER JOIN eva_mpal.evadepartamentalanual ON eva_mpal.productos.codigoagronetcultivo = eva_mpal.evadepartamentalanual.codigoagronetproducto_eva
                                        INNER JOIN base.departamento ON base.departamento.codigo::VARCHAR = eva_mpal.evadepartamentalanual.codigodepartamento_eva
                                        ORDER BY anho_eva";
                            Parameter parameter2 = new Parameter { name = "anio", data = new List<ParameterData>() };
                            DataTable data2 = adapter.GetDataTable(sql2);
                            foreach (var p in (from p in data2.AsEnumerable()
                                               select p["anho_eva"]))
                            {
                                ParameterData param = new ParameterData { name = Convert.ToString(p), value = Convert.ToString(p) };
                                parameter2.data.Add(param);
                            }
                            returnData = (Parameter)parameter2;
                            break;
                    }

                    break;
                case "grafico":

                    string sqlString1 = @"SELECT 
                                    base.departamento.codigo,
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
                                WHERE eva_mpal.evadepartamentalanual.anho_eva >= " + parameters.anio_inicial + " AND eva_mpal.evadepartamentalanual.anho_eva <= " + parameters.anio_final + " AND eva_mpal.productos.nombredescriptorcultivo = '" + parameters.producto + "'";
                    
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
                                Data data1 = new Data { name = Convert.ToString(d1["anho_eva"]), y = Convert.ToDouble(d1["produccion_eva"]) };
                                Data data2 = new Data { name = Convert.ToString(d1["anho_eva"]), y = Convert.ToDouble(d1["area_eva"]) };

                                serie1.data.Add(data1);
                                serie2.data.Add(data2);
                            }

                            returnData = (Chart)chart1;
                            break;
                        case 2:
                            Chart chart2 = new Chart { subtitle = "", series = new List<Series>() };

                            Series serie3 = new Series { name = "Producción", data = new List<Data>() };
                            Series serie4 = new Series { name = "Área", data = new List<Data>() };

                            chart2.series.Add(serie3);
                            chart2.series.Add(serie4);
                        
                            foreach (var d1 in (from d in results.AsEnumerable()
                                                select d))
                            {
                                Data data1 = new Data { name = Convert.ToString(d1["anho_eva"]), y = Convert.ToDouble(d1["participacion_transi_produccion"]) };
                                Data data2 = new Data { name = Convert.ToString(d1["anho_eva"]), y = Convert.ToDouble(d1["participacion_transi_area"]) };

                                serie3.data.Add(data1);
                                serie4.data.Add(data2);
                            }

                            returnData = (Chart)chart2;
                            break;
                        case 3:
                            Chart chart3 = new Chart { subtitle = "", series = new List<Series>() };

                            Series serie5 = new Series { name = "Rendimiento", data = new List<Data>() };
                            chart3.series.Add(serie5);
                        
                            foreach (var d1 in (from d in results.AsEnumerable()
                                                select d))
                            {
                                Data data1 = new Data { name = Convert.ToString(d1["anho_eva"]), y = Convert.ToDouble(d1["rendimiento"]) };
                                serie5.data.Add(data1);
                            }

                            returnData = (Chart)chart3;
                            break;
                    }

                    break;
                case "tabla":

                    string sqlString2 = @"SELECT 
                                    base.departamento.codigo,
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
                                WHERE eva_mpal.evadepartamentalanual.anho_eva >= " + parameters.anio_inicial + " AND eva_mpal.evadepartamentalanual.anho_eva <= " + parameters.anio_final + " AND eva_mpal.productos.nombredescriptorcultivo = '" + parameters.producto + "'";
                    
                    switch (parameters.id)
                    {
                        case 1:
                            Table table = new Table { rows = adapter.GetDataTable(sqlString2) };
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

            string sqlString = @"SELECT 	base.departamento.codigo,
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
                                    WHERE eva_mpal.evadepartamentalanual.anho_eva >= "+parameters.anio_inicial+" AND eva_mpal.evadepartamentalanual.anho_eva <= "+parameters.anio_final+" AND eva_mpal.productos.nombredescriptorcultivo = '"+parameters.producto+"' AND base.departamento.nombre IN ("+ string.Join(",",parameters.departamento.Select(d => "'"+d+"'")) +");";
                    
            switch (parameters.tipo)
            {
                case "parametro":

                    switch (parameters.id)
                    {
                        case 1:
                            string sql1 = @"SELECT DISTINCT
                                            eva_mpal.productos.nombredescriptorcultivo
                                            FROM eva_mpal.productos INNER JOIN eva_mpal.evadepartamentalanual ON eva_mpal.productos.codigoagronetcultivo = eva_mpal.evadepartamentalanual.codigoagronetproducto_eva
                                            INNER JOIN base.departamento ON base.departamento.codigo::VARCHAR = eva_mpal.evadepartamentalanual.codigodepartamento_eva
                                            ORDER BY nombredescriptorcultivo";
                            DataTable data1 = adapter.GetDataTable(sql1);
                            Parameter parameter1 = new Parameter { name = "producto", data = new List<ParameterData>() };
                            foreach (var p in (from p in data1.AsEnumerable()
                                               select p["nombredescriptorcultivo"]))
                            {
                                ParameterData param = new ParameterData { name = Convert.ToString(p), value = Convert.ToString(p) };
                                parameter1.data.Add(param);
                            }
                            returnData = (Parameter)parameter1;
                            break;
                        case 2:
                            string sql2 = @"SELECT DISTINCT
                                            base.departamento.nombre 
                                            FROM eva_mpal.productos INNER JOIN eva_mpal.evadepartamentalanual ON eva_mpal.productos.codigoagronetcultivo = eva_mpal.evadepartamentalanual.codigoagronetproducto_eva
                                            INNER JOIN base.departamento ON base.departamento.codigo::VARCHAR = eva_mpal.evadepartamentalanual.codigodepartamento_eva
                                            ORDER BY base.departamento.nombre asc";
                            DataTable data2 = adapter.GetDataTable(sql2);
                            Parameter parameter2 = new Parameter { name = "departamento", data = new List<ParameterData>() };
                            foreach (var p in (from p in data2.AsEnumerable()
                                               select p["nombre"]))
                            {
                                ParameterData param = new ParameterData { name = Convert.ToString(p), value = Convert.ToString(p) };
                                parameter2.data.Add(param);
                            }
                            returnData = (Parameter)parameter2;
                            break;
                        case 3:
                            string sql3 = @"SELECT DISTINCT
                                            COALESCE(eva_mpal.evadepartamentalanual.anho_eva, 0) as anho_eva
                                            FROM eva_mpal.productos INNER JOIN eva_mpal.evadepartamentalanual ON eva_mpal.productos.codigoagronetcultivo = eva_mpal.evadepartamentalanual.codigoagronetproducto_eva
                                            INNER JOIN base.departamento ON base.departamento.codigo::VARCHAR = eva_mpal.evadepartamentalanual.codigodepartamento_eva
                                            ORDER BY anho_eva asc";
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

                    DataTable results = adapter.GetDataTable(sqlString);
                    switch (parameters.id)
                    {
                        case 1:
                            Chart chart1 = new Chart { subtitle = "", series = new List<Series>() };

                            var query1 = from r in results.AsEnumerable()
                                         group r by r["nombre"];

                            foreach (var deptosGroup in query1)
                            {
                                var serie1 = new Series { name = deptosGroup.Key.ToString(), data = new List<Data>() };
                                foreach (var el1 in deptosGroup)
                                {
                                    var data = new Data { name = Convert.ToString(el1["anho_eva"]), y = Convert.ToDouble(el1["area_eva"]) };
                                    serie1.data.Add(data);

                                }
                                chart1.series.Add(serie1);
                            }

                            returnData = (Chart)chart1;
                            break;
                        case 2:
                            Chart chart2 = new Chart { subtitle = "", series = new List<Series>() };

                            var query2 = from r in results.AsEnumerable()
                                         group r by r["nombre"];

                            foreach (var deptosGroup in query2)
                            {
                                var serie1 = new Series { name = deptosGroup.Key.ToString(), data = new List<Data>() };
                                foreach (var el1 in deptosGroup)
                                {
                                    var data = new Data { name = Convert.ToString(el1["anho_eva"]), y = Convert.ToDouble(el1["produccion_eva"]) };
                                    serie1.data.Add(data);

                                }
                                chart2.series.Add(serie1);
                            }

                            returnData = (Chart)chart2;
                            break;
                        case 3:
                            Chart chart3 = new Chart { subtitle = "", series = new List<Series>() };

                            var query3 = from r in results.AsEnumerable()
                                         group r by r["nombre"];

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

                            var query4 = from r in results.AsEnumerable()
                                         group r by r["nombre"];

                            foreach (var deptosGroup in query4)
                            {
                                var serie1 = new Series { name = deptosGroup.Key.ToString(), data = new List<Data>() };
                                foreach (var el1 in deptosGroup)
                                {
                                    var data = new Data { name = Convert.ToString(el1["anho_eva"]), y = Convert.ToDouble(el1["participacion_produccion_nacional"]) };
                                    serie1.data.Add(data);

                                }
                                chart4.series.Add(serie1);
                            }

                            returnData = (Chart)chart4;
                            break;
                        case 5:
                            Chart chart5 = new Chart { subtitle = "", series = new List<Series>() };

                            var query5 = from r in results.AsEnumerable()
                                         group r by r["nombre"];

                            foreach (var deptosGroup in query5)
                            {
                                var serie1 = new Series { name = deptosGroup.Key.ToString(), data = new List<Data>() };
                                foreach (var el1 in deptosGroup)
                                {
                                    var data = new Data { name = Convert.ToString(el1["anho_eva"]), y = Convert.ToDouble(el1["participacion_transi_produccion"]) };
                                    serie1.data.Add(data);

                                }
                                chart5.series.Add(serie1);
                            }

                            returnData = (Chart)chart5;
                            break;
                        case 6:
                            Chart chart6 = new Chart { subtitle = "", series = new List<Series>() };

                            var query6 = from r in results.AsEnumerable()
                                         group r by r["nombre"];

                            foreach (var deptosGroup in query6)
                            {
                                var serie1 = new Series { name = deptosGroup.Key.ToString(), data = new List<Data>() };
                                foreach (var el1 in deptosGroup)
                                {
                                    var data = new Data { name = Convert.ToString(el1["anho_eva"]), y = Convert.ToDouble(el1["participacion_area_nacional"]) };
                                    serie1.data.Add(data);

                                }
                                chart6.series.Add(serie1);
                            }

                            returnData = (Chart)chart6;
                            break;
                        
                        case 7:
                            Chart chart7 = new Chart { subtitle = "", series = new List<Series>() };

                            var query7 = from r in results.AsEnumerable()
                                         group r by r["nombre"];

                            foreach (var deptosGroup in query7)
                            {
                                var serie1 = new Series { name = deptosGroup.Key.ToString(), data = new List<Data>() };
                                foreach (var el1 in deptosGroup)
                                {
                                    var data = new Data { name = Convert.ToString(el1["anho_eva"]), y = Convert.ToDouble(el1["participacion_transi_area"]) };
                                    serie1.data.Add(data);

                                }
                                chart7.series.Add(serie1);
                            }

                            returnData = (Chart)chart7;
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

        [Route("api/Report/104")]
        public IHttpActionResult postReport104(report104 parameters)
        {
            Object returnData = null;
            PostgresqlAdapter adapter = new PostgresqlAdapter();

            string sqlString = @"SELECT base.departamento.codigo,
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
                                    WHERE eva_mpal.evadepartamentalanual.anho_eva = " + parameters.anio + " AND eva_mpal.productos.nombredescriptorcultivo = '" + parameters.producto + "' ORDER BY eva_mpal.evadepartamentalanual.produccion_eva desc";

            switch (parameters.tipo)
            {
                case "parametro":

                    switch (parameters.id)
                    {
                        case 1:
                            string sql1 = @"SELECT DISTINCT
                                            COALESCE(eva_mpal.evadepartamentalanual.anho_eva, 0) as anho_eva
                                            FROM eva_mpal.productos INNER JOIN eva_mpal.evadepartamentalanual ON eva_mpal.productos.codigoagronetcultivo = eva_mpal.evadepartamentalanual.codigoagronetproducto_eva
                                            INNER JOIN base.departamento ON base.departamento.codigo::VARCHAR = eva_mpal.evadepartamentalanual.codigodepartamento_eva
                                            ORDER BY anho_eva asc";
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
                                            eva_mpal.productos.nombredescriptorcultivo
                                            FROM eva_mpal.productos INNER JOIN eva_mpal.evadepartamentalanual ON eva_mpal.productos.codigoagronetcultivo = eva_mpal.evadepartamentalanual.codigoagronetproducto_eva
                                            INNER JOIN base.departamento ON base.departamento.codigo::VARCHAR = eva_mpal.evadepartamentalanual.codigodepartamento_eva
                                            ORDER BY nombredescriptorcultivo";
                            DataTable data2 = adapter.GetDataTable(sql2);
                            Parameter parameter2 = new Parameter { name = "producto", data = new List<ParameterData>() };
                            foreach (var p in (from p in data2.AsEnumerable()
                                               select p["nombredescriptorcultivo"]))
                            {
                                ParameterData param = new ParameterData { name = Convert.ToString(p), value = Convert.ToString(p) };
                                parameter2.data.Add(param);
                            }
                            returnData = (Parameter)parameter2;
                            break;
                        case 3:
                            Parameter parameter3 = new Parameter { name = "ordenamiento", data = new List<ParameterData>() };
                            ParameterData pdata1 = new ParameterData { name = "Área", value = "area_eva" };
                            ParameterData pdata2 = new ParameterData { name = "Producción", value = "produccion_eva" };
                            ParameterData pdata3 = new ParameterData { name = "Rendimiento", value = "rendimiento" };
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

            string sqlString = @"SELECT base.departamento.codigo,
                                    base.departamento.nombre as departamento,
                                    eva_mpal.productos.codigoagronetcultivo,
                                    eva_mpal.productos.nombredescriptorcultivo as producto,
                                    COALESCE(eva_mpal.evadepartamentalanual.anho_eva, 0) as anho_eva,
                                    COALESCE(eva_mpal.evadepartamentalanual.area_eva, 0) as area_eva,
                                    COALESCE(eva_mpal.evadepartamentalanual.produccion_eva, 0) as produccion_eva,
                                    COALESCE((eva_mpal.evadepartamentalanual.produccion_eva/NULLIF(eva_mpal.evadepartamentalanual.area_eva,0)), 0) AS rendimiento
                                    FROM eva_mpal.productos INNER JOIN eva_mpal.evadepartamentalanual ON eva_mpal.productos.codigoagronetcultivo = eva_mpal.evadepartamentalanual.codigoagronetproducto_eva
                                    INNER JOIN base.departamento ON base.departamento.codigo::VARCHAR = eva_mpal.evadepartamentalanual.codigodepartamento_eva
                                    WHERE eva_mpal.evadepartamentalanual.anho_eva >= "+parameters.anio_inicial+" AND eva_mpal.evadepartamentalanual.anho_eva <= "+parameters.anio_final+" AND base.departamento.nombre = '"+parameters.departamento+"'";
            switch (parameters.tipo)
            {
                case "parametro":

                    switch (parameters.id)
                    {
                        case 1:
                            string sql1 = @"SELECT DISTINCT
                                            COALESCE(eva_mpal.evadepartamentalanual.anho_eva, 0) as anho_eva
                                            FROM eva_mpal.productos INNER JOIN eva_mpal.evadepartamentalanual ON eva_mpal.productos.codigoagronetcultivo = eva_mpal.evadepartamentalanual.codigoagronetproducto_eva
                                            INNER JOIN base.departamento ON base.departamento.codigo::VARCHAR = eva_mpal.evadepartamentalanual.codigodepartamento_eva
                                            ORDER BY anho_eva asc";
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
                                            base.departamento.nombre
                                            FROM eva_mpal.productos INNER JOIN eva_mpal.evadepartamentalanual ON eva_mpal.productos.codigoagronetcultivo = eva_mpal.evadepartamentalanual.codigoagronetproducto_eva
                                            INNER JOIN base.departamento ON base.departamento.codigo::VARCHAR = eva_mpal.evadepartamentalanual.codigodepartamento_eva
                                            ORDER BY base.departamento.nombre";
                            DataTable data2 = adapter.GetDataTable(sql2);
                            Parameter parameter2 = new Parameter { name = "departamento", data = new List<ParameterData>() };
                            foreach (var p in (from p in data2.AsEnumerable()
                                               select p["nombre"]))
                            {
                                ParameterData param = new ParameterData { name = Convert.ToString(p), value = Convert.ToString(p) };
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
                            string sql1 = @"SELECT DISTINCT pecuario.inventariobovinogrupo.anho 
                                            FROM base.departamento INNER JOIN pecuario.inventariobovinogrupo ON base.departamento.codigo = pecuario.inventariobovinogrupo.codigodepto
                                            ORDER BY pecuario.inventariobovinogrupo.anho ASC";
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
                                            base.departamento.nombre
                                            FROM base.departamento INNER JOIN pecuario.inventariobovinogrupo ON base.departamento.codigo = pecuario.inventariobovinogrupo.codigodepto
                                            ORDER BY base.departamento.nombre ASC";
                            DataTable data2 = adapter.GetDataTable(sql2);
                            Parameter parameter2 = new Parameter { name = "departamento", data = new List<ParameterData>() };
                            foreach (var p in (from p in data2.AsEnumerable()
                                               select p["nombre"]))
                            {
                                ParameterData param = new ParameterData { name = Convert.ToString(p), value = Convert.ToString(p) };
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
                            string sql3 = @"SELECT	pecuario.inventariobovinogrupo.anho,  
                                    SUM(pecuario.inventariobovinogrupo.totalmachos) total_machos_nacional,
                                    SUM(pecuario.inventariobovinogrupo.totalhembras) total_hembras_nacional
                                    FROM pecuario.inventariobovinogrupo
                                    WHERE inventariobovinogrupo.anho >= "+parameters.anio_inicial+" AND inventariobovinogrupo.anho <= "+parameters.anio_final+" GROUP BY pecuario.inventariobovinogrupo.anho ORDER BY pecuario.inventariobovinogrupo.anho ASC";
                    
                            DataTable results1 = adapter.GetDataTable(sql3);                    

                            Chart chart1 = new Chart { subtitle = "", series = new List<Series>() };

                            Series serie1 = new Series { name = "Machos", data = new List<Data>() };
                            Series serie2 = new Series { name = "Hembras", data = new List<Data>() };

                            chart1.series.Add(serie1);
                            chart1.series.Add(serie2);
                        
                            foreach (var d1 in (from d in results1.AsEnumerable()
                                                select d))
                            {
                                Data data1 = new Data { name = Convert.ToString(d1["anho"]), y = Convert.ToDouble(d1["total_machos_nacional"]) };
                                Data data2 = new Data { name = Convert.ToString(d1["anho"]), y = Convert.ToDouble(d1["total_hembras_nacional"]) };

                                serie1.data.Add(data1);
                                serie2.data.Add(data2);
                            }

                            returnData = (Chart)chart1;
                            break;
                        case 2:
                            string sql4 = @"SELECT base.departamento.nombre AS departamento,
                                            pecuario.inventariobovinogrupo.anho AS anio,  
                                            pecuario.inventariobovinogrupo.totalmachos AS machos,
                                            pecuario.inventariobovinogrupo.totalhembras AS hembras
                                            FROM pecuario.inventariobovinogrupo
                                            INNER JOIN base.departamento ON base.departamento.codigo = pecuario.inventariobovinogrupo.codigodepto
                                            WHERE inventariobovinogrupo.anho >= " + parameters.anio_inicial + " AND inventariobovinogrupo.anho <= " + parameters.anio_final + " AND base.departamento.nombre IN (" + string.Join(",", parameters.departamento.Select(d => "'" + d + "'")) + ") ORDER BY pecuario.inventariobovinogrupo.anho, base.departamento.nombre ASC";

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
                                    double y = anioGroup.Sum(d => Convert.ToDouble(d["machos"]));
                                    var data = new Data { name = Convert.ToString(anioGroup.Key.ToString()), y = y };
                                    serie.data.Add(data);
                                }
                                chart2.series.Add(serie);
                            }

                            returnData = (Chart)chart2;
                            break;
                        case 3:
                            string sql5 = @"SELECT base.departamento.nombre AS departamento,
                                            pecuario.inventariobovinogrupo.anho AS anio,  
                                            pecuario.inventariobovinogrupo.totalmachos AS machos,
                                            pecuario.inventariobovinogrupo.totalhembras AS hembras
                                            FROM pecuario.inventariobovinogrupo
                                            INNER JOIN base.departamento ON base.departamento.codigo = pecuario.inventariobovinogrupo.codigodepto
                                            WHERE pecuario.inventariobovinogrupo.anho >= " + parameters.anio_inicial + " AND pecuario.inventariobovinogrupo.anho <= " + parameters.anio_final + " AND base.departamento.nombre IN (" + string.Join(",", parameters.departamento.Select(d => "'" + d + "'")) + ") ORDER BY pecuario.inventariobovinogrupo.anho, base.departamento.nombre ASC";

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
                                    double y = anioGroup.Sum(d => Convert.ToDouble(d["hembras"]));
                                    var data = new Data { name = Convert.ToString(anioGroup.Key.ToString()), y = y };
                                    serie.data.Add(data);
                                }
                                chart3.series.Add(serie);
                            }

                            returnData = (Chart)chart3;
                            break;

                        case 4:
                            string sql6 = @"SELECT pecuario.orientacionbovino.descripcion AS orientacion,
                                            pecuario.inventariobovinoorientacion.anho AS anio,  
                                            pecuario.inventariobovinoorientacion.total AS total_animales
                                            FROM pecuario.inventariobovinoorientacion
                                            INNER JOIN pecuario.orientacionbovino ON pecuario.orientacionbovino.codigo = pecuario.inventariobovinoorientacion.codigoorientacion
                                            WHERE pecuario.inventariobovinoorientacion.anho >= 2003 AND pecuario.inventariobovinoorientacion.anho <= 2009
                                            ORDER BY pecuario.orientacionbovino.descripcion, pecuario.inventariobovinoorientacion.anho";
                            
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

                    string sql7 = @"SELECT	base.departamento.nombre AS departamento,
                                    pecuario.orientacionbovino.descripcion AS descripcion,
                                    pecuario.inventariobovinoorientacion.anho AS anio,  
                                    pecuario.edadtipobovino.descripcion AS edad,
                                    SUM(pecuario.inventariobovinogrupo.totalmachos) AS total_machos,
                                    SUM(pecuario.inventariobovinogrupo.totalhembras) AS total_hembras
                                    FROM base.departamento
                                    INNER JOIN pecuario.inventariobovinogrupo ON base.departamento.codigo = pecuario.inventariobovinogrupo.codigodepto
                                    INNER JOIN pecuario.edadtipobovino ON pecuario.edadtipobovino.codigo = pecuario.inventariobovinogrupo.codigoedadtipobovino
                                    INNER JOIN pecuario.inventariobovinoorientacion ON base.departamento.codigo = pecuario.inventariobovinoorientacion.codigodepto
                                    INNER JOIN pecuario.orientacionbovino ON pecuario.orientacionbovino.codigo = pecuario.inventariobovinoorientacion.codigoorientacion
                                    WHERE pecuario.inventariobovinogrupo.anho >= " + parameters.anio_inicial + " AND pecuario.inventariobovinogrupo.anho <= " + parameters.anio_final + " AND base.departamento.nombre IN (" + string.Join(",", parameters.departamento.Select(d => "'" + d + "'")) + ") ";
                            sql7 += @"GROUP BY base.departamento.nombre,
                                    pecuario.orientacionbovino.codigo,
                                    pecuario.inventariobovinoorientacion.anho,  
                                    pecuario.edadtipobovino.codigo
                                    ORDER BY base.departamento.nombre, pecuario.orientacionbovino.descripcion, pecuario.inventariobovinoorientacion.anho, pecuario.edadtipobovino.codigo";

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

            string sqlString = @"SELECT base.departamento.nombre AS departamento_nombre, 
                                pecuario.produccionpecuaria.periodo,
                                COALESCE(pecuario.produccionpecuaria.produccion, 0) as produccion_pecuaria, 
                                pecuario.produccionpecuaria.unidad
                                FROM pecuario.producto INNER JOIN pecuario.produccionpecuaria ON pecuario.producto.codigo = pecuario.produccionpecuaria.producto
                                INNER JOIN base.departamento ON base.departamento.codigo = pecuario.produccionpecuaria.departamento
                                WHERE pecuario.produccionpecuaria.periodo >= "+parameters.anio_inicial+" AND pecuario.produccionpecuaria.periodo <= "+parameters.anio_final+" AND pecuario.producto.codigo = "+parameters.tipo_pecuario+" AND base.departamento.nombre IN ("+ string.Join(",", parameters.departamento.Select(d => "'" + d + "'")) +") ORDER BY base.departamento.nombre, pecuario.produccionpecuaria.periodo";

            PostgresqlAdapter adapter = new PostgresqlAdapter();
            switch (parameters.tipo)
            {
                case "parametro":

                    switch (parameters.id)
                    {
                        case 1:
                            string sql1 = @"SELECT DISTINCT 
                                            pecuario.produccionpecuaria.periodo
                                            FROM pecuario.producto INNER JOIN pecuario.produccionpecuaria ON pecuario.producto.codigo = pecuario.produccionpecuaria.producto
                                            INNER JOIN base.departamento ON base.departamento.codigo = pecuario.produccionpecuaria.departamento
                                            ORDER BY pecuario.produccionpecuaria.periodo asc";
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
                                            pecuario.producto.codigo, 
                                            pecuario.producto.nombre, 
                                            base.departamento.codigo AS departamento_codigo, 
                                            base.departamento.nombre AS departamento_nombre 
                                            FROM pecuario.producto INNER JOIN pecuario.produccionpecuaria ON pecuario.producto.codigo = pecuario.produccionpecuaria.producto
                                            INNER JOIN base.departamento ON base.departamento.codigo = pecuario.produccionpecuaria.departamento
                                            WHERE pecuario.producto.codigo = " + parameters.tipo_pecuario + " ORDER BY base.departamento.codigo";
                            DataTable data2 = adapter.GetDataTable(sql2);
                            Parameter parameter2 = new Parameter { name = "departamento", data = new List<ParameterData>() };
                            foreach (var p in (from p in data2.AsEnumerable()
                                               select p["departamento_nombre"]))
                            {
                                ParameterData param = new ParameterData { name = Convert.ToString(p), value = Convert.ToString(p) };
                                parameter2.data.Add(param);
                            }
                            returnData = (Parameter)parameter2;
                            break;
                        case 3:
                            string sql3 = @"SELECT DISTINCT
                                            pecuario.producto.codigo, 
                                            pecuario.producto.nombre
                                            FROM pecuario.producto INNER JOIN pecuario.produccionpecuaria ON pecuario.producto.codigo = pecuario.produccionpecuaria.producto
                                            INNER JOIN base.departamento ON base.departamento.codigo = pecuario.produccionpecuaria.departamento
                                            ORDER BY pecuario.producto.nombre";
                            DataTable data3 = adapter.GetDataTable(sql3);
                            Parameter parameter3 = new Parameter { name = "tipo_pecuario", data = new List<ParameterData>() };
                            foreach (var p in (from p in data3.AsEnumerable()
                                               select p))
                            {
                                ParameterData param = new ParameterData { name = Convert.ToString(p["codigo"]), value = Convert.ToString(p["nombre"]) };
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
    }
}
