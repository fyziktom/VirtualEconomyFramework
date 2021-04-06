
$(document).ready(function () {

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

    $("#btnShopAddNewShopItem").off();
    $("#btnShopAddNewShopItem").click(function() {
        createNewItem();
    });
    // add to shop items object and refresh 
    //AccountShopItems[uid] = CHESSItem;
    AccountShopItems[uid1] = MSGTItem;
    for (var sid in AccountShopItems) {
        // load actual data from Neblio API
        AccountShopItems[sid].refreshAddressInfo();           
    }
    setTimeout(() => {
        // draw items
        refreshShopItems();
        refreshShopItemNFTs();
    }, 500);
    
    // start auto refreshing each 2s
    setInterval(() => {
        for (var sid in AccountShopItems) {
            // load actual data from Neblio API
            AccountShopItems[sid].refreshAddressInfo();           
        }
        setTimeout(() => {
            // draw items
            refreshShopItems();
            refreshShopItemNFTs();
        }, 500);
        
    }, 2000);
});

var AccountShopItems = {};

function createNewItem() {
    
}


function sendNewItem(password) {
    if (selectedToken != null) {
        var metadata = {};

        metadata['NFT'] = true;
        metadata['Author'] = 'fyziktom';
        metadata['image'] = '';

        var amount = parseInt($('#sendTokenModalAmount').val());
        if (amount == 0) {
            alert('Cannot send 0 tokens');
            return;
        }

        var pass = '';
        if (password != null) {
            pass = password;
        }

        var token = {
            "ReceiverAddress": ActualAccount.Address,
            "SenderAddress": ActualAccount.Address,
            "Symbol": selectedToken.Symbol,
            "Id": selectedToken.Id,
            "Amount": 1,
            "Metadata": metadata,
            "Password": pass
        };
    
        $("#confirmButtonOk").off();
        $("#confirmButtonOk").click(function() {
            sendTokenApiCommand('SendNTP1Token', token);
        });
        
        ShowConfirmModal('', 'Do you realy want to send these tokens?');  
    }
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

        var nnfts = shopItem.getNFTShopComponents();
        for (var nft in nnfts) {

            var drawIt = false;
            var filterActive = false;

            //shopItem.refreshAddressInfo();

            if (!(nft in nfts)) {
                if (shopItem != undefined && shopItem != null) {

                    // todo get tx details and check sender address
                    filterActive = true;
                    drawIt = true;
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
                $('#'+ n).delete();
            }
        }
    }		
}