var barcodereader = null;
var dotnetRef = null;
var loadedImage = null;
var html5QrCode = null;
var ipfsHolder = null;
var uploadReference = null;
var ipfs = null;
(async () => {
    //global init
})();

window.setMusicInfo = (title, artist, description) => {
    
    if ('mediaSession' in navigator) {
        navigator.mediaSession.metadata = new MediaMetadata({
            title: title,
            artist: artist,
            album: description,
            artwork: [
                { src: 'https://dummyimage.com/96x96', sizes: '96x96', type: 'image/png' },
                { src: 'https://dummyimage.com/128x128', sizes: '128x128', type: 'image/png' },
                { src: 'https://dummyimage.com/192x192', sizes: '192x192', type: 'image/png' },
                { src: 'https://dummyimage.com/256x256', sizes: '256x256', type: 'image/png' },
                { src: 'https://dummyimage.com/384x384', sizes: '384x384', type: 'image/png' },
                { src: 'https://dummyimage.com/512x512', sizes: '512x512', type: 'image/png' },
            ]
        });
    }
}
window.setCoruzantPodcastInfo = (title, artist, description) => {

    if ('mediaSession' in navigator) {
        navigator.mediaSession.metadata = new MediaMetadata({
            title: title,
            artist: artist,
            album: description,
            artwork: [
                { src: 'https://ve-nft.com/CoruzantTokenLogo-256x256.png', sizes: '256x256', type: 'image/png' }
            ]
        });
    }
}

window.alertMessage = (text) => {
    alert(text);
}

window.clearElementHTMLContentById = (id) => {
    document.getElementById(id).innerHTML = '';
}

window.jsFunctions = {
    init: function (obj) {
        dotnetRef = obj;
        if (html5QrCode != null) {
            html5QrCode.clear();
        }
    },
    buzzsproutPodcast: function (link) {
        var script = document.createElement('script');
        script.src = link;
        document.head.appendChild(script);
    },
    downloadText: function (data, filename) {
        var text = data;
        //text = text.replace(/\n/g, "\r\n"); // To retain the Line breaks.
        var blob = new Blob([text], { type: "text/plain" });
        var anchor = document.createElement("a");
        anchor.download = filename;
        anchor.href = window.URL.createObjectURL(blob);
        anchor.target = "_blank";
        anchor.style.display = "none"; // just to be safe!
        document.body.appendChild(anchor);
        anchor.click();
        document.body.removeChild(anchor);
    },
    copyToClipboard: function (text) {
        navigator.clipboard.writeText(text);
    },
    focusElement: function (element) {
        try {
            element.focus();
        }
        catch {

        }
    },
    getQRCode: function (text, codediv) {
        codediv.innerHTML = '';
        new QRCode(codediv, text);
    }
};
