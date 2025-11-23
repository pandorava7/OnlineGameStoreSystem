document.addEventListener("DOMContentLoaded", () => {
    const textareas = document.querySelectorAll("textarea");

    textareas.forEach(textarea => {
        // 初始化高度
        autoResize(textarea);

        // 监听输入事件
        textarea.addEventListener("input", () => autoResize(textarea));
    });

    function autoResize(el) {
        el.style.height = "auto"; // 重置高度
        el.style.height = el.scrollHeight + "px"; // 设置为内容高度
    }
});
