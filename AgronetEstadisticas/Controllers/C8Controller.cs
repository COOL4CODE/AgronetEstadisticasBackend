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
    public class C8Controller : ApiController
    {
        [Route("api/Report/801")]
        public IHttpActionResult postReport810(report801 parameters)
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
                            foreach (var d in (from p in adapter.GetDatatable(@"USE [AgronetIndicadores];
                                                                                select 
                                                                                distinct year(dbo.Indicadores_IndicadoresDiarios.fecha_IndicadoresDiarios) as anho
                                                                                from dbo.Indicadores_TipoIndicadoresDiarios
                                                                                inner join dbo.Indicadores_IndicadoresDiarios
                                                                                on dbo.Indicadores_TipoIndicadoresDiarios.codigoTipoInd_TipoIndDiarios =
                                                                                dbo.Indicadores_IndicadoresDiarios.codigoTipoInd_IndicadoresDiarios
                                                                                where dbo.Indicadores_TipoIndicadoresDiarios.codigoTipoInd_TipoIndDiarios = 1
                                                                                order by  year(dbo.Indicadores_IndicadoresDiarios.fecha_IndicadoresDiarios);").AsEnumerable()
                                               select p))
                            {
                                ParameterData parameter = new ParameterData { name = Convert.ToString(d["anho"]), value = Convert.ToString(d["anho"]) };
                                param.data.Add(parameter);
                            }
                            returnData = (Parameter)param;
                            break;
                    }
                    break;
                case "grafico":
                    DataTable results = adapter.GetDatatable(String.Format(@"USE [AgronetIndicadores];
                                                                            select  DATEDIFF(ss, '01/01/1970', [fecha_IndicadoresDiarios]) as fechaunix, [fecha_IndicadoresDiarios], [nombre_TipoIndDiarios], [valorDTF_IndicadoresDiarios]
                                                                            from dbo.Indicadores_TipoIndicadoresDiarios
                                                                            inner join dbo.Indicadores_IndicadoresDiarios on dbo.Indicadores_TipoIndicadoresDiarios.codigoTipoInd_TipoIndDiarios = 
                                                                            dbo.Indicadores_IndicadoresDiarios.codigoTipoInd_IndicadoresDiarios
                                                                            where dbo.Indicadores_IndicadoresDiarios.fecha_IndicadoresDiarios
                                                                            between '{0}' and '{1}'
                                                                            and dbo.Indicadores_TipoIndicadoresDiarios.codigoTipoInd_TipoIndDiarios = 1
                                                                            order by fecha_IndicadoresDiarios", parameters.anio_inicial, parameters.anio_final));
                    Chart chart = new Chart { series = new List<Series>() };
                    switch (parameters.id)
                    {
                        case 1:
                            chart.subtitle = "";
                            
                            var serie = new Series { name = "DTF", data = new List<Data>() };
                            foreach (var d in  (from r in results.AsEnumerable()
                                                       select r))
                            {
                                var data = new Data { name = Convert.ToString(d["fechaunix"]), y = Convert.ToDouble(d["valorDTF_IndicadoresDiarios"]) };
                                serie.data.Add(data);

                            }
                            chart.series.Add(serie);

                            returnData = (Chart)chart;
                            break;
                    }
                    break;
                case "tabla":
                    switch (parameters.id)
                    {
                        case 1:
                            DataTable tableResults = adapter.GetDatatable(String.Format(@"USE [AgronetIndicadores];
                                                                            select [fecha_IndicadoresDiarios], [nombre_TipoIndDiarios], [valorDTF_IndicadoresDiarios]
                                                                            from dbo.Indicadores_TipoIndicadoresDiarios
                                                                            inner join dbo.Indicadores_IndicadoresDiarios on dbo.Indicadores_TipoIndicadoresDiarios.codigoTipoInd_TipoIndDiarios = 
                                                                            dbo.Indicadores_IndicadoresDiarios.codigoTipoInd_IndicadoresDiarios
                                                                            where dbo.Indicadores_IndicadoresDiarios.fecha_IndicadoresDiarios
                                                                            between '{0}' and '{1}'
                                                                            and dbo.Indicadores_TipoIndicadoresDiarios.codigoTipoInd_TipoIndDiarios = 1
                                                                            order by fecha_IndicadoresDiarios", parameters.anio_inicial, parameters.anio_final));
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

        [Route("api/Report/802")]
        public IHttpActionResult postReport810(report802 parameters)
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
                            foreach (var d in (from p in adapter.GetDatatable(@"USE [AgronetIndicadores];
                                                                                select 
                                                                                distinct year(dbo.Indicadores_IndicadoresDiarios.fecha_IndicadoresDiarios) as anho
                                                                                from dbo.Indicadores_TipoIndicadoresDiarios
                                                                                inner join dbo.Indicadores_IndicadoresDiarios
                                                                                on dbo.Indicadores_TipoIndicadoresDiarios.codigoTipoInd_TipoIndDiarios =
                                                                                dbo.Indicadores_IndicadoresDiarios.codigoTipoInd_IndicadoresDiarios
                                                                                where dbo.Indicadores_TipoIndicadoresDiarios.codigoTipoInd_TipoIndDiarios = 2
                                                                                order by  year(dbo.Indicadores_IndicadoresDiarios.fecha_IndicadoresDiarios);").AsEnumerable()
                                               select p))
                            {
                                ParameterData parameter = new ParameterData { name = Convert.ToString(d["anho"]), value = Convert.ToString(d["anho"]) };
                                param.data.Add(parameter);
                            }
                            returnData = (Parameter)param;
                            break;
                    }
                    break;
                case "grafico":
                    DataTable results = adapter.GetDatatable(String.Format(@"USE [AgronetIndicadores];
                                                                            select  DATEDIFF(ss, '01/01/1970', [fecha_IndicadoresDiarios]) as fechaunix, [fecha_IndicadoresDiarios], [nombre_TipoIndDiarios], [valorDTF_IndicadoresDiarios]
                                                                            from dbo.Indicadores_TipoIndicadoresDiarios
                                                                            inner join dbo.Indicadores_IndicadoresDiarios on dbo.Indicadores_TipoIndicadoresDiarios.codigoTipoInd_TipoIndDiarios = 
                                                                            dbo.Indicadores_IndicadoresDiarios.codigoTipoInd_IndicadoresDiarios
                                                                            where dbo.Indicadores_IndicadoresDiarios.fecha_IndicadoresDiarios
                                                                            between '{0}' and '{1}'
                                                                            and dbo.Indicadores_TipoIndicadoresDiarios.codigoTipoInd_TipoIndDiarios = 2
                                                                            order by fecha_IndicadoresDiarios", parameters.anio_inicial, parameters.anio_final));
                    Chart chart = new Chart { series = new List<Series>() };
                    switch (parameters.id)
                    {
                        case 1:
                            chart.subtitle = "";

                            var serie = new Series { name = "Tasa interbancaria", data = new List<Data>() };
                            foreach (var d in (from r in results.AsEnumerable()
                                               select r))
                            {
                                //var data = new Data { name = Convert.ToString(d["fechaunix"]), y = Convert.ToDouble(d["valorDTF_IndicadoresDiarios"]) };
                                var data = new Data { name = Convert.ToString(ToUnixTimestamp(Convert.ToDateTime(d["fecha_IndicadoresDiarios"]))), y = Convert.ToDouble(d["valorDTF_IndicadoresDiarios"]) };
                                serie.data.Add(data);

                            }
                            chart.series.Add(serie);

                            returnData = (Chart)chart;
                            break;
                    }
                    break;
                case "tabla":
                    switch (parameters.id)
                    {
                        case 1:
                            DataTable tableResults = adapter.GetDatatable(String.Format(@"USE [AgronetIndicadores];
                                                                            select [fecha_IndicadoresDiarios], [nombre_TipoIndDiarios], [valorDTF_IndicadoresDiarios]
                                                                            from dbo.Indicadores_TipoIndicadoresDiarios
                                                                            inner join dbo.Indicadores_IndicadoresDiarios on dbo.Indicadores_TipoIndicadoresDiarios.codigoTipoInd_TipoIndDiarios = 
                                                                            dbo.Indicadores_IndicadoresDiarios.codigoTipoInd_IndicadoresDiarios
                                                                            where dbo.Indicadores_IndicadoresDiarios.fecha_IndicadoresDiarios
                                                                            between '{0}' and '{1}'
                                                                            and dbo.Indicadores_TipoIndicadoresDiarios.codigoTipoInd_TipoIndDiarios = 2
                                                                            order by fecha_IndicadoresDiarios", parameters.anio_inicial, parameters.anio_final));
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

        [Route("api/Report/803")]
        public IHttpActionResult postReport803(report803 parameters)
        {
            Object returnData = null;
            SQLAdapter adapter = new SQLAdapter();
            switch (parameters.tipo)
            {
                case "parametro":
                    switch (parameters.id)
                    {
                        case 1:
                            String sql1 = "SELECT DISTINCT anho_PIBOfertaSinIlicitos AS anios FROM AgronetIndicadores.dbo.Indicadores_PIBOfertaSinIlicitos ORDER BY anho_PIBOfertaSinIlicitos;";
                            DataTable data = adapter.GetDatatable(sql1);
                            Parameter param = new Parameter { name = "anios" , data = new List<ParameterData>() };
                            foreach (var d in (from p in data.AsEnumerable() select p[@"anios"])){
                                ParameterData parameter = new ParameterData { name = Convert.ToString(d), value = Convert.ToString(d) };
                                param.data.Add(parameter);
                            }

                            returnData = (Parameter)param;
                            break;
                    }
                    break;
                case "tabla":
                    switch (parameters.id)
                    {
                        case 1:
                            String sql1 = String.Format(@"USE [AgronetIndicadores]
                                            EXEC	[dbo].[SP_PIB_SIN_ILICITOS_PP_ABSOLUTO]
		                                            @FECHA_INICIAL = {0},
		                                            @FECHA_FINAL = {1}", parameters.anio_inicial, parameters.anio_final);

                            DataTable tableResults = adapter.GetDatatable(sql1);
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

        [Route("api/Report/804")]
        public IHttpActionResult postReport804(report804 parameters)
        {
            Object returnData = null;
            SQLAdapter adapter = new SQLAdapter();
            switch (parameters.tipo)
            {
                case "parametro":
                    switch (parameters.id)
                    {
                        case 1:
                            String sql1 = "SELECT DISTINCT anho_PIBOfertaSinIlicitos AS anios FROM AgronetIndicadores.dbo.Indicadores_PIBOfertaSinIlicitos ORDER BY anho_PIBOfertaSinIlicitos;";
                            DataTable data = adapter.GetDatatable(sql1);
                            Parameter param = new Parameter { name = "anios", data = new List<ParameterData>() };
                            foreach (var d in (from p in data.AsEnumerable() select p[@"anios"]))
                            {
                                ParameterData parameter = new ParameterData { name = Convert.ToString(d), value = Convert.ToString(d) };
                                param.data.Add(parameter);
                            }

                            returnData = (Parameter)param;
                            break;
                    }
                    break;
                case "tabla":
                    switch (parameters.id)
                    {
                        case 1:
                            String sql1 = String.Format(@"USE [AgronetIndicadores]
                                            EXEC	[dbo].[SP_PIB_SIN_ILICITOS_TRI_PP]
		                                            @FECHA_INICIAL = {0},
		                                            @FECHA_FINAL = {1}", parameters.anio_inicial, parameters.anio_final);

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

        [Route("api/Report/805")]
        public IHttpActionResult postReport805(report805 parameters)
        {
            Object returnData = null;
            SQLAdapter adapter = new SQLAdapter();
            switch (parameters.tipo)
            {
                case "parametro":
                    switch (parameters.id)
                    {
                        case 1:
                            String sql1 = @"SELECT DISTINCT anho_PIBDepartamental AS anios FROM AgronetIndicadores.dbo.Indicadores_PIBDepartamental ORDER BY anho_PIBDepartamental;";
                            DataTable data = adapter.GetDatatable(sql1);
                            Parameter param = new Parameter { name = "anios", data = new List<ParameterData>() };
                            foreach (var d in (from p in data.AsEnumerable() select p[@"anios"]))
                            {
                                ParameterData parameter = new ParameterData { name = Convert.ToString(d), value = Convert.ToString(d) };
                                param.data.Add(parameter);
                            }

                            returnData = (Parameter)param;
                            break;
                        case 2:
                            Parameter regiones = new Parameter { name = "regiones", data = new List<ParameterData>() };
                           
                            regiones.data.Add(new ParameterData { name = "Región Amazonía", value = "5" });
                            regiones.data.Add(new ParameterData { name = "Región Andina", value = "2" });
                            regiones.data.Add(new ParameterData { name = "Región Caribe", value = "1" });
                            regiones.data.Add(new ParameterData { name = "Región Insular", value = "6" });
                            regiones.data.Add(new ParameterData { name = "Región Orinoquía", value = "4" });
                            regiones.data.Add(new ParameterData { name = "Región Pacífico", value = "3" });
                            
                            returnData = (Parameter)regiones;
                            break;
                        case 3:
                            String sql2 = @"SELECT codigoSubrama_PIBSubramas, nombreSubrama_PIBSubramas from AgronetIndicadores.dbo.Indicadores_PIBSubramas where codigoRama_PIBSubramas=1 order by nombreSubrama_PIBSubramas;";
                            DataTable data2 = adapter.GetDatatable(sql2);
                            Parameter param2 = new Parameter { name = "subramas", data = new List<ParameterData>() };
                            foreach (var d in (from p in data2.AsEnumerable() select p))
                            {
                                ParameterData parameter = new ParameterData { name = Convert.ToString(d["nombreSubrama_PIBSubramas"]), value = Convert.ToString(d["codigoSubrama_PIBSubramas"]) };
                                param2.data.Add(parameter);
                            }

                            returnData = (Parameter)param2;
                            break;
                    }
                    break;
                case "tabla":
                    switch (parameters.id)
                    {
                        case 1:
                            String sql1 = String.Format(@"USE [AgronetIndicadores]
                                            EXEC	[dbo].[SP_PIB_POR_REGION_Y_DEPARTAMENTO]
		                                            @SUBRAMA = {0},
		                                            @REGION = {1},
		                                            @FECHA = {2}", parameters.subrama, parameters.region, parameters.anio);

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

        [Route("api/Report/806")]
        public IHttpActionResult postReport806(report806 parameters)
        {
            Object returnData = null;
            SQLAdapter adapter = new SQLAdapter();
            switch (parameters.tipo)
            {
                case "parametro":
                    switch (parameters.id)
                    {
                        case 1:
                            String sql1 = @"SELECT DISTINCT anho_PIBOfertaSinIlicitos AS anios FROM AgronetIndicadores.dbo.Indicadores_PIBOfertaSinIlicitos ORDER BY anho_PIBOfertaSinIlicitos;";
                            DataTable data = adapter.GetDatatable(sql1);
                            Parameter param = new Parameter { name = "anios", data = new List<ParameterData>() };
                            foreach (var d in (from p in data.AsEnumerable() select p[@"anios"]))
                            {
                                ParameterData parameter = new ParameterData { name = Convert.ToString(d), value = Convert.ToString(d) };
                                param.data.Add(parameter);
                            }

                            returnData = (Parameter)param;
                            break;
                    }
                    break;
                case "tabla":
                    switch (parameters.id)
                    {
                        case 1:
                            String sql1 = String.Format(@"USE [AgronetIndicadores]
                                                          EXEC	[dbo].[SP_PIB_SIN_ILICITOS_PP_PARTICIPACION]
		                                                        @FECHA_INICIAL = {0},
		                                                        @FECHA_FINAL = {1}", parameters.anio_inicial, parameters.anio_final);

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

        [Route("api/Report/807")]
        public IHttpActionResult postReport807(report807 parameters)
        {
            Object returnData = null;
            SQLAdapter adapter = new SQLAdapter();
            switch (parameters.tipo)
            {
                case "parametro":
                    switch (parameters.id)
                    {
                        case 1:
                            String sql1 = @"SELECT  DISTINCT anho_PIBOfertaSinIlicitos AS anios FROM  AgronetIndicadores.dbo.Indicadores_PIBOfertaSinIlicitos ORDER BY anho_PIBOfertaSinIlicitos;";
                            DataTable data = adapter.GetDatatable(sql1);
                            Parameter param = new Parameter { name = "anios", data = new List<ParameterData>() };
                            foreach (var d in (from p in data.AsEnumerable() select p[@"anios"]))
                            {
                                ParameterData parameter = new ParameterData { name = Convert.ToString(d), value = Convert.ToString(d) };
                                param.data.Add(parameter);
                            }

                            returnData = (Parameter)param;
                            break;
                    }
                    break;
                case "tabla":
                    switch (parameters.id)
                    {
                        case 1:
                            String sql1 = String.Format(@"USE [AgronetIndicadores]
                                                        EXEC	[dbo].[SP_PIB_SIN_ILICITOS_TRI_RAMAS]
		                                                        @FECHA_INICIAL = {0},
		                                                        @FECHA_FINAL = {1}", parameters.anio_inicial, parameters.anio_final);

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

        [Route("api/Report/808")]
        public IHttpActionResult postReport808(report808 parameters)
        {
            Object returnData = null;
            SQLAdapter adapter = new SQLAdapter();
            switch (parameters.tipo)
            {
                case "parametro":
                    
                    switch (parameters.id)
                    {
                        case 1:
                            Parameter niveles = new Parameter { name = "niveles", data = new List<ParameterData>() };
                           
                            niveles.data.Add(new ParameterData { name = "Grupo", value = "1" });
                            niveles.data.Add(new ParameterData { name = "Subgrupo", value = "2" });
                            niveles.data.Add(new ParameterData { name = "Clase de gasto", value = "3" });
                            
                            returnData = (Parameter)niveles;
                            break;
                        case 2:
                            Parameter grupos = new Parameter { name = "Grupos", data = new List<ParameterData>() };
                           
                            grupos.data.Add(new ParameterData { name = "Total", value = "10" });
                            grupos.data.Add(new ParameterData { name = "Alimentos", value = "1" });
                            grupos.data.Add(new ParameterData { name = "Vivienda", value = "2" });
                            grupos.data.Add(new ParameterData { name = "Vestuario", value = "3" });
                            grupos.data.Add(new ParameterData { name = "Salud", value = "4" });
                            grupos.data.Add(new ParameterData { name = "Educación", value = "5" });
                            grupos.data.Add(new ParameterData { name = "Diversión", value = "6" });
                            grupos.data.Add(new ParameterData { name = "Transporte", value = "7" });
                            grupos.data.Add(new ParameterData { name = "Comunicaciones", value = "8" });
                            grupos.data.Add(new ParameterData { name = "Otros Gastos", value = "9" });
                            
                            returnData = (Parameter)grupos;
                            break;
                        case 3:
                            Parameter tipos_variaciones = new Parameter { name = "tipos_variaciones", data = new List<ParameterData>() };

                            tipos_variaciones.data.Add(new ParameterData { name = "Mensual", value = "1" });
                            tipos_variaciones.data.Add(new ParameterData { name = "Anual", value = "2" });
                            tipos_variaciones.data.Add(new ParameterData { name = "Año Corrido", value = "3" });

                            returnData = (Parameter)tipos_variaciones;
                            break;
                        case 4:
                            Parameter param = new Parameter { name = "anios", data = new List<ParameterData>() };

                            foreach (var d in (from p in adapter.GetDatatable(@"USE [AgronetIndicadores];
                                                                                SELECT DISTINCT YEAR([fecha_ipcNacional]) as anho FROM [AgronetIndicadores].[dbo].[Indicadores_IPCNacional]
                                                                                WHERE YEAR([fecha_ipcNacional]) >= '2009'
                                                                                ORDER BY YEAR([fecha_ipcNacional]);").AsEnumerable()
                                               select p))
                            {
                                ParameterData parameter = new ParameterData { name = Convert.ToString(d["anho"]), value = Convert.ToString(d["anho"]) };
                                param.data.Add(parameter);
                            }
                            returnData = (Parameter)param;

                            break;
                    }
                    break;
                case "grafico":
                    DataTable results = adapter.GetDatatable(String.Format(@"USE [AgronetIndicadores]
                                                                            EXEC	[dbo].[SP_IPC_NACIONAL]
		                                                                            @Fecha_inicial = N'{0}-01-01',
		                                                                            @Fecha_final = N'{1}-12-31',
		                                                                            @Nivel = {2},
		                                                                            @Sector = {3},
		                                                                            @TipoVariacion = {4}", parameters.fecha_inicial, parameters.fecha_final, parameters.nivel, parameters.grupo, parameters.tipo_variacion));
                    Chart chart = new Chart { series = new List<Series>() };

                    switch (parameters.id)
                    {
                        case 1:
                            chart.subtitle = "";
                            foreach (var descGroup in (from r in results.AsEnumerable()
                                                          group r by r["descripcionGrupo"]))
                            {
                                var serie = new Series { name = descGroup.Key.ToString().Trim(), data = new List<Data>() };
                                foreach (var anioData in descGroup)
                                {
                                    var data = new Data { name = Convert.ToString(ToUnixTimestamp(Convert.ToDateTime(anioData["fecha"]))), y = Convert.ToDouble(anioData["ipc"]) };
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
                            DataTable tableResults = adapter.GetDatatable(String.Format(@"USE [AgronetIndicadores]
                                                                            EXEC	[dbo].[SP_IPC_NACIONAL]
		                                                                            @Fecha_inicial = N'{0}-01-01',
		                                                                            @Fecha_final = N'{1}-12-31',
		                                                                            @Nivel = {2},
		                                                                            @Sector = {3},
		                                                                            @TipoVariacion = {4}", parameters.fecha_inicial, parameters.fecha_final, parameters.nivel, parameters.grupo, parameters.tipo_variacion));
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

        [Route("api/Report/809")]
        public IHttpActionResult postReport809(report809 parameters)
        {
            Object returnData = null;
            SQLAdapter adapter = new SQLAdapter();
            switch (parameters.tipo)
            {
                case "parametro":
                    switch (parameters.id)
                    {
                        case 1:
                            Parameter param1 = new Parameter { name = "anios", data = new List<ParameterData>() };

                            foreach (var d in (from p in adapter.GetDatatable(@"USE [AgronetIndicadores];
                                                                                SELECT DISTINCT YEAR(fecha_IPPExpo) anho FROM Indicadores_IPPExpo
                                                                                ORDER BY YEAR(fecha_IPPExpo);").AsEnumerable()
                                               select p))
                            {
                                ParameterData parameter = new ParameterData { name = Convert.ToString(d["anho"]), value = Convert.ToString(d["anho"]) };
                                param1.data.Add(parameter);
                            }
                            returnData = (Parameter)param1;
                            break;
                        case 2:
                            Parameter param2 = new Parameter { name = "grupo", data = new List<ParameterData>() };

                            foreach (var d in (from p in adapter.GetDatatable(@"USE [AgronetIndicadores];
SELECT codigoGrupo_GruposIPP, descripcionGrupo_GruposIPP from Indicadores_GruposIPP order by descripcionGrupo_GruposIPP;").AsEnumerable()
                                               select p))
                            {
                                ParameterData parameter = new ParameterData { name = Convert.ToString(d["descripcionGrupo_GruposIPP"]), value = Convert.ToString(d["codigoGrupo_GruposIPP"]) };
                                param2.data.Add(parameter);
                            }
                            returnData = (Parameter)param2;
                            break;
                        case 3:
                            Parameter categorias = new Parameter { name = "categorias", data = new List<ParameterData>() };

                            categorias.data.Add(new ParameterData { name = "Exportados", value = "1" });
                            categorias.data.Add(new ParameterData { name = "Importados", value = "2" });
                            categorias.data.Add(new ParameterData { name = "Consumidos", value = "3" });
                            categorias.data.Add(new ParameterData { name = "Total nacional", value = "4" });

                            returnData = (Parameter)categorias;
                            break;
                    }
                    break;
                case "grafico":
                    DataTable results = adapter.GetDatatable(String.Format(@"USE [AgronetIndicadores];
                                                                            EXEC	[dbo].[SP_IPP]
		                                                                            @FECHA_INICIAL = N'{0}',
		                                                                            @FECHA_FINAL = N'{1}',
		                                                                            @PARAMETRO = {2},
		                                                                            @GRUPO = {3}", parameters.fecha_inicial, parameters.fecha_final, parameters.categoria, parameters.grupo));
                    Chart chart = new Chart { series = new List<Series>() };

                    switch (parameters.id)
                    {
                        case 1:
                            chart.subtitle = "";
                            
                            var serie = new Series { name = "", data = new List<Data>() };
                            foreach (var anioData in (from r in results.AsEnumerable()
                                                          select r))
                            {
                                var data = new Data { name = Convert.ToString(ToUnixTimestamp(Convert.ToDateTime(anioData["fecha"]))), y = Convert.ToDouble(anioData["valor"]) };
                                serie.data.Add(data);

                            }
                            chart.series.Add(serie);
                            
                            returnData = (Chart)chart;
                            break;
                    }
                    break;
                case "tabla":
                    switch (parameters.id)
                    {
                        case 1:
                            DataTable tableResults = adapter.GetDatatable(String.Format(@"USE [AgronetIndicadores];
                                                                            EXEC	[dbo].[SP_IPP]
		                                                                            @FECHA_INICIAL = N'{0}',
		                                                                            @FECHA_FINAL = N'{1}',
		                                                                            @PARAMETRO = {2},
		                                                                            @GRUPO = {3}", parameters.fecha_inicial, parameters.fecha_final, parameters.categoria, parameters.grupo));
                    
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

        [Route("api/Report/810")]
        public IHttpActionResult postReport810(report810 parameters)
        {
            Object returnData = null;
            SQLAdapter adapter = new SQLAdapter();
            switch (parameters.tipo)
            {
                case "parametro":
                    switch (parameters.id)
                    {
                        case 1:
                            Parameter param1 = new Parameter { name = "anios", data = new List<ParameterData>() };

                            foreach (var d in (from p in adapter.GetDatatable(@"USE AgronetIndicadores;
                                                                                SELECT DISTINCT [anho_EmpleoRuralTrim] anho FROM Indicadores_EmpleoRuralTrimestral
                                                                                ORDER BY [anho_EmpleoRuralTrim];").AsEnumerable()
                                               select p))
                            {
                                ParameterData parameter = new ParameterData { name = Convert.ToString(d["anho"]), value = Convert.ToString(d["anho"]) };
                                param1.data.Add(parameter);
                            }
                            returnData = (Parameter)param1;
                            break;
                    }
                    break;
                case "grafico":

                    DataTable results = adapter.GetDatatable(String.Format(@"USE [AgronetIndicadores]
                                                                            CREATE TABLE #SP_EMPLEO_RURAL_TRIMESTRAL (
	                                                                            ahno date,
	                                                                            trimestre int,
	                                                                            codVariable int,
	                                                                            codZona int,
	                                                                            valor float
                                                                            )
                                                                            INSERT INTO #SP_EMPLEO_RURAL_TRIMESTRAL EXEC [dbo].[SP_EMPLEO_RURAL_TRIMESTRAL]
		                                                                            @Anho_inicial = {0},
		                                                                            @Anho_final = {1}

                                                                            SELECT YEAR(#SP_EMPLEO_RURAL_TRIMESTRAL.ahno) anio,
                                                                            #SP_EMPLEO_RURAL_TRIMESTRAL.trimestre, 
                                                                            Indicadores_EmpleoRuralZonas.descripcion_Zona, 
                                                                            Indicadores_EmpleoRuralVariables.descripcion_Variable,
                                                                            #SP_EMPLEO_RURAL_TRIMESTRAL.valor,
                                                                            Indicadores_EmpleoRuralVariables.unidad_Variable
                                                                            FROM   AgronetIndicadores.dbo.Indicadores_EmpleoRuralVariables Indicadores_EmpleoRuralVariables
                                                                            INNER JOIN #SP_EMPLEO_RURAL_TRIMESTRAL ON Indicadores_EmpleoRuralVariables.codigo_Variable = #SP_EMPLEO_RURAL_TRIMESTRAL.codVariable
                                                                            INNER JOIN AgronetIndicadores.dbo.Indicadores_EmpleoRuralZonas Indicadores_EmpleoRuralZonas ON  Indicadores_EmpleoRuralZonas.codigo_Zona = #SP_EMPLEO_RURAL_TRIMESTRAL.codZona
                                                                            ORDER BY Indicadores_EmpleoRuralVariables.descripcion_Variable, Indicadores_EmpleoRuralZonas.descripcion_Zona, YEAR(#SP_EMPLEO_RURAL_TRIMESTRAL.ahno), #SP_EMPLEO_RURAL_TRIMESTRAL.trimestre

                                                                            DROP TABLE #SP_EMPLEO_RURAL_TRIMESTRAL;", parameters.anio_inicial, parameters.anio_final));

                    var queryCharts = from r in results.AsEnumerable()
                                      group r by r["descripcion_Variable"] into chartGroup
                                from seriesGroup in
                                    (from r in chartGroup
                                     group r by r["descripcion_Zona"])
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
                                    var data = new Data { name = Convert.ToString(element["anio"] + " Trim " + element["trimestre"]), y = Convert.ToDouble(element["valor"]) };
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
                            DataTable tableResults = adapter.GetDatatable(String.Format(@"USE [AgronetIndicadores]
                                                                                        CREATE TABLE #SP_EMPLEO_RURAL_TRIMESTRAL (
	                                                                                        ahno date,
	                                                                                        trimestre int,
	                                                                                        codVariable int,
	                                                                                        codZona int,
	                                                                                        valor float
                                                                                        )
                                                                                        INSERT INTO #SP_EMPLEO_RURAL_TRIMESTRAL EXEC [dbo].[SP_EMPLEO_RURAL_TRIMESTRAL]
		                                                                                        @Anho_inicial = {0},
		                                                                                        @Anho_final = {1}

                                                                                        SELECT YEAR(#SP_EMPLEO_RURAL_TRIMESTRAL.ahno) anio,
                                                                                        #SP_EMPLEO_RURAL_TRIMESTRAL.trimestre, 
                                                                                        Indicadores_EmpleoRuralZonas.descripcion_Zona, 
                                                                                        Indicadores_EmpleoRuralVariables.descripcion_Variable,
                                                                                        #SP_EMPLEO_RURAL_TRIMESTRAL.valor,
                                                                                        Indicadores_EmpleoRuralVariables.unidad_Variable
                                                                                        FROM   AgronetIndicadores.dbo.Indicadores_EmpleoRuralVariables Indicadores_EmpleoRuralVariables
                                                                                        INNER JOIN #SP_EMPLEO_RURAL_TRIMESTRAL ON Indicadores_EmpleoRuralVariables.codigo_Variable = #SP_EMPLEO_RURAL_TRIMESTRAL.codVariable
                                                                                        INNER JOIN AgronetIndicadores.dbo.Indicadores_EmpleoRuralZonas Indicadores_EmpleoRuralZonas ON  Indicadores_EmpleoRuralZonas.codigo_Zona = #SP_EMPLEO_RURAL_TRIMESTRAL.codZona
                                                                                        ORDER BY Indicadores_EmpleoRuralVariables.descripcion_Variable, Indicadores_EmpleoRuralZonas.descripcion_Zona, YEAR(#SP_EMPLEO_RURAL_TRIMESTRAL.ahno), #SP_EMPLEO_RURAL_TRIMESTRAL.trimestre

                                                                                        DROP TABLE #SP_EMPLEO_RURAL_TRIMESTRAL;", parameters.anio_inicial, parameters.anio_final));

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

        [Route("api/Report/811")]
        public IHttpActionResult postReport810(report811 parameters)
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
                            foreach (var d in (from p in adapter.GetDatatable(@"USE [AgronetIndicadores];
                                                                                select 
                                                                                distinct year(dbo.Indicadores_IndicadoresDiarios.fecha_IndicadoresDiarios) as anho
                                                                                from dbo.Indicadores_TipoIndicadoresDiarios
                                                                                inner join dbo.Indicadores_IndicadoresDiarios
                                                                                on dbo.Indicadores_TipoIndicadoresDiarios.codigoTipoInd_TipoIndDiarios =
                                                                                dbo.Indicadores_IndicadoresDiarios.codigoTipoInd_IndicadoresDiarios
                                                                                where dbo.Indicadores_TipoIndicadoresDiarios.codigoTipoInd_TipoIndDiarios = 3
                                                                                order by  year(dbo.Indicadores_IndicadoresDiarios.fecha_IndicadoresDiarios);").AsEnumerable()
                                               select p))
                            {
                                ParameterData parameter = new ParameterData { name = Convert.ToString(d["anho"]), value = Convert.ToString(d["anho"]) };
                                param.data.Add(parameter);
                            }
                            returnData = (Parameter)param;
                            break;
                    }
                    break;
                case "grafico":
                    DataTable results = adapter.GetDatatable(String.Format(@"USE [AgronetIndicadores];
                                                                            select  DATEDIFF(ss, '01/01/1970', [fecha_IndicadoresDiarios]) as fechaunix, [fecha_IndicadoresDiarios], [nombre_TipoIndDiarios], [valorDTF_IndicadoresDiarios]
                                                                            from dbo.Indicadores_TipoIndicadoresDiarios
                                                                            inner join dbo.Indicadores_IndicadoresDiarios on dbo.Indicadores_TipoIndicadoresDiarios.codigoTipoInd_TipoIndDiarios = 
                                                                            dbo.Indicadores_IndicadoresDiarios.codigoTipoInd_IndicadoresDiarios
                                                                            where dbo.Indicadores_IndicadoresDiarios.fecha_IndicadoresDiarios
                                                                            between '{0}' and '{1}'
                                                                            and dbo.Indicadores_TipoIndicadoresDiarios.codigoTipoInd_TipoIndDiarios = 3
                                                                            order by fecha_IndicadoresDiarios", parameters.anio_inicial, parameters.anio_final));
                    Chart chart = new Chart { series = new List<Series>() };
                    switch (parameters.id)
                    {
                        case 1:
                            chart.subtitle = "";

                            var serie = new Series { name = "Tasa interés activa", data = new List<Data>() };
                            foreach (var d in (from r in results.AsEnumerable()
                                               select r))
                            {
                                var data = new Data { name = Convert.ToString(d["fechaunix"]), y = Convert.ToDouble(d["valorDTF_IndicadoresDiarios"]) };
                                serie.data.Add(data);

                            }
                            chart.series.Add(serie);

                            returnData = (Chart)chart;
                            break;
                    }
                    break;
                case "tabla":
                    switch (parameters.id)
                    {
                        case 1:
                            DataTable tableResults = adapter.GetDatatable(String.Format(@"USE [AgronetIndicadores];
                                                                            select [fecha_IndicadoresDiarios], [nombre_TipoIndDiarios], [valorDTF_IndicadoresDiarios]
                                                                            from dbo.Indicadores_TipoIndicadoresDiarios
                                                                            inner join dbo.Indicadores_IndicadoresDiarios on dbo.Indicadores_TipoIndicadoresDiarios.codigoTipoInd_TipoIndDiarios = 
                                                                            dbo.Indicadores_IndicadoresDiarios.codigoTipoInd_IndicadoresDiarios
                                                                            where dbo.Indicadores_IndicadoresDiarios.fecha_IndicadoresDiarios
                                                                            between '{0}' and '{1}'
                                                                            and dbo.Indicadores_TipoIndicadoresDiarios.codigoTipoInd_TipoIndDiarios = 3
                                                                            order by fecha_IndicadoresDiarios", parameters.anio_inicial, parameters.anio_final));
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
