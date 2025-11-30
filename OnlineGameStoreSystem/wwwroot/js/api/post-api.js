// post_api.js
window.PostAPI = {
    post: null, // 当前操作的 post 数据

    async likePost(postId) {
        const res = await fetch(`/Post/Like`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': this.getAntiForgeryToken()
            },
            body: JSON.stringify({ postId })
        });

        const data = await res.json();

        if (!data.success) {
            showTemporaryMessage(data.message || "Like failed", "error");
        } else {
            showTemporaryMessage("Liked post", "info");
            this.post = data.post; // 保存后端返回的 post 数据
        }

        console.log(data.post);

        return data.post;
    },

    getAntiForgeryToken() {
        // 获取 Razor 页面的防伪令牌
        const tokenEl = document.querySelector('input[name="__RequestVerificationToken"]');
        return tokenEl ? tokenEl.value : '';
    }
};
