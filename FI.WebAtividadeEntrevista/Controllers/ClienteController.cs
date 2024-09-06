using FI.AtividadeEntrevista.BLL;
using WebAtividadeEntrevista.Models;
using System;
using System.Linq;
using System.Web.Mvc;
using FI.AtividadeEntrevista.DML;

namespace WebAtividadeEntrevista.Controllers
{
    public class ClienteController : Controller
    {
        private readonly BoCliente _boCliente;
        private readonly BoBeneficiario _boBeneficiario;

        public ClienteController()
        {
            _boCliente = new BoCliente();
            _boBeneficiario = new BoBeneficiario();
        }

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Incluir()
        {
            return View();
        }

        [HttpPost]
        public JsonResult Incluir(ClienteModel model)
        {
            return ProcessarInclusaoOuAlteracao(model, isInclusao: true);
        }

        [HttpPost]
        public JsonResult Alterar(ClienteModel model)
        {
            return ProcessarInclusaoOuAlteracao(model, isInclusao: false);
        }

        [HttpGet]
        public ActionResult Alterar(long id)
        {
            var cliente = _boCliente.Consultar(id);
            if (cliente == null) return HttpNotFound();

            var model = MapearClienteParaModel(cliente);
            return View(model);
        }

        [HttpPost]
        public JsonResult ClienteList(int jtStartIndex = 0, int jtPageSize = 0, string jtSorting = null)
        {
            try
            {
                var (sortField, sortAscending) = ProcessarOrdenacao(jtSorting);
                var clientes = _boCliente.Pesquisa(jtStartIndex, jtPageSize, sortField, sortAscending, out int totalRecords);

                return Json(new { Result = "OK", Records = clientes, TotalRecordCount = totalRecords });
            }
            catch (Exception ex)
            {
                return Json(new { Result = "ERROR", ex.Message });
            }
        }

        private JsonResult ProcessarInclusaoOuAlteracao(ClienteModel model, bool isInclusao)
        {
            try
            {
                if (!ModelState.IsValid)
                    return RetornarErrosDeValidacao();

                if (CpfJaCadastrado(model, isInclusao))
                    return Json("O CPF inserido já está cadastrado");

                if (BeneficiariosComCpfDuplicado(model))
                    return Json("Há beneficiários com CPFs duplicados");

                long clienteId = isInclusao ? InserirCliente(model) : AlterarCliente(model);

                ProcessarBeneficiarios(model, clienteId);

                return Json(isInclusao ? "Cadastro efetuado com sucesso" : "Cadastro alterado com sucesso");
            }
            catch (Exception ex)
            {
                return Json(ex.Message);
            }
        }

        private JsonResult RetornarErrosDeValidacao()
        {
            var erros = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();

            return Json(string.Join(Environment.NewLine, erros));
        }

        private bool CpfJaCadastrado(ClienteModel model, bool isInclusao)
        {
            if (!_boCliente.VerificarExistencia(model.CPF)) return false;

            if (!isInclusao && _boCliente.Consultar(model.Id)?.CPF == model.CPF) return false;

            return true;
        }

        private bool BeneficiariosComCpfDuplicado(ClienteModel model)
        {
            var beneficiariosDuplicados = model.Beneficiarios
                .GroupBy(b => b.CPF)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();

            return beneficiariosDuplicados.Any();
        }

        private long InserirCliente(ClienteModel model)
        {
            var cliente = MapearModelParaCliente(model);
            return _boCliente.Incluir(cliente);
        }

        private long AlterarCliente(ClienteModel model)
        {
            var cliente = MapearModelParaCliente(model);
            _boCliente.Alterar(cliente);
            return cliente.Id;
        }

        private void ProcessarBeneficiarios(ClienteModel model, long clienteId)
        {
            var beneficiariosExistentes = _boBeneficiario.Listar(clienteId);
            var beneficiariosAAlterar = model.Beneficiarios
                .Where(b => b.Id != null)
                .ToList();

            foreach (var beneficiarioModel in beneficiariosAAlterar)
            {
                var beneficiario = MapearModelParaBeneficiario(beneficiarioModel, clienteId);
                _boBeneficiario.Alterar(beneficiario);
                beneficiariosExistentes.RemoveAll(b => b.Id == beneficiario.Id);
            }

            var beneficiariosANovos = model.Beneficiarios
                .Where(b => b.Id == null)
                .Select(b => MapearModelParaBeneficiario(b, clienteId))
                .ToList();

            foreach (var beneficiario in beneficiariosANovos)
            {
                _boBeneficiario.Incluir(beneficiario);
            }

            foreach (var beneficiario in beneficiariosExistentes)
            {
                _boBeneficiario.Excluir(beneficiario.Id);
            }
        }

        private Cliente MapearModelParaCliente(ClienteModel model)
        {
            return new Cliente
            {
                Id = model.Id,
                Nome = model.Nome,
                Sobrenome = model.Sobrenome,
                CPF = model.CPF,
                CEP = model.CEP,
                Cidade = model.Cidade,
                Estado = model.Estado,
                Logradouro = model.Logradouro,
                Email = model.Email,
                Telefone = model.Telefone,
                Nacionalidade = model.Nacionalidade
            };
        }

        private Beneficiario MapearModelParaBeneficiario(BeneficiariosModel beneficiarioModel, long clienteId)
        {
            return new Beneficiario
            {
                Id = beneficiarioModel.Id ?? 0,
                Nome = beneficiarioModel.Nome,
                CPF = beneficiarioModel.CPF,
                IdCliente = clienteId
            };
        }

        private ClienteModel MapearClienteParaModel(Cliente cliente)
        {
            var beneficiarios = _boBeneficiario.Listar(cliente.Id);

            return new ClienteModel
            {
                Id = cliente.Id,
                Nome = cliente.Nome,
                Sobrenome = cliente.Sobrenome,
                CPF = cliente.CPF,
                CEP = cliente.CEP,
                Cidade = cliente.Cidade,
                Estado = cliente.Estado,
                Logradouro = cliente.Logradouro,
                Email = cliente.Email,
                Telefone = cliente.Telefone,
                Nacionalidade = cliente.Nacionalidade,
                Beneficiarios = beneficiarios.Select(b => new BeneficiariosModel
                {
                    Id = b.Id,
                    Nome = b.Nome,
                    CPF = b.CPF
                }).ToList()
            };
        }

        private (string sortField, bool sortAscending) ProcessarOrdenacao(string jtSorting)
        {
            string sortField = null;
            bool sortAscending = true;

            if (!string.IsNullOrEmpty(jtSorting))
            {
                var parts = jtSorting.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length > 0)
                    sortField = parts[0];
                if (parts.Length > 1)
                    sortAscending = string.Equals(parts[1], "ASC", StringComparison.OrdinalIgnoreCase);
            }

            return (sortField, sortAscending);
        }
    }
}
