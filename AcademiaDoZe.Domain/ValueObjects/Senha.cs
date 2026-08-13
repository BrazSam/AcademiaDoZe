using System; //Samuel Braz dos Santos
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace AcademiaDoZe.Domain.ValueObjects
{
    public record Senha
    {
        public string Valor { get; }
        private Senha(string valor)
        {
            Valor = valor;
        }

        // metodo de fabrica
        public static Senha Criar(string valor)
        {
            // validacoes e normalizacoes
            if (string.IsNullOrWhiteSpace(valor))
                throw new Exception("SENHA_OBRIGATORIO");

            if (valor.Length < 6)
                throw new Exception("SENHA_INVALIDA");

            // criacao e retorno do objeto
            return new Senha(valor);
        }

        public override string ToString() => Valor;
    }
}