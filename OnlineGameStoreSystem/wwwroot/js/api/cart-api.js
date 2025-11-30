// cart-store.js
window.CartAPI = {
    cart: [],

    async loadCartFromServer() {
        const res = await fetch(`/Cart/GetItems`);
        const data = await res.json();
        this.cart = data.items || [];
        this.updateCartBadge();
        return this.cart;
    },

    async removeCartItem(cartItemId) {
        const res = await fetch(`/Cart/RemoveItem?cartItemId=${cartItemId}`, {
            method: 'POST'
        });

        const result = await res.json();
        if (!result.success) {
            showTemporaryMessage("Removed failed", "error");
        }
        else {
            showTemporaryMessage("Removed Item", "info");
        }

        return result;
    },

    updateCartBadge() {
        const cartBadge = document.getElementById('cart-badge');
        if (cartBadge) {
            cartBadge.textContent = this.cart.length;
            cartBadge.style.display = this.cart.length > 0 ? 'block' : 'none';
        }
    }
};
