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
                            DataTable data3 = adapter.GetDatatable(@"SELECT DISTINCT YEAR(fecha_SipsaSemanal) as fecha FROM [AgronetSIPSA].dbo.SipsaSemanal
WHERE [AgronetSIPSA].dbo.SipsaSemanal.codMercado_SipsaSemanal = 29
ORDER BY YEAR(fecha_SipsaSemanal);");
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
                            break;
                        case 2:
                            break;
                        case 3:
                            break;
                    }
                    break;
                case "grafico":
                    switch (parameters.id)
                    {
                        case 1:
                            break;
                        case 2:
                            break;
                        case 3:
                            break;
                    }
                    break;
                case "tabla":
                    switch (parameters.id)
                    {
                        case 1:
                            break;
                        case 2:
                            break;
                        case 3:
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
                            break;
                        case 2:
                            break;
                        case 3:
                            break;
                    }
                    break;
                case "grafico":
                    switch (parameters.id)
                    {
                        case 1:
                            break;
                        case 2:
                            break;
                        case 3:
                            break;
                    }
                    break;
                case "tabla":
                    switch (parameters.id)
                    {
                        case 1:
                            break;
                        case 2:
                            break;
                        case 3:
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
                            break;
                        case 2:
                            break;
                        case 3:
                            break;
                    }
                    break;
                case "grafico":
                    switch (parameters.id)
                    {
                        case 1:
                            break;
                        case 2:
                            break;
                        case 3:
                            break;
                    }
                    break;
                case "tabla":
                    switch (parameters.id)
                    {
                        case 1:
                            break;
                        case 2:
                            break;
                        case 3:
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
                            Parameter param3 = new Parameter { name = "grupos", data = new List<ParameterData>() };
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
                                             AND Abastecimiento_Sitios.ciudad_sitioAbastecimiento IN ("+string.Join(",",parameters.ciudad.Select(d => "'"+d+"'")) +
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
                                    var name = Convert.ToDateTime(el1["fecha"]);
                                    var data = new Data { name = String.Format("{0:y}", name), y = Convert.ToDouble(el1["toneladas"]) };
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
                            break;
                        case 2:
                            break;
                        case 3:
                            break;
                    }
                    break;
                case "grafico":
                    switch (parameters.id)
                    {
                        case 1:
                            break;
                        case 2:
                            break;
                        case 3:
                            break;
                    }
                    break;
                case "tabla":
                    switch (parameters.id)
                    {
                        case 1:
                            break;
                        case 2:
                            break;
                        case 3:
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
                            break;
                        case 2:
                            break;
                        case 3:
                            break;
                    }
                    break;
                case "grafico":
                    switch (parameters.id)
                    {
                        case 1:
                            break;
                        case 2:
                            break;
                        case 3:
                            break;
                    }
                    break;
                case "tabla":
                    switch (parameters.id)
                    {
                        case 1:
                            break;
                        case 2:
                            break;
                        case 3:
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
                            break;
                        case 2:
                            break;
                        case 3:
                            break;
                    }
                    break;
                case "grafico":
                    switch (parameters.id)
                    {
                        case 1:
                            break;
                        case 2:
                            break;
                        case 3:
                            break;
                    }
                    break;
                case "tabla":
                    switch (parameters.id)
                    {
                        case 1:
                            break;
                        case 2:
                            break;
                        case 3:
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
                            break;
                        case 2:
                            break;
                        case 3:
                            break;
                    }
                    break;
                case "grafico":
                    switch (parameters.id)
                    {
                        case 1:
                            break;
                        case 2:
                            break;
                        case 3:
                            break;
                    }
                    break;
                case "tabla":
                    switch (parameters.id)
                    {
                        case 1:
                            break;
                        case 2:
                            break;
                        case 3:
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
                            break;
                        case 2:
                            break;
                        case 3:
                            break;
                    }
                    break;
                case "grafico":
                    switch (parameters.id)
                    {
                        case 1:
                            break;
                        case 2:
                            break;
                        case 3:
                            break;
                    }
                    break;
                case "tabla":
                    switch (parameters.id)
                    {
                        case 1:
                            break;
                        case 2:
                            break;
                        case 3:
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
                            break;
                        case 2:
                            break;
                        case 3:
                            break;
                    }
                    break;
                case "grafico":
                    switch (parameters.id)
                    {
                        case 1:
                            break;
                        case 2:
                            break;
                        case 3:
                            break;
                    }
                    break;
                case "tabla":
                    switch (parameters.id)
                    {
                        case 1:
                            break;
                        case 2:
                            break;
                        case 3:
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
