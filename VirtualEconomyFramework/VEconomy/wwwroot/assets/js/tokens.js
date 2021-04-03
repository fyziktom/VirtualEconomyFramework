

function tokensAfterLoad() {

    $("#btnTokenDetailsSendToken").off();
    $("#btnTokenDetailsSendToken").click(function() {
        showSendTokenModal();
    });

    $("#btnSendTokenModalAddMetadataField").off();
    $("#btnSendTokenModalAddMetadataField").click(function() {
        getNewMetadataLine();
    });

    if (ActualPage == Pages.tokens) {
        $("#btnLoadAccountTokens").off();
        $("#btnLoadAccountTokens").click(function() {
            ReloadAcountTokens();
        });
    }
}

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
            "SenderAddress": ActualAccount.Address,
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
                    if($('#transactionSentModal').hasClass('in')) {
                        $('#transactionSentModal').modal("toggle"); 
                    }
                }, 2500);
            },
            error: function (jqXhr, textStatus, errorMessage) { // error callback 
                console.log('Error: "' + errorMessage + '"');

                $('#txNotSendMessage').text(jqXhr.responseText);

                $('#transactionNotSentModal').modal("show"); 
                setTimeout(() => {
                    if($('#transactionNotSentModal').hasClass('in')) {
                        $('#transactionNotSentModal').modal("toggle"); 
                    }
                }, 5000);
            }
        });
}

function getTokenComponent(token) {
    
    var meta = 'No Metadata';
    var metastyle = 'style="font-size: 12px;';
    if (token.MetadataAvailable) {
        meta = 'Click for details';
        metastyle = 'class="bold" style="font-size: 14px;"';
    }

    var dir = 'Incoming';
    if (token.Direction == 1) {
        dir = 'Outgoing';
    }

    var tokenComponent = '<div class="card shadow" style="width: 350px;">'+
    '    <div class="card-header d-xl-flex justify-content-xl-center align-items-xl-center py-3">'+
    '        <h4 id="chessBoardHeading">' + token.Symbol + '</h4>'+
    '    </div>'+
    '    <div class="card-body">'+
    '        <div class="col">'+
    '            <div class="row" style="margin-bottom: 30px;">'+
    '                <div class="col">'+
    '                    <div class="row">'+
    '                        <div class="col d-flex d-xl-flex justify-content-center justify-content-xl-center align-items-xl-center"><span>Token Name</span></div>'+
    '                        <div class="col d-flex d-xl-flex justify-content-center justify-content-xl-center align-items-xl-center"><span>' + token.Name + '</span></div>'+
    '                    </div>'+
    '                    <div class="row">'+
    '                        <div class="col d-flex d-xl-flex justify-content-center justify-content-xl-center align-items-xl-center"><span>Token Link</span></div>'+
    '                        <div class="col d-flex d-xl-flex justify-content-center justify-content-xl-center align-items-xl-center"><a target="_blank" href="https://explorer.nebl.io/token/' + token.Id +'">Go To Explorer</a></div>'+
    '                    </div>'+
    '                    <div class="row">'+
    '                        <div class="col d-flex d-xl-flex justify-content-center justify-content-xl-center align-items-xl-center"><span>Tx Details Link</span></div>'+
    '                        <div class="col d-flex d-xl-flex justify-content-center justify-content-xl-center align-items-xl-center"><a target="_blank" href="https://explorer.nebl.io/tx/' + token.TxId +'">Go To Explorer</a></div>'+
    '                    </div>'+
    '                    <div class="row">'+
    '                        <div class="col d-flex d-xl-flex justify-content-center justify-content-xl-center align-items-xl-center"><span>Time Stamp</span></div>'+
    '                        <div class="col d-flex d-xl-flex justify-content-center justify-content-xl-center align-items-xl-center"><span style="font-size:12px">'+ token.TimeStamp.toString() + '</span></div>'+
    '                    </div>'+
    '                    <div class="row">'+
    '                        <div class="col d-flex d-xl-flex justify-content-center justify-content-xl-center align-items-xl-center"><span>Amount</span></div>'+
    '                        <div class="col d-flex d-xl-flex justify-content-center justify-content-xl-center align-items-xl-center"><span>'+ token.ActualBalance + '</span></div>'+
    '                    </div>'+
    '                    <div class="row">'+
    '                        <div class="col d-flex d-xl-flex justify-content-center justify-content-xl-center align-items-xl-center"><span>Direction</span></div>'+
    '                        <div class="col d-flex d-xl-flex justify-content-center justify-content-xl-center align-items-xl-center"><span>'+ dir + '</span></div>'+
    '                    </div>'+
    '                    <div class="row">'+
    '                        <div class="col d-flex d-xl-flex justify-content-center justify-content-xl-center align-items-xl-center"><span>Metadata</span></div>'+
    '                        <div class="col d-flex d-xl-flex justify-content-center justify-content-xl-center align-items-xl-center"><a ' + metastyle + ' onclick="showTokenMetadata(\'' + token.TxId + '\')">' + meta + '</a></div>'+
    '                    </div>'+
    '                    <div class="row">'+
    '                        <div class="col d-flex d-xl-flex justify-content-center justify-content-xl-center align-items-xl-center"><span>Image</span></div>'+
    '                    </div>'+
    '                    <div class="row">'+
    '                        <div class="col d-flex d-xl-flex justify-content-center justify-content-xl-center align-items-xl-center"><img style="width: auto;max-width: 100%;min-height: 100%; max-height: 400px;" src="' + token.ImageUrl + '" /></div>'+
    '                    </div>'+
    '                </div>'+
    '            </div>'+
    '        </div>'+
    '    </div>'+
    '</div>';
    
    return tokenComponent;
}

