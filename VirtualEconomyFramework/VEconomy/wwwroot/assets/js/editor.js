let jar = null;
try {
    var editor = document.querySelector('.editor');
    if (editor != null) {
        jar = CodeJar(editor, Prism.highlightElement);
    }
}
catch {}