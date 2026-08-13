using System; //Samuel Braz dos Santos
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace AcademiaDoZe.Domain.ValueObjects
{
    public record Cpf
    {
        public string Valor { get; }
        private Cpf(string valor)
        {
            Valor = valor;
        }

        // metodo de fabrica
        public static Cpf Criar(string valor)
        {
            // validacoes e normalizacoes
            if (string.IsNullOrWhiteSpace(valor))
                throw new Exception("CPF_OBRIGATORIO");

            // remove pontos, tracos e espacos
            string cpfLimpo = Regex.Replace(valor, @"[^\d]", "");

            if (!EValido(cpfLimpo))
                throw new Exception("CPF_INVALIDO");

            // criacao e retorno do objeto
            return new Cpf(cpfLimpo);
        }

        // validacao simplificada dos digitos
        private static bool EValido(string cpf)
        {
            // precisa ter 11 digitos
            if (cpf.Length != 11) return false;

            // rejeita cpfs com numeros todos iguais
            if (cpf.Distinct().Count() == 1) return false;

            // calculo do primeiro digito
            int soma = 0;
            for (int i = 0; i < 9; i++)
                soma += (cpf[i] - '0') * (10 - i);

            int resto = (soma * 10) % 11;
            if (resto == 10 || resto == 11) resto = 0;
            if (resto != (cpf[9] - '0')) return false;

            // calculo do segundo digito
            soma = 0;
            for (int i = 0; i < 10; i++)
                soma += (cpf[i] - '0') * (11 - i);

            resto = (soma * 10) % 11;
            if (resto == 10 || resto == 11) resto = 0;
            if (resto != (cpf[10] - '0')) return false;

            return true;
        }

    }
}
