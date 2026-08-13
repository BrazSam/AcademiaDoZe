using AcademiaDoZe.Domain.Entities;
using System; //Samuel Braz dos Santos
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace AcademiaDoZe.Domain.ValueObjects
{
    public record Endereco
    {
        public Logradouro Logradouro { get; }
        public string Numero { get; }
        public string Complemento { get; }
        private Endereco(Logradouro logradouro, string numero, string complemento)
        {
            Logradouro = logradouro;
            Numero = numero;
            Complemento = complemento;
        }

        // metodo de fabrica
        public static Endereco Criar(string logradouro, string numero, string bairro, string cidade, string estado, Cep cep)
        {
            // validacoes e normalizacoes
            if (string.IsNullOrWhiteSpace(logradouro))
                throw new Exception("LOGRADOURO_OBRIGATORIO");

            if (string.IsNullOrWhiteSpace(numero))
                throw new Exception("NUMERO_OBRIGATORIO");

            if (string.IsNullOrWhiteSpace(bairro))
                throw new Exception("BAIRRO_OBRIGATORIO");

            if (string.IsNullOrWhiteSpace(cidade))
                throw new Exception("CIDADE_OBRIGATORIO");

            if (string.IsNullOrWhiteSpace(estado))
                throw new Exception("ESTADO_OBRIGATORIO");

            if (cep == null)
                throw new Exception("CEP_OBRIGATORIO");

            // criacao e retorno do objeto
            return new Endereco(logradouro.Trim(), numero.Trim(), bairro.Trim(), cidade.Trim(), estado.Trim().ToUpper(), cep);
        }
    }
}