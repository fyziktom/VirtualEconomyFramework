
function accountsAfterLoad() {

    $("#btnAddNewAccount").off();
    $("#btnAddNewAccount").click(function() {
        AddNewAccount();
    });

    $("#btnUpdateAccountDetails").off();
    $("#btnUpdateAccountDetails").click(function() {
        ActualAccount.Name = $('#accountDetailsName').val();
        UpdateAccount(ActualAccount, true);
    });

    $("#btnDeleteAccount").off();
    $("#btnDeleteAccount").click(function() {
        deleteAccount();
    });

    $("#btnAccountTransactionsDetails").off();
    $("#btnAccountTransactionsDetails").click(function() {
        showAccountTransactions();
    });

    $("#btnAccountDetailsLockUnlock").off();
    $("#btnAccountDetailsLockUnlock").click(function() {
        lockunlockAccount();
    });

    $("#btnAccountDetailsImportKey").off();
    $("#btnAccountDetailsImportKey").click(function() {
        showImportAccountKeyModal();
    });

    $("#btnAccountModalSendTx").off();
    $("#btnAccountModalSendTx").click(function() {
        fillAndShowSendTxModal();
    });

    $("#btnSendTxModalConfirm").off();
    $("#btnSendTxModalConfirm").click(function() {
        prepareSendTx(null);
    });

    $('#loginModal').on('shown.bs.modal', function () {
        $(function() {
            $('#loginName').focus();
        });
    });
    

    try { 
        $('#addPassForTxMessageModal').on('shown.bs.modal', function () {
            $(function() {
                $('#unlockAccountForOneTxPassword').focus();
            });
        });
    }
    catch {}

    try {    
        $('#unlockAccountModal').on('shown.bs.modal', function () {
            $(function() {
                $('#accountPasswordInput').focus();
            });
        });
    }
    catch {}

    try {
        getAccountTypes();
    }
    catch {
        console.log('Cannot load wallet types from API.');
    }
}


function AddNewAccount() {

    $("#btnAddNewAccountConfirm").off();
    $("#btnAddNewAccountConfirm").click(function() {

        var justToDb = true;

        if(!$("#chboxSaveNewAccountJustToDb").is(':checked')) {
            justToDb = false;
        }

        var account = $('#newAccountAddress').val();

        if (account == '' && justToDb) {
            alert('You have to fill the address if you want to save it just to db!');
            $('#addNewAccountModal').modal('show');
            return;
        }

        if (account != '' && !justToDb) {
            alert('You cannot save to db already existing address!');
            $('#addNewAccountModal').modal('show');
            return;
        }

        if (!justToDb || account == '') {
            var password = $('#newAccountPassword').val();
            if (password == '' || password == ' ')
            {
                alert('password cannot be empty for new account!');
                return;
            }
        }
        
        var acc = {
            "walletId": $('#newAccountWalletId').val(),
            "Address": account,
            "Name": $('#newAccountName').val(),
            "password" : password,
            "saveJustToDb": justToDb,
            "accountType": 1//account.Type //now support just for neblio, todo reddcoin and bitcoin
        };

        $('#addNewAccountModal').modal('toggle');
        UpdateAccount(acc, justToDb);
    });

    $('#newAccountWalletId').val(ActualWallet.Id);
    reloaNewAccountTypesDropDown();
    setNewAccountType(getAccountTypeByName('Neblio'));

    $('#addNewAccountModal').modal('show');
}

function UpdateAccount(account, justToDb) {

    if (account != null && ActualWallet != null) {

        var password = '';
        if (account.password != undefined) {
            if (account.password != null) {
                password = account.password;
            }
        }
        
        var acc = {
            "walletId": ActualWallet.Id,
            "accountAddress": account.Address,
            "accountName": account.Name,
            "password" : password,
            "saveJustToDb": justToDb,
            "accountType": 1//account.Type //now support just for neblio, todo reddcoin and bitcoin
        };
    
        $("#confirmButtonOk").off();
        $("#confirmButtonOk").click(function() {
            ComandAPIRequest('UpdateAccount', acc);
        });
        
        ShowConfirmModal('', 'Do you realy want to add or update this account?');  
    }
    else {
        console.log('Add or Update account - Account or Wallet cannot be empty!')
    }
}

var accountToDeleteDto = {};

