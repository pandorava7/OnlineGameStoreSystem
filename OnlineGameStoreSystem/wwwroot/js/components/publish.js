window.addEventListener('DOMContentLoaded', () => {
    const container = document.querySelector('.published-list');
    const cards = container.querySelectorAll('.published-game-card'); // FIXED class name
    const leftBtn = document.querySelector('.published-left');
    const rightBtn = document.querySelector('.published-right');
    const dotsContainer = document.querySelector('.published-dots');
    const dots = dotsContainer ? dotsContainer.querySelectorAll('.published-dot') : [];

    const CARD_GAP = 20;
    const visibleCardsDesktop = 4;
    const visibleCardsMobile = 2;

    let totalCards = cards.length;
    let currentPage = 0;
    let totalPages = 0;

    function getVisibleCards() {
        return window.innerWidth <= 768 ? visibleCardsMobile : visibleCardsDesktop;
    }

    function updateSlider() {
        if (cards.length === 0) return;

        const currentVisibleCards = getVisibleCards();
        totalPages = Math.ceil(totalCards / currentVisibleCards);

        const cardWidthTotal = cards[0].offsetWidth + CARD_GAP;

        if (currentPage >= totalPages) currentPage = totalPages - 1;
        if (currentPage < 0) currentPage = 0;

        const translate = currentPage * currentVisibleCards * cardWidthTotal;
        container.style.transform = `translateX(-${translate}px)`;

        leftBtn.style.display = currentPage === 0 ? 'none' : 'block';
        rightBtn.style.display = currentPage >= totalPages - 1 ? 'none' : 'block';

        dots.forEach((dot, index) => {
            dot.classList.toggle('active', index === currentPage);
        });
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

    dots.forEach(dot => {
        dot.addEventListener('click', () => {
            currentPage = parseInt(dot.getAttribute('data-index'));
            updateSlider();
        });
    });

    window.addEventListener('resize', updateSlider);

    updateSlider();
});
