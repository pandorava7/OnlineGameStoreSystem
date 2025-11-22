// =========================================
// 3. 购物车辅助函数 (提到顶部，确保 loadCartFromLocalStorage 和 addToCart 可以调用)
// =========================================

// 更新购物车徽章数量 (现在是购物车中不同商品的数量)
function updateCartBadge() {
    if (cartBadge) {
        const totalUniqueItems = cart.length;
        cartBadge.textContent = totalUniqueItems;
        // 只有登录后才显示徽章
        cartBadge.style.display = isLoggedIn && totalUniqueItems > 0 ? 'block' : 'none';
    }
}

// 根据购物车内容更新“添加到购物车”按钮的状态
function updateAddToCartButtonStates() {
    const addToCartButtons = document.querySelectorAll('.btn-add-to-cart');
    addToCartButtons.forEach(button => {
        const gameCard = button.closest('.game-card');
        const gameId = gameCard ? gameCard.dataset.gameId : null;

        if (gameId && cart.some(item => item.id === gameId)) {
            button.disabled = true;
            button.innerHTML = '<i class="fas fa-check"></i> 已在购物车';
            button.classList.add('added');
        } else {
            button.disabled = false;
            button.innerHTML = '<i class="fas fa-plus"></i> 加入购物车';
            button.classList.remove('added');
        }
        // 未登录时，也禁用所有“加入购物车”按钮
        if (!isLoggedIn) {
            button.disabled = true;
            button.innerHTML = '<i class="fas fa-lock"></i> 请先登录';
            button.classList.add('disabled-by-login');
        } else {
            button.classList.remove('disabled-by-login');
        }
    });
}

// 保存购物车数据到 localStorage
function saveCartToLocalStorage() {
    console.log("saveCartToLocalStorage: 准备保存购物车到 localStorage:", cart); // DEBUG S1
    localStorage.setItem('roxyCart', JSON.stringify(cart));
    console.log("saveCartToLocalStorage: 购物车已保存到 localStorage。"); // DEBUG S2
    updateCartBadge();
    updateAddToCartButtonStates();
}

// 从 localStorage 加载购物车数据
function loadCartFromLocalStorage() {
    console.log("loadCartFromLocalStorage: 尝试从 localStorage 加载购物车..."); // DEBUG L1
    const storedCart = localStorage.getItem('roxyCart');
    console.log("loadCartFromLocalStorage: localStorage 中的 roxyCart:", storedCart); // DEBUG L2

    if (storedCart) {
        try {
            cart = JSON.parse(storedCart);
            cart = cart.map(item => ({
                id: item.id,
                name: item.name,
                price: parseFloat(item.price) || 0,
                image: item.image
            }));
            console.log("loadCartFromLocalStorage: 成功解析的购物车数据:", cart); // DEBUG L3
        } catch (e) {
            console.error("loadCartFromLocalStorage: 解析购物车数据失败", e); // DEBUG L4
            cart = [];
        }
    } else {
        console.log("loadCartFromLocalStorage: localStorage 中没有找到 roxyCart 数据。购物车清空。"); // DEBUG L5
        cart = [];
    }
    updateCartBadge();
    updateAddToCartButtonStates();

    // ===============================================
    // 关键修改：加载完数据后，如果是在购物车页面，立即渲染
    // ===============================================
    if (cartItemsContainer && cartTotalElement && cartTotalFinalElement) {
        console.log("loadCartFromLocalStorage: 检测到在购物车页面，准备渲染商品...");
        // 购物车页面加载时，如果未登录，则不渲染商品并显示登录提示
        if (!isLoggedIn) {
            cartItemsContainer.innerHTML = '<p class="empty-cart-message">请先登录才能查看您的购物车内容。</p>';
            updateCartTotal(); // 清空总价显示
            return; // 不执行后续渲染
        }
        renderCartItems(); // 确保在 cart 数组填充后才调用
    } else {
        console.log("loadCartFromLocalStorage: 不在购物车页面，不进行购物车商品渲染。");
    }
}

// =========================================
// 4. 购物车页面辅助函数 (提到顶部，确保渲染时可以调用)
// =========================================
function addCartEventListeners() {
    document.querySelectorAll('.remove-item-btn').forEach(button => {
        button.onclick = (e) => removeCartItem(e.target.closest('button').dataset.id);
    });
}

function removeCartItem(itemId) {
    cart = cart.filter(item => item.id !== itemId);
    saveCartToLocalStorage();
    // 移除后需要重新渲染
    // 只有在购物车页面才调用 renderCartItems，因为 saveCartToLocalStorage 也会在其他页面调用
    if (cartItemsContainer && cartTotalElement && cartTotalFinalElement) {
        renderCartItems();
    }
    showTemporaryMessage("商品已移除", "info");
}

function updateCartTotal() {
    let total = cart.reduce((sum, item) => sum + item.price, 0);
    if (cartTotalElement) {
        cartTotalElement.textContent = total.toFixed(2);
    }
    if (cartTotalFinalElement) {
        cartTotalFinalElement.textContent = total.toFixed(2);
    }
}

function renderCartItems() {
    console.log("renderCartItems: 正在渲染购物车商品，当前购物车:", cart); // DEBUG R1
    cartItemsContainer.innerHTML = '';
    if (cart.length === 0) {
        cartItemsContainer.innerHTML = '<p class="empty-cart-message">您的购物车是空的。快去探索我们的游戏吧！</p>';
        updateCartTotal();
        return;
    }

    cart.forEach(item => {
        console.log("renderCartItems: 渲染商品:", item.name, "ID:", item.id); // DEBUG R2
        const itemElement = document.createElement('div');
        itemElement.classList.add('cart-item');
        itemElement.dataset.itemId = item.id;

        itemElement.innerHTML = `
                <div class="item-details">
                    <img src="${item.image}" alt="${item.name}" class="item-img">
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
        cartItemsContainer.appendChild(itemElement);
    });

    addCartEventListeners();
    updateCartTotal();
}

那个cart 的 js的逻辑