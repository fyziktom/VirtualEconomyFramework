try {
    var editor = document.querySelector('.editor');
    let jar = null;
    if (editor != null) {
        let jar = CodeJar(editor, Prism.highlightElement);
    }
}
catch {}