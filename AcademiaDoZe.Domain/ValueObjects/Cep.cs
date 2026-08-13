using System; //Samuel Braz dos Santos
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace AcademiaDoZe.Domain.ValueObjects
{
    public record Cep
    {
        public string Valor { get; }
        private Cep(string valor)
        {
            Valor = valor;
        }

        // metodo de fabrica
        public static Cep Criar(string valor)
        {
            // validacoes e normalizacoes
            if (string.IsNullOrWhiteSpace(valor))
                throw new Exception("CEP_OBRIGATORIO");

            // remove tracos e espacos
            string cepLimpo = Regex.Replace(valor, @"[^\d]", "");

            // cep deve conter exatamente 8 digitos
            if (cepLimpo.Length != 8)
                throw new Exception("CEP_INVALIDO");

            // criacao e retorno do objeto
            return new Cep(cepLimpo);
        }

        //polimorfismo
        public override string ToString() => Valor;
    }
}