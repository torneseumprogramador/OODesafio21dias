using ORMDesafio21Dias;
using System;
using System.Collections.Generic;

namespace UsoORMDesafio21Dias
{
    class Program
    {
        static void Main(string[] args)
        {
            Service.DropTable<Pessoa>();
            Service.CreateTable<Pessoa>();

            new Pessoa() { Nome = "Danilo" }.Save();

            var pessoas = Service.All<Pessoa>();

            foreach (var pessoa in pessoas)
            {
                Console.WriteLine(pessoa.Nome);
            }

            Console.WriteLine("=================");

            var clientes = Service.All<Cliente>();

            foreach(var cliente in clientes)
            {
                Console.WriteLine(cliente.Nome);
            }

            Console.ReadLine();
        }
    }
}
