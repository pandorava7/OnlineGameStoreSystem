// prettier-ignore
const itemSizes = [
    "2em", "3em", "1.6em", "4em", "3.2em",
    "3em", "4.5em", "1em", "3.5em", "2.8em",
];
const items = document.querySelectorAll(".item");
for (let i = 0; i < items.length; i++) {
    items[i].style.blockSize = itemSizes[i];
}


document.addEventListener("DOMContentLoaded", () => {
    const container = document.querySelector(".community-container");

    const resizeAll = () => {
        const rowGap = parseInt(window.getComputedStyle(container).getPropertyValue('gap'));
    const allItems = container.querySelectorAll('.post-card');

        allItems.forEach(item => {
        item.style.gridRowEnd = `span ${Math.ceil((item.getBoundingClientRect().height + rowGap) / 10)}`;
        });
    };

    // 执行一次
    resizeAll();

    // 图片加载完成后修复高度
    window.addEventListener("load", resizeAll);
    window.addEventListener("resize", resizeAll);
});
