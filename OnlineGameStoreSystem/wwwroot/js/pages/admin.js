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

// confirm refund approve and reject
document.addEventListener('DOMContentLoaded', () => {

    // Approve refund buttons
    const approveRefundBtns = document.querySelectorAll(".approve-refund-btn");
    approveRefundBtns.forEach(btn => {
        btn.addEventListener('click', () => {
            confirmMessage(() => {
                const purchaseId = btn.getAttribute('data-purchase-id');
                document.getElementById('refundPurchaseId').value = purchaseId;
                document.getElementById('refundApproveStatus').value = true; // Approve = true
                document.getElementById('refundActionForm').submit();
            }, "Confirm approving this refund request?");
        });
    });

    // Reject refund buttons
    const rejectRefundBtns = document.querySelectorAll(".reject-refund-btn");
    rejectRefundBtns.forEach(btn => {
        btn.addEventListener('click', () => {
            confirmMessage(() => {
                const purchaseId = btn.getAttribute('data-purchase-id');
                document.getElementById('refundPurchaseId').value = purchaseId;
                document.getElementById('refundApproveStatus').value = false; // Reject = false
                document.getElementById('refundActionForm').submit();
            }, "Confirm rejecting this refund request?");
        });
    });
});

document.addEventListener('DOMContentLoaded', () => {
    // Remove buttons
    const removePostBtns = document.querySelectorAll(".remove-post-btn");
    removePostBtns.forEach(btn => {
        btn.addEventListener('click', () => {
            confirmMessage(() => {
                const postId = btn.getAttribute('data-post-id');
                document.getElementById('removePostId').value = postId;
                document.getElementById('removePostForm').submit();
            }, "Confirm the removal of this post?");
        });
    });

    // Remove buttons
    const removeCommentBtns = document.querySelectorAll(".remove-comment-btn");
    removeCommentBtns.forEach(btn => {
        btn.addEventListener('click', () => {
            confirmMessage(() => {
                const commentId = btn.getAttribute('data-comment-id');
                document.getElementById('removeCommentId').value = commentId;
                document.getElementById('removeCommentForm').submit();
            }, "Confirm the removal of this comment?");
        });
    });

    //// Remove buttons
    const removeReviewBtns = document.querySelectorAll(".remove-review-btn");
    removeReviewBtns.forEach(btn => {
        btn.addEventListener('click', () => {
            confirmMessage(() => {
                const reviewId = btn.getAttribute('data-review-id');
                document.getElementById('removeReviewId').value = reviewId;
                document.getElementById('removeReviewForm').submit();
            }, "Confirm the removal of this review?");
        });
    });
});