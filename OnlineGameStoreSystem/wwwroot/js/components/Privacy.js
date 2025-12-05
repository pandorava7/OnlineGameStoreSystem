document.addEventListener('DOMContentLoaded', () => {
    const toggle = document.getElementById('toggleSwitch');
    const tokenInput = document.querySelector('input[name="__RequestVerificationToken"]');
    if (!toggle || !tokenInput) return;

    const token = tokenInput.value;
    let isPublic = toggle.dataset.public === 'True';

    // 更新开关 UI
    const updateUI = () => {
        const handle = toggle.querySelector('.switch-handle');
        if (isPublic) {
            handle.style.transform = 'translateX(0px)';
            toggle.style.backgroundColor = '#ccc'; // 公共 = 灰色
        } else {
            handle.style.transform = 'translateX(26px)';
            toggle.style.backgroundColor = '#4CAF50'; // 私密 = 绿色
        }
    };

    updateUI();

    // 封装确认弹窗
    const confirmPrivacyChange = (onConfirm) => {
        if (typeof Modal.close === 'function') Modal.close();

        Modal.show({
            message: "Are you sure you want to make your profile private?\n(Other users will not be able to see your profile.)",
            buttons: [
                {
                    text: "Cancel",
                    type: "secondary-btn",
                    onClick: () => console.log("Cancel clicked")
                },
                {
                    text: "Confirm",
                    type: "primary-btn",
                    onClick: () => {
                        if (typeof onConfirm === "function") onConfirm();
                    }
                }
            ]
        });
    };

    // 点击开关
    toggle.addEventListener('click', () => {
        confirmPrivacyChange(async () => {
            // 切换状态（前端 UI）
            isPublic = !isPublic;
            updateUI();

            try {
                const res = await fetch('/Profile/UpdatePrivacy', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json',
                        'RequestVerificationToken': token
                    },
                    body: JSON.stringify({ isPublic })
                });

                if (!res.ok) throw new Error("Update failed");

                // 成功后刷新页面
                location.reload();

            } catch (err) {
                console.error(err);
                alert("Update failed, please try again.");
                isPublic = !isPublic; // 回滚状态
                updateUI();
            }
        });
    });
});
