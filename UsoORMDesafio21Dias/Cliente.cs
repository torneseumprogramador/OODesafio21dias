using ORMDesafio21Dias;
using System;
using System.Collections.Generic;
using System.Text;

namespace UsoORMDesafio21Dias
{
    public class Cliente : CType
    {
        [Table(IsNotOnDataBase = true)]
        public override string ConnectionString => @"Server=localhost;Database=AulaAPI;Uid=sa;Pwd=!1#2a3d4c5g6v";
        public string Nome { get; set; }
        public string Telefone { get; set; }
        public string Endereco { get; set; }
    }
}
