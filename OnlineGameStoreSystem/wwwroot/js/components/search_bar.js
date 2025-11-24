const input = document.querySelector(".search-input");
const btn = document.querySelector(".search-submit");

function doSearch() {
    const term = input.value.trim();
    if (term === "") {
        return; // 空字符串不执行搜索
    }

    // 跳转到 SearchController 的 Index
    window.location.href = `/Search?term=${encodeURIComponent(term)}`;
    // 或者你写了路由可用 `/Search/Index?term=...`
}

// 点击按钮
btn.addEventListener("click", doSearch);

// 按 Enter 也能搜索
input.addEventListener("keydown", function (e) {
    if (e.key === "Enter") {
        doSearch();
    }
});