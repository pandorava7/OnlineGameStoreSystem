const searchBtn = document.querySelector('.nav-right .icon-btn[aria-label="Search"]');
const searchBar = document.querySelector('.search-bar-container');

searchBtn.addEventListener('click', () => {
    searchBar.classList.toggle('active');
});
function closeAlert() {
    // 找到最外层的容器并隐藏它
    var alertContainer = document.getElementById("temp-alert-container");
    if (alertContainer) {
        alertContainer.style.display = 'none';
    }
}