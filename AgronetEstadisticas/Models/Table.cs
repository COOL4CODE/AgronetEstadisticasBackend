using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;

namespace AgronetEstadisticas.Models
{
    public class Table
    {
        public List<Column> columns { get; set; }
        public List<ColumnGroup> columnsgroups { get; set; }
        public DataTable rows { get; set; }
    }

    public class Column
    {
        public string text { get; set; }
        public string datafield { get; set; }
        public string columngroup { get; set; }
    }

    public class ColumnGroup
    {
        public string text { get; set; }
        public string name { get; set; }
    }

}