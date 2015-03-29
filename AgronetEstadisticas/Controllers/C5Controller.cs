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
    public class C5Controller : ApiController
    {
        [Route("api/Report/501")]
        public IHttpActionResult postReport501(report501 parameters)
        {
            Object returnData = null;
            SQLAdapter adapter = new SQLAdapter();
            switch (parameters.tipo)
            {
                case "parametro":
                    switch (parameters.id)
                    {
                        case 1:
                            DataTable data = adapter.GetDatatable(@"SELECT DISTINCT [AgronetSIPSA].dbo.Sipsa_ProductosSemanales.nombreProducto_ProductosSemanales as codigo, [AgronetSIPSA].dbo.Sipsa_ProductosSemanales.nombreProducto_ProductosSemanales  as descripcion
FROM [AgronetSIPSA].dbo.Sipsa_ProductosSemanales INNER JOIN 
[AgronetSIPSA].dbo.SipsaSemanal ON [AgronetSIPSA].dbo.Sipsa_ProductosSemanales.codigoProducto_ProductosSemanales =[AgronetSIPSA]. dbo.SipsaSemanal.codProducto_SipsaSemanal 
ORDER BY [AgronetSIPSA].dbo.Sipsa_ProductosSemanales.nombreProducto_ProductosSemanales;");
                            Parameter param = new Parameter { name = "productos", data = new List<ParameterData>() };
                            foreach (var d in (from p in data.AsEnumerable() select p))
                            {
                                ParameterData parameter = new ParameterData { name = Convert.ToString(d["descripcion"]), value = Convert.ToString(d["codigo"]) };
                                param.data.Add(parameter);
                            }

                            returnData = (Parameter)param;
                            break;
                        case 2:
                            DataTable data2 = adapter.GetDatatable(String.Format(@"SELECT DISTINCT [AgronetSIPSA].dbo.Sipsa_MercadosSem.nombreMercado_MercadosSem AS descripcion, [AgronetSIPSA].dbo.Sipsa_MercadosSem.codMercado_MercadosSem AS codigo 
FROM [AgronetSIPSA].dbo.Sipsa_MercadosSem INNER JOIN [AgronetSIPSA].dbo.SipsaSemanal ON Sipsa_MercadosSem.codMercado_MercadosSem = SipsaSemanal.codMercado_SipsaSemanal 
INNER JOIN [AgronetSIPSA].dbo.Sipsa_ProductosSemanales ON [AgronetSIPSA].dbo.SipsaSemanal.codProducto_SipsaSemanal = [AgronetSIPSA].dbo.Sipsa_ProductosSemanales.codigoProducto_ProductosSemanales 
WHERE ([AgronetSIPSA].dbo.Sipsa_ProductosSemanales.nombreProducto_ProductosSemanales = '{0}') ORDER BY descripcion;", parameters.producto));
                            Parameter param2 = new Parameter { name = "mercados", data = new List<ParameterData>() };
                            foreach (var d in (from p in data2.AsEnumerable() select p))
                            {
                                ParameterData parameter = new ParameterData { name = Convert.ToString(d["descripcion"]), value = Convert.ToString(d["codigo"]) };
                                param2.data.Add(parameter);
                            }

                            returnData = (Parameter)param2;
                            break;
                        case 3:
                            DataTable data3 = adapter.GetDatatable(string.Format(@"SELECT DISTINCT YEAR(fecha_SipsaSemanal) as fecha FROM [AgronetSIPSA].dbo.SipsaSemanal
WHERE [AgronetSIPSA].dbo.SipsaSemanal.codMercado_SipsaSemanal = {0}
ORDER BY YEAR(fecha_SipsaSemanal);", parameters.mercado));
                            Parameter param3 = new Parameter { name = "fecha", data = new List<ParameterData>() };
                            foreach (var d in (from p in data3.AsEnumerable() select p))
                            {
                                ParameterData parameter = new ParameterData { name = Convert.ToString(d["fecha"]), value = Convert.ToString(d["fecha"]) };
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
                            DataTable results = adapter.GetDatatable(String.Format(@"SELECT DATEDIFF(ss, '01/01/1970', SipsaMensual.fecha_SipsaSemanal) as fechaunix,
SipsaMensual.fecha_SipsaSemanal as fecha, 
SipsaMensual.precioPromedioSemanal_SipsaSemanal as preciopromediosemanal, 
Sipsa_Productos.nombreProducto_ProductosSemanales, 
Sipsa_Mercados.nombreMercado_MercadosSem, 
Sipsa_UnidadesSemMen.nombreUnidad_SipsaUnidadesSemMen
FROM   ((AgronetSIPSA.dbo.SipsaSemanal SipsaMensual 
INNER JOIN AgronetSIPSA.dbo.Sipsa_MercadosSem Sipsa_Mercados 
ON SipsaMensual.codMercado_SipsaSemanal=Sipsa_Mercados.codMercado_MercadosSem) 
INNER JOIN AgronetSIPSA.dbo.Sipsa_ProductosSemanales Sipsa_Productos
ON SipsaMensual.codProducto_SipsaSemanal=Sipsa_Productos.codigoProducto_ProductosSemanales) 
INNER JOIN AgronetSIPSA.dbo.Sipsa_UnidadesSemMen Sipsa_UnidadesSemMen 
ON SipsaMensual.codUnidad_SipsaSemanal=Sipsa_UnidadesSemMen.codUnidad_SipsaUnidadesSemMen
WHERE Sipsa_Productos.nombreProducto_ProductosSemanales = '{0}'
AND Sipsa_Mercados.codMercado_MercadosSem = {1}
AND  SipsaMensual.fecha_SipsaSemanal BETWEEN '{2}' AND '{3}'
ORDER BY SipsaMensual.fecha_SipsaSemanal", parameters.producto, parameters.mercado, parameters.fecha_inicial, parameters.fecha_final));
                            Chart chart1 = new Chart { subtitle = "Tendencia y Ciclo‎", series = new List<Series>() };

                            Series serie1 = new Series { name = "Precio", data = new List<Data>() };
                            Series serie2 = new Series { name = "PMC", data = new List<Data>() };
                            
                            Dictionary<int, Double> pm = new Dictionary<int, Double>();
                            var periodo = 1;
                            foreach (var d1 in (from d in results.AsEnumerable()
                                                select d))
                            {
                                Double precio = Convert.ToDouble(d1["preciopromediosemanal"]);
                                Data data1 = new Data { name = Convert.ToString(d1["fechaunix"]), y = precio };
                                Data data2 = new Data { name = Convert.ToString(d1["fechaunix"]) };

                                pm.Add(periodo, precio);
                                if (parameters.periodo_corto >= periodo)
                                {
                                    data2.y = pm.Select(x => x.Value).Sum() / periodo;
                                }
                                else
                                {
                                    data2.y = pm.Where(x => x.Key > periodo - parameters.periodo_corto && x.Key <= periodo).Select(x => x.Value).Sum() / parameters.periodo_corto;
                                }
                                periodo++;
                                data1.y = Math.Round(data1.y, 2);
                                data2.y = Math.Round(data2.y, 2);
                                
                                serie1.data.Add(data1);
                                serie2.data.Add(data2);
                            }
                            chart1.series.Add(serie1);
                            chart1.series.Add(serie2);
                        
                            returnData = (Chart)chart1;
                            break;
                        case 2:
                            DataTable results2 = adapter.GetDatatable(String.Format(@"SELECT DATEDIFF(ss, '01/01/1970', SipsaMensual.fecha_SipsaSemanal) as fechaunix,
SipsaMensual.fecha_SipsaSemanal as fecha, 
SipsaMensual.precioPromedioSemanal_SipsaSemanal as preciopromediosemanal, 
Sipsa_Productos.nombreProducto_ProductosSemanales, 
Sipsa_Mercados.nombreMercado_MercadosSem, 
Sipsa_UnidadesSemMen.nombreUnidad_SipsaUnidadesSemMen
FROM   ((AgronetSIPSA.dbo.SipsaSemanal SipsaMensual 
INNER JOIN AgronetSIPSA.dbo.Sipsa_MercadosSem Sipsa_Mercados 
ON SipsaMensual.codMercado_SipsaSemanal=Sipsa_Mercados.codMercado_MercadosSem) 
INNER JOIN AgronetSIPSA.dbo.Sipsa_ProductosSemanales Sipsa_Productos
ON SipsaMensual.codProducto_SipsaSemanal=Sipsa_Productos.codigoProducto_ProductosSemanales) 
INNER JOIN AgronetSIPSA.dbo.Sipsa_UnidadesSemMen Sipsa_UnidadesSemMen 
ON SipsaMensual.codUnidad_SipsaSemanal=Sipsa_UnidadesSemMen.codUnidad_SipsaUnidadesSemMen
WHERE Sipsa_Productos.nombreProducto_ProductosSemanales = '{0}'
AND Sipsa_Mercados.codMercado_MercadosSem = {1}
AND  SipsaMensual.fecha_SipsaSemanal BETWEEN '{2}' AND '{3}'
ORDER BY SipsaMensual.fecha_SipsaSemanal", parameters.producto, parameters.mercado, parameters.fecha_inicial, parameters.fecha_final));
                            Chart chart2 = new Chart { subtitle = "Comparativo promedio móvil corto plazo y largo plazo‎", series = new List<Series>() };

                            Series serie21 = new Series { name = "Precio", data = new List<Data>() };
                            Series serie22 = new Series { name = "PMC", data = new List<Data>() };
                            Series serie23 = new Series { name = "PML", data = new List<Data>() };

                            Dictionary<int, Double> pm2 = new Dictionary<int, Double>();
                            var periodo2 = 1;
                            foreach (var d1 in (from d in results2.AsEnumerable()
                                                select d))
                            {
                                Double precio = Convert.ToDouble(d1["preciopromediosemanal"]);
                                Data data1 = new Data { name = Convert.ToString(d1["fechaunix"]), y = precio };
                                Data data2 = new Data { name = Convert.ToString(d1["fechaunix"]) };
                                Data data3 = new Data { name = Convert.ToString(d1["fechaunix"]) };

                                pm2.Add(periodo2, precio);
                                if (parameters.periodo_corto >= periodo2)
                                {
                                    data2.y = pm2.Select(x => x.Value).Sum() / periodo2;
                                }
                                else
                                {
                                    data2.y = pm2.Where(x => x.Key > periodo2 - parameters.periodo_corto && x.Key <= periodo2).Select(x => x.Value).Sum() / parameters.periodo_corto;
                                }                               

                                if (parameters.periodo_largo >= periodo2)
                                {
                                    data3.y = pm2.Select(x => x.Value).Sum() / periodo2;
                                }
                                else
                                {
                                    data3.y = pm2.Where(x => x.Key > periodo2 - parameters.periodo_largo && x.Key <= periodo2).Select(x => x.Value).Sum() / parameters.periodo_largo;
                                }
                                periodo2++;
                                data1.y = Math.Round(data1.y, 2);
                                data2.y = Math.Round(data2.y, 2);
                                data3.y = Math.Round(data3.y, 2);
                                
                                serie21.data.Add(data1);
                                serie22.data.Add(data2);
                                serie23.data.Add(data3);
                            }
                            chart2.series.Add(serie21);
                            chart2.series.Add(serie22);
                            chart2.series.Add(serie23);
                        
                            returnData = (Chart)chart2;
                            break;
                        case 3:
                             DataTable results3 = adapter.GetDatatable(String.Format(@"SELECT DATEDIFF(ss, '01/01/1970', SipsaMensual.fecha_SipsaSemanal) as fechaunix,
SipsaMensual.fecha_SipsaSemanal as fecha, 
SipsaMensual.precioPromedioSemanal_SipsaSemanal as preciopromediosemanal, 
Sipsa_Productos.nombreProducto_ProductosSemanales, 
Sipsa_Mercados.nombreMercado_MercadosSem, 
Sipsa_UnidadesSemMen.nombreUnidad_SipsaUnidadesSemMen
FROM   ((AgronetSIPSA.dbo.SipsaSemanal SipsaMensual 
INNER JOIN AgronetSIPSA.dbo.Sipsa_MercadosSem Sipsa_Mercados 
ON SipsaMensual.codMercado_SipsaSemanal=Sipsa_Mercados.codMercado_MercadosSem) 
INNER JOIN AgronetSIPSA.dbo.Sipsa_ProductosSemanales Sipsa_Productos
ON SipsaMensual.codProducto_SipsaSemanal=Sipsa_Productos.codigoProducto_ProductosSemanales) 
INNER JOIN AgronetSIPSA.dbo.Sipsa_UnidadesSemMen Sipsa_UnidadesSemMen 
ON SipsaMensual.codUnidad_SipsaSemanal=Sipsa_UnidadesSemMen.codUnidad_SipsaUnidadesSemMen
WHERE Sipsa_Productos.nombreProducto_ProductosSemanales = '{0}'
AND Sipsa_Mercados.codMercado_MercadosSem = {1}
AND  SipsaMensual.fecha_SipsaSemanal BETWEEN '{2}' AND '{3}'
ORDER BY SipsaMensual.fecha_SipsaSemanal", parameters.producto, parameters.mercado, parameters.fecha_inicial, parameters.fecha_final));
                            Chart chart3 = new Chart { subtitle = "Oscilador de Precios‎‎", series = new List<Series>() };

                            Series serie31 = new Series { name = "OSC", data = new List<Data>() };
                            Dictionary<int, Double> pm3 = new Dictionary<int, Double>();
                            var periodo3 = 1;
                            foreach (var d1 in (from d in results3.AsEnumerable()
                                                select d))
                            {
                                Double precio = Convert.ToDouble(d1["preciopromediosemanal"]);
                                Data data1 = new Data { name = Convert.ToString(d1["fechaunix"]) };

                                pm3.Add(periodo3, precio);
                                Double pmc = 0;
                                if (parameters.periodo_corto >= periodo3)
                                {
                                    pmc = pm3.Select(x => x.Value).Sum() / periodo3;
                                }
                                else
                                {
                                    pmc = pm3.Where(x => x.Key > periodo3 - parameters.periodo_corto && x.Key <= periodo3).Select(x => x.Value).Sum() / parameters.periodo_corto;
                                }
                                Double pml = 0;
                                if (parameters.periodo_largo >= periodo3)
                                {
                                    pml = pm3.Select(x => x.Value).Sum() / periodo3;
                                }
                                else
                                {
                                    pml = pm3.Where(x => x.Key > periodo3 - parameters.periodo_largo && x.Key <= periodo3).Select(x => x.Value).Sum() / parameters.periodo_largo;
                                }
                                periodo3++;

                                data1.y = Math.Round(pmc - pml, 2);                                
                                serie31.data.Add(data1);
                            }
                            chart3.series.Add(serie31);
                        
                            returnData = (Chart)chart3;
                            break;
                        case 4:
                             DataTable results4 = adapter.GetDatatable(String.Format(@"SELECT DATEDIFF(ss, '01/01/1970', SipsaMensual.fecha_SipsaSemanal) as fechaunix,
SipsaMensual.fecha_SipsaSemanal as fecha, 
SipsaMensual.precioPromedioSemanal_SipsaSemanal as preciopromediosemanal, 
Sipsa_Productos.nombreProducto_ProductosSemanales, 
Sipsa_Mercados.nombreMercado_MercadosSem, 
Sipsa_UnidadesSemMen.nombreUnidad_SipsaUnidadesSemMen
FROM   ((AgronetSIPSA.dbo.SipsaSemanal SipsaMensual 
INNER JOIN AgronetSIPSA.dbo.Sipsa_MercadosSem Sipsa_Mercados 
ON SipsaMensual.codMercado_SipsaSemanal=Sipsa_Mercados.codMercado_MercadosSem) 
INNER JOIN AgronetSIPSA.dbo.Sipsa_ProductosSemanales Sipsa_Productos
ON SipsaMensual.codProducto_SipsaSemanal=Sipsa_Productos.codigoProducto_ProductosSemanales) 
INNER JOIN AgronetSIPSA.dbo.Sipsa_UnidadesSemMen Sipsa_UnidadesSemMen 
ON SipsaMensual.codUnidad_SipsaSemanal=Sipsa_UnidadesSemMen.codUnidad_SipsaUnidadesSemMen
WHERE Sipsa_Productos.nombreProducto_ProductosSemanales = '{0}'
AND Sipsa_Mercados.codMercado_MercadosSem = {1}
AND  SipsaMensual.fecha_SipsaSemanal BETWEEN '{2}' AND '{3}'
ORDER BY SipsaMensual.fecha_SipsaSemanal", parameters.producto, parameters.mercado, parameters.fecha_inicial, parameters.fecha_final));
                            Chart chart4 = new Chart { subtitle = "Índice de Fuerza Relativa‎", series = new List<Series>() };

                            Series serie41 = new Series { name = "IFR", data = new List<Data>() };

                            Dictionary<int, Double> cp = new Dictionary<int, Double>();
                            Dictionary<int, Double> cn = new Dictionary<int, Double>();
                            Double precioant = 0;
                            var periodo4 = 1;
                            foreach (var d1 in (from d in results4.AsEnumerable()
                                                select d))
                            {
                                Double cambio =  Convert.ToDouble(d1["preciopromediosemanal"]) - precioant;
                                if (cambio > 0)
                                {
                                    cp.Add(periodo4, cambio);
                                }
                                else
                                {
                                    cn.Add(periodo4, cambio);
                                }
                                precioant = Convert.ToDouble(d1["preciopromediosemanal"]);

                                Double pcp = 0;
                                if (parameters.numero_periodos_ifr >= periodo4)
                                {
                                    pcp = cp.Select(x => x.Value).Sum() / cp.Count();
                                }
                                else
                                {
                                    pcp = cp.Where(x => x.Key > periodo4 - parameters.numero_periodos_ifr && x.Key <= periodo4).Select(x => x.Value).Sum() / cp.Where(x => x.Key > periodo4 - parameters.numero_periodos_ifr && x.Key <= periodo4).Count();
                                }

                                Double pcn = 0;
                                if (parameters.numero_periodos_ifr >= periodo4)
                                {
                                    pcn = cn.Select(x => x.Value).Sum() / cn.Count();
                                }
                                else
                                {
                                    pcn = cn.Where(x => x.Key > periodo4 - parameters.numero_periodos_ifr && x.Key <= periodo4).Select(x => x.Value).Sum() / cn.Where(x => x.Key > periodo4 - parameters.numero_periodos_ifr && x.Key <= periodo4).Count();
                                }
                                periodo4++;

                                Data data1 = new Data { name = Convert.ToString(d1["fechaunix"]), y = 100 - (100/(1 - (pcp / pcn))) };
                                serie41.data.Add(data1);
                            }
                            chart4.series.Add(serie41);                        
                            returnData = (Chart)chart4;
                            break;
                    }
                    break;
                case "tabla":
                    switch (parameters.id)
                    {
                        case 1:
                            DataTable tableResults =  adapter.GetDatatable(String.Format(@"SELECT DATEDIFF(ss, '01/01/1970', SipsaMensual.fecha_SipsaSemanal) as fechaunix,
SipsaMensual.fecha_SipsaSemanal as fecha, 
SipsaMensual.precioPromedioSemanal_SipsaSemanal as preciopromediosemanal, 
Sipsa_Productos.nombreProducto_ProductosSemanales, 
Sipsa_Mercados.nombreMercado_MercadosSem, 
Sipsa_UnidadesSemMen.nombreUnidad_SipsaUnidadesSemMen
FROM   ((AgronetSIPSA.dbo.SipsaSemanal SipsaMensual 
INNER JOIN AgronetSIPSA.dbo.Sipsa_MercadosSem Sipsa_Mercados 
ON SipsaMensual.codMercado_SipsaSemanal=Sipsa_Mercados.codMercado_MercadosSem) 
INNER JOIN AgronetSIPSA.dbo.Sipsa_ProductosSemanales Sipsa_Productos
ON SipsaMensual.codProducto_SipsaSemanal=Sipsa_Productos.codigoProducto_ProductosSemanales) 
INNER JOIN AgronetSIPSA.dbo.Sipsa_UnidadesSemMen Sipsa_UnidadesSemMen 
ON SipsaMensual.codUnidad_SipsaSemanal=Sipsa_UnidadesSemMen.codUnidad_SipsaUnidadesSemMen
WHERE Sipsa_Productos.nombreProducto_ProductosSemanales = '{0}'
AND Sipsa_Mercados.codMercado_MercadosSem = {1}
AND  SipsaMensual.fecha_SipsaSemanal BETWEEN '{2}' AND '{3}'
ORDER BY SipsaMensual.fecha_SipsaSemanal", parameters.producto, parameters.mercado, parameters.fecha_inicial, parameters.fecha_final));
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

        [Route("api/Report/502")]
        public IHttpActionResult postReport502(report502 parameters)
        {
            Object returnData = null;
            SQLAdapter adapter = new SQLAdapter();
            switch (parameters.tipo)
            {
                case "parametro":
                    switch (parameters.id)
                    {
                        case 1:
                            DataTable data = adapter.GetDatatable(@"SELECT DISTINCT 
Sipsa_ProductosSemanales.nombreProducto_ProductosSemanales AS descripcion, 
Sipsa_ProductosSemanales.codigoProducto_ProductosSemanales AS codigo 
FROM [AgronetSIPSA].dbo.SipsaSemanal INNER JOIN [AgronetSIPSA].dbo.Sipsa_ProductosSemanales ON SipsaSemanal.codProducto_SipsaSemanal = Sipsa_ProductosSemanales.codigoProducto_ProductosSemanales 
ORDER BY descripcion;");
                            Parameter param = new Parameter { name = "productos", data = new List<ParameterData>() };
                            foreach (var d in (from p in data.AsEnumerable() select p))
                            {
                                ParameterData parameter = new ParameterData { name = Convert.ToString(d["descripcion"]), value = Convert.ToString(d["codigo"]) };
                                param.data.Add(parameter);
                            }

                            returnData = (Parameter)param;
                            break;
                        case 2:
                            DataTable data2 = adapter.GetDatatable(String.Format(@"SELECT DISTINCT [AgronetSIPSA].dbo.Sipsa_MercadosSem.nombreMercado_MercadosSem AS descripcion, [AgronetSIPSA].dbo.Sipsa_MercadosSem.codMercado_MercadosSem AS codigo 
FROM [AgronetSIPSA].dbo.Sipsa_MercadosSem INNER JOIN [AgronetSIPSA].dbo.SipsaSemanal ON Sipsa_MercadosSem.codMercado_MercadosSem = SipsaSemanal.codMercado_SipsaSemanal 
INNER JOIN [AgronetSIPSA].dbo.Sipsa_ProductosSemanales ON [AgronetSIPSA].dbo.SipsaSemanal.codProducto_SipsaSemanal = [AgronetSIPSA].dbo.Sipsa_ProductosSemanales.codigoProducto_ProductosSemanales 
WHERE ([AgronetSIPSA].dbo.Sipsa_ProductosSemanales.codigoProducto_ProductosSemanales = {0}) ORDER BY descripcion;", parameters.producto));
                            Parameter param2 = new Parameter { name = "mercados", data = new List<ParameterData>() };
                            foreach (var d in (from p in data2.AsEnumerable() select p))
                            {
                                ParameterData parameter = new ParameterData { name = Convert.ToString(d["descripcion"]), value = Convert.ToString(d["codigo"]) };
                                param2.data.Add(parameter);
                            }

                            returnData = (Parameter)param2;
                            break;
                        case 3:
                            DataTable data3 = adapter.GetDatatable(@"SELECT DISTINCT YEAR(fecha_SipsaSemanal) as fecha FROM [AgronetSIPSA].dbo.SipsaSemanal ORDER BY YEAR(fecha_SipsaSemanal);");
                            Parameter param3 = new Parameter { name = "fecha", data = new List<ParameterData>() };
                            foreach (var d in (from p in data3.AsEnumerable() select p))
                            {
                                ParameterData parameter = new ParameterData { name = Convert.ToString(d["fecha"]), value = Convert.ToString(d["fecha"]) };
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
                            DataTable results1 = adapter.GetDatatable(String.Format(@"CREATE TABLE #SP_COMPARACION_MERCADOS1(producto text,mercado text,fecha_sipsa date,mercado2 float,unidad text,uno int)
CREATE TABLE #SP_COMPARACION_MERCADOS2(productob text,mercadob text,fecha_sipsab date,mercado2b float,unidadb text,unob int)
INSERT INTO #SP_COMPARACION_MERCADOS1
EXEC	[AgronetSIPSA].[dbo].[SP_ComparacionMercados]
		@PRODUCTO = {0},
		@MERCADO1 = {1},
		@MERCADO2 = {2}
INSERT INTO #SP_COMPARACION_MERCADOS2
EXEC	[AgronetSIPSA].[dbo].[SP_COMPARACIONMERCADOS_2]
		@PRODUCTO = {3},
		@MERCADO1 = {4},
		@MERCADO2 = {5}
SELECT DATEDIFF(ss, '01/01/1970', #SP_COMPARACION_MERCADOS1.fecha_sipsa) as fechaunix,* FROM #SP_COMPARACION_MERCADOS1
INNER JOIN #SP_COMPARACION_MERCADOS2 ON #SP_COMPARACION_MERCADOS1.fecha_sipsa = #SP_COMPARACION_MERCADOS2.fecha_sipsab
WHERE #SP_COMPARACION_MERCADOS1.fecha_sipsa BETWEEN '{6}' AND '{7}'
ORDER BY #SP_COMPARACION_MERCADOS1.fecha_sipsa
DROP TABLE #SP_COMPARACION_MERCADOS1
DROP TABLE #SP_COMPARACION_MERCADOS2;", parameters.producto, parameters.mercado1, parameters.mercado2, parameters.producto, parameters.mercado1, parameters.mercado2, parameters.fecha_inicial, parameters.fecha_final));
                            Chart chart1 = new Chart { series = new List<Series>() };

                            Series serie1 = new Series { data = new List<Data>() };
                            Series serie2 = new Series { data = new List<Data>() };

                            foreach (var d1 in (from d in results1.AsEnumerable()
                                                select d))
                            {
                                serie1.name = Convert.ToString(d1["mercado"]);
                                serie2.name = Convert.ToString(d1["mercadob"]);
                                serie1.data.Add(new Data { name = Convert.ToString(d1["fechaunix"]), y = Convert.ToDouble(d1["mercado2"]) });
                                serie2.data.Add(new Data { name = Convert.ToString(d1["fechaunix"]), y = Convert.ToDouble(d1["mercado2b"]) });
                            }
                            chart1.series.Add(serie1);
                            chart1.series.Add(serie2);

                            returnData = (Chart)chart1;
                            break;
                    }
                    break;
                case "tabla":
                    DataTable results2 = adapter.GetDatatable(String.Format(@"CREATE TABLE #SP_COMPARACION_MERCADOS1(producto text,mercado text,fecha_sipsa date,mercado2 float,unidad text,uno int)
CREATE TABLE #SP_COMPARACION_MERCADOS2(productob text,mercadob text,fecha_sipsab date,mercado2b float,unidadb text,unob int)
INSERT INTO #SP_COMPARACION_MERCADOS1
EXEC	[AgronetSIPSA].[dbo].[SP_ComparacionMercados]
		@PRODUCTO = {0},
		@MERCADO1 = {1},
		@MERCADO2 = {2}
INSERT INTO #SP_COMPARACION_MERCADOS2
EXEC	[AgronetSIPSA].[dbo].[SP_COMPARACIONMERCADOS_2]
		@PRODUCTO = {3},
		@MERCADO1 = {4},
		@MERCADO2 = {5}
SELECT DATEDIFF(ss, '01/01/1970', #SP_COMPARACION_MERCADOS1.fecha_sipsa) as fechaunix,* FROM #SP_COMPARACION_MERCADOS1
INNER JOIN #SP_COMPARACION_MERCADOS2 ON #SP_COMPARACION_MERCADOS1.fecha_sipsa = #SP_COMPARACION_MERCADOS2.fecha_sipsab
WHERE #SP_COMPARACION_MERCADOS1.fecha_sipsa BETWEEN '{6}' AND '{7}'
ORDER BY #SP_COMPARACION_MERCADOS1.fecha_sipsa
DROP TABLE #SP_COMPARACION_MERCADOS1
DROP TABLE #SP_COMPARACION_MERCADOS2;", parameters.producto, parameters.mercado1, parameters.mercado2, parameters.producto, parameters.mercado1, parameters.mercado2, parameters.fecha_inicial, parameters.fecha_final));
                            
                    switch (parameters.id)
                    {
                        case 1:
                            Table table1 = new Table { rows = results2 };
                            returnData = (Table)table1;
                            break;
                        case 2:
                            
                            Double proPrecioMercadoA = results2.AsEnumerable().Average(x => x.Field<Double>("mercado2"));
                            Double proPrecioMercadoB = results2.AsEnumerable().Average(x => x.Field<Double>("mercado2B"));
                            Double maxPrecioMercadoA = results2.AsEnumerable().Max(x => x.Field<Double>("mercado2"));
                            Double maxPrecioMercadoB = results2.AsEnumerable().Max(x => x.Field<Double>("mercado2B"));
                            Double minPrecioMercadoA = results2.AsEnumerable().Min(x => x.Field<Double>("mercado2"));
                            Double minPrecioMercadoB = results2.AsEnumerable().Min(x => x.Field<Double>("mercado2B"));
                            Double dsvPrecioMercadoA = StdDev(results2.AsEnumerable().Select(x => x.Field<Double>("mercado2")));
                            Double dsvPrecioMercadoB = StdDev(results2.AsEnumerable().Select(x => x.Field<Double>("mercado2B")));
                            Double cvPrecioMercadoA = (dsvPrecioMercadoA / proPrecioMercadoA) * 100;
                            Double cvPrecioMercadoB = (dsvPrecioMercadoB / proPrecioMercadoB) * 100;
                            Double covMercadoAMercadoB = Covariance(results2.AsEnumerable().Select(x => x.Field<Double>("mercado2")), results2.AsEnumerable().Select(x => x.Field<Double>("mercado2B")));
                            Double volPrecioMercadoA = VolatilidadHistorica(results2.AsEnumerable().Select(x => x.Field<Double>("mercado2")));
                            Double volPrecioMercadoB = VolatilidadHistorica(results2.AsEnumerable().Select(x => x.Field<Double>("mercado2B")));

                            Table table2 = new Table { rows = new DataTable() };

                            table2.rows.Columns.Add("descripcion", typeof(string));
                            table2.rows.Columns.Add("mercadoA", typeof(double));
                            table2.rows.Columns.Add("mercadoB", typeof(double));
                            table2.rows.Columns.Add("relacion_precio", typeof(double));
                            table2.rows.Columns.Add("diferencia", typeof(double));

                            table2.rows.Rows.Add("Promedio histórico‎", 
                                Math.Round(proPrecioMercadoA,2), 
                                Math.Round(proPrecioMercadoB,2), 
                                Math.Round(proPrecioMercadoA / proPrecioMercadoB,2),
                                Math.Round(proPrecioMercadoA - proPrecioMercadoB,2));

                            table2.rows.Rows.Add("Máximo histórico‎",
                                Math.Round(maxPrecioMercadoA, 2),
                                Math.Round(maxPrecioMercadoB, 2),
                                Math.Round(maxPrecioMercadoA / maxPrecioMercadoB, 2),
                                Math.Round(maxPrecioMercadoA - maxPrecioMercadoB, 2));

                            table2.rows.Rows.Add("Mínimo histórico‎",
                                Math.Round(minPrecioMercadoA, 2),
                                Math.Round(minPrecioMercadoB, 2),
                                Math.Round(minPrecioMercadoA / minPrecioMercadoB, 2),
                                Math.Round(minPrecioMercadoA - minPrecioMercadoB, 2));

                            table2.rows.Rows.Add("Desviación estándar‎",
                                Math.Round(dsvPrecioMercadoA, 2),
                                Math.Round(dsvPrecioMercadoB, 2),
                                Math.Round(dsvPrecioMercadoA / dsvPrecioMercadoB, 2),
                                Math.Round(dsvPrecioMercadoA - dsvPrecioMercadoB, 2));

                            table2.rows.Rows.Add("Coeficiente de variación‎",
                               Math.Round(cvPrecioMercadoA, 2),
                               Math.Round(cvPrecioMercadoB, 2),
                               Math.Round(cvPrecioMercadoA / cvPrecioMercadoB, 2),
                               Math.Round(cvPrecioMercadoA - cvPrecioMercadoB, 2));

                            table2.rows.Rows.Add("Volatilidad histórica semanal‎‎",
                               Math.Round(volPrecioMercadoA, 2),
                               Math.Round(volPrecioMercadoB, 2),
                               Math.Round(volPrecioMercadoA / volPrecioMercadoB, 2), null);

                            table2.rows.Rows.Add("Coeficiente de correlación‎‎",
                               Math.Round(covMercadoAMercadoB / (dsvPrecioMercadoA * dsvPrecioMercadoB), 2), null, null, null);


                            returnData = (Table)table2;
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

        [Route("api/Report/503")]
        public IHttpActionResult postReport503(report503 parameters)
        {
            Object returnData = null;
            SQLAdapter adapter = new SQLAdapter();
            switch (parameters.tipo)
            {
                case "parametro":
                    switch (parameters.id)
                    {
                        case 1:
                            DataTable data = adapter.GetDatatable(@"SELECT DISTINCT 
Sipsa_ProductosSemanales.nombreProducto_ProductosSemanales AS descripcion, 
Sipsa_ProductosSemanales.codigoProducto_ProductosSemanales AS codigo 
FROM [AgronetSIPSA].dbo.SipsaSemanal INNER JOIN [AgronetSIPSA].dbo.Sipsa_ProductosSemanales ON SipsaSemanal.codProducto_SipsaSemanal = Sipsa_ProductosSemanales.codigoProducto_ProductosSemanales 
ORDER BY descripcion;");
                            Parameter param = new Parameter { name = "productos", data = new List<ParameterData>() };
                            foreach (var d in (from p in data.AsEnumerable() select p))
                            {
                                ParameterData parameter = new ParameterData { name = Convert.ToString(d["descripcion"]), value = Convert.ToString(d["codigo"]) };
                                param.data.Add(parameter);
                            }

                            returnData = (Parameter)param;
                            break;
                        case 2:
                            DataTable data2 = adapter.GetDatatable(String.Format(@"SELECT DISTINCT [AgronetSIPSA].dbo.Sipsa_MercadosSem.nombreMercado_MercadosSem AS descripcion, [AgronetSIPSA].dbo.Sipsa_MercadosSem.codMercado_MercadosSem AS codigo 
FROM [AgronetSIPSA].dbo.Sipsa_MercadosSem INNER JOIN [AgronetSIPSA].dbo.SipsaSemanal ON Sipsa_MercadosSem.codMercado_MercadosSem = SipsaSemanal.codMercado_SipsaSemanal 
INNER JOIN [AgronetSIPSA].dbo.Sipsa_ProductosSemanales ON [AgronetSIPSA].dbo.SipsaSemanal.codProducto_SipsaSemanal = [AgronetSIPSA].dbo.Sipsa_ProductosSemanales.codigoProducto_ProductosSemanales 
WHERE ([AgronetSIPSA].dbo.Sipsa_ProductosSemanales.codigoProducto_ProductosSemanales = {0}) ORDER BY descripcion;", parameters.producto));
                            Parameter param2 = new Parameter { name = "mercados", data = new List<ParameterData>() };
                            foreach (var d in (from p in data2.AsEnumerable() select p))
                            {
                                ParameterData parameter = new ParameterData { name = Convert.ToString(d["descripcion"]), value = Convert.ToString(d["codigo"]) };
                                param2.data.Add(parameter);
                            }

                            returnData = (Parameter)param2;
                            break;
                        case 3:
                            DataTable data3 = adapter.GetDatatable(@"SELECT DISTINCT YEAR(fecha_SipsaSemanal) as fecha FROM [AgronetSIPSA].dbo.SipsaSemanal ORDER BY YEAR(fecha_SipsaSemanal);");
                            Parameter param3 = new Parameter { name = "fecha", data = new List<ParameterData>() };
                            foreach (var d in (from p in data3.AsEnumerable() select p))
                            {
                                ParameterData parameter = new ParameterData { name = Convert.ToString(d["fecha"]), value = Convert.ToString(d["fecha"]) };
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
                            Chart chart1 = new Chart { subtitle = "Precios Mensuales", series = new List<Series>() };

                            foreach (var d1 in (from d in (adapter.GetDatatable(String.Format(@"SELECT DATEDIFF(ss, '01/01/1970', SipsaMensual.fecha_SipsaSemanal) as fechaunix, SipsaMensual.fecha_SipsaSemanal, SipsaMensual.precioPromedioSemanal_SipsaSemanal, Sipsa_Productos.nombreProducto_ProductosSemanales, Sipsa_Mercados.nombreMercado_MercadosSem, Sipsa_UnidadesSemMen.nombreUnidad_SipsaUnidadesSemMen
 FROM   ((AgronetSIPSA.dbo.SipsaSemanal SipsaMensual 
 INNER JOIN AgronetSIPSA.dbo.Sipsa_MercadosSem Sipsa_Mercados 
 ON SipsaMensual.codMercado_SipsaSemanal=Sipsa_Mercados.codMercado_MercadosSem) 
 INNER JOIN AgronetSIPSA.dbo.Sipsa_ProductosSemanales Sipsa_Productos 
 ON SipsaMensual.codProducto_SipsaSemanal=Sipsa_Productos.codigoProducto_ProductosSemanales) 
 INNER JOIN AgronetSIPSA.dbo.Sipsa_UnidadesSemMen Sipsa_UnidadesSemMen 
 ON SipsaMensual.codUnidad_SipsaSemanal=Sipsa_UnidadesSemMen.codUnidad_SipsaUnidadesSemMen
 WHERE SipsaMensual.fecha_SipsaSemanal between '{0}' and '{1}'
 and Sipsa_Productos.codigoProducto_ProductosSemanales = {2}
 and Sipsa_Mercados.codMercado_MercadosSem in (" + string.Join(",",parameters.mercado.Select(d => d)) + @")
 ORDER BY Sipsa_Mercados.nombreMercado_MercadosSem, SipsaMensual.fecha_SipsaSemanal;", parameters.fecha_inicial, parameters.fecha_final, parameters.producto))).AsEnumerable()
                                           group d by d["nombreMercado_MercadosSem"] into mercadoG
                                                select mercadoG))
                            {
                                var serie = new Series { name = d1.Key.ToString(), data = new List<Data>() };

                                foreach (var seriesData in d1)
                                {   
                                    var data = new Data { name = Convert.ToString(seriesData["fechaunix"]), y = Convert.ToDouble(seriesData["precioPromedioSemanal_SipsaSemanal"]) };
                                    serie.data.Add(data);
                                }
                                chart1.series.Add(serie);
                            }
                            
                            returnData = (Chart)chart1;
                            break;
                        case 2:
                            DataTable results1 = adapter.GetDatatable(String.Format(@"SELECT DATEDIFF(ss, '01/01/1970', SipsaMensual.fecha_SipsaSemanal) as fechaunix, SipsaMensual.fecha_SipsaSemanal, SipsaMensual.precioPromedioSemanal_SipsaSemanal preciopromediosemanal, Sipsa_Productos.nombreProducto_ProductosSemanales, Sipsa_Mercados.nombreMercado_MercadosSem, Sipsa_UnidadesSemMen.nombreUnidad_SipsaUnidadesSemMen
 FROM   ((AgronetSIPSA.dbo.SipsaSemanal SipsaMensual 
 INNER JOIN AgronetSIPSA.dbo.Sipsa_MercadosSem Sipsa_Mercados 
 ON SipsaMensual.codMercado_SipsaSemanal=Sipsa_Mercados.codMercado_MercadosSem) 
 INNER JOIN AgronetSIPSA.dbo.Sipsa_ProductosSemanales Sipsa_Productos 
 ON SipsaMensual.codProducto_SipsaSemanal=Sipsa_Productos.codigoProducto_ProductosSemanales) 
 INNER JOIN AgronetSIPSA.dbo.Sipsa_UnidadesSemMen Sipsa_UnidadesSemMen 
 ON SipsaMensual.codUnidad_SipsaSemanal=Sipsa_UnidadesSemMen.codUnidad_SipsaUnidadesSemMen
 WHERE SipsaMensual.fecha_SipsaSemanal between '{0}' and '{1}'
 and Sipsa_Productos.codigoProducto_ProductosSemanales = {2}
 and Sipsa_Mercados.codMercado_MercadosSem in (" + string.Join(",",parameters.mercado.Select(d => d)) + @")
 ORDER BY Sipsa_Mercados.nombreMercado_MercadosSem, SipsaMensual.fecha_SipsaSemanal;", parameters.fecha_inicial, parameters.fecha_final, parameters.producto));

                            Chart chart2 = new Chart { subtitle = "Promedio móvil semanal (52 periodos)", series = new List<Series>() };
                            Series serie1 = new Series { name = "Índice", data = new List<Data>() };

                            Dictionary<int, Double> pm = new Dictionary<int, Double>();
                            List<IndiceEstacional> ie = new List<IndiceEstacional>();
                            var periodo = 1;
                            foreach (var d1 in (from d in results1.AsEnumerable()
                                                select d))
                            {
                                Double precio = Convert.ToDouble(d1["preciopromediosemanal"]);
                                DateTime fechaSemanal = Convert.ToDateTime(d1["fecha_SipsaSemanal"]);
                                Double promedioMovil = 0;

                                pm.Add(periodo, precio);
                                if (12 >= periodo)
                                {
                                    promedioMovil = pm.Select(x => x.Value).Sum() / periodo;
                                }
                                else
                                {
                                    promedioMovil = pm.Where(x => x.Key > periodo - 12 && x.Key <= periodo).Select(x => x.Value).Sum() / 12;
                                }
                                periodo++;
                                ie.Add(new IndiceEstacional { fechaSemanal = fechaSemanal, precioSobrePromedioMovil = precio / promedioMovil });
                                Double ieResult = ie.Where(x => x.fechaSemanal.Month == fechaSemanal.Month).Select(x => x.precioSobrePromedioMovil).Average();

                                Data data1 = new Data { name = Convert.ToString(d1["fechaunix"]), y = ieResult };
                                serie1.data.Add(data1);
                            }
                            chart2.series.Add(serie1);
                            returnData = (Chart)chart2;
                            break;
                    }
                    break;
                case "tabla":
                    switch (parameters.id)
                    {
                        case 1:
                            DataTable results1 = adapter.GetDatatable(String.Format(@"SELECT SipsaMensual.fecha_SipsaSemanal, Sipsa_Mercados.nombreMercado_MercadosSem, Sipsa_Productos.nombreProducto_ProductosSemanales, SipsaMensual.precioPromedioSemanal_SipsaSemanal preciopromediosemanal, Sipsa_UnidadesSemMen.nombreUnidad_SipsaUnidadesSemMen
 FROM   ((AgronetSIPSA.dbo.SipsaSemanal SipsaMensual 
 INNER JOIN AgronetSIPSA.dbo.Sipsa_MercadosSem Sipsa_Mercados 
 ON SipsaMensual.codMercado_SipsaSemanal=Sipsa_Mercados.codMercado_MercadosSem) 
 INNER JOIN AgronetSIPSA.dbo.Sipsa_ProductosSemanales Sipsa_Productos 
 ON SipsaMensual.codProducto_SipsaSemanal=Sipsa_Productos.codigoProducto_ProductosSemanales) 
 INNER JOIN AgronetSIPSA.dbo.Sipsa_UnidadesSemMen Sipsa_UnidadesSemMen 
 ON SipsaMensual.codUnidad_SipsaSemanal=Sipsa_UnidadesSemMen.codUnidad_SipsaUnidadesSemMen
 WHERE SipsaMensual.fecha_SipsaSemanal between '{0}' and '{1}'
 and Sipsa_Productos.codigoProducto_ProductosSemanales = {2}
 and Sipsa_Mercados.codMercado_MercadosSem in (" + string.Join(",", parameters.mercado.Select(d => d)) + @")
 ORDER BY Sipsa_Mercados.nombreMercado_MercadosSem, SipsaMensual.fecha_SipsaSemanal;", parameters.fecha_inicial, parameters.fecha_final, parameters.producto));

                            Table table = new Table { rows = results1 };
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

        [Route("api/Report/504")]
        public IHttpActionResult postReport504(report504 parameters)
        {
            Object returnData = null;
            SQLAdapter adapter = new SQLAdapter();
            switch (parameters.tipo)
            {
                case "parametro":
                    switch (parameters.id)
                    {
                        case 1:
                            DataTable data1 = adapter.GetDatatable(@"SELECT distinct codigoProducto_ProductosMensuales  as codigo,
                                                                        nombreProducto_ProductosMensuales as descripcion  
                                                                        FROM AgronetPesca.dbo.Pesca_PreciosProductosMensuales;");
                            Parameter param1 = new Parameter { name = "producto", data = new List<ParameterData>() };
                            foreach (var d in (from p in data1.AsEnumerable() select p))
                            {
                                ParameterData parameter = new ParameterData { name = Convert.ToString(d["descripcion"]), value = Convert.ToString(d["codigo"]) };
                                param1.data.Add(parameter);
                            }

                            returnData = (Parameter)param1;
                            break;
                        case 2:
                            DataTable data2 = adapter.GetDatatable(string.Format(@"SELECT     
                                                                    AgronetPesca.dbo.Pesca_PreciosPescadoMensual.codMercado_PreciosMensual as codigo, 
                                                                    AgronetPesca.dbo.Pesca_PreciosMercadosMen.nombreMercado_MercadosMen as descripcion
                                                                    FROM  AgronetPesca.dbo.Pesca_PreciosPescadoMensual 
                                                                    INNER JOIN AgronetPesca.dbo.Pesca_PreciosMercadosMen ON 
                                                                    AgronetPesca.dbo.Pesca_PreciosPescadoMensual.codMercado_PreciosMensual = AgronetPesca.dbo.Pesca_PreciosMercadosMen.codMercado_MercadosMen 
                                                                    WHERE AgronetPesca.dbo.Pesca_PreciosPescadoMensual.codProducto_PreciosMensual = {0}
                                                                    GROUP BY AgronetPesca.dbo.Pesca_PreciosPescadoMensual.codMercado_PreciosMensual, 
                                                                    AgronetPesca.dbo.Pesca_PreciosMercadosMen.nombreMercado_MercadosMen, 
                                                                    AgronetPesca.dbo.Pesca_PreciosPescadoMensual.codProducto_PreciosMensual", parameters.producto));
                            Parameter param2 = new Parameter { name = "mercados", data = new List<ParameterData>() };
                            foreach (var d in (from p in data2.AsEnumerable() select p))
                            {
                                ParameterData parameter = new ParameterData { name = Convert.ToString(d["descripcion"]), value = Convert.ToString(d["codigo"]) };
                                param2.data.Add(parameter);
                            }

                            returnData = (Parameter)param2;
                            break;
                        case 3:
                            DataTable data3 = adapter.GetDatatable(@"SELECT DISTINCT YEAR(Fecha_PreciosMensual) as fecha
                                                                        FROM AgronetPesca.dbo.Pesca_PreciosPescadoMensual 
                                                                        ORDER BY YEAR(Fecha_PreciosMensual);");
                            Parameter param3 = new Parameter { name = "fecha", data = new List<ParameterData>() };
                            foreach (var d in (from p in data3.AsEnumerable() select p))
                            {
                                ParameterData parameter = new ParameterData { name = Convert.ToString(d["fecha"]), value = Convert.ToString(d["fecha"]) };
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
                            Chart chart1 = new Chart { subtitle = "", series = new List<Series>() };

                            foreach (var d1 in (from d in (adapter.GetDatatable(String.Format(
                                                @"SELECT DATEDIFF(ss, '01/01/1970', Fecha_PreciosMensual) as fechaunix, Fecha_PreciosMensual, nombreMercado_MercadosMen, nombreProducto_ProductosMensuales, precio_PreciosMensual
                                                FROM  AgronetPesca.dbo.Pesca_PreciosPescadoMensual 
                                                INNER JOIN AgronetPesca.dbo.Pesca_PreciosMercadosMen ON AgronetPesca.dbo.Pesca_PreciosPescadoMensual.codMercado_PreciosMensual = AgronetPesca.dbo.Pesca_PreciosMercadosMen.codMercado_MercadosMen
                                                INNER JOIN AgronetPesca.dbo.Pesca_PreciosProductosMensuales ON AgronetPesca.dbo.Pesca_PreciosPescadoMensual.codProducto_PreciosMensual = AgronetPesca.dbo.Pesca_PreciosProductosMensuales.codigoProducto_ProductosMensuales
                                                WHERE AgronetPesca.dbo.Pesca_PreciosProductosMensuales.codigoProducto_ProductosMensuales = {0}
                                                AND AgronetPesca.dbo.Pesca_PreciosMercadosMen.codMercado_MercadosMen IN (" + string.Join(",",parameters.mercado.Select(d => d)) + @")
                                                AND AgronetPesca.dbo.Pesca_PreciosPescadoMensual.Fecha_PreciosMensual BETWEEN '{1}' AND '{2}'
                                                ORDER BY AgronetPesca.dbo.Pesca_PreciosPescadoMensual.Fecha_PreciosMensual", parameters.producto, parameters.fecha_inicial, parameters.fecha_final))).AsEnumerable()
                                                group d by d["nombreMercado_MercadosMen"] into mercadoG
                                                select mercadoG))
                            {
                                var serie = new Series { name = d1.Key.ToString(), data = new List<Data>() };

                                foreach (var seriesData in d1)
                                {
                                    var data = new Data { name = Convert.ToString(seriesData["fechaunix"]), y = Convert.ToDouble(seriesData["precio_PreciosMensual"]) };
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
                            DataTable results1 = adapter.GetDatatable(String.Format(
                                                @"SELECT Fecha_PreciosMensual, nombreMercado_MercadosMen, nombreProducto_ProductosMensuales, precio_PreciosMensual
                                                FROM  AgronetPesca.dbo.Pesca_PreciosPescadoMensual 
                                                INNER JOIN AgronetPesca.dbo.Pesca_PreciosMercadosMen ON AgronetPesca.dbo.Pesca_PreciosPescadoMensual.codMercado_PreciosMensual = AgronetPesca.dbo.Pesca_PreciosMercadosMen.codMercado_MercadosMen
                                                INNER JOIN AgronetPesca.dbo.Pesca_PreciosProductosMensuales ON AgronetPesca.dbo.Pesca_PreciosPescadoMensual.codProducto_PreciosMensual = AgronetPesca.dbo.Pesca_PreciosProductosMensuales.codigoProducto_ProductosMensuales
                                                WHERE AgronetPesca.dbo.Pesca_PreciosProductosMensuales.codigoProducto_ProductosMensuales = {0}
                                                AND AgronetPesca.dbo.Pesca_PreciosMercadosMen.codMercado_MercadosMen IN (" + string.Join(",", parameters.mercado.Select(d => d)) + @")
                                                AND AgronetPesca.dbo.Pesca_PreciosPescadoMensual.Fecha_PreciosMensual BETWEEN '{1}' AND '{2}'
                                                ORDER BY AgronetPesca.dbo.Pesca_PreciosPescadoMensual.Fecha_PreciosMensual", parameters.producto, parameters.fecha_inicial, parameters.fecha_final));

                            Table table = new Table { rows = results1 };
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

        [Route("api/Report/505")]
        public IHttpActionResult postReport505(report505 parameters)
        {
            Object returnData = null;
            SQLAdapter adapter = new SQLAdapter();
            switch (parameters.tipo)
            {
                case "parametro":
                    switch (parameters.id)
                    {
                        case 1:
                            String sql1 = @"SELECT AgronetSIPSA.dbo.SipsaDiario.codigoProducto_SipsaDiario as codigo, AgronetSIPSA.dbo.Sipsa_ProductosDiarios.nombreProducto_ProductosDiarios as descripcion
                                            FROM   AgronetSIPSA.dbo.Sipsa_ProductosDiarios 
                                            INNER JOIN AgronetSIPSA.dbo.SipsaDiario ON AgronetSIPSA.dbo.Sipsa_ProductosDiarios.codigoProducto_ProductosDiarios = AgronetSIPSA.dbo.SipsaDiario.codigoProducto_SipsaDiario
                                            GROUP BY AgronetSIPSA.dbo.Sipsa_ProductosDiarios.nombreProducto_ProductosDiarios, AgronetSIPSA.dbo.SipsaDiario.codigoProducto_SipsaDiario
                                            ORDER BY AgronetSIPSA.dbo.Sipsa_ProductosDiarios.nombreProducto_ProductosDiarios;";
                            DataTable data = adapter.GetDatatable(sql1);
                            Parameter param = new Parameter { name = "productos", data = new List<ParameterData>() };
                            foreach (var d in (from p in data.AsEnumerable() select p))
                            {
                                ParameterData parameter = new ParameterData { name = Convert.ToString(d["descripcion"]), value = Convert.ToString(d["codigo"]) };
                                param.data.Add(parameter);
                            }

                            returnData = (Parameter)param;
                            break;
                    }
                    break;
                case "grafico":
                    switch (parameters.id)
                    {
                        case 1:
                            String sql1 = String.Format(@"SELECT Sipsa_ProductosDiarios.nombreProducto_ProductosDiarios, 
                                             VW_PrecioPequeñosProductores.fecha, 
                                             VW_PrecioPequeñosProductores.nombreMercado_MercadosDia, 
                                             VW_PrecioPequeñosProductores.PrecioSemanaAnt, 
                                             VW_PrecioPequeñosProductores.PrecioSemanaAct, 
                                             VW_PrecioPequeñosProductores.Variacion, 
                                             VW_PrecioPequeñosProductores.codigoProducto_SipsaDiario, 
                                             VW_PrecioPequeñosProductores.unidad_SipsaDiario, 
                                             Sipsa_MercadosDiarios.nombreMercado_MercadosDia
                                             FROM   (AgronetSIPSA.dbo.VW_PrecioPequeñosProductores VW_PrecioPequeñosProductores 
                                             INNER JOIN AgronetSIPSA.dbo.Sipsa_MercadosDiarios Sipsa_MercadosDiarios ON VW_PrecioPequeñosProductores.codMercado_MercadosDia=Sipsa_MercadosDiarios.codMercado_MercadosDia) 
                                             INNER JOIN AgronetSIPSA.dbo.Sipsa_ProductosDiarios Sipsa_ProductosDiarios ON VW_PrecioPequeñosProductores.nombreProducto_ProductosDiarios=Sipsa_ProductosDiarios.nombreProducto_ProductosDiarios
                                             WHERE Sipsa_ProductosDiarios.codigoProducto_ProductosDiarios = {0}
                                             ORDER BY VW_PrecioPequeñosProductores.nombreMercado_MercadosDia", parameters.producto);

                            DataTable results = adapter.GetDatatable(sql1);
                            Chart chart1 = new Chart { subtitle = "", series = new List<Series>() };

                            Series serie1 = new Series { name = "Precios Diarios", data = new List<Data>() };
                            chart1.series.Add(serie1);
                        
                            foreach (var d1 in (from d in results.AsEnumerable()
                                                select d))
                            {
                                Data data1 = new Data { name = Convert.ToString(d1["nombreMercado_MercadosDia"]), y = Convert.ToDouble(d1["PrecioSemanaAct"]) };
                                serie1.data.Add(data1);
                            }

                            returnData = (Chart)chart1;
                            break;
                    }
                    break;
                case "tabla":
                    switch (parameters.id)
                    {
                        case 1:
                             String sql1 = String.Format(@"SELECT Sipsa_ProductosDiarios.nombreProducto_ProductosDiarios, 
                                             VW_PrecioPequeñosProductores.fecha, 
                                             VW_PrecioPequeñosProductores.nombreMercado_MercadosDia, 
                                             VW_PrecioPequeñosProductores.PrecioSemanaAnt, 
                                             VW_PrecioPequeñosProductores.PrecioSemanaAct, 
                                             VW_PrecioPequeñosProductores.Variacion, 
                                             VW_PrecioPequeñosProductores.codigoProducto_SipsaDiario, 
                                             VW_PrecioPequeñosProductores.unidad_SipsaDiario, 
                                             Sipsa_MercadosDiarios.nombreMercado_MercadosDia
                                             FROM   (AgronetSIPSA.dbo.VW_PrecioPequeñosProductores VW_PrecioPequeñosProductores 
                                             INNER JOIN AgronetSIPSA.dbo.Sipsa_MercadosDiarios Sipsa_MercadosDiarios ON VW_PrecioPequeñosProductores.codMercado_MercadosDia=Sipsa_MercadosDiarios.codMercado_MercadosDia) 
                                             INNER JOIN AgronetSIPSA.dbo.Sipsa_ProductosDiarios Sipsa_ProductosDiarios ON VW_PrecioPequeñosProductores.nombreProducto_ProductosDiarios=Sipsa_ProductosDiarios.nombreProducto_ProductosDiarios
                                             WHERE Sipsa_ProductosDiarios.codigoProducto_ProductosDiarios = {0}
                                             ORDER BY VW_PrecioPequeñosProductores.nombreMercado_MercadosDia", parameters.producto);

                            DataTable tableResults = adapter.GetDatatable(sql1);
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

        [Route("api/Report/506")]
        public IHttpActionResult postReport506(report506 parameters)
        {
            Object returnData = null;
            SQLAdapter adapter = new SQLAdapter();
            switch (parameters.tipo)
            {
                case "parametro":
                    switch (parameters.id)
                    {
                        case 1:
                            String sql1 = @"SELECT DISTINCT YEAR(fecha_AbastecimientoMensual) AS anios FROM AgronetSIPSA.dbo.AbastecimientoMensual ORDER BY YEAR(fecha_AbastecimientoMensual);";
                            DataTable data = adapter.GetDatatable(sql1);
                            Parameter param = new Parameter { name = "anios", data = new List<ParameterData>() };
                            foreach (var d in (from p in data.AsEnumerable() select p))
                            {
                                ParameterData parameter = new ParameterData { name = Convert.ToString(d["anios"]), value = Convert.ToString(d["anios"]) };
                                param.data.Add(parameter);
                            }

                            returnData = (Parameter)param;
                            break;
                        case 2:
                            String sql2 = @"SELECT [codigoGrupo_Grupos],[grupo_Grupos] FROM [AgronetSIPSA].[dbo].[Abastecimiento_Grupos] ORDER BY 2;";
                            DataTable data2 = adapter.GetDatatable(sql2);
                            Parameter param2 = new Parameter { name = "grupos", data = new List<ParameterData>() };
                            foreach (var d in (from p in data2.AsEnumerable() select p))
                            {
                                ParameterData parameter = new ParameterData { name = Convert.ToString(d["grupo_Grupos"]), value = Convert.ToString(d["codigoGrupo_Grupos"]) };
                                param2.data.Add(parameter);
                            }

                            returnData = (Parameter)param2;
                            break;
                        case 3:
                            String sql3 = @"SELECT [codigoMunicipio],[nombreMunicipio] FROM [AgronetSIPSA].[dbo].[Abastecimiento_Ciudades] ORDER BY 2;";
                            DataTable data3 = adapter.GetDatatable(sql3);
                            Parameter param3 = new Parameter { name = "ciudades", data = new List<ParameterData>() };
                            foreach (var d in (from p in data3.AsEnumerable() select p))
                            {
                                ParameterData parameter = new ParameterData { name = Convert.ToString(d["nombreMunicipio"]), value = Convert.ToString(d["codigoMunicipio"]) };
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
                            String sql1 = String.Format(@"create table  #SP_ABASTECIMIENTO_PORPRODUCTO(
                                                fecha date,
                                                ciudad nvarchar(5),
                                                sitio nvarchar(7),
                                                grupo int,
                                                toneladas float,
                                                variacionToneladas float
                                            )
                                            insert into #SP_ABASTECIMIENTO_PORPRODUCTO EXEC [AgronetSipsa].[dbo].[SP_ABASTECIMIENTO_PORPRODUCTO]
		                                            @Fecha_inicial = N'{0}-01-01',
		                                            @Fecha_final = N'{1}-12-31',
		                                            @Grupo = {2}

                                            SELECT 
                                            Abastecimiento_Sitios.sitioSipsa_sitioAbastecimiento, 
                                            Abastecimiento_Sitios.ciudad_sitioAbastecimiento, 
                                            Abastecimiento_Ciudades.nombreMunicipio, 
                                            Abastecimiento_Sitios.sitio_sitioAbastecimiento, 
                                            Abastecimiento_Grupos.grupo_Grupos, 
                                            Abastecimiento_Grupos.codigoGrupo_Grupos, 
                                            Abastecimiento_Sitios.codigo_sitioAbastecimiento,
                                            DATEDIFF(ss, '01/01/1970', #SP_ABASTECIMIENTO_PORPRODUCTO.fecha) as fechaunix,
                                            #SP_ABASTECIMIENTO_PORPRODUCTO.fecha,
                                            #SP_ABASTECIMIENTO_PORPRODUCTO.ciudad,
                                            #SP_ABASTECIMIENTO_PORPRODUCTO.sitio,
                                            #SP_ABASTECIMIENTO_PORPRODUCTO.grupo,
                                            #SP_ABASTECIMIENTO_PORPRODUCTO.toneladas,
                                            #SP_ABASTECIMIENTO_PORPRODUCTO.variacionToneladas
                                             FROM   
                                             (AgronetSIPSA.dbo.Abastecimiento_Grupos Abastecimiento_Grupos 
                                             CROSS JOIN AgronetSIPSA.dbo.Abastecimiento_Sitios Abastecimiento_Sitios
                                             ) 
                                             INNER JOIN AgronetSIPSA.dbo.Abastecimiento_Ciudades Abastecimiento_Ciudades 
                                             ON Abastecimiento_Sitios.ciudad_sitioAbastecimiento=Abastecimiento_Ciudades.codigoMunicipio
                                             INNER JOIN #SP_ABASTECIMIENTO_PORPRODUCTO ON Abastecimiento_Sitios.codigo_sitioAbastecimiento = #SP_ABASTECIMIENTO_PORPRODUCTO.sitio
                                             WHERE  
                                             --PARAMETROS
                                             Abastecimiento_Grupos.codigoGrupo_Grupos= {3} 
                                             AND Abastecimiento_Sitios.ciudad_sitioAbastecimiento IN (" + string.Join(",",parameters.ciudad.Select(d => "'"+d+"'")) +
                                             @") ORDER BY #SP_ABASTECIMIENTO_PORPRODUCTO.fecha, 
                                             Abastecimiento_Sitios.sitioSipsa_sitioAbastecimiento

                                            DROP TABLE #SP_ABASTECIMIENTO_PORPRODUCTO",parameters.fecha_inicial, parameters.fecha_final, parameters.grupo, parameters.grupo);
                            DataTable results = adapter.GetDatatable(sql1);

                            Chart chart1 = new Chart { subtitle = "", series = new List<Series>() };

                            var query1 = from r in results.AsEnumerable()
                                         group r by r["nombreMunicipio"];

                            foreach (var deptosGroup in query1)
                            {
                                var serie1 = new Series { name = deptosGroup.Key.ToString(), data = new List<Data>() };
                                foreach (var el1 in deptosGroup)
                                {
                                    var data = new Data { name = Convert.ToString(el1["fechaunix"]), y = Convert.ToDouble(el1["toneladas"]) };
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
                            String sql1 = String.Format(@"create table  #SP_ABASTECIMIENTO_PORPRODUCTO(
                                                fecha date,
                                                ciudad nvarchar(5),
                                                sitio nvarchar(7),
                                                grupo int,
                                                toneladas float,
                                                variacionToneladas float
                                            )
                                            insert into #SP_ABASTECIMIENTO_PORPRODUCTO EXEC [AgronetSipsa].[dbo].[SP_ABASTECIMIENTO_PORPRODUCTO]
		                                            @Fecha_inicial = N'{0}-01-01',
		                                            @Fecha_final = N'{1}-12-31',
		                                            @Grupo = {2}

                                            SELECT 
                                            Abastecimiento_Sitios.sitioSipsa_sitioAbastecimiento, 
                                            Abastecimiento_Sitios.ciudad_sitioAbastecimiento, 
                                            Abastecimiento_Ciudades.nombreMunicipio, 
                                            Abastecimiento_Sitios.sitio_sitioAbastecimiento, 
                                            Abastecimiento_Grupos.grupo_Grupos, 
                                            Abastecimiento_Grupos.codigoGrupo_Grupos, 
                                            Abastecimiento_Sitios.codigo_sitioAbastecimiento,
                                            #SP_ABASTECIMIENTO_PORPRODUCTO.fecha,
                                            #SP_ABASTECIMIENTO_PORPRODUCTO.ciudad,
                                            #SP_ABASTECIMIENTO_PORPRODUCTO.sitio,
                                            #SP_ABASTECIMIENTO_PORPRODUCTO.grupo,
                                            #SP_ABASTECIMIENTO_PORPRODUCTO.toneladas,
                                            #SP_ABASTECIMIENTO_PORPRODUCTO.variacionToneladas
                                             FROM   
                                             (AgronetSIPSA.dbo.Abastecimiento_Grupos Abastecimiento_Grupos 
                                             CROSS JOIN AgronetSIPSA.dbo.Abastecimiento_Sitios Abastecimiento_Sitios
                                             ) 
                                             INNER JOIN AgronetSIPSA.dbo.Abastecimiento_Ciudades Abastecimiento_Ciudades 
                                             ON Abastecimiento_Sitios.ciudad_sitioAbastecimiento=Abastecimiento_Ciudades.codigoMunicipio
                                             INNER JOIN #SP_ABASTECIMIENTO_PORPRODUCTO ON Abastecimiento_Sitios.codigo_sitioAbastecimiento = #SP_ABASTECIMIENTO_PORPRODUCTO.sitio
                                             WHERE  
                                             --PARAMETROS
                                             Abastecimiento_Grupos.codigoGrupo_Grupos= {3} 
                                             AND Abastecimiento_Sitios.ciudad_sitioAbastecimiento IN (" + string.Join(",", parameters.ciudad.Select(d => "'" + d + "'")) +
                                             @") ORDER BY Abastecimiento_Ciudades.nombreMunicipio, 
                                             Abastecimiento_Sitios.sitioSipsa_sitioAbastecimiento

                                            DROP TABLE #SP_ABASTECIMIENTO_PORPRODUCTO", parameters.fecha_inicial, parameters.fecha_final, parameters.grupo, parameters.grupo);
                            DataTable tableResults = adapter.GetDatatable(sql1);
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

        [Route("api/Report/507")]
        public IHttpActionResult postReport507(report507 parameters)
        {
            Object returnData = null;
            SQLAdapter adapter = new SQLAdapter();
            switch (parameters.tipo)
            {
                case "parametro":
                    switch (parameters.id)
                    {
                        case 1:
                            DataTable data1 = adapter.GetDatatable(@"SELECT  distinct   
                                                                    [AgronetSIPSA].dbo.Insumos_Productos.producto_InsumosProductos as descripcion,
                                                                    [AgronetSIPSA].dbo.Insumos_Productos.codProducto_InsumosProductos as  codigo
                                                                    FROM  [AgronetSIPSA].dbo.Insumos_Productos 
                                                                    INNER JOIN [AgronetSIPSA].dbo.Insumos_Precios ON [AgronetSIPSA].dbo.Insumos_Productos.codProducto_InsumosProductos = [AgronetSIPSA].dbo.Insumos_Precios.codProducto_InsumosPrecios 
                                                                    ORDER BY producto_InsumosProductos;");
                            Parameter param1 = new Parameter { name = "productos", data = new List<ParameterData>() };
                            foreach (var d in (from p in data1.AsEnumerable() select p))
                            {
                                ParameterData parameter = new ParameterData { name = Convert.ToString(d["descripcion"]), value = Convert.ToString(d["codigo"]) };
                                param1.data.Add(parameter);
                            }

                            returnData = (Parameter)param1;
                            break;
                        case 2:
                            DataTable data2 = adapter.GetDatatable(string.Format(@"SELECT  distinct   [AgronetSIPSA].dbo.Insumos_Unidad.codUnidad_InsumosUnidad as codigo,
                                                                                [AgronetSIPSA].dbo.Insumos_Unidad.unidad_InsumosUnidad as descripcion
                                                                                FROM [AgronetSIPSA].dbo.Insumos_Precios INNER JOIN
                                                                                [AgronetSIPSA].dbo.Insumos_Unidad ON 
                                                                                [AgronetSIPSA].dbo.Insumos_Precios.codUnidad_InsumosPrecios = [AgronetSIPSA].dbo.Insumos_Unidad.codUnidad_InsumosUnidad
                                                                                WHERE [AgronetSIPSA].dbo.Insumos_Precios.codProducto_InsumosPrecios = {0};", parameters.producto));
                            Parameter param2 = new Parameter { name = "unidades", data = new List<ParameterData>() };
                            foreach (var d in (from p in data2.AsEnumerable() select p))
                            {
                                ParameterData parameter = new ParameterData { name = Convert.ToString(d["descripcion"]), value = Convert.ToString(d["codigo"]) };
                                param2.data.Add(parameter);
                            }

                            returnData = (Parameter)param2;
                            break;
                        case 3:
                            DataTable data3 = adapter.GetDatatable(string.Format(@"SELECT DISTINCT 
                                                                                [AgronetSIPSA].dbo.Insumos_Departamentos.codigoDepartamento_InsumosDepto AS codigo, 
                                                                                [AgronetSIPSA].dbo.Insumos_Departamentos.departamento_InsumosDepto AS descripcion 
                                                                                FROM [AgronetSIPSA].dbo.Insumos_Departamentos 
                                                                                INNER JOIN [AgronetSIPSA].dbo.Insumos_Precios 
                                                                                ON [AgronetSIPSA].dbo.Insumos_Departamentos.codigoDepartamento_InsumosDepto = [AgronetSIPSA].dbo.Insumos_Precios.codigoDepartamento_InsumosPrecios 
                                                                                WHERE [AgronetSIPSA].dbo.Insumos_Precios.codProducto_InsumosPrecios = {0} ORDER BY descripcion;", parameters.producto));
                            Parameter param3 = new Parameter { name = "regiones", data = new List<ParameterData>() };
                            foreach (var d in (from p in data3.AsEnumerable() select p))
                            {
                                ParameterData parameter = new ParameterData { name = Convert.ToString(d["descripcion"]), value = Convert.ToString(d["codigo"]) };
                                param3.data.Add(parameter);
                            }

                            returnData = (Parameter)param3;
                            break;
                        case 4:
                            DataTable data4 = adapter.GetDatatable(@"SELECT DISTINCT YEAR(fecha_InsumosPrecios) as fecha 
                                                                    FROM  [AgronetSIPSA].dbo.Insumos_Precios
                                                                    ORDER BY YEAR(fecha_InsumosPrecios);");
                            Parameter param4 = new Parameter { name = "fecha", data = new List<ParameterData>() };
                            foreach (var d in (from p in data4.AsEnumerable() select p))
                            {
                                ParameterData parameter = new ParameterData { name = Convert.ToString(d["fecha"]), value = Convert.ToString(d["fecha"]) };
                                param4.data.Add(parameter);
                            }

                            returnData = (Parameter)param4;
                            break;
                    }
                    break;
                case "tabla":
                    switch (parameters.id)
                    {
                        case 1:
                            DataTable results1 = adapter.GetDatatable(String.Format(@"CREATE TABLE #SP_VARIACION_INSUMOS(
	                                                                                    codproducto int ,
	                                                                                    codmunicipio int,
	                                                                                    coddepto int,
	                                                                                    codunidad int,
	                                                                                    anho int,
	                                                                                    mes int,
	                                                                                    valor1 int,
	                                                                                    valor2 int,
	                                                                                    variacion float
                                                                                    )
                                                                                    INSERT INTO #SP_VARIACION_INSUMOS EXEC  [AgronetSIPSA].[dbo].[SP_VARIACION_INSUMOS]
		                                                                                    @FECHA_INICIAL = N'{0}-01-01',
		                                                                                    @FECHA_FINAL = N'{1}-12-31',
		                                                                                    @DEPARTAMENTO = {2},
		                                                                                    @PRODUCTO = {3},
		                                                                                    @UNIDAD = {4}

                                                                                    SELECT 
                                                                                    Insumos_Precios.anho_InsumosPrecios, 
                                                                                    mes_InsumosPrecios,
                                                                                    Insumos_Municipio.municipio_InsumosMuni, 
                                                                                    Insumos_Productos.producto_InsumosProductos,
                                                                                    Insumos_Precios.precio_InsumosPrecios,
                                                                                        #SP_VARIACION_INSUMOS.variacion
                                                                                        FROM   (AgronetSIPSA.dbo.Insumos_Precios Insumos_Precios 
	                                                                                    INNER JOIN AgronetSIPSA.dbo.Insumos_Municipio Insumos_Municipio	
	                                                                                    ON (Insumos_Precios.codigoMunicipio_InsumosPrecios=Insumos_Municipio.codigoMunicipio_InsumosMuni) 
	                                                                                    AND (Insumos_Precios.codigoDepartamento_InsumosPrecios=Insumos_Municipio.codigoDepartamento_InsumosMuni)
	                                                                                    ) 
                                                                                    INNER JOIN AgronetSIPSA.dbo.Insumos_Productos Insumos_Productos 
                                                                                    ON Insumos_Precios.codProducto_InsumosPrecios=Insumos_Productos.codProducto_InsumosProductos
                                                                                    INNER JOIN #SP_VARIACION_INSUMOS 
                                                                                    ON  #SP_VARIACION_INSUMOS.codproducto = Insumos_Precios.codProducto_InsumosPrecios
                                                                                    and #SP_VARIACION_INSUMOS.coddepto = Insumos_Precios.codigoDepartamento_InsumosPrecios
                                                                                    and #SP_VARIACION_INSUMOS.codmunicipio = Insumos_Precios.codigoMunicipio_InsumosPrecios
                                                                                    and #SP_VARIACION_INSUMOS.anho = Insumos_Precios.anho_InsumosPrecios
                                                                                    and #SP_VARIACION_INSUMOS.codunidad = Insumos_Precios.codUnidad_InsumosPrecios
                                                                                    WHERE  
                                                                                        Insumos_Precios.codProducto_InsumosPrecios= {5} 
                                                                                        AND Insumos_Precios.codUnidad_InsumosPrecios= {6} 
                                                                                        AND Insumos_Precios.codigoDepartamento_InsumosPrecios= {7}
                                                                                        ORDER BY Insumos_Municipio.municipio_InsumosMuni
                                                                                        DROP TABLE #SP_VARIACION_INSUMOS", parameters.fecha_inicial, 
                                                                                                                         parameters.fecha_final, 
                                                                                                                         parameters.region, 
                                                                                                                         parameters.producto, 
                                                                                                                         parameters.unidad, 
                                                                                                                         parameters.producto,
                                                                                                                         parameters.unidad,
                                                                                                                         parameters.region));

                            Table table = new Table { rows = results1 };
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

        [Route("api/Report/508")]
        public IHttpActionResult postReport508(report508 parameters)
        {
            Object returnData = null;
            SQLAdapter adapter = new SQLAdapter();
            switch (parameters.tipo)
            {
                case "parametro":
                    switch (parameters.id)
                    {
                        case 1:
                            DataTable data1 = adapter.GetDatatable(@"SELECT  distinct   
                                                                    [AgronetSIPSA].dbo.Insumos_Productos.producto_InsumosProductos as descripcion,
                                                                    [AgronetSIPSA].dbo.Insumos_Productos.codProducto_InsumosProductos as  codigo
                                                                    FROM  [AgronetSIPSA].dbo.Insumos_Productos 
                                                                    INNER JOIN [AgronetSIPSA].dbo.Insumos_Precios ON [AgronetSIPSA].dbo.Insumos_Productos.codProducto_InsumosProductos = [AgronetSIPSA].dbo.Insumos_Precios.codProducto_InsumosPrecios 
                                                                    ORDER BY producto_InsumosProductos;");
                            Parameter param1 = new Parameter { name = "productos", data = new List<ParameterData>() };
                            foreach (var d in (from p in data1.AsEnumerable() select p))
                            {
                                ParameterData parameter = new ParameterData { name = Convert.ToString(d["descripcion"]), value = Convert.ToString(d["codigo"]) };
                                param1.data.Add(parameter);
                            }

                            returnData = (Parameter)param1;
                            break;
                        case 2:
                            DataTable data2 = adapter.GetDatatable(string.Format(@"SELECT  distinct   [AgronetSIPSA].dbo.Insumos_Unidad.codUnidad_InsumosUnidad as codigo,
                                                                                [AgronetSIPSA].dbo.Insumos_Unidad.unidad_InsumosUnidad as descripcion
                                                                                FROM [AgronetSIPSA].dbo.Insumos_Precios INNER JOIN
                                                                                [AgronetSIPSA].dbo.Insumos_Unidad ON 
                                                                                [AgronetSIPSA].dbo.Insumos_Precios.codUnidad_InsumosPrecios = [AgronetSIPSA].dbo.Insumos_Unidad.codUnidad_InsumosUnidad
                                                                                WHERE [AgronetSIPSA].dbo.Insumos_Precios.codProducto_InsumosPrecios = {0};", parameters.producto));
                            Parameter param2 = new Parameter { name = "unidades", data = new List<ParameterData>() };
                            foreach (var d in (from p in data2.AsEnumerable() select p))
                            {
                                ParameterData parameter = new ParameterData { name = Convert.ToString(d["descripcion"]), value = Convert.ToString(d["codigo"]) };
                                param2.data.Add(parameter);
                            }

                            returnData = (Parameter)param2;
                            break;
                        case 3:
                            DataTable data3 = adapter.GetDatatable(string.Format(@"SELECT DISTINCT 
                                                                                [AgronetSIPSA].dbo.Insumos_Departamentos.codigoDepartamento_InsumosDepto AS codigo, 
                                                                                [AgronetSIPSA].dbo.Insumos_Departamentos.departamento_InsumosDepto AS descripcion 
                                                                                FROM [AgronetSIPSA].dbo.Insumos_Departamentos 
                                                                                INNER JOIN [AgronetSIPSA].dbo.Insumos_Precios 
                                                                                ON [AgronetSIPSA].dbo.Insumos_Departamentos.codigoDepartamento_InsumosDepto = [AgronetSIPSA].dbo.Insumos_Precios.codigoDepartamento_InsumosPrecios 
                                                                                WHERE [AgronetSIPSA].dbo.Insumos_Precios.codProducto_InsumosPrecios = {0} ORDER BY descripcion;", parameters.producto));
                            Parameter param3 = new Parameter { name = "regiones", data = new List<ParameterData>() };
                            foreach (var d in (from p in data3.AsEnumerable() select p))
                            {
                                ParameterData parameter = new ParameterData { name = Convert.ToString(d["descripcion"]), value = Convert.ToString(d["codigo"]) };
                                param3.data.Add(parameter);
                            }

                            returnData = (Parameter)param3;
                            break;
                        case 4:
                            DataTable data4 = adapter.GetDatatable(@"SELECT DISTINCT YEAR(fecha_InsumosPrecios) as fecha 
                                                                    FROM  [AgronetSIPSA].dbo.Insumos_Precios
                                                                    ORDER BY YEAR(fecha_InsumosPrecios);");
                            Parameter param4 = new Parameter { name = "fecha", data = new List<ParameterData>() };
                            foreach (var d in (from p in data4.AsEnumerable() select p))
                            {
                                ParameterData parameter = new ParameterData { name = Convert.ToString(d["fecha"]), value = Convert.ToString(d["fecha"]) };
                                param4.data.Add(parameter);
                            }

                            returnData = (Parameter)param4;
                            break;
                    }
                    break;
                case "tabla":
                    switch (parameters.id)
                    {
                        case 1:
                            DataTable results1 = adapter.GetDatatable(String.Format(@"CREATE TABLE #SP_VARIACION_INSUMOS(
	                                                                                    codproducto int ,
	                                                                                    codmunicipio int,
	                                                                                    coddepto int,
	                                                                                    codunidad int,
	                                                                                    anho int,
	                                                                                    mes int,
	                                                                                    valor1 int,
	                                                                                    valor2 int,
	                                                                                    variacion float
                                                                                    )
                                                                                    INSERT INTO #SP_VARIACION_INSUMOS EXEC  [AgronetSIPSA].[dbo].[SP_VARIACION_INSUMOS]
		                                                                                    @FECHA_INICIAL = N'{0}-01-01',
		                                                                                    @FECHA_FINAL = N'{1}-12-31',
		                                                                                    @DEPARTAMENTO = {2},
		                                                                                    @PRODUCTO = {3},
		                                                                                    @UNIDAD = {4}

                                                                                    SELECT 
                                                                                    mes_InsumosPrecios , 
                                                                                    Insumos_Precios.anho_InsumosPrecios, 
                                                                                    Insumos_Municipio.municipio_InsumosMuni, 
                                                                                    Insumos_Precios.precio_InsumosPrecios, 
                                                                                    Insumos_Precios.codProducto_InsumosPrecios, 
                                                                                    Insumos_Precios.codUnidad_InsumosPrecios, 
                                                                                    Insumos_Productos.producto_InsumosProductos, 
                                                                                    Insumos_Precios.codigoDepartamento_InsumosPrecios, 
                                                                                    Insumos_Precios.codigoMunicipio_InsumosPrecios, 
                                                                                    Insumos_Municipio.codigoDepartamento_InsumosMuni, 
                                                                                    Insumos_Productos.idGrupo_InsumosProductos
                                                                                        FROM   (AgronetSIPSA.dbo.Insumos_Precios Insumos_Precios 
	                                                                                    INNER JOIN AgronetSIPSA.dbo.Insumos_Municipio Insumos_Municipio	
	                                                                                    ON (Insumos_Precios.codigoMunicipio_InsumosPrecios=Insumos_Municipio.codigoMunicipio_InsumosMuni) 
	                                                                                    AND (Insumos_Precios.codigoDepartamento_InsumosPrecios=Insumos_Municipio.codigoDepartamento_InsumosMuni)
	                                                                                    ) 
                                                                                    INNER JOIN AgronetSIPSA.dbo.Insumos_Productos Insumos_Productos 
                                                                                    ON Insumos_Precios.codProducto_InsumosPrecios=Insumos_Productos.codProducto_InsumosProductos
                                                                                    INNER JOIN #SP_VARIACION_INSUMOS 
                                                                                    ON  #SP_VARIACION_INSUMOS.codproducto = Insumos_Precios.codProducto_InsumosPrecios
                                                                                    and #SP_VARIACION_INSUMOS.coddepto = Insumos_Precios.codigoDepartamento_InsumosPrecios
                                                                                    and #SP_VARIACION_INSUMOS.codmunicipio = Insumos_Precios.codigoMunicipio_InsumosPrecios
                                                                                    and #SP_VARIACION_INSUMOS.anho = Insumos_Precios.anho_InsumosPrecios
                                                                                    and #SP_VARIACION_INSUMOS.codunidad = Insumos_Precios.codUnidad_InsumosPrecios
                                                                                    WHERE  
                                                                                        Insumos_Precios.codProducto_InsumosPrecios= {5} 
                                                                                        AND Insumos_Precios.codUnidad_InsumosPrecios= {6} 
                                                                                        AND Insumos_Precios.codigoDepartamento_InsumosPrecios= {7}
                                                                                        ORDER BY Insumos_Municipio.municipio_InsumosMuni
                                                                                        DROP TABLE #SP_VARIACION_INSUMOS", parameters.fecha_inicial,
                                                                                                                         parameters.fecha_final,
                                                                                                                         parameters.region,
                                                                                                                         parameters.producto,
                                                                                                                         parameters.unidad,
                                                                                                                         parameters.producto,
                                                                                                                         parameters.unidad,
                                                                                                                         parameters.region));
                            Table table = new Table { rows = results1 };
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

        [Route("api/Report/509")]
        public IHttpActionResult postReport509(report509 parameters)
        {
            Object returnData = null;
            SQLAdapter adapter = new SQLAdapter();
            switch (parameters.tipo)
            {
                case "parametro":
                    switch (parameters.id)
                    {
                        case 1:
                            DataTable data1 = adapter.GetDatatable(@"SELECT DISTINCT VW_Franjas.anho_FranjasArancel AS fecha
                                                                        FROM   AgronetComercio.dbo.VW_Franjas VW_Franjas
                                                                        WHERE  VW_Franjas.codigoTipoProducto_FranjasPrecios=1
                                                                        ORDER BY VW_Franjas.anho_FranjasArancel;");
                            Parameter param1 = new Parameter { name = "fecha", data = new List<ParameterData>() };
                            foreach (var d in (from p in data1.AsEnumerable() select p))
                            {
                                ParameterData parameter = new ParameterData { name = Convert.ToString(d["fecha"]), value = Convert.ToString(d["fecha"]) };
                                param1.data.Add(parameter);
                            }

                            returnData = (Parameter)param1;
                            break;
                    }
                    break;
                case "tabla":
                    switch (parameters.id)
                    {
                        case 1:
                            DataTable results1 = adapter.GetDatatable(String.Format(@"SELECT VW_Franjas.anho_FranjasArancel, 
                                                                                        VW_Franjas.mes_FranjasMeses, 
                                                                                        VW_Franjas.quincena_FranjasArancel, 
                                                                                        VW_Franjas.nombreFranja_Franjas, 
                                                                                        VW_Franjas.precio_FranjasPrecios, 
                                                                                        VW_Franjas.arancel_FranjasArancel, 
                                                                                        VW_Franjas.numMes_FranjasMeses, 
                                                                                        VW_Franjas.codigoTipoProducto_FranjasPrecios
                                                                                        FROM   AgronetComercio.dbo.VW_Franjas VW_Franjas
                                                                                        WHERE  VW_Franjas.codigoTipoProducto_FranjasPrecios=1
                                                                                        AND VW_Franjas.anho_FranjasArancel = '{0}'
                                                                                        ORDER BY VW_Franjas.nombreFranja_Franjas", parameters.anio));
                            Table table = new Table { rows = results1 };
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

        [Route("api/Report/510")]
        public IHttpActionResult postReport510(report510 parameters)
        {
            Object returnData = null;
            SQLAdapter adapter = new SQLAdapter();
            switch (parameters.tipo)
            {
                case "parametro":
                    switch (parameters.id)
                    {
                        case 1:
                            DataTable data1 = adapter.GetDatatable(@"SELECT DISTINCT VW_Franjas.anho_FranjasArancel AS fecha
                                                                        FROM   AgronetComercio.dbo.VW_Franjas VW_Franjas
                                                                        WHERE  VW_Franjas.codigoTipoProducto_FranjasPrecios=1
                                                                        ORDER BY VW_Franjas.anho_FranjasArancel;");
                            Parameter param1 = new Parameter { name = "fecha", data = new List<ParameterData>() };
                            foreach (var d in (from p in data1.AsEnumerable() select p))
                            {
                                ParameterData parameter = new ParameterData { name = Convert.ToString(d["fecha"]), value = Convert.ToString(d["fecha"]) };
                                param1.data.Add(parameter);
                            }

                            returnData = (Parameter)param1;
                            break;
                        case 2:
                            Parameter param2 = new Parameter { name = "mes", data = new List<ParameterData>() };
                            for (int mes = 1; mes <= 12; mes++)
                                param2.data.Add(new ParameterData { name = Convert.ToString(mes), value = Convert.ToString(mes) });

                            returnData = (Parameter)param2;
                            break;
                        case 3:
                            Parameter param3 = new Parameter { name = "quincena", data = new List<ParameterData>() };
                            param3.data.Add(new ParameterData { name = "1", value = "1" });
                            param3.data.Add(new ParameterData { name = "2", value = "2" });

                            returnData = (Parameter)param3;
                            break;
                    }
                    break;
                case "tabla":
                    switch (parameters.id)
                    {
                        case 1:
                            DataTable results1 = adapter.GetDatatable(String.Format(@"SELECT Franjas_Arancel.nandina_FranjasArancel, Franjas_Arancel.mes_FranjasArancel, Franjas_Arancel.quincena_FranjasArancel, Franjas_Arancel.anho_FranjasArancel, Franjas_TipoProducto.nombreTipoProducto_FranjasTipoProducto, Franjas.nombreFranja_Franjas, Franjas_Arancel.arancel_FranjasArancel, Franjas_Precios.precio_FranjasPrecios
FROM   (AgronetComercio.dbo.Franjas_Precios Franjas_Precios FULL OUTER JOIN (AgronetComercio.dbo.Franjas Franjas INNER JOIN AgronetComercio.dbo.Franjas_Arancel Franjas_Arancel ON Franjas.codigoFranja_Franjas=Franjas_Arancel.codigoFranja_FranjasArancel) ON (((Franjas_Precios.nandina_FranjasPrecios=Franjas_Arancel.nandina_FranjasArancel) AND (Franjas_Precios.mes_FranjasPrecios=Franjas_Arancel.mes_FranjasArancel)) AND (Franjas_Precios.quincena_FranjasPrecios=Franjas_Arancel.quincena_FranjasArancel)) AND (Franjas_Precios.anho_FranjasPrecios=Franjas_Arancel.anho_FranjasArancel)) INNER JOIN AgronetComercio.dbo.Franjas_TipoProducto Franjas_TipoProducto ON Franjas_Arancel.codigoTipoProducto_FranjasArancel=Franjas_TipoProducto.codigoTipoProducto_FranjasTipoProducto
WHERE  Franjas_TipoProducto.nombreTipoProducto_FranjasTipoProducto=N'Producto Marcador'
AND Franjas_Arancel.mes_FranjasArancel = {0}
AND Franjas_Arancel.quincena_FranjasArancel = {1}
AND Franjas_Arancel.anho_FranjasArancel = {2}", parameters.mes, parameters.quincena, parameters.anio));
                            Table table = new Table { rows = results1 };
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

        [Route("api/Report/511")]
        public IHttpActionResult postReport511(report511 parameters)
        {
            Object returnData = null;
            SQLAdapter adapter = new SQLAdapter();
            switch (parameters.tipo)
            {
                case "parametro":
                    switch (parameters.id)
                    {
                        case 1:
                            DataTable data1 = adapter.GetDatatable(@"select distinct YEAR(fecha_FertilizantesPrecios) anio
                                                                    from [AgronetProyecciones].dbo.Proyecciones_FertilizantesPrecios
                                                                    order by YEAR(fecha_FertilizantesPrecios);");
                            Parameter param1 = new Parameter { name = "fecha", data = new List<ParameterData>() };
                            foreach (var d in (from p in data1.AsEnumerable() select p))
                            {
                                ParameterData parameter = new ParameterData { name = Convert.ToString(d["anio"]), value = Convert.ToString(d["anio"]) };
                                param1.data.Add(parameter);
                            }

                            returnData = (Parameter)param1;
                            break;
                        case 2:
                            DataTable data2 = adapter.GetDatatable(@"select codigoProducto_ProductosFertilizantes, producto_ProductosFertilizantes 
                                                                        from [AgronetProyecciones].dbo.Proyecciones_FertilizantesProductos
                                                                        where codigoProducto_ProductosFertilizantes <> 7;");
                            Parameter param2 = new Parameter { name = "producto", data = new List<ParameterData>() };
                            foreach (var d in (from p in data2.AsEnumerable() select p))
                            {
                                ParameterData parameter = new ParameterData { name = Convert.ToString(d["producto_ProductosFertilizantes"]), value = Convert.ToString(d["codigoProducto_ProductosFertilizantes"]) };
                                param2.data.Add(parameter);
                            }

                            returnData = (Parameter)param2;
                            break;
                    }
                    break;
                case "grafico":
                    switch (parameters.id)
                    {
                        case 1:
                            DataTable results1 = adapter.GetDatatable(String.Format(@"CREATE TABLE #SP_PRECIOS_FERTILIZANTES (
                                                                                        fecha date,
                                                                                        codigoProducto int,
                                                                                        precio int,
                                                                                        variacionSemPrecio float
                                                                                        ) 
                                                                                        INSERT INTO #SP_PRECIOS_FERTILIZANTES EXEC [AgronetProyecciones].[dbo].[SP_PRECIOS_FERTILIZANTES]
		                                                                                    --PARAMETROS
		                                                                                    @Fecha_Inicial = N'{0}-01-01',
		                                                                                    @Fecha_Final = N'{1}-12-31',
		                                                                                    @Producto = {2}
                                                                                        SELECT
                                                                                        DATEDIFF(ss, '01/01/1970',#SP_PRECIOS_FERTILIZANTES.fecha) as fechaunix,
                                                                                        #SP_PRECIOS_FERTILIZANTES.precio
                                                                                        FROM   
                                                                                        AgronetProyecciones.dbo.Proyecciones_FertilizantesProductos Proyecciones_FertilizantesProductos
                                                                                    INNER JOIN  
                                                                                    #SP_PRECIOS_FERTILIZANTES 
                                                                                    ON #SP_PRECIOS_FERTILIZANTES.codigoProducto = Proyecciones_FertilizantesProductos.codigoProducto_ProductosFertilizantes
                                                                                    DROP TABLE #SP_PRECIOS_FERTILIZANTES", parameters.fecha_inicial, parameters.fecha_final, parameters.producto));
                            Chart chart1 = new Chart { subtitle = "", series = new List<Series>() };

                            Series serie1 = new Series { name = "$US/ton", data = new List<Data>() };
                            foreach (var d1 in (from d in results1.AsEnumerable()
                                                select d))
                            {
                                Data data1 = new Data { name = Convert.ToString(d1["fechaunix"]), y = Convert.ToDouble(d1["precio"]) };
                                serie1.data.Add(data1);
                            }
                            chart1.series.Add(serie1);
                        
                            returnData = (Chart)chart1;
                            break;
                    }
                    break;
                case "tabla":
                    switch (parameters.id)
                    {
                        case 1:
                           DataTable results1 = adapter.GetDatatable(String.Format(@"CREATE TABLE #SP_PRECIOS_FERTILIZANTES (
                                                                                        fecha date,
                                                                                        codigoProducto int,
                                                                                        precio int,
                                                                                        variacionSemPrecio float
                                                                                        ) 
                                                                                        INSERT INTO #SP_PRECIOS_FERTILIZANTES EXEC [AgronetProyecciones].[dbo].[SP_PRECIOS_FERTILIZANTES]
		                                                                                    --PARAMETROS
		                                                                                    @Fecha_Inicial = N'{0}-01-01',
		                                                                                    @Fecha_Final = N'{1}-12-31',
		                                                                                    @Producto = {2}
                                                                                        SELECT
                                                                                        DATEDIFF(ss, '01/01/1970',#SP_PRECIOS_FERTILIZANTES.fecha) as fechaunix,
                                                                                        #SP_PRECIOS_FERTILIZANTES.fecha,
                                                                                        #SP_PRECIOS_FERTILIZANTES.precio,
                                                                                        ISNULL(#SP_PRECIOS_FERTILIZANTES.variacionSemPrecio,0) variacion,
                                                                                        Proyecciones_FertilizantesProductos.Unidad_ProductosFertilizantes, 
                                                                                        Proyecciones_FertilizantesProductos.producto_ProductosFertilizantes
                                                                                        FROM   
                                                                                        AgronetProyecciones.dbo.Proyecciones_FertilizantesProductos Proyecciones_FertilizantesProductos
                                                                                    INNER JOIN  
                                                                                    #SP_PRECIOS_FERTILIZANTES 
                                                                                    ON #SP_PRECIOS_FERTILIZANTES.codigoProducto = Proyecciones_FertilizantesProductos.codigoProducto_ProductosFertilizantes
                                                                                    DROP TABLE #SP_PRECIOS_FERTILIZANTES
                                                                                    ", parameters.fecha_inicial, parameters.fecha_final, parameters.producto));
                            Table table = new Table { rows = results1 };
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

        [Route("api/Report/514")]
        public IHttpActionResult postReport515(report514 parameters)
        {
            Object returnData = null;
            SQLAdapter adapter = new SQLAdapter();
            switch (parameters.tipo)
            {
                case "parametro":
                    switch (parameters.id)
                    {
                        case 1:
                            DataTable data1 = adapter.GetDatatable(@"SELECT distinct [anho_Proyecciones] as codigo,  [anho_Proyecciones] as descripcion
                                                                    FROM [AgronetProyecciones].[dbo].[Proyecciones_Fletes]
                                                                    order by 1;");
                            Parameter param1 = new Parameter { name = "anio", data = new List<ParameterData>() };
                            foreach (var d in (from p in data1.AsEnumerable() select p))
                            {
                                ParameterData parameter = new ParameterData { name = Convert.ToString(d["descripcion"]), value = Convert.ToString(d["codigo"]) };
                                param1.data.Add(parameter);
                            }

                            returnData = (Parameter)param1;
                            break;
                        case 2:
                            DataTable data2 = adapter.GetDatatable(@"SELECT [codMunicipio_MunicipioOrigen] as codigo, [municipio_MunicipioOrigen] as descripcion
                                                                        FROM [AgronetProyecciones].[dbo].[Proyecciones_MunicipiosOrigen] order by 2");
                            Parameter param2 = new Parameter { name = "ciudad", data = new List<ParameterData>() };
                            foreach (var d in (from p in data2.AsEnumerable() select p))
                            {
                                ParameterData parameter = new ParameterData { name = Convert.ToString(d["descripcion"]), value = Convert.ToString(d["codigo"]) };
                                param2.data.Add(parameter);
                            }

                            returnData = (Parameter)param2;
                            break;
                    }
                    break;
                case "grafico":
                    switch (parameters.id)
                    {
                        case 1:
                            DataTable results1 = adapter.GetDatatable(String.Format(@"SELECT 
                                        Proyecciones_Fletes.codOrigen_Proyecciones, 
                                        Proyecciones_Fletes.anho_Proyecciones, 
                                        Proyecciones_MunicipiosDestino.municipio_MunicipioDestino, 
                                        Proyecciones_Fletes.flete_Proyecciones, 
                                        Proyecciones_MunicipiosOrigen.municipio_MunicipioOrigen
                                        FROM   
                                        (AgronetProyecciones.dbo.Proyecciones_Fletes Proyecciones_Fletes 
                                        INNER JOIN AgronetProyecciones.dbo.Proyecciones_MunicipiosDestino Proyecciones_MunicipiosDestino ON Proyecciones_Fletes.codDestino_Proyecciones=Proyecciones_MunicipiosDestino.codMunicipio_MunicipioDestino) 
                                        INNER JOIN AgronetProyecciones.dbo.Proyecciones_MunicipiosOrigen Proyecciones_MunicipiosOrigen ON Proyecciones_Fletes.codOrigen_Proyecciones=Proyecciones_MunicipiosOrigen.codMunicipio_MunicipioOrigen
                                        WHERE  
                                        Proyecciones_Fletes.codOrigen_Proyecciones=N'{0}' 
                                        AND Proyecciones_Fletes.anho_Proyecciones={1}
                                        ORDER BY Proyecciones_MunicipiosDestino.municipio_MunicipioDestino", parameters.ciudad, parameters.anio));
                            Chart chart1 = new Chart { subtitle = "", series = new List<Series>() };

                            Series serie1 = new Series { name = "$ por tonelada", data = new List<Data>() };
                            foreach (var d1 in (from d in results1.AsEnumerable()
                                                select d))
                            {
                                Data data1 = new Data { name = Convert.ToString(d1["municipio_MunicipioDestino"]), y = Convert.ToDouble(d1["flete_Proyecciones"]) };
                                serie1.data.Add(data1);
                            }
                            chart1.series.Add(serie1);
                        
                            returnData = (Chart)chart1;
                            break;
                    }
                    break;
                case "tabla":
                    switch (parameters.id)
                    {
                        case 1:
                            DataTable results1 = adapter.GetDatatable(String.Format(@"SELECT 
                                        Proyecciones_Fletes.codOrigen_Proyecciones, 
                                        Proyecciones_Fletes.anho_Proyecciones, 
                                        Proyecciones_MunicipiosDestino.municipio_MunicipioDestino, 
                                        Proyecciones_Fletes.flete_Proyecciones, 
                                        Proyecciones_MunicipiosOrigen.municipio_MunicipioOrigen
                                        FROM   
                                        (AgronetProyecciones.dbo.Proyecciones_Fletes Proyecciones_Fletes 
                                        INNER JOIN AgronetProyecciones.dbo.Proyecciones_MunicipiosDestino Proyecciones_MunicipiosDestino ON Proyecciones_Fletes.codDestino_Proyecciones=Proyecciones_MunicipiosDestino.codMunicipio_MunicipioDestino) 
                                        INNER JOIN AgronetProyecciones.dbo.Proyecciones_MunicipiosOrigen Proyecciones_MunicipiosOrigen ON Proyecciones_Fletes.codOrigen_Proyecciones=Proyecciones_MunicipiosOrigen.codMunicipio_MunicipioOrigen
                                        WHERE  
                                        Proyecciones_Fletes.codOrigen_Proyecciones=N'{0}' 
                                        AND Proyecciones_Fletes.anho_Proyecciones={1}
                                        ORDER BY Proyecciones_MunicipiosDestino.municipio_MunicipioDestino", parameters.ciudad, parameters.anio));
                            Table table = new Table { rows = results1 };
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

        [Route("api/Report/515")]
        public IHttpActionResult postReport515(report515 parameters)
        {
            Object returnData = null;
            SQLAdapter adapter = new SQLAdapter();
            switch (parameters.tipo)
            {
                case "parametro":
                    switch (parameters.id)
                    {
                        case 1:
                            DataTable data1 = adapter.GetDatatable(@"select distinct year(fecha_Existencias) anio from [AgronetIndicadores].dbo.Indicadores_Existencias
                                                                        order by year(fecha_Existencias);");
                            Parameter param1 = new Parameter { name = "anio", data = new List<ParameterData>() };
                            foreach (var d in (from p in data1.AsEnumerable() select p))
                            {
                                ParameterData parameter = new ParameterData { name = Convert.ToString(d["anio"]), value = Convert.ToString(d["anio"]) };
                                param1.data.Add(parameter);
                            }

                            returnData = (Parameter)param1;
                            break;
                        case 2:
                            DataTable data2 = adapter.GetDatatable(@"SELECT [codigoProducto_ExistenciaProductos], [nombreProducto_ExistenciaProductos]
                                                                        FROM [AgronetIndicadores].[dbo].[Indicadores_ExistenciaProductos];");
                            Parameter param2 = new Parameter { name = "productos", data = new List<ParameterData>() };
                            foreach (var d in (from p in data2.AsEnumerable() select p))
                            {
                                ParameterData parameter = new ParameterData { name = Convert.ToString(d["nombreProducto_ExistenciaProductos"]), value = Convert.ToString(d["codigoProducto_ExistenciaProductos"]) };
                                param2.data.Add(parameter);
                            }

                            returnData = (Parameter)param2;
                            break;
                    }
                    break;
                case "grafico":
                    switch (parameters.id)
                    {
                        case 1:
                            DataTable results1 = adapter.GetDatatable(String.Format(@"SELECT 
DATEDIFF(ss, '01/01/1970',Indicadores_Existencias.fecha_Existencias) as fechaunix,
Indicadores_Existencias.fecha_Existencias, 
Indicadores_Existencias.codigoProducto_Existencias,
Indicadores_ExistenciaProductos.nombreProducto_ExistenciaProductos, 
Indicadores_Existencias.valor_Existencias
FROM   AgronetIndicadores.dbo.Indicadores_Existencias Indicadores_Existencias 
INNER JOIN AgronetIndicadores.dbo.Indicadores_ExistenciaProductos Indicadores_ExistenciaProductos
ON Indicadores_Existencias.codigoProducto_Existencias=Indicadores_ExistenciaProductos.codigoProducto_ExistenciaProductos
WHERE
Indicadores_Existencias.fecha_Existencias BETWEEN '{0}-01-01' AND '{1}-12-31'
AND  Indicadores_Existencias.codigoProducto_Existencias = {2}", parameters.fecha_inicial, parameters.fecha_final, parameters.producto));
                            Chart chart1 = new Chart { subtitle = "", series = new List<Series>() };

                            Series serie1 = new Series { name = "ton", data = new List<Data>() };
                            foreach (var d1 in (from d in results1.AsEnumerable()
                                                select d))
                            {
                                Data data1 = new Data { name = Convert.ToString(d1["fechaunix"]), y = Convert.ToDouble(d1["valor_Existencias"]) };
                                serie1.data.Add(data1);
                            }
                            chart1.series.Add(serie1);
                        
                            returnData = (Chart)chart1;
                            break;
                    }
                    break;
                case "tabla":
                    switch (parameters.id)
                    {
                        case 1:
                            DataTable results1 = adapter.GetDatatable(String.Format(@"SELECT 
Indicadores_Existencias.fecha_Existencias, 
Indicadores_ExistenciaProductos.nombreProducto_ExistenciaProductos, 
Indicadores_Existencias.valor_Existencias
FROM   AgronetIndicadores.dbo.Indicadores_Existencias Indicadores_Existencias 
INNER JOIN AgronetIndicadores.dbo.Indicadores_ExistenciaProductos Indicadores_ExistenciaProductos
ON Indicadores_Existencias.codigoProducto_Existencias=Indicadores_ExistenciaProductos.codigoProducto_ExistenciaProductos
WHERE
Indicadores_Existencias.fecha_Existencias BETWEEN '{0}-01-01' AND '{1}-12-31'
AND  Indicadores_Existencias.codigoProducto_Existencias = {2}", parameters.fecha_inicial, parameters.fecha_final, parameters.producto));
                            Table table = new Table { rows = results1 };
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

        private double VolatilidadHistorica(IEnumerable<double> values)
        {
            double ret = 0;
            List<double> vol = new List<double>();
            if (values.Count() > 0)
            {
                for (int i = 0; i < values.Count(); i++)
                {
                    if (i > 0)
                    {
                        vol.Add((Math.Log(values.ElementAt(i) / values.ElementAt(i - 1)) * 100));
                    }
                }

                ret = StdDev(vol.AsEnumerable());
            }
            return ret;
        }

        private double StdDev(IEnumerable<double> values)
        {
            double ret = 0;

            if (values.Count() > 0)
            {
                double avg = values.Average();
                double sum = values.Sum(d => Math.Pow(d - avg, 2));
                ret = Math.Sqrt((sum) / values.Count() - 1);
            }
            return ret;
        }

        private double Covariance(IEnumerable<double> source, IEnumerable<double> other)
        {
            int len = source.Count();

            double avgSource = source.Average();
            double avgOther = other.Average();
            double covariance = 0;

            for (int i = 0; i < len; i++)
                covariance += (source.ElementAt(i) - avgSource) * (other.ElementAt(i) - avgOther);

            return covariance / len;
        }
    }

}
