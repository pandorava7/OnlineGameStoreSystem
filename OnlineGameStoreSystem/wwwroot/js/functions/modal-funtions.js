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

function confirmDeleteComment(onConfirm) {
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
