const input = document.querySelector(".search-input");
const btn = document.querySelector(".search-submit");

function doSearch() {
    const term = input.value.trim();
    // 不再阻止空字符串
    // 跳转到 SearchController 的 Index
    window.location.href = `/Search?term=${encodeURIComponent(term)}`;
}

// 点击按钮
btn.addEventListener("click", doSearch);

// 按 Enter 也能搜索
input.addEventListener("keydown", function (e) {
    if (e.key === "Enter") {
        doSearch();
    }
});
