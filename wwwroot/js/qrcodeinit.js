function initQrCode() {
    const el = document.getElementById('qrCodeData');
    const qrEl = document.getElementById('qrCode');
    if (el && qrEl && !qrEl.hasChildNodes()) {
        new QRCode(qrEl, {
            text: el.dataset.url,
            width: 200,
            height: 200
        });
    }
}

// Run on initial page load
initQrCode();

// Re-run after Blazor enhanced navigation patches the DOM
function waitForBlazor() {
    if (typeof Blazor !== 'undefined' && Blazor.addEventListener) {
        Blazor.addEventListener('enhancedload', initQrCode);
    } else {
        setTimeout(waitForBlazor, 50);
    }
}
waitForBlazor();
