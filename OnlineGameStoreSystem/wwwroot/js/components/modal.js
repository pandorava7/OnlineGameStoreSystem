const Modal = (() => {
    const modal = document.getElementById("customModal");
    const messageEl = document.getElementById("modalMessage");
    const buttonsEl = document.getElementById("modalButtons");

    function show({ message = "", buttons = [] }) {
        // 清空旧内容
        messageEl.textContent = message;
        buttonsEl.innerHTML = "";

        // 创建按钮
        buttons.forEach(btn => {
            const b = document.createElement("button");
            b.textContent = btn.text || "Button";
            b.className = btn.type || "primary";
            b.addEventListener("click", () => {
                if (btn.onClick) btn.onClick();
                hide();
            });
            buttonsEl.appendChild(b);
        });

        modal.classList.remove("hidden");
    }

    function hide() {
        modal.classList.add("hidden");
    }

    // 点击遮罩也可关闭
    modal.querySelector(".modal-overlay").addEventListener("click", hide);

    return { show, hide };
})();
