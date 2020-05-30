using System;
using System.Collections.Generic;
using System.Text;

namespace ORMDesafio21Dias
{
    public class TableAttribute: Attribute
    {
        public string Name { set; get; }
        public string PrimaryKey { set; get; }
        public string Collection { set; get; }
        public string ForeignKey { set; get; }
        public bool IsNotOnDataBase { set; get; }
    }
}
