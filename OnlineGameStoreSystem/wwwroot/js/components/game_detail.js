
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
    /* 星星交互 */
    let starBox = document.getElementById("star-box");
    let stars = starBox.querySelectorAll("span");
    let rating = 0;

    stars.forEach(star => {
        star.addEventListener("mouseenter", () => highlight(star.dataset.v));
        star.addEventListener("click", () => {
            rating = star.dataset.v;
            highlight(rating);
        });
    });

    starBox.addEventListener("mouseleave", () => highlight(rating));

    function highlight(v) {
        stars.forEach(s => {
            if (s.dataset.v <= v) s.classList.add("active");
            else s.classList.remove("active");
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













//let currentSlide = 0;
//const carouselInner = document.querySelector('.carousel-inner');
//const slides = document.querySelectorAll('.carousel-item');

//function showSlide(index) {
//    if (index < 0) index = slides.length - 1;
//    if (index >= slides.length) index = 0;
//    currentSlide = index;
//    carouselInner.style.transform = `translateX(-${currentSlide * 100}%)`;
//}
//function prevSlide() { showSlide(currentSlide - 1); }
//function nextSlide() { showSlide(currentSlide + 1); }

//// 添加到购物车按钮事件
//document.querySelectorAll('.btn-add-to-cart').forEach(btn => {
//    btn.addEventListener('click', async () => {
//        const gameid = btn.dataset.gameid;
//        await addToCart(gameid);
//    });
//});

//// =====================
//// 添加到购物车（服务器）
//// =====================
//async function addToCart(gameId) {
//    const res = await fetch(`/Cart/AddItem?gameId=${gameId}`, {
//        method: 'POST'
//    });

//    const result = await res.json();

//    if (result.message == "User not found") {
//        showTemporaryMessage("Please log in before your add item!", "error");
//    }

//    if (result.message == "You already own this game") {
//        showTemporaryMessage("You already own this game", "error");
//    }

//    if (result.success) {
//        showTemporaryMessage("Added to your cart", "success");

//        // 更新购物车徽章
//        await CartAPI.loadCartFromServer();
//    }
//}