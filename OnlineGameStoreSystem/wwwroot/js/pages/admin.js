document.addEventListener('DOMContentLoaded', () => {
    const delUserBtn = document.getElementById('delUserBtn');
    if (!delUserBtn) return;
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
    if (!approveGameBtns) return;
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
    if (!removeGameBtns) return;
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

    // Restore buttons
    const restoreGameBtns = document.querySelectorAll(".restore-game-btn");
    if (!restoreGameBtns) return;
    restoreGameBtns.forEach(btn => {
        btn.addEventListener('click', () => {
            confirmMessage(() => {
                const gameId = btn.getAttribute('data-game-id');
                document.getElementById('restoreGameId').value = gameId;
                document.getElementById('restoreGameForm').submit();
            }, "Restore this game data?");
        });
    });
});

// confirm refund approve and reject
document.addEventListener('DOMContentLoaded', () => {

    // Approve refund buttons
    const approveRefundBtns = document.querySelectorAll(".approve-refund-btn");
    if (!approveRefundBtns) return;
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
    if (!rejectRefundBtns) return;
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

    // Approve refund buttons
    const failPaymentBtn = document.querySelectorAll(".fail-payment-btn");
    if (!failPaymentBtn) return;
    failPaymentBtn.forEach(btn => {
        btn.addEventListener('click', () => {
            confirmMessage(() => {
                const paymentId = btn.getAttribute('data-payment-id');
                console.log("Payment ID:", paymentId);
                document.getElementById('refundPaymentId').value = paymentId;
                document.getElementById('refundApproveStatus').value = true; // Approve = true
                document.getElementById('refundActionForm').submit();
            }, "Confirm approving this refund request?");
        });
    });

    // Reject refund buttons
    const restorePaymentBtn = document.querySelectorAll(".restore-payment-btn");
    if (!restorePaymentBtn) return;
    restorePaymentBtn.forEach(btn => {
        btn.addEventListener('click', () => {
            confirmMessage(() => {
                const paymentId = btn.getAttribute('data-payment-id');
                document.getElementById('refundPaymentId').value = paymentId;
                document.getElementById('refundApproveStatus').value = false; // Reject = false
                document.getElementById('refundActionForm').submit();
            }, "Confirm rejecting this refund request?");
        });
    });
});

document.addEventListener('DOMContentLoaded', () => {
    // Remove buttons
    const removePostBtns = document.querySelectorAll(".remove-post-btn");
    if (!removePostBtns) return;
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
    if (!removeCommentBtns) return;
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
    if (!removeReviewBtns) return;
    removeReviewBtns.forEach(btn => {
        btn.addEventListener('click', () => {
            confirmMessage(() => {
                const reviewId = btn.getAttribute('data-review-id');
                document.getElementById('removeReviewId').value = reviewId;
                document.getElementById('removeReviewForm').submit();
            }, "Confirm the removal of this review?");
        });
    });

    //// Restore buttons
    const restoreReviewBtns = document.querySelectorAll(".restore-review-btn");
    if (!restoreReviewBtns) return;
    restoreReviewBtns.forEach(btn => {
        btn.addEventListener('click', () => {
            confirmMessage(() => {
                const reviewId = btn.getAttribute('data-review-id');
                document.getElementById('restoreReviewId').value = reviewId;
                document.getElementById('restoreReviewForm').submit();
            }, "Confirm the restore of this review?");
        });
    });

    //// Restore buttons
    const restorePostBtns = document.querySelectorAll(".restore-post-btn");
    if (!restorePostBtns) return;
    restorePostBtns.forEach(btn => {
        btn.addEventListener('click', () => {
            confirmMessage(() => {
                const postId = btn.getAttribute('data-post-id');
                document.getElementById('restorePostId').value = postId;
                document.getElementById('restorePostForm').submit();
            }, "Confirm the restore of this post?");
        });
    });

    //// Restore buttons
    const restoreCommentBtns = document.querySelectorAll(".restore-comment-btn");
    if (!restoreCommentBtns) return;
    restoreCommentBtns.forEach(btn => {
        btn.addEventListener('click', () => {
            confirmMessage(() => {
                const commentId = btn.getAttribute('data-comment-id');
                document.getElementById('restoreCommentId').value = commentId;
                document.getElementById('restoreCommentForm').submit();
            }, "Confirm the restore of this comment?");
        });
    });

});