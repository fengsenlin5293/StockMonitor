using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Structures.Attibutes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class ColumnDescriptionAttribute : Attribute
    {
        public ColumnDescriptionAttribute(string columnHeaderName, string columnPropertyName)
        {
            this.ColumnHeaderName = columnHeaderName;
            this.ColumnPropertyName = columnPropertyName;
        }
        public string ColumnHeaderName { get; private set; }

        public string ColumnPropertyName { get; private set; }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class ColumnCollectionAttribute : Attribute
    {

    }
}
