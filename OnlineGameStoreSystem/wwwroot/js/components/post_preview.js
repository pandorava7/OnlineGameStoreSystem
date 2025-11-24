document.addEventListener("DOMContentLoaded", () => {
    const titleInput = document.getElementById("title");
    const contentInput = document.getElementById("content");
    const thumbnailUrlInput = document.getElementById("thumbnailUrl");
    const thumbnailInput = document.getElementById("thumbnail");

    const preview = document.getElementById("postPreview");
    const previewTitle = preview.querySelector(".post-title");
    const previewContent = preview.querySelector(".post-snippet");
    const previewThumbnail = preview.querySelector(".post-thumbnail");

    // 封装更新函数
    function updatePreview() {
        previewTitle.textContent = titleInput.value || "Post Title";
        previewContent.textContent = contentInput.value || "Post content preview...";
        previewThumbnail.src = thumbnailUrlInput.value; 

        if (thumbnailInput.files[0]) {
            const reader = new FileReader();
            reader.onload = function (e) {
                previewThumbnail.src = e.target.result;
            };
            reader.readAsDataURL(thumbnailInput.files[0]);
        } else if (document.getElementById("thumbnailUrl").value) {
            // 编辑页已有缩略图
            previewThumbnail.src = document.getElementById("thumbnailUrl").value;
        } else {
            previewThumbnail.src = "/images/placeholder.png";
        }
    }


    // 监听用户输入
    titleInput.addEventListener("input", updatePreview);
    contentInput.addEventListener("input", updatePreview);
    thumbnailInput.addEventListener("change", updatePreview);

    // 页面初始化时先调用一次，投射已有值
    updatePreview();
});
