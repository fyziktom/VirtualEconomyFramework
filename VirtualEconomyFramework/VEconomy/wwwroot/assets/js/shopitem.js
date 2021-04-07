class Token {
    constructor(tokenSymbol, tokenId) {
        this.tokenSymbol = tokenSymbol;
        this.tokenId = tokenId;
        this.tokenInfo = {};
        this.tokenImage = '';
    }

    getInfo = function() {
        if (this.tokenId != '' || this.tokenId != ' ') {
            var url = 'https://ntp1node.nebl.io/ntp1/tokenmetadata/' + this.tokenId;
            var thisClass = this ;
            $.ajax(url,
            {
                contentType: 'application/json;charset=utf-8',
                method: 'GET',
                dataType: 'json',   // type of response data
                timeout: 10000,     // timeout milliseconds
                success: function (data, status, xhr) {   // success callback function
                    //console.log(data);
                    if (data != null){
                        thisClass.tokenInfo = data;
                        try {
                            thisClass.tokenImage = data.metadataOfIssuance.data.urls[0].url;
                        }
                        catch {}
                    }
                },
                error: function (jqXhr, textStatus, errorMessage) { // error callback 
                    console.log('Error: "' + errorMessage + '"');
                }
            });
        }
    }
}

class Utxo {
    constructor(txid, amount, toAddress, token) {
        this.txid = txid;
        this.amount = amount;
        this.toAddress = toAddress;
        this.fromAddress = '';
        this.txInfo = {};
        this.tokens = {};
        this.isSellableToken = false;
        this.sellableToken = token;
        this.amountOfTokensForSale = 0;

        this.tokenId = '';
        this.isNFT = false;
        this.NFTtxInfo = {}
        this.NFTInitData = {};
        this.originalNFTData = {};
        this.orignalNFTDataFound = false;
        this.orignalNFTDataNotFound = true;
        this.nftTradeRequestDataFound = false;
        this.nftTradeResponseDataFound = false;
        this.firstNFTtxId = '';
        this.originalNFTType = '';
        this.originalNFTAuthor = '';
        this.originalNFTImage = '';
        this.originalNFTDecription = '';
        this.nftTradeRequestUtxo = '';
        this.nftTradeRequestMessage = '';

        if (txid != null || txid != '' || txid != ' ') {
            this.getInfo();
        }
    }

    parseFromAddress = function() {
        if (this.txInfo != {}) {
            let vin = this.txInfo.vin;
            if (vin != undefined) {
                if (vin != null) {
                    for (var i in vin) {
                        var inp = vin[i];
                        if (inp.previousOutput != null) {
                            if (inp.previousOutput.addresses != null) {
                                this.fromAddress = inp.previousOutput.addresses[0];
                                return true;
                            }
                        }
                    }
                }
            }
        }
        return false;
    }

    checkToAddress = function() {
        if (this.txInfo != {}) {
            let vout = this.txInfo.vout;
            if (vout != undefined) {
                if (vout != null) {
                    for (var o in vout) {
                        var ot = vout[o];
                        if (ot.scriptPubKey != null) {
                            if (ot.scriptPubKey.addresses != null) {
                                if (ot.scriptPubKey.addresses[0] == this.toAddress) {
                                    return true;
                                }
                            }
                        }
                    }
                }
            }
        }
        return false;
    }

