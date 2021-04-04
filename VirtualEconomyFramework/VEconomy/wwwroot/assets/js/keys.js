

function keysAfterLoad() {

    $("#btnLoadAccountKeys").off();
    $("#btnLoadAccountKeys").click(function() {
        ReloadAcountKeys();
    });

    $('#btnDecryptMessage').off();
    $("#btnDecryptMessage").click(function() {
        showDecryptMessage();
    });

    $('#btnKeysExportKey').off();
    $("#btnKeysExportKey").click(function() {
        addNewBasicKey();
    });

    $('#btnKeysAddNewKey').off();
    $("#btnKeysAddNewKey").click(function() {
        if (selectedKeysAccountAddress == '') {
            alert('Select account please!');
            return;
        }
        addNewBasicKey();
    });
    
}

var AccountKeys = {};
var selectedKey = {};
var decryptedMessage = {};

function showKey(keyId) {

    selectedKey = AccountKeys[keyId];
    $('#keyModalKeyType').text(selectedKey.Type);
    $('#keyModalKeyName').text(selectedKey.KeyName);
    $('#keyModalKeyId').text(selectedKey.Id);
    
    $('#keyModal').modal('show');
}

function getKeyComponent(key) {
    
    var type = 'Basic Key';
    if (key.Type == 1) {
        type = 'Account Key';
    }


    var keyComponent = '<div class="card shadow" style="min-width: 350px;">'+
    '    <div class="card-header d-xl-flex justify-content-xl-center align-items-xl-center py-3">'+
    '        <h4>' + key.KeyName + ' - Key</h4>'+
    '    </div>'+
    '    <div class="card-body" style="padding-bottom: 0px;">'+
    '        <div class="col">'+
    '            <div class="row" style="margin-bottom: 30px;">'+
    '                <div class="col">'+
    '                    <div class="row">'+
    '                        <div class="col d-flex d-xl-flex justify-content-center justify-content-xl-center align-items-xl-center">' +
    '                           <span>Name</span>'+
    '                        </div>'+
    '                    </div>'+
    '                    <div class="row d-flex d-xl-flex justify-content-center justify-content-xl-center" style="margin-top: 5px;">'+
    '                        <div class="col-auto d-flex d-xl-flex justify-content-center justify-content-xl-center align-items-xl-center">'+
    '                           <span>' + key.KeyName + '</span>'+
    '                        </div>'+
    '                        <div class="col-auto d-flex d-xl-flex justify-content-center justify-content-xl-center align-items-xl-center">'+
    '                           <button onclick="changeKeyName(\'' + key.KeyId + '\')" class="btn btn-primary" type="button" style="width: 20px;height: 20px;padding-left: 0px;padding-right: 0px;padding-top: 0px;padding-bottom: 0px;font-size: 12px;">'+
    '                               <i class="fa fa-pencil" style="font-size: 10px;"></i>'+
    '                           </button>'+
    '                        </div>'+
    '                    </div>'+
    '                    <div class="row">'+
    '                        <div class="col d-flex d-xl-flex justify-content-center justify-content-xl-center align-items-xl-center" style="margin-top: 10px;">'+
    '                           <span>Type</span>'+
    '                        </div>'+
    '                    </div>'+
    '                    <div class="row">'+
    '                        <div class="col d-flex d-xl-flex justify-content-center justify-content-xl-center align-items-xl-center">'+
    '                           <span style>'+ type + '</span>'+
    '                        </div>'+
    '                    </div>'+
    '                    <div class="row">'+
    '                        <div class="col d-flex d-xl-flex justify-content-center justify-content-xl-center align-items-xl-center" style="margin-top: 10px;">'+
    '                           <span>Id</span>'+
    '                        </div>'+
    '                    </div>'+
    '                    <div class="row">'+
    '                       <div class="col d-flex d-xl-flex justify-content-center justify-content-xl-center align-items-xl-center">'+
    '                           <span style="font-size: 12px;">' + key.KeyId + '</span>'+
    '                       </div>'+
    '                    </div>'+
    '                </div>'+
    '            </div>'+
    '        </div>'+
    '    </div>'+
    '    <div class="card-footer" style="padding-top: 5px;">'+
    '        <div class="row">'+
    '            <div class="col d-flex d-xl-flex justify-content-center justify-content-xl-center align-items-xl-center">'+
    '               <button class="btn btn-primary" type="button" onclick="downloadKey(\'' + key.KeyId + '\')">'+
    '                   <i class="fa fa-save"></i>'+
    '               </button>'+
    '            </div>'+
    '            <div class="col d-flex d-xl-flex justify-content-center justify-content-xl-center align-items-xl-center">'+
    '               <button class="btn btn-danger" type="button" onclick="deleteKey(\'' + key.KeyId + '\')">'+
    '                   <i class="fa fa-trash-o"></i>'+
    '               </button>'+
    '            </div>'+
    '        </div>'+
    '    </div>'+
    '</div>';
    
    return keyComponent;
}

