using System; //Samuel Braz dos Santos
using System.Collections.Generic;
using System.Text;
using AcademiaDoZe.Domain.ValueObjects;

namespace AcademiaDoZe.Domain.Entities
{
    public sealed class Logradouro : Entity
    {
        // encapsulamento das propriedades, aplicando imutabilidade
        public Cep Cep { get; }
        public string Nome { get; }
        public string Bairro { get; }
        public string Cidade { get; }
        public string Estado { get; }
        public string Pais { get; }
        // construtor privado para evitar instância direta
        private Logradouro(int id, Cep cep, string nome, string bairro, string cidade, string estado, string pais) : base(id)
        {
            Cep = cep;
            Nome = nome;
            Bairro = bairro;
            Cidade = cidade;
            Estado = estado;
            Pais = pais;
        }
    }
}