function confirmDeletePost() {
    Modal.show({
        message: "Are you sure you want to delete this post?",
        buttons: [
            { text: "Cancel", type: "secondary", onClick: () => console.log("Cancel clicked") },
            {
                text: "Delete", type: "primary", onClick: () => {
                    console.log("Deleting...");
                    document.getElementById("postForm").submit(); // 提交表单
                }
            }
        ]
    });
}