function ReloadAcountKeys() {

    if (selectedKeysAccountAddress == null || selectedKeysAccountAddress == '') {
        alert('Please select the account address!');
        return;
    }

    $("#confirmButtonOk").off();
    $("#confirmButtonOk").click(function() {
        keysReloadAccountKeys();
    }); 

    ShowConfirmModal('', 'Do you realy want load all account keys?');

}

function refreshKeys() {

    $('#keysCards').empty();
    $('#keysCards').append('<div class="row"><div class="col"><div id="keysCardsRow" class="row d-flex justify-content-center"></div></div></div>');
    for (var t in AccountKeys) {
        var tok = AccountKeys[t];
        var drawIt = false;
        var filterActive = false;

        if (tok != undefined && tok != null) {

            // todo get tx details and check sender address
            
            var filter = $('#keysListSearchByNameAddress').val();
            if (filter != '' && filter != ' ') {
                filter = filter.toLowerCase();
                if (tok.KeyName.toLowerCase().includes(filter) || tok.KeyId.toLowerCase().includes(filter)) {
                    filterActive = true;
                }
            }
            else {
                filterActive = true;
            }
                        
            if ($('#chbxKeysListAccountKey').is(':checked')) {
                if (tok.Type == 1) {
                    drawIt = true;
                }
            }

            if ($('#chbxKeysListBasicKey').is(':checked')) {
                if (tok.Type == 0) {
                    drawIt = true;
                }
            }

            if (drawIt && filterActive) {
                $('#keysCardsRow').append(
                    '<div class="col-auto">' +
                    getKeyComponent(tok) +
                    '</div>'
                );
            }
        }
    }		
}

var selectedKeysAccountAddress = '';
var selectedKeyWallet = '';

function setKeysListAccountAddress(accountAddress) {
    selectedKeysAccountAddress = accountAddress;
    var a = Accounts[accountAddress];
    selectedKeyWallet = a.WalletId;

    //checkAccountLockStatus(accountAddress);

    if (a != null) {
        document.getElementById('btnAccountAddressToLoadKeys').innerText = accountAddress;
    }
}

function reloadKeysListAccountsAddressesDropDown() {
    if (Accounts != null) {
        document.getElementById('keysListAccountAddressesDropDown').innerHTML = '';
        for (var acc in Accounts) {
            var a = Accounts[acc];     
            var add = acc.substring(0,3) + '...' + acc.substring(acc.length-3);      
            document.getElementById('keysListAccountAddressesDropDown').innerHTML += '<button style=\"font-size:12px\" class=\"dropdown-item btn btn-light\" ' +  'onclick=\"setKeysListAccountAddress(\'' + acc + '\')\">' + a.Name + ' - ' + add + '</button>';
        }
    }
}


////////////
// load account keys
function keysReloadAccountKeys() {
    var url = document.location.origin + '/api/GetAccountKeys/' + selectedKeysAccountAddress;

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
                    AccountKeys = data;
                    refreshKeys();
                }
            },
            error: function (jqXhr, textStatus, errorMessage) { // error callback 
                console.log('Error: "' + errorMessage + '"');
            }
        });
}

/////////////////////////////////
// add new message key

function keysAddNewMessageKey() {
    importAccountClearFields();

    $("#btnConfirmImportAccountKey").off();
    $("#btnConfirmImportAccountKey").click(function() {
        keysAddNewMessageKeyRequest();
    });

    $('#importAccountKeyModal').modal('show');
}

function keysAddNewMessageKeyRequest() {

    var name = $('#importAccountKeyName').val();
    var key = $('#importAccountKeyKey').val();
    var password = $('#importAccountKeyPassword').val();

    var url = ApiUrl + '/LoadAccountKey';

    if (ActualWallet != {} && ActualAccount != {}) {
        var data = {
            'walletId' : selectedKeyWallet,
            'accountAddress': selectedKeysAccountAddress,
            'key' : key,
            'name': name,
            'password' : password,
            'isItMainAccountKey' : false
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
            //checkAccountLockStatus(ActualAccount.Address);
            ReloadAcountKeys();
        },
        error: function (jqXhr, textStatus, errorMessage) { // error callback 
            console.log('Error: "' + errorMessage + '"');
            importAccountClearFields();
            //checkAccountLockStatus(ActualAccount.Address);
        }
    }); 
}

///////////////////////////////
// change Key name

function changeKeyName(keyId) {
    var key = AccountKeys[keyId];

    if (key != null) {
        $('#changeKeyNameInput').val(key.KeyName);

        $("#btnChangeKeyNameConfirm").off();
        $("#btnChangeKeyNameConfirm").click(function() {

            $("#confirmButtonOk").off();
            $("#confirmButtonOk").click(function() {
                
                var newName = $('#changeKeyNameInput').val();

                if (newName == '' || newName == ' ') {
                    alert('Name cannot be empty!');
                    return;
                }

                changeAccountNameAPI(keyId, newName);
            }); 

            ShowConfirmModal('', 'Do you realy want change key name?');
        }); 

        $('#changeKeyNameModal').modal('show');
    }
}


