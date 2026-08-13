using System;
using System.Collections.Generic;
using System.Text;

namespace AcademiaDoZe.Domain.Exceptions;
// classe base para exceções de domínio
// permitindo exceções específicas de regras de negócio
// uso de construtor primário para simplificar a criação de exceções com mensagem
// sealed para evitar herança adicional, mantendo a hierarquia de exceções clara
public sealed class DomainException(string message) : Exception(message) 
{
}
