
$(document).ready(function () {

    $("#btnDeleteWallet").off();
    $("#btnDeleteWallet").click(function() {
        DeleteWallet();
    });

    $("#btnUpdateWallet").off();
    $("#btnUpdateWallet").click(function() {
        UpdateWallet();
    });

    try {
        getWalletTypes();
    }
    catch {
        console.log('Cannot load wallet types from API.');
    }

});

var selectedWalletType = 'Neblio';
var actualWalletID = '';

function setWalletType(type) {
    console.log(type);
    selectedWalletType = type;
    document.getElementById('btnWalletType').innerText = type;
}

function getWalletTypes() {

    var url = ApiUrl + '/GetWalletTypes/';
    
    console.log('sending...: ' + url);

    $.get(url, function (data, status) {
        console.log(status);
        console.log(data);

        WalletTypes = data;
        
        try{
            document.getElementById('walletTypesDropDown').innerHTML = '';

            for (var i = 0; i < data.length; i++ ) {
            
                document.getElementById('walletTypesDropDown').innerHTML += '<button class=\"dropdown-item btn btn-light\" ' +  'onclick=\"setWalletType(\'' + data[i].toString() + '\')\">' + data[i] + '</button>';
            }
        }
        catch{
        }
    });
}

function loadWalletDetails(w) {

    document.getElementById('walletId').value = w.Id;
    document.getElementById('btnWalletType').innerText = GetWalletType(w.Type);
    document.getElementById('walletName').value = w.Name;
    document.getElementById('walletHost').value = w.ConnectionUrlBaseAddress;
    document.getElementById('walletPort').value = w.ConnectionPort;
    $('#walletDetailsTotalNEBL').text(w.TotalBalance.toString());
    getAllTokenCount();
    $('#walletDetailsTotalPLFTokens').text(totalTokensInWallet.toString());
}

function showWalletDetails(id) {
    actualWalletID = id;
    ActualWallet = Wallets[id];
    loadWalletDetails(ActualWallet);
    fillWalletDetails(ActualWallet);
    loadAccounts(ActualWallet);
    $('#walletDetailsModal').modal("show"); 
}

function fillWalletDetails(wallet) {
    
    $('#walletDetailsId').val(wallet["Id"]);
    $('#walletDetailsName').val(wallet["Name"]);
    $('#walletDetailsHost').val(wallet["ConnectionUrlBaseAddress"]);
    $('#walletDetailsPort').val(wallet["ConnectionPort"]);
    $('#walletDetailsType').text(GetWalletType(wallet.Type));
}

var totalTokensInWallet = 0;

function getAllTokenCount() {
    totalTokensInWallet = 0;
    if (ActualWallet != null) {
        if (ActualWallet.Accounts != null) {
            for (var a in ActualWallet.Accounts) {
                if (ActualWallet.Accounts[a].Tokens != null) {
                    for(var t in ActualWallet.Accounts[a].Tokens) {
                        totalTokensInWallet += ActualWallet.Accounts[a].Tokens[t].ActualBalance;
                    }
                }
            }
        }
    }
}

function WalletsRefresh(wallets){

    Wallets = wallets;

    if (actualWalletID != '') {
        if (ActualWallet != null || ActualWallet != undefined) {
            //loadWalletDetails(ActualWallet);
            //fillWalletDetails(ActualWallet);
            loadAccounts(ActualWallet);
        
            ActualWallet = Wallets[actualWalletID];
            
            if (actualAccountId != '') {
                ActualAccount = ActualWallet.Accounts[actualAccountId];

                if (ActualAccount != null || ActualAccount != undefined) {
                    fillAccountDetails(ActualAccount, true);
                    loadTokens(ActualAccount);
                }
            }
        }
    }

    if (document.getElementById('walletList') != null) {
        document.getElementById('walletList').innerHTML = '';
    }

    for (var wall in wallets){

        var w = wallets[wall];

        document.getElementById('walletList').innerHTML +=
        '<tr><td>' +
        w['Id'] + ', ' +
        w['Name'] + ', ' +
        GetWalletType(w['Type']) + ', ' +
        w['ConnectionUrlBaseAddress'] + '&nbsp;</td><td>' +
        '<button class="btn btn-success rights-admin" onclick=\'showWalletDetails(\"' + w['Id'] + '\")\'>Load Details</button>' +
        '</td></tr>';
    }

    HideVisibleItemsByRights();

}

