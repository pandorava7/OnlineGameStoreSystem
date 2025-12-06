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

// confirm game release approve and remove
document.addEventListener('DOMContentLoaded', () => {
    // Approve buttons
    const approveGameBtns = document.querySelectorAll(".approve-game-btn");
    approveGameBtns.forEach(btn => {
        btn.addEventListener('click', () => {
            confirmMessage(() => {
                const gameId = btn.getAttribute('data-game-id');
                document.getElementById('approveGameId').value = gameId;
                document.getElementById('approveGameStatus').value = true; // Approve = true
                document.getElementById('approveGameForm').submit();
            }, "Confirm the release of this game?");
        });
    });

    // Remove buttons
    const removeGameBtns = document.querySelectorAll(".remove-game-btn");
    removeGameBtns.forEach(btn => {
        btn.addEventListener('click', () => {
            confirmMessage(() => {
                const gameId = btn.getAttribute('data-game-id');
                document.getElementById('approveGameId').value = gameId;
                document.getElementById('approveGameStatus').value = false; // ❗ Remove = false
                document.getElementById('approveGameForm').submit();
            }, "Confirm the removal of this game?");
        });
    });
});
