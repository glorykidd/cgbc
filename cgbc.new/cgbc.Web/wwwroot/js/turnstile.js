let apiScriptPromise = null;

function loadApiScript() {
    if (!apiScriptPromise) {
        apiScriptPromise = new Promise(function (resolve, reject) {
            const script = document.createElement('script');
            script.src = 'https://challenges.cloudflare.com/turnstile/v0/api.js';
            script.async = true;
            script.defer = true;
            script.onload = resolve;
            script.onerror = reject;
            document.head.appendChild(script);
        });
    }
    return apiScriptPromise;
}

window.cgbcTurnstile = {
    // Cloudflare's api.js is only loaded on demand, when a page actually
    // renders a Turnstile widget, rather than unconditionally on every page.
    render: async function (elementId, siteKey) {
        await loadApiScript();
        window.turnstile.render('#' + elementId, { sitekey: siteKey });
    },

    getResponse: function (elementId) {
        if (!window.turnstile) {
            return null;
        }
        return window.turnstile.getResponse(document.getElementById(elementId)) || null;
    },

    reset: function (elementId) {
        if (!window.turnstile) {
            return;
        }
        window.turnstile.reset(document.getElementById(elementId));
    }
};
