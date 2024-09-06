using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace WebAtividadeEntrevista.Models
{
    /// <summary>
    /// Classe de Modelo de Cliente
    /// </summary>
    public class BeneficiariosModel
    {
        private string _cpf;

        public long? Id { get; set; }

        /// <summary>
        /// Nome
        /// </summary>
        [Required]
        public string Nome { get; set; }

        /// <summary>
        /// CPF
        /// </summary>
        [Required]
        [ValidateCPF]
        public string CPF
        {
            get => _cpf;
            set => _cpf = Regex.Replace(value, @"\D", "");
        }
    }
}