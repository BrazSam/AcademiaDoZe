using System; //Samuel Braz dos Santos
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace AcademiaDoZe.Domain.ValueObjects
{
    public record Arquivo
    {
        public byte[] Conteudo { get; }
        private Arquivo(byte[] conteudo)
        {
            Conteudo = conteudo;
        }


        // metodo de fabrica
        public static Arquivo Criar(byte[] bytes)
        {
            // validacoes e normalizacoes
            if (bytes == null || bytes.Length == 0)
                throw new Exception("ARQUIVO_OBRIGATORIO");

            // criacao e retorno do objeto
            return new Arquivo(bytes);
        }
    }
}