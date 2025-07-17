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

window.digestSha256 = async function (dataHex) {
    const data = new Uint8Array(
        dataHex.match(/.{1,2}/g).map(byte => parseInt(byte, 16))
    );

    const hashBuffer = await crypto.subtle.digest("SHA-256", data);
    const hashArray = new Uint8Array(hashBuffer);

    const hashHex = Array.from(hashArray)
        .map(b => b.toString(16).padStart(2, '0'))
        .join('');

    return hashHex;
};
window.encryptAes = async function encryptBytes(keyHex, dataHex, ivHex) {

    const key = new Uint8Array(keyHex.match(/.{1,2}/g).map(byte => parseInt(byte, 16)));
    const data = new Uint8Array(dataHex.match(/.{1,2}/g).map(byte => parseInt(byte, 16)));
    const iv = new Uint8Array(ivHex.match(/.{1,2}/g).map(byte => parseInt(byte, 16)));

    try {
        const cryptoKey = await crypto.subtle.importKey(
            'raw',
            key,
            { name: 'AES-CBC' },
            false,
            ['encrypt']
        );

        const encryptedBuffer = await crypto.subtle.encrypt(
            { name: 'AES-CBC', iv: iv },
            cryptoKey,
            data
        );

        const encryptedBytes = new Uint8Array(encryptedBuffer);
        const encryptedHex = Array.from(encryptedBytes)
            .map(b => b.toString(16).padStart(2, '0'))
            .join('');

        return encryptedHex;
    } catch (error) {
        console.error('Encryption failed:', error);
        throw error;
    }
};

window.decryptAes = async function decryptBytes(keyHex, secretHex, ivHex) {
    const key = new Uint8Array(keyHex.match(/.{1,2}/g).map(byte => parseInt(byte, 16)));
    const secret = new Uint8Array(secretHex.match(/.{1,2}/g).map(byte => parseInt(byte, 16)));
    const iv = new Uint8Array(ivHex.match(/.{1,2}/g).map(byte => parseInt(byte, 16)));

    const algorithm = {
        name: 'AES-CBC',
        iv: iv
    };

    try {
        const cryptoKey = await crypto.subtle.importKey(
            'raw',
            key,
            { name: 'AES-CBC' },
            false,
            ['decrypt']
        );

        const decryptedBuffer = await crypto.subtle.decrypt(
            algorithm,
            cryptoKey,
            secret
        );

        const decoder = new TextDecoder();
        const decryptedText = decoder.decode(decryptedBuffer);

        return decryptedText;
    } catch (error) {
        console.error('Decryption failed:', error);
        throw error;
    }
};
window.decryptAes1 = async function (key, iv, data) {
    try {
        console.log(`Key Length: ${key.byteLength}`);
        console.log(`IV Length: ${iv.byteLength}`);
        console.log(`Data Length: ${data.byteLength}`);

        const cryptoKey = await window.crypto.subtle.importKey(
            "raw",
            key,
            { name: "AES-CBC" },
            false,
            ["decrypt"]
        );
        console.log("Crypto key imported successfully");

        const decrypted = await window.crypto.subtle.decrypt(
            {
                name: "AES-CBC",
                iv: iv
            },
            cryptoKey,
            data
        );

        return new Uint8Array(decrypted);
    } catch (error) {
        console.error("Decryption error:", error);
        throw error; // rethrow error for further handling
    }
};
