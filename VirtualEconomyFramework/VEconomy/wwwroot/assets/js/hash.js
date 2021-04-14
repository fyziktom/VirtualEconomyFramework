var hld = null;
try {
    hld = document.getElementById('holder');
}
catch{}

var holder = null;
if (hld != undefined && hld != null) {
    var holder = hld;
}

if (holder != null) {
    var finalHash = '';
    var fileHashName = '';

    holder.addEventListener('dragenter', function(e) {
        e.stopPropagation();
        e.preventDefault();
    });

    holder.addEventListener('dragleave', function(e) {
        e.stopPropagation();
        e.preventDefault();
    });

    holder.addEventListener('dragover', function(e) {
        e.stopPropagation();
        e.preventDefault();
        e.dataTransfer.dropEffect = 'copy';
    });

    holder.addEventListener('drop', function(e) {
        e.preventDefault();
        var d = e.dataTransfer.getData('text');
        var file = e.dataTransfer.files[0];
        var reader = new FileReader();
        if (file != null) {
            $('#hashModalFileName').text('Selected file: ' + file.name.toString());
            fileHashName = file.name.toString();
            reader.onload = function(event) {
            var binary = event.target.result;
            var sha256 = CryptoJS.SHA256(binary).toString();
            $('#hashOfFile').text(sha256);
            finalHash = sha256;
            //console.log(sha256);
            };
        
            reader.readAsBinaryString(file);
        }
    });
}

