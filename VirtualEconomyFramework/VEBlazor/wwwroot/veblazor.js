class VEBlazorInterop {
    constructor() {
    }

    downloadText(data, filename) {
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
    }

    copyToClipboard(text) {
        navigator.clipboard.writeText(text);
    }

    readFromClipboard() {
        return navigator.clipboard.readText();
    }

    focusElement(element) {
        try {
            element.focus();
        }
        catch {

        }
    }
    MermaidInitialize() {
        mermaid.initialize({
            startOnLoad: true,
            securityLevel: "loose",
            // Other options.
        });
    }
    MermaidRender() {
        mermaid.init();
    }
}

window.veblazor = new VEBlazorInterop()