    checkIfIsSellableToken = function() {
        if (this.txInfo != {}) {
            let vout = this.txInfo.vout;
            if (vout != undefined) {
                if (vout != null) {
                    for (var o in vout) {
                        var ot = vout[o];
                        if (ot.tokens != {}) {
                            if (ot.scriptPubKey.addresses != undefined && ot.scriptPubKey.addresses[0] != undefined && ot.scriptPubKey.addresses[0] == this.toAddress) {
                                this.tokens[o] = ot.tokens;
                                for (var t in ot.tokens) {
                                    var tok = ot.tokens[t];
                                    if (tok.amount == 1) {
                                        this.isNFT = true;
                                        if (this.sellableToken.tokenId == tok.tokenId) {
                                            this.tokenId = tok.tokenId;
                                            this.checkIfIsNFT(this.txid);
                                        }
                                    }
                                    if (tok.tokenId == this.sellableToken.tokenId) {
                                        this.isSellableToken = true;
                                        this.amountOfTokensForSale += tok.amount;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        return false;     
    }

    getInfo = function() {
        if (this.txid != '' || this.txid != ' ') {
            var url = 'https://ntp1node.nebl.io/ntp1/transactioninfo/' + this.txid;
            var thisClass = this ;
            $.ajax(url,
            {
                contentType: 'application/json;charset=utf-8',
                method: 'GET',
                dataType: 'json',   // type of response data
                timeout: 10000,     // timeout milliseconds
                success: function (data, status, xhr) {   // success callback function
                    //console.log(data);
                    if (data != null){
                        thisClass.txInfo = data;
                        if (thisClass.checkToAddress()) {
                            thisClass.parseFromAddress();
                            thisClass.checkIfIsSellableToken();
                            //console.log(thisClass);
                        }
                    }
                },
                error: function (jqXhr, textStatus, errorMessage) { // error callback 
                    console.log('Error: "' + errorMessage + '"');
                }
            });
        }
    }

    checkTxNFTInputs = function(inTxId) {

        if (inTxId != '' || inTxId != ' ') {
            var url = 'https://ntp1node.nebl.io/ntp1/transactioninfo/' + inTxId;
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
                        try {
                            if (data.vin[0].tokens[0].amount == 1 && !thisClass.nftTradeRequestDataFound) {
                                if (data.vin[0].tokens[0].tokenId == thisClass.tokenId) {
                                    var txid = data.vin[0].txid;
                                    thisClass.firstNFTtxId = inTxId;
                                    // reset flags because NFT must starts from some bigger lot
                                    thisClass.orignalNFTDataFound = false;
                                    thisClass.orignalNFTDataNotFound = true;
                                    thisClass.checkIfIsNFT(txid);
                                }
                            }
                        }
                        catch {}
                    }
                },
                error: function (jqXhr, textStatus, errorMessage) { // error callback 
                    console.log('Error: "' + errorMessage + '"');
                }
            });
        }      
    }

    getUtxoTxId() {
        return this.txid;
    }
    getNFTImage () {
        return this.originalNFTImage;
    }
    getNFTAuthor () {
        return this.originalNFTAuthor;
    }
    getOriginalNFTDescription () {
        return this.originalNFTDecription;
    }
    isOrignalNFTDataFound () {
        return this.orignalNFTDataFound;
    }
    isThisNFT () {
        return this.isNFT;
    }
    isNFTTradeRequest() {
        return this.nftTradeRequestDataFound;
    }
    isNFTTradeResponse() {
        return this.nftTradeResponseDataFound;
    }


    checkIfIsNFT = function(initTxId) {

        if (this.orignalNFTDataFound) {
            return;
        }

        if (!this.orignalNFTDataNotFound) {
            return;
        }

        if (initTxId != '' || initTxId != ' ') {

            var url = 'https://ntp1node.nebl.io/ntp1/tokenmetadata/' + this.tokenId + '/' + initTxId;
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
                        try 
                        {
                            if (data.metadataOfUtxo.userData.meta != undefined) {
                                if (data.metadataOfUtxo.userData.meta.length == 0) {
                                    thisClass.checkTxNFTInputs(initTxId);
                                    return;
                                }

                                for( var m in data.metadataOfUtxo.userData.meta) {
                                    var nft = data.metadataOfUtxo.userData.meta[m]['NFT'];
                                    if (nft != undefined){
                                        //console.log('NFT!!!');
                                        thisClass.firstNFTtxId = initTxId;
                                        thisClass.isNFT = true;
                                        thisClass.orignalNFTDataFound = true;
                                        thisClass.orignalNFTDataNotFound = false;
                                        thisClass.originalNFTType = 'NFT';
                                        thisClass.originalNFTData = data;
                                    }
                                    else if (nft != undefined && !thisClass.orignalNFTDataFound)
                                    {
                                        thisClass.checkTxNFTInputs(initTxId);
                                        return;
                                    }
                                    var aut = data.metadataOfUtxo.userData.meta[m]['Author'];
                                    if (aut != undefined){
                                        thisClass.originalNFTAuthor = aut;
                                    }
                                    var img = data.metadataOfUtxo.userData.meta[m]['Image'];
                                    if (img != undefined){
                                        thisClass.originalNFTImage = img;
                                    }
                                    var desc = data.metadataOfUtxo.userData.meta[m]['Description'];
                                    if (desc != undefined){
                                        thisClass.originalNFTDecription = desc;
                                    }
                                    
                                    var trade = data.metadataOfUtxo.userData.meta[m]['NFT Trade Request'];
                                    if (trade != undefined) {
                                        if (trade) {
                                            thisClass.nftTradeRequestDataFound = true;
                                            thisClass.firstNFTtxId = initTxId;
                                            var tradeutxo = data.metadataOfUtxo.userData.meta[1]['NFT Utxo'];
                                            if (tradeutxo != undefined){
                                                thisClass.nftTradeRequestUtxo = tradeutxo;
                                                thisClass.loadTradedNFTData(tradeutxo);
                                            }

                                            var msg = data.metadataOfUtxo.userData.meta[2]['Message'];
                                            if (msg != undefined){
                                                thisClass.nftTradeRequestMessage = msg;
                                            }
                                        }
                                    }

                                    var tradeResp = data.metadataOfUtxo.userData.meta[m]['NFT Trade Response'];
                                    if (tradeResp != undefined) {
                                        if (tradeResp) {
                                            thisClass.nftTradeResponseDataFound = true;
                                            thisClass.firstNFTtxId = initTxId;
                                            var tradeutxo = data.metadataOfUtxo.userData.meta[1]['NFT Utxo'];
                                            if (tradeutxo != undefined){
                                                thisClass.nftTradeRequestUtxo = tradeutxo;
                                                thisClass.loadTradedNFTData(tradeutxo);
                                            }

                                            var msg = data.metadataOfUtxo.userData.meta[2]['Message'];
                                            if (msg != undefined){
                                                thisClass.nftTradeRequestMessage = msg;
                                            }

                                        }
                                    }
                                }

                                if (thisClass.orignalNFTDataFound) {
                                    // check if the previous input was amount 1
                                    thisClass.checkTxNFTInputs(initTxId);
                                    return;
                                }
                            }
                            else
                            {
                                thisClass.checkTxNFTInputs(initTxId);
                                return;
                            }
                        }
                        catch {}
                    }
                },
                error: function (jqXhr, textStatus, errorMessage) { // error callback 
                    console.log('Error: "' + errorMessage + '"');
                }
            });
        }
    }

