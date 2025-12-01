// JS说明：
// carousel = 900px
// carousel-item = 1200px + 100px margin-right
// 因此每次移动的宽度是900px


// 轮播图主要元素
const track = document.querySelector('.carousel-track');
const slides = Array.from(document.querySelectorAll('.carousel-item'));
const prevBtn = document.querySelector('.prev');
const nextBtn = document.querySelector('.next');
const dotsContainer = document.querySelector('.carousel-dots');
let dots = [];

let currentIndex = 0;
// 为什么是1100px？见上方注释
const slideWidth = 1100;
// 自动播放计时器变量
let autoPlayTimer = null;
// 自动播放间隔时间（4000ms = 4秒）
const autoPlayInterval = 5000;
// 获取总幻灯片数
const totalSlides = slides.length;

// 新增：动态生成 Dots 的函数
function generateDots() {
    // 清空容器（防止重复生成）
    dotsContainer.innerHTML = '';

    // 根据 slide 数量循环生成
    slides.forEach((_, index) => {
        // 创建元素 (你可以根据 CSS 实际情况改成 button 或 span)
        const dot = document.createElement('div');
        dot.classList.add('dot');

        // 如果是第一个，默认激活
        if (index === 0) {
            dot.classList.add('active');
        }

        // 【重要】直接在这里绑定点击事件，比在底部遍历更高效
        dot.addEventListener('click', () => {
            moveToSlide(index);
        });

        // 添加到容器
        dotsContainer.appendChild(dot);
    });

    // 【重要】更新 dots 变量，让后续的 updateDots 函数能找到它们
    dots = Array.from(document.querySelectorAll('.dot'));
}

// --- 核心功能：移动到某个 slide ---
function moveToSlide(index) {
    // 1. 限制 index 范围：确保 index 在 [0, totalSlides - 1] 之间
    if (index < 0 || index >= totalSlides) {
        return; // 如果超出范围，则不执行任何操作
    }

    currentIndex = index;

    // 2. 移动 track
    track.style.transition = "transform 0.5s ease";
    const offset = -currentIndex * slideWidth;
    track.style.transform = `translateX(${offset}px)`;

    // 3. 更新指示点
    updateDots();

    // 4. 【新增功能】更新按钮状态
    //updateButtonVisibility();

    // 5. 重置进度条动画
    resetProgressBarAnimation(slides[currentIndex]);

    // 6. 重置自动播放计时器 (注意: 既然是有限端点模式，通常会禁用自动播放)
    // 如果需要保留自动播放，请保持 resetAutoPlay() 调用。
    // 如果不需自动播放，请注释掉下面这行
     resetAutoPlay(); 
}

// --- 更新下方小点 ---
function updateDots() {
    dots.forEach((dot, i) => {
        dot.classList.toggle('active', i === currentIndex);
    });
}

// --- 【新增功能】更新按钮显示/隐藏状态 ---
function updateButtonVisibility() {
    // 如果在第一张 (index = 0)，隐藏左按钮
    if (currentIndex === 0) {
        prevBtn.style.visibility = 'hidden';
    } else {
        prevBtn.style.visibility = 'visible';
    }

    // 如果在最后一张 (index = totalSlides - 1)，隐藏右按钮
    if (currentIndex === totalSlides - 1) {
        nextBtn.style.visibility = 'hidden';
    } else {
        nextBtn.style.visibility = 'visible';
    }
}

// --- 自动轮播启动 ---
function startAutoPlay() {
    // 使用 setTimeout 递归调用，而非 setInterval，
    // 以便在手动操作后能立即且干净地重置计时器。
    autoPlayTimer = setTimeout(function tick() {
        let index = currentIndex + 1;
        // 循环到第一张
        if (index >= slides.length) index = 0;

        // 移动到下一个 slide，并在移动完成后再次启动计时器
        // 移除 transition，防止在循环切换时出现“闪烁”
        track.style.transition = "none";

        // 立即切换到目标位置（实现无缝循环的障眼法）
        const offset = -index * slideWidth;
        track.style.transform = `translateX(${offset}px)`;

        // 更新 index 和 dots
        currentIndex = index;
        updateDots();

        // 重新设置计时器
        autoPlayTimer = setTimeout(tick, autoPlayInterval);

    }, autoPlayInterval);
}


