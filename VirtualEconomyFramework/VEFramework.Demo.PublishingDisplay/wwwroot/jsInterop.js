$(document).ready(function () {
    $(document).on("click", ".text", function () {
        $(this).find("a").attr('target', '_blank');
    });
});

window.jsFunctions = {
    buzzsproutPodcast: function (link) {
        var script = document.createElement('script');
        script.src = link;
        document.head.appendChild(script);
    }
}