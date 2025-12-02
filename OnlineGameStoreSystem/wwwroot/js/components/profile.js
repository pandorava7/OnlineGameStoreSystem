// Razor 生成的卡片数量
const gameList = document.getElementById('gameList');
const cards = document.querySelectorAll('.game-card');
const leftBtn = document.getElementById('leftBtn');
const rightBtn = document.getElementById('rightBtn');

const visibleCards = 4; // 每页显示 4 个
const totalCards = cards.length;
let currentPage = 0;
const totalPages = Math.ceil(totalCards / visibleCards);

function updateButtons() {
    leftBtn.style.display = currentPage === 0 ? 'none' : 'block';
    rightBtn.style.display = currentPage === totalPages - 1 ? 'none' : 'block';
}

function updateSlider() {
    if (cards.length === 0) return;
    const cardWidth = cards[0].offsetWidth + 20; // 卡片宽度 + gap
    gameList.style.transform = `translateX(-${currentPage * visibleCards * cardWidth}px)`;
    updateButtons();
}

leftBtn.addEventListener('click', () => {
    if (currentPage > 0) {
        currentPage--;
        updateSlider();
    }
});

rightBtn.addEventListener('click', () => {
    if (currentPage < totalPages - 1) {
        currentPage++;
        updateSlider();
    }
});

updateSlider();
window.addEventListener('resize', updateSlider);