
var ipfsHolder = null;
try {
    ipfsHolder = document.getElementById('ipfsImageUpload');
}
catch{}

if (ipfsHolder != null) {

    ipfsHolder.addEventListener('dragenter', function(e) {
        e.stopPropagation();
        e.preventDefault();
    });

    ipfsHolder.addEventListener('dragleave', function(e) {
        e.stopPropagation();
        e.preventDefault();
    });

    ipfsHolder.addEventListener('dragover', function(e) {
        e.stopPropagation();
        e.preventDefault();
        e.dataTransfer.dropEffect = 'copy';
    });

    const ipfs = window.IpfsHttpClient('ipfs.infura.io', '5001', { protocol: 'https' });

    ipfsHolder.addEventListener('drop', function(e) {
        var spinner = '<div class="spinner-border" role="status"><span class="sr-only">Loading...</span></div>';
        $('#ipfsImageColumn').html(spinner);
        e.preventDefault();
        var d = e.dataTransfer.getData('text');
        var file = e.dataTransfer.files[0];
        var reader = new FileReader();
        if (file != null) {

            reader.onload = function(event) {
                const magic_array_buffer_converted_to_buffer = buffer.Buffer(reader.result); // honestly as a web developer I do not fully appreciate the difference between buffer and arrayBuffer 
                ipfs.add(magic_array_buffer_converted_to_buffer, (err, result) => {
                    console.log(err, result);
        
                    let ipfsLink = "https://gateway.ipfs.io/ipfs/" + result[0].hash;
                    $("#mintNFTImage").val(ipfsLink);

                    $('#ipfsImageColumn').html('<img id="ipfsUploadedImage" src="' + ipfsLink + '" style="width: auto;height: 100%; max-height:140px" alt="Image Uploaded...Waiting for confirmation now..." />');
                });
            };

            reader.readAsArrayBuffer(file);
        }
    });
}

function ipfsImageLoaded() {
    
}