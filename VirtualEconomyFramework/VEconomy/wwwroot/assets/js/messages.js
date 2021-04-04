

function messagesAfterLoad() {

    $("#btnLoadAccountMessages").off();
    $("#btnLoadAccountMessages").click(function() {
        ReloadAcountMessages();
    });

    $('#btnDecryptMessage').off();
    $("#btnDecryptMessage").click(function() {
        showDecryptMessage();
    });

    $('#btnMessagesAddNewKey').off();
    $("#btnMessagesAddNewKey").click(function() {
        addNewMessageKey();
    });

    $('#btnInitMessagesAddNewKey').off();
    $("#btnInitMessagesAddNewKey").click(function() {
        addNewMessageKey();
    });

    $('#btnSendInitMessage').off();
    $("#btnSendInitMessage").click(function() {
        if (selectedMessagesAccountAddress == '') {
            alert('Select account please!');
            return;
        }
        $('#initMessageModal').modal('show');
    });
    
}

var selectedMessage = {};


//////////////////////////////
// send token

function showSendMessageModal() {
    fillSendMessageModal();

    $("#btnSendTokenModalConfirm").off();
    $("#btnSendTokenModalConfirm").click(function() {
        sendMessage();
    });

    $('#sendMessageModal').modal("show"); 
}

function fillSendMessageModal() {
    if (selectedMessage != null) {
       $('#sendTokenModalWalletName').text(ActualWallet.Name);
        $('#sendTokenModalAccountAddress').text(ActualAccount.Name + ' - ' + ActualAccount.Address);
    }
}

function prepareSendInitMessage() {

    if (accountLockState) {
        $('#unlockAccountForOneTxConfirm').off();
        $("#unlockAccountForOneTxConfirm").click(function() {
            var password = $('#unlockAccountForOneTxPassword').val();
            sendMesage(password, true);
        });

        $('#addPassForTxMessageModal').modal('show');
    }
    else {
        sendMesage(null, true);
    }
}

function prepareSendMessage() {

    if (accountLockState) {
        $('#unlockAccountForOneTxConfirm').off();
        $("#unlockAccountForOneTxConfirm").click(function() {
            var password = $('#unlockAccountForOneTxPassword').val();
            sendMesage(password, false);
        });

        $('#addPassForTxMessageModal').modal('show');
    }
    else {
        sendMesage(null);
    }
}

function hideMessageKeyPasswordItems(initmsg){
    /*
    if (!initmsg) {
        if(!$("#chboxEncryptMessage").is(':checked')) {
            $('.messageKeyPasswordItems').hide();
        }
        else {
            $('.messageKeyPasswordItems').show();
        }
    }
    else {
        if(!$("#chboxEncryptInitMessage").is(':checked')) {
            $('.initMessageKeyPasswordItems').hide();            
        }
        else {
            $('.initMessageKeyPasswordItems').show();
        }
    }*/
}

function sendMesage(password, initmsg) {
    if (selectedMessage != null) {
        
        var add = '';
        if (!initmsg) {
            add = $('#sendMessageModalReceiverAddress').text();
        }
        else {
            add = $('#initMessageModalReceiverAddress').val();
        }

        if (add == selectedMessagesAccountAddress) {
            alert('You cannot send message to yourself!');
            return;
        }

        if (add == '') {
            alert('Address Cannot Be empty');
            return;
        }

        var keyId = selectedKey.KeyId;

        if (!accountLockState) {
            password = '';
        }

        if (accountLockState && password == null) {
            alert('Account is locked and you did not provided password!');
            return;
        }

        var message = '';
        if (!initmsg) {
            message = $('#messageModalNewMessageTextarea').val();
        }
        else {
            message = $('#initMessageModalNewMessageTextarea').val();
        }

        if (message == '' || message == ' ') {
            alert('You must add some message!');
            return;
        }

        var msgStreamUID = '';
        var receiverPubKey = '';
        var tokenTxId = '';
        var keyPass = '';
        if (!initmsg) {
            msgStreamUID = selectedMessage.Metadata['MessageStreamUID'];
            receiverPubKey = selectedMessage.Metadata['SenderPubKey'];
            tokenTxId = selectedMessage.TxId;
        }
        
        var encmsg = true;

        if (!initmsg) {
            if(!$("#chboxEncryptMessage").is(':checked')) {
                encmsg = false;
            }
            keyPass = $('#sendMessageKeyPassword').val();
        }
        else {
            if(!$("#chboxEncryptInitMessage").is(':checked')) {
                encmsg = false;
            }
            keyPass = $('#sendInitMessageKeyPassword').val();
        }

        var message = {
            "WalletId" : selectedWallet,
            "MessageStreamUID" : msgStreamUID,
            "ReceiverAddress": add,
            "ReceiverPubKey" : receiverPubKey,
            "SenderAddress": selectedMessagesAccountAddress,
            "TokenTxId" : tokenTxId,
            "InitMessage" : initmsg,
            "KeyId" : keyId,
            "Message": message,
            "Password" : keyPass,
            "AccountPassword" : password,
            "Encrypt" : encmsg
        };
    
        $("#confirmButtonOk").off();
        $("#confirmButtonOk").click(function() {
            sendTokenApiCommand('SendMessageToken', message);
        });
        
        ShowConfirmModal('', 'Do you realy want to send this message?');  
    }
}

function sendMessageApiCommand(apicommand, data) {
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

var decryptedMessage = {};

function sendDecryptMessageRequest(txid, password, spubkey) {

    var data = {
        "WalletId" : selectedWallet,
        "accountAddress" : selectedMessagesAccountAddress,
        "Password": password,
        "TxId" : txid
    };

    var url = document.location.origin + "/api/" + "DecryptMessage";

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
                decryptedMessage = data;
                if (data != null) {
                    $('#messageModalPreviousReceivedTextarea').val(decryptedMessage['PrevMsg']);
                    $('#messageModalNewReceivedTextarea').val(decryptedMessage['NewMsg']);
                    $('#messageModal').modal('show');
                }
            },
            error: function (jqXhr, textStatus, errorMessage) { // error callback 
                console.log('Error: "' + errorMessage + '"');
            }
        });
}

function showDecryptMessage() {
    
    $('#decryptMessageConfirm').off();
    $("#decryptMessageConfirm").click(function() {
        var password = $('#decryptMessagePassword').val();
        var spubkey = selectedMessage.Metadata['SenderPubKey'];
        sendDecryptMessageRequest(selectedMessage.TxId, password, spubkey);
    });

    $('#decryptMessageModal').modal('show');
}

var selectedMessage = {}
function showMessage(txid) {

    selectedMessage = AcountMessages[txid];
    $('#messageModalStreamId').text(selectedMessage.Metadata['MessageStreamUID']);

    getMessageReceiver(selectedWallet, selectedMessagesAccountAddress, txid);
    
    var init = selectedMessage.Metadata['InitMessage'];
    if (init == "true") {
        $('#messageModalNewReceivedTextarea').val(selectedMessage.Metadata['MessageData']);
    }
    else {
        $('#messageModalNewReceivedTextarea').val(selectedMessage.Metadata['MessageData']);
        $('#messageModalPreviousReceivedTextarea').val(selectedMessage.Metadata['PreviousMessage']);
    }
    
    $('#messageModal').modal('show');
}

function getMessageReceiver(wallet, address, txid) {
    var url = ApiUrl + '/GetAccountTransaction';

    var data = {
        'walletId' : wallet,
        'accountAddress': address,
        'txId' : txid
    };

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
            //console.log(`Status: ${status}, Data:${data}`);
            if (data.From[0] == selectedMessagesAccountAddress) {
                $('#sendMessageModalReceiverAddress').text('You are receiver of this message.');
            }
            $('#sendMessageModalReceiverAddress').text(data.From[0]);
        },
        error: function (jqXhr, textStatus, errorMessage) { // error callback 
            console.log('Error: "' + errorMessage + '"');
        }
    });   
}

