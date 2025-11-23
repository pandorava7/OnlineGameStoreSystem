document.addEventListener("DOMContentLoaded", () => {
    const titleInput = document.getElementById("title");
    const contentInput = document.getElementById("content");
    const thumbnailInput = document.getElementById("thumbnail");

    const preview = document.getElementById("postPreview");
    const previewTitle = preview.querySelector(".post-title");
    const previewContent = preview.querySelector(".post-snippet");
    const previewThumbnail = preview.querySelector(".post-thumbnail");

    // 监听文字输入
    titleInput.addEventListener("input", () => {
        previewTitle.textContent = titleInput.value || "Post Title";
    });

    contentInput.addEventListener("input", () => {
        previewContent.textContent = contentInput.value || "Post content preview...";
    });

    // 监听缩略图更新
    thumbnailInput.addEventListener("change", () => {
        const file = thumbnailInput.files[0];
        if (file) {
            const reader = new FileReader();
            reader.onload = function (e) {
                previewThumbnail.src = e.target.result;
            };
            reader.readAsDataURL(file);     
        } else {
            previewThumbnail.src = "/images/placeholder.png";
        }
    });
});
