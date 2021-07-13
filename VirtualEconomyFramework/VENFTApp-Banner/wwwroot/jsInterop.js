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
    initUpload: function (obj) {
        if (uploadReference == null) {
            uploadReference = obj;
        }

        try {
            ipfsHolder = document.getElementById('ipfsImageUpload');
        }
        catch {
            console.log("Cannot find the frame for image upload.");
            return;
        }
        
        if (ipfsHolder != null) {

            //var jQueryScript = document.createElement('script');
            //jQueryScript.setAttribute('src', 'https://bundle.run/buffer@5.2.1');
            //document.head.appendChild(jQueryScript);
            /*
            var jQueryScript1 = document.createElement('script');
            jQueryScript1.setAttribute('src', 'https://unpkg.com/ipfs-http-client@30.1.3/dist/index.js');
            document.head.appendChild(jQueryScript1);
            */
            
            ipfsHolder.addEventListener('dragenter', function (e) {
                e.stopPropagation();
                e.preventDefault();
            });

            ipfsHolder.addEventListener('dragleave', function (e) {
                e.stopPropagation();
                e.preventDefault();
            });

            ipfsHolder.addEventListener('dragover', function (e) {
                e.stopPropagation();
                e.preventDefault();
                e.dataTransfer.dropEffect = 'copy';
            });

            var jQueryScript1 = document.createElement('script');
            jQueryScript1.setAttribute('src', 'buffer@5.2.1.js');
            jQueryScript1.setAttribute('type', 'text/javascript');
            document.head.appendChild(jQueryScript1);
            var jQueryScript = document.createElement('script');
            jQueryScript.setAttribute('src', 'https://unpkg.com/ipfs-http-client@30.1.3/dist/index.js');
            jQueryScript.setAttribute('type', 'text/javascript');
            document.head.appendChild(jQueryScript);

            ipfs = window.IpfsHttpClient('ipfs.infura.io', '5001', { protocol: 'https' });

            // example from https://ethereum.stackexchange.com/questions/4531/how-to-add-a-file-to-ipfs-using-the-api

            ipfsHolder.addEventListener('drop', function (e) {

                //const ipfs = window.IpfsHttpClient('ipfs.infura.io', '5001', { protocol: 'https' });

                e.preventDefault();
                var d = e.dataTransfer.getData('text');
                var file = e.dataTransfer.files[0];
                var reader = new FileReader();

                if (uploadReference) {
                    uploadReference.invokeMethodAsync('UploadStartedAsync', 'Started');
                }

                if (file != null) {

                    reader.onload = function (event) {
                        const buf = buffer.Buffer(reader.result); // honestly as a web developer I do not fully appreciate the difference between buffer and arrayBuffer 

                        ipfs.add(buf, function (err, result) {
                            console.log(err, result);

                            let ipfsLink = "https://gateway.ipfs.io/ipfs/" + result[0].hash;
                            if (uploadReference) {
                                uploadReference.invokeMethodAsync('UploadImageResultsAsync', ipfsLink);
                            }
                        });
                    };

                    reader.readAsArrayBuffer(file);
                }
            });
        }
    },
    uploadToIpfsWithButton: function (obj) {
        if (uploadReference == null) {
            uploadReference = obj;
        }
        try {
            let input = document.createElement("input");
            input.setAttribute("capture", "");
            input.setAttribute("accept", "image/*");
            input.type = "file";
            
            // example from https://ethereum.stackexchange.com/questions/4531/how-to-add-a-file-to-ipfs-using-the-api
            
            input.onchange = async function () {

                try {
                    const file = input.files[0];
                    var reader = new FileReader();

                    if (uploadReference) {
                        uploadReference.invokeMethodAsync('UploadStartedAsync', 'Started');
                    }

                    if (file != null) {
                        reader.onload = function (event) {

                            const buf = buffer.Buffer(reader.result); // honestly as a web developer I do not fully appreciate the difference between buffer and arrayBuffer 
                            ipfs.add(buf, function (err, result) {
                                console.log(err, result);
                                let ipfsLink = "https://gateway.ipfs.io/ipfs/" + result[0].hash;
                                if (uploadReference) {
                                    uploadReference.invokeMethodAsync('UploadImageResultsAsync', ipfsLink);
                                }
                            });
                        };
                        
                        reader.readAsArrayBuffer(file);
                    }

                } catch (error) {
                    console.log(error);
                    if (dotnetRef) {
                        //dotnetRef.invokeMethodAsync('ReturnBarcodeResultsAsync', `Cannot Load QR Code! ${error}`);
                    }
                }

            };
            input.click();
        }
        catch (err) {
            console.log(err);
        }
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
    },
    stopScanning: function () {
        
        if (html5QrCode != null) {
            html5QrCode.stop().then(ignore => {
                // QR Code scanning is stopped.
                dotnetRef.invokeMethodAsync('CameraStoppedAsync', 'Cammera Stopped');
                console.log("QR Code scanning stopped.");
            }).catch(err => {
                // Stop failed, handle it.
                dotnetRef.invokeMethodAsync('CameraStoppedAsync', 'Unable to Stop Camera');
                console.log("Unable to stop scanning.");
            });
        }
    },
    startScanning: function (qrCodeRef) {
        html5QrCode = new Html5Qrcode(qrCodeRef);
        var start = true;
        var tryread = 300;
        const config = { fps: 10, qrbox: 190 };
        var qrCodeSuccessCallback = message => {
            console.log(`QR Code detected: ${message}`);
            if (dotnetRef) {
                dotnetRef.invokeMethodAsync('ReturnBarcodeResultsAsync', message);
            }
        };

        var qrCodeErrorCallback = errorMessage => {
            // parse error, ideally ignore it. For example:
            if (start) {
                dotnetRef.invokeMethodAsync('CameraStartedAsync', 'Camera Started');
                start = false;
            }
            console.log(`QR Code no longer in front of camera.` + errorMessage);
            if (errorMessage.includes('Failed')) {
                html5QrCode.clear();
                html5QrCode = null;
                document.getElementById(qrCodeRef).innerHTML = '';
                this.startScanning(qrCodeRef);
            }
            tryread--;
            if (tryread < 0) {
                html5QrCode.stop().then(ignore => {
                    // QR Code scanning is stopped.
                    dotnetRef.invokeMethodAsync('CameraStoppedAsync', 'Camera Stopped');
                    console.log("QR Code scanning stopped.");
                }).catch(err => {
                    // Stop failed, handle it.
                    dotnetRef.invokeMethodAsync('CameraStoppedAsync', 'Unable to Stop Camera');
                    console.log("Unable to stop scanning.");
                });
            }
        };

        Html5Qrcode.getCameras({ facingMode: "environment" }).then(devices => {

            if (devices && devices.length > 0) {
                var cameraId = devices[0].id;

                // back camera or fail https://github.com/mebjas/html5-qrcode/issues/65
                // { facingMode: { exact: "environment"} }
                // { facingMode: "environment" }
                html5QrCode.start(cameraId, config, qrCodeSuccessCallback, qrCodeErrorCallback)
                    .catch(err => {
                        // Start failed, handle it. For example,
                        console.log(`Unable to start scanning, error: ${err}`);
                        if (dotnetRef) {
                            dotnetRef.invokeMethodAsync('ReturnBarcodeResultsAsync', qrCodeMessage);
                        }
                    });
            }
        });
    },
    selectFile: function () {
        
        let input = document.createElement("input");
        input.setAttribute("capture", "");
        input.setAttribute("accept", "image/*");
        input.type = "file";
        input.onchange = async function () {
            
            try {
                const file = input.files[0];
                console.log(file.name);
                console.log(file.size);

                const html5QrCode = new Html5Qrcode("reader");
                await html5QrCode.scanFile(file, /* showImage= */false)
                    .then(qrCodeMessage => {
                        // success, use qrCodeMessage
                        console.log(qrCodeMessage);
                        if (dotnetRef) {
                            dotnetRef.invokeMethodAsync('ReturnBarcodeResultsAsync', qrCodeMessage);
                        }
                    })
                    .catch(err => {
                        // failure, handle it.
                        console.log(`Error scanning file. Reason: ${err}`);
                        if (dotnetRef) {
                            dotnetRef.invokeMethodAsync('ReturnBarcodeResultsAsync', `Cannot Load QR Code ${err}!`);
                        }
                        console.log("No code found");
                    });
                // my function
                /*
                var reader = new FileReader();
                reader.readAsDataURL(file);
                reader.onload = function (e) {
                    //loadedImage = new Image();
                    //loadedImage.src = e.target.result;

                    
                    loadedImage.onload = function () {

                        console.log("Start decoding QR Code");
                        console.log(this.width + " " + this.height);

                        
                        var canvas = document.createElement("canvas");
                        canvas.width = loadedImage.width;
                        canvas.height = loadedImage.height;
                        var ctx = canvas.getContext("2d");
                        ctx.drawImage(loadedImage, 0, 0, loadedImage.width, loadedImage.height);

                        var imageData = ctx.getImageData(0, 0, canvas.width, canvas.height);
                        var code = jsQR(imageData.data, imageData.width, imageData.height, {
                            inversionAttempts: "dontInvert",
                        });

                        if (code) {
                            console.log("Found QR code", code.data);

                            if (dotnetRef) {
                                dotnetRef.invokeMethodAsync('ReturnBarcodeResultsAsync', code.data);
                            }
                        }
                        else {
                            if (dotnetRef) {
                                dotnetRef.invokeMethodAsync('ReturnBarcodeResultsAsync', 'Cannot Load QR Code!');
                            }
                            console.log("No code found");
                        }
                    };
                }*/
            } catch (error) {
                if (dotnetRef) {
                    dotnetRef.invokeMethodAsync('ReturnBarcodeResultsAsync', `Cannot Load QR Code! ${error}`);
                }
            }

        };
        input.click();
    },
};
