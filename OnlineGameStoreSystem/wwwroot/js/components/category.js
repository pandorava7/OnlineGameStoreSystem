document.addEventListener("DOMContentLoaded", function () {
    const categories = document.querySelectorAll(".category-block");
    const gap = 25; // 假设 CSS 中的 gap 是 25px

    categories.forEach(block => {
        // 1. 获取 DOM 元素
        const gameListWrapper = block.querySelector(".game-list-wrapper");
        const gameList = block.querySelector(".game-list");
        const leftBtn = block.querySelector(".left-btn");
        const rightBtn = block.querySelector(".right-btn");
        const dotsContainer = block.querySelector(".dots");
        const cards = block.querySelectorAll(".game-card");

        // 2. 边界检查：如果没有游戏卡片，直接跳过或隐藏该区域
        if (cards.length === 0) {
            block.style.display = 'none'; // 可选：如果没有游戏，隐藏整个块
            return;
        }

        // 3. 计算参数
        // 获取单个卡片宽度（包含 CSS gap）
        // 注意：这里假设 gap 是 20px，也可以通过 getComputedStyle 动态获取
        const cardStyle = window.getComputedStyle(gameList);
        //const gap = parseFloat(cardStyle.columnGap || cardStyle.gap || "20px");
        const cardWidth = cards[0].offsetWidth + gap;

        // 计算每页能显示多少个 (wrapper 宽度 / 单个卡片总宽)
        // 你的容器是 1200px，卡片约 240px (220+20)，结果应该是 5
        const wrapperWidth = gameListWrapper.offsetWidth;
        const cardsPerPage = Math.floor(wrapperWidth / cards[0].offsetWidth); // 近似计算，或者直接设为 5

        // 计算总页数
        const totalPages = Math.ceil(cards.length / cardsPerPage);
        let currentPage = 0;

        // 4. 动态生成 Dots (核心修改)
        function initDots() {
            dotsContainer.innerHTML = ""; // 清空原有内容

            // 只有当大于 1 页时才显示圆点
            if (totalPages > 1) {
                for (let i = 0; i < totalPages; i++) {
                    const dot = document.createElement("span");
                    dot.classList.add("dot");
                    if (i === 0) dot.classList.add("active");

                    // 绑定点击事件
                    dot.addEventListener("click", () => {
                        currentPage = i;
                        updateSlider();
                    });

                    dotsContainer.appendChild(dot);
                }
            } else {
                // 如果只有1页，隐藏导航按钮
                leftBtn.style.display = "none";
                rightBtn.style.display = "none";
            }
        }

        // 5. 更新滑块视图状态
        function updateSlider() {
            // 计算移动距离
            const moveDistance = currentPage * cardWidth * cardsPerPage;
            gameList.style.transform = `translateX(-${moveDistance}px)`;

            // 更新 Dots 高亮
            const allDots = dotsContainer.querySelectorAll(".dot");
            allDots.forEach((dot, index) => {
                dot.classList.toggle("active", index === currentPage);
            });

            // 更新按钮显隐 (第一页不显示左，最后一页不显示右)
            if (totalPages > 1) {
                leftBtn.style.display = currentPage === 0 ? "none" : "block";
                rightBtn.style.display = currentPage >= totalPages - 1 ? "none" : "block";
            }
        }

        // 6. 绑定按钮事件
        leftBtn.addEventListener("click", () => {
            if (currentPage > 0) {
                currentPage--;
                updateSlider();
            }
        });

        rightBtn.addEventListener("click", () => {
            if (currentPage < totalPages - 1) {
                currentPage++;
                updateSlider();
            }
        });

        // 7. 初始化
        initDots();
        // 初始状态下检查一次按钮显示情况
        updateSlider();
    });
});