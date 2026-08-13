using System; //Samuel Braz dos Santos
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace AcademiaDoZe.Domain.ValueObjects
{
    public record Telefone
    {
        public string Valor { get; }
        private Telefone(string valor)
        {
            Valor = valor;
        }

        // metodo de fabrica
        public static Telefone Criar(string valor)
        {
            // validacoes e normalizacoes
            if (string.IsNullOrWhiteSpace(valor))
                throw new Exception("TELEFONE_OBRIGATORIO");

            // remove parenteses, tracos e espacos
            string telLimpo = Regex.Replace(valor, @"[^\d]", "");

            // telefone deve ter 10 (fixo) ou 11 (celular com ddd) digitos
            if (telLimpo.Length < 10 || telLimpo.Length > 11)
                throw new Exception("TELEFONE_INVALIDO");

            // criacao e retorno do objeto
            return new Telefone(telLimpo);
        }

        //polimorfismo
        public override string ToString() => Valor;
    }
}