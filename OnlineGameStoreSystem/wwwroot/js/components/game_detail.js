let currentSlide = 0;
const carouselInner = document.querySelector('.carousel-inner');
const slides = document.querySelectorAll('.carousel-item');

function showSlide(index) {
    if (index < 0) index = slides.length - 1;
    if (index >= slides.length) index = 0;
    currentSlide = index;
    carouselInner.style.transform = `translateX(-${currentSlide * 100}%)`;
}
function prevSlide() { showSlide(currentSlide - 1); }
function nextSlide() { showSlide(currentSlide + 1); }

// 添加到购物车按钮事件
document.querySelectorAll('.btn-add-to-cart').forEach(btn => {
    btn.addEventListener('click', async () => {
        const gameid = btn.dataset.gameid;
        await addToCart(gameid);
    });
});

// =====================
// 添加到购物车（服务器）
// =====================
async function addToCart(gameId) {
    const res = await fetch(`/Cart/AddItem?gameId=${gameId}`, {
        method: 'POST'
    });

    const result = await res.json();
    if (result.success) {
        showTemporaryMessage("已加入购物车", "success");
    }
}