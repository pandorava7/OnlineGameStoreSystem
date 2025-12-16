document.addEventListener('DOMContentLoaded', () => {
    const likeBtn = document.getElementById('likeBtn');
    const likeCount = document.getElementById('likeCount');

    if (likeBtn && likeCount) {
        likeBtn.addEventListener('click', async () => {
            console.log("like " + likeBtn.dataset.postid);

            const postId = likeBtn.dataset.postid;

            await PostAPI.likePost(postId);  // 等后端完成点赞逻辑并加载帖子数据

            // 更新这个帖子的点赞数
            likeCount.innerHTML = `<i class="fa fa-heart"></i> ${PostAPI.post.likeCount}`;

        });
    }
    
});

document.addEventListener('DOMContentLoaded', () => {
    const likeBtn = document.getElementById('likeGameBtn');
    //const likeCount = document.getElementById('likeGameCount');

    if (likeBtn) {
        likeBtn.addEventListener('click', async () => {
            const gameId = likeBtn.dataset.gameId;

            console.log("before likeGame " + gameId);
            await GameAPI.likeGame(gameId); // 等后端完成点赞逻辑并返回 game 数据

            // 更新该游戏的点赞数
            //likeCount.innerHTML = `<i class="fa fa-heart"></i> ${GameAPI.game.likeCount}`;
        });
    }
    
});
