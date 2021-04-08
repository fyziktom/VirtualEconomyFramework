
function shopOnLoad(_isShopHostedOnVEF) {

    isShopHostedOnVEF = _isShopHostedOnVEF;
    /*
    // create data for shop item
    var uid = 'f65be0d4-bd51-4b0c-a855-f619d30b71f4';
    var name = 'CHESS Token For sale';
    var description = 'CHESS Token can be used in VEFramework chess game';
    var tokenSymbol = 'CHESS';
    var tokenId = 'La46fzZotg6wgr2DncwfuakqadHanPWFTARqRL';
    var lot = 10;
    var price = 10000000;
    var address = 'NPWBL3i8ZQ8tmhDtrixXwYd93nofmunvhA';
    var CHESSItem = new ShopItem(uid, address, 0, tokenSymbol, tokenId, lot, price, name, description);

    // create data for shop item
    var uid1 = 'cecd86f6-bd5e-4735-9092-908bb6700548';
    var name = 'CART Token For sale';
    var description = 'CART Token nft';
    var tokenSymbol = 'CART';
    var tokenId = 'La8N1QroEDxxjkKYaPdPzatRj12nvRnL9JbUei';
    var lot = 1;
    var price = 100000000;
    var address = 'NikErRpjtRXpryFRc3RkP5nxRzm1ApxFH8';
    var MSGTItem = new ShopItem(uid, address, 0, tokenSymbol, tokenId, lot, price, name, description);
    */

    //initShop('NikErRpjtRXpryFRc3RkP5nxRzm1ApxFH8');

    $("#btnShopAddNewShopItem").off();
    $("#btnShopAddNewShopItem").click(function() {

        if (selectedShopAccountAddress == '' || selectedShopAccountAddress == ' ') {
            alert('Please select the address first!');
            return;
        }
        $('#mintNewNFTModal').modal('show');
    });

    $("#btnConfirmMintNFT").off();
    $("#btnConfirmMintNFT").click(function() {

        if (selectedMintReceiverAccountAddress == selectedShopAccountAddress) {
            alert('Receiver must be different than address which creates NFT!');
            return;
        }

        createNewItem();
    });
    
    // add to shop items object and refresh 
    //AccountShopItems[uid] = CHESSItem;
    //AccountShopItems[uid1] = MSGTItem;
    refreshItems();
    
    // start auto refreshing each 2s
    setInterval(() => {
        for (var sid in AccountShopItems) {
            // load actual data from Neblio API
            AccountShopItems[sid].refreshAddressInfo();           
        }
        setTimeout(() => {
            // draw items
            //refreshShopItems();
            refreshShopItemNFTs();
            refreshShopItemNFTRequests();
        }, 500);
        
    }, 2000);
}

function refreshItems() {
    for (var sid in AccountShopItems) {
        // load actual data from Neblio API
        AccountShopItems[sid].refreshAddressInfo();           
    }
    setTimeout(() => {
        // draw items
        //refreshShopItems();
        nfts = {};
        $('#shopNFTItemsCards').empty();
        refreshShopItemNFTs();
        refreshShopItemNFTRequests();
    }, 500);
}

var isShopHostedOnVEF = false;
var isAsShop = false;

function uuidv4() {
    return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function(c) {
      var r = Math.random() * 16 | 0, v = c == 'x' ? r : (r & 0x3 | 0x8);
      return v.toString(16);
    });
  }

function initShop(address) {
        AccountShopItems = {};
        $('#shopNFTItemsCards').empty();
        // create data for shop item
        var uid = uuidv4();
        var name = 'CART Token';
        var description = 'CART Token nft';
        var tokenSymbol = 'CART';
        var tokenId = 'La8N1QroEDxxjkKYaPdPzatRj12nvRnL9JbUei';
        var lot = 1;
        var price = 100000000;
        var MSGTItem = new ShopItem(uid, address, 0, tokenSymbol, tokenId, lot, price, name, description, isShopHostedOnVEF);
    
        // add to shop items object and refresh 
        //AccountShopItems[uid] = CHESSItem;
        AccountShopItems[uid] = MSGTItem;
}

var AccountShopItems = {};

