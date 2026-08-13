using System; //Samuel Braz dos Santos
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace AcademiaDoZe.Domain.ValueObjects
{
    public record Email
    {
        public string Valor { get; }
        private Email(string valor)
        {
            Valor = valor;
        }

        // metodo de fabrica
        public static Email Criar(string valor)
        {
            // validacoes e normalizacoes
            if (string.IsNullOrWhiteSpace(valor))
                throw new Exception("EMAIL_OBRIGATORIO");

            string emailLimpo = valor.Trim().ToLower();

            // expressao regular para validar formato de email
            string padrao = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            if (!Regex.IsMatch(emailLimpo, padrao))
                throw new Exception("EMAIL_INVALIDO");

            // criacao e retorno do objeto
            return new Email(emailLimpo);
        }

        public override string ToString() => Valor;
    }
}