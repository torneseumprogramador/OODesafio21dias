using ORMDesafio21Dias;
using System;
using System.Collections.Generic;
using System.Text;

namespace UsoORMDesafio21Dias
{
    public class Pessoa : CType
    {
        [Table(IsNotOnDataBase = true)]
        public override string ConnectionString => @"Server=localhost;Database=AulaAPI;Uid=sa;Pwd=!1#2a3d4c5g6v";
        public string Endereco { get; set; }
        public string Nome { get; set; }
        public string Tipo { get; set; }
        public string CpfCnpj { get; set; }
    }
}