function requestNeblioNFTTrade(owner, tokenId, utxoTxId, firstNFTUtxo, symbol, imageSrc) {

    if (!isShopHostedOnVEF) {
        alert('This shop is not hosted on VEF you cannot create new item now!');
        return;
    }

    $("#btnConfirmRequestNFT").off();
    $("#btnConfirmRequestNFT").click(function() {
        var sender = selectedNFTTradeRequestSenderAccountAddress;
        var message = $('#requestNFTModalMessage').val();

        sendNeblioNFTrequestAPI(owner, sender, message, utxoTxId, firstNFTUtxo, tokenId);
    });

    $('#sendNFTModalOwnerLink').attr('href', 'https://explorer.nebl.io/address/' + owner);
    $('#requestNFTModalUtxoLink').attr('href', 'https://explorer.nebl.io/tx/' + utxoTxId);
    $('#requestNFTModalFirstLink').attr('href', 'https://explorer.nebl.io/tx/' + firstNFTUtxo);
    $('#requestNFTModalTokenLink').attr('href', 'https://explorer.nebl.io/token/' + tokenId);

    $('#requestNFTModalImage').attr('src', imageSrc);

    $('#requestNFTTradeModal').modal('show');   
}

function sendNeblioNFTrequestAPI(receiver, sender, message, utxoTxId, firstNFTUtxo, tokenId, symbol) {

    if (!isShopHostedOnVEF) {
        alert('This shop is not hosted on VEF you cannot create new item now!');
        return;
    }

    var metadata = {};

    if (receiver == '' || receiver == ' ') {
        alert('Receiver cannot be empty!');
        return;
    }

    if (sender == '' || sender == ' ') {
        alert('Sender cannot be empty!');
        return;
    }

    if (message != '' && message != ' ') {
        metadata['NFT Trade Request'] = true;
        metadata['NFT Utxo'] = utxoTxId;
        metadata['Message'] = message;
    }    
    else {
        alert('Message cannot be empty, please write down to your partner your offer!');
        return;
    }

    metadata['SourceUtxo'] = firstNFTUtxo;

    var nft = {
        "senderAddress": sender,
        "receiverAddress": receiver,
        "symbol": "CART",
        "Id": 'La8N1QroEDxxjkKYaPdPzatRj12nvRnL9JbUei',
        "amount": 1,
        "metadata": metadata,
    };

    $("#confirmButtonOk").off();
    $("#confirmButtonOk").click(function() {
        sendShopApiCommand('SendNTP1Token', nft);
    });
    
    ShowConfirmModal('', 'Do you realy want to send this NFT Trade Request?');  
}

function sendNeblioNFTTradeResponse(msgUtxo, receiver, sender, tokenId, utxoTxId, firstNFTUtxo, symbol, imageSrc) {

    if (!isShopHostedOnVEF) {
        alert('This shop is not hosted on VEF you cannot create new item now!');
        return;
    }

    $("#btnConfirmResponseNFTTrade").off();
    $("#btnConfirmResponseNFTTrade").click(function() {
        var message = $('#responseNFTModalMessage').val();

        sendNeblioNFTTradeResponseAPI(msgUtxo, receiver, sender, message, utxoTxId, firstNFTUtxo);
    });

    $('#responseNFTTradeModalPartnerLink').attr('href', 'https://explorer.nebl.io/address/' + receiver);
    $('#responseNFTTRadeModalUtxoLink').attr('href', 'https://explorer.nebl.io/tx/' + utxoTxId);
    $('#responseNFTTradeModalFirstLink').attr('href', 'https://explorer.nebl.io/tx/' + firstNFTUtxo);
    $('#responseNFTTradeModalTokenLink').attr('href', 'https://explorer.nebl.io/token/' + tokenId);

    $('#responseNFTModalImage').attr('src', imageSrc);

    $('#requestNFTTradeModalResponse').modal('show');   
}

function sendNeblioNFTTradeResponseAPI(msgUtxo, receiver, sender, message, utxoTxId, firstNFTUtxo) {

    if (!isShopHostedOnVEF) {
        alert('This shop is not hosted on VEF you cannot create new item now!');
        return;
    }

    var metadata = {};

    if (receiver == '' || receiver == ' ') {
        alert('Receiver cannot be empty!');
        return;
    }

    if (sender == '' || sender == ' ') {
        alert('Sender cannot be empty!');
        return;
    }

    if (message != '' && message != ' ') {
        metadata['NFT Trade Response'] = true;
        metadata['NFT Utxo'] = utxoTxId;
        metadata['Message'] = message;
    }    
    else {
        alert('Message cannot be empty, please write down to your partner your response!');
        return;
    }

    metadata['SourceUtxo'] = firstNFTUtxo;

    var nft = {
        "senderAddress": sender,
        "receiverAddress": receiver,
        "symbol": "CART",
        "Id": 'La8N1QroEDxxjkKYaPdPzatRj12nvRnL9JbUei',
        "amount": 1,
        "sendUtxo" : [
            msgUtxo
        ],
        "metadata": metadata,
    };

    $("#confirmButtonOk").off();
    $("#confirmButtonOk").click(function() {
        sendShopApiCommand('SendNTP1Token', nft);
    });
    
    ShowConfirmModal('', 'Do you realy want to send this response?');  
}

