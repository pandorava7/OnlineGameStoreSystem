const searchBtn = document.querySelector('.nav-right .icon-btn[aria-label="Search"]');
const searchBar = document.querySelector('.search-bar-container');
const searchInput = searchBar.querySelector('input'); // 获取输入框

searchBtn.addEventListener('click', (e) => {
    e.stopPropagation();
    searchBar.classList.toggle('active');

    if (searchBar.classList.contains('active')) {
        // 展开时自动 focus
        searchInput.focus();
    }
});

// 点击页面其他地方关闭搜索栏
document.addEventListener('click', () => {
    searchBar.classList.remove('active');
});

// 防止点击搜索栏自身关闭
searchBar.addEventListener('click', (e) => {
    e.stopPropagation();
});
