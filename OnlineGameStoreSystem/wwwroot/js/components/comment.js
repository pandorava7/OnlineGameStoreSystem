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

            div.innerHTML = `
                <div class="user-info">
                    <img src="${c.authorAvatarUrl || '/images/avatar_default.png'}" class="author-avatar" />
                    <div class="right">
                        <div class="comment-header" style="display:flex; justify-content:space-between; align-items:center;">
                            <span class="author-name">${c.authorName}</span>
                        </div>
                        <p class="comment-content">${c.content}</p>
                        <span class="comment-date">${dateText}</span>
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

                    confirmDeleteComment(async () => {
                        try {
                            const res = await fetch(`/Comment/Delete?commentId=${commentId}`, {
                                method: 'POST',
                                headers: { 'RequestVerificationToken': token }
                            });

                            if (res.ok) {
                                divToRemove.remove();
                            } else {
                                alert('删除失败');
                            }
                        } catch (err) {
                            console.error(err);
                            alert('删除出错');
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
        renderComments(data.comments, data.currentUserId);
    }

    // 点击发布
    postCommentBtn.addEventListener('click', async () => {
        const text = commentContent.value.trim();
        if (!text) return alert('评论不能为空');

        const token = document.querySelector('input[name="__RequestVerificationToken"]').value;

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
            alert(msg || '评论失败');
        }
    });

    // 页面加载时获取评论
    loadComments();
});