function sendNeblioNFT(tokenId, utxoTxId, firstNFTUtxo, symbol, imageSrc) {

    if (!isShopHostedOnVEF) {
        alert('This shop is not hosted on VEF you cannot create new item now!');
        return;
    }

    $("#btnConfirmSendNFT").off();
    $("#btnConfirmSendNFT").click(function() {
        var receiver = $('#sendNFTModalReceiverAddress').val();
        var message = $('#sendNFTModalMessage').val();

        sendNeblioNFTAPI(receiver, message, utxoTxId, firstNFTUtxo, tokenId, symbol);
    });

    $('#sendNFTModalUtxoLink').attr('href', 'https://explorer.nebl.io/tx/' + utxoTxId);
    $('#sendNFTModalFirstLink').attr('href', 'https://explorer.nebl.io/tx/' + firstNFTUtxo);
    $('#sendNFTModalTokenLink').attr('href', 'https://explorer.nebl.io/token/' + tokenId);

    $('#sendNFTModalImage').attr('src', imageSrc);

    $('#sendNFTModal').modal('show');
    
}


function sendNeblioNFTAPI(receiver, message, utxoTxId, firstNFTUtxo, tokenId, symbol) {

    if (!isShopHostedOnVEF) {
        alert('This shop is not hosted on VEF you cannot create new item now!');
        return;
    }

    var metadata = {};

    if (receiver == '' || receiver == ' ') {
        alert('Receiver cannot be empty!');
        return;
    }

    if (message != '' && message != ' ') {
        metadata['Message'] = message;
    }    

    metadata['SourceUtxo'] = firstNFTUtxo;

    var nft = {
        "senderAddress": selectedShopAccountAddress,
        "receiverAddress": receiver,
        "symbol": "CART",
        "Id": 'La8N1QroEDxxjkKYaPdPzatRj12nvRnL9JbUei',
        "amount": 1,
        "sendUtxo": [
          utxoTxId
        ],
        "metadata": metadata,
    };

    $("#confirmButtonOk").off();
    $("#confirmButtonOk").click(function() {
        sendShopApiCommand('SendNeblioNFT', nft);
    });
    
    ShowConfirmModal('', 'Do you realy want to send this NFT?');  
}


function createNewItem() {
    if (!isShopHostedOnVEF) {
        alert('This shop is not hosted on VEF you cannot create new item now!');
        return;
    }
    
    //$("#confirmButtonOk").off();
    //$("#confirmButtonOk").click(function() {
        var author = $('#mintNFTAuthor').val();
        var description = $('#mintNFTDescription').val();
        var image = $('#mintNFTImage').val();
        var link = $('#mintNFTLink').val();
        var type = $('#btnMintNFTType').text();
        mintNewNFT(author, description, image, link, type);
    //}); 

    //ShowConfirmModal('', 'Do you realy want create this NFT?');
}

function changeType(type) {
    $('#btnMintNFTType').text(type);
}

function mintNewNFT(author, description, image, link, type, tokenId) {

        if (!isShopHostedOnVEF) {
            alert('This shop is not hosted on VEF you cannot create new item now!');
            return;
        }

        var metadata = {};

        if (type == 'NFT Image') {
            if (image == '' || image == ' ' || !image.includes('http')) {
                alert('You selected NFT Image but no image link!');
                return;
            }
        }
        if (author == '' || author == ' ') {
            alert('Author cannot be empty!');
            return;
        }

        if (description == '' || description == ' ') {
            alert('Description cannot be empty!');
            return;
        }

        metadata['NFT'] = true;
        metadata['Author'] = author;
        metadata['Description'] = description;
        metadata['Image'] = image;
        metadata['Link'] = link;
        metadata['Type'] = type;

        var nft = {
            "SenderAddress": selectedShopAccountAddress,
            "ReceiverAddress": selectedShopAccountAddress,
            "Id": 'La8N1QroEDxxjkKYaPdPzatRj12nvRnL9JbUei',
            "Metadata": metadata,
        };
    
        $("#confirmButtonOk").off();
        $("#confirmButtonOk").click(function() {
            sendShopApiCommand('MintNeblioNFT', nft);
        });
        
        ShowConfirmModal('', 'Do you realy want to create this NFT?');  
}