    loadTradedNFTData = function(initTxId) {

        if (this.orignalNFTDataFound) {
            return;
        }

        if (!this.orignalNFTDataNotFound) {
            return;
        }

        if (initTxId != '' || initTxId != ' ') {

            var url = 'https://ntp1node.nebl.io/ntp1/tokenmetadata/' + this.tokenId + '/' + initTxId;
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
                        try 
                        {
                            if (data.metadataOfUtxo.userData.meta != undefined) {
                                if (data.metadataOfUtxo.userData.meta.length == 0) {
                                    thisClass.checkTxNFTInputs(initTxId);
                                    return;
                                }

                                for( var m in data.metadataOfUtxo.userData.meta) {
                                    var nft = data.metadataOfUtxo.userData.meta[m]['NFT'];
                                    if (nft != undefined){
                                        //console.log('NFT!!!');
                                        
                                        //thisClass.isNFT = true;
                                        //thisClass.orignalNFTDataFound = true;
                                        //thisClass.orignalNFTDataNotFound = false;
                                        thisClass.originalNFTType = 'NFT';
                                        thisClass.originalNFTData = data;
                                    }
                                    
                                    var aut = data.metadataOfUtxo.userData.meta[m]['Author'];
                                    if (aut != undefined){
                                        thisClass.originalNFTAuthor = aut;
                                    }
                                    var img = data.metadataOfUtxo.userData.meta[m]['Image'];
                                    if (img != undefined){
                                        thisClass.originalNFTImage = img;
                                    }
                                    var desc = data.metadataOfUtxo.userData.meta[m]['Description'];
                                    if (desc != undefined){
                                        thisClass.originalNFTDecription = desc;
                                    }
                                }
                            }
                        }
                        catch {}
                    }
                },
                error: function (jqXhr, textStatus, errorMessage) { // error callback 
                    console.log('Error: "' + errorMessage + '"');
                }
            });
        }
    }
}

class ShopItem {
    constructor(id, address, type, tokenSymbol, tokenId, amount, price, name, description, _isShopHostedOnVEF) {
        this.address = address;
        this.addressInfo = {};
        this.utxos = {};
        this.type = type;
        this.name = name;
        this.description = description;
        this.token = new Token(tokenSymbol, tokenId);
        this.token.getInfo();
        this.id = id;
        this.amount = amount;
        this.price = price;
        this.isActive = false;
        this.maxLotsPerBuy = 9;
        this.NeblFromSatToMainRatio = 100000000;
        this.isShopHostedOnVEF = _isShopHostedOnVEF;
    }

    setActive() {
        this.isActive = true;
    }
    setInactive() {
        this.isActive = false;
    }

    getUnsolvedOrders() {

        var usos = {};
        for (var usoi in this.utxos) {
            var uso = this.utxos[usoi];
            for (var i = 1; i < this.maxLotsPerBuy; i++) {
                if (uso.fromAddress != this.address && ((this.price*i + 10000) >= uso.amount &&  uso.amount >= (this.price*i - 10000))) {
                    usos[uso.txid] = uso;
                }
            }
        }

        return usos;
    }

