// cart-store.js
window.CartAPI = {
    cart: [],

    async loadCartFromServer() {
        // 从后端获取购物车数据
        const res = await fetch(`/Cart/GetItems`);
        const data = await res.json();
        this.cart = data.items || [];
        // 更新徽章数量
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
            // 更新数字
            cartBadge.textContent = this.cart.length;
            // 显示或隐藏徽章
            cartBadge.style.display = this.cart.length > 0 ? 'block' : 'none';
        }
    }
};
