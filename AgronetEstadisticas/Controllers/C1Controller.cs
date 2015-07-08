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
                                        v_dep.codigo AS departamentocod,
                                        v_dep.nombre departamento
                                        FROM agromapas.base.departamento v_dep
                                        ORDER BY v_dep.nombre ASC";
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
                                        agromapas.eva_mpal.v_evadepartamental.anho_eva as anio
                                        FROM agromapas.eva_mpal.v_evadepartamental
                                        ORDER BY agromapas.eva_mpal.v_evadepartamental.anho_eva";
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
                        string sql3 = String.Format(@"SELECT DISTINCT
                                                        agromapas.eva_mpal.v_productodetalle.codigoagronetproducto as productocod, 
                                                        agromapas.eva_mpal.v_productodetalle.nombrecomun as producto
                                                    FROM agromapas.eva_mpal.v_evadepartamental 
                                                    INNER JOIN agromapas.base.departamento ON agromapas.eva_mpal.v_evadepartamental.codigodepartamento_eva = right('0'::text || agromapas.base.departamento.codigo::VARCHAR, 2)
                                                    INNER JOIN agromapas.eva_mpal.v_productodetalle ON agromapas.eva_mpal.v_evadepartamental.codigoagronetproducto_eva = agromapas.eva_mpal.v_productodetalle.codigoagronetproducto
                                                    WHERE agromapas.base.departamento.codigo = {0}
                                                    ORDER BY agromapas.eva_mpal.v_productodetalle.nombrecomun", parameters.departamento);
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
                DataTable result = adapter.GetDataTable(String.Format(@"SELECT
                                                                        eva_anual.anho_eva as anho_eva,
                                                                        agromapas.eva_mpal.v_productodetalle.nombrecomun as producto,
                                                                        SUM(eva_anual.areacosechada_eva) as area_eva,
                                                                        SUM(eva_anual.produccion_eva) as produccion_eva,
                                                                        SUM(eva_anual.rendimiento_eva) as rendimiento,

                                                                        /** PARTICIPACION NACIONAL X DEPARTAMENTO DE LA PRODUCCION **/
                                                                        SUM(eva_anual.produccion_eva /
                                                                        (SELECT SUM(v_eva_dptal.produccion_eva)
                                                                        FROM agromapas.eva_mpal.v_evadepartamental v_eva_dptal
                                                                        INNER JOIN agromapas.base.departamento ON v_eva_dptal.codigodepartamento_eva = right('0'::text || agromapas.base.departamento.codigo::VARCHAR, 2)
                                                                        WHERE v_eva_dptal.anho_eva = eva_anual.anho_eva
                                                                        AND v_eva_dptal.codigoagronetproducto_eva = agromapas.eva_mpal.v_productodetalle.codigoagronetproducto
                                                                        GROUP BY v_eva_dptal.anho_eva)*100) as participacion_produccion_nacional,

                                                                        /** PARTICIPACION NACIONAL X DEPARTAMENTO DEL AREA COSECHADA **/
                                                                        SUM(eva_anual.areacosechada_eva /
                                                                        (SELECT SUM(v_eva_dptal.areacosechada_eva)
                                                                        FROM agromapas.eva_mpal.v_evadepartamental v_eva_dptal
                                                                        INNER JOIN agromapas.base.departamento ON v_eva_dptal.codigodepartamento_eva = right('0'::text || agromapas.base.departamento.codigo::VARCHAR, 2)
                                                                        WHERE v_eva_dptal.anho_eva = eva_anual.anho_eva
                                                                        AND v_eva_dptal.codigoagronetproducto_eva = agromapas.eva_mpal.v_productodetalle.codigoagronetproducto
                                                                        GROUP BY v_eva_dptal.anho_eva)*100) as participacion_area_nacional

                                                                        FROM agromapas.eva_mpal.v_evadepartamental eva_anual
                                                                        INNER JOIN agromapas.base.departamento ON eva_anual.codigodepartamento_eva = right('0'::text || agromapas.base.departamento.codigo::VARCHAR, 2)
                                                                        INNER JOIN agromapas.eva_mpal.v_productodetalle ON eva_anual.codigoagronetproducto_eva = agromapas.eva_mpal.v_productodetalle.codigoagronetproducto
                                                                        WHERE eva_anual.anho_eva >= {0}
                                                                        AND eva_anual.anho_eva <= {1}
                                                                        AND agromapas.eva_mpal.v_productodetalle.codigoagronetproducto = {2}
                                                                        AND agromapas.base.departamento.codigo = {3}
                                                                        GROUP BY
                                                                        eva_anual.anho_eva,
                                                                        agromapas.eva_mpal.v_productodetalle.nombrecomun,
                                                                        agromapas.base.departamento.nombre

                                                                        ORDER BY eva_anual.anho_eva", parameters.anio_inicial, parameters.anio_final, parameters.producto, parameters.departamento));
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
                            DateTime dateValue = new DateTime(Convert.ToInt32(d1["anho_eva"]), 1, 1);
                            Data data1 = new Data { name = Convert.ToString(ToUnixTimestamp(dateValue)), y = Convert.ToDouble(d1["produccion_eva"]) };
                            Data data2 = new Data { name = Convert.ToString(ToUnixTimestamp(dateValue)), y = Convert.ToDouble(d1["area_eva"]) };

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
                            DateTime dateValue = new DateTime(Convert.ToInt32(d1["anho_eva"]), 1, 1);
                            Data data1 = new Data { name = Convert.ToString(ToUnixTimestamp(dateValue)), y = Convert.ToDouble(d1["rendimiento"]) };
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
                            DateTime dateValue = new DateTime(Convert.ToInt32(d1["anho_eva"]), 1, 1);
                            Data data1 = new Data { name = Convert.ToString(ToUnixTimestamp(dateValue)), y = Convert.ToDouble(d1["participacion_produccion_nacional"]) };
                            Data data2 = new Data { name = Convert.ToString(ToUnixTimestamp(dateValue)), y = Convert.ToDouble(d1["participacion_area_nacional"]) };

                            serie4.data.Add(data1);
                            serie5.data.Add(data2);
                        }

                        returnData = (Chart)chart3;
                        break;
                   
                }

            }
            else if (parameters.tipo == "tabla")
            {
                switch (parameters.id)
                {
                    case 1:
                        Table table = new Table
                        {
                            rows = adapter.GetDataTable(String.Format(@"SELECT
                                                                eva_anual.anho_eva as anho_eva,
                                                                agromapas.base.departamento.nombre,
                                                                agromapas.eva_mpal.v_productodetalle.nombrecomun as producto,
                                                                SUM(eva_anual.areacosechada_eva) as area_eva,
                                                                SUM(eva_anual.produccion_eva) as produccion_eva,
                                                                SUM(eva_anual.rendimiento_eva) as rendimiento,

                                                                /** PARTICIPACION NACIONAL X DEPARTAMENTO DE LA PRODUCCION **/
                                                                SUM(eva_anual.produccion_eva /
                                                                    (SELECT SUM(v_eva_dptal.produccion_eva)
                                                                FROM agromapas.eva_mpal.v_evadepartamental v_eva_dptal
                                                                INNER JOIN agromapas.base.departamento ON v_eva_dptal.codigodepartamento_eva =  right('0'::text || agromapas.base.departamento.codigo::VARCHAR, 2)
                                                                WHERE v_eva_dptal.anho_eva = eva_anual.anho_eva
                                                                AND v_eva_dptal.codigoagronetproducto_eva = agromapas.eva_mpal.v_productodetalle.codigoagronetproducto
                                                                GROUP BY v_eva_dptal.anho_eva)*100) as participacion_produccion_nacional,

                                                                /** PARTICIPACION NACIONAL X DEPARTAMENTO DEL AREA COSECHADA **/
                                                                SUM(eva_anual.areacosechada_eva /
                                                                    (SELECT SUM(v_eva_dptal.areacosechada_eva)
                                                                FROM agromapas.eva_mpal.v_evadepartamental v_eva_dptal
                                                                INNER JOIN agromapas.base.departamento ON v_eva_dptal.codigodepartamento_eva =  right('0'::text || agromapas.base.departamento.codigo::VARCHAR, 2)
                                                                WHERE v_eva_dptal.anho_eva = eva_anual.anho_eva
                                                                AND v_eva_dptal.codigoagronetproducto_eva = agromapas.eva_mpal.v_productodetalle.codigoagronetproducto
                                                                GROUP BY v_eva_dptal.anho_eva)*100) as participacion_area_nacional

                                                                FROM agromapas.eva_mpal.v_evadepartamental eva_anual
                                                                INNER JOIN agromapas.base.departamento ON eva_anual.codigodepartamento_eva =  right('0'::text || agromapas.base.departamento.codigo::VARCHAR, 2)
                                                                INNER JOIN agromapas.eva_mpal.v_productodetalle ON eva_anual.codigoagronetproducto_eva = agromapas.eva_mpal.v_productodetalle.codigoagronetproducto
                                                                WHERE eva_anual.anho_eva >= {0}
                                                                AND eva_anual.anho_eva <= {1}
                                                                AND agromapas.eva_mpal.v_productodetalle.codigoagronetproducto = {2}
                                                                AND agromapas.base.departamento.codigo = '{3}'
                                                                GROUP BY
                                                                eva_anual.anho_eva,
                                                                agromapas.eva_mpal.v_productodetalle.nombrecomun,
                                                                agromapas.base.departamento.nombre

                                                                ORDER BY eva_anual.anho_eva", parameters.anio_inicial, parameters.anio_final, parameters.producto, parameters.departamento))
                        };
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
                                agromapas.eva_mpal.v_evadepartamental.anho_eva as anho_eva
                            FROM agromapas.eva_mpal.v_evadepartamental INNER JOIN agromapas.base.departamento ON agromapas.eva_mpal.v_evadepartamental.codigodepartamento_eva =  right('0'::text || agromapas.base.departamento.codigo::VARCHAR, 2)
                                INNER JOIN agromapas.eva_mpal.v_productodetalle ON agromapas.eva_mpal.v_evadepartamental.codigoagronetproducto_eva = agromapas.eva_mpal.v_productodetalle.codigoagronetproducto
                            WHERE 
                                agromapas.eva_mpal.v_productodetalle.codigoagronetproducto = " + parameters.producto + @"
                            ORDER BY agromapas.eva_mpal.v_evadepartamental.anho_eva";
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
                                agromapas.eva_mpal.v_productodetalle.codigoagronetproducto as productocod, 
                                agromapas.eva_mpal.v_productodetalle.nombrecomun as producto
                            FROM agromapas.eva_mpal.v_evadepartamental INNER JOIN agromapas.base.departamento ON agromapas.eva_mpal.v_evadepartamental.codigodepartamento_eva =  right('0'::text || agromapas.base.departamento.codigo::VARCHAR, 2)
                                INNER JOIN agromapas.eva_mpal.v_productodetalle ON agromapas.eva_mpal.v_evadepartamental.codigoagronetproducto_eva = agromapas.eva_mpal.v_productodetalle.codigoagronetproducto

                            ORDER BY agromapas.eva_mpal.v_productodetalle.nombrecomun
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
                    DataTable results = adapter.GetDataTable(String.Format(@"SELECT
                                                                eva_anual.anho_eva as anho_eva,
                                                                SUM(eva_anual.areacosechada_eva) as area_eva,
                                                                SUM(eva_anual.produccion_eva) as produccion_eva,
                                                                SUM(eva_anual.rendimiento_eva) as rendimiento
                                                                
                                                                FROM agromapas.eva_mpal.v_evadepartamental eva_anual
                                                                INNER JOIN agromapas.base.departamento ON eva_anual.codigodepartamento_eva =  right('0'::text || agromapas.base.departamento.codigo::VARCHAR, 2)
                                                                INNER JOIN agromapas.eva_mpal.v_productodetalle ON eva_anual.codigoagronetproducto_eva = agromapas.eva_mpal.v_productodetalle.codigoagronetproducto
                                                                WHERE eva_anual.anho_eva >= {0}
                                                                AND eva_anual.anho_eva <= {1}
                                                                AND agromapas.eva_mpal.v_productodetalle.codigoagronetproducto = {2}

                                                                GROUP BY eva_anual.anho_eva
                                                                ORDER BY eva_anual.anho_eva", parameters.anio_inicial, parameters.anio_final, parameters.producto));

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
                                DateTime dateValue = new DateTime(Convert.ToInt32(d1["anho_eva"]), 1, 1);
                                Data data1 = new Data { name = Convert.ToString(ToUnixTimestamp(dateValue)), y = Convert.ToDouble(d1["produccion_eva"]) };
                                Data data2 = new Data { name = Convert.ToString(ToUnixTimestamp(dateValue)), y = Convert.ToDouble(d1["area_eva"]) };

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
                                DateTime dateValue = new DateTime(Convert.ToInt32(d1["anho_eva"]), 1, 1);
                                Data data1 = new Data { name = Convert.ToString(ToUnixTimestamp(dateValue)), y = Convert.ToDouble(d1["rendimiento"]) };


                                serie3.data.Add(data1);

                            }

                            returnData = (Chart)chart2;
                            break;
                    }

                    break;
                case "tabla":
                    switch (parameters.id)
                    {
                        case 1:
                            Table table = new Table
                            {
                                rows = adapter.GetDataTable(String.Format(@"SELECT
                                                                eva_anual.anho_eva as anho_eva,
                                                                agromapas.eva_mpal.v_productodetalle.nombrecomun as producto,
                                                                agromapas.base.departamento.nombre,
                                                                SUM(eva_anual.areacosechada_eva) as area_eva,
                                                                SUM(eva_anual.produccion_eva) as produccion_eva,
                                                                SUM(eva_anual.rendimiento_eva) as rendimiento,

                                                                /** PARTICIPACION NACIONAL X DEPARTAMENTO DE LA PRODUCCION **/
                                                                SUM(eva_anual.produccion_eva /
                                                                    (SELECT SUM(v_eva_dptal.produccion_eva)
                                                                FROM agromapas.eva_mpal.v_evadepartamental v_eva_dptal
                                                                INNER JOIN agromapas.base.departamento ON v_eva_dptal.codigodepartamento_eva =  right('0'::text || agromapas.base.departamento.codigo::VARCHAR, 2)
                                                                WHERE v_eva_dptal.anho_eva = eva_anual.anho_eva
                                                                AND v_eva_dptal.codigoagronetproducto_eva = agromapas.eva_mpal.v_productodetalle.codigoagronetproducto
                                                                GROUP BY v_eva_dptal.anho_eva)*100) as participacion_produccion_nacional,

                                                                /** PARTICIPACION NACIONAL X DEPARTAMENTO DEL AREA COSECHADA **/
                                                                SUM(eva_anual.areacosechada_eva /
                                                                    (SELECT SUM(v_eva_dptal.areacosechada_eva)
                                                                FROM agromapas.eva_mpal.v_evadepartamental v_eva_dptal
                                                                INNER JOIN agromapas.base.departamento ON v_eva_dptal.codigodepartamento_eva =  right('0'::text || agromapas.base.departamento.codigo::VARCHAR, 2)
                                                                WHERE v_eva_dptal.anho_eva = eva_anual.anho_eva
                                                                AND v_eva_dptal.codigoagronetproducto_eva = agromapas.eva_mpal.v_productodetalle.codigoagronetproducto
                                                                GROUP BY v_eva_dptal.anho_eva)*100) as participacion_area_nacional

                                                                FROM agromapas.eva_mpal.v_evadepartamental eva_anual
                                                                INNER JOIN agromapas.base.departamento ON eva_anual.codigodepartamento_eva =  right('0'::text || agromapas.base.departamento.codigo::VARCHAR, 2)
                                                                INNER JOIN agromapas.eva_mpal.v_productodetalle ON eva_anual.codigoagronetproducto_eva = agromapas.eva_mpal.v_productodetalle.codigoagronetproducto
                                                                WHERE eva_anual.anho_eva >= {0}
                                                                AND eva_anual.anho_eva <= {1}
                                                                AND agromapas.eva_mpal.v_productodetalle.codigoagronetproducto = {2}

                                                                GROUP BY
                                                                eva_anual.anho_eva,
                                                                agromapas.eva_mpal.v_productodetalle.nombrecomun,
                                                                agromapas.base.departamento.nombre

                                                                ORDER BY eva_anual.anho_eva", parameters.anio_inicial, parameters.anio_final, parameters.producto))
                            };
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
                                  agromapas.eva_mpal.v_evadepartamental ev,
                                  agromapas.base.departamento b,
                                  agromapas.eva_mpal.producto ep
                                WHERE
                                  right('0'::text || b.codigo::VARCHAR, 2) = ev.codigodepartamento_eva AND
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
                           
                            Parameter parameter2 = new Parameter { name = "departamento", data = new List<ParameterData>() };
                            foreach (var p in (from p in adapter.GetDataTable(String.Format(@"SELECT DISTINCT
                                          ev.codigodepartamento_eva as departamentocod,
                                          b.nombre as departamento
                                        FROM
                                          agromapas.eva_mpal.v_evadepartamental ev,
                                          agromapas.base.departamento b,
                                          agromapas.eva_mpal.producto ep
                                        WHERE
                                          right('0'::text || b.codigo::VARCHAR, 2) = ev.codigodepartamento_eva AND
                                          ep.codigoagronetcultivo = ev.codigoagronetproducto_eva
                                          /*PARAMETROS*/
                                          AND ev.codigoagronetproducto_eva = {0}
                                          ORDER BY b.nombre;", parameters.producto)).AsEnumerable()
                                               select p))
                            {
                                ParameterData param = new ParameterData { name = Convert.ToString(p["departamento"]).Trim(), value = Convert.ToString(p["departamentocod"]).Trim() };
                                parameter2.data.Add(param);
                            }
                            returnData = (Parameter)parameter2;
                            break;
                        case 3:
                            string sql3 = @"SELECT DISTINCT
                                              ev.anho_eva
 
                                            FROM
                                              agromapas.eva_mpal.v_evadepartamental ev,
                                              agromapas.base.departamento b,
                                              agromapas.eva_mpal.producto ep
                                            WHERE
                                              right('0'::text || b.codigo::VARCHAR, 2) = ev.codigodepartamento_eva AND
                                              ep.codigoagronetcultivo = ev.codigoagronetproducto_eva
                                              /*PARAMETROS*/
                                              AND ev.codigoagronetproducto_eva = " + parameters.producto + @"
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

                    DataTable results = adapter.GetDataTable(String.Format(@"SELECT
                                                                eva_anual.anho_eva as anho_eva,
                                                                agromapas.eva_mpal.v_productodetalle.nombrecomun as producto,
                                                                agromapas.base.departamento.nombre as departamento,
                                                                SUM(eva_anual.areacosechada_eva) as area_eva,
                                                                SUM(eva_anual.produccion_eva) as produccion_eva,
                                                                SUM(eva_anual.rendimiento_eva) as rendimiento,

                                                                /** PARTICIPACION NACIONAL X DEPARTAMENTO DE LA PRODUCCION **/
                                                                SUM(eva_anual.produccion_eva /
                                                                    (SELECT SUM(v_eva_dptal.produccion_eva)
                                                                FROM agromapas.eva_mpal.v_evadepartamental v_eva_dptal
                                                                INNER JOIN agromapas.base.departamento ON v_eva_dptal.codigodepartamento_eva =  right('0'::text || agromapas.base.departamento.codigo::VARCHAR, 2)
                                                                WHERE v_eva_dptal.anho_eva = eva_anual.anho_eva
                                                                AND v_eva_dptal.codigoagronetproducto_eva = agromapas.eva_mpal.v_productodetalle.codigoagronetproducto
                                                                GROUP BY v_eva_dptal.anho_eva)*100) as participacion_produccion_nacional,

                                                                /** PARTICIPACION NACIONAL X DEPARTAMENTO DEL AREA COSECHADA **/
                                                                SUM(eva_anual.areacosechada_eva /
                                                                    (SELECT SUM(v_eva_dptal.areacosechada_eva)
                                                                FROM agromapas.eva_mpal.v_evadepartamental v_eva_dptal
                                                                INNER JOIN agromapas.base.departamento ON v_eva_dptal.codigodepartamento_eva =  right('0'::text || agromapas.base.departamento.codigo::VARCHAR, 2)
                                                                WHERE v_eva_dptal.anho_eva = eva_anual.anho_eva
                                                                AND v_eva_dptal.codigoagronetproducto_eva = agromapas.eva_mpal.v_productodetalle.codigoagronetproducto
                                                                GROUP BY v_eva_dptal.anho_eva)*100) as participacion_area_nacional

                                                                FROM agromapas.eva_mpal.v_evadepartamental eva_anual
                                                                INNER JOIN agromapas.base.departamento ON eva_anual.codigodepartamento_eva =  right('0'::text || agromapas.base.departamento.codigo::VARCHAR, 2)
                                                                INNER JOIN agromapas.eva_mpal.v_productodetalle ON eva_anual.codigoagronetproducto_eva = agromapas.eva_mpal.v_productodetalle.codigoagronetproducto
                                                                WHERE eva_anual.anho_eva >= {0}
                                                                AND eva_anual.anho_eva <= {1}
                                                                AND agromapas.eva_mpal.v_productodetalle.codigoagronetproducto = {2}
                                                                AND eva_anual.codigodepartamento_eva IN (" + string.Join(",", parameters.departamento.Select(d => "'" + d + "'")) + @")
                                                                GROUP BY
                                                                eva_anual.anho_eva,
                                                                agromapas.eva_mpal.v_productodetalle.nombrecomun,
                                                                agromapas.base.departamento.nombre

                                                                ORDER BY eva_anual.anho_eva", parameters.anio_inicial, parameters.anio_final, parameters.producto));

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
                                    DateTime dateValue = new DateTime(Convert.ToInt32(el1["anho_eva"]), 1, 1);
                                    var data = new Data { name = Convert.ToString(ToUnixTimestamp(dateValue)), y = Convert.ToDouble(el1["produccion_eva"]) };
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
                                    DateTime dateValue = new DateTime(Convert.ToInt32(el1["anho_eva"]), 1, 1);
                                    var data = new Data { name = Convert.ToString(ToUnixTimestamp(dateValue)), y = Convert.ToDouble(el1["area_eva"]) };
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
                                    DateTime dateValue = new DateTime(Convert.ToInt32(el1["anho_eva"]), 1, 1);
                                    var data = new Data { name = Convert.ToString(ToUnixTimestamp(dateValue)), y = Convert.ToDouble(el1["rendimiento"]) };
                                    serie1.data.Add(data);

                                }
                                chart3.series.Add(serie1);
                            }

                            returnData = (Chart)chart3;
                            break;
                        case 4:

                            Chart chart4 = new Chart { subtitle = "", series = new List<Series>() };

                            var query4 = from r in results.AsEnumerable()
                                         group r by r["departamento"];

                            foreach (var deptosGroup in query4)
                            {
                                var serie1 = new Series { name = deptosGroup.Key.ToString(), data = new List<Data>() };
                                foreach (var el1 in deptosGroup)
                                {
                                    DateTime dateValue = new DateTime(Convert.ToInt32(el1["anho_eva"]), 1, 1);
                                    var data = new Data { name = Convert.ToString(ToUnixTimestamp(dateValue)), y = Convert.ToDouble(el1["participacion_produccion_nacional"]) };
                                    serie1.data.Add(data);

                                }
                                chart4.series.Add(serie1);
                            }

                            returnData = (Chart)chart4;
                            break;
                        case 5:

                            Chart chart5 = new Chart { subtitle = "", series = new List<Series>() };

                            var query5 = from r in results.AsEnumerable()
                                         group r by r["departamento"];

                            foreach (var deptosGroup in query5)
                            {
                                var serie1 = new Series { name = deptosGroup.Key.ToString(), data = new List<Data>() };
                                foreach (var el1 in deptosGroup)
                                {
                                    DateTime dateValue = new DateTime(Convert.ToInt32(el1["anho_eva"]), 1, 1);
                                    var data = new Data { name = Convert.ToString(ToUnixTimestamp(dateValue)), y = Convert.ToDouble(el1["participacion_area_nacional"]) };
                                    serie1.data.Add(data);

                                }
                                chart5.series.Add(serie1);
                            }

                            returnData = (Chart)chart5;
                            break;
                    }

                    break;
                case "tabla":
                    
                    switch (parameters.id)
                    {
                        case 1:
                            Table table = new Table { rows = adapter.GetDataTable(String.Format(@"SELECT
                                                                eva_anual.anho_eva as anho_eva,
                                                                agromapas.eva_mpal.v_productodetalle.nombrecomun as producto,
                                                                agromapas.base.departamento.nombre as departamento,
                                                                SUM(eva_anual.areacosechada_eva) as area_eva,
                                                                SUM(eva_anual.produccion_eva) as produccion_eva,
                                                                SUM(eva_anual.rendimiento_eva) as rendimiento,

                                                                /** PARTICIPACION NACIONAL X DEPARTAMENTO DE LA PRODUCCION **/
                                                                SUM(eva_anual.produccion_eva /
                                                                    (SELECT SUM(v_eva_dptal.produccion_eva)
                                                                FROM agromapas.eva_mpal.v_evadepartamental v_eva_dptal
                                                                INNER JOIN agromapas.base.departamento ON v_eva_dptal.codigodepartamento_eva =  right('0'::text || agromapas.base.departamento.codigo::VARCHAR, 2)
                                                                WHERE v_eva_dptal.anho_eva = eva_anual.anho_eva
                                                                AND v_eva_dptal.codigoagronetproducto_eva = agromapas.eva_mpal.v_productodetalle.codigoagronetproducto
                                                                GROUP BY v_eva_dptal.anho_eva)*100) as participacion_produccion_nacional,

                                                                /** PARTICIPACION NACIONAL X DEPARTAMENTO DEL AREA COSECHADA **/
                                                                SUM(eva_anual.areacosechada_eva /
                                                                    (SELECT SUM(v_eva_dptal.areacosechada_eva)
                                                                FROM agromapas.eva_mpal.v_evadepartamental v_eva_dptal
                                                                INNER JOIN agromapas.base.departamento ON v_eva_dptal.codigodepartamento_eva =  right('0'::text || agromapas.base.departamento.codigo::VARCHAR, 2)
                                                                WHERE v_eva_dptal.anho_eva = eva_anual.anho_eva
                                                                AND v_eva_dptal.codigoagronetproducto_eva = agromapas.eva_mpal.v_productodetalle.codigoagronetproducto
                                                                GROUP BY v_eva_dptal.anho_eva)*100) as participacion_area_nacional

                                                                FROM agromapas.eva_mpal.v_evadepartamental eva_anual
                                                                INNER JOIN agromapas.base.departamento ON eva_anual.codigodepartamento_eva =  right('0'::text || agromapas.base.departamento.codigo::VARCHAR, 2)
                                                                INNER JOIN agromapas.eva_mpal.v_productodetalle ON eva_anual.codigoagronetproducto_eva = agromapas.eva_mpal.v_productodetalle.codigoagronetproducto
                                                                WHERE eva_anual.anho_eva >= {0}
                                                                AND eva_anual.anho_eva <= {1}
                                                                AND agromapas.eva_mpal.v_productodetalle.codigoagronetproducto = {2}
                                                                AND eva_anual.codigodepartamento_eva IN (" + string.Join(",", parameters.departamento.Select(d => "'" + d + "'")) + @")
                                                                GROUP BY
                                                                eva_anual.anho_eva,
                                                                agromapas.eva_mpal.v_productodetalle.nombrecomun,
                                                                agromapas.base.departamento.nombre

                                                                ORDER BY eva_anual.anho_eva", parameters.anio_inicial, parameters.anio_final, parameters.producto)) };
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

            switch (parameters.tipo)
            {
                case "parametro":

                    switch (parameters.id)
                    {
                        case 1:
                            string sql1 = @"SELECT DISTINCT
                                          ev.anho_eva 
                                        FROM 
                                          agromapas.eva_mpal.v_evadepartamental ev, 
                                          agromapas.base.departamento b, 
                                          agromapas.eva_mpal.producto ep
                                        WHERE 
                                          right('0'::text || b.codigo::VARCHAR, 2) = ev.codigodepartamento_eva AND
                                          ep.codigoagronetcultivo = ev.codigoagronetproducto_eva
                                          /*PARAMETROS*/
                                          AND ev.codigoagronetproducto_eva = " + parameters.producto + @"
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
                                          agromapas.eva_mpal.v_evadepartamental ev, 
                                          agromapas.base.departamento b, 
                                          agromapas.eva_mpal.producto ep
                                        WHERE 
                                          right('0'::text || b.codigo::VARCHAR, 2) = ev.codigodepartamento_eva AND
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

                    DataTable results = adapter.GetDataTable(String.Format(@"SELECT
                                                                agromapas.eva_mpal.v_productodetalle.nombrecomun as producto,
                                                                agromapas.base.departamento.nombre as departamento,
                                                                SUM(eva_anual.areacosechada_eva) as area_eva,
                                                                SUM(eva_anual.produccion_eva) as produccion_eva,
                                                                SUM(eva_anual.rendimiento_eva) as rendimiento_eva
                                                                FROM agromapas.eva_mpal.v_evadepartamental eva_anual
                                                                INNER JOIN agromapas.base.departamento ON eva_anual.codigodepartamento_eva =  right('0'::text || agromapas.base.departamento.codigo::VARCHAR, 2)
                                                                INNER JOIN agromapas.eva_mpal.v_productodetalle ON eva_anual.codigoagronetproducto_eva = agromapas.eva_mpal.v_productodetalle.codigoagronetproducto
                                                                WHERE eva_anual.anho_eva = {0}
                                                                AND agromapas.eva_mpal.v_productodetalle.codigoagronetproducto = {1}
                                                                GROUP BY
                                                                eva_anual.codigodepartamento_eva, agromapas.base.departamento.nombre, agromapas.eva_mpal.v_productodetalle.nombrecomun
                                                                ORDER BY agromapas.base.departamento.nombre", parameters.anio, parameters.producto));
                    switch (parameters.id)
                    {
                        case 1:
                            Chart chart1 = new Chart { subtitle = "", series = new List<Series>() };

                            foreach (var productoGroup in (from r in results.AsEnumerable()
                                                           group r by r["producto"]))
                            {
                                var serie1 = new Series { name = productoGroup.Key.ToString().Trim(), data = new List<Data>() };
                                foreach (var el1 in productoGroup)
                                {
                                    var data = new Data { name = Convert.ToString(el1["departamento"]).Trim(), y = Convert.ToDouble(el1[parameters.ordenamiento]) };
                                    serie1.data.Add(data);

                                }
                                chart1.series.Add(serie1);
                            }

                            returnData = (Chart)chart1;
                            break;
                    }

                    break;

                case "tabla":

                    switch (parameters.id)
                    {
                        case 1:
                            Table table = new Table
                            {
                                rows = adapter.GetDataTable(String.Format(@"SELECT
                                                                eva_anual.anho_eva as anho_eva,
                                                                agromapas.eva_mpal.v_productodetalle.nombrecomun as producto,
                                                                agromapas.base.departamento.nombre as departamento,
                                                                SUM(eva_anual.areacosechada_eva) as area_eva,
                                                                SUM(eva_anual.produccion_eva) as produccion_eva,
                                                                SUM(eva_anual.rendimiento_eva) as rendimiento
                                                                FROM agromapas.eva_mpal.v_evadepartamental eva_anual
                                                                INNER JOIN agromapas.base.departamento ON eva_anual.codigodepartamento_eva =  right('0'::text || agromapas.base.departamento.codigo::VARCHAR, 2)
                                                                INNER JOIN agromapas.eva_mpal.v_productodetalle ON eva_anual.codigoagronetproducto_eva = agromapas.eva_mpal.v_productodetalle.codigoagronetproducto
                                                                WHERE eva_anual.anho_eva = {0}
                                                                AND agromapas.eva_mpal.v_productodetalle.codigoagronetproducto = {1}
                                                                GROUP BY
                                                                eva_anual.anho_eva,
                                                                agromapas.eva_mpal.v_productodetalle.nombrecomun,
                                                                agromapas.base.departamento.nombre

                                                                ORDER BY eva_anual.anho_eva, agromapas.base.departamento.nombre", parameters.anio, parameters.producto))
                            };
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

        [Route("api/Report/105")]
        public IHttpActionResult postReport105(report105 parameters)
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
                                          v_evadepartamental.anho_eva as anho_eva
  
                                        FROM 
                                          agromapas.eva_mpal.v_evadepartamental, 
                                          agromapas.base.departamento, 
                                          agromapas.eva_mpal.producto
                                        WHERE 
                                          right('0'::text || departamento.codigo::VARCHAR, 2) = v_evadepartamental.codigodepartamento_eva AND
                                          producto.codigoagronetcultivo = v_evadepartamental.codigoagronetproducto_eva
                                          /*PARAMETROS*/
                                          AND departamento.codigo = " + parameters.departamento + @"
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
                                          agromapas.eva_mpal.v_evadepartamental, 
                                          agromapas.base.departamento, 
                                          agromapas.eva_mpal.producto
                                        WHERE 
                                          right('0'::text || departamento.codigo::VARCHAR, 2) = v_evadepartamental.codigodepartamento_eva AND
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

                    DataTable results = adapter.GetDataTable(String.Format(@"SELECT
                                                                agromapas.base.departamento.nombre as departamento,
                                                                agromapas.eva_mpal.v_productodetalle.nombrecomun as producto,
                                                                SUM(eva_anual.areacosechada_eva) as area_eva,
                                                                SUM(eva_anual.produccion_eva) as produccion_eva,
                                                                SUM(eva_anual.rendimiento_eva) as rendimiento
                                                                FROM agromapas.eva_mpal.v_evadepartamental eva_anual
                                                                INNER JOIN agromapas.base.departamento ON eva_anual.codigodepartamento_eva =  right('0'::text || agromapas.base.departamento.codigo::VARCHAR, 2)
                                                                INNER JOIN agromapas.eva_mpal.v_productodetalle ON eva_anual.codigoagronetproducto_eva = agromapas.eva_mpal.v_productodetalle.codigoagronetproducto
                                                                WHERE eva_anual.anho_eva >= {0}
                                                                AND eva_anual.anho_eva <= {1}
                                                                AND eva_anual.codigodepartamento_eva = '{2}'
                                                                GROUP BY
                                                                eva_anual.codigoagronetproducto_eva,
                                                                agromapas.eva_mpal.v_productodetalle.nombrecomun,
                                                                agromapas.base.departamento.nombre
                                                                ORDER BY agromapas.eva_mpal.v_productodetalle.nombrecomun", parameters.anio_inicial, parameters.anio_final, parameters.departamento));
                    switch (parameters.id)
                    {
                        case 1:
                            Chart chart1 = new Chart { subtitle = "", series = new List<Series>() };

                            var query1 = from r in results.AsEnumerable()
                                         group r by r["departamento"] into deptoGroup
                                         from productGroup in
                                             (from d in deptoGroup
                                              group d by d["producto"])
                                         group productGroup by deptoGroup.Key;

                            foreach (var deptoGroup in query1)
                            {
                                var serie1 = new Series { name = deptoGroup.Key.ToString().Trim(), data = new List<Data>() };
                                foreach (var productGroup in deptoGroup)
                                {
                                    double y = productGroup.Sum(d => Convert.ToDouble(d["area_eva"]));
                                    var data = new Data { name = Convert.ToString(productGroup.Key.ToString().Trim()), y = y };
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
                                         from productGroup in
                                             (from d in deptoGroup
                                              group d by d["producto"])
                                         group productGroup by deptoGroup.Key;

                            foreach (var deptoGroup in query2)
                            {
                                var serie1 = new Series { name = deptoGroup.Key.ToString().Trim(), data = new List<Data>() };
                                foreach (var productGroup in deptoGroup)
                                {
                                    double y = productGroup.Sum(d => Convert.ToDouble(d["produccion_eva"]));
                                    var data = new Data { name = Convert.ToString(productGroup.Key.ToString().Trim()), y = y };
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
                            Table table = new Table
                            {
                                rows = adapter.GetDataTable(String.Format(@"SELECT
                                                                agromapas.base.departamento.nombre as departamento,
                                                                agromapas.eva_mpal.v_productodetalle.nombrecomun as producto,
                                                                eva_anual.anho_eva,
                                                                SUM(eva_anual.areacosechada_eva) as area_eva,
                                                                SUM(eva_anual.produccion_eva) as produccion_eva,
                                                                SUM(eva_anual.rendimiento_eva) as rendimiento
                                                                FROM agromapas.eva_mpal.v_evadepartamental eva_anual
                                                                INNER JOIN agromapas.base.departamento ON eva_anual.codigodepartamento_eva =  right('0'::text || agromapas.base.departamento.codigo::VARCHAR, 2)
                                                                INNER JOIN agromapas.eva_mpal.v_productodetalle ON eva_anual.codigoagronetproducto_eva = agromapas.eva_mpal.v_productodetalle.codigoagronetproducto
                                                                WHERE eva_anual.anho_eva >= {0}
                                                                AND eva_anual.anho_eva <= {1}
                                                                AND eva_anual.codigodepartamento_eva = '{2}'
                                                                GROUP BY
                                                                eva_anual.codigoagronetproducto_eva,
                                                                agromapas.eva_mpal.v_productodetalle.nombrecomun,
                                                                agromapas.base.departamento.nombre,
                                                                eva_anual.anho_eva
                                                                ORDER BY agromapas.eva_mpal.v_productodetalle.nombrecomun, eva_anual.anho_eva", parameters.anio_inicial, parameters.anio_final, parameters.departamento))
                            };
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
                                              agromapas.pecuario.inventariobovinogrupo pi, 
                                              agromapas.base.departamento b, 
                                              agromapas.pecuario.orientacionbovino po
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
                                          agromapas.pecuario.inventariobovinogrupo pi, 
                                          agromapas.base.departamento b, 
                                          agromapas.pecuario.orientacionbovino po
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
                                              agromapas.pecuario.inventariobovinogrupo p, 
                                              agromapas.pecuario.orientacionbovino po
                                            WHERE 
                                              po.codigo = p.codigoedadtipobovino
                                              AND p.anho = inventariobovinogrupo.anho

                                            GROUP BY p.anho) as total_machos_nal,
                                          (SELECT 
                                              SUM(p.totalhembras) hembras_nal
                                            FROM 
                                              agromapas.pecuario.inventariobovinogrupo p, 
                                              agromapas.pecuario.orientacionbovino po
                                            WHERE 
                                              po.codigo = p.codigoedadtipobovino
                                              AND p.anho = inventariobovinogrupo.anho

                                            GROUP BY p.anho) as total_hembras_nal
                                        FROM 
                                          agromapas.pecuario.inventariobovinogrupo, 
                                          agromapas.base.departamento, 
                                          agromapas.pecuario.orientacionbovino
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
                                DateTime dateValue = new DateTime(Convert.ToInt32(d1["anho"]), 1, 1);
                                Data data1 = new Data { name = Convert.ToString(ToUnixTimestamp(dateValue)), y = Convert.ToDouble(d1["total_machos_nal"]) };
                                Data data2 = new Data { name = Convert.ToString(ToUnixTimestamp(dateValue)), y = Convert.ToDouble(d1["total_hembras_nal"]) };

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
                                          agromapas.pecuario.inventariobovinogrupo pi, 
                                          agromapas.base.departamento b, 
                                          agromapas.pecuario.orientacionbovino po
                                        WHERE 
                                          b.codigo = pi.codigodepto AND
                                          po.codigo = pi.codigoedadtipobovino
                                          /*PARAMETROS*/
                                          AND pi.anho >= " + parameters.anio_inicial + @" AND pi.anho <= " + parameters.anio_final + @"
                                          AND pi.codigodepto in (" + string.Join(",", parameters.departamento.Select(d => "'" + d + "'")) + @")
                                        GROUP BY pi.anho, pi.codigodepto, b.nombre, pi.codigoedadtipobovino, 
                                          po.descripcion
                                        ORDER BY pi.anho, pi.codigoedadtipobovino";

                            DataTable results2 = adapter.GetDataTable(sql4);
                            Chart chart2 = new Chart { subtitle = "", series = new List<Series>() };

                            var query2 = from r in results2.AsEnumerable()
                                         group r by r["departamento"] into deptoGroup
                                         from anioGroup in
                                             (from d in deptoGroup
                                              group d by d["anio"])
                                         group anioGroup by deptoGroup.Key;

                            foreach (var deptoGroup in query2)
                            {
                                var serie = new Series { name = deptoGroup.Key.ToString(), data = new List<Data>() };
                                foreach (var anioGroup in deptoGroup)
                                {
                                    double y = anioGroup.Sum(d => Convert.ToDouble(d["total_machos_depto"]));
                                    DateTime dateValue = new DateTime(Convert.ToInt32(anioGroup.Key.ToString()), 1, 1);
                                    var data = new Data { name = Convert.ToString(ToUnixTimestamp(dateValue)), y = y };
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
                                          agromapas.pecuario.inventariobovinogrupo pi, 
                                          agromapas.base.departamento b, 
                                          agromapas.pecuario.orientacionbovino po
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
                                         from anioGroup in
                                             (from d in deptoGroup
                                              group d by d["anio"])
                                         group anioGroup by deptoGroup.Key;

                            foreach (var deptoGroup in query3)
                            {
                                var serie = new Series { name = deptoGroup.Key.ToString(), data = new List<Data>() };
                                foreach (var anioGroup in deptoGroup)
                                {
                                    double y = anioGroup.Sum(d => Convert.ToDouble(d["total_hembras_depto"]));
                                    DateTime dateValue = new DateTime(Convert.ToInt32(anioGroup.Key.ToString()), 1, 1);
                                    var data = new Data { name = Convert.ToString(ToUnixTimestamp(dateValue)), y = y };
                                    serie.data.Add(data);
                                }
                                chart3.series.Add(serie);
                            }

                            returnData = (Chart)chart3;
                            break;

                        case 4:
                            DataTable results4 = adapter.GetDataTable(@"SELECT 
                                                                        pi.anho as anio, 
                                                                        pi.codigoedadtipobovino as codigoedadbovino, 
                                                                        po.descripcion as orientacion, 
                                                                        SUM(pi.totalhembras) + SUM(totalmachos) AS total_animales
                                                                        FROM agromapas.pecuario.inventariobovinogrupo pi
                                                                        INNER JOIN agromapas.pecuario.orientacionbovino po ON po.codigo = pi.codigoedadtipobovino
                                                                        WHERE 
                                                                        /*PARAMETROS*/
                                                                        pi.anho >= " + parameters.anio_inicial + @" AND pi.anho <= " + parameters.anio_final + @"
                                                                        GROUP BY pi.anho, pi.codigoedadtipobovino, po.descripcion
                                                                        ORDER BY pi.anho, pi.codigoedadtipobovino");
                            Chart chart4 = new Chart { subtitle = "", series = new List<Series>() };

                            foreach (var productGroup in (from r in results4.AsEnumerable()
                                                          group r by r["orientacion"]))
                            {
                                var serie = new Series { name = productGroup.Key.ToString().Trim(), data = new List<Data>() };
                                foreach (var anioData in productGroup)
                                {
                                    DateTime dateValue = new DateTime(Convert.ToInt32(anioData["anio"]), 1, 1);
                                    var data = new Data { name = Convert.ToString(ToUnixTimestamp(dateValue)), y = Convert.ToDouble(anioData["total_animales"]) };
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
                                    po.descripcion, 
                                    SUM(pi.totalmachos) AS total_machos_depto,  
                                    SUM(pi.totalhembras) AS total_hembras_depto
                                FROM 
                                    agromapas.pecuario.inventariobovinogrupo pi, 
                                    agromapas.base.departamento b, 
                                    agromapas.pecuario.orientacionbovino po
                                WHERE 
                                    b.codigo = pi.codigodepto AND
                                    po.codigo = pi.codigoedadtipobovino
                                    /*PARAMETROS*/
                                    AND pi.anho >= " + parameters.anio_inicial + @" AND pi.anho <= " + parameters.anio_final + @"
                                    AND pi.codigodepto in (" + string.Join(",", parameters.departamento.Select(d => "'" + d + "'")) + @")
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
                                          agromapas.pecuario.produccionpecuaria pp, 
                                          agromapas.pecuario.producto p, 
                                          agromapas.base.departamento b
                                        WHERE 
                                          p.codigo = pp.producto AND
                                          b.codigo = pp.departamento AND 
                                          /*PARAMETROS*/    
                                          pp.departamento IN (" + string.Join(",", parameters.departamento.Select(d => "'" + d + "'")) + @") ORDER BY pp.periodo";


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
                                          agromapas.pecuario.produccionpecuaria pp, 
                                          agromapas.pecuario.producto p, 
                                          agromapas.base.departamento b
                                        WHERE 
                                          p.codigo = pp.producto AND
                                          b.codigo = pp.departamento AND 
                                          /*PARAMETROS*/    
                                          pp.producto = " + parameters.tipo_pecuario + @"
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
                                              agromapas.pecuario.produccionpecuaria pp, 
                                              agromapas.pecuario.producto p, 
                                              agromapas.base.departamento b
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
                    string sqlString = @"SELECT 
                              pp.producto,  
                              p.nombre, 
                              pp.periodo as periodo, 
                              pp.departamento as departamentocod, 
                              b.nombre as departamento_nombre,
                              pp.produccion as produccion_pecuaria, 
                              pp.unidad 

                            FROM 
                              agromapas.pecuario.produccionpecuaria pp, 
                              agromapas.pecuario.producto p, 
                              agromapas.base.departamento b
                            WHERE 
                              p.codigo = pp.producto AND
                              b.codigo = pp.departamento
                              /*PARAMETROS*/    
                              AND pp.producto = " + parameters.tipo_pecuario + @" AND pp.periodo >= " + parameters.anio_inicial + @" 
                              AND pp.periodo <= " + parameters.anio_final + @"
                              AND pp.departamento IN (" + string.Join(",", parameters.departamento.Select(d => "'" + d + "'")) + ") ";

                    switch (parameters.id)
                    {
                        case 1:
                            Chart chart1 = new Chart { subtitle = "", series = new List<Series>() };

                            var query1 = from r in adapter.GetDataTable(sqlString).AsEnumerable()
                                         group r by r["departamento_nombre"] into deptoGroup
                                         from anioGroup in
                                             (from d in deptoGroup
                                              group d by d["periodo"])
                                         group anioGroup by deptoGroup.Key;

                            foreach (var deptoGroup in query1)
                            {
                                var serie = new Series { name = deptoGroup.Key.ToString(), data = new List<Data>() };
                                foreach (var anioGroup in deptoGroup)
                                {
                                    double y = anioGroup.Sum(d => Convert.ToDouble(d["produccion_pecuaria"]));
                                    DateTime dateValue = new DateTime(Convert.ToInt32(anioGroup.Key.ToString()), 1, 1);
                                    var data = new Data { name = Convert.ToString(ToUnixTimestamp(dateValue)), y = y };
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
                                              agromapas.pecuario.produccionpecuaria pp, 
                                              agromapas.pecuario.producto p, 
                                              agromapas.base.departamento b
                                            WHERE 
                                              p.codigo = pp.producto AND
                                              b.codigo = pp.departamento AND 
                                              /*PARAMETROS*/    
                                              pp.departamento IN (" + string.Join(",", parameters.departamento.Select(d => "'" + d + "'")) + @") 
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
    
        [Route("api/Report/108")]
        public IHttpActionResult postReport108(report108 parameters)
        {
            Object returnData = null;
            PostgresqlAdapter adapter = new PostgresqlAdapter();
            switch (parameters.tipo)
            {
                case "parametro":
                    Parameter parameter = new Parameter { data = new List<ParameterData>() };
                    switch (parameters.id)
                    {
                        case 1:
                            parameter.name = "departamento";
                            foreach (var p in (from p in adapter.GetDataTable(@"SELECT DISTINCT
                                                                                v_dep.codigo departamentocod, 
                                                                                v_dep.nombre departamento
                                                                                FROM agromapas.base.departamento v_dep
                                                                                ORDER BY v_dep.nombre ASC").AsEnumerable()
                                               select p))
                            {
                                ParameterData param = new ParameterData { value = Convert.ToString(p["departamentocod"]), name = Convert.ToString(p["departamento"]).Trim() };
                                parameter.data.Add(param);
                            }
                            returnData = (Parameter)parameter;
                            break;
                        case 2:
                            parameter.name = "municipio";
                            foreach (var p in (from p in adapter.GetDataTable(String.Format(@"SELECT right('0'::text || v_mun.departamento, 2) || right('00'::text || v_mun.codigo, 3) municipiocod, v_mun.nombre municipio 
                                                                                            FROM agromapas.base.municipio v_mun
                                                                                            WHERE v_mun.departamento = {0}
                                                                                            ORDER BY v_mun.nombre ASC;", parameters.departamento)).AsEnumerable()
                                               select p))
                            {
                                ParameterData param = new ParameterData { value = Convert.ToString(p["municipiocod"]), name = Convert.ToString(p["municipio"]).Trim() };
                                parameter.data.Add(param);
                            }
                            returnData = (Parameter)parameter;
                            break;
                        case 3:
                            parameter.name = "anio";
                            foreach (var p in (from p in adapter.GetDataTable(@"SELECT DISTINCT v_evamun.anho_eva as anio
                                                                                FROM agromapas.eva_mpal.v_evamunicipal v_evamun
                                                                                ORDER BY v_evamun.anho_eva;").AsEnumerable()
                                               select p["anio"]))
                            {
                                ParameterData param = new ParameterData { name = Convert.ToString(p), value = Convert.ToString(p) };
                                parameter.data.Add(param);
                            }
                            returnData = (Parameter)parameter;
                            break;
                    }
                    break;
                case "grafico":
                    DataTable results = adapter.GetDataTable(String.Format(@"SELECT
                                                                            v_evamun.anho_eva as anio,
                                                                            v_prod.nombrecomun as producto,
                                                                            SUM(v_evamun.areasembrada_eva) as area_sembrada,
                                                                            SUM(v_evamun.areacosechada_eva) as area_cosechada,
                                                                            SUM(v_evamun.produccion_eva) as produccion,
                                                                            SUM(v_evamun.rendimiento_eva) as rendimiento
                                                                            FROM agromapas.eva_mpal.v_evamunicipal v_evamun
                                                                            INNER JOIN agromapas.eva_mpal.v_productodetalle v_prod ON v_prod.codigoagronetproducto = v_evamun.codigoagronetproducto_eva
                                                                            WHERE v_evamun.anho_eva >= {0}
                                                                            AND v_evamun.anho_eva <= {1}
                                                                            AND v_evamun.codigomunicipio_eva = '{2}'
                                                                            GROUP BY v_prod.nombrecomun, v_evamun.anho_eva
                                                                            ORDER BY v_prod.nombrecomun, v_evamun.anho_eva;", parameters.anio_inicial, parameters.anio_final, parameters.municipio));

                    Chart chart = new Chart { series = new List<Series>() };
                    switch (parameters.id)
                    {
                        case 1:
                            chart.subtitle = "Área sembrada de " + parameters.anio_inicial + " a " + parameters.anio_final;
                            foreach (var productGroup in (from r in results.AsEnumerable()
                                                         group r by r["producto"]))
                            {
                                var serie = new Series { name = productGroup.Key.ToString().Trim(), data = new List<Data>() };
                                foreach (var anioData in productGroup)
                                {
                                    DateTime dateValue = new DateTime(Convert.ToInt32(anioData["anio"]), 1, 1);
                                    var data = new Data { name = Convert.ToString(ToUnixTimestamp(dateValue)), y = Convert.ToDouble(anioData["area_sembrada"]) };
                                    serie.data.Add(data);

                                }
                                chart.series.Add(serie);
                            }

                            returnData = (Chart)chart;
                            break;
                        case 2:
                            chart.subtitle = "Área cosechada de " + parameters.anio_inicial + " a " + parameters.anio_final;
                            foreach (var productGroup in (from r in results.AsEnumerable()
                                                         group r by r["producto"]))
                            {
                                var serie = new Series { name = productGroup.Key.ToString().Trim(), data = new List<Data>() };
                                foreach (var anioData in productGroup)
                                {
                                    DateTime dateValue = new DateTime(Convert.ToInt32(anioData["anio"]), 1, 1);
                                    var data = new Data { name = Convert.ToString(ToUnixTimestamp(dateValue)), y = Convert.ToDouble(anioData["area_cosechada"]) };
                                    serie.data.Add(data);

                                }
                                chart.series.Add(serie);
                            }

                            returnData = (Chart)chart;
                            break;
                        case 3:
                            chart.subtitle = "Producción de " + parameters.anio_inicial + " a " + parameters.anio_final;
                            foreach (var productGroup in (from r in results.AsEnumerable()
                                                         group r by r["producto"]))
                            {
                                var serie = new Series { name = productGroup.Key.ToString().Trim(), data = new List<Data>() };
                                foreach (var anioData in productGroup)
                                {
                                    DateTime dateValue = new DateTime(Convert.ToInt32(anioData["anio"]), 1, 1);
                                    var data = new Data { name = Convert.ToString(ToUnixTimestamp(dateValue)), y = Convert.ToDouble(anioData["produccion"]) };
                                    serie.data.Add(data);

                                }
                                chart.series.Add(serie);
                            }

                            returnData = (Chart)chart;
                            break;
                        case 4:
                            chart.subtitle = "Rendimiento de " + parameters.anio_inicial + " a " + parameters.anio_final;
                            foreach (var productGroup in (from r in results.AsEnumerable()
                                                         group r by r["producto"]))
                            {
                                var serie = new Series { name = productGroup.Key.ToString().Trim(), data = new List<Data>() };
                                foreach (var anioData in productGroup)
                                {
                                    DateTime dateValue = new DateTime(Convert.ToInt32(anioData["anio"]), 1, 1);
                                    var data = new Data { name = Convert.ToString(ToUnixTimestamp(dateValue)), y = Convert.ToDouble(anioData["rendimiento"]) };
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
                            Table table = new Table
                            {
                                rows = adapter.GetDataTable(String.Format(@"SELECT
                                                                            v_evamun.anho_eva as anio,
                                                                            v_prod.nombrecomun as producto,
                                                                            SUM(v_evamun.areasembrada_eva) as area_sembrada,
                                                                            SUM(v_evamun.areacosechada_eva) as area_cosechada,
                                                                            SUM(v_evamun.produccion_eva) as produccion,
                                                                            SUM(v_evamun.rendimiento_eva) as rendimiento
                                                                            FROM agromapas.eva_mpal.v_evamunicipal v_evamun
                                                                            INNER JOIN agromapas.eva_mpal.v_productodetalle v_prod ON v_prod.codigoagronetproducto = v_evamun.codigoagronetproducto_eva
                                                                            WHERE v_evamun.anho_eva >= {0} 
                                                                            AND v_evamun.anho_eva <= {1}
                                                                            AND v_evamun.codigomunicipio_eva = '{2}'
                                                                            GROUP BY v_evamun.anho_eva, v_prod.nombrecomun
                                                                            ORDER BY v_evamun.anho_eva, v_prod.nombrecomun;", parameters.anio_inicial, parameters.anio_final, parameters.municipio))
                            };
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