function UpdateWallet() {

    var wallet = {
        "walletId": $('#walletId').val(),
        "walletName": $('#walletName').val(),
        "walletBaseHost": $('#walletHost').val(),
        "walletPort": $('#walletPort').val(),
        "walletType": GetWalletTypeByName(selectedWalletType)
     };
 
     $("#confirmButtonOk").off();
     $("#confirmButtonOk").click(function() {
        ComandAPIRequest('UpdateWallet', wallet);
    });
    
    ShowConfirmModal('', 'Do you realy want to add this wallet: '+ wallet.walletName +'?');  
}

function DeleteWallet() {

    var wallet = {
        "walletId": $('#walletId').val(),
        //todo checkbox
        "withAccounts": $('#deleteWalletWithAccounts').val()
     };
 
     $("#confirmButtonOk").off();
     $("#confirmButtonOk").click(function() {
        ComandAPIRequest('DeleteWallet', wallet);
    });
    
    ShowConfirmModal('', 'Do you realy want to remove this wallet: '+ wallet.Id +'?');  
}

function GetWalletType(wallettype) {
    if (WalletTypes != null && wallettype != null) {
        return WalletTypes[wallettype];
    }
    else {
        return "Neblio";
    }
}

function GetWalletTypeByName(wallettype) {
    if (WalletTypes != null && wallettype != null) {
        var type = 1;
        for (var i = 0; i < WalletTypes.length; i++) {
            if (WalletTypes[i] == wallettype) {
                type = i;
            }
        }

        return type;
    }
    else {
        return 1;
    }
}

function showNewTxDetails(NewTx) {

    fillTxDetails(NewTx);
    $('#newTransactionModal').modal("show"); 

    setTimeout(function(){
        $('#newTransactionModal').modal("toggle"); 
    },5000);
}

function fillTxDetails(NewTx) {
    
    $('#newTxModalAccountAddress').text(NewTx["AccountAddress"]);
    $('#newTxModalAccountName').text(NewTx["WalletName"]);
    $('#newTxModalTxId').text(NewTx["TxId"]);
}

function showNewTxConfDetails(NewTx) {

    fillTxConfDetails(NewTx);
    $('#newTransactionConfirmedModal').modal("show"); 

    setTimeout(function(){
        $('#newTransactionConfirmedModal').modal("toggle"); 
    },5000);
}

function fillTxConfDetails(NewTx) {
    
    $('#newTxConfModalAccountAddress').text(NewTx["AccountAddress"]);
    $('#newTxConfModalAccountName').text(NewTx["WalletName"]);
    $('#newTxConfModalTxId').text(NewTx["TxId"]);
    $('#newTxConfModalTokens').text(NewTx["TxId"]);
}

function showNewTokensDetails(NewTx) {

    fillTokensDetails(NewTx);
    $('#newTokensReceivedModal').modal("show"); 

    setTimeout(function(){
        $('#newTokensReceivedModal').modal("toggle"); 
    },5000);
}

function fillTokensDetails(NewTx) {
    
    $('#newTokenModalSymbolName').text(NewTx["token"]["Symbol"] + ' - ' + NewTx["token"]["Name"]);
    $('#newTokensModalAccountAddress').text(NewTx["AccountAddress"]);
    $('#newTokensModalWalletName').text(NewTx["WalletName"]);
    
    $('#newTokensModalAmount').text(NewTx["token"]["ActualBalance"].toString());
}