function refreshShopItems() {

    $('#shopItemsCards').empty();
    $('#shopItemsCards').append('<div class="row"><div class="col"><div id="shopItemsCardsRow" class="row d-flex justify-content-center"></div></div></div>');
    for (var sid in AccountShopItems) {
        var shopItem = AccountShopItems[sid];
        var drawIt = false;
        var filterActive = false;

        shopItem.refreshAddressInfo();

        if (shopItem != undefined && shopItem != null) {

            // todo get tx details and check sender address
            filterActive = true;
            drawIt = true;
            /*
            var filter = $('#keysListSearchByNameAddress').val();
            if (filter != '' && filter != ' ') {
                filter = filter.toLowerCase();
                if (shopItem.KeyName.toLowerCase().includes(filter) || shopItem.KeyId.toLowerCase().includes(filter)) {
                    filterActive = true;
                }
            }
            else {
                filterActive = true;
            }
                        
            if ($('#chbxKeysListAccountKey').is(':checked')) {
                if (shopItem.type == 1) {
                    drawIt = true;
                }
            }

            if ($('#chbxKeysListBasicKey').is(':checked')) {
                if (shopItem.type == 0) {
                    drawIt = true;
                }
            }
            */
            if (drawIt && filterActive) {
                $('#shopItemsCardsRow').append(
                    '<div class="col-auto">' +
                    shopItem.getShopComponent() +
                    '</div>'
                );
            }
        }
    }		
}

var nfts = {};

function refreshShopItemNFTs() {

    //$('#shopNFTItemsCards').empty();
    $('#shopNFTItemsCards').append('<div class="row"><div class="col"><div id="shopItemsNFTCardsRow" class="row d-flex justify-content-center"></div></div></div>');
    for (var sid in AccountShopItems) {
        var shopItem = AccountShopItems[sid];

        var nnfts = shopItem.getNFTShopComponents(isAsShop);
        for (var nft in nnfts) {

            var drawIt = false;
            var filterActive = false;

            //shopItem.refreshAddressInfo();

            if (!(nft in nfts)) {
                if (shopItem != undefined && shopItem != null) {

                    var filter = $('#shopListSearchByAny').val();
                    if (filter != '' && filter != ' ') {
                        filter = filter.toLowerCase();
                        if (nnfts[nft].toLowerCase().includes(filter)) {
                            filterActive = true;
                        }
                    }
                    else {
                        filterActive = true;
                    }
                                
                    if ($('#chbxShopListShopItemsImages').is(':checked')) {
                        if (nnfts[nft].includes('NFT Image')) {
                            drawIt = true;
                        }
                    }
        
                    if ($('#chbxShopListShopItemsFiles').is(':checked')) {
                        if (nnfts[nft].includes('NFT File')) {
                            drawIt = true;
                        }
                    }

                    if ($('#chbxShopListShopItemsDocuments').is(':checked')) {
                        if (nnfts[nft].includes('NFT Document')) {
                            drawIt = true;
                        }
                    }

                    //filterActive = true;
                    //drawIt = true;

                    nfts[nft] = nnfts[nft];

                    if (drawIt && filterActive) {
                        $('#shopItemsNFTCardsRow').append(
                            '<div id="' + nft + '" class="col-auto">' +
                            nfts[nft] +
                            '</div>'
                        );
                    }
                }
            }
        }
        // delete old ones
        for (var n in nfts) {
            if (!(n in nnfts)) {
                delete nfts[n];
                $('#'+ n).remove();
            }
        }
    }		
}

var tradeRequests = {};

