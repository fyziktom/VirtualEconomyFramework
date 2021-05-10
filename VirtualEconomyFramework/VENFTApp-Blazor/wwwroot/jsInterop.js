var barcodereader = null;
var dotnetRef = null;
var loadedImage = null;
var html5QrCode = null;
(async () => {
    //global init
})();

window.jsFunctions = {
    init: function (obj) {
        dotnetRef = obj;
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
        html5QrCode.stop().then(ignore => {
            // QR Code scanning is stopped.
            console.log("QR Code scanning stopped.");
        }).catch(err => {
            // Stop failed, handle it.
            console.log("Unable to stop scanning.");
        });
    },
    startScanning: function (qrCodeRef) {
        html5QrCode = new Html5Qrcode(qrCodeRef);

        Html5Qrcode.getCameras({ facingMode: "environment" }).then(devices => {
            /**{ facingMode: "environment" }
             * devices would be an array of objects of type:
             * { id: "id", label: "label" }
             */
            if (devices && devices.length > 0) {
                var cameraId = devices[0].id;
                var tryread = 200;

                html5QrCode.start(
                    cameraId,     // retreived in the previous step.
                    {
                        fps: 10,    // sets the framerate to 10 frame per second
                        qrbox: 300  // sets only 250 X 250 region of viewfinder to
                        // scannable, rest shaded.
                    },
                    qrCodeMessage => {
                        // do something when code is read. For example:
                        console.log(`QR Code detected: ${qrCodeMessage}`);
                        if (dotnetRef) {
                            dotnetRef.invokeMethodAsync('ReturnBarcodeResultsAsync', qrCodeMessage);
                        }
                    },
                    errorMessage => {
                        // parse error, ideally ignore it. For example:
                        console.log(`QR Code no longer in front of camera.`);
                        tryread--;
                        if (tryread < 0) {
                            html5QrCode.stop().then(ignore => {
                                // QR Code scanning is stopped.
                                console.log("QR Code scanning stopped.");
                            }).catch(err => {
                                // Stop failed, handle it.
                                console.log("Unable to stop scanning.");
                            });
                        }
                    })
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
