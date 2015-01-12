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
        public IHttpActionResult postReport401(report401 parameters)
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
            else if (parameters.tipo == "tabla")
            {
                Table table = new Table { rows = new DataTable() };
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

        [Route("api/Report/402")]
        public IHttpActionResult postReport402(report402 parameters)
        {
            Object returnData = null;
            SQLAnalysisAdaper adapter = new SQLAnalysisAdaper();
            string connectionName = "AgronetSQLAnalysisServicesComercio";

            List<MdxParameter> mdxParams = new List<MdxParameter>();
            mdxParams.Add(new MdxParameter("~[Measures].[Ton Netas Expo]", "volumen"));
            mdxParams.Add(new MdxParameter("~[Measures].[Valor Expo Miles FOB Dol]", "valor"));
            mdxParams.Add(new MdxParameter("~[Periodo].[anho].[anho]", "anio"));
            switch (parameters.tipo)
            {
                case "parametro":
                    Parameter parameter = new Parameter { data = new List<ParameterData>() };
                    switch (parameters.id)
                    {
                        case 1:
                            parameter.name = "anio";
                            mdxParams.Add(new MdxParameter("@anio", "[Periodo].[anho].[anho]"));
                            string mdx1 = @"SELECT 
                                NON EMPTY {[Measures].[Ton Netas Expo],[Measures].[Valor Expo Miles FOB Dol]} ON 0, 
                                NON EMPTY (@anio) ON 1
                                FROM [Agronet Comercio];";

                            DataTable data1 = adapter.GetDataTable(connectionName, mdx1, mdxParams);
                            foreach (var p in (from p in data1.AsEnumerable()
                                               select p["anio"]))
                            {
                                ParameterData param = new ParameterData { name = Convert.ToString(p).Trim(), value = Convert.ToString(p).Trim() };
                                parameter.data.Add(param);
                            }

                            returnData = (Parameter)parameter;
                            break;
                    }
                    break;
                case "tabla":
                    mdxParams.Add(new MdxParameter("@anio", String.Format("[Periodo].[anho].&[{0}]:[Periodo].[anho].&[{1}]", parameters.anio_inicial, parameters.anio_final)));
                    Table table = new Table { rows = new DataTable() };
                    switch (parameters.id)
                    {
                        case 1:                            
                            mdxParams.Add(new MdxParameter("~[Producto].[Capitulo-Partida4-Partida10].[Descripcion Partida4 Dig]", "partida"));
                            string mdx1 = @"SELECT { [Measures].[Ton Netas Expo], [Measures].[Valor Expo Miles FOB Dol] } ON COLUMNS, CROSSJOIN({NONEMPTY({
                                            [Producto].[Capitulo-Partida4-Partida10].[Descripcion Partida4 Dig].&[5201-Algodón sin cardar ni peinar.],
                                            [Producto].[Capitulo-Partida4-Partida10].[Descripcion Partida4 Dig].&[5202-Desperdicios de algodón (incluidos los desperdicios de hilados y las hilachas).],
                                            [Producto].[Capitulo-Partida4-Partida10].[Descripcion Partida4 Dig].&[5203-Algodón cardado o peinado.]
                                            })}, { @anio } ) ON ROWS FROM [Agronet Comercio]";
                            table.rows = adapter.GetDataTable(connectionName, mdx1, mdxParams);
                            returnData = (Table)table;
                            break;
                        case 2:
                            mdxParams.Add(new MdxParameter("~[Producto].[Capitulo-Partida4-Partida10].[Descripcion Capitulo]", "partida"));
                            string mdx2 = @"SELECT { [Measures].[Ton Netas Expo], [Measures].[Valor Expo Miles FOB Dol] } ON COLUMNS, CROSSJOIN({NONEMPTY({
                                            [Producto].[Capitulo-Partida4-Partida10].[Descripcion Capitulo].&[01-Animales vivos],
		                                    [Producto].[Capitulo-Partida4-Partida10].[Descripcion Capitulo].&[02-Carnes y despojos comestibles],
		                                    [Producto].[Capitulo-Partida4-Partida10].[Descripcion Capitulo].&[03-Pescados y crustáceos, moluscos e invertebrados acuáticos],
		                                    [Producto].[Capitulo-Partida4-Partida10].[Descripcion Capitulo].&[04-Leche y productos lácteos, huevos, miel],
		                                    [Producto].[Capitulo-Partida4-Partida10].[Descripcion Capitulo].&[05-Demás productos de origen animal]
                                            })}, { @anio } ) ON ROWS FROM [Agronet Comercio]";
                            table.rows = adapter.GetDataTable(connectionName, mdx2, mdxParams);
                            returnData = (Table)table;
                            break;
                        case 3:
                            mdxParams.Add(new MdxParameter("~[Producto].[Capitulo-Partida4-Partida10].[Descripcion Capitulo]", "partida"));
                            string mdx3 = @"SELECT { [Measures].[Ton Netas Expo], [Measures].[Valor Expo Miles FOB Dol] } ON COLUMNS, CROSSJOIN({NONEMPTY({
                                            [Producto].[Capitulo-Partida4-Partida10].[Descripcion Capitulo].&[15-Grasas y aceites animales o vegetales]
                                            })}, { @anio } ) ON ROWS FROM [Agronet Comercio]";
                            table.rows = adapter.GetDataTable(connectionName, mdx3, mdxParams);
                            returnData = (Table)table;
                            break;
                        case 4:
                            mdxParams.Add(new MdxParameter("~[Producto].[Capitulo-Partida4-Partida10].[Descripcion Capitulo]", "partida"));
                            string mdx4 = @"SELECT { [Measures].[Ton Netas Expo], [Measures].[Valor Expo Miles FOB Dol] } ON COLUMNS, CROSSJOIN({NONEMPTY({
                                            [Producto].[Capitulo-Partida4-Partida10].[Descripcion Capitulo].&[15-Grasas y aceites animales o vegetales],
                                            [Producto].[Capitulo-Partida4-Partida10].[Descripcion Capitulo].&[16-Preparaciones de carne, pescado, crustáceos, moluscos],
                                            [Producto].[Capitulo-Partida4-Partida10].[Descripcion Capitulo].&[17-Azucares y artículos confitería],
                                            [Producto].[Capitulo-Partida4-Partida10].[Descripcion Capitulo].&[18-Cacao y sus preparaciones],
                                            [Producto].[Capitulo-Partida4-Partida10].[Descripcion Capitulo].&[19-Preparaciones a base de cereal, harina, leche; pastelería],
                                            [Producto].[Capitulo-Partida4-Partida10].[Descripcion Capitulo].&[20-Preparaciones de legumbres u hortalizas, frutos, otras],
                                            [Producto].[Capitulo-Partida4-Partida10].[Descripcion Capitulo].&[21-Preparaciones alimenticias diversas],
                                            [Producto].[Capitulo-Partida4-Partida10].[Descripcion Capitulo].&[22-Bebidas, líquidos alcohólicos y vinagre],
                                            [Producto].[Capitulo-Partida4-Partida10].[Descripcion Capitulo].&[23-Residuos industrias alimentarias. Alimentos para animales],
                                            [Producto].[Capitulo-Partida4-Partida10].[Descripcion Capitulo].&[24-Tabaco sucedáneos del tabaco elaborados]
                                            })}, { @anio } ) ON ROWS FROM [Agronet Comercio]";
                            table.rows = adapter.GetDataTable(connectionName, mdx4, mdxParams);
                            returnData = (Table)table;
                            break;
                        case 5:
                            mdxParams.Add(new MdxParameter("~[Producto].[Capitulo-Partida4-Partida10].[Descripcion Capitulo]", "partida"));
                            string mdx5 = @"SELECT { [Measures].[Ton Netas Expo], [Measures].[Valor Expo Miles FOB Dol] } ON COLUMNS, CROSSJOIN({NONEMPTY({
                                            [Producto].[Capitulo-Partida4-Partida10].[Descripcion Capitulo].&[06-Plantas vivas y productos de la floricultura],
		                                    [Producto].[Capitulo-Partida4-Partida10].[Descripcion Capitulo].&[07-Legumbres y hortalizas, plantas, raíces y tubérculos],
		                                    [Producto].[Capitulo-Partida4-Partida10].[Descripcion Capitulo].&[08-Frutos comestibles, cortezas de agrios o melones],
		                                    [Producto].[Capitulo-Partida4-Partida10].[Descripcion Capitulo].&[09-Café, té, yerbamate y especias],
		                                    [Producto].[Capitulo-Partida4-Partida10].[Descripcion Capitulo].&[10-Cereales],
		                                    [Producto].[Capitulo-Partida4-Partida10].[Descripcion Capitulo].&[11-Productos de molinería, malta, almidón y fécula],
		                                    [Producto].[Capitulo-Partida4-Partida10].[Descripcion Capitulo].&[12-Semillas y frutos oleaginosos, forrajes],
		                                    [Producto].[Capitulo-Partida4-Partida10].[Descripcion Capitulo].&[13-Gomas, resinas, y demás jugos y extractos vegetales],
		                                    [Producto].[Capitulo-Partida4-Partida10].[Descripcion Capitulo].&[14-Materias trenzables y demás productos vegetales]
                                            })}, { @anio } ) ON ROWS FROM [Agronet Comercio]";
                            table.rows = adapter.GetDataTable(connectionName, mdx5, mdxParams);
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

        [Route("api/Report/403")]
        public IHttpActionResult postReport403(report403 parameters)
        {
            Object returnData = null;
            SQLAnalysisAdaper adapter = new SQLAnalysisAdaper();
            string connectionName = "AgronetSQLAnalysisServicesComercio";

            List<MdxParameter> mdxParams = new List<MdxParameter>();
            mdxParams.Add(new MdxParameter("~[Measures].[Ton Netas Expo]", "volumen"));
            mdxParams.Add(new MdxParameter("~[Measures].[Valor Expo Miles FOB Dol]", "valor"));
            mdxParams.Add(new MdxParameter("~[Periodo].[anho].[anho]", "anio"));
            switch (parameters.tipo)
            {
                case "parametro":
                    Parameter parameter = new Parameter { data = new List<ParameterData>() };
                    switch (parameters.id)
                    {
                        case 1:
                            parameter.name = "producto";
                            mdxParams.Add(new MdxParameter("@producto", "[Producto].[Producto General].[Producto General]"));
                            mdxParams.Add(new MdxParameter("~[Producto].[Producto General].[Producto General]", "producto"));
                            string mdx1 = @"SELECT 
                            NON EMPTY {[Measures].[Ton Netas Expo],[Measures].[Valor Expo Miles FOB Dol]} ON 0, 
                            NON EMPTY (@producto) ON 1
                            FROM [Agronet Comercio];";

                            DataTable data1 = adapter.GetDataTable(connectionName, mdx1, mdxParams);
                            foreach (var p in (from p in data1.AsEnumerable()
                                               select p["producto"]))
                            {
                                ParameterData param = new ParameterData { name = Convert.ToString(p).Trim(), value = Convert.ToString(p).Trim() };
                                parameter.data.Add(param);
                            }
                            returnData = (Parameter)parameter;
                            break;
                        case 2:
                            parameter.name = "partida";
                            mdxParams.Add(new MdxParameter("@partidas", "[Producto].[Cadena-Partida10].[Descripcion Partida10 Dig Union]"));
                            mdxParams.Add(new MdxParameter("~[Producto].[Cadena-Partida10].[Descripcion Partida10 Dig Union]", "partidas"));
                            mdxParams.Add(new MdxParameter("@producto", String.Format("[Producto].[Producto General].&[{0}]", parameters.producto)));
                            string mdx3 = @"SELECT 
                            NON EMPTY {[Measures].[Ton Netas Expo],[Measures].[Valor Expo Miles FOB Dol]} ON 0, 
                            NON EMPTY (@partidas) ON 1
                            FROM [Agronet Comercio]
                            WHERE (@producto);";

                            DataTable data3 = adapter.GetDataTable(connectionName, mdx3, mdxParams);
                            foreach (var p in (from p in data3.AsEnumerable()
                                               select p["partidas"]))
                            {
                                ParameterData param = new ParameterData { name = Convert.ToString(p).Trim(), value = Convert.ToString(p).Trim() };
                                parameter.data.Add(param);
                            }
                            returnData = (Parameter)parameter;
                            break;
                        case 3:
                            parameter.name = "anio";
                            mdxParams.Add(new MdxParameter("@anio", "[Periodo].[anho].[anho]"));
                            string mdx2 = @"SELECT 
                            NON EMPTY {[Measures].[Ton Netas Expo],[Measures].[Valor Expo Miles FOB Dol]} ON 0, 
                            NON EMPTY (@anio) ON 1
                            FROM [Agronet Comercio];";

                            DataTable data2 = adapter.GetDataTable(connectionName, mdx2, mdxParams);
                            foreach (var p in (from p in data2.AsEnumerable()
                                               select p["anio"]))
                            {
                                ParameterData param = new ParameterData { name = Convert.ToString(p).Trim(), value = Convert.ToString(p).Trim() };
                                parameter.data.Add(param);
                            }
                            returnData = (Parameter)parameter;
                            break;
                    }
                    
                    break;
                case "tabla":
                    switch (parameters.id)
                    {
                        case 1:
                            Table table = new Table { rows = new DataTable() };
                            mdxParams.Add(new MdxParameter("@producto", String.Format("[Producto].[Producto General].&[{0}]", parameters.producto)));
                            mdxParams.Add(new MdxParameter("~[Producto].[Producto General].[Producto General]", "producto"));
                            mdxParams.Add(new MdxParameter("~[Producto].[Cadena-Partida10].[Descripcion Partida10 Dig Union]", "partidas"));
                            mdxParams.Add(new MdxParameter("@anio", String.Format("[Periodo].[anho].&[{0}]:[Periodo].[anho].&[{1}]", parameters.anio_inicial, parameters.anio_final)));
                            mdxParams.Add(new MdxParameter("~[Pais].[Pais].[Pais]", "pais"));
                            mdxParams.Add(new MdxParameter("~[Periodo].[Mes].[Mes]", "mes"));
                            string mdx = @"SELECT 
                                            NON EMPTY {[Measures].[Ton Netas Expo],[Measures].[Valor Expo Miles FOB Dol]} ON 0, 
                                            NON EMPTY ([Pais].[Pais].[Pais], @producto, @anio, {";
                            for (int i = 0; i < parameters.partida.Count; i++)
                            {
                                mdx += (i + 1 == parameters.partida.Count) ? "@partida" + i : "@partida" + i + ",";
                                mdxParams.Add(new MdxParameter("@partida" + i, String.Format("[Producto].[Cadena-Partida10].&[{0}]", parameters.partida[i])));
                            }
                            mdx += @"}, [Periodo].[Mes].[Mes]) ON 1 FROM [Agronet Comercio];";                

                            table.rows = adapter.GetDataTable(connectionName, mdx, mdxParams);
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

        [Route("api/Report/404")]
        public IHttpActionResult postReport404(report404 parameters)
        {
            Object returnData = null;
            SQLAnalysisAdaper adapter = new SQLAnalysisAdaper();
            string connectionName = "AgronetSQLAnalysisServicesComercio";

            List<MdxParameter> mdxParams = new List<MdxParameter>();
            switch (parameters.tipo)
            {
                case "parametro":
                    Parameter parameter = new Parameter { data = new List<ParameterData>() };
                    switch (parameters.id)
                    {
                        case 1:
                            parameter.name = "anio";
                            mdxParams.Add(new MdxParameter("@anio", "[Periodo].[anho].[anho]"));
                            mdxParams.Add(new MdxParameter("~[Periodo].[anho].[anho]", "anio"));
                            string mdx1 = @"SELECT NonEmpty({{}}) ON 0, NonEmpty({ @anio }) ON 1 FROM [Agronet Comercio];";

                            DataTable data1 = adapter.GetDataTable(connectionName, mdx1, mdxParams);
                            foreach (var p in (from p in data1.AsEnumerable()
                                               select p["anio"]))
                            {
                                ParameterData param = new ParameterData { name = Convert.ToString(p).Trim(), value = Convert.ToString(p).Trim() };
                                parameter.data.Add(param);
                            }
                            returnData = (Parameter)parameter;
                            break;
                        case 2:
                            parameter.name = "pais";
                            mdxParams.Add(new MdxParameter("@pais", "[Pais].[Pais].[Pais]"));
                            mdxParams.Add(new MdxParameter("~[Pais].[Pais].[Pais]", "pais"));
                            string mdx2 = @"SELECT NonEmpty({{}}) ON 0, NonEmpty({ @pais }) ON 1 FROM [Agronet Comercio];";

                            DataTable data2 = adapter.GetDataTable(connectionName, mdx2, mdxParams);
                            foreach (var p in (from p in data2.AsEnumerable()
                                               select p["pais"]))
                            {
                                ParameterData param = new ParameterData { name = Convert.ToString(p).Trim(), value = Convert.ToString(p).Trim() };
                                parameter.data.Add(param);
                            }
                            returnData = (Parameter)parameter;
                            break;
                    }
                    break;
                case "grafico":
                    switch (parameters.id)
                    {
                        case 1:
                            mdxParams.Add(new MdxParameter("~[Measures].[Ton Netas Expo]", "volumen"));
                            mdxParams.Add(new MdxParameter("~[Producto].[Producto-Partida10].[Descripcion Partida10 Dig Union]", "partida"));
                            mdxParams.Add(new MdxParameter("@anio", String.Format("[Periodo].[anho].&[{0}]:[Periodo].[anho].&[{1}]", parameters.anio_inicial, parameters.anio_final)));
                            mdxParams.Add(new MdxParameter("@pais", String.Format("[Pais].[Pais].[{0}]", parameters.pais)));
                            string mdx1 = @"SELECT NON EMPTY {[Measures].[Ton Netas Expo]} On 0,--TONELADAS DE EXPORTACION Y VALOR DE EXPORTACIÓN EN DÓLARES
                                            TopCount(Filter(NonEmpty({[Producto].[Producto-Partida10].[Descripcion Partida10 Dig Union].MEMBERS}),[Measures].[Ton Netas Expo]>0),10,[Measures].[Ton Netas Expo]) ON 1
                                            FROM [Agronet Comercio] WHERE {
                                            [Producto].[Capitulo-Partida4-Partida10].[Descripcion Capitulo].&[01-Animales vivos]:
                                            [Producto].[Capitulo-Partida4-Partida10].[Descripcion Capitulo].&[24-Tabaco sucedáneos del tabaco elaborados],
                                            [Producto].[Capitulo-Partida4-Partida10].[Descripcion Partida10 Dig Union].&[2905430000 - Manitol.],
                                            [Producto].[Capitulo-Partida4-Partida10].[Descripcion Partida10 Dig Union].&[2905440000 - D-glusitol (sorbitol).],
                                            [Producto].[Capitulo-Partida4-Partida10].[Descripcion Partida4 Dig].&[3301-Aceites esenciales (desterpenados o no), incluidos los ""concretos"" o ""absolutos"", resinoides; o],
                                            [Producto].[Capitulo-Partida4-Partida10].[Descripcion Partida4 Dig].&[3501-Caseína, caseinatos y demás derivados de la caseína; colas de caseína.]:[Producto].[Capitulo-Partida4-Partida10].[Descripcion Partida4 Dig].&[3505-Dextrina y demás almidones y féculas modificados (por ejemplo: almidones y féculas pregelatiniz],
                                            [Producto].[Capitulo-Partida4-Partida10].[Descripcion Partida10 Dig Union].&[3809100000 - Aprestos y productos de acabado a base de materias amiláceas.],
                                            [Producto].[Capitulo-Partida4-Partida10].[Descripcion Partida10 Dig Union].&[3823600000 - Sorbitol, excepto el de la subpartida 29.05.44.00.],
                                            [Producto].[Capitulo-Partida4-Partida10].[Descripcion Partida4 Dig].&[4001-Caucho natural, balata, gutapercha, guayule, chicle y gomas naturales análogas, en formas prima],
                                            [Producto].[Capitulo-Partida4-Partida10].[Descripcion Partida4 Dig].&[4101-Cueros y pieles, en bruto, de bovino o de equino (frescos o salados, secos, encalados, piquelad]:[Producto].[Capitulo-Partida4-Partida10].[Descripcion Partida4 Dig].&[4103-Los demás cueros y pieles, en bruto (frescos o salados, secos, encalados, piquelados o conserva],
                                            [Producto].[Capitulo-Partida4-Partida10].[Descripcion Partida4 Dig].&[4401-Leña; madera en plaquitas o partículas, aserrín, desperdicios y desechos, de madera, incluso ag]:[Producto].[Capitulo-Partida4-Partida10].[Descripcion Partida4 Dig].&[4407-Madera aserrada o debastada longitudinalmente, cortada o desenrollada, incluso cepillada, lijad],
                                            [Producto].[Capitulo-Partida4-Partida10].[Descripcion Partida4 Dig].&[5001-Capullos de seda aptos. Paa el devanado.]:[Producto].[Capitulo-Partida4-Partida10].[Descripcion Partida4 Dig].&[5003-Desperdicios de seda (incluidos los capullos no aptos para el devanado, desperdicios de hilados],
                                            [Producto].[Capitulo-Partida4-Partida10].[Descripcion Partida4 Dig].&[5101-Lana sin cardar ni peinar.]:[Producto].[Capitulo-Partida4-Partida10].[Descripcion Partida4 Dig].&[5103-Desperdicios de lana o de pelo fino u ordinario, incluidos los desperdicios de hilados, excepto],
                                            [Producto].[Capitulo-Partida4-Partida10].[Descripcion Partida4 Dig].&[5201-Algodón sin cardar ni peinar.]:[Producto].[Capitulo-Partida4-Partida10].[Descripcion Partida4 Dig].&[5203-Algodón cardado o peinado.],
                                            [Producto].[Capitulo-Partida4-Partida10].[Descripcion Partida4 Dig].&[5301-Lino en bruto o trabajado, pero sin hilar, estopas y desperdicios, de lino (incluidos los despe],
                                            [Producto].[Capitulo-Partida4-Partida10].[Descripcion Partida4 Dig].&[5302-Cañamo (cannabis sativa) en bruto o trabajado, pero sin hilar, estopas y desperdicios de cáñamo]
                                            } * { @pais } * { @anio }";
                            
                            DataTable data1 = adapter.GetDataTable(connectionName, mdx1, mdxParams);
                            Chart chart1 = new Chart { series = new List<Series>() };

                            Series serie1 = new Series { name = "Partida", data = new List<Data>() };
                            foreach (var d in (from d in data1.AsEnumerable() select d))
                            {
                                Data data = new Data { name = Convert.ToString(d["partida"]).Substring(0,20)+"...", y = Convert.ToDouble(d["volumen"])};
                                serie1.data.Add(data);
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
                            mdxParams.Add(new MdxParameter("~[Measures].[Ton Netas Expo]", "volumen"));
                            mdxParams.Add(new MdxParameter("~[Measures].[Valor Expo Miles FOB Dol]", "valor"));
                            mdxParams.Add(new MdxParameter("~[Periodo].[anho].[anho]", "anio"));
                            mdxParams.Add(new MdxParameter("~[Producto].[Producto-Partida10].[Descripcion Partida10 Dig Union]", "partida"));
                            mdxParams.Add(new MdxParameter("@anio", String.Format("[Periodo].[anho].&[{0}]:[Periodo].[anho].&[{1}]", parameters.anio_inicial, parameters.anio_final)));
                            mdxParams.Add(new MdxParameter("@pais", String.Format("[Pais].[Pais].[{0}]", parameters.pais)));
                            
                            string mdx1 = @"SELECT NON EMPTY {[Measures].[Ton Netas Expo],[Measures].[Valor Expo Miles FOB Dol]} On 0,--TONELADAS DE EXPORTACION Y VALOR DE EXPORTACIÓN EN DÓLARES
                                            TopCount(Filter(NonEmpty({ @anio } * { [Producto].[Producto-Partida10].[Descripcion Partida10 Dig Union].MEMBERS }),[Measures].[Ton Netas Expo]>0),10,[Measures].[Ton Netas Expo]) ON 1
                                            FROM [Agronet Comercio] WHERE {
                                            [Producto].[Capitulo-Partida4-Partida10].[Descripcion Capitulo].&[01-Animales vivos]:
                                            [Producto].[Capitulo-Partida4-Partida10].[Descripcion Capitulo].&[24-Tabaco sucedáneos del tabaco elaborados],
                                            [Producto].[Capitulo-Partida4-Partida10].[Descripcion Partida10 Dig Union].&[2905430000 - Manitol.],
                                            [Producto].[Capitulo-Partida4-Partida10].[Descripcion Partida10 Dig Union].&[2905440000 - D-glusitol (sorbitol).],
                                            [Producto].[Capitulo-Partida4-Partida10].[Descripcion Partida4 Dig].&[3301-Aceites esenciales (desterpenados o no), incluidos los ""concretos"" o ""absolutos"", resinoides; o],
                                            [Producto].[Capitulo-Partida4-Partida10].[Descripcion Partida4 Dig].&[3501-Caseína, caseinatos y demás derivados de la caseína; colas de caseína.]:[Producto].[Capitulo-Partida4-Partida10].[Descripcion Partida4 Dig].&[3505-Dextrina y demás almidones y féculas modificados (por ejemplo: almidones y féculas pregelatiniz],
                                            [Producto].[Capitulo-Partida4-Partida10].[Descripcion Partida10 Dig Union].&[3809100000 - Aprestos y productos de acabado a base de materias amiláceas.],
                                            [Producto].[Capitulo-Partida4-Partida10].[Descripcion Partida10 Dig Union].&[3823600000 - Sorbitol, excepto el de la subpartida 29.05.44.00.],
                                            [Producto].[Capitulo-Partida4-Partida10].[Descripcion Partida4 Dig].&[4001-Caucho natural, balata, gutapercha, guayule, chicle y gomas naturales análogas, en formas prima],
                                            [Producto].[Capitulo-Partida4-Partida10].[Descripcion Partida4 Dig].&[4101-Cueros y pieles, en bruto, de bovino o de equino (frescos o salados, secos, encalados, piquelad]:[Producto].[Capitulo-Partida4-Partida10].[Descripcion Partida4 Dig].&[4103-Los demás cueros y pieles, en bruto (frescos o salados, secos, encalados, piquelados o conserva],
                                            [Producto].[Capitulo-Partida4-Partida10].[Descripcion Partida4 Dig].&[4401-Leña; madera en plaquitas o partículas, aserrín, desperdicios y desechos, de madera, incluso ag]:[Producto].[Capitulo-Partida4-Partida10].[Descripcion Partida4 Dig].&[4407-Madera aserrada o debastada longitudinalmente, cortada o desenrollada, incluso cepillada, lijad],
                                            [Producto].[Capitulo-Partida4-Partida10].[Descripcion Partida4 Dig].&[5001-Capullos de seda aptos. Paa el devanado.]:[Producto].[Capitulo-Partida4-Partida10].[Descripcion Partida4 Dig].&[5003-Desperdicios de seda (incluidos los capullos no aptos para el devanado, desperdicios de hilados],
                                            [Producto].[Capitulo-Partida4-Partida10].[Descripcion Partida4 Dig].&[5101-Lana sin cardar ni peinar.]:[Producto].[Capitulo-Partida4-Partida10].[Descripcion Partida4 Dig].&[5103-Desperdicios de lana o de pelo fino u ordinario, incluidos los desperdicios de hilados, excepto],
                                            [Producto].[Capitulo-Partida4-Partida10].[Descripcion Partida4 Dig].&[5201-Algodón sin cardar ni peinar.]:[Producto].[Capitulo-Partida4-Partida10].[Descripcion Partida4 Dig].&[5203-Algodón cardado o peinado.],
                                            [Producto].[Capitulo-Partida4-Partida10].[Descripcion Partida4 Dig].&[5301-Lino en bruto o trabajado, pero sin hilar, estopas y desperdicios, de lino (incluidos los despe],
                                            [Producto].[Capitulo-Partida4-Partida10].[Descripcion Partida4 Dig].&[5302-Cañamo (cannabis sativa) en bruto o trabajado, pero sin hilar, estopas y desperdicios de cáñamo]
                                            } * { @pais }";
                            returnData = (Table)new Table { rows = adapter.GetDataTable(connectionName, mdx1, mdxParams) };
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

        [Route("api/Report/405")]
        public IHttpActionResult postReport405(report405 parameters)
        {
            Object returnData = null;
            SQLAnalysisAdaper adapter = new SQLAnalysisAdaper();
            string connectionName = "AgronetSQLAnalysisServicesComercio";

            List<MdxParameter> mdxParams = new List<MdxParameter>();
            switch (parameters.tipo)
            {
                case "parametro":
                    Parameter parameter = new Parameter { data = new List<ParameterData>() };
                    switch (parameters.id)
                    {
                        case 1:
                            parameter.name = "anio";
                            mdxParams.Add(new MdxParameter("@anio", "[Periodo].[anho].[anho]"));
                            mdxParams.Add(new MdxParameter("~[Periodo].[anho].[anho]", "anio"));
                            string mdx1 = @"SELECT NonEmpty({{}}) ON 0, NonEmpty({ @anio }) ON 1 FROM [Agronet Comercio];";

                            DataTable data1 = adapter.GetDataTable(connectionName, mdx1, mdxParams);
                            foreach (var p in (from p in data1.AsEnumerable()
                                               select p["anio"]))
                            {
                                ParameterData param = new ParameterData { name = Convert.ToString(p).Trim(), value = Convert.ToString(p).Trim() };
                                parameter.data.Add(param);
                            }
                            returnData = (Parameter)parameter;
                            break;
                        case 2:
                            parameter.name = "partida";
                            mdxParams.Add(new MdxParameter("@partida", "[Producto].[Producto-Partida10].[Descripcion Partida10 Dig Union]"));
                            mdxParams.Add(new MdxParameter("~[Producto].[Producto-Partida10].[Descripcion Partida10 Dig Union]", "partida"));
                            string mdx2 = @"SELECT NonEmpty({{}}) ON 0, NonEmpty({ @partida }) ON 1 FROM [Agronet Comercio];";

                            DataTable data2 = adapter.GetDataTable(connectionName, mdx2, mdxParams);
                            foreach (var p in (from p in data2.AsEnumerable()
                                               select p["partida"]))
                            {
                                ParameterData param = new ParameterData { name = Convert.ToString(p).Trim(), value = Convert.ToString(p).Trim() };
                                parameter.data.Add(param);
                            }
                            returnData = (Parameter)parameter;
                            break;
                    }
                    break;
                case "grafico":
                    switch (parameters.id)
                    {
                        case 1:
                            mdxParams.Add(new MdxParameter("@anio", String.Format("[Periodo].[anho].&[{0}]:[Periodo].[anho].&[{1}]", parameters.anio_inicial, parameters.anio_final)));
                            mdxParams.Add(new MdxParameter("~[Periodo].[anho].[anho]", "anio"));
                            mdxParams.Add(new MdxParameter("~[Pais].[Pais].[Pais]", "pais"));
                            mdxParams.Add(new MdxParameter("~[Measures].[Ton Netas Expo]", "volumen"));
                            string mdx1 = @"SELECT 
                                            NonEmpty({{[Measures].[Ton Netas Expo]}}) ON 0,
                                            NonEmpty({[Pais].[Pais].[Pais]} * { @anio }) ON 1
                                            FROM [Agronet Comercio] WHERE {
                                            [Producto].[Capitulo-Partida4-Partida10].[Descripcion Capitulo].&[01-Animales vivos]:
                                            [Producto].[Capitulo-Partida4-Partida10].[Descripcion Capitulo].&[24-Tabaco sucedáneos del tabaco elaborados],
                                            [Producto].[Capitulo-Partida4-Partida10].[Descripcion Partida10 Dig Union].&[2905430000 - Manitol.],
                                            [Producto].[Capitulo-Partida4-Partida10].[Descripcion Partida10 Dig Union].&[2905440000 - D-glusitol (sorbitol).],
                                            [Producto].[Capitulo-Partida4-Partida10].[Descripcion Partida4 Dig].&[3301-Aceites esenciales (desterpenados o no), incluidos los ""concretos"" o ""absolutos"", resinoides; o],
                                            [Producto].[Capitulo-Partida4-Partida10].[Descripcion Partida4 Dig].&[3501-Caseína, caseinatos y demás derivados de la caseína; colas de caseína.]:[Producto].[Capitulo-Partida4-Partida10].[Descripcion Partida4 Dig].&[3505-Dextrina y demás almidones y féculas modificados (por ejemplo: almidones y féculas pregelatiniz],
                                            [Producto].[Capitulo-Partida4-Partida10].[Descripcion Partida10 Dig Union].&[3809100000 - Aprestos y productos de acabado a base de materias amiláceas.],
                                            [Producto].[Capitulo-Partida4-Partida10].[Descripcion Partida10 Dig Union].&[3823600000 - Sorbitol, excepto el de la subpartida 29.05.44.00.],
                                            [Producto].[Capitulo-Partida4-Partida10].[Descripcion Partida4 Dig].&[4001-Caucho natural, balata, gutapercha, guayule, chicle y gomas naturales análogas, en formas prima],
                                            [Producto].[Capitulo-Partida4-Partida10].[Descripcion Partida4 Dig].&[4101-Cueros y pieles, en bruto, de bovino o de equino (frescos o salados, secos, encalados, piquelad]:[Producto].[Capitulo-Partida4-Partida10].[Descripcion Partida4 Dig].&[4103-Los demás cueros y pieles, en bruto (frescos o salados, secos, encalados, piquelados o conserva],
                                            [Producto].[Capitulo-Partida4-Partida10].[Descripcion Partida4 Dig].&[4401-Leña; madera en plaquitas o partículas, aserrín, desperdicios y desechos, de madera, incluso ag]:[Producto].[Capitulo-Partida4-Partida10].[Descripcion Partida4 Dig].&[4407-Madera aserrada o debastada longitudinalmente, cortada o desenrollada, incluso cepillada, lijad],
                                            [Producto].[Capitulo-Partida4-Partida10].[Descripcion Partida4 Dig].&[5001-Capullos de seda aptos. Paa el devanado.]:[Producto].[Capitulo-Partida4-Partida10].[Descripcion Partida4 Dig].&[5003-Desperdicios de seda (incluidos los capullos no aptos para el devanado, desperdicios de hilados],
                                            [Producto].[Capitulo-Partida4-Partida10].[Descripcion Partida4 Dig].&[5101-Lana sin cardar ni peinar.]:[Producto].[Capitulo-Partida4-Partida10].[Descripcion Partida4 Dig].&[5103-Desperdicios de lana o de pelo fino u ordinario, incluidos los desperdicios de hilados, excepto],
                                            [Producto].[Capitulo-Partida4-Partida10].[Descripcion Partida4 Dig].&[5201-Algodón sin cardar ni peinar.]:[Producto].[Capitulo-Partida4-Partida10].[Descripcion Partida4 Dig].&[5203-Algodón cardado o peinado.],
                                            [Producto].[Capitulo-Partida4-Partida10].[Descripcion Partida4 Dig].&[5301-Lino en bruto o trabajado, pero sin hilar, estopas y desperdicios, de lino (incluidos los despe],
                                            [Producto].[Capitulo-Partida4-Partida10].[Descripcion Partida4 Dig].&[5302-Cañamo (cannabis sativa) en bruto o trabajado, pero sin hilar, estopas y desperdicios de cáñamo]
                                            } * {";
                            for (int i = 0; i < parameters.partida.Count; i++)
                            {
                                mdx1 += (i + 1 == parameters.partida.Count) ? "@partida" + i : "@partida" + i + ",";
                                mdxParams.Add(new MdxParameter("@partida" + i, String.Format("[Producto].[Producto-Partida10].[Descripcion Partida10 Dig Union].&[{0}]", parameters.partida[i])));
                            }
                            mdx1 += "}";
                            Chart chart1 = new Chart { subtitle = "", series = new List<Series>() };

                            var query1 = from r in (adapter.GetDataTable(connectionName, mdx1, mdxParams)).AsEnumerable()
                                         group r by r["pais"];

                            foreach (var paisGroup in query1)
                            {
                                var serie1 = new Series { name = paisGroup.Key.ToString(), data = new List<Data>() };
                                foreach (var el1 in paisGroup)
                                {
                                    var data = new Data { name = Convert.ToString(el1["anio"]), y = Convert.ToDouble(el1["volumen"]) };
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
                            mdxParams.Add(new MdxParameter("@anio", String.Format("[Periodo].[anho].&[{0}]:[Periodo].[anho].&[{1}]", parameters.anio_inicial, parameters.anio_final)));
                            mdxParams.Add(new MdxParameter("~[Periodo].[anho].[anho]", "anio"));
                            mdxParams.Add(new MdxParameter("~[Producto].[Producto-Partida10].[Descripcion Partida10 Dig Union]", "partida"));
                            mdxParams.Add(new MdxParameter("~[Periodo].[Mes].[Mes]", "mes"));                            
                            mdxParams.Add(new MdxParameter("~[Pais].[Pais].[Pais]", "pais"));
                            mdxParams.Add(new MdxParameter("~[Measures].[Ton Netas Expo]", "volumen"));
                            mdxParams.Add(new MdxParameter("~[Measures].[Valor Expo Miles FOB Dol]", "valor"));
                            string mdx1 = @"SELECT NonEmpty({{[Measures].[Ton Netas Expo],[Measures].[Valor Expo Miles FOB Dol]}}) ON 0,
                            NonEmpty({@anio} * {[Periodo].[Mes].[Mes]} * {";
                            for (int i = 0; i < parameters.partida.Count; i++)
                            {
                                mdx1 += (i + 1 == parameters.partida.Count) ? "@partida" + i : "@partida" + i + ",";
                                mdxParams.Add(new MdxParameter("@partida" + i, String.Format("[Producto].[Producto-Partida10].[Descripcion Partida10 Dig Union].&[{0}]", parameters.partida[i])));
                            }
                            mdx1 += @"} * {[Pais].[Pais].[Pais]} ) ON 1 FROM [Agronet Comercio] WHERE {[Producto].[Capitulo-Partida4-Partida10].[Descripcion Capitulo].&[01-Animales vivos]:
                            [Producto].[Capitulo-Partida4-Partida10].[Descripcion Capitulo].&[24-Tabaco sucedáneos del tabaco elaborados],
                            [Producto].[Capitulo-Partida4-Partida10].[Descripcion Partida10 Dig Union].&[2905430000 - Manitol.],
                            [Producto].[Capitulo-Partida4-Partida10].[Descripcion Partida10 Dig Union].&[2905440000 - D-glusitol (sorbitol).],
                            [Producto].[Capitulo-Partida4-Partida10].[Descripcion Partida4 Dig].&[3301-Aceites esenciales (desterpenados o no), incluidos los ""concretos"" o ""absolutos"", resinoides; o],
                            [Producto].[Capitulo-Partida4-Partida10].[Descripcion Partida4 Dig].&[3501-Caseína, caseinatos y demás derivados de la caseína; colas de caseína.]:[Producto].[Capitulo-Partida4-Partida10].[Descripcion Partida4 Dig].&[3505-Dextrina y demás almidones y féculas modificados (por ejemplo: almidones y féculas pregelatiniz],
                            [Producto].[Capitulo-Partida4-Partida10].[Descripcion Partida10 Dig Union].&[3809100000 - Aprestos y productos de acabado a base de materias amiláceas.],
                            [Producto].[Capitulo-Partida4-Partida10].[Descripcion Partida10 Dig Union].&[3823600000 - Sorbitol, excepto el de la subpartida 29.05.44.00.],
                            [Producto].[Capitulo-Partida4-Partida10].[Descripcion Partida4 Dig].&[4001-Caucho natural, balata, gutapercha, guayule, chicle y gomas naturales análogas, en formas prima],
                            [Producto].[Capitulo-Partida4-Partida10].[Descripcion Partida4 Dig].&[4101-Cueros y pieles, en bruto, de bovino o de equino (frescos o salados, secos, encalados, piquelad]:[Producto].[Capitulo-Partida4-Partida10].[Descripcion Partida4 Dig].&[4103-Los demás cueros y pieles, en bruto (frescos o salados, secos, encalados, piquelados o conserva],
                            [Producto].[Capitulo-Partida4-Partida10].[Descripcion Partida4 Dig].&[4401-Leña; madera en plaquitas o partículas, aserrín, desperdicios y desechos, de madera, incluso ag]:[Producto].[Capitulo-Partida4-Partida10].[Descripcion Partida4 Dig].&[4407-Madera aserrada o debastada longitudinalmente, cortada o desenrollada, incluso cepillada, lijad],
                            [Producto].[Capitulo-Partida4-Partida10].[Descripcion Partida4 Dig].&[5001-Capullos de seda aptos. Paa el devanado.]:[Producto].[Capitulo-Partida4-Partida10].[Descripcion Partida4 Dig].&[5003-Desperdicios de seda (incluidos los capullos no aptos para el devanado, desperdicios de hilados],
                            [Producto].[Capitulo-Partida4-Partida10].[Descripcion Partida4 Dig].&[5101-Lana sin cardar ni peinar.]:[Producto].[Capitulo-Partida4-Partida10].[Descripcion Partida4 Dig].&[5103-Desperdicios de lana o de pelo fino u ordinario, incluidos los desperdicios de hilados, excepto],
                            [Producto].[Capitulo-Partida4-Partida10].[Descripcion Partida4 Dig].&[5201-Algodón sin cardar ni peinar.]:[Producto].[Capitulo-Partida4-Partida10].[Descripcion Partida4 Dig].&[5203-Algodón cardado o peinado.],
                            [Producto].[Capitulo-Partida4-Partida10].[Descripcion Partida4 Dig].&[5301-Lino en bruto o trabajado, pero sin hilar, estopas y desperdicios, de lino (incluidos los despe],
                            [Producto].[Capitulo-Partida4-Partida10].[Descripcion Partida4 Dig].&[5302-Cañamo (cannabis sativa) en bruto o trabajado, pero sin hilar, estopas y desperdicios de cáñamo]}";
                            returnData = (Table)new Table { rows = adapter.GetDataTable(connectionName, mdx1, mdxParams) };
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

        [Route("api/Report/406")]
        public IHttpActionResult postReport406(report406 parameters)
        {
            Object returnData = null;
            SQLAnalysisAdaper adapter = new SQLAnalysisAdaper();
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

        [Route("api/Report/407")]
        public IHttpActionResult postReport407(report407 parameters)
        {
            Object returnData = null;
            SQLAnalysisAdaper adapter = new SQLAnalysisAdaper();
            string connectionName = "AgronetSQLAnalysisServicesComercio";

            List<MdxParameter> mdxParams = new List<MdxParameter>();
            switch (parameters.tipo)
            {
                case "parametro":
                    Parameter parameter = new Parameter { data = new List<ParameterData>() };
                    switch (parameters.id)
                    {
                        case 1:
                            parameter.name = "anio";
                            mdxParams.Add(new MdxParameter("@anio", "[Periodo].[anho].[anho]"));
                            mdxParams.Add(new MdxParameter("~[Periodo].[anho].[anho]", "anio"));
                            string mdx1 = @"SELECT NonEmpty({{}}) ON 0, NonEmpty({ @anio }) ON 1 FROM [Agronet Comercio];";

                            DataTable data1 = adapter.GetDataTable(connectionName, mdx1, mdxParams);
                            foreach (var p in (from p in data1.AsEnumerable()
                                               select p["anio"]))
                            {
                                ParameterData param = new ParameterData { name = Convert.ToString(p).Trim(), value = Convert.ToString(p).Trim() };
                                parameter.data.Add(param);
                            }
                            returnData = (Parameter)parameter;
                            break;
                        case 2:
                            parameter.name = "producto";
                            mdxParams.Add(new MdxParameter("@producto", "[Producto].[Producto General].[Producto General]"));
                            mdxParams.Add(new MdxParameter("~[Producto].[Producto General].[Producto General]", "producto"));
                            string mdx2 = @"SELECT NonEmpty({{}}) ON 0, NonEmpty({ @producto }) ON 1 FROM [Agronet Comercio];";

                            DataTable data2 = adapter.GetDataTable(connectionName, mdx2, mdxParams);
                            foreach (var p in (from p in data2.AsEnumerable()
                                               select p["producto"]))
                            {
                                ParameterData param = new ParameterData { name = Convert.ToString(p).Trim(), value = Convert.ToString(p).Trim() };
                                parameter.data.Add(param);
                            }
                            returnData = (Parameter)parameter;
                            break;
                        case 3:
                            parameter.name = "partida";
                            mdxParams.Add(new MdxParameter("@partida", "[Producto].[Cadena-Partida10].[Descripcion Partida10 Dig Union]"));
                            mdxParams.Add(new MdxParameter("~[Producto].[Cadena-Partida10].[Descripcion Partida10 Dig Union]", "partida"));
                            mdxParams.Add(new MdxParameter("@producto", String.Format("[Producto].[Producto General].&[{0}]", parameters.producto)));
                            string mdx3 = @"SELECT NonEmpty({{}}) ON 0, NonEmpty({ @partida }) ON 1 FROM [Agronet Comercio] WHERE (@producto);";

                            DataTable data3 = adapter.GetDataTable(connectionName, mdx3, mdxParams);
                            foreach (var p in (from p in data3.AsEnumerable()
                                               select p["partida"]))
                            {
                                ParameterData param = new ParameterData { name = Convert.ToString(p).Trim(), value = Convert.ToString(p).Trim() };
                                parameter.data.Add(param);
                            }
                            returnData = (Parameter)parameter;
                            break;
                    }
                    break;
                case "grafico":
                    switch (parameters.id)
                    {
                        case 1:
                            mdxParams.Add(new MdxParameter("~[Periodo].[anho].[anho]", "anio"));
                            mdxParams.Add(new MdxParameter("~[Measures].[Ton Netas Impo]", "volumen"));
                            mdxParams.Add(new MdxParameter("~[Measures].[Valor Impo Miles CIF Dol]", "valor"));
                            mdxParams.Add(new MdxParameter("@anio", String.Format("[Periodo].[anho].&[{0}]:[Periodo].[anho].&[{1}]", parameters.anio_inicial, parameters.anio_final)));
                            mdxParams.Add(new MdxParameter("@producto", String.Format("[Producto].[Producto General].&[{0}]", parameters.producto)));
                            string mdx1 = @"SELECT NON EMPTY {[Measures].[Ton Netas Impo],[Measures].[Valor Impo Miles CIF Dol]} ON 0,
                            NON EMPTY Filter({ @anio },[Measures].[Ton Netas Impo] > 0) ON 1
                            FROM [Agronet Comercio] WHERE {";
                            for (int i = 0; i < parameters.partida.Count; i++)
                            {
                                mdx1 += (i + 1 == parameters.partida.Count) ? "@partida" + i : "@partida" + i + ",";
                                mdxParams.Add(new MdxParameter("@partida" + i, String.Format("[Producto].[Cadena-Partida10].[Descripcion Partida10 Dig Union].&[{0}]", parameters.partida[i])));
                            }
                            mdx1 += @"} * { @producto }";

                            Chart chart1 = new Chart { series = new List<Series>() };

                            Series serie1 = new Series { name = "Volumen", data = new List<Data>() };
                            Series serie2 = new Series { name = "Valor", data = new List<Data>() };

                            foreach (var d in (from d in (adapter.GetDataTable(connectionName, mdx1, mdxParams)).AsEnumerable() select d))
                            {
                                Data data1 = new Data { name = Convert.ToString(d["anio"]), y = Convert.ToDouble(d["volumen"])};
                                Data data2 = new Data { name = Convert.ToString(d["anio"]), y = Convert.ToDouble(d["valor"]) };
                                
                                serie1.data.Add(data1);
                                serie2.data.Add(data2);
                            }
                            chart1.series.Add(serie1);
                            chart1.series.Add(serie2);

                            returnData = (Chart)chart1;
                            break;
                    }
                    break;
                case "tabla":
                    switch (parameters.id)
                    {
                        case 1:
                            mdxParams.Add(new MdxParameter("~[Periodo].[anho].[anho]", "anio"));
                            mdxParams.Add(new MdxParameter("~[Periodo].[Mes].[Mes]", "mes"));
                            mdxParams.Add(new MdxParameter("~[Pais].[Pais].[Pais]", "pais"));
                            mdxParams.Add(new MdxParameter("~[Producto].[Cadena-Partida10].[Descripcion Partida10 Dig Union]", "partida"));
                            mdxParams.Add(new MdxParameter("~[Producto].[Cadena].[Cadena]", "cadena"));
                            mdxParams.Add(new MdxParameter("~[Measures].[Ton Netas Impo]", "volumen"));
                            mdxParams.Add(new MdxParameter("~[Measures].[Valor Impo Miles CIF Dol]", "valor"));
                            mdxParams.Add(new MdxParameter("@anio", String.Format("[Periodo].[anho].&[{0}]:[Periodo].[anho].&[{1}]", parameters.anio_inicial, parameters.anio_final)));
                            mdxParams.Add(new MdxParameter("@producto", String.Format("[Producto].[Producto General].&[{0}]", parameters.producto)));
                            string mdx1 = @"SELECT NON EMPTY {[Measures].[Ton Netas Impo],[Measures].[Valor Impo Miles CIF Dol]} ON 0,
                            NON EMPTY Filter({{[Pais].[Pais].[Pais]} * {[Producto].[Cadena].[Cadena]} * {";
                            for (int i = 0; i < parameters.partida.Count; i++)
                            {
                                mdx1 += (i + 1 == parameters.partida.Count) ? "@partida" + i : "@partida" + i + ",";
                                mdxParams.Add(new MdxParameter("@partida" + i, String.Format("[Producto].[Cadena-Partida10].[Descripcion Partida10 Dig Union].&[{0}]", parameters.partida[i])));
                            }
                            mdx1 += @"}*{ @anio }*{[Periodo].[Mes].[Mes]}},[Measures].[Ton Netas Impo] > 0) ON 1
                            FROM [Agronet Comercio] WHERE { @producto }";
                            returnData = (Table)new Table { rows = adapter.GetDataTable(connectionName, mdx1, mdxParams) };
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

        [Route("api/Report/408")]
        public IHttpActionResult postReport408(report408 parameters)
        {
            Object returnData = null;
            SQLAnalysisAdaper adapter = new SQLAnalysisAdaper();
            string connectionName = "AgronetSQLAnalysisServicesComercio";

            List<MdxParameter> mdxParams = new List<MdxParameter>();
            switch (parameters.tipo)
            {
                case "parametro":
                    Parameter parameter = new Parameter { data = new List<ParameterData>() };
                    switch (parameters.id)
                    {
                        case 1:
                            parameter.name = "anio";
                            mdxParams.Add(new MdxParameter("@anio", "[Periodo].[anho].[anho]"));
                            mdxParams.Add(new MdxParameter("~[Periodo].[anho].[anho]", "anio"));
                            string mdx1 = @"SELECT NonEmpty({{}}) ON 0, NonEmpty({ @anio }) ON 1 FROM [Agronet Comercio];";

                            DataTable data1 = adapter.GetDataTable(connectionName, mdx1, mdxParams);
                            foreach (var p in (from p in data1.AsEnumerable()
                                               select p["anio"]))
                            {
                                ParameterData param = new ParameterData { name = Convert.ToString(p).Trim(), value = Convert.ToString(p).Trim() };
                                parameter.data.Add(param);
                            }
                            returnData = (Parameter)parameter;
                            break;
                        case 2:
                            parameter.name = "cadena";
                            mdxParams.Add(new MdxParameter("@cadena", "[Producto].[Cadena].[Cadena]"));
                            mdxParams.Add(new MdxParameter("~[Producto].[Cadena].[Cadena]", "cadena"));
                            string mdx2 = @"SELECT NonEmpty({{}}) ON 0, NonEmpty({ @cadena }) ON 1 FROM [Agronet Comercio];";

                            DataTable data2 = adapter.GetDataTable(connectionName, mdx2, mdxParams);
                            foreach (var p in (from p in data2.AsEnumerable()
                                               select p["cadena"]))
                            {
                                ParameterData param = new ParameterData { name = Convert.ToString(p).Trim(), value = Convert.ToString(p).Trim() };
                                parameter.data.Add(param);
                            }
                            returnData = (Parameter)parameter;
                            break;
                        case 3:
                            parameter.name = "partida";
                            mdxParams.Add(new MdxParameter("@partida", "[Producto].[Cadena-Partida10].[Descripcion Partida10 Dig Union]"));
                            mdxParams.Add(new MdxParameter("~[Producto].[Cadena-Partida10].[Descripcion Partida10 Dig Union]", "partida"));
                            mdxParams.Add(new MdxParameter("@cadena", String.Format("[Producto].[Cadena].&[{0}]", parameters.cadena)));
                            string mdx3 = @"SELECT NonEmpty({{}}) ON 0, NonEmpty({ @partida }) ON 1 FROM [Agronet Comercio] WHERE (@cadena);";

                            DataTable data3 = adapter.GetDataTable(connectionName, mdx3, mdxParams);
                            foreach (var p in (from p in data3.AsEnumerable()
                                               select p["partida"]))
                            {
                                ParameterData param = new ParameterData { name = Convert.ToString(p).Trim(), value = Convert.ToString(p).Trim() };
                                parameter.data.Add(param);
                            }
                            returnData = (Parameter)parameter;
                            break;
                    }
                    break;
                case "grafico":
                    switch (parameters.id)
                    {
                        case 1:
                            mdxParams.Add(new MdxParameter("~[Periodo].[anho].[anho]", "anio"));
                            mdxParams.Add(new MdxParameter("~[Measures].[Ton Netas Impo]", "volumen"));
                            mdxParams.Add(new MdxParameter("~[Measures].[Valor Impo Miles CIF Dol]", "valor"));
                            mdxParams.Add(new MdxParameter("@anio", String.Format("[Periodo].[anho].&[{0}]:[Periodo].[anho].&[{1}]", parameters.anio_inicial, parameters.anio_final)));
                            mdxParams.Add(new MdxParameter("@cadena", String.Format("[Producto].[Cadena].&[{0}]", parameters.cadena)));
                            string mdx1 = @"SELECT NON EMPTY {[Measures].[Ton Netas Impo],[Measures].[Valor Impo Miles CIF Dol]} ON 0,
                            NON EMPTY Filter({ @anio },[Measures].[Ton Netas Impo] > 0) ON 1
                            FROM [Agronet Comercio] WHERE {";
                            for (int i = 0; i < parameters.partida.Count; i++)
                            {
                                mdx1 += (i + 1 == parameters.partida.Count) ? "@partida" + i : "@partida" + i + ",";
                                mdxParams.Add(new MdxParameter("@partida" + i, String.Format("[Producto].[Cadena-Partida10].[Descripcion Partida10 Dig Union].&[{0}]", parameters.partida[i])));
                            }
                            mdx1 += @"} * { @cadena }";

                            Chart chart1 = new Chart { series = new List<Series>() };

                            Series serie1 = new Series { name = "Volumen", data = new List<Data>() };
                            Series serie2 = new Series { name = "Valor", data = new List<Data>() };

                            foreach (var d in (from d in (adapter.GetDataTable(connectionName, mdx1, mdxParams)).AsEnumerable() select d))
                            {
                                Data data1 = new Data { name = Convert.ToString(d["anio"]), y = Convert.ToDouble(d["volumen"]) };
                                Data data2 = new Data { name = Convert.ToString(d["anio"]), y = Convert.ToDouble(d["valor"]) };

                                serie1.data.Add(data1);
                                serie2.data.Add(data2);
                            }
                            chart1.series.Add(serie1);
                            chart1.series.Add(serie2);

                            returnData = (Chart)chart1;
                            break;
                    }
                    break;
                case "tabla":
                    switch (parameters.id)
                    {
                        case 1:
                            mdxParams.Add(new MdxParameter("~[Periodo].[anho].[anho]", "anio"));
                            mdxParams.Add(new MdxParameter("~[Periodo].[Mes].[Mes]", "mes"));
                            mdxParams.Add(new MdxParameter("~[Pais].[Pais].[Pais]", "pais"));
                            mdxParams.Add(new MdxParameter("~[Producto].[Cadena-Partida10].[Descripcion Partida10 Dig Union]", "partida"));
                            mdxParams.Add(new MdxParameter("~[Producto].[Cadena].[Cadena]", "cadena"));
                            mdxParams.Add(new MdxParameter("~[Measures].[Ton Netas Impo]", "volumen"));
                            mdxParams.Add(new MdxParameter("~[Measures].[Valor Impo Miles CIF Dol]", "valor"));
                            mdxParams.Add(new MdxParameter("@anio", String.Format("[Periodo].[anho].&[{0}]:[Periodo].[anho].&[{1}]", parameters.anio_inicial, parameters.anio_final)));
                            mdxParams.Add(new MdxParameter("@cadena", String.Format("[Producto].[Cadena].&[{0}]", parameters.cadena)));
                            string mdx1 = @"SELECT NON EMPTY {[Measures].[Ton Netas Impo],[Measures].[Valor Impo Miles CIF Dol]} ON 0,
                            NON EMPTY Filter({{[Pais].[Pais].[Pais]} * { @cadena } * {";
                            for (int i = 0; i < parameters.partida.Count; i++)
                            {
                                mdx1 += (i + 1 == parameters.partida.Count) ? "@partida" + i : "@partida" + i + ",";
                                mdxParams.Add(new MdxParameter("@partida" + i, String.Format("[Producto].[Cadena-Partida10].[Descripcion Partida10 Dig Union].&[{0}]", parameters.partida[i])));
                            }
                            mdx1 += @"}*{ @anio }*{[Periodo].[Mes].[Mes]}},[Measures].[Ton Netas Impo] > 0) ON 1
                            FROM [Agronet Comercio]";
                            returnData = (Table)new Table { rows = adapter.GetDataTable(connectionName, mdx1, mdxParams) };
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

        [Route("api/Report/409")]
        public IHttpActionResult postReport409(report409 parameters)
        {
            Object returnData = null;
            SQLAnalysisAdaper adapter = new SQLAnalysisAdaper();
            string connectionName = "AgronetSQLAnalysisServicesComercio";

            List<MdxParameter> mdxParams = new List<MdxParameter>();
            switch (parameters.tipo)
            {
                case "parametro":
                    Parameter parameter = new Parameter { data = new List<ParameterData>() };
                    switch (parameters.id)
                    {
                        case 1:
                            parameter.name = "anio";
                            mdxParams.Add(new MdxParameter("@anio", "[Periodo].[anho].[anho]"));
                            mdxParams.Add(new MdxParameter("~[Periodo].[anho].[anho]", "anio"));
                            string mdx1 = @"SELECT NonEmpty({{}}) ON 0, NonEmpty({ @anio }) ON 1 FROM [Agronet Comercio];";

                            DataTable data1 = adapter.GetDataTable(connectionName, mdx1, mdxParams);
                            foreach (var p in (from p in data1.AsEnumerable()
                                               select p["anio"]))
                            {
                                ParameterData param = new ParameterData { name = Convert.ToString(p).Trim(), value = Convert.ToString(p).Trim() };
                                parameter.data.Add(param);
                            }
                            returnData = (Parameter)parameter;
                            break;
                    }
                    break;
                case "tabla":
                    mdxParams.Add(new MdxParameter("@anio", String.Format("[Periodo].[anho].&[{0}]:[Periodo].[anho].&[{1}]", parameters.anio_inicial, parameters.anio_final)));
                    switch (parameters.id)
                    {
                        case 1:
                            string mdx1 = @"";
                            returnData = (Table)new Table { rows = adapter.GetDataTable(connectionName, mdx1, mdxParams) };
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

        [Route("api/Report/410")]
        public IHttpActionResult postReport410(report410 parameters)
        {
            Object returnData = null;
            SQLAnalysisAdaper adapter = new SQLAnalysisAdaper();
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

        [Route("api/Report/411")]
        public IHttpActionResult postReport411(report411 parameters)
        {
            Object returnData = null;
            SQLAnalysisAdaper adapter = new SQLAnalysisAdaper();
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

        [Route("api/Report/412")]
        public IHttpActionResult postReport412(report412 parameters)
        {
            Object returnData = null;
            SQLAnalysisAdaper adapter = new SQLAnalysisAdaper();
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

        [Route("api/Report/413")]
        public IHttpActionResult postReport413(report413 parameters)
        {
            Object returnData = null;
            SQLAnalysisAdaper adapter = new SQLAnalysisAdaper();
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

        [Route("api/Report/414")]
        public IHttpActionResult postReport414(report414 parameters)
        {
            Object returnData = null;
            SQLAnalysisAdaper adapter = new SQLAnalysisAdaper();
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

        [Route("api/Report/415")]
        public IHttpActionResult postReport415(report415 parameters)
        {
            Object returnData = null;
            SQLAnalysisAdaper adapter = new SQLAnalysisAdaper();
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

        [Route("api/Report/416")]
        public IHttpActionResult postReport416(report416 parameters)
        {
            Object returnData = null;
            SQLAnalysisAdaper adapter = new SQLAnalysisAdaper();
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

        [Route("api/Report/418")]
        public IHttpActionResult postReport418(report418 parameters)
        {
            Object returnData = null;
            SQLAnalysisAdaper adapter = new SQLAnalysisAdaper();
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

        [Route("api/Report/419")]
        public IHttpActionResult postReport419(report419 parameters)
        {
            Object returnData = null;
            SQLAnalysisAdaper adapter = new SQLAnalysisAdaper();
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

        [Route("api/Report/420")]
        public IHttpActionResult postReport420(report420 parameters)
        {
            Object returnData = null;
            SQLAnalysisAdaper adapter = new SQLAnalysisAdaper();
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

        [Route("api/Report/421")]
        public IHttpActionResult postReport421(report421 parameters)
        {
            Object returnData = null;
            SQLAnalysisAdaper adapter = new SQLAnalysisAdaper();
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

        [Route("api/Report/422")]
        public IHttpActionResult postReport422(report422 parameters)
        {
            Object returnData = null;
            SQLAnalysisAdaper adapter = new SQLAnalysisAdaper();
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
