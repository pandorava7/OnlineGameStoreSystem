function toggleUserDropdown() {
    var menu = document.getElementById("userDropdown");
    menu.style.display = menu.style.display === "block" ? "none" : "block";
}

document.addEventListener("click", function (e) {
    var menu = document.getElementById("userDropdown");
    var userMenu = document.querySelector(".user-menu");

    if (!userMenu.contains(e.target)) {
        menu.style.display = "none";
    }
});

function logout(e) {
    e.preventDefault(); // 阻止默认跳转

    // 创建隐藏表单
    const form = document.createElement('form');
    form.method = 'POST';
    form.action = '/Account/Logout';
    document.body.appendChild(form);
    form.submit();
}

