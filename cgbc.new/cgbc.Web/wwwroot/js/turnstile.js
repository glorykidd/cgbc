window.cgbcTurnstile = {
    render: function (elementId, siteKey) {
        if (!window.turnstile) {
            return;
        }
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