    getSellableTokensAmount() {

        var totalSellableTokenAmount = 0;
        for (var usoi in this.utxos) {
            var uso = this.utxos[usoi];
            if (uso.isSellableToken) {
                totalSellableTokenAmount += uso.amountOfTokensForSale;
            }
        }

        return totalSellableTokenAmount;
    }

    getUnsolvedOrdersLines = function() {

        var lines = '<div class="row">' + 
                    '<div class="col">';

        for (var usoi in this.utxos) {
            var uso = this.utxos[usoi];
            for (var i = 1; i < this.maxLotsPerBuy; i++) {
                if (uso.fromAddress != this.address && ((this.price*i + 10000) >= uso.amount &&  uso.amount >= (this.price*i - i*10000))) {
                    var lots = parseInt(uso.amount / (this.price-10000));
                    var amount = lots * this.amount;

                    var line = '<div class="row">' + 
                    '               <div class="col">' +
                    '                   <div class="row">' +
                    '                       <div class="col">' +
                    '                           <hr/>' +
                    '                       </div>' +
                    '                   </div>' +
                    '                   <div class="row d-flex d-xl-flex justify-content-center justify-content-xl-center">' + 
                    '                       <div class="col d-flex d-xl-flex justify-content-center justify-content-xl-center align-items-xl-center">' +
                    '                           <span>For: <span style="font-size:12px">' + uso.fromAddress + '</span></span>' +
                    '                       </div>' +
                    '                   </div>' +
                    '                   <div class="row d-flex d-xl-flex justify-content-center justify-content-xl-center">' + 
                    '                       <div class="col d-flex d-xl-flex justify-content-center justify-content-xl-center align-items-xl-center">' +
                    '                           <span>Amount: ' + amount.toString() + ' of ' + this.token.tokenSymbol + '</span>' +
                    '                       </div>' +
                    '                   </div>' +
                    '               </div>' +
                    '           </div>';
                    lines += line;
                }
            }
        }
        lines += '</div></div>';

        return lines;
    }

