

$(document).ready(function () {

    $("#btnTokenDetailsSendToken").off();
    $("#btnTokenDetailsSendToken").click(function() {
        showSendTokenModal();
    });

    $("#btnSendTokenModalAddMetadataField").off();
    $("#btnSendTokenModalAddMetadataField").click(function() {
        getNewMetadataLine();
    });
});

function fillTokenDetails(token) {
    if (token != null) {
        $('#tokenDetailsSymbolAndName').text(token.Symbol + ' - ' + token.Name);
        $('#tokenDetailsId').text(token.Id);
        $('#tokenDetailsIssuer').text(token.IssuerName);
        $('#tokenDetailsActualAmount').text(token.ActualBalance.toString());
        $('#tokenDetailsMaximumSuply').text(token.MaxSupply.toString());
        $('#tokenDetailsImage').attr("src", token.ImageUrl);
    }
}

var selectedToken = {};

function showTokenDetails(tokenId) {
    if (ActualAccount != null) {
        if (ActualAccount.Tokens != null) {
            selectedToken = ActualAccount.Tokens[tokenId];
            if (selectedToken != null) {
                fillTokenDetails(selectedToken);

                $('#tokenDetailsImage').ready(function() {
                    $('#tokenDetailsModal').modal("show"); 
                });
            }
        }
    }
}

//////////////////////////////
// send token

function showSendTokenModal() {
    fillSendTokenModal();

    $("#btnSendTokenModalConfirm").off();
    $("#btnSendTokenModalConfirm").click(function() {
        sendToken();
    });

    $('#sendTokenModal').modal("show"); 
}

function fillSendTokenModal() {
    if (selectedToken != null) {
        $('#sendTokenModalSymbolName').text(selectedToken.Symbol + ' - ' + selectedToken.Name);
        $('#sendTokenModalWalletName').text(ActualWallet.Name);
        $('#sendTokenModalAccountAddress').text(ActualAccount.Name + ' - ' + ActualAccount.Address);
    }
}

function createMetadataFromFileAndFillToLine(line) {
    $('#calcHashOfFileModal').modal('show');

    $("#btnUseHashAsMetadata").off();
    $("#btnUseHashAsMetadata").click(function() {
        var parent = $(line).parent().parent();
        parent.find('td.metadataName').find('textarea.metadataName').val('SHA256 Hash of file: ' + fileHashName);
        parent.find('td.metadataContent').find('textarea.metadataContent').val(finalHash);
    });
}

function getNewMetadataLine() {

    var line = '<tr>'+
        '    <td class="d-xl-flex justify-content-xl-end metadataName">'+
        '       <textarea type="text" class="metadataName" style="width: 200px;"></textarea>'+
        '    </td>'+
        '    <td class="metadataContent">'+
        '       <textarea class="metadataContent" style="width: 200px;margin-left: 20px;"></textarea>'+
        '    </td>'+
        '    <td>'+
        '       <button class="btn btn-secondary" type="button" style="padding-top: 2px;padding-bottom: 2px;padding-right: 10px;padding-left: 10px;" onclick="createMetadataFromFileAndFillToLine(this)">Hash</button>'+
        '    </td>'+
        '    <td>'+
        '       <button class="btn btn-secondary" type="button" style="padding-top: 2px;padding-bottom: 2px;padding-right: 10px;padding-left: 10px;" onclick="removeMetadataLine(this)">-</button>'+
        '    </td>'+
        '</tr>';
        
    $('#sendTokenMetadataTable tbody').append(line);
}

function getMetadataToSend() {
    var metadata = {};
    var lines = $('#sendTokenMetadataTable > tbody > tr').each(function(index, tr) {
        var name = $(tr).find('td.metadataName').find('textarea.metadataName').val();
        var content = $(tr).find('td.metadataContent').find('textarea.metadataContent').val();
        metadata[name] = content;
    });

    return metadata;
}

function removeMetadataLine(line) {
    $(line).parents('tr').remove();
}

function sendToken() {
    if (selectedToken != null) {
        var metadata = {};

        metadata = getMetadataToSend();

        var add = $('#sendTokenModalReceiverAddress').val();
        if (add == '') {
            alert('Address Cannot Be empty');
            return;
        }

        var amount = parseInt($('#sendTokenModalAmount').val());
        if (amount == 0) {
            alert('Cannot send 0 tokens');
            return;
        }

        var token = {
            "ReceiverAddress": add,
            "Symbol": selectedToken.Symbol,
            "Id": selectedToken.Id,
            "Amount": amount,
            "Metadata": metadata
        };
    
        $("#confirmButtonOk").off();
        $("#confirmButtonOk").click(function() {
            sendTokenApiCommand('SendNTP1Token', token);
        });
        
        ShowConfirmModal('', 'Do you realy want to send these tokens?');  
    }
}

function sendTokenApiCommand(apicommand, data) {
    var url = document.location.origin + "/api/" + apicommand;

    if (bootstrapstudio) {
        url = url.replace('8000','8080');
    }

    $.ajax(url,
        {
            contentType: 'application/json;charset=utf-8',
            data: JSON.stringify(data),
            method: 'PUT',
            dataType: 'json',   // type of response data
            timeout: 10000,     // timeout milliseconds
            success: function (data, status, xhr) {   // success callback function
                console.log(`Status: ${status}, Data:${data}`);
                $('#transactionSentModal').modal("show"); 
                setTimeout(() => {
                    $('#transactionSentModal').modal("toggle");
                }, 2500);
            },
            error: function (jqXhr, textStatus, errorMessage) { // error callback 
                console.log('Error: "' + errorMessage + '"');

                $('#transactionNotSentModal').modal("show"); 
                setTimeout(() => {
                    $('#transactionNotSentModal').modal("toggle");
                }, 2500);
            }
        });
}