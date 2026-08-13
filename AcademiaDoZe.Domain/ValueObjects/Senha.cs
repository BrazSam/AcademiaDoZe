using System; //Samuel Braz dos Santos
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using AcademiaDoZe.Domain.Common;

namespace AcademiaDoZe.Domain.ValueObjects
{
    public record Senha
    {
        public string Valor { get; }
        private Senha(string valor)
        {
            Valor = valor;
        }

        public static Result<Senha> Criar(string valor)
        {
            if (string.IsNullOrWhiteSpace(valor))
                return Result<Senha>.Failure("Senha", "SENHA_OBRIGATORIO");

            if (valor.Length < 6)
                return Result<Senha>.Failure("Senha", "SENHA_INVALIDA");

            return Result<Senha>.Success(new Senha(valor));
        }

        public override string ToString() => Valor;
    }
}