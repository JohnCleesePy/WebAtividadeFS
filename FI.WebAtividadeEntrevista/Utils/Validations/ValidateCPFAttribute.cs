using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.RegularExpressions;

public class ValidateCPFAttribute : ValidationAttribute
{
    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        string cpf = value.ToString();

        if (!Regex.IsMatch(MascararCPF(cpf), @"^\d{3}\.\d{3}\.\d{3}-\d{2}$"))
        {
            return new ValidationResult("CPF deve estar no formato 999.999.999-99");
        }

        cpf = cpf.Replace(".", "").Replace("-", "");

        if (new string(cpf[0], cpf.Length) == cpf)
        {
            return new ValidationResult("CPF inválido");
        }

        if (cpf.Length != 11)
        {
            return new ValidationResult("CPF deve conter 11 dígitos");
        }

        if (!ValidarDigitosCPF(cpf))
        {
            return new ValidationResult("CPF inválido");
        }

        return ValidationResult.Success;
    }

    private bool ValidarDigitosCPF(string cpf)
    {
        int[] multiplicador1 = new int[9] { 10, 9, 8, 7, 6, 5, 4, 3, 2 };
        int[] multiplicador2 = new int[10] { 11, 10, 9, 8, 7, 6, 5, 4, 3, 2 };

        string tempCpf = cpf.Substring(0, 9);
        int soma = 0;

        for (int i = 0; i < 9; i++)
        {
            soma += int.Parse(tempCpf[i].ToString()) * multiplicador1[i];
        }

        int resto = soma % 11;
        if (resto < 2)
        {
            resto = 0;
        }
        else
        {
            resto = 11 - resto;
        }

        string digito = resto.ToString();
        tempCpf = tempCpf + digito;
        soma = 0;

        for (int i = 0; i < 10; i++)
        {
            soma += int.Parse(tempCpf[i].ToString()) * multiplicador2[i];
        }

        resto = soma % 11;
        if (resto < 2)
        {
            resto = 0;
        }
        else
        {
            resto = 11 - resto;
        }

        digito = digito + resto.ToString();

        return cpf.EndsWith(digito);
    }

    public static string MascararCPF(string cpf)
    {
        cpf = new string(cpf.Where(char.IsDigit).ToArray());

        return Convert.ToUInt64(cpf).ToString(@"000\.000\.000\-00");
    }
}
