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

                            String sqlp1 = @"SELECT DISTINCT Sipsa_ProductosSemanales.nombreProducto_ProductosSemanales AS producto, 
                                            Sipsa_ProductosSemanales.codigoProducto_ProductosSemanales AS codigo 
                                            FROM SipsaSemanal INNER JOIN Sipsa_ProductosSemanales 
	                                            ON SipsaSemanal.codProducto_SipsaSemanal =      Sipsa_ProductosSemanales.codigoProducto_ProductosSemanales
                                            /*WHERE  Sipsa_ProductosSemanales.codigoProducto_ProductosSemanales = 121*/
                                            ORDER BY descripcion";

                            DataTable datap1 = adapter.GetDatatable(sqlp1);
                            Parameter param1 = new Parameter { name = "producto", data = new List<ParameterData>() };
                            foreach (var d in (from p in datap1.AsEnumerable() select p[@"producto"]))
                            {
                                ParameterData parameter = new ParameterData { name = Convert.ToString(d), value = Convert.ToString(d) };
                                param1.data.Add(parameter);
                            }

                            returnData = (Parameter)param1;


                            break;
                        case 2:

                            String sqlp2 = @"SELECT 
                                            AgronetCadenas.compraLeche.precioDepartamental.codigoDepartamento_PrecioDepartamental as codigoDepartamento,
                                             AgronetCadenas.Leche.regionDepartamento.descripcionDepartamento_RegionDepartamento as departamento
                                             FROM  AgronetCadenas.compraLeche.precioDepartamental 
                                             INNER JOIN   AgronetCadenas.Leche.regionDepartamento 
                                             ON AgronetCadenas.compraLeche.precioDepartamental.codigoDepartamento_PrecioDepartamental = 
                                             AgronetCadenas.Leche.regionDepartamento.codigoDepartamento_RegionDepartamento 
                                             GROUP BY AgronetCadenas.compraLeche.precioDepartamental.codigoDepartamento_PrecioDepartamental, 
                                             AgronetCadenas.Leche.regionDepartamento.descripcionDepartamento_RegionDepartamento";

                            DataTable datap2 = adapter.GetDatatable(sqlp2);
                            Parameter param2 = new Parameter { name = "departamentos", data = new List<ParameterData>() };
                            foreach (var d in (from p in datap2.AsEnumerable() select p[@"departamento"]))
                            {
                                ParameterData parameter = new ParameterData { name = Convert.ToString(d), value = Convert.ToString(d) };
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
                                        @Fecha_final = N'" + parameters.fecha_final + @"-10-01'

                                SELECT 
                                regionDepartamento.descripcionDepartamento_RegionDepartamento as departamento, 
                                regionDepartamento.codigoDepartamento_RegionDepartamento as codigoDepartamento,
                                #SP_PRECIOS_LECHE_GANADERO_DEPTO.fecha as fecha,
                                ISNULL(#SP_PRECIOS_LECHE_GANADERO_DEPTO.precio,0) as precio,
                                ISNULL(#SP_PRECIOS_LECHE_GANADERO_DEPTO.VariacionMesPrecio,0) as variacionPrecio,
                                ISNULL(#SP_PRECIOS_LECHE_GANADERO_DEPTO.VariacionAnualPrecio,0) as variacionVolumen
                                FROM   AgronetCadenas.Leche.regionDepartamento regionDepartamento INNER JOIN #SP_PRECIOS_LECHE_GANADERO_DEPTO 
                                ON #SP_PRECIOS_LECHE_GANADERO_DEPTO.codigoDepartamento = regionDepartamento.codigoDepartamento_RegionDepartamento 
                                WHERE #SP_PRECIOS_LECHE_GANADERO_DEPTO.fecha between '" + parameters.fecha_inicial + @"-01-01' and '" + parameters.fecha_final + @"-01-01'
                                 and regionDepartamento.descripcionDepartamento_RegionDepartamento in (" + string.Join(",", parameters.departamento.Select(d => "'" + d + "'")) + @")
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
                                                        @Fecha_final = N'" + parameters.fecha_final + @"-10-01'

                                                SELECT 
                                                regionDepartamento.descripcionDepartamento_RegionDepartamento as departamento, 
                                                regionDepartamento.codigoDepartamento_RegionDepartamento as codigoDepartamento,
                                                #SP_PRECIOS_LECHE_GANADERO_DEPTO.fecha as fecha,
                                                ISNULL(#SP_PRECIOS_LECHE_GANADERO_DEPTO.precio,0) as precio,
                                                ISNULL(#SP_PRECIOS_LECHE_GANADERO_DEPTO.VariacionMesPrecio,0) as variacionPrecio,
                                                ISNULL(#SP_PRECIOS_LECHE_GANADERO_DEPTO.VariacionAnualPrecio,0) as variacionVolumen
                                                FROM   AgronetCadenas.Leche.regionDepartamento regionDepartamento INNER JOIN #SP_PRECIOS_LECHE_GANADERO_DEPTO 
                                                ON #SP_PRECIOS_LECHE_GANADERO_DEPTO.codigoDepartamento = regionDepartamento.codigoDepartamento_RegionDepartamento 
                                                WHERE #SP_PRECIOS_LECHE_GANADERO_DEPTO.fecha between '" + parameters.fecha_inicial + @"-01-01' and '" + parameters.fecha_final + @"-01-01'
                                                    and regionDepartamento.descripcionDepartamento_RegionDepartamento in (" + string.Join(",", parameters.departamento.Select(d => "'" + d + "'")) + @")
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

        [Route("api/Report/503")]
        public IHttpActionResult postReport503(report503 parameters)
        {
            Object returnData = null;
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
            if (returnData == null)
            {
                return NotFound();
            }

            return Ok(returnData);
        }
    }
}
