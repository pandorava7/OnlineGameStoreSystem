document.addEventListener('DOMContentLoaded', () => {
    const toggle = document.getElementById('toggleSwitch');
    const tokenInput = document.querySelector('input[name="__RequestVerificationToken"]');
    if (!toggle || !tokenInput) return;

    const token = tokenInput.value;

    // 读取数据库状态
    let isPublic = toggle.dataset.public === 'True';

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

    updateUI(); // 初始化

    toggle.addEventListener('click', () => {
        const confirmChange = confirm("Are you sure you want to change your profile privacy settings?");
        if (!confirmChange) return;

        isPublic = !isPublic; // 切换状态
        updateUI();

        fetch('/Profile/UpdatePrivacy', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': token
            },
            body: JSON.stringify({ isPublic })
        })
            .catch(() => {
                alert("Update failed");
                isPublic = !isPublic; // 回滚
                updateUI();
            });
    });
});
