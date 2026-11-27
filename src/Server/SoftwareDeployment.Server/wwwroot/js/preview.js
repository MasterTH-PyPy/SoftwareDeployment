// Live-Update des Vorschau-CSS im isolierten iframe, ohne das iframe neu zu laden.
window.setPreviewCss = function (css) {
    var frame = document.getElementById('cssPreviewFrame');
    if (!frame || !frame.contentDocument) return;
    var style = frame.contentDocument.getElementById('userCss');
    if (style) style.textContent = css || '';
};
