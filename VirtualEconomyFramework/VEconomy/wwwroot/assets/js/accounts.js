
$(document).ready(function () {

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

    try {
        getAccountTypes();
    }
    catch {
        console.log('Cannot load wallet types from API.');
    }

});


function AddNewAccount() {

    $("#btnAddNewAccountConfirm").off();
    $("#btnAddNewAccountConfirm").click(function() {

        var justToDb = true;

        if(!$("#chboxSaveNewAccountJustToDb").is(':checked')) {
            justToDb = false;
        }
        
        var acc = {
            "walletId": $('#newAccountWalletId').val(),
            "Address": $('#newAccountAddress').val(),
            "Name": $('#newAccountName').val(),
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

        var acc = {
            "walletId": ActualWallet.Id,
            "accountAddress": account.Address,
            "accountName": account.Name,
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
            list.append(
                '<tr>' +
                '<td>' + acc.Name + ', ' + acc.Address + '</td>' +
                '<td>' +
                    '<div class="row">' + 
                        '<div class="col">' + 
                            '<button class="btn btn-primary" style="margin: 2px; width: 80px;" onclick=\'showAccountDetails("' + acc.Address + '")\'>Details</button>' +
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
    
    $('#accountDetailsAddress').val(account["Address"]);

    if (!refresh) {
        $('#accountDetailsName').val(account["Name"]);
    }

    $('#accountDetailsNumOfTx').text(account["NumberOfTransaction"].toString());
    
    $('#accountDetailsTotalNEBL').text(account["TotalBalance"].toString());
    $('#accountDetailsWalletId').val(account["WalletId"]);
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
                '<td>' + tok.Symbol + '</td>' +
                '<td>' + tok.ActualBalance + '</td>' +
                '<td>' +
                    '<div class="row">' + 
                        '<div class="col">' + 
                            '<button class="btn btn-primary" style="margin: 2px; width: 80px;" onclick=\'showTokenDetails("' + tok.Id + '")\'>Details</button>' +
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
                                '<td>' + trx.TxId + '</td>' +
                                '<td>' + direction + '</td>' +
                                '<td>' + trx.Amount.toString() + '</td>' +
                                '<td>' + withTokens + '</td>' +
                                '<td>' +
                                    '<div class="row">' + 
                                        '<div class="col">' + 
                                            '<button class="btn btn-primary" style="margin: 2px; width: 80px;" onclick=\'showTxDetails("' + trx.TxId + '")\'>Details</button>' +
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