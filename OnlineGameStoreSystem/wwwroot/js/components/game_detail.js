
    document.addEventListener("DOMContentLoaded", () => {
        const main = document.getElementById("gd-main-media");
        const thumbs = document.getElementById("gd-thumbs");

        let mediaItems = [];
        let currentIndex = 0;
        let timer = null;

        // 收集所有媒体
        document.querySelectorAll(".gd-thumb").forEach(btn => {
            mediaItems.push({ type: btn.dataset.type, src: btn.dataset.src });
        });
        if (mediaItems.length === 0) return;

        function clearTimer() {
            if (timer) {
                clearTimeout(timer);
                timer = null;
            }
        }

        function renderMedia(index) {
            clearTimer(); // 先清除旧定时器
            currentIndex = index;
            main.innerHTML = "";
            const item = mediaItems[index];

            if (item.type === "video") {
                const video = document.createElement("video");
                video.src = item.src;
                video.autoplay = true;
                video.controls = true;
                main.appendChild(video);

                // 视频播放完再切换到下一媒体
                video.onended = () => nextMedia();
            } else {
                const img = document.createElement("img");
                img.src = item.src;
                main.appendChild(img);

                // 图片轮播 3 秒后切换
                timer = setTimeout(() => nextMedia(), 3000);
            }
        }

        function nextMedia() {
            currentIndex = (currentIndex + 1) % mediaItems.length;
            renderMedia(currentIndex);
        }

        // 缩略图点击立即跳转
        thumbs.addEventListener("click", e => {
            const btn = e.target.closest(".gd-thumb");
            if (!btn) return;
            const index = parseInt(btn.dataset.index);
            renderMedia(index);
        });

        // 启动轮播
        // 找到第一个视频索引
        let firstVideoIndex = mediaItems.findIndex(item => item.type === "video");
        if (firstVideoIndex !== -1) {
            // 先播放视频
            renderMedia(firstVideoIndex);
        } else {
            // 没有视频，播放第一个图片
            renderMedia(0);
        }
    });





document.addEventListener("DOMContentLoaded", () => {
    let starBox = document.getElementById("star-box");
    let stars = starBox.querySelectorAll("span");
    let rating = 0;

    // hover
    stars.forEach(star => {
        star.addEventListener("mouseenter", () => {
            highlight(star.dataset.v);
        });

        // click 选择评分
        star.addEventListener("click", () => {
            rating = star.dataset.v;
            lock(rating);

            // 添加动画 class
            star.classList.add("pop");

            // 动画结束后移除，保证下次点击仍然能播放
            setTimeout(() => star.classList.remove("pop"), 400);
        });
    });

    // 鼠标移出恢复到点击状态
    starBox.addEventListener("mouseleave", () => {
        lock(rating);
        highlight(rating);
    });

    // hover 效果
    function highlight(v) {
        stars.forEach(s => {
            if (s.dataset.v <= v) {
                s.classList.add("active");
            } else {
                s.classList.remove("active");
            }
        });
    }

    // 点击后的固定效果
    function lock(v) {
        stars.forEach(s => {
            if (s.dataset.v <= v) {
                s.classList.add("selected");
            } else {
                s.classList.remove("selected");
            }
        });
    }

    /* 提交评论到 MVC */
    document.getElementById("btn-submit-review").onclick = async () => {
        let content = document.getElementById("review-content").value.trim();
        if (rating == 0) return alert("请给星级！");
        if (!content) return alert("评价内容不能为空！");

        let token = document.querySelector("input[name='__RequestVerificationToken']").value;

        let res = await fetch("/Game/Review", {
            method: "POST",
            headers: {
                "Content-Type": "application/json",
                "RequestVerificationToken": token
            },
            body: JSON.stringify({
                GameId: 2,
                Rating: rating,
                Content: content
            })
        });

        let json = await res.json();
        if (json.success) {
            alert("评价已提交！");
            location.reload();
        } else {
            alert(json.message || "提交失败");
        }
    };
});


// ================ Review Submission ======================

