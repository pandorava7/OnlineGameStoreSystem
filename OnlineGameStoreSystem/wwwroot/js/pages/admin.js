function imagesLoaded(container, callback) {
    const images = container.getElementsByTagName("img");
    console.log(images);
    let loadedCount = 0;
    const total = images.length;

    if (total === 0) {
        callback();
        return;
    }

    const onImageLoad = () => {
        loadedCount++;
        if (loadedCount === total) callback();
    };

    for (let img of images) {
        if (img.complete) {
            onImageLoad();
        } else {
            img.onload = onImageLoad;
            img.onerror = onImageLoad;
        }
    }
}

function masonryLayout(containerSelector, itemSelector, columnCount) {
    const container = document.querySelector(containerSelector);
    const items = Array.from(container.querySelectorAll(itemSelector));

    function layout() {
        const containerWidth = container.clientWidth;
        const columnHeights = Array(columnCount).fill(0);

        const gap = 30;

        const columnWidth = (containerWidth - gap * (columnCount - 1)) / columnCount;

        items.forEach(item => {
            item.style.width = columnWidth + "px";

            const minColumn = columnHeights.indexOf(Math.min(...columnHeights));

            item.style.left = minColumn * (columnWidth + gap) + "px";
            item.style.top = columnHeights[minColumn] + "px";

            columnHeights[minColumn] += item.offsetHeight + gap;
        });

        container.style.height = Math.max(...columnHeights) + "px";
    }

    imagesLoaded(container, layout);

    window.addEventListener("resize", () => {
        setTimeout(layout, 200);
    });
}

// ⬇ 初始化（自动响应式列数）
function initMasonry() {
    let cols = 3;
    if (window.innerWidth < 900) cols = 2;
    if (window.innerWidth < 600) cols = 1;

    masonryLayout(".masonry-container", ".masonry-item", cols);
}

initMasonry();
window.addEventListener("resize", initMasonry);
