let cart = [];
const userId = 1;
const shoppingCartId = 1;
const isLoggedIn = true;


var cartItemsContainer = document.getElementById('cart-items-container');
var cartTotalElement = document.getElementById('cart-total');
var cartTotalFinalElement = document.getElementById('cart-total-final');
var cartBadge = document.getElementById('cart-badge');

// =====================
// 读取数据库购物车
// =====================
async function loadCartFromServer() {
    const res = await fetch(`/Cart/GetItems?userId=${userId}`);
    const data = await res.json();
    cart = data.items || [];
    renderCartItems();
    updateCartBadge();
    updateAddToCartButtonStates();
}

// =====================
// 删除商品（服务器）
// =====================
async function removeCartItem(cartId) {
    const res = await fetch(`/Cart/RemoveItem?cartId=${cartId}`, {
        method: 'POST'
    });

    const result = await res.json();
    if (!result.success) {
        console.error("Failed to remove item from cart:", result.message);
        showTemporaryMessage("移除失败，请稍后重试", "error");
        return;
    }

    await loadCartFromServer();
    showTemporaryMessage("已移除", "info");
}

// =====================
// 以下函数保持原样（渲染 UI）
// =====================

function updateCartBadge() {
    if (cartBadge) {
        cartBadge.textContent = cart.length;
        cartBadge.style.display = cart.length > 0 ? 'block' : 'none';
    }
}

function updateAddToCartButtonStates() {
    document.querySelectorAll('.btn-add-to-cart').forEach(btn => {
        const gameId = btn.dataset.gameId;

        if (cart.some(i => i.id == gameId)) {
            btn.disabled = true;
            btn.innerHTML = '<i class="fas fa-check"></i> 已在购物车';
        } else {
            btn.disabled = false;
            btn.innerHTML = '<i class="fas fa-plus"></i> 加入购物车';
        }
    });
}

function updateCartTotal() {
    let sum = cart.reduce((t, i) => t + i.price, 0);
    cartTotalElement.textContent = sum.toFixed(2);
    cartTotalFinalElement.textContent = sum.toFixed(2);
}

function renderCartItems() {
    cartItemsContainer.innerHTML = "";
    if (cart.length === 0) {
        cartItemsContainer.innerHTML = `<p class="empty-cart-message">购物车是空的</p>`;
        updateCartTotal();
        return;
    }

    cart.forEach(item => {
        const el = document.createElement('div');
        el.classList.add("cart-item");
        el.innerHTML = `
            <div class="item-details">
                <img src="${item.image}">
                <div class="item-info">
                    <span class="item-name">${item.name}</span>
                    <span class="item-price">RM ${item.price.toFixed(2)}</span>
                </div>
            </div>
            <span class="item-subtotal">RM ${item.price.toFixed(2)}</span>
            <button class="remove-item-btn" data-id="${item.id}">
                <i class="fas fa-trash-alt"></i>
            </button>
        `;
        cartItemsContainer.appendChild(el);
    });

    document.querySelectorAll('.remove-item-btn').forEach(btn => {
        btn.addEventListener("click", () => {
            removeCartItem(btn.dataset.id);
        });
    });

    updateCartTotal();
}

// 初始加载购物车
document.addEventListener("DOMContentLoaded", () => {
    if (isLoggedIn) {
        loadCartFromServer();
    }
});