function refreshShopItemNFTRequests() {

    //$('#shopNFTItemsCards').empty();
    $('#shopNFTTradeRequestsCards').append('<div class="row"><div class="col"><div id="shopItemsNFTTradeReqCardsRow" class="row d-flex justify-content-center"></div></div></div>');
    for (var sid in AccountShopItems) {
        var shopItem = AccountShopItems[sid];

        var nnfts = shopItem.getNFTTradeRequestComponents(isAsShop);
        for (var nft in nnfts) {

            var drawIt = false;
            var filterActive = false;

            //shopItem.refreshAddressInfo();

            if (!(nft in tradeRequests)) {
                if (shopItem != undefined && shopItem != null) {

                    // todo get tx details and check sender address
                    filterActive = true;
                    drawIt = true;
                    tradeRequests[nft] = nnfts[nft];

                    if (drawIt && filterActive) {
                        $('#shopItemsNFTTradeReqCardsRow').append(
                            '<div id="' + nft + '" class="col-auto">' +
                            tradeRequests[nft] +
                            '</div>'
                        );
                    }
                }
            }
        }
        // delete old ones
        for (var n in tradeRequests) {
            if (!(n in nnfts)) {
                delete tradeRequests[n];
                $('#'+ n).remove();
            }
        }
    }		
}

function sendShopApiCommand(apicommand, data) {
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
                    if($('#transactionSentModal').hasClass('show')) {
                        $('#transactionSentModal').modal("toggle"); 
                    }
                }, 2500);
            },
            error: function (jqXhr, textStatus, errorMessage) { // error callback 
                console.log('Error: "' + errorMessage + '"');

                $('#txNotSendMessage').text(jqXhr.responseText);

                $('#transactionNotSentModal').modal("show"); 
                setTimeout(() => {
                    if($('#transactionNotSentModal').hasClass('show')) {
                        $('#transactionNotSentModal').modal("toggle"); 
                    }
                }, 5000);
            }
        });
}


var selectedShopAccountAddress = '';

function setShopListAccountAddress(accountAddress) {
    selectedShopAccountAddress = accountAddress;
    var a = Accounts[accountAddress];

    initShop(accountAddress);

    reloadShopListAccountsAddressesDropDown();

    if (a != null) {
        document.getElementById('btnAccountAddressToLoadShop').innerText = a.Name + ' - ' + accountAddress.substring(0,3) + '...' + accountAddress.substring(accountAddress.length-3);
        
    }
}

var selectedMintReceiverAccountAddress = '';

function setShopMintReceiverAccountAddress(accountAddress) {
    selectedMintReceiverAccountAddress = accountAddress;
    var a = Accounts[accountAddress];

    if (a != null) {
        document.getElementById('btnMintNFTReceiverAddress').innerText = a.Name + ' - ' + accountAddress.substring(0,3) + '...' + accountAddress.substring(accountAddress.length-3);
    }
}

var selectedNFTTradeRequestSenderAccountAddress = '';

function setShopRequestNFTTradeSenderAccountAddress(accountAddress) {
    selectedNFTTradeRequestSenderAccountAddress = accountAddress;
    var a = Accounts[accountAddress];

    if (a != null) {
        document.getElementById('btnRequestNFTTradeSenderAddress').innerText = a.Name + ' - ' + accountAddress.substring(0,3) + '...' + accountAddress.substring(accountAddress.length-3);
    }
}

function reloadShopListAccountsAddressesDropDown() {
    if (Accounts != null) {
        document.getElementById('shopListAccountAddressesDropDown').innerHTML = '';
        document.getElementById('mintNFTbtnMintNFTReceiverAddressDrowpDown').innerHTML = '';
        document.getElementById('requestNFTTradeSenderAddressDrowpDown').innerHTML = '';
        for (var acc in Accounts) {
            var a = Accounts[acc];     
            var add = acc.substring(0,3) + '...' + acc.substring(acc.length-3);      
            document.getElementById('shopListAccountAddressesDropDown').innerHTML += '<button style=\"font-size:12px\" class=\"dropdown-item btn btn-light\" ' +  'onclick=\"setShopListAccountAddress(\'' + acc + '\')\">' + a.Name + ' - ' + add + '</button>';
            
            if (selectedShopAccountAddress != acc) {
                document.getElementById('mintNFTbtnMintNFTReceiverAddressDrowpDown').innerHTML += '<button style=\"font-size:12px\" class=\"dropdown-item btn btn-light\" ' +  'onclick=\"setShopMintReceiverAccountAddress(\'' + acc + '\')\">' + a.Name + ' - ' + add + '</button>';
            }

            document.getElementById('requestNFTTradeSenderAddressDrowpDown').innerHTML += '<button style=\"font-size:12px\" class=\"dropdown-item btn btn-light\" ' +  'onclick=\"setShopRequestNFTTradeSenderAccountAddress(\'' + acc + '\')\">' + a.Name + ' - ' + add + '</button>';
        }
    }
}