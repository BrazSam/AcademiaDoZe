using System.ComponentModel.DataAnnotations; //Samuel Braz dos Santos
namespace AcademiaDoZe.Application.Enums;

public enum AppAcessoTipo
{
    [Display(Name = "Entrada")] Entrada = 0,
    [Display(Name = "Saída")] Saida = 1
}