function deleteAccount() {

    if (ActualAccount != null && ActualWallet != null) {

        var acc = {
            "walletId": ActualWallet.Id,
            "accountAddress": ActualAccount.Address,
            "withNodes" : false
        };
    
        accountToDeleteDto = acc;

        $("#confirmButtonOk").off();
        $("#confirmButtonOk").click(function() {

            $("#confirmButtonCancel").off();
            $("#confirmButtonCancel").click(function() {
                accountToDeleteDto.withNodes = false;
                ComandAPIRequest('DeleteAccount', accountToDeleteDto);
            });
    
            $("#confirmButtonOk").off();
            $("#confirmButtonOk").click(function() {
                accountToDeleteDto.withNodes = true;
                ComandAPIRequest('DeleteAccount', acc);
            });

            setTimeout(function() {
                ShowConfirmModal('', 'Do you want to also delete all related nodes?');  
            },500);
        });
        
        ShowConfirmModal('', 'Do you realy want to delete this account?');  
    }
    else {
        console.log('Add or Update account - Account or Wallet cannot be empty!')
    }
}


function refreshAccountsStatus(accounts) {
    Accounts = accounts;

    /*
    var list = $('#nodesTable tbody');
    list.children().remove();

    for (var n in nodes) {
         nd = nodes[n];
         if (nd != null && nd != undefined) {
            list.append(getNodeComponent(nd));
         }
    }		
    */
}

function getAccountByID(id) {
    if (Accounts != undefined && Accounts != null){
        for (var a in Accounts) {
            if (Accounts[a] != undefined && Accounts[a] != null) {
                if (Accounts[a].Id == id) {
                    return Accounts[a];
                }
            }
        }
    }
}

function loadAccounts(wallet){

    var list = $('#walletDetailsAccountsTable tbody');
    list.children().remove();

    if (wallet != null) {
        for (var a in wallet.Accounts) {
            var acc = wallet.Accounts[a];
            var shortadd = acc.Address.substring(0,3) + '...' + acc.Address.substring(acc.Address.length-3);
            list.append(
                '<tr>' +
                '<td>' + acc.Name + ', ' + shortadd + '</td>' +
                '<td>' +
                    '<div class="row">' + 
                        '<div class="col">' + 
                            '<button class="btn btn-primary" style="margin: 2px;" onclick=\'showAccountDetails("' + acc.Address + '")\'><i class="fa fa-info-circle"></i></button>' +
                        '</div>' +
                    '</div>' +
                '</td>' +
                '</tr>');
        }
    }
}

var actualAccountId = '';
function showAccountDetails(id) {
    actualAccountId = id;
    ActualAccount = ActualWallet.Accounts[id];
    fillAccountDetails(ActualAccount, false);
    loadTokens(ActualAccount);
    $('#accountDetailsModal').modal("show"); 
}

function fillAccountDetails(account,refresh) {

    checkAccountLockStatus(account.Address);
    $('#accountDetailsAddress').text(account.Address);

    if (!refresh) {
        $('#accountDetailsName').val(account.Name);
    }
    var ntxs = 'Loaded: ' + account.NumberOfLoadedTransaction.toString() + ' of ' + account.NumberOfTransaction.toString();
    $('#accountDetailsNumOfTx').text(ntxs);

    $('#accountDetailsTotalNEBL').text(account.TotalBalance.toString());
    //$('#accountDetailsWalletId').val(account.WalletId);
}

var accountLockState = true;
function checkAccountLockStatus(address) {

    var url = ApiUrl + '/IsAccountLocked';

    if (ActualWallet != {}) {
        var data = {
            'walletId' : ActualWallet.Id,
            'accountAddress': address
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
            if(data) {
                $('#accountDetailsModalIsLocked').html('<i class="fa fa-lock"></i> Locked');
                $('#btnAccountDetailsLockUnlock').html('<i class="fa fa-unlock"></i> Unlock');
                accountLockState = true;
            }
            else {
                $('#accountDetailsModalIsLocked').html('<i class="fa fa-unlock"></i> Unlocked');
                $('#btnAccountDetailsLockUnlock').html('<i class="fa fa-lock"></i> Lock');
                accountLockState = false;
            }
        },
        error: function (jqXhr, textStatus, errorMessage) { // error callback 
            console.log('Error: "' + errorMessage + '"');
        }
    });
}

function lockunlockAccount() {
    if(!accountLockState) {
        lockAccount();
    }
    else {

        $('#accountPasswordInput').bind("enterKey",function(e){
            var password = $('#accountPasswordInput').val();
            $('#accountPasswordInput').val('');
            unlockAccount(password);
            $('#unlockAccountModal').modal('hide');
        });
        $('#accountPasswordInput').keyup(function(e){
            if(e.keyCode == 13)
            {
                $(this).trigger("enterKey");
            }
        });
         
        $("#confirmUnlockAccountPassword").off();
        $("#confirmUnlockAccountPassword").click(function() {
            var password = $('#accountPasswordInput').val();
            $('#accountPasswordInput').val('');
            unlockAccount(password);
        });

        $('#accountPasswordInput').val('');
        $('#unlockAccountModal').modal('show');
    }
}

