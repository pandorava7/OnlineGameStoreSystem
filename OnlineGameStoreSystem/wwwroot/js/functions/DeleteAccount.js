document.addEventListener('DOMContentLoaded', () => {
    const deleteBtn = document.getElementById('deleteAccountBtn');
    const form = document.getElementById('deleteAccountForm');

    deleteBtn.addEventListener('click', (e) => {
        e.preventDefault();

        Modal.show({
            message: "Are you sure you want to delete your account?\n           (This action cannot be undone)",
            buttons: [
                {
                    text: "Cancel",
                    type: "secondary-btn"
                },
                {
                    text: "Delete",
                    type: "primary-btn",
                    onClick: () => {
                        // 直接提交隐藏表单 → TempData 生效
                        form.submit();
                    }
                }
            ]
        });
    });
});
