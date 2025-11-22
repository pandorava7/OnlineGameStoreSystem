const isLoggedIn = true; // 假设用户已登录
const userId = 1; // 假设用户ID为1

// 初始化
document.addEventListener("DOMContentLoaded", async () => {
    if (isLoggedIn) {
        await CartAPI.loadCartFromServer(userId);  // 初始化购物车
    }
});