    getShopComponent(){

        var type = 'Token Item';
        if (this.type == 1) {
            type = 'Other Item';
        }

        let unsolvedOrders = this.getUnsolvedOrders();

        var price = (this.price/this.NeblFromSatToMainRatio).toString();
    
        var keyComponent = '<div class="card shadow" style="min-width: 350px; width:100%">'+
        '    <div class="card-header d-xl-flex justify-content-xl-center align-items-xl-center py-3">'+
        '        <h4>' + this.name + ' - Item</h4>'+
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
        '                           <span>' + this.name + '</span>'+
        '                        </div>'+
        '                    </div>'+
        '                    <div class="row">'+
        '                        <div class="col d-flex d-xl-flex justify-content-center justify-content-xl-center align-items-xl-center" style="margin-top: 10px;">'+
        '                           <span>Type</span>'+
        '                        </div>'+
        '                    </div>'+
        '                    <div class="row">'+
        '                        <div class="col d-flex d-xl-flex justify-content-center justify-content-xl-center align-items-xl-center">'+
        '                           <span>'+ type + '</span>'+
        '                        </div>'+
        '                    </div>'+
        '                    <div class="row">'+
        '                        <div class="col d-flex d-xl-flex justify-content-center justify-content-xl-center align-items-xl-center" style="margin-top: 10px;">'+
        '                           <span>Seller</span>'+
        '                        </div>'+
        '                    </div>'+
        '                    <div class="row">'+
        '                        <div class="col d-flex d-xl-flex justify-content-center justify-content-xl-center align-items-xl-center">'+
        '                           <span style="font-size:12px"><a href="https://explorer.nebl.io/address/' + this.address + '" target="blank">' + this.address + '</a></span>'+
        '                        </div>'+
        '                    </div>'+
        '                    <div class="row">'+
        '                        <div class="col d-flex d-xl-flex justify-content-center justify-content-xl-center align-items-xl-center" style="margin-top: 10px;">'+
        '                           <span>Id</span>'+
        '                        </div>'+
        '                    </div>'+
        '                    <div class="row">'+
        '                       <div class="col d-flex d-xl-flex justify-content-center justify-content-xl-center align-items-xl-center">'+
        '                           <span style="font-size: 12px;">' + this.id + '</span>'+
        '                       </div>'+
        '                    </div>'+
        '                    <div class="row">'+
        '                        <div class="col d-flex d-xl-flex justify-content-center justify-content-xl-center align-items-xl-center" style="margin-top: 10px;">'+
        '                           <span>Description</span>'+
        '                        </div>'+
        '                    </div>'+
        '                    <div class="row">'+
        '                       <div class="col d-flex d-xl-flex justify-content-center justify-content-xl-center align-items-xl-center">'+
        '                           <span style="font-size: 12px;">' + this.description + '</span>'+
        '                       </div>'+
        '                    </div>'+
        '                    <div class="row">'+
        '                        <div class="col d-flex d-xl-flex justify-content-center justify-content-xl-center align-items-xl-center" style="margin-top: 10px;">'+
        '                           <span>Nuber Of Tokens For Sale</span>'+
        '                        </div>'+
        '                    </div>'+
        '                    <div class="row">'+
        '                       <div class="col d-flex d-xl-flex justify-content-center justify-content-xl-center align-items-xl-center">'+
        '                           <span style="font-size: 12px;">' + this.amount + ' / one lot</span>'+
        '                       </div>'+
        '                    </div>'+
        '                    <div class="row">'+
        '                        <div class="col d-flex d-xl-flex justify-content-center justify-content-xl-center align-items-xl-center" style="margin-top: 10px;">'+
        '                           <span>Price</span>'+
        '                        </div>'+
        '                    </div>'+
        '                    <div class="row">'+
        '                       <div class="col d-flex d-xl-flex justify-content-center justify-content-xl-center align-items-xl-center">'+
        '                           <span style="font-size: 12px;">' + price + ' Nebl</span>'+
        '                       </div>'+
        '                    </div>'+
        '                    <div class="row">'+
        '                        <div class="col d-flex d-xl-flex justify-content-center justify-content-xl-center align-items-xl-center" style="margin-top: 10px;">'+
        '                           <span>Tokens Left for Sale</span>'+
        '                        </div>'+
        '                    </div>'+
        '                    <div class="row">'+
        '                       <div class="col d-flex d-xl-flex justify-content-center justify-content-xl-center align-items-xl-center">'+
        '                           <span style="font-size: 12px;">' + this.getSellableTokensAmount().toString() + ' of <a href="https://explorer.nebl.io/token/' + this.token.tokenId + '" target="blank">' + this.token.tokenSymbol + '<img style="max-width:25px;max-height:25px;margin-left:5px" src="' + this.token.tokenImage + '"></a></span>'+
        '                       </div>'+
        '                    </div>'+
        '                    <div class="row">'+
        '                        <div class="col d-flex d-xl-flex justify-content-center justify-content-xl-center align-items-xl-center" style="margin-top: 10px;">'+
        '                           <span>Unresolved Orders</span>'+
        '                        </div>'+
        '                    </div>'+
        '                    <div class="row">'+
        '                       <div id="' + this.id + '-unresolvedOrders" class="col d-flex d-xl-flex justify-content-center justify-content-xl-center align-items-xl-center">'+
                                    this.getUnsolvedOrdersLines() +
        '                       </div>'+
        '                    </div>'+
        '                </div>'+
        '            </div>'+
        '        </div>'+
        '    </div>'+
        '</div>';
        
        return keyComponent;
    }

