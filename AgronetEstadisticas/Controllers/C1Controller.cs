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
        public async Task<IHttpActionResult> postReport101(report101 parameters)
        {
            Object returnData = null;

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
                                WHERE eva_mpal.evadepartamentalanual.anho_eva >= " + parameters.anio_inicial + @" AND eva_mpal.evadepartamentalanual.anho_eva <= " + parameters.anio_final + @" AND eva_mpal.productos.nombredescriptorcultivo = '" + parameters.producto + @"' AND base.departamento.nombre = '" + parameters.departamento + "'";

            
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
               
                DataTable result = adapter.GetDataTable(sqlString);
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
               switch (parameters.id)
               {
                   case 1:
                       Table table = new Table { rows = adapter.GetDataTable(sqlString) };
                       returnData = (Table)table;
                       break;
               }
            }

            if (returnData == null) {
                return NotFound();
            }

            return Ok(returnData);
        }
    }
}
