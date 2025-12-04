function confirmDeletePost() {
    Modal.show({
        message: "Are you sure you want to delete this post?",
        buttons: [
            {
                text: "Cancel",
                type: "secondary-btn",
                onClick: () => console.log("Cancel clicked")
            },
            {
                text: "Delete",
                type: "primary-btn",
                onClick: () => {
                    console.log("Deleting...");
                    document.getElementById("deleteForm").submit();
                }
            }
        ]
    });
}

function confirmDeleteReview(onConfirm) {
    Modal.show({
        message: "Delete your comment?",
        buttons: [
            {
                text: "Cancel",
                type: "secondary-btn",
                onClick: () => console.log("Cancel clicked")
            },
            {
                text: "Delete",
                type: "primary-btn",
                onClick: () => {
                    console.log("Deleting...");
                    if (typeof onConfirm === "function") {
                        onConfirm(); // 调用回调
                    }
                }
            }
        ]
    });
}

function confirmDownloadGame(onConfirm) {
    Modal.show({
        message: "Confirm to download?",
        buttons: [
            {
                text: "Cancel",
                type: "secondary-btn",
                onClick: () => console.log("Cancel clicked")
            },
            {
                text: "Download",
                type: "primary-btn",
                onClick: () => {
                    if (typeof onConfirm === "function") {
                        onConfirm(); // 调用回调
                    }
                }
            }
        ]
    });
}

function confirmMessage(onConfirm, message) {
    Modal.show({
        message: message,
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
                    if (typeof onConfirm === "function") {
                        onConfirm(); // 调用回调
                    }
                }
            }
        ]
    });
}