    getNFTShopComponents(asShopItem) {

        var type = 'NFT Image';

        //let unsolvedOrders = this.getUnsolvedOrders();
        var components = {};

        var hidden = 'd-none';
        if (this.isShopHostedOnVEF) {
            hidden = '';
        }

        var shopItemHidden = 'd-none';
        if (asShopItem) {
            shopItemHidden = '';
        }

        for (var uo in this.utxos) {
            var uno = this.utxos[uo];
            if (uno.getNFTImage() != '' && uno.isOrignalNFTDataFound) {

                if (!uno.isNFTTradeRequest() && !uno.isNFTTradeResponse()) {
                
                    var price = (this.price/this.NeblFromSatToMainRatio).toString();

                    var nftComponent = '<div class="card shadow" style="min-width: 350px; width:100%">'+
                    '    <div class="card-header d-xl-flex justify-content-xl-center align-items-xl-center py-3">'+
                    '        <h4>' + this.name + ' - Item</h4>'+
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
                    '                           <span>' + this.name + '</span>'+
                    '                        </div>'+
                    '                    </div>'+
                    '                    <div class="row">'+
                    '                        <div class="col d-flex d-xl-flex justify-content-center justify-content-xl-center align-items-xl-center" style="margin-top: 10px;">'+
                    '                           <span>Type</span>'+
                    '                        </div>'+
                    '                    </div>'+
                    '                    <div class="row">'+
                    '                        <div class="col d-flex d-xl-flex justify-content-center justify-content-xl-center align-items-xl-center">'+
                    '                           <span>'+ type + '</span>'+
                    '                        </div>'+
                    '                    </div>'+
                    '                    <div class="row">'+
                    '                        <div class="col d-flex d-xl-flex justify-content-center justify-content-xl-center align-items-xl-center" style="margin-top: 10px;">'+
                    '                           <span>Owner</span>'+
                    '                        </div>'+
                    '                    </div>'+
                    '                    <div class="row">'+
                    '                        <div class="col d-flex d-xl-flex justify-content-center justify-content-xl-center align-items-xl-center">'+
                    '                           <span style="font-size:12px"><a href="https://explorer.nebl.io/address/' + this.address + '" target="blank">' + this.address + '</a></span>'+
                    '                        </div>'+
                    '                    </div>'+
                    '                    <div class="row">'+
                    '                        <div class="col d-flex d-xl-flex justify-content-center justify-content-xl-center align-items-xl-center" style="margin-top: 10px;">'+
                    '                           <span>Tx Id</span>'+
                    '                        </div>'+
                    '                    </div>'+
                    '                    <div class="row">'+
                    '                           <div class="col d-flex d-xl-flex justify-content-center justify-content-xl-center align-items-xl-center">'+
                    '                               <span style="font-size: 12px;"><a href="https://explorer.nebl.io/tx/' + uno.firstNFTtxId + '" target="blank">Go To Explorer</a></span>'+
                    '                           </div>'+
                    '                    </div>'+
                    '                    <div class="row">'+
                    '                        <div class="col d-flex d-xl-flex justify-content-center justify-content-xl-center align-items-xl-center" style="margin-top: 10px;">'+
                    '                           <span>Description</span>'+
                    '                        </div>'+
                    '                    </div>'+
                    '                    <div class="row">'+
                    '                       <div class="col d-flex d-xl-flex justify-content-center justify-content-xl-center align-items-xl-center">'+
                    '                           <span style="font-size: 12px;">' + uno.getOriginalNFTDescription() + '</span>'+
                    '                       </div>'+
                    '                    </div>'+
                    '                    <div class="row ' + shopItemHidden + '">'+
                    '                        <div class="col d-flex d-xl-flex justify-content-center justify-content-xl-center align-items-xl-center" style="margin-top: 10px;">'+
                    '                           <span>Nuber Of Tokens For Sale</span>'+
                    '                        </div>'+
                    '                    </div>'+
                    '                    <div class="row ' + shopItemHidden + '">'+
                    '                       <div class="col d-flex d-xl-flex justify-content-center justify-content-xl-center align-items-xl-center">'+
                    '                           <span style="font-size: 12px;">' + this.amount + ' / one lot</span>'+
                    '                       </div>'+
                    '                    </div>'+
                    '                    <div class="row ' + shopItemHidden + '">'+
                    '                        <div class="col d-flex d-xl-flex justify-content-center justify-content-xl-center align-items-xl-center" style="margin-top: 10px;">'+
                    '                           <span>Price</span>'+
                    '                        </div>'+
                    '                    </div>'+
                    '                    <div class="row ' + shopItemHidden + '">'+
                    '                       <div class="col d-flex d-xl-flex justify-content-center justify-content-xl-center align-items-xl-center">'+
                    '                           <span style="font-size: 12px;">' + price + ' Nebl</span>'+
                    '                       </div>'+
                    '                    </div>'+
                    '                    <div class="row">'+
                    '                        <div class="col d-flex d-xl-flex justify-content-center justify-content-xl-center align-items-xl-center" style="margin-top: 10px;">'+
                    '                           <span>NFT Last TxId</span>'+
                    '                        </div>'+
                    '                    </div>'+
                    '                    <div class="row">'+
                    '                       <div class="col d-flex d-xl-flex justify-content-center justify-content-xl-center align-items-xl-center">'+
                    '                           <span style="font-size: 12px;"><a href="https://explorer.nebl.io/tx/' + uno.getUtxoTxId() +'" target="blank">Last Transaction With This NFT</a></span>'+
                    '                       </div>'+
                    '                    </div>'+
                    '                    <div class="row ' + shopItemHidden + '">'+
                    '                        <div class="col d-flex d-xl-flex justify-content-center justify-content-xl-center align-items-xl-center" style="margin-top: 10px;">'+
                    '                           <span>This NFT</span>'+
                    '                        </div>'+
                    '                    </div>'+
                    '                    <div class="row">'+
                    '                       <div class="col d-flex d-xl-flex justify-content-center justify-content-xl-center align-items-xl-center">'+
                    '                           <span style="font-size: 12px;"><a href="https://explorer.nebl.io/tx/' + uno.firstNFTtxId +'" target="blank"><img style="max-width:200px;max-height:200px;margin-left:5px" src="' + uno.getNFTImage() + '"></a></span>'+
                    '                       </div>'+
                    '                    </div>'+
                    '                    <div class="row '+ shopItemHidden + '">'+
                    '                        <div class="col d-flex d-xl-flex justify-content-center justify-content-xl-center align-items-xl-center" style="margin-top: 10px;">'+
                    '                           <span>Unresolved Orders</span>'+
                    '                        </div>'+
                    '                    </div>'+
                    '                    <div class="row '+ shopItemHidden + '">'+
                    '                       <div id="' + this.id + '-unresolvedOrders" class="col d-flex d-xl-flex justify-content-center justify-content-xl-center align-items-xl-center">'+
                                                this.getUnsolvedOrdersLines() +
                    '                       </div>'+
                    '                    </div>'+
                    '                   <div class="card-footer ' + hidden + '" style="padding-top: 5px; margin-top: 20px;">'+
                    '                    <div class="row">'+
                    '                         <div class="col d-flex d-xl-flex justify-content-center justify-content-xl-center align-items-xl-center">'+
                    '                               <button class="btn btn-primary" type="button" onclick="sendNeblioNFT(\'' + this.token.tokenId + '\',\'' + uno.getUtxoTxId() + '\',\'' + uno.firstNFTtxId + '\',\'' + this.token.tokenSymbol + '\',\'' + uno.getNFTImage() + '\')">'+
                    '                                   <i class="fa fa-send-o"></i>'+
                    '                               </button>'+
                    '                         </div>'+
                    '                         <div class="col d-flex d-xl-flex justify-content-center justify-content-xl-center align-items-xl-center">'+
                    '                               <button class="btn btn-primary" type="button" onclick="requestNeblioNFTTrade(\'' + this.address + '\',\'' + this.token.tokenId + '\',\'' + uno.getUtxoTxId() + '\',\'' + uno.firstNFTtxId + '\',\'' + this.token.tokenSymbol + '\',\'' + uno.getNFTImage() + '\')">'+
                    '                                   <i class="fa fa-comments-o"></i>'+
                    '                               </button>'+
                    '                         </div>'+
                    '                      </div>'+
                    '                   </div>'+
                    '                </div>'+
                    '            </div>'+
                    '        </div>'+
                    '    </div>'+
                    '</div>';

                    components[uo] = nftComponent;
                    
                }
            }
        }
        
        return components;
    }


