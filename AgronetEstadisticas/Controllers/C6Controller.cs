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
    public class C6Controller : ApiController
    {
        [Route("api/Report/601")]
        public IHttpActionResult postReport601(report601 parameters)
        {
            Object returnData = null;
            SQLAnalysisAdaper adapter = new SQLAnalysisAdaper();
            string connectionName = "AgronetSQLAnalysisServicesCredito";

            List<MdxParameter> mdxParams = new List<MdxParameter>();
            switch (parameters.tipo)
            {
                case "parametro":
                    Parameter parameter = new Parameter { data = new List<ParameterData>() };
                    switch (parameters.id)
                    {
                        case 1:
                            parameter.name = "anio";
                            mdxParams.Add(new MdxParameter("@anio", "[Periodo].[Anho].[Anho]"));
                            mdxParams.Add(new MdxParameter("~[Periodo].[Anho].[Anho]", "anio"));
                            string mdx1 = @"SELECT NonEmpty({{}}) ON 0, NonEmpty({ @anio }) ON 1 FROM [Agronet Credito DW];";

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
                case "grafico":
                    switch (parameters.id)
                    {
                        case 1:
                            mdxParams.Add(new MdxParameter("~[Measures].[Valor Millones Pesos]", "valor"));
                            mdxParams.Add(new MdxParameter("~[Geografia].[Departamento].[Departamento]", "departamento"));
                            mdxParams.Add(new MdxParameter("@anio", String.Format("[Periodo].[Anho].&[{0}]:[Periodo].[Anho].&[{1}]", parameters.anio_inicial, parameters.anio_final)));
                            
                            string mdx1 = @"SELECT {[Measures].[Valor Millones Pesos]} ON 0,
                            NON EMPTY TopCount({[Geografia].[Departamento].[Departamento]},10,[Measures].[Valor Millones Pesos])  ON 1
                            FROM [Agronet Credito DW] WHERE [Intermediario Financiero].[Intermediario Financiero].&[40] * { @anio }";
                            Chart chart1 = new Chart { series = new List<Series>() };

                            Series serie1 = new Series { name = "Participación Acumulada por Departamento", data = new List<Data>() };

                            foreach (var d in (from d in (adapter.GetDataTable(connectionName, mdx1, mdxParams)).AsEnumerable() select d))
                            {
                                Data data1 = new Data { name = Convert.ToString(d["departamento"]), y = Convert.ToDouble(d["valor"]) };
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
                            mdxParams.Add(new MdxParameter("~[Periodo].[Anho].[Anho]", "anio"));
                            mdxParams.Add(new MdxParameter("~[Measures].[Valor Millones Pesos]", "valor"));
                            mdxParams.Add(new MdxParameter("~[Geografia].[Departamento].[Departamento]", "departamento"));
                            mdxParams.Add(new MdxParameter("@anio", String.Format("[Periodo].[Anho].&[{0}]:[Periodo].[Anho].&[{1}]", parameters.anio_inicial, parameters.anio_final)));
                            
                            string mdx1 = @"SELECT {[Measures].[Valor Millones Pesos]} ON 0,
                            NON EMPTY TopCount({[Geografia].[Departamento].[Departamento]},10,[Measures].[Valor Millones Pesos]) * { @anio } ON 1
                            FROM [Agronet Credito DW] WHERE [Intermediario Financiero].[Intermediario Financiero].&[40]";
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

        [Route("api/Report/602")]
        public IHttpActionResult postReport602(report602 parameters)
        {
            Object returnData = null;
            SQLAnalysisAdaper adapter = new SQLAnalysisAdaper();
            string connectionName = "AgronetSQLAnalysisServicesCredito";

            List<MdxParameter> mdxParams = new List<MdxParameter>();
            switch (parameters.tipo)
            {
                case "parametro":
                    Parameter parameter = new Parameter { data = new List<ParameterData>() };
                    switch (parameters.id)
                    {
                        case 1:
                            parameter.name = "anio";
                            mdxParams.Add(new MdxParameter("@anio", "[Periodo].[Anho].[Anho]"));
                            mdxParams.Add(new MdxParameter("~[Periodo].[Anho].[Anho]", "anio"));
                            string mdx1 = @"SELECT NonEmpty({{}}) ON 0, NonEmpty({ @anio }) ON 1 FROM [Agronet Credito DW];";

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
                case "grafico":
                    switch (parameters.id)
                    {
                        case 1:
                            mdxParams.Add(new MdxParameter("~[Measures].[Valor Millones Pesos]", "valor"));
                            mdxParams.Add(new MdxParameter("~[Geografia].[Departamento].[Departamento]", "departamento"));
                            mdxParams.Add(new MdxParameter("@anio", String.Format("[Periodo].[Anho].&[{0}]:[Periodo].[Anho].&[{1}]", parameters.anio_inicial, parameters.anio_final)));

                            string mdx1 = @"SELECT NON EMPTY {[Measures].[Valor Millones Pesos]} ON 0,
                            NON EMPTY TopCount({[Geografia].[Departamento].[Departamento]},10,[Measures].[Valor Millones Pesos]) ON 1
                            FROM [Agronet Credito DW]
                            WHERE {[Intermediario Financiero].[Intermediario Financiero].&[40]} * 
                            {[Tipo Productor].[Tipo de Productor].[Descripcion Min Tipo Productor].&[Pequeños Productores]} * { @anio }";
                            Chart chart1 = new Chart { series = new List<Series>() };

                            Series serie1 = new Series { name = "Participación Acumulada por Departamento", data = new List<Data>() };

                            foreach (var d in (from d in (adapter.GetDataTable(connectionName, mdx1, mdxParams)).AsEnumerable() select d))
                            {
                                Data data1 = new Data { name = Convert.ToString(d["departamento"]), y = Convert.ToDouble(d["valor"]) };
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
                            mdxParams.Add(new MdxParameter("~[Periodo].[Anho].[Anho]", "anio"));
                            mdxParams.Add(new MdxParameter("~[Measures].[Valor Millones Pesos]", "valor"));
                            mdxParams.Add(new MdxParameter("~[Geografia].[Departamento].[Departamento]", "departamento"));
                            mdxParams.Add(new MdxParameter("@anio", String.Format("[Periodo].[Anho].&[{0}]:[Periodo].[Anho].&[{1}]", parameters.anio_inicial, parameters.anio_final)));
                            
                            string mdx1 = @"SELECT NON EMPTY {[Measures].[Valor Millones Pesos]} ON 0,
                            NON EMPTY TopCount({[Geografia].[Departamento].[Departamento]},10,[Measures].[Valor Millones Pesos]) * { @anio } ON 1
                            FROM [Agronet Credito DW]
                            WHERE {[Intermediario Financiero].[Intermediario Financiero].&[40]} * 
                            {[Tipo Productor].[Tipo de Productor].[Descripcion Min Tipo Productor].&[Pequeños Productores]}";
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

        [Route("api/Report/603")]
        public IHttpActionResult postReport603(report603 parameters)
        {
            Object returnData = null;
            SQLAnalysisAdaper adapter = new SQLAnalysisAdaper();
            string connectionName = "AgronetSQLAnalysisServicesCredito";

            List<MdxParameter> mdxParams = new List<MdxParameter>();
            switch (parameters.tipo)
            {
                case "parametro":
                    Parameter parameter = new Parameter { data = new List<ParameterData>() };
                    switch (parameters.id)
                    {
                        case 1:
                            parameter.name = "anio";
                            mdxParams.Add(new MdxParameter("@anio", "[Periodo].[Anho].[Anho]"));
                            mdxParams.Add(new MdxParameter("~[Periodo].[Anho].[Anho]", "anio"));
                            string mdx1 = @"SELECT NonEmpty({{}}) ON 0, NonEmpty({ @anio }) ON 1 FROM [Agronet Credito DW];";

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
                            parameter.name = "departamento";
                            mdxParams.Add(new MdxParameter("@departamento", "[Geografia].[Departamento].[Departamento]"));
                            mdxParams.Add(new MdxParameter("~[Geografia].[Departamento].[Departamento]", "departamento"));
                            string mdx2 = @"SELECT NonEmpty({{}}) ON 0, NonEmpty({ @departamento }) ON 1 FROM [Agronet Credito DW];";

                            DataTable data2 = adapter.GetDataTable(connectionName, mdx2, mdxParams);
                            foreach (var p in (from p in data2.AsEnumerable()
                                               select p["departamento"]))
                            {
                                ParameterData param = new ParameterData { name = Convert.ToString(p).Trim(), value = Convert.ToString(p).Trim() };
                                parameter.data.Add(param);
                            }
                            returnData = (Parameter)parameter;
                            break;
                        case 3:
                            parameter.name = "linea";
                            mdxParams.Add(new MdxParameter("@linea", "[Rubro].[Linea].[Linea]"));
                            mdxParams.Add(new MdxParameter("~[Rubro].[Linea].[Linea]", "linea"));
                            string mdx3 = @"SELECT NonEmpty({{}}) ON 0, NonEmpty({ @linea }) ON 1 FROM [Agronet Credito DW];";

                            DataTable data3 = adapter.GetDataTable(connectionName, mdx3, mdxParams);
                            foreach (var p in (from p in data3.AsEnumerable()
                                               select p["linea"]))
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
                            mdxParams.Add(new MdxParameter("~[Periodo].[Anho].[Anho]", "anio"));
                            mdxParams.Add(new MdxParameter("~[Measures].[Valor Millones Pesos]", "valor"));
                            mdxParams.Add(new MdxParameter("~[Measures].[Numero Créditos]", "creditos"));
                            mdxParams.Add(new MdxParameter("~[Rubro].[Linea].[Linea]", "linea"));
                            mdxParams.Add(new MdxParameter("~[Rubro].[Grupo].[Grupo]", "grupo"));
                            mdxParams.Add(new MdxParameter("~[Rubro].[Subgrupo].[Subgrupo]", "subgrupo"));
                            mdxParams.Add(new MdxParameter("~[Rubro].[Rubro].[Rubro]", "rubro"));
                            mdxParams.Add(new MdxParameter("@anio", String.Format("[Periodo].[Anho].&[{0}]:[Periodo].[Anho].&[{1}]", parameters.anio_inicial, parameters.anio_final)));
                            mdxParams.Add(new MdxParameter("@linea", String.Format("[Rubro].[Linea].[{0}]", parameters.linea)));
                            mdxParams.Add(new MdxParameter("@departamento", String.Format("[Geografia].[Departamento].[{0}]", parameters.departamento)));
                            
                            String mdx1 = @"SELECT NON EMPTY {[Measures].[Numero Créditos], [Measures].[Valor Millones Pesos]} ON 0, 
                                            NonEmpty({ @linea }* 
                                            { [Rubro].[Grupo].[Grupo] }* 
                                            { [Rubro].[Subgrupo].[Subgrupo] }* 
                                            { [Rubro].[Rubro].[Rubro] }* 
                                            { @anio }) ON 1 
                                            FROM [Agronet Credito DW] WHERE { @departamento };";

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

        [Route("api/Report/604")]
        public IHttpActionResult postReport604(report604 parameters)
        {
            Object returnData = null;
            SQLAnalysisAdaper adapter = new SQLAnalysisAdaper();
            string connectionName = "AgronetSQLAnalysisServicesCredito";

            List<MdxParameter> mdxParams = new List<MdxParameter>();
            switch (parameters.tipo)
            {
                case "parametro":
                    Parameter parameter = new Parameter { data = new List<ParameterData>() };
                    switch (parameters.id)
                    {
                        case 1:
                            parameter.name = "anio";
                            mdxParams.Add(new MdxParameter("@anio", "[Periodo].[Anho].[Anho]"));
                            mdxParams.Add(new MdxParameter("~[Periodo].[Anho].[Anho]", "anio"));
                            string mdx1 = @"SELECT NonEmpty({{}}) ON 0, NonEmpty({ @anio }) ON 1 FROM [Agronet Credito DW];";

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
                case "grafico":
                    switch (parameters.id)
                    {
                        case 1:
                            mdxParams.Add(new MdxParameter("~[Measures].[Valor Millones Pesos]", "valor"));
                            mdxParams.Add(new MdxParameter("~[Geografia].[Departamento].[Departamento]", "departamento"));
                            mdxParams.Add(new MdxParameter("@anio", String.Format("[Periodo].[Anho].&[{0}]:[Periodo].[Anho].&[{1}]", parameters.anio_inicial, parameters.anio_final)));

                            string mdx1 = @"SELECT {[Measures].[Valor Millones Pesos]} ON 0,
                            NON EMPTY {ORDER([Geografia].[Departamento].[Departamento],[Measures].[Valor Millones Pesos],DESC)} ON 1 
                            FROM [Agronet Credito DW] WHERE {[Tipo Credito].[Tipo Credito].&[A]} * { @anio }";
                            Chart chart1 = new Chart { series = new List<Series>() };

                            Series serie1 = new Series { name = "Participación Acumulada por Departamento", data = new List<Data>() };

                            foreach (var d in (from d in (adapter.GetDataTable(connectionName, mdx1, mdxParams)).AsEnumerable() select d))
                            {
                                Data data1 = new Data { name = Convert.ToString(d["departamento"]), y = Convert.ToDouble(d["valor"]) };
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
                            mdxParams.Add(new MdxParameter("~[Periodo].[Anho].[Anho]", "anio"));
                            mdxParams.Add(new MdxParameter("~[Measures].[Valor Millones Pesos]", "valor"));
                            mdxParams.Add(new MdxParameter("~[Geografia].[Departamento].[Departamento]", "departamento"));
                            mdxParams.Add(new MdxParameter("@anio", String.Format("[Periodo].[Anho].&[{0}]:[Periodo].[Anho].&[{1}]", parameters.anio_inicial, parameters.anio_final)));

                            string mdx1 = @"SELECT {[Measures].[Valor Millones Pesos]} ON 0,
                            NON EMPTY {ORDER([Geografia].[Departamento].[Departamento],[Measures].[Valor Millones Pesos],DESC)} * { @anio } ON 1
                            FROM [Agronet Credito DW] WHERE [Tipo Credito].[Tipo Credito].&[A]";
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

        [Route("api/Report/605")]
        public IHttpActionResult postReport605(report605 parameters)
        {
            Object returnData = null;
            SQLAnalysisAdaper adapter = new SQLAnalysisAdaper();
            string connectionName = "AgronetSQLAnalysisServicesCredito";

            List<MdxParameter> mdxParams = new List<MdxParameter>();
            switch (parameters.tipo)
            {
                case "parametro":
                    Parameter parameter = new Parameter { data = new List<ParameterData>() };
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

        [Route("api/Report/606")]
        public IHttpActionResult postReport606(report606 parameters)
        {
            Object returnData = null;
            SQLAnalysisAdaper adapter = new SQLAnalysisAdaper();
            string connectionName = "AgronetSQLAnalysisServicesCredito";

            List<MdxParameter> mdxParams = new List<MdxParameter>();
            switch (parameters.tipo)
            {
                case "parametro":
                    Parameter parameter = new Parameter { data = new List<ParameterData>() };
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

        [Route("api/Report/607")]
        public IHttpActionResult postReport607(report607 parameters)
        {
            Object returnData = null;
            SQLAnalysisAdaper adapter = new SQLAnalysisAdaper();
            string connectionName = "AgronetSQLAnalysisServicesCredito";

            List<MdxParameter> mdxParams = new List<MdxParameter>();
            switch (parameters.tipo)
            {
                case "parametro":
                    Parameter parameter = new Parameter { data = new List<ParameterData>() };
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

        [Route("api/Report/608")]
        public IHttpActionResult postReport608(report608 parameters)
        {
            Object returnData = null;
            SQLAnalysisAdaper adapter = new SQLAnalysisAdaper();
            string connectionName = "AgronetSQLAnalysisServicesCredito";

            List<MdxParameter> mdxParams = new List<MdxParameter>();
            switch (parameters.tipo)
            {
                case "parametro":
                    Parameter parameter = new Parameter { data = new List<ParameterData>() };
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

        [Route("api/Report/609")]
        public IHttpActionResult postReport609(report609 parameters)
        {
            Object returnData = null;
            SQLAnalysisAdaper adapter = new SQLAnalysisAdaper();
            string connectionName = "AgronetSQLAnalysisServicesCredito";

            List<MdxParameter> mdxParams = new List<MdxParameter>();
            switch (parameters.tipo)
            {
                case "parametro":
                    Parameter parameter = new Parameter { data = new List<ParameterData>() };
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