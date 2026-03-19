let stripeInstance = null;
let checkoutInstance = null;

window.stripeCheckout = {
    initialize: async function (publishableKey, clientSecret) {
        if (!stripeInstance) {
            stripeInstance = Stripe(publishableKey);
        }

        if (checkoutInstance) {
            checkoutInstance.destroy();
        }

        checkoutInstance = await stripeInstance.initEmbeddedCheckout({
            clientSecret: clientSecret
        });

        checkoutInstance.mount('#stripe-checkout');
    },

    destroy: function () {
        if (checkoutInstance) {
            checkoutInstance.destroy();
            checkoutInstance = null;
        }
    }
};
