
window.alertMessage = (text) => {
    alert(text);
}

window.clearElementHTMLContentById = (id) => {
    document.getElementById(id).innerHTML = '';
}

window.jsFunctions = {
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
    }
};
