document.addEventListener('DOMContentLoaded', () => {
    const delUserBtn = document.getElementById('delUserBtn');
    delUserBtn.addEventListener('click', () => {
        confirmMessage(() => {
            const userId = delUserBtn.getAttribute('data-user-id');
            document.getElementById('deleteUserId').value = userId;
            document.getElementById('deleteUserForm').submit();
        },
        "Confirm to delete this user?")
    });
});