function showImportAccountKeyModal() {
    importAccountClearFields();

    $("#btnConfirmImportAccountKey").off();
    $("#btnConfirmImportAccountKey").click(function() {
        importAccountKey();
    });

    $('#importAccountKeyModal').modal('show');
}

function importAccountClearFields() {
    $('#importAccountKeyName').val('');
    $('#importAccountKeyName').val('');
    $('#importAccountKeyName').val('');
}

function importAccountKey() {

    var name = $('#importAccountKeyName').val();
    var key = $('#importAccountKeyKey').val();
    var password = $('#importAccountKeyPassword').val();

    if (key == undefined || key == null || key == '') {
        alert('You must fill the key!');
        $('#importAccountKeyModal').modal('show');
        return;
    }

    var alreadyEncrypted = false;
    if ($('#chbxAccountImportAlreadyEncryptedKey').is(':checked')) {
        alreadyEncrypted = true;
    }

    var url = ApiUrl + '/LoadAccountKey';

    if (ActualWallet != {} && ActualAccount != {}) {
        var data = {
            'walletId' : ActualWallet.Id,
            'accountAddress': ActualAccount.Address,
            'key' : key,
            'name': name,
            'password' : password,
            'isItMainAccountKey' : true,
            'alreadyEncrypted' : alreadyEncrypted
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
        },
        error: function (jqXhr, textStatus, errorMessage) { // error callback 
            console.log('Error: "' + errorMessage + '"');
            importAccountClearFields();
            checkAccountLockStatus(ActualAccount.Address);
        }
    });   
}

function unlockAccount(password) {

    var url = ApiUrl + '/UnlockAccount';

    if (ActualWallet != {} && ActualAccount != {}) {
        var data = {
            'walletId' : ActualWallet.Id,
            'accountAddress': ActualAccount.Address,
            'password' : password
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
            checkAccountLockStatus(ActualAccount.Address);
        },
        error: function (jqXhr, textStatus, errorMessage) { // error callback 
            console.log('Error: "' + errorMessage + '"');
        }
    });
}

function lockAccount() {
    
    var url = ApiUrl + '/LockAccount';

    if (ActualWallet != {} && ActualAccount != {}) {
        var data = {
            'walletId' : ActualWallet.Id,
            'accountAddress': ActualAccount.Address
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
            checkAccountLockStatus(ActualAccount.Address);
        },
        error: function (jqXhr, textStatus, errorMessage) { // error callback 
            console.log('Error: "' + errorMessage + '"');
        }
    });
}

var totalTokens = 0;

function loadTokens(account){

    var list = $('#accountDetailsTokensTable tbody');
    list.children().remove();
    totalTokens = 0;

    if (account != null) {
        for (var a in account.Tokens) {
            var tok = account.Tokens[a];

            totalTokens += tok.ActualBalance;

            list.append(
                '<tr>' +
                '<td>' + 
                    '<img style="width: 25px;margin-right: 10px;;" src="' + tok.ImageUrl + '" />' + tok.Symbol + 
                '</td>' +
                '<td>' + tok.ActualBalance + '</td>' +
                '<td>' +
                    '<div class="row">' + 
                        '<div class="col">' + 
                            '<button class="btn btn-primary" style="margin: 2px;" onclick=\'showTokenDetails("' + tok.Id + '")\'><i class="fa fa-info-circle"></i></button>' +
                        '</div>' +
                    '</div>' +
                '</td>' +
                '</tr>');
        }
    }

    $('#accountDetailsTotalPLFTokens').text(totalTokens.toString());
}

function getAccountTypes() {
    var url = ApiUrl + "/GetAccountTypes";

    if (bootstrapstudio) {
        url = url.replace(':8000',':8080'); //just for debug with BootstrapStudio
    }

    $.ajax(url,
        {
            method: 'GET',
            dataType: 'json',   // type of response data
            timeout: 10000,     // timeout milliseconds
            success: function (data, status, xhr) {   // success callback function

                AccountTypes = data;
            },
            error: function (jqXhr, textStatus, errorMessage) { // error callback 
                console.log("Cannot load configuration");
            }
        });
}

function getAccountTypeByIndx(typeIndx) {
    if (AccountTypes != null) {
        if (AccountTypes.length > typeIndx) {
            return AccountTypes[typeIndx];
        }
    }
}