function changeAccountNameAPI(keyId, newName) {
    var url = ApiUrl + '/ChangeKeyName';

    var data = {
        'walletId' : selectedKeyWallet,
        'accountAddress': selectedKeysAccountAddress,
        'keyId' : keyId,
        'newName': newName,
    };

    if (bootstrapstudio) {
        url = url.replace('8000','8080');
    }

    $.ajax(url,
    {
        contentType: 'application/json;charset=utf-8',
        data: JSON.stringify(data),
        method: 'PUT',
        dataType: 'text',   // type of response data
        timeout: 10000,     // timeout milliseconds
        success: function (data, status, xhr) {   // success callback function
            console.log(`Status: ${status}, Data:${data}`);
            $('#changeKeyNameInput').val('');
            setTimeout(() => {
                keysReloadAccountKeys();
            }, 500);
        },
        error: function (jqXhr, textStatus, errorMessage) { // error callback 
            console.log('Error: "' + errorMessage + '"');
        }
    }); 
}


/////////////////////////////////////////////////////
// Download the key

function downloadKey(keyId) {
    var key = AccountKeys[keyId];

    if (key != null) {
        
        $("#confirmButtonOk").off();
        $("#confirmButtonOk").click(function() {
            downloadKeyAPI(keyId);
        }); 

        ShowConfirmModal('', 'Do you realy want download the key? It will be downloaded in encrypted form');
    }
}


function downloadKeyAPI(keyId) {
    var url = ApiUrl + '/DownloadKey';

    var data = {
        'walletId' : selectedKeyWallet,
        'accountAddress': selectedKeysAccountAddress,
        'keyId' : keyId
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
            console.log(`Status: ${status}, Data:${data}`);

            downloadKeyDataAsFile(data);
        },
        error: function (jqXhr, textStatus, errorMessage) { // error callback 
            console.log('Error: "' + errorMessage + '"');

        }
    }); 
}

function downloadKeyDataAsFile(data) {

    var fn = 'Key - ' + data.Name + ' - ' + Date.now().toString() + '.json';
    downloadDataAsTextFile(JSON.stringify(data, null, 2), fn);
}

/////////////////////////////////////////////////////
// Delete the key

function deleteKey(keyId) {
    var key = AccountKeys[keyId];

    if (key != null) {
        
        $("#confirmButtonOk").off();
        $("#confirmButtonOk").click(function() {
            deleteKeyAPI(keyId);
        }); 

        ShowConfirmModal('', 'Do you realy want delete the key? You will not be able to use it anymore.');
    }
}


function deleteKeyAPI(keyId) {
    var url = ApiUrl + '/DeleteKey';

    var data = {
        'walletId' : selectedKeyWallet,
        'accountAddress': selectedKeysAccountAddress,
        'keyId' : keyId
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
            console.log(`Status: ${status}, Data:${data}`);

            downloadKeyDataAsFile(data);

            setTimeout(() => {
                keysReloadAccountKeys();
            }, 500);
        },
        error: function (jqXhr, textStatus, errorMessage) { // error callback 
            console.log('Error: "' + errorMessage + '"');

        }
    }); 
}

///////////////////////////////
// add new basic key

/////////////////////////////////
// add new message key

function addNewBasicKey() {
    importAccountClearFields();

    $("#btnConfirmImportAccountKey").off();
    $("#btnConfirmImportAccountKey").click(function() {
        addNewBasicKeyRequest();
    });

    $('#importAccountKeyModal').modal('show');
}

function addNewBasicKeyRequest() {

    var name = $('#importAccountKeyName').val();
    var key = $('#importAccountKeyKey').val();
    var publicKey = $('#importAccountKeyPublicKey').val();
    
    var password = $('#importAccountKeyPassword').val();

    var url = ApiUrl + '/LoadAccountKey';

    var alreadyEncrypted = false;
    if ($('#chbxKeysImportAlreadyEncryptedKey').is(':checked')) {
        alreadyEncrypted = true;
    }

    if (ActualWallet != {} && ActualAccount != {}) {
        var data = {
            'walletId' : selectedKeyWallet,
            'accountAddress': selectedKeysAccountAddress,
            'key' : key,
            'pubkey': publicKey,
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

            setTimeout(() => {
                keysReloadAccountKeys();
            }, 500);
        },
        error: function (jqXhr, textStatus, errorMessage) { // error callback 
            console.log('Error: "' + errorMessage + '"');
            importAccountClearFields();

            setTimeout(() => {
                keysReloadAccountKeys();
            }, 500);
        }
    }); 
}