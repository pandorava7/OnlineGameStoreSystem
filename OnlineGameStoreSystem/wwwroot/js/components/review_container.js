// 自动扩展 textarea 高度
const reviewInput = document.getElementById("reviewInput");
reviewInput.addEventListener("input", function () {
    this.style.height = "auto";
    this.style.height = this.scrollHeight + "px";
});

// 星星评分
let selectedRating = 0;
const stars = document.querySelectorAll("#ratingStars .star");

stars.forEach(star => {
    // 悬浮高亮
    star.addEventListener("mouseover", () => {
        const value = parseInt(star.dataset.star);
        highlightStars(value);
    });

    // 移出恢复为已选状态
    star.addEventListener("mouseleave", () => {
        highlightStars(selectedRating);
    });

    // 点击选中
    star.addEventListener("click", () => {
        selectedRating = parseInt(star.dataset.star);
        highlightStars(selectedRating);
    });
});

// 高亮某个值及之前所有星星
function highlightStars(count) {
    stars.forEach(s => {
        const value = parseInt(s.dataset.star);
        s.classList.toggle("highlight", value <= count);
    });
}

document.getElementById("submitReview").addEventListener("click", () => {
    const text = reviewInput.value.trim();

    if (!selectedRating) {
        alert("请给评分！");
        return;
    }
    if (!text) {
        alert("请写点内容！");
        return;
    }

    fetch('/Review/Add', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify({ selectedRating, text })
    })
        .then(res => res.json())
        .then(data => {
            console.log("评论已提交", data);
        });
});