window.addEventListener('DOMContentLoaded', () => {
    const wrapper = document.querySelector('.comment-wrapper');
    const list = document.querySelector('.comment-list');
    const cards = list.querySelectorAll('.comment-card');
    const leftBtn = document.querySelector('.comment-left-btn');
    const rightBtn = document.querySelector('.comment-right-btn');
    const dots = document.querySelectorAll('.comment-dot');

    const cardWidth = cards[0].offsetWidth;
    const gap = parseInt(getComputedStyle(cards[0]).marginRight) || 20; // 与 CSS gap 一致
    const cardsPerPage = 4;
    const totalCards = cards.length;
    const totalPages = Math.ceil(totalCards / cardsPerPage);
    let currentIndex = 0;

    function updateSlider() {
        let translate = 0;

        if (currentIndex === totalPages - 1) {
            // 最后一页只显示剩余卡片
            const remaining = totalCards - cardsPerPage * (totalPages - 1);
            translate = (cardsPerPage * (totalPages - 1)) * (cardWidth + gap);
        } else {
            translate = currentIndex * cardsPerPage * (cardWidth + gap);
        }

        list.style.transform = `translateX(-${translate}px)`;

        // 更新 dots
        dots.forEach(dot => dot.classList.remove('active'));
        if (dots[currentIndex]) dots[currentIndex].classList.add('active');

        // 按钮显示逻辑
        leftBtn.style.display = currentIndex === 0 ? 'none' : 'block';
        rightBtn.style.display = currentIndex === totalPages - 1 ? 'none' : 'block';
    }

    leftBtn.addEventListener('click', () => {
        currentIndex = Math.max(0, currentIndex - 1);
        updateSlider();
    });

    rightBtn.addEventListener('click', () => {
        currentIndex = Math.min(totalPages - 1, currentIndex + 1);
        updateSlider();
    });

    dots.forEach(dot => {
        dot.addEventListener('click', () => {
            currentIndex = parseInt(dot.getAttribute('data-index'));
            updateSlider();
        });
    });

    updateSlider(); // 初始化
});
