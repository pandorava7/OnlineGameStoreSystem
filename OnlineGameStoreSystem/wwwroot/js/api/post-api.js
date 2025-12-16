// post_api.js
window.PostAPI = {
    post: null, // 当前操作的 post 数据

    async likePost(postId) {
        console.log("execute likePost");
        const res = await fetch(`/Post/Like`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({ postId })
        });


        console.log(res);
        const data = await res.json();
        console.log(data);

        if (!data.success) {
            console.log("Like post failed:", data.message);
            showTemporaryMessage(data.message || "Like failed", "error");
            return null;
        } else {
            console.log("Like post success");
            showTemporaryMessage("Liked post", "info");
            this.post = data.post; // 保存后端返回的 post 数据
        }

        return data.post;
    }
};


window.GameAPI = {
    game: null,

    async likeGame(gameId) {
        console.log("execute likeGame " + gameId);

        const res = await fetch(`/Game/Like`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({ gameId })
        });

        console.log(res);

        const data = await res.json();
        console.log(data);

        if (!data.success) {
            console.log("Like game failed:", data.message);
            showTemporaryMessage(data.message || "Like failed", "error");
            return null;
        } else {
            console.log("Like game success");
            showTemporaryMessage("Game liked", "info");

            // 保存后端返回的 game 数据
            this.game = data.game;
        }

        return data.game;
    }
};
