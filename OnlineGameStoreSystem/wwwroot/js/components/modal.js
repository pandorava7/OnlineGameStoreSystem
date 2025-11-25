const Modal = (() => {
    const modal = document.getElementById("customModal");
    const messageEl = document.getElementById("modalMessage");
    const buttonsEl = document.getElementById("modalButtons");
    const overlay = modal.querySelector(".modal-overlay");
    const content = modal.querySelector(".modal-content");

    function show({ message = "", buttons = [] }) {
        messageEl.textContent = message;
        buttonsEl.innerHTML = "";

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

        // 先显示 modal
        modal.classList.remove("hidden");

        // 强制浏览器渲染一次（避免跳帧）
        void content.offsetWidth;

        // 入场动画
        overlay.classList.add("fade-in");
        content.classList.remove("hide");
        content.classList.add("show");
    }

    function hide() {
        // 退场动画
        overlay.classList.remove("fade-in");
        content.classList.remove("show");
        content.classList.add("hide");

        // 动画结束后再真正隐藏
        content.addEventListener("transitionend", function handler(e) {
            if (e.propertyName === "opacity") {
                modal.classList.add("hidden");
                content.removeEventListener("transitionend", handler);
            }
        });
    }

    overlay.addEventListener("click", hide);

    return { show, hide };
})();