// --- 进度条控制功能 ---
function resetProgressBarAnimation(slide) {
    const progressBar = slide.querySelector('.progress-bar');
    if (!progressBar) return;

    // 停止动画并重置
    progressBar.classList.remove('active');

    // 强制浏览器重绘
    void progressBar.offsetWidth;

    // 触发动画
    progressBar.classList.add('active');
}


// --- 自动轮播重置 ---
function resetAutoPlay() {
    clearTimeout(autoPlayTimer);
    // 在手动操作后，确保新的自动播放是从当前 slide 开始
    // 并且重新应用过渡效果，以便下一次自动播放是平滑的
    track.style.transition = "transform 0.5s ease";

    // 重启自动播放
    // **注意:** 为了保持平滑，我们不应该调用 startAutoPlay()，而应该只清除并重新计时。
    // 但是考虑到无缝循环的复杂性，我们简化处理，只依赖 moveToSlide。

    // 重新启动一个延时的自动播放，让用户有时间查看
    autoPlayTimer = setTimeout(() => {
        let index = currentIndex + 1;
        if (index >= slides.length) index = 0;
        moveToSlide(index);
    }, autoPlayInterval);
}


// --- 事件监听器 ---

// 左键
prevBtn.addEventListener('click', () => {
    let index = currentIndex - 1;
    if (index < 0) index = slides.length - 1;
    moveToSlide(index);
});

// 右键
nextBtn.addEventListener('click', () => {
    let index = currentIndex + 1;
    if (index >= slides.length) index = 0;
    moveToSlide(index);
});


// --- 初始启动 ---
// 首次进入页面时，确保显示第一个 slide 并开始自动播放
generateDots();
moveToSlide(0);
resetAutoPlay(); // 使用 resetAutoPlay 启动初始计时器

// --- 新增功能：缩略图悬浮淡入淡出切换主图 ---
function enableThumbnailHover() {

    const items = document.querySelectorAll('.carousel-item');

    items.forEach(item => {
        const topImg = item.querySelector('.main-img-top');
        const bottomImg = item.querySelector('.main-img-bottom');
        const thumbs = item.querySelectorAll('.thumbs img');

        if (!topImg || !bottomImg || thumbs.length === 0) return;

        const originalSrc = topImg.getAttribute('src');

        thumbs.forEach(thumb => {

            // 进入：hover 图淡入、原图淡出
            thumb.addEventListener('mouseenter', () => {
                const hoverSrc = thumb.getAttribute('src');

                // 底层准备显示 hover 图
                bottomImg.setAttribute('src', hoverSrc);

                // 交叉淡化
                topImg.style.opacity = 0;
                bottomImg.style.opacity = 1;
            });


            // 离开：原图淡入、hover 图淡出
            thumb.addEventListener('mouseleave', () => {

                // 将原图放到底层（准备淡入）
                //bottomImg.setAttribute('src', originalSrc);

                // 交叉淡化（反向）
                topImg.style.opacity = 1;     // 原图淡入
                bottomImg.style.opacity = 0;  // hover 图淡出
            });
        });
    });
}

enableThumbnailHover();

document.addEventListener('DOMContentLoaded', () => {
    const thumbs = document.querySelectorAll('.thumbs'); // 正确方法

    thumbs.forEach(t => {
        t.addEventListener('wheel', function (e) {
            e.preventDefault();       // 阻止页面上下滚动
            t.scrollLeft += e.deltaY; // 上下滚动转为横向滚动
            console.log(e.deltaY);
        });
    });
});

