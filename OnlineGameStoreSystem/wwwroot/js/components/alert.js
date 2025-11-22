// 显示临时消息
function showTemporaryMessage(message, type = "info") {
    const container = document.getElementById("flash-message-container");
    if (!container) return;

    const msg = document.createElement("div");
    msg.classList.add("flash-message");

    // 根据类型选择颜色
    switch (type) {
        case "success":
            msg.classList.add("flash-success");
            break;
        case "error":
            msg.classList.add("flash-error");
            break;
        case "warning":
            msg.classList.add("flash-warning");
            break;
        default:
            msg.classList.add("flash-info");
    }

    msg.innerText = message;

    container.appendChild(msg);

    // 3秒后移除（和动画时间对应）
    setTimeout(() => {
        msg.remove();
    }, 3000);
}

//// 页面加载后让 alert 动画显示，并在 3 秒后自动消失
//document.addEventListener("DOMContentLoaded", () => {
//    const container = document.getElementById("temp-alert-container");
//    if (!container) return;

//    const alert = container.querySelector(".alert");
//    if (!alert) return;

//    // 显示动画
//    setTimeout(() => {
//        alert.classList.add("show-alert");
//    }, 50); // 延迟保证 transition 生效

//    // 自动隐藏
//    setTimeout(() => {
//        closeAlert();
//    }, 3000);
//});

//// 手动关闭
//function closeAlert() {
//    const container = document.getElementById("temp-alert-container");
//    if (!container) return;

//    const alert = container.querySelector(".alert");
//    if (!alert) return;

//    // 添加隐藏动画
//    alert.classList.remove("show-alert");
//    alert.classList.add("hide-alert");

//    // 动画结束后移除 DOM
//    setTimeout(() => {
//        container.remove();
//    }, 500); // 与 CSS transition 时间一致
//}