function showTokenMetadata(tokenTxId) {

    tokMeta = AcountTokens[tokenTxId];
    if (tokMeta != null) {
        if (tokMeta.MetadataAvailable) {
            if (tokMeta.Metadata != undefined && tokMeta.Metadata != null) {
                meta = JSON.stringify(tokMeta.Metadata, null, 2);
                $('#tokenMetadataModalTextarea').val(meta);
                $('#tokenMetadataModal').modal('show');
            }
        }
    }
}

var AcountTokens = {};

function ReloadAcountTokens() {

    if (selectedTokensAccountAddress == null) {
        alert('Please select the account address!');
        return;
    }

    $("#confirmButtonOk").off();
    $("#confirmButtonOk").click(function() {
        var url = document.location.origin + '/api/GetAccountTokens/' + selectedTokensAccountAddress;

        if (bootstrapstudio) {
            url = url.replace('8000','8080');
        }

        $.ajax(url,
            {
                contentType: 'application/json;charset=utf-8',
                method: 'GET',
                dataType: 'json',   // type of response data
                timeout: 10000,     // timeout milliseconds
                success: function (data, status, xhr) {   // success callback function
                    console.log(data);
                    if (data != null){
                        AcountTokens = data;
                        refreshTokens();
                    }
                },
                error: function (jqXhr, textStatus, errorMessage) { // error callback 
                    console.log('Error: "' + errorMessage + '"');
                }
            });
    }); 

    ShowConfirmModal('', 'Do you realy want load all account tokens?');

}

function refreshTokens() {

    $('#tokensCards').empty();
    $('#tokensCards').append('<div class="row"><div class="col"><div id="tokensCardsRow" class="row d-flex justify-content-center"></div></div></div>');
    for (var t in AcountTokens) {
        var tok = AcountTokens[t];
        var drawIt = false;
        var filterActive = false;

        if (tok != undefined && tok != null) {

            var filter = $('#tokenListSearchByNameSymbol').val();
            if (filter != '' && filter != ' ') {
                filter = filter.toLowerCase();
                if (tok.Symbol.toLowerCase().includes(filter) || tok.Name.toLowerCase().includes(filter)) {
                    filterActive = true;
                }
            }
            else {
                filterActive = true;
            }

            if ($('#chbxTokenListOutgoing').is(':checked')) {
                if (tok.Direction == 1) {
                    drawIt = true;
                }
            }

            if ($('#chbxTokenListIncoming').is(':checked')) {
                if (tok.Direction == 0) {
                    drawIt = true;
                }
            }

            if ($('#chbxTokenListWithMetadata').is(':checked')) {
                if (!tok.MetadataAvailable) {
                    drawIt = false;
                }
            }

            if (drawIt && filterActive) {
                $('#tokensCardsRow').append(
                    '<div class="col-auto">' +
                    getTokenComponent(tok) +
                    '</div>'
                );
            }
        }
    }		
}

var selectedTokensAccountAddress = '';

function setTokenListAccountAddress(accountAddress) {
    selectedTokensAccountAddress = accountAddress;
    var a = Accounts[accountAddress];
    if (a != null) {
        document.getElementById('btnAcountAddressToLoadTokens').innerText = accountAddress;
    }
}

function reloadTokenLIstAccountsAddressesDropDown() {
    if (Accounts != null) {
        document.getElementById('tokenListAccountAddressesDropDown').innerHTML = '';
        for (var acc in Accounts) {
            var a = Accounts[acc];
            document.getElementById('tokenListAccountAddressesDropDown').innerHTML += '<button style=\"width: 400px;font-size:12px\" class=\"dropdown-item btn btn-light\" ' +  'onclick=\"setTokenListAccountAddress(\'' + acc + '\')\">' + a.Name + ' - ' + acc + '</button>';
        }
    }
}