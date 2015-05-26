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
    public class C2Controller : ApiController
    {
        
        [Route("api/Report/201")]
        public IHttpActionResult postReport201(report201 parameters)
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
                            param.name = "anios";
                            foreach (var d in (from p in  adapter.GetDatatable(@"SELECT DISTINCT YEAR([fecha]) anios
                                                FROM [AgronetProyecciones].[dbo].[Algodon_Valores]
                                                ORDER BY YEAR([fecha])").AsEnumerable() select p[@"anios"]))
                            {
                                ParameterData parameter = new ParameterData { name = Convert.ToString(d), value = Convert.ToString(d) };
                                param.data.Add(parameter);
                            }

                            returnData = (Parameter)param;

                            break;
                        case 2:
                            param.name = "regiones";
                            foreach (var d in (from p in adapter.GetDatatable(@"SELECT DISTINCT [region]
                                                                                FROM [AgronetProyecciones].[dbo].[Algodon_Valores]
                                                                                ORDER BY [region]").AsEnumerable()
                                               select p[@"region"]))
                            {
                                ParameterData parameter = new ParameterData { name = Convert.ToString(d), value = Convert.ToString(d) };
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
                            Chart chart = new Chart { subtitle = "", series = new List<Series>() };
                            
                            var serie1 = new Series { name = "Precio Pago Industrial", data = new List<Data>() };
                            var serie2 = new Series { name = "Valor Compensación", data = new List<Data>() };
                            var serie3 = new Series { name = "Precio Mínimo Garantía", data = new List<Data>() };
                            foreach (var anioData in adapter.GetDatatable(String.Format(@"SELECT [fecha], [region], [precioPagIndustrial_PreciosPagInd], ISNULL([ValorCompensacion_ValorCompensacion], 0.00) as ValorCompensacion_ValorCompensacion, [precioMinGarantia_precioMinGarantia]
                                            FROM [AgronetProyecciones].[dbo].[Algodon_Valores]
                                            WHERE [fecha] BETWEEN '{0}' AND '{1}' AND [region] = '{2}'
                                            ORDER BY [fecha], [region];", parameters.anio_inicial, parameters.anio_final, parameters.region)).AsEnumerable())
                            {
                                serie1.data.Add(new Data { name = Convert.ToString(ToUnixTimestamp(Convert.ToDateTime(anioData["fecha"]))), y = Convert.ToDouble(anioData["precioPagIndustrial_PreciosPagInd"]) });
                                serie2.data.Add(new Data { name = Convert.ToString(ToUnixTimestamp(Convert.ToDateTime(anioData["fecha"]))), y = Convert.ToDouble(anioData["ValorCompensacion_ValorCompensacion"]) });
                                serie3.data.Add(new Data { name = Convert.ToString(ToUnixTimestamp(Convert.ToDateTime(anioData["fecha"]))), y = Convert.ToDouble(anioData["precioMinGarantia_precioMinGarantia"]) });
                            }
                            chart.series.Add(serie1);
                            chart.series.Add(serie2);
                            chart.series.Add(serie3);

                            returnData = (Chart)chart;

                            break;
                    }

                    break;
                case "tabla":
                    switch (parameters.id)
                    {
                        case 1:

                            DataTable tableResults = adapter.GetDatatable(String.Format(@"SELECT [fecha], [region], [precioPagIndustrial_PreciosPagInd], [ValorCompensacion_ValorCompensacion], [precioMinGarantia_precioMinGarantia]
                                            FROM [AgronetProyecciones].[dbo].[Algodon_Valores]
                                            WHERE [fecha] BETWEEN '{0}' AND '{1}' AND [region] = '{2}'
                                            ORDER BY [fecha], [region];", parameters.anio_inicial, parameters.anio_final, parameters.region));
                            returnData = (Table) new Table { rows = tableResults };
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

        [Route("api/Report/202")]
        public IHttpActionResult postReport202(report202 parameters)
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
                            param.name = "anios";
                            foreach (var d in (from p in adapter.GetDatatable(@"select distinct year(Proyecciones_PreciosPergamino.fecha_PreciosPergamino) anios
                                                                                from AgronetProyecciones.dbo.Proyecciones_MunicipioRegion
                                                                                inner join AgronetProyecciones.dbo.Proyecciones_PreciosPergamino
                                                                                on Proyecciones_PreciosPergamino.codigoCiudad_PreciosPergamino = Proyecciones_MunicipioRegion.codigoCiudad_MunicipioRegion
                                                                                inner join AgronetProyecciones.dbo.Proyecciones_Regiones
                                                                                on Proyecciones_MunicipioRegion.codigoRegion_MunicipioRegion = Proyecciones_Regiones.codigoRegion_Regiones
                                                                                order by year(Proyecciones_PreciosPergamino.fecha_PreciosPergamino)").AsEnumerable()
                                               select p[@"anios"]))
                            {
                                ParameterData parameter = new ParameterData { name = Convert.ToString(d), value = Convert.ToString(d) };
                                param.data.Add(parameter);
                            }

                            returnData = (Parameter)param;

                            break;
                        case 2:
                            param.name = "regiones";
                            foreach (var d in (from p in adapter.GetDatatable(String.Format(@"select distinct Proyecciones_Regiones.codigoRegion_Regiones, region_Regiones
                                                                                from AgronetProyecciones.dbo.Proyecciones_MunicipioRegion
                                                                                inner join AgronetProyecciones.dbo.Proyecciones_PreciosPergamino
                                                                                on Proyecciones_PreciosPergamino.codigoCiudad_PreciosPergamino = Proyecciones_MunicipioRegion.codigoCiudad_MunicipioRegion
                                                                                inner join AgronetProyecciones.dbo.Proyecciones_Regiones
                                                                                on Proyecciones_MunicipioRegion.codigoRegion_MunicipioRegion = Proyecciones_Regiones.codigoRegion_Regiones
                                                                                where Proyecciones_PreciosPergamino.fecha_PreciosPergamino 
                                                                                between '{0}' and '{1}'
                                                                                order by region_Regiones", parameters.anio_inicial, parameters.anio_final)).AsEnumerable()
                                               select p))
                            {
                                ParameterData parameter = new ParameterData { name = Convert.ToString(d["region_Regiones"]), value = Convert.ToString(d["codigoRegion_Regiones"]) };
                                param.data.Add(parameter);
                            }

                            returnData = (Parameter)param;
                            break;
                        case 3:
                            param.name = "ciudades";
                            foreach (var d in (from p in adapter.GetDatatable(String.Format(@"select distinct codigoCiudad_MunicipioRegion, ciudad_MunicipioRegion
                                                                                    from AgronetProyecciones.dbo.Proyecciones_MunicipioRegion
                                                                                    inner join AgronetProyecciones.dbo.Proyecciones_PreciosPergamino
                                                                                    on Proyecciones_PreciosPergamino.codigoCiudad_PreciosPergamino = Proyecciones_MunicipioRegion.codigoCiudad_MunicipioRegion
                                                                                    inner join AgronetProyecciones.dbo.Proyecciones_Regiones
                                                                                    on Proyecciones_MunicipioRegion.codigoRegion_MunicipioRegion = Proyecciones_Regiones.codigoRegion_Regiones
                                                                                    where Proyecciones_Regiones.codigoRegion_Regiones = {0}
                                                                                    and Proyecciones_PreciosPergamino.fecha_PreciosPergamino 
                                                                                    between '{1}' and '{2}'
                                                                                    order by ciudad_MunicipioRegion;", parameters.region, parameters.anio_inicial, parameters.anio_final)).AsEnumerable()
                                               select p))
                            {
                                ParameterData parameter = new ParameterData { name = Convert.ToString(d["ciudad_MunicipioRegion"]), value = Convert.ToString(d["codigoCiudad_MunicipioRegion"]) };
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
                            Chart chart = new Chart { subtitle = "", series = new List<Series>() };
                            foreach (var productGroup in (from r in adapter.GetDatatable(String.Format(@"select Proyecciones_Regiones.region_Regiones, Proyecciones_MunicipioRegion.ciudad_MunicipioRegion, Proyecciones_PreciosPergamino.fecha_PreciosPergamino, Proyecciones_PreciosPergamino.precio_PreciosPergamino
                                                                                                        from AgronetProyecciones.dbo.Proyecciones_MunicipioRegion
                                                                                                        inner join AgronetProyecciones.dbo.Proyecciones_PreciosPergamino
                                                                                                        on Proyecciones_PreciosPergamino.codigoCiudad_PreciosPergamino = Proyecciones_MunicipioRegion.codigoCiudad_MunicipioRegion
                                                                                                        inner join AgronetProyecciones.dbo.Proyecciones_Regiones
                                                                                                        on Proyecciones_MunicipioRegion.codigoRegion_MunicipioRegion = Proyecciones_Regiones.codigoRegion_Regiones
                                                                                                        where Proyecciones_Regiones.codigoRegion_Regiones = {0}
                                                                                                        and Proyecciones_MunicipioRegion.codigoCiudad_MunicipioRegion IN (" + string.Join(",", parameters.ciudad.Select(d => "'" + d + "'")) + @")
                                                                                                        and Proyecciones_PreciosPergamino.fecha_PreciosPergamino between '{1}' and '{2}'
                                                                                                        order by Proyecciones_Regiones.region_Regiones,
                                                                                                        Proyecciones_MunicipioRegion.ciudad_MunicipioRegion,
                                                                                                        Proyecciones_PreciosPergamino.fecha_PreciosPergamino;", parameters.region, parameters.anio_inicial, parameters.anio_final)).AsEnumerable()
                                                          group r by r["ciudad_MunicipioRegion"]))
                            {
                                var serie = new Series { name = productGroup.Key.ToString().Trim(), data = new List<Data>() };
                                foreach (var anioData in productGroup)
                                {
                                    var data = new Data { name =  Convert.ToString(ToUnixTimestamp(Convert.ToDateTime(anioData["fecha_PreciosPergamino"]))), y = Convert.ToDouble(anioData["precio_PreciosPergamino"]) };
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

                            DataTable tableResults = adapter.GetDatatable(String.Format(@"select Proyecciones_Regiones.region_Regiones, Proyecciones_MunicipioRegion.ciudad_MunicipioRegion, Proyecciones_PreciosPergamino.fecha_PreciosPergamino, Proyecciones_PreciosPergamino.precio_PreciosPergamino
                                                                                                        from AgronetProyecciones.dbo.Proyecciones_MunicipioRegion
                                                                                                        inner join AgronetProyecciones.dbo.Proyecciones_PreciosPergamino
                                                                                                        on Proyecciones_PreciosPergamino.codigoCiudad_PreciosPergamino = Proyecciones_MunicipioRegion.codigoCiudad_MunicipioRegion
                                                                                                        inner join AgronetProyecciones.dbo.Proyecciones_Regiones
                                                                                                        on Proyecciones_MunicipioRegion.codigoRegion_MunicipioRegion = Proyecciones_Regiones.codigoRegion_Regiones
                                                                                                        where Proyecciones_Regiones.codigoRegion_Regiones = {0}
                                                                                                        and Proyecciones_MunicipioRegion.codigoCiudad_MunicipioRegion IN (" + string.Join(",", parameters.ciudad.Select(d => "'" + d + "'")) + @")
                                                                                                        and Proyecciones_PreciosPergamino.fecha_PreciosPergamino between '{1}' and '{2}'
                                                                                                        order by Proyecciones_Regiones.region_Regiones,
                                                                                                        Proyecciones_MunicipioRegion.ciudad_MunicipioRegion,
                                                                                                        Proyecciones_PreciosPergamino.fecha_PreciosPergamino;", parameters.region, parameters.anio_inicial, parameters.anio_final));
                            returnData = (Table)new Table { rows = tableResults };
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
