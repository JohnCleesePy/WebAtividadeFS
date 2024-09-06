$(document).ready(function () {
    configurarMascaras();
    configurarEventos();

    $('#formCadastro').submit(function (e) {
        e.preventDefault();
        enviarFormularioCadastro();
    });

    $('#formIncluirBeneficiario').submit(function (e) {
        e.preventDefault();
        adicionarBeneficiario();
    });
});

function configurarMascaras() {
    $('#CPF, #CPFBeneficiario').mask('000.000.000-00', { reverse: true });
}

function configurarEventos() {
    $('#btBeneficiario').click(function (event) {
        event.preventDefault();
        $('#beneficiariosForms').modal('show');
    });

    $('#tabelaBeneficiarios').on('click', 'button.btn-excluir', function () {
        $(this).closest('tr').remove();
    });

    $('#tabelaBeneficiarios').on('click', 'button.btn-alterar', function () {
        alterarLinhaBeneficiario($(this));
    });
}

function enviarFormularioCadastro() {
    let beneficiarios = coletarBeneficiarios();

    $.ajax({
        url: urlPost,
        method: "POST",
        data: {
            "Nome": $('#formCadastro #Nome').val(),
            "CEP": $('#formCadastro #CEP').val(),
            "Email": $('#formCadastro #Email').val(),
            "Sobrenome": $('#formCadastro #Sobrenome').val(),
            "Nacionalidade": $('#formCadastro #Nacionalidade').val(),
            "Estado": $('#formCadastro #Estado').val(),
            "Cidade": $('#formCadastro #Cidade').val(),
            "Logradouro": $('#formCadastro #Logradouro').val(),
            "Telefone": $('#formCadastro #Telefone').val(),
            "CPF": $('#formCadastro #CPF').val(),
            "Beneficiarios": beneficiarios
        },
        error: function (r) {
            var mensagem = (r.status == 400) ? r.responseJSON : "Ocorreu um erro interno no servidor.";
            ModalDialog("Ocorreu um erro", mensagem);
        },
        success: function (r) {
            ModalDialog("Sucesso!", r);
            $("#formCadastro")[0].reset();
            window.location.href = urlRetorno;
        }
    });
}

function coletarBeneficiarios() {
    let beneficiarios = [];
    $('#tabelaBeneficiarios tbody tr').each(function () {
        var id = $(this).find('td:eq(0)').text();
        var cpf = $(this).find('td:eq(1)').text();
        var nome = $(this).find('td:eq(2)').text();
        beneficiarios.push({ Id: id, CPF: cpf, Nome: nome });
    });
    return beneficiarios;
}

function adicionarBeneficiario() {
    var cpf = $('#BeneficiarioCPF').val();
    var nome = $('#BeneficiarioNome').val();

    adicionarLinhaTabela('', cpf, nome);

    $('#BeneficiarioCPF, #BeneficiarioNome').val('');
}

function adicionarLinhaTabela(id, cpf, nome) {
    var newRow = `
        <tr>
            <td class="hidden-xs hidden">${id}</td>
            <td>${cpf}</td>
            <td>${nome}</td>
            <td class="text-center">
                <button type="button" class="btn btn-sm btn-primary btn-alterar" style="margin-right: 0.4rem">Alterar</button>
                <button type="button" class="btn btn-sm btn-primary btn-excluir">Excluir</button>
            </td>
        </tr>`;

    $('#tabelaBeneficiarios tbody').append(newRow);
}

function alterarLinhaBeneficiario(botao) {
    var linha = botao.closest('tr');
    var cpfColuna = linha.find('td:eq(1)');
    var nomeColuna = linha.find('td:eq(2)');

    if (linha.hasClass('em-edicao')) {
        salvarEdicaoLinha(linha, botao, cpfColuna, nomeColuna);
    } else {
        iniciarEdicaoLinha(linha, botao, cpfColuna, nomeColuna);
    }
}

function iniciarEdicaoLinha(linha, botao, cpfColuna, nomeColuna) {
    cpfColuna.html('<div class="input-group"><input id="beneficiario_Alt_CPF" type="text" class="form-control" style="width: 13rem;" value="' + cpfColuna.text() + '"></div>');
    nomeColuna.html('<div class="input-group"><input type="text" class="form-control" style="width: 150px;" value="' + nomeColuna.text() + '"></div>');
    $('#beneficiario_Alt_CPF').mask('000.000.000-00', { reverse: true });

    botao.text('Salvar').addClass('btn-success');
    linha.addClass('em-edicao');
}

function salvarEdicaoLinha(linha, botao, cpfColuna, nomeColuna) {
    var novosValores = linha.find('input').map(function () { return $(this).val(); }).get();

    cpfColuna.text(novosValores[0]);
    nomeColuna.text(novosValores[1]);

    botao.text('Alterar').removeClass('btn-success');
    linha.removeClass('em-edicao');
}

function ModalDialog(titulo, texto) {
    var random = Math.random().toString().replace('.', '');
    var modalHTML = `
        <div id="${random}" class="modal fade">
            <div class="modal-dialog">
                <div class="modal-content">
                    <div class="modal-header">
                        <button type="button" class="close" data-dismiss="modal" aria-hidden="true">×</button>
                        <h4 class="modal-title">${titulo}</h4>
                    </div>
                    <div class="modal-body">
                        <p>${texto}</p>
                    </div>
                    <div class="modal-footer">
                        <button type="button" class="btn btn-default" data-dismiss="modal">Fechar</button>
                    </div>
                </div>
            </div>
        </div>`;

    $('body').append(modalHTML);
    $('#' + random).modal('show');
}