    getNFTTradeRequestComponents(asShopItem) {

        var components = {};

        var hidden = 'd-none';
        if (this.isShopHostedOnVEF) {
            hidden = '';
        }

        var shopItemHidden = 'd-none';
        if (asShopItem) {
            shopItemHidden = '';
        }

        for (var uo in this.utxos) {
            var uno = this.utxos[uo];
            if (uno.isNFTTradeRequest() || uno.isNFTTradeResponse()) {

                var image = '';
                var firstTxId = '';
                var reqresp = 'Trade Request';
                if (uno.isNFTTradeResponse()) {
                    reqresp = 'Trade Response';
                }

                if (uno.isNFTTradeRequest()) {
                    if (this.utxos[uno.nftTradeRequestUtxo] == undefined || this.utxos[uno.nftTradeRequestUtxo] == null) {
                        return;
                    }

                    var requestedNft = this.utxos[uno.nftTradeRequestUtxo];

                    if (requestedNft == undefined || requestedNft == null) {
                        return;
                    }

                    // wait until image is loaded
                    if (requestedNft.getNFTImage() == '' || !requestedNft.isOrignalNFTDataFound) {
                        return;
                    }

                    image = requestedNft.getNFTImage();
                    firstTxId = requestedNft.firstNFTtxId;
                }
                else {
                    image = uno.getNFTImage();
                    firstTxId = uno.firstNFTtxId;
                }
              
                var nftComponent = '<div class="card shadow" style="min-width: 350px; width:100%">'+
                '    <div class="card-header d-xl-flex justify-content-xl-center align-items-xl-center py-3">'+
                '        <h4>' + reqresp + '</h4>'+
                '    </div>'+
                '    <div class="card-body" style="padding-bottom: 0px;">'+
                '        <div class="col">'+
                '            <div class="row" style="margin-bottom: 30px;">'+
                '                <div class="col">'+
                '                    <div class="row">'+
                '                        <div class="col d-flex d-xl-flex justify-content-center justify-content-xl-center align-items-xl-center">' +
                '                           <span>From</span>'+
                '                        </div>'+
                '                    </div>'+
                '                    <div class="row">'+
                '                        <div class="col d-flex d-xl-flex justify-content-center justify-content-xl-center align-items-xl-center">'+
                '                           <span style="font-size:12px"><a href="https://explorer.nebl.io/address/' + uno.fromAddress + '" target="blank">' + uno.fromAddress + '</a></span>'+
                '                        </div>'+
                '                    </div>'+
                '                    <div class="row">'+
                '                        <div class="col d-flex d-xl-flex justify-content-center justify-content-xl-center align-items-xl-center" style="margin-top: 10px;">'+
                '                           <span>NFT Utxo</span>'+
                '                        </div>'+
                '                    </div>'+
                '                    <div class="row">'+
                '                           <div class="col d-flex d-xl-flex justify-content-center justify-content-xl-center align-items-xl-center">'+
                '                               <span style="font-size: 12px;"><a href="https://explorer.nebl.io/tx/' + uno.nftTradeRequestUtxo + '" target="blank">Go To Explorer</a></span>'+
                '                           </div>'+
                '                    </div>'+
                '                    <div class="row">'+
                '                        <div class="col d-flex d-xl-flex justify-content-center justify-content-xl-center align-items-xl-center" style="margin-top: 10px;">'+
                '                           <span>Message</span>'+
                '                        </div>'+
                '                    </div>'+
                '                    <div class="row">'+
                '                       <div class="col d-flex d-xl-flex justify-content-center justify-content-xl-center align-items-xl-center">'+
                '                           <span style="font-size: 12px;">' + uno.nftTradeRequestMessage + '</span>'+
                '                       </div>'+
                '                    </div>'+
                '                   <div class="card-footer ' + hidden + '" style="padding-top: 5px; margin-top: 20px;">'+
                '                    <div class="row">'+
                '                         <div class="col d-flex d-xl-flex justify-content-center justify-content-xl-center align-items-xl-center">'+
                '                               <button class="btn btn-primary" type="button" onclick="sendNeblioNFT(\'' + this.token.tokenId + '\',\'' + uno.nftTradeRequestUtxo + '\',\'' + firstTxId + '\',\'' + this.token.tokenSymbol + '\',\'' + image + '\')">'+
                '                                   <i class="fa fa-send-o"></i>'+
                '                               </button>'+
                '                         </div>'+
                '                         <div class="col d-flex d-xl-flex justify-content-center justify-content-xl-center align-items-xl-center">'+
                '                               <button class="btn btn-primary" type="button" onclick="sendNeblioNFTTradeResponse(\'' + uno.txid + '\',\'' + uno.fromAddress + '\',\'' + uno.toAddress + '\',\'' + this.token.tokenId + '\',\'' + uno.nftTradeRequestUtxo + '\',\'' + firstTxId + '\',\'' + this.token.tokenSymbol + '\',\'' + image + '\')">'+
                '                                   <i class="fa fa-comments-o"></i>'+
                '                               </button>'+
                '                         </div>'+
                '                      </div>'+
                '                   </div>'+
                '                </div>'+
                '            </div>'+
                '        </div>'+
                '    </div>'+
                '</div>';

                components[uo] = nftComponent;
            }
        }
        
        return components;
    }

