function clearCartUI() {
    // 1️⃣ 清空左侧商品列表
    const cartContainer = document.querySelector(".cart-item-container");
    if (cartContainer) {
        cartContainer.innerHTML = "";
    }

    // 2️⃣ 清空支付说明
    const paymentDesc = document.querySelector(".payment-description");
    if (paymentDesc) {
        paymentDesc.innerHTML = "";
    }

    // 3️⃣ 重置统计数字
    const cartTotal = document.getElementById("cart-total");
    const cartTotalFinal = document.getElementById("cart-total-final");
    const summarySubtotal = document.getElementById("summary-subtotal");

    if (cartTotal) cartTotal.innerText = "0.00";
    if (cartTotalFinal) cartTotalFinal.innerText = "0.00";
    if (summarySubtotal) summarySubtotal.innerText = "RM 0.00";
}



document.getElementById("proceed-payment-btn").addEventListener("click", async function () {

    const selectedMethod = parseInt(this.dataset.selectedMethod);
    //const selectedMethod = this.dataset.selectedMethod;
    console.log(selectedMethod)

    if (selectedMethod === -1) {
        alert("Please select a payment method");
        return;
    }

    // 打开 loading 弹窗
    showLoadingModal();

    // 调用 MVC 后端
    const res = await fetch('/Payment/ProcessPayment', {
        method: 'POST',
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({
            selectedPaymentMethod: selectedMethod
        })
    });

    const data = await res.json();

    // 显示结果
    if (data.success) {
        showSuccessModal(data);
        clearCartUI();
    } else {
        showFailureModal(data.message);
    }
});


/* -----------------------
   弹窗控制逻辑
------------------------ */
function showLoadingModal() {
    const modal = document.getElementById("payment-modal");
    modal.classList.remove("hidden");

    document.getElementById("modal-icon").innerHTML = `<div class="loader"></div>`;
    document.getElementById("modal-title").innerText = "Payment Process...";
    document.getElementById("modal-desc").innerText = "Please wait while we process your transaction.";

    document.getElementById("modal-actions").classList.add("hidden");
    document.getElementById("modal-actions").innerHTML = "";
}

function showSuccessModal(data) {
    document.getElementById("modal-icon").innerHTML =
        `<i class="fas fa-check-circle success-icon"></i>`;

    document.getElementById("modal-title").innerText = "Your payment is successfully!";
    document.getElementById("modal-desc").innerText = "You can check your receipt via email.";

    const actions = document.getElementById("modal-actions");
    actions.classList.remove("hidden");

    actions.innerHTML = `
        <button class="primary-btn" onclick="location.href='/Library'">
            Play Now
        </button>

        <a href="/Invoice/${data.paymentId}" target="_blank" class="secondary-btn button">
            Invoice
        </a>

        <button class="secondary-btn" onclick="location.href='/'">
            Home
        </button>
    `;
}

function showFailureModal(message) {
    document.getElementById("modal-icon").innerHTML =
        `<i class="fas fa-times-circle error-icon"></i>`;

    document.getElementById("modal-title").innerText = "Payment Failed";
    document.getElementById("modal-desc").innerText = message || "An unexpected error occurred.";

    const actions = document.getElementById("modal-actions");
    actions.classList.remove("hidden");

    actions.innerHTML = `
        <button class="secondary-btn" onclick="location.reload()">
            Try Again
        </button>
    `;
}
