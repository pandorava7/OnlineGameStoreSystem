function togglePassword(inputId, iconElement) {
    const input = document.getElementById(inputId);

    if (input.type === "password") {
        input.type = "text"; // 显示密码

        // 切换成开眼（无斜杠）
        iconElement.innerHTML = `
        <svg class="eye-icon" width="22" height="22" viewBox="0 0 24 24" fill="none"
             stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
            <circle cx="12" cy="12" r="3"/>
            <path d="M2 12C4.5 7 7.5 5 12 5s7.5 2 10 7c-2.5 5-5.5 7-10 7s-7.5-2-10-7z"/>
        </svg>`;
    } else {
        input.type = "password"; // 隐藏密码

        // 切换成闭眼（带斜杠）
        iconElement.innerHTML = `
        <svg class="eye-icon" width="22" height="22" viewBox="0 0 24 24" fill="none"
             stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
            <path d="M17.94 17.94A10.06 10.06 0 0 1 12 19c-5 0-9-7-9-7a19.45 19.45 0 0 1 5.06-6.06"/>
            <line x1="1" y1="1" x2="23" y2="23"/> <!-- 斜杠 -->
            <circle cx="12" cy="12" r="3"/>
        </svg>`;
    }
}