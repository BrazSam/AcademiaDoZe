using System; //Samuel Braz dos Santos
using System.Collections.Generic;
using System.Text;
using AcademiaDoZe.Domain.Enums;
using AcademiaDoZe.Domain.ValueObjects;

namespace AcademiaDoZe.Domain.Entities
{
    public class Aluno : Pessoa
    {
        // construtor privado para evitar instância direta
        private Aluno(int id, string nome,

        Cpf cpf,
        DateOnly dataNascimento,
        Telefone telefone,
        Email email,
        Endereco endereco,
        Senha senha,
        Arquivo foto)

        : base(id, nome, cpf, dataNascimento, telefone, email, endereco, senha, foto)
        {
        }
    }