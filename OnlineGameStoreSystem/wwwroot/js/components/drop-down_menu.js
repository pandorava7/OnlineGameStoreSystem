document.addEventListener("DOMContentLoaded", function () {
    const trigger = document.getElementById("userMenuButton");
    const menu = document.getElementById("userDropdown");

    if (!trigger || !menu) return;

    trigger.addEventListener("click", function (e) {
        e.stopPropagation();
        menu.classList.toggle("open");
    });

    document.addEventListener("click", function (e) {
        if (!menu.contains(e.target) && !trigger.contains(e.target)) {
            menu.classList.remove("open");
        }
    });
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

