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
    public class C4Controller : ApiController
    {

        [Route("api/Report/401")]
        public async Task<IHttpActionResult> postReport401(report401 parameters)
        {
            Object returnData = null;
            var adapter = new SQLAnalysisAdaper();
            string connectionName = "AgronetSQLAnalysisServicesComercio";
            
            List<MdxParameter> mdxParams = new List<MdxParameter>();
            mdxParams.Add(new MdxParameter("~[Measures].[Ton Netas Expo]", "volumen"));
            mdxParams.Add(new MdxParameter("~[Measures].[Valor Expo Miles FOB Dol]", "valor"));


            if (parameters.tipo == "parametro")
            {
                Parameter parameter = new Parameter { data = new List<ParameterData>() };

                switch (parameters.id)
                {
                    case 1:
                        parameter.name = "cadena";
                        mdxParams.Add(new MdxParameter("@cadena", "[Producto].[Cadena].[Cadena]")); 
                        mdxParams.Add(new MdxParameter("~[Producto].[Cadena].[Cadena]", "cadena"));
                        string mdx1 = @"SELECT 
                            NON EMPTY {[Measures].[Ton Netas Expo],[Measures].[Valor Expo Miles FOB Dol]} ON 0, 
                            NON EMPTY (@cadena) ON 1
                            FROM [Agronet Comercio];";

                        DataTable data1 = adapter.GetDataTable(connectionName, mdx1, mdxParams);
                        foreach (var p in (from p in data1.AsEnumerable()
                                           select p["cadena"]))
                        {
                            ParameterData param = new ParameterData { name = (string)p, value = (string)p };
                            parameter.data.Add(param);
                        }
                        break;
                    case 2:
                        parameter.name = "partida";
                        mdxParams.Add(new MdxParameter("@partidas", "[Producto].[Cadena-Partida10].[Descripcion Partida10 Dig Union]"));
                        mdxParams.Add(new MdxParameter("~[Producto].[Cadena-Partida10].[Descripcion Partida10 Dig Union]", "partidas"));
                        mdxParams.Add(new MdxParameter("@cadena", String.Format("[Producto].[Cadena].&[{0}]", parameters.cadena))); 
                        string mdx3 = @"SELECT 
                            NON EMPTY {[Measures].[Ton Netas Expo],[Measures].[Valor Expo Miles FOB Dol]} ON 0, 
                            NON EMPTY (@partidas) ON 1
                            FROM [Agronet Comercio]
                            WHERE (@cadena);";

                        DataTable data3 = adapter.GetDataTable(connectionName, mdx3, mdxParams);
                        foreach (var p in (from p in data3.AsEnumerable()
                                           select p["partidas"]))
                        {
                            ParameterData param = new ParameterData { name = (string)p, value = (string)p };
                            parameter.data.Add(param);
                        }
                        break;
                    case 3:
                        parameter.name = "anio";
                        mdxParams.Add(new MdxParameter("@anio", "[Periodo].[anho].[anho]"));
                        mdxParams.Add(new MdxParameter("~[Periodo].[anho].[anho]", "anio"));
                        string mdx2 = @"SELECT 
                            NON EMPTY {[Measures].[Ton Netas Expo],[Measures].[Valor Expo Miles FOB Dol]} ON 0, 
                            NON EMPTY (@anio) ON 1
                            FROM [Agronet Comercio];";

                        DataTable data2 = adapter.GetDataTable(connectionName, mdx2, mdxParams);
                        foreach (var p in (from p in data2.AsEnumerable()
                                           select p["anio"]))
                        {
                            ParameterData param = new ParameterData { name = (string)p, value = (string)p };
                            parameter.data.Add(param);
                        }
                        break;
                }
                returnData = (Parameter)parameter;
            }
            else if (parameters.tipo == "grafico")
            {

            }
            else if (parameters.tipo == "tabla")
            {
                Table table = new Table { columns = new List<Column>(), rows = new DataTable() };
                mdxParams.Add(new MdxParameter("@cadena", String.Format("[Producto].[Cadena].&[{0}]", parameters.cadena)));
                mdxParams.Add(new MdxParameter("~[Producto].[Cadena].[Cadena]", "cadena"));
                mdxParams.Add(new MdxParameter("~[Producto].[Cadena-Partida10].[Descripcion Partida10 Dig Union]", "partidas"));
                mdxParams.Add(new MdxParameter("@anio", String.Format("[Periodo].[anho].&[{0}]:[Periodo].[anho].&[{1}]", parameters.anio_inicial, parameters.anio_final)));
                mdxParams.Add(new MdxParameter("~[Periodo].[anho].[anho]", "anio"));
                mdxParams.Add(new MdxParameter("~[Pais].[Pais].[Pais]", "pais"));
                mdxParams.Add(new MdxParameter("~[Periodo].[Mes].[Mes]", "mes"));
                string mdx = @"SELECT 
                                NON EMPTY {[Measures].[Ton Netas Expo],[Measures].[Valor Expo Miles FOB Dol]} ON 0, 
                                NON EMPTY ([Pais].[Pais].[Pais], @cadena, @anio, {";
                for (int i = 0; i < parameters.partida.Count; i++)
                {
                    mdx += (i + 1 == parameters.partida.Count) ? "@partida" + i : "@partida" + i + ",";
                    mdxParams.Add(new MdxParameter("@partida" + i, String.Format("[Producto].[Cadena-Partida10].&[{0}]", parameters.partida[i])));
                }
                mdx += @"}, [Periodo].[Mes].[Mes]) ON 1 FROM [Agronet Comercio];";                

                table.rows = adapter.GetDataTable(connectionName, mdx, mdxParams);
                returnData = (Table)table;
            }


            if (returnData == null)
            {
                return NotFound();
            }

            return Ok(returnData);
        }
    }
}
