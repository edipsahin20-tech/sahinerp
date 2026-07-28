function exportElementToPdf(elementId, filename) {
    var element = document.getElementById(elementId);
    if (!element) {
        return Promise.reject(new Error('PDF element not found: ' + elementId));
    }

    var opt = {
        margin: [0, 0, 0, 0],
        filename: filename,
        image: { type: 'jpeg', quality: 0.98 },
        html2canvas: { scale: 2, useCORS: true, letterRendering: true },
        jsPDF: { unit: 'mm', format: 'a4', orientation: 'portrait' }
    };

    return html2pdf().set(opt).from(element).save();
}
