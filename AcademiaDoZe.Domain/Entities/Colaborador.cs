using AcademiaDoZe.Domain.Enums; //Samuel Braz dos Santos
using System;
using System.Collections.Generic;
using System.Text;
using AcademiaDoZe.Domain.ValueObjects;

namespace AcademiaDoZe.Domain.Entities
{
    public class Colaborador : Pessoa
    {
        // encapsulamento das propriedades, aplicando imutabilidade
        public DateOnly DataAdmissao { get; private set; }
        public ColaboradorTipo Tipo { get; private set; }
        public ColaboradorVinculo Vinculo { get; private set; }
        // construtor privado para evitar instância direta
        private Colaborador(int id, string nome, Cpf cpf,

        DateOnly dataNascimento, Telefone telefone,
        Email email, Endereco endereco,
        Senha senha, Arquivo foto,
        DateOnly dataAdmissao,
        ColaboradorTipo tipo,
        ColaboradorVinculo vinculo)

        : base(id, nome, cpf, dataNascimento, telefone, email, endereco, senha, foto)
        {
            DataAdmissao = dataAdmissao;
            Tipo = tipo;
            Vinculo = vinculo;
        }
    }
}
