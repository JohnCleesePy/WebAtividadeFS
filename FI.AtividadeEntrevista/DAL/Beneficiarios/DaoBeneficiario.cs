using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace FI.AtividadeEntrevista.DAL
{
    /// <summary>
    /// Classe de acesso a dados de Beneficiários.
    /// </summary>
    internal class DaoBeneficiario : AcessoDados
    {
        /// <summary>
        /// Inclui um novo beneficiário.
        /// </summary>
        /// <param name="beneficiario">Objeto de beneficiário.</param>
        internal long Incluir(DML.Beneficiario beneficiario)
        {
            try
            {
                var parametros = CriarParametrosComuns(beneficiario);
                DataSet dataSet = base.Consultar("FI_SP_IncBeneficiario", parametros);

                if (dataSet.Tables[0].Rows.Count > 0 && long.TryParse(dataSet.Tables[0].Rows[0][0].ToString(), out long result))
                {
                    return result;
                }

                return 0;
            }
            catch (Exception ex)
            {
                // Aqui você pode logar a exceção ou tratá-la de acordo com a necessidade
                throw new Exception("Erro ao incluir beneficiário.", ex);
            }
        }

        /// <summary>
        /// Lista todos os beneficiários de um cliente.
        /// </summary>
        /// <param name="idCliente">Id do cliente.</param>
        internal List<DML.Beneficiario> Listar(long idCliente)
        {
            try
            {
                var parametros = new List<SqlParameter>
                {
                    new SqlParameter("IdCliente", idCliente)
                };

                DataSet dataSet = base.Consultar("FI_SP_ConsBeneficiario", parametros);

                return ConverterParaBeneficiarios(dataSet);
            }
            catch (Exception ex)
            {
                throw new Exception("Erro ao listar beneficiários.", ex);
            }
        }

        /// <summary>
        /// Altera um beneficiário.
        /// </summary>
        /// <param name="beneficiario">Objeto de beneficiário.</param>
        internal void Alterar(DML.Beneficiario beneficiario)
        {
            try
            {
                var parametros = CriarParametrosComuns(beneficiario);
                parametros.Add(new SqlParameter("ID", beneficiario.Id));

                base.Executar("FI_SP_AltBeneficiario", parametros);
            }
            catch (Exception ex)
            {
                throw new Exception("Erro ao alterar beneficiário.", ex);
            }
        }

        /// <summary>
        /// Exclui um beneficiário.
        /// </summary>
        /// <param name="Id">Id do beneficiário.</param>
        internal void Excluir(long Id)
        {
            try
            {
                var parametros = new List<SqlParameter>
                {
                    new SqlParameter("Id", Id)
                };

                base.Executar("FI_SP_DelBeneficiario", parametros);
            }
            catch (Exception ex)
            {
                throw new Exception("Erro ao excluir beneficiário.", ex);
            }
        }

        /// <summary>
        /// Converte o DataSet em uma lista de beneficiários.
        /// </summary>
        /// <param name="dataSet">DataSet com os dados de beneficiários.</param>
        private List<DML.Beneficiario> ConverterParaBeneficiarios(DataSet dataSet)
        {
            var listaBeneficiarios = new List<DML.Beneficiario>();

            if (dataSet?.Tables != null && dataSet.Tables.Count > 0 && dataSet.Tables[0].Rows.Count > 0)
            {
                foreach (DataRow row in dataSet.Tables[0].Rows)
                {
                    var beneficiario = new DML.Beneficiario
                    {
                        Id = row.Field<long>("Id"),
                        Nome = row.Field<string>("Nome"),
                        CPF = row.Field<string>("CPF"),
                        IdCliente = row.Field<long>("IdCliente")
                    };

                    listaBeneficiarios.Add(beneficiario);
                }
            }

            return listaBeneficiarios;
        }

        /// <summary>
        /// Cria uma lista de parâmetros SQL comuns para operações com beneficiário.
        /// </summary>
        /// <param name="beneficiario">Objeto de beneficiário.</param>
        private List<SqlParameter> CriarParametrosComuns(DML.Beneficiario beneficiario)
        {
            return new List<SqlParameter>
            {
                new SqlParameter("Nome", beneficiario.Nome),
                new SqlParameter("CPF", beneficiario.CPF),
                new SqlParameter("IdCliente", beneficiario.IdCliente)
            };
        }
    }
}
