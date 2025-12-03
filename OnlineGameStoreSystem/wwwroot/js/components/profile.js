// Razor 生成的卡片数量
const gameList = document.getElementById('gameList');
const cards = document.querySelectorAll('.game-card');
const leftBtn = document.getElementById('leftBtn');
const rightBtn = document.getElementById('rightBtn');
const dotsContainer = document.querySelector('.dots');

const visibleCards = 4; // 每页显示 4 个
const totalCards = cards.length;
let currentPage = 0;
const totalPages = Math.ceil(totalCards / visibleCards);

// 创建 dots
function createDots() {
    dotsContainer.innerHTML = ''; // 清空原有圆点
    for (let i = 0; i < totalPages; i++) {
        const dot = document.createElement('div');
        dot.classList.add('dot');
        if (i === currentPage) dot.classList.add('active');

        // 点击 dot 切换页面
        dot.addEventListener('click', () => {
            currentPage = i;
            updateSlider();
        });

        dotsContainer.appendChild(dot);
    }
}

// 更新 dots 状态
function updateDots() {
    const dots = document.querySelectorAll('.dot');
    dots.forEach((dot, index) => {
        dot.classList.toggle('active', index === currentPage);
    });
}

// 更新左右按钮显示
function updateButtons() {
    leftBtn.style.display = currentPage === 0 ? 'none' : 'block';
    rightBtn.style.display = currentPage === totalPages - 1 ? 'none' : 'block';
}

// 更新滑块位置 + 按钮 + dots
function updateSlider() {
    if (cards.length === 0) return;
    const cardWidth = cards[0].offsetWidth + 20; // 卡片宽度 + gap
    gameList.style.transform = `translateX(-${currentPage * visibleCards * cardWidth}px)`;
    updateButtons();
    updateDots();
}

// 左右按钮事件
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

// 初始化
createDots();
updateSlider();

// 监听窗口大小变化
window.addEventListener('resize', updateSlider);
