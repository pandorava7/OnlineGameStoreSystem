async function addToCart(gameId) {
    confirmMessage(async () => {
        try {
            const res = await fetch(`/Cart/AddItem?gameId=${gameId}`, {
                method: 'POST'
            });
            const result = await res.json();

            if (result.success) {
                showTemporaryMessage("Added to your cart", "success");
                await CartAPI.loadCartFromServer(); // 更新购物车徽章
            } else {
                showTemporaryMessage(result.message, "error");
            }
        } catch (err) {
            console.error(err);
            showTemporaryMessage("Network error", "error");
        }
    }, "Confirm add to your cart?");
}

// 从 Wishlist 删除
async function removeFromWishlist(wishItemId, btn) {
    confirmMessage(async () => {
        try {
            const res = await fetch(`/Wishlist/RemoveItem?wishItemId=${wishItemId}`, {
                method: 'POST'
            });
            const result = await res.json();

            if (result.success) {
                showTemporaryMessage("Removed from wishlist", "success");

                // 从页面中删除该条目
                const itemDiv = btn.closest('.wishlist-item');
                if (itemDiv) itemDiv.remove();
            } else {
                showTemporaryMessage("Failed to remove item", "error");
            }
        } catch (err) {
            console.error(err);
            showTemporaryMessage("Network error", "error");
        }
    }, "Confirm remove from your wishlist?");
}
