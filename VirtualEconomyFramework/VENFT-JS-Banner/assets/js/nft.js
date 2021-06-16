class NFT {
    constructor(txid, tokenId) {
        this.txid = txid;
        this.fromAddress = '';
        this.txInfo = {};
        this.tokens = {};
        this.isSellableToken = false;
        this.amountOfTokensForSale = 0;

        this.TokenId = tokenId;
        this.isNFT = false;
        this.NFTtxInfo = {}
        this.NFTInitData = {};
        this.originalNFTData = {};
        this.NFTInitTxFound = false;
        this.NFTFirstTxFound = false;
        this.orignalNFTDataFound = false;
        this.orignalNFTDataNotFound = true;
        this.nftTradeRequestDataFound = false;
        this.nftTradeResponseDataFound = false;
        this.firstNFTtxId = '';
        this.originalNFTType = '';
        this.originalNFTName = '';
        this.originalNFTAuthor = '';
        this.originalNFTImage = '';
        this.originalNFTLink = '';
        this.youtubeCode = '';
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
            if (vout != undefined && vout != null) {
                for (var o in vout) {
                    var ot = vout[o];
                    if (ot.scriptPubKey != null && ot.scriptPubKey.addresses != null) {
                        if (ot.scriptPubKey.addresses[0] == this.toAddress) {
                            return true;
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
            if (vout != undefined && vout != null) {
                for (var o in vout) {
                    var ot = vout[o];
                    if (ot.tokens != {}) {
                        if (ot.scriptPubKey.addresses != undefined && ot.scriptPubKey.addresses[0] != undefined) {
                            this.tokens[o] = ot.tokens;
                            for (var t in ot.tokens) {
                                var tok = ot.tokens[t];
                                if (tok.amount == 1) {
                                    this.isNFT = true;
                                    if (this.TokenId == tok.tokenId) {
                                        this.checkIfIsNFT(this.txid);
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
                        if (true) {
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
                                if (data.vin[0].tokens[0].tokenId == thisClass.TokenId) {
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

            var url = 'https://ntp1node.nebl.io/ntp1/tokenmetadata/' + this.TokenId + '/' + initTxId;
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

                                // check if it is NFT
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
                                }

                                // check if it is NFT init tx
                                for( var m in data.metadataOfUtxo.userData.meta) {
                                    var nftfirst = data.metadataOfUtxo.userData.meta[m]['NFT FirstTx'];
                                    if (nftfirst != undefined){
                                        //console.log('NFT First Tx!!!');
                                        thisClass.NFTFirstTxFound = true;
                                        thisClass.firstNFTtxId = initTxId;
                                        // This is first NFT tx so we can load all info from it
                                        for( var m in data.metadataOfUtxo.userData.meta) {
                                            
                                            var name = data.metadataOfUtxo.userData.meta[m]['Name'];
                                            if (name != undefined){
                                                thisClass.originalNFTName = name;
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

                                            var type = data.metadataOfUtxo.userData.meta[m]['Type'];
                                            if (type != undefined){
                                                thisClass.originalNFTType = type;
                                            }

                                            var link = data.metadataOfUtxo.userData.meta[m]['Link'];
                                            if (link != undefined){
                                                thisClass.originalNFTLink = link;
                                            }

                                            var youtube = data.metadataOfUtxo.userData.meta[m]['Youtube'];
                                            if (youtube != undefined){
                                                thisClass.youtubeCode = youtube;
                                            }
                                        }

                                        return;
                                    }
                                }
                                // load source Utxo info
                                for( var m in data.metadataOfUtxo.userData.meta) {

                                    var srcutxo = data.metadataOfUtxo.userData.meta[m]['SourceUtxo'];
                                    if (srcutxo != undefined){
                                        thisClass.firstNFTtxId = srcutxo;
                                        // if the NFT wasnt found but sourceUtxo info is available check this txid
                                        if (!thisClass.orignalNFTDataFound) {
                                            thisClass.checkIfIsNFT(srcutxo);
                                            return;
                                        }
                                    }
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

    getNFTComponent() {
        if (this.getNFTImage() != '' && this.isOrignalNFTDataFound) {
            var videoComponent = '';
            if (this.youtubeCode != '' && this.youtubeCode != ' ') {
                var link = 'https://youtube.com/embed/' + this.youtubeCode;

                videoComponent = 
                '<div class="row" style="margin-top: 10px">' +
                '   <div class="col">' +
                '       <iframe width="100%" height="100%" src="' + link + '" ' +
                '           style="max-width: 350px;" ' +
                '           title="YouTube video NFT" ' +
                '           frameborder="0" ' +
                '           allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture" ' +
                '           allowfullscreen>' +
                '       </iframe>' +
                '   </div>' +
                '</div>';
            }

            var nftComponent = '<div class="card shadow" style="max-width: 350px; width:100%">'+
            '    <div class="card-header d-xl-flex justify-content-xl-center align-items-xl-center py-3">'+
            '        <h4>' + this.originalNFTName + '</h4>'+
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
            '                           <span>' + this.originalNFTName + '</span>'+
            '                        </div>'+
            '                    </div>'+
            '                    <div class="row">'+
            '                        <div class="col d-flex d-xl-flex justify-content-center justify-content-xl-center align-items-xl-center" style="margin-top: 10px;">'+
            '                           <span>Type</span>'+
            '                        </div>'+
            '                    </div>'+
            '                    <div class="row">'+
            '                        <div class="col d-flex d-xl-flex justify-content-center justify-content-xl-center align-items-xl-center">'+
            '                           <span>'+ this.originalNFTType + '</span>'+
            '                        </div>'+
            '                    </div>'+
            '                    <div class="row">'+
            '                        <div class="col d-flex d-xl-flex justify-content-center justify-content-xl-center align-items-xl-center" style="margin-top: 10px;">'+
            '                           <span>Tx Id</span>'+
            '                        </div>'+
            '                    </div>'+
            '                    <div class="row">'+
            '                           <div class="col d-flex d-xl-flex justify-content-center justify-content-xl-center align-items-xl-center">'+
            '                               <span style="font-size: 12px;"><a href="https://explorer.nebl.io/tx/' + this.firstNFTtxId + '" target="blank">Go To Explorer</a></span>'+
            '                           </div>'+
            '                    </div>'+
            '                    <div class="row">'+
            '                        <div class="col d-flex d-xl-flex justify-content-center justify-content-xl-center align-items-xl-center" style="margin-top: 10px;">'+
            '                           <span>Description</span>'+
            '                        </div>'+
            '                    </div>'+
            '                    <div class="row">'+
            '                       <div class="col d-flex d-xl-flex justify-content-center justify-content-xl-center align-items-xl-center">'+
            '                           <span style="font-size: 12px;">' + this.originalNFTDecription + '</span>'+
            '                       </div>'+
            '                    </div>'+
            '                    <div class="row">'+
            '                        <div class="col d-flex d-xl-flex justify-content-center justify-content-xl-center align-items-xl-center" style="margin-top: 10px;">'+
            '                           <span>NFT Last TxId</span>'+
            '                        </div>'+
            '                    </div>'+
            '                    <div class="row">'+
            '                       <div class="col d-flex d-xl-flex justify-content-center justify-content-xl-center align-items-xl-center">'+
            '                           <span style="font-size: 12px;"><a href="https://explorer.nebl.io/tx/' + this.txid +'"  target="blank">Last Transaction With This NFT</a></span>'+
            '                       </div>'+
            '                    </div>'+
            '                    <div class="row">'+
            '                       <div class="col d-flex d-xl-flex justify-content-center justify-content-xl-center align-items-xl-center">'+
            '                           <span style="font-size: 12px;"><a href="' + this.originalNFTImage +'" target="blank"><img style="max-width:200px;max-height:200px;margin-left:5px" src="' + this.originalNFTImage + '"></a></span>'+
            '                       </div>'+
            '                    </div>'+
                                videoComponent +
            '                </div>'+
            '            </div>'+
            '        </div>'+
            '    </div>'+
            '</div>'; 
            
            return nftComponent;
        }
    }
}