document.addEventListener('DOMContentLoaded', () => {
    const reviewContent = document.getElementById('reviewContent');
    const postReviewBtn = document.getElementById('postReviewBtn');
    const reviewContainer = document.querySelector('.review-container');
    const gameId = reviewContent.dataset.gameId;

    // 渲染评论
    let openOptionsCard = null; // 当前打开的 card

    function renderReviews(reviews, currentUserId) {
        reviewContainer.innerHTML = '';
        reviews.forEach(r => {
            console.log(r.id)
            const div = document.createElement('div');
            div.className = 'review-item';

            // 安全解析日期
            let dateText = '';
            if (r.createdAt) {
                const d = new Date(r.createdAt);
                dateText = isNaN(d) ? '' : d.toISOString().split('T')[0];
            }

            // 判断是否显示删除按钮
            const showDelete = r.userId == currentUserId;
            console.log(r.userId);
            // 判断是否点过赞
            const likedClass = r.isLiked ? "liked" : "";

            div.innerHTML = `
                <div class="user-info">
                    <img src="${r.authorAvatarUrl || '/images/avatar_default.png'}" class="author-avatar" />
                    <div class="right">
                        <div class="review-header" style="display:flex; justify-content:space-between; align-items:center;">
                            <span class="author-name">${r.authorName}</span>
                        </div>

                        <p class="review-content">${r.content}</p>

                        <div class="review-footer">
                            <span class="review-date">${dateText}</span>

                            <!-- 这里：点赞按钮 -->
                            <div class="like-area ${likedClass}" data-liked="false">
                                <i class="fa fa-heart like-icon"></i>
                                <span class="like-count">${r.likeCount ?? 0}</span>
                            </div>
                        </div>
                    </div>
                </div>

                <div class="review-options">
                    <span class="dots">&#x22EE;</span>
                    <div class="options-card">
                        <button class="report-btn" data-reviewid="${r.id}">Reporting</button>
                        ${showDelete ? `<button class="delete-btn" data-reviewid="${r.id}">Delete</button>` : ''}
                    </div>
                </div>
            `;

            reviewContainer.appendChild(div);

            // 点赞按钮交互（只做视觉，不调用后端）
            const likeArea = div.querySelector('.like-area');

            likeArea.addEventListener('click', async () => {

                const isLiked = likeArea.classList.contains('liked');
                const reviewId = r.id;
                const token = document.querySelector('input[name="__RequestVerificationToken"]').value;

                const res = await fetch('/Review/ToggleLike', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json',
                        'RequestVerificationToken': token
                    },
                    body: JSON.stringify({ ReviewId: reviewId })
                });

                const data = await res.json();

                if (res.status === 401) {
                    alert("Please login first.");
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
            const options = div.querySelector('.review-options');
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

                    const reviewtId = deleteBtn.dataset.reviewid;
                    const divToRemove = div; // 闭包保存当前评论元素
                    const token = document.querySelector('input[name="__RequestVerificationToken"]').value;

                    confirmDeleteReview(async () => {
                        try {
                            const res = await fetch(`/Review/Delete?reviewId=${reviewtId}`, {
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
                alert(`举报评论 ${reportBtn.dataset.reviewid}`);
                card.style.display = 'none';
                openOptionsCard = null;
            });
        });
    }



    // 获取评论
    async function loadReviews() {
        const res = await fetch(`/Review/GetByPost?postId=${gameId}`);
        const data = await res.json();
        const { reviews, currentUserId } = data;
        console.log(reviews)
        renderReviews(reviews, currentUserId);

    }

    // 点击发布
    postReviewBtn.addEventListener('click', async () => {
        const text = reviewContent.value.trim();
        if (!text) {
            showTemporaryMessage('Review cannot be empty', 'error')
            return;
        }

        const token = document.querySelector('input[name="__RequestVerificationToken"]').value;

        const res = await fetch('/Review/Add', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/x-www-form-urlencoded',
                'RequestVerificationToken': token
            },
            body: `postId=${gameId}&content=${encodeURIComponent(text)}`
        });

        if (res.ok) {
            reviewContent.value = '';
            await loadReviews(); // 重新加载评论
        } else {
            const msg = await res.text();
            showTemporaryMessage(msg + 'failed to review', 'error')
        }
    });

    // 页面加载时获取评论
    loadReviews();
});
