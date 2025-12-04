window.addEventListener('DOMContentLoaded', () => {
    const container = document.querySelector('.comment-list');
    const cards = container.querySelectorAll('.comment-card');
    const leftBtn = document.querySelector('.comment-left-btn');
    const rightBtn = document.querySelector('.comment-right-btn');
    const dotsContainer = document.querySelector('.comment-dots');
    const dots = dotsContainer.querySelectorAll('.comment-dot');

    const CARD_GAP = 20;            // 核心间距：与 CSS 中的 gap: 20px 保持一致
    const visibleCardsDesktop = 4;  // 桌面端每页显示 4 个
    const visibleCardsMobile = 2;   // 移动端每页显示 2 个

    let totalCards = cards.length;
    let currentPage = 0;
    let totalPages = 0;

    function getVisibleCards() {
        // 根据窗口宽度动态确定可见卡片数 (匹配 CSS @media (max-width: 768px))
        return window.innerWidth <= 768 ? visibleCardsMobile : visibleCardsDesktop;
    }

    function updateSlider() {
        if (cards.length === 0) return;

        const currentVisibleCards = getVisibleCards();
        totalPages = Math.ceil(totalCards / currentVisibleCards);

        // ----------------------------------------------------
        // **核心修正：计算卡片总宽度 (卡片宽度 + 20px 间隙)**
        // offsetWidth 包含卡片自身的 210px 宽度。
        // ----------------------------------------------------
        const cardWidthTotal = cards[0].offsetWidth + CARD_GAP;

        // 防止 currentPage 超出范围（resize 时或卡片数量变化时）
        if (currentPage >= totalPages) {
            currentPage = totalPages - 1;
            if (currentPage < 0) currentPage = 0;
        }

        // 计算移动距离
        // 移动距离 = 当前页数 * 每页可见卡片数 * (卡片实际总宽度)
        const translate = currentPage * currentVisibleCards * cardWidthTotal;
        container.style.transform = `translateX(-${translate}px)`;

        // 更新按钮显示
        leftBtn.style.display = currentPage === 0 ? 'none' : 'block';
        rightBtn.style.display = currentPage >= totalPages - 1 ? 'none' : 'block';

        // 更新 dots 状态
        dots.forEach((dot, index) => {
            dot.classList.toggle('active', index === currentPage);
        });
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

    // Dots 点击事件
    dots.forEach(dot => {
        dot.addEventListener('click', () => {
            currentPage = parseInt(dot.getAttribute('data-index'));
            updateSlider();
        });
    });

    // 窗口 resize 重新计算 (关键：处理响应式和动态宽度)
    window.addEventListener('resize', updateSlider);

    // 初始化
    updateSlider();
});