const searchBtn = document.querySelector('.nav-right .icon-btn[aria-label="Search"]');
const searchBar = document.querySelector('.search-bar-container');

searchBtn.addEventListener('click', () => {
    searchBar.classList.toggle('active');
});