function getAccountTypeByName(name) {
    if (AccountTypes != null) {
        for(var i = 0; i < AccountTypes.length; i++) {
            if (AccountTypes[i] == name) {
                return i;
            }
        }
    }
}


///////////////////////////////////////
// Dropdowns
var selectedNewAccountType = 0;

function setNewAccountType(accountType) {
    selectedNewAccountType = accountType;
    var a = getAccountTypeByIndx(accountType);
    if (a != null) {
        document.getElementById('btnNewAccountType').innerText = a;
    }
}

function reloaNewAccountTypesDropDown() {
    if (AccountTypes != null) {
        document.getElementById('newAccountTypesDropDown').innerHTML = '';
        for (var act in AccountTypes) {
            var a = getAccountTypeByIndx(act);
            document.getElementById('newAccountTypesDropDown').innerHTML += '<button style=\"width: 400px;font-size:12px\" class=\"dropdown-item btn btn-light\" ' +  'onclick=\"setNewAccountType(\'' + act + '\')\">' + a + '</button>';
        }
    }
}

///////////////////////////////////////////////////////////
// account transactions
var transactions = {};

function loadAccountTransactions(account) {

    var list = $('#accountTransactionsTable tbody');
    list.children().remove();
   
    if (account != null) {

        var data = {
            "walletId": ActualWallet.Id,
            "accountAddress": account.Address,
            "maxItems": 1000
        };

        var url = ApiUrl + "/GetAccountTransactions";

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
                    transactions = data;
                    if (transactions != null) {                    
                        for (var tr in transactions) {
                            var trx = transactions[tr];
                
                            var withTokens = "No";
                            if (trx.VinTokens != null && trx.VoutTokens != null) {
                                withTokens = "Yes";
                            }
                
                            var direction = "In";
                            if (trx.Direction == 1) {
                                direction = "Out";
                            }
                
                            list.append(
                                '<tr>' +
                                '<td>' + '...' + trx.TxId.substring(trx.TxId.length-5) + '</td>' +
                                '<td>' + direction + '</td>' +
                                '<td>' + parseFloat(trx.Amount).toString() + '</td>' +
                                '<td>' + withTokens + '</td>' +
                                '<td>' +
                                    '<div class="row">' + 
                                        '<div class="col">' + 
                                            '<button class="btn btn-primary" style="margin: 2px; width: 80px;" onclick=\'showTxDetails("' + trx.TxId + '")\'><i class="fa fa-info-circle"></i></button>' +
                                        '</div>' +
                                    '</div>' +
                                '</td>' +
                                '</tr>');
                        }  
                    }             
                },
                error: function (jqXhr, textStatus, errorMessage) { // error callback 
                    console.log('Loading Transactions Error: "' + errorMessage + '"');
                }
            });
    }
}

function showAccountTransactions() {
    loadAccountTransactions(ActualAccount);
    $('#accountTransactionsListModal').modal('show');
}

function showTxDetails(txId) {
    for (var tr in transactions) {
        var trx = transactions[tr];
        if (trx.TxId == txId) {
            $('#txDetailsWalletName').text(ActualWallet.Name);

            if (trx.Direction == 0) {
                $('#txDetailsDirection').text('Incoming');
            }
            else {
                $('#txDetailsDirection').text('Outgoing');
            }

            if (trx.From != null) {
                if (trx.From[0] != null) {
                    $('#txDetailsAccountAddress').text(trx.From[0]);
                }
            }

            if (trx.To != null) {
                if (trx.To[0] != null) {
                    $('#txDetailsRecipientAccountAddress').text(trx.To[0]);
                }
            }
            
            $('#txDetailsAmount').text(trx.Amount.toString());
            $('#txDetailsTxId').text(trx.TxId);
            $('#txDetailsLinkToExplorer').attr('href', 'https://explorer.nebl.io/tx/' + txId);
            
            $("#btnDownloadTxDetails").off();
            $("#btnDownloadTxDetails").click(function() {
                getTxReceipt(ActualWallet.Name, trx.AccountAddress, txId);
            });
            
            $('#transactionDetailsModal').modal('show');
        }
    }
}

function getTxReceipt(walletId, accountAddr, txid) {
    var url = ApiUrl + "/GetTxReceipt";

    if (bootstrapstudio) {
        url = url.replace('8000','8080');
    }

    walletId = "41ea1423-199f-432c-af3d-9b6181f77f3b";
    accountAddr = "NPWBL3i8ZQ8tmhDtrixXwYd93nofmunvhA";
    txid = "afbd2731720260c0cccfacd5123cbb3af112306b2995735bb40550f9003f64ab";

    var data = {
        "walletId": walletId,
        "accountAddress": accountAddr,
        "txId": txid
    };

    $.ajax(url,
        {
            contentType: 'application/json;charset=utf-8',
            data: JSON.stringify(data),
            method: 'PUT',
            dataType: 'json',   // type of response data
            timeout: 10000,     // timeout milliseconds
            success: function (data, status, xhr) {   // success callback function
                //console.log(`Status: ${status}, Data:${data}`);
                downloadReceiptPageAsFile(data.htmlReceipt);
            },
            error: function (jqXhr, textStatus, errorMessage) { // error callback 
                console.log('Error: "' + errorMessage + '"');
            }
        });
}