function getMessageComponent(token) {
    
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

    var from = '';
    var to = '';

    if (token.From == selectedMessagesAccountAddress) {
        from = 'You';
    }
    else {
        from = token.From.substring(0,3) + '...' + token.From.substring(token.From.length-3);
    }

    if (token.To == selectedMessagesAccountAddress) {
        to = 'You';
    }
    else {
        to = token.To.substring(0,3) + '...' + token.To.substring(token.To.length-3);
    }   

    var tokenComponent = '<div class="card shadow" style="width: 350px;">'+
    '    <div class="card-header d-xl-flex justify-content-xl-center align-items-xl-center py-3">'+
    '        <h4 id="chessBoardHeading">' + token.Symbol + ' - Message</h4>'+
    '    </div>'+
    '    <div class="card-body">'+
    '        <div class="col">'+
    '            <div class="row" style="margin-bottom: 30px;">'+
    '                <div class="col">'+
    '                    <div class="row">'+
    '                        <div class="col d-flex d-xl-flex justify-content-center justify-content-xl-center align-items-xl-center"><span>From</span></div>'+
    '                        <div class="col d-flex d-xl-flex justify-content-center justify-content-xl-center align-items-xl-center"><span>'+ from + '</span></div>'+
    '                    </div>'+
    '                    <div class="row">'+
    '                        <div class="col d-flex d-xl-flex justify-content-center justify-content-xl-center align-items-xl-center"><span>To</span></div>'+
    '                        <div class="col d-flex d-xl-flex justify-content-center justify-content-xl-center align-items-xl-center"><span>'+ to + '</span></div>'+
    '                    </div>'+
    '                    <div class="row">'+
    '                        <div class="col d-flex d-xl-flex justify-content-center justify-content-xl-center align-items-xl-center"><span>Token Name</span></div>'+
    '                        <div class="col d-flex d-xl-flex justify-content-center justify-content-xl-center align-items-xl-center"><span>' + token.Name + '</span></div>'+
    '                    </div>'+
    '                    <div class="row">'+
    '                        <div class="col d-flex d-xl-flex justify-content-center justify-content-xl-center align-items-xl-center"><span>Token Link</span></div>'+
    '                        <div class="col d-flex d-xl-flex justify-content-center justify-content-xl-center align-items-xl-center"><a target="_blank" href="https://explorer.nebl.io/token/' + token.Id +'">Go To Explorer</a></div>'+
    '                    </div>'+
    '                    <div class="row">'+
    '                        <div class="col d-flex d-xl-flex justify-content-center justify-content-xl-center align-items-xl-center"><span>Time Stamp</span></div>'+
    '                        <div class="col d-flex d-xl-flex justify-content-center justify-content-xl-center align-items-xl-center"><span style="font-size: 12px">'+ token.TimeStamp.toString() + '</span></div>'+
    '                    </div>'+
    '                    <div class="row">'+
    '                        <div class="col d-flex d-xl-flex justify-content-center justify-content-xl-center align-items-xl-center"><span>Direction</span></div>'+
    '                        <div class="col d-flex d-xl-flex justify-content-center justify-content-xl-center align-items-xl-center"><span>'+ dir + '</span></div>'+
    '                    </div>'+
    '                    <div class="row">'+
    '                        <div class="col d-flex d-xl-flex justify-content-center justify-content-xl-center align-items-xl-center"><span>Details</span></div>'+
    '                        <div class="col d-flex d-xl-flex justify-content-center justify-content-xl-center align-items-xl-center"><a ' + metastyle + ' onclick="showMessage(\'' + token.TxId + '\')">' + meta + '</a></div>'+
    '                    </div>'+
    '                    <div class="row">'+
    '                        <div class="col d-flex d-xl-flex justify-content-center justify-content-xl-center align-items-xl-center"><img style="width: auto;max-width: 100%;min-height: 100%; max-height: 200px;" src="' + token.ImageUrl + '" /></div>'+
    '                    </div>'+
    '                </div>'+
    '            </div>'+
    '        </div>'+
    '    </div>'+
    '</div>';
    
    return tokenComponent;
}

var AcountMessages = {};

function ReloadAcountMessages() {

    if (selectedMessagesAccountAddress == null || selectedMessagesAccountAddress == '') {
        alert('Please select the account address!');
        return;
    }

    $("#confirmButtonOk").off();
    $("#confirmButtonOk").click(function() {
        var url = document.location.origin + '/api/GetAccountMessages/' + selectedMessagesAccountAddress;

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
                        AcountMessages = data;
                        refreshMessages();
                    }
                },
                error: function (jqXhr, textStatus, errorMessage) { // error callback 
                    console.log('Error: "' + errorMessage + '"');
                }
            });
    }); 

    ShowConfirmModal('', 'Do you realy want load all account messages?');

}

function refreshMessages() {

    $('#messagesCards').empty();
    $('#messagesCards').append('<div class="row"><div class="col"><div id="messagesCardsRow" class="row d-flex justify-content-center"></div></div></div>');
    for (var t in AcountMessages) {
        var tok = AcountMessages[t];
        var drawIt = false;
        var filterActive = false;

        if (tok != undefined && tok != null) {

            // todo get tx details and check sender address
            var filter = $('#messagesListSearchByNameAddress').val();
            if (filter != '' && filter != ' ') {
                filter = filter.toLowerCase();
                if (tok.Symbol.toLowerCase().includes(filter) || tok.Name.toLowerCase().includes(filter)) {
                    filterActive = true;
                }
            }
            else {
                filterActive = true;
            }

            if ($('#chbxMessagesListOutgoing').is(':checked')) {
                if (tok.Direction == 1) {
                    drawIt = true;
                }
            }

            if ($('#chbxMessagesListIncoming').is(':checked')) {
                if (tok.Direction == 0) {
                    drawIt = true;
                }
            }

            if (!tok.MetadataAvailable) {
                drawIt = false;
            }

            if (drawIt && filterActive) {
                $('#messagesCardsRow').append(
                    '<div class="col-auto">' +
                    getMessageComponent(tok) +
                    '</div>'
                );
            }
        }
    }		
}

var selectedMessagesAccountAddress = '';
var selectedWallet = '';

function setMessagesListAccountAddress(accountAddress) {
    selectedMessagesAccountAddress = accountAddress;
    var a = Accounts[accountAddress];
    selectedWallet = a.WalletId;

    checkAccountLockStatus(accountAddress);

    reloadAccountKeys();

    if (a != null) {
        document.getElementById('btnAccountAddressToLoadMessages').innerText = accountAddress;
    }
}

function reloadMessagesListAccountsAddressesDropDown() {
    if (Accounts != null) {
        document.getElementById('messagesListAccountAddressesDropDown').innerHTML = '';
        for (var acc in Accounts) {
            var a = Accounts[acc];
            var add = acc.substring(0,3) + '...' + acc.substring(acc.length-3);         
            document.getElementById('messagesListAccountAddressesDropDown').innerHTML += '<button style=\"font-size:12px\" class=\"dropdown-item btn btn-light\" ' +  'onclick=\"setMessagesListAccountAddress(\'' + acc + '\')\">' + a.Name + ' - ' + add + '</button>';
        }
    }
}


////////////
// load account keys
var accountKeys = {};
function reloadAccountKeys() {
    var url = document.location.origin + '/api/GetAccountKeys/' + selectedMessagesAccountAddress;

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
                    accountKeys = data;
                    reloadMessagesAccountsKeysDropDown();
                }
            },
            error: function (jqXhr, textStatus, errorMessage) { // error callback 
                console.log('Error: "' + errorMessage + '"');
            }
        });
}

var selectedKey = '';

function setMessagesAccountKey(keyId) {
    selectedKey = accountKeys[keyId];

    if (selectedKey != null) {
        document.getElementById('btnAccountMessageKey').innerText = selectedKey.KeyName;
        document.getElementById('btnAccountInitMessageKey').innerText = selectedKey.KeyName;
    }
}

function reloadMessagesAccountsKeysDropDown() {
    if (accountKeys != null) {
        document.getElementById('messagesAccountKeysDropDown').innerHTML = '';
        document.getElementById('initMessagesAccountKeysDropDown').innerHTML = '';
        for (var kid in accountKeys) {
            var k = accountKeys[kid];
            document.getElementById('messagesAccountKeysDropDown').innerHTML += '<button style=\"font-size:12px\" class=\"dropdown-item btn btn-light\" ' +  'onclick=\"setMessagesAccountKey(\'' + kid + '\')\">' + k.KeyName + '</button>';
            document.getElementById('initMessagesAccountKeysDropDown').innerHTML += '<button style=\"font-size:12px\" class=\"dropdown-item btn btn-light\" ' +  'onclick=\"setMessagesAccountKey(\'' + kid + '\')\">' + k.KeyName + '</button>';
        }
    }
}

/////////////////////////////////
// add new message key

function addNewMessageKey() {
    importAccountClearFields();

    $("#btnConfirmImportAccountKey").off();
    $("#btnConfirmImportAccountKey").click(function() {
        addNewMessageKeyRequest();
    });

    $('#importAccountKeyModal').modal('show');
}

function addNewMessageKeyRequest() {

    var name = $('#importAccountKeyName').val();
    var key = $('#importAccountKeyKey').val();
    var pubkey = $('#importAccountKeyPubKey').val();
    var password = $('#importAccountKeyPassword').val();

    var url = ApiUrl + '/LoadAccountKey';

    var alreadyEncrypted = false;
    if ($('#chbxMessagesImportAlreadyEncryptedKey').is(':checked')) {
        alreadyEncrypted = true;
    }

    if (ActualWallet != {} && ActualAccount != {}) {
        var data = {
            'walletId' : selectedWallet,
            'accountAddress': selectedMessagesAccountAddress,
            'key' : key,
            'pubkey': pubkey,
            'name': name,
            'password' : password,
            'isItMainAccountKey' : false,
            'alreadyEncrypted': alreadyEncrypted
        };
    }

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
            //console.log(`Status: ${status}, Data:${data}`);
            importAccountClearFields();
            checkAccountLockStatus(ActualAccount.Address);
            reloadMessagesAccountsKeysDropDown();
        },
        error: function (jqXhr, textStatus, errorMessage) { // error callback 
            console.log('Error: "' + errorMessage + '"');
            importAccountClearFields();
            checkAccountLockStatus(ActualAccount.Address);
        }
    }); 
}