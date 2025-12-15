document.addEventListener('DOMContentLoaded', () => {
    const commentContent = document.getElementById('commentContent');
    const postCommentBtn = document.getElementById('postCommentBtn');
    const commentContainer = document.querySelector('.comment-container');
    const postId = commentContent.dataset.postid;

    // 渲染评论
    let openOptionsCard = null; // 当前打开的 card

    function renderComments(comments, currentUserId) {
        commentContainer.innerHTML = '';
        comments.forEach(c => {
            console.log(c.id)
            const div = document.createElement('div');
            div.className = 'comment-item';

            // 安全解析日期
            let dateText = '';
            if (c.createdAt) {
                const d = new Date(c.createdAt);
                dateText = isNaN(d) ? '' : d.toISOString().split('T')[0];
            }

            // 判断是否显示删除按钮
            const showDelete = c.userId == currentUserId;
            console.log(c.userId);
            // 判断是否点过赞
            const likedClass = c.isLiked ? "liked" : "";

            div.innerHTML = `
                <div class="user-info">
                    <img src="${c.authorAvatarUrl || '/images/avatar_default.png'}" class="author-avatar" />
                    <div class="right">
                        <div class="comment-header" style="display:flex; justify-content:space-between; align-items:center;">
                            <span class="author-name">${c.authorName}</span>
                        </div>

                        <p class="comment-content">${c.content}</p>

                        <div class="comment-footer">
                            <span class="comment-date">${dateText}</span>

                            <!-- 这里：点赞按钮 -->
                            <div class="like-area ${likedClass}" data-liked="false">
                                <i class="fa fa-heart like-icon"></i>
                                <span class="like-count">${c.likeCount ?? 0}</span>
                            </div>
                        </div>
                    </div>
                </div>

                <div class="comment-options">
                    <span class="dots">&#x22EE;</span>
                    <div class="options-card">
                        <button class="report-btn" data-commentid="${c.id}">Reporting</button>
                        ${showDelete ? `<button class="delete-btn" data-commentid="${c.id}">Delete</button>` : ''}
                    </div>
                </div>
            `;

            commentContainer.appendChild(div);

            // 点赞按钮交互（只做视觉，不调用后端）
            const likeArea = div.querySelector('.like-area');

            likeArea.addEventListener('click', async () => {

                const isLiked = likeArea.classList.contains('liked');
                const commentId = c.id;
                const token = document.querySelector('input[name="__RequestVerificationToken"]').value;

                const res = await fetch('/Comment/ToggleLike', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json',
                        'RequestVerificationToken': token
                    },
                    body: JSON.stringify({ CommentId: commentId })
                });

                const data = await res.json();

                if (res.status === 401) {
                    showTemporaryMessage("Please login first.", "error");
                    return;
                }

                if (!data.success) return;

                // 根据后端返回的状态决定视觉
                if (data.liked === true) {
                    likeArea.classList.add('liked');
                } else {
                    likeArea.classList.remove('liked');
                }

                likeArea.querySelector('.like-count').textContent = data.likeCount;
            });


            // 添加操作按钮事件
            const options = div.querySelector('.comment-options');
            const dots = options.querySelector('.dots');
            const card = options.querySelector('.options-card');

            dots.addEventListener('click', (e) => {
                e.stopPropagation();

                // 关闭之前打开的 card（如果不是当前这个）
                if (openOptionsCard && openOptionsCard !== card) {
                    openOptionsCard.style.display = 'none';
                }

                // 切换当前 card
                card.style.display = card.style.display === 'flex' ? 'none' : 'flex';
                openOptionsCard = card.style.display === 'flex' ? card : null;
            });

            // 点击页面其他地方关闭 card
            document.addEventListener('click', () => {
                if (openOptionsCard) {
                    openOptionsCard.style.display = 'none';
                    openOptionsCard = null;
                }
            });

            // 删除按钮
            const deleteBtn = options.querySelector('.delete-btn');
            if (deleteBtn) {
                deleteBtn.addEventListener('click', (e) => {
                    e.stopPropagation();

                    const commentId = deleteBtn.dataset.commentid;
                    const divToRemove = div; // 闭包保存当前评论元素
                    const token = document.querySelector('input[name="__RequestVerificationToken"]').value;

                    confirmDeleteReview(async () => {
                        try {
                            const res = await fetch(`/Comment/Delete?commentId=${commentId}`, {
                                method: 'POST',
                                headers: { 'RequestVerificationToken': token }
                            });

                            if (res.ok) {
                                divToRemove.remove();
                            } else {
                                showTemporaryMessage('Failed to delete', 'error')
                            }
                        } catch (err) {
                            console.error(err);
                            showTemporaryMessage('Error when delete', 'error')
                        }
                    });
                });

            }

            // 举报按钮
            const reportBtn = options.querySelector('.report-btn');
            reportBtn.addEventListener('click', (e) => {
                e.stopPropagation();
                alert(`举报评论 ${reportBtn.dataset.commentid}`);
                card.style.display = 'none';
                openOptionsCard = null;
            });
        });
    }



    // 获取评论
    async function loadComments() {
        const res = await fetch(`/Comment/GetByPost?postId=${postId}`);
        const data = await res.json();
        const { comments, currentUserId } = data;
        console.log(comments)
        renderComments(comments, currentUserId);

    }

    // 点击发布
    postCommentBtn.addEventListener('click', async () => {
        const text = commentContent.value.trim();
        if (!text) {
            showTemporaryMessage('Comment cannot be empty', 'error')
            return;
        }

        const token = document.querySelector('input[name="__RequestVerificationToken"]').value;

        console.log(text)
        console.log(postId)
        const res = await fetch('/Comment/Add', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/x-www-form-urlencoded',
                'RequestVerificationToken': token
            },
            body: `postId=${postId}&content=${encodeURIComponent(text)}`
        });

        if (res.ok) {
            commentContent.value = '';
            await loadComments(); // 重新加载评论
        } else {
            const msg = await res.text();
            showTemporaryMessage(msg + 'failed to comment', 'error')
        }
    });

    // 页面加载时获取评论
    loadComments();
});