    refreshAddressInfoData = function (data) {
        if (data != null){
            this.addressInfo = data;
            //this.utxos = {};
            if (this.addressInfo.utxos != undefined) {
                if (this.addressInfo.utxos != null) {
                    for (var u in this.addressInfo.utxos) {
                        var ut = this.addressInfo.utxos[u];
                        if (this.utxos[ut.txid] == undefined || this.utxos[ut.txid] == null) {
                            this.utxos[ut.txid] = new Utxo(ut.txid, ut.value, ut.scriptPubKey.addresses[0], this.token);
                        }
                        //console.log(this.utxos[u.txid]);
                    }

                    for (var u in this.utxos) {
                        var ut = this.utxos[u];
                        var isin = false;
                        for (var ui in this.addressInfo.utxos) {
                            var uti = this.addressInfo.utxos[ui];
                            if (ut.txid == uti.txid) {
                                isin = true;
                            }
                        }
                        if (!isin) {
                            delete this.utxos[u];
                        }
                    }
                }
            }
        }
    }

   refreshAddressInfo() {
        var url = 'https://ntp1node.nebl.io/ntp1/addressinfo/' + this.address;
        var thisClass = this;
        $.ajax(url,
        {
            contentType: 'application/json;charset=utf-8',
            method: 'GET',
            dataType: 'json',   // type of response data
            timeout: 10000,     // timeout milliseconds
            success: function (data, status, xhr) {   // success callback function
                //console.log(data);
                thisClass.refreshAddressInfoData(data);
            },
            error: function (jqXhr, textStatus, errorMessage) { // error callback 
                console.log('Error: "' + errorMessage + '"');
            }
        });
    }

}