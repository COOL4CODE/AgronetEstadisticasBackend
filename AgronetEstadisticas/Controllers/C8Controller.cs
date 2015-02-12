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
                            Parameter grupos = new Parameter { name = "niveles", data = new List<ParameterData>() };
                           
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
