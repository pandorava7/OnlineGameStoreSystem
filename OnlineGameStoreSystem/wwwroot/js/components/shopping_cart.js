var cartItemsContainer = document.getElementById('cart-items-container');
var cartTotalElement = document.getElementById('cart-total');
var cartTotalFinalElement = document.getElementById('cart-total-final');

function updateCartTotal(cart) {
    let sum = cart.reduce((t, i) => t + i.price, 0);
    cartTotalElement.textContent = sum.toFixed(2);
    cartTotalFinalElement.textContent = sum.toFixed(2);
}

function renderCartItems(cart) {
    console.log("Rendering cart items:", cart);
    cartItemsContainer.innerHTML = "";
    if (cart.length === 0) {
        cartItemsContainer.innerHTML = `<p class="empty-cart-message">Your cart is empty</p>`;
        updateCartTotal(cart);
        return;
    }

    cart.forEach(item => {
        const el = document.createElement('div');
        el.classList.add("cart-item");

        // 价格显示逻辑
        let priceHtml = '';
        let discountPercent = '';
        if (item.discount_price == null) {
            // 无折扣
            priceHtml = `
            <div class="price-left"></div>
            <div class="price-right">
                <div class="item-price">RM ${item.price.toFixed(2)}</div>
            </div>`;
        } else if (item.discount_price === 0) {
            // 免费
            priceHtml = `
            <div class="price-left"></div>
            <div class="price-right">
                <div class="item-original-price">RM ${item.price.toFixed(2)}</div>
                <div class="item-price">Free</div>
            </div>`;
        } else {
            // 有折扣
            discountPercent = Math.round((1 - item.discount_price / item.price) * 100);
            priceHtml = `
            <div class="price-left">
                <span class="item-discount">-${discountPercent}%</span>
            </div>
            <div class="price-right">
                <div class="item-original-price">RM ${item.price.toFixed(2)}</div>
                <div class="item-price">RM ${item.discount_price.toFixed(2)}</div>
            </div>`;
        }

        el.innerHTML = `
            <div class="item-left">
                <img src="${item.image}" alt="${item.name}" class="item-img">
            </div>
            <div class="item-right">
                <div class="item-name">${item.name}</div>
                <div class="item-price-container">
                    ${priceHtml}
                </div>
                <div class="item-actions">
                    <button class="secondary-btn remove-item-btn" data-id="${item.id}">Remove</button>
                </div>
            </div>
        `;

        // 渲染完成后添加事件
        el.querySelectorAll('.remove-item-btn').forEach(btn => {
            btn.addEventListener('click', async () => {
                const cartItemId = btn.dataset.id;

                await CartAPI.removeCartItem(cartItemId);  // 等后端删除 + reload 执行完
                await CartAPI.loadCartFromServer(); // 重新加载购物车数据

                renderCartItems(CartAPI.cart);             // 用新的 cart 渲染
            });
        });

        cartItemsContainer.appendChild(el);
    });

    updateCartTotal(cart);
}


// 初始加载购物车
document.addEventListener("DOMContentLoaded", async () => {
    if (isLoggedIn) {
        await CartAPI.loadCartFromServer(); // 确保购物车数据最新
        renderCartItems(CartAPI.cart);            // 渲染UI
    }
});