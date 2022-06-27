window.jsFunctions = {
    buzzsproutPodcast: function (link) {
        var script = document.createElement('script');
        script.src = link;
        document.head.appendChild(script);
    },
    copyToClipboard(text) {
        navigator.clipboard.writeText(text);
    }
}