function downloadReceiptPageAsFile(data) {
    var fn = 'TxReceipt-' + Date.now().toString() + '.html';
    downloadDataAsTextFile(data, fn);
}

/////////////////////
// sending currency transaction

function fillAndShowSendTxModal() {    
    $('#sendTxModalWalletName').text(ActualWallet.Name);
    $('#sendTxModalAccountAddress').text(ActualAccount.Name + ' - ' + ActualAccount.Address);
    $('#sendTxModal').modal('show');
}

function prepareSendTx() {
    if (accountLockState) {

        $('#unlockAccountForOneTxPassword').bind("enterKey",function(e){
            var password = $('#unlockAccountForOneTxPassword').val();
            sendTx(password);
            $('#addPassForTxMessageModal').modal('hide');
        });
        $('#unlockAccountForOneTxPassword').keyup(function(e){
            if(e.keyCode == 13)
            {
                $(this).trigger("enterKey");
            }
        });

        $('#unlockAccountForOneTxConfirm').off();
        $("#unlockAccountForOneTxConfirm").click(function() {
            var password = $('#unlockAccountForOneTxPassword').val();
            sendTx(password);
        });

        $('#addPassForTxMessageModal').modal('show');
    }
    else {
        sendTx(null);
    }
}

function sendTx(password) {

    var add = $('#sendTxModalReceiverAddress').val();
    if (add == '') {
        alert('Address Cannot Be empty');
        return;
    }

    var amount = parseFloat($('#sendTxModalAmount').val());
    if (amount == 0) {
        alert('Cannot send 0 Nebl');
        return;
    }

    var pass = '';
    if (password != null) {
        pass = password;
    }

    var data = {
        "ReceiverAddress": add,
        "SenderAddress": ActualAccount.Address,
        "Symbol": "NEBL",
        "CustomMessage": '',
        "Amount": amount,
        "Password": pass
    };

    $("#confirmButtonOk").off();
    $("#confirmButtonOk").click(function() {
        sendTxApiCommand('SendNeblioTx', data);
    });
    
    ShowConfirmModal('', 'Do you realy want to send this transaction?');  

}

function sendTxApiCommand(apicommand, data) {
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


class NeblioAddressBanner {
    constructor(address) {
        this.address = address;
        this.balance = 0.0;
        this.balanceInSat = 0.0;
        this.unconfirmedBalance = 0.0;
        this.unconfirmedBalanceInSat = 0.0;
        this.unconfirmedTx = 0.0;
        this.totalTx = 0.0;
        this.totalSent = 0.0;
        this.totalSentSat = 0.0;
        this.totalReceived = 0.0;
        this.totalReceivedSat = 0.0;

        this.getInfo();

        setInterval(() => {
            this.getInfo();
        }, 2500);
    }

    getInfo = function() {
        if (this.address != '' || this.address != ' ') {
            var url = 'https://ntp1node.nebl.io/ins/addr/' + this.address;
            var thisClass = this;
            $.ajax(url,
            {
                contentType: 'application/json;charset=utf-8',
                method: 'GET',
                dataType: 'json',   // type of response data
                timeout: 10000,     // timeout milliseconds
                success: function (data, status, xhr) {   // success callback function
                    //console.log(data);
                    if (data != null){
                        thisClass.balance = data.balance;
                        thisClass.balanceInSat = data.balanceSat;
                        thisClass.unconfirmedBalance = data.unconfirmedBalance;
                        thisClass.unconfirmedBalanceInSat = data.unconfirmedBalanceSat;
                        thisClass.unconfirmedTx = data.unconfirmedTxApperances;
                        thisClass.totalTx = data.txApperances;
                        thisClass.totalSent = data.totalSent;
                        thisClass.totalSentSat = data.totalSentSat;
                        thisClass.totalReceived = data.totalReceived;
                        thisClass.totalReceivedSat = data.totalReceivedSat;
                    }
                },
                error: function (jqXhr, textStatus, errorMessage) { // error callback 
                    console.log('Error: "' + errorMessage + '"');
                }
            });
        }
    }
    
}