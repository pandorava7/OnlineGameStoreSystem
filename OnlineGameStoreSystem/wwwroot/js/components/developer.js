// 删除游戏确认弹窗
document.addEventListener('DOMContentLoaded', () => {
    const removeGameBtns = document.querySelectorAll(".remove-game-btn");
    
    removeGameBtns.forEach(btn => {
        btn.addEventListener('click', () => {
            confirmMessage(() => {
                const gameId = btn.getAttribute('data-game-id');
                document.getElementById('removeGameId').value = gameId;
                document.getElementById('removeGameForm').submit();
            },
                "Confirm to remove your game? Your game data is still storing in our database, you can call admin to restore your game.")
        });
    });
});

// Thumbnail preview (single) — enhanced: drag/drop, click-to-select, remove button
// Refactored to use a single container for both upload prompt and preview.
(function () {
    var input = document.getElementById('thumbnailInput');
    var container = document.getElementById('thumbPreviewContainer');

    if (!input || !container) return;

    // 1. 创建预览图片元素和移除按钮元素（只创建一次，重复使用）
    var img = document.createElement('img');
    img.className = 'thumb-img';
    container.appendChild(img);

    var removeBtn = document.createElement('button');
    removeBtn.type = 'button';
    removeBtn.className = 'thumb-remove-btn';
    removeBtn.textContent = '✖';
    removeBtn.title = 'Remove Thumbnail';
    container.appendChild(removeBtn);

    // 用于存储当前的对象 URL，以便清理
    var currentUrl = null;

    // 渲染文件的函数
    function renderFile(file) {
        if (!file || !file.type.startsWith('image/')) return;

        // 清理旧的 URL
        if (currentUrl) URL.revokeObjectURL(currentUrl);

        // 创建新的 URL 并设置给图片
        currentUrl = URL.createObjectURL(file);
        img.src = currentUrl;

        // 添加 has-image 类，CSS 会据此显示图片和移除按钮，隐藏占位符
        container.classList.add('has-image');
    }

    // 清除预览的函数
    function clearPreview(e) {
        // 阻止事件冒泡，防止触发 label 的点击打开文件选择框
        if (e) {
            e.preventDefault();
            e.stopPropagation();
        }

        // 清理 URL
        if (currentUrl) URL.revokeObjectURL(currentUrl);
        currentUrl = null;

        // 重置图片源
        img.src = '';

        // 清空 input 的值，确保表单不会提交已删除的文件
        input.value = '';

        // 移除 has-image 类，恢复到上传提示状态
        container.classList.remove('has-image');
    }

    // --- 事件监听 ---

    // input 改变时（用户选择了文件）
    input.addEventListener('change', function () {
        var file = input.files && input.files[0];
        if (file) {
            renderFile(file);
        } else {
            // 用户打开了选择框但点了取消，我们保持现有状态不变
            // 或者如果需要清空，可以调用 clearPreview();
        }
    });

    // 点击移除按钮
    removeBtn.addEventListener('click', clearPreview);

    // 拖放支持
    container.addEventListener('dragover', function (e) {
        e.preventDefault();
        container.classList.add('dragover');
    });

    container.addEventListener('dragleave', function () {
        container.classList.remove('dragover');
    });

    container.addEventListener('drop', function (e) {
        e.preventDefault();
        container.classList.remove('dragover');

        var files = Array.from(e.dataTransfer.files || []).filter(f => f.type.startsWith('image/'));
        if (files.length === 0) return;

        var file = files[0];

        // 尝试将拖放的文件赋值给 input，以便表单提交
        try {
            var dt = new DataTransfer();
            dt.items.add(file);
            input.files = dt.files;
        } catch (err) {
            console.warn('DataTransfer not supported, form submission might not work for dropped file.');
        }

        // 渲染预览
        renderFile(file);
    });
})();

// Image preview helpers (use Object URLs for file previews)
(function () {

    document.querySelectorAll('.dashed-dropzone').forEach(function (zone) {
        zone.addEventListener('click', function () {
            var inputId = zone.getAttribute('data-for');
            var input = document.getElementById(inputId);
            if (input) {
                input.click();
            }
        });
    });

    // 辅助函数：创建元素
    function el(tag, cls) { var e = document.createElement(tag); if (cls) e.className = cls; return e; }

    // =================================================================
    // 1. Preview images input (max 8) - 修正：文件累积、上限检查、移除功能
    // =================================================================
    (function () {
        var input = document.getElementById('previewImagesInput');
        var grid = document.getElementById('previewGrid');
        if (!input || !grid) return;

        // 【核心】文件累积器
        var filesAccumulator = new DataTransfer();
        var MAX_PREVIEW_COUNT = 8;
        var urlMap = new Map(); // 用于跟踪和撤销 Object URLs

        // 【新增/重构】渲染所有当前累积的文件
        function renderAllPreviews() {
            // 1. 清理旧的 DOM 和 URL
            urlMap.forEach(function (url) { try { URL.revokeObjectURL(url); } catch (e) { } });
            urlMap.clear();
            while (grid.firstChild) grid.removeChild(grid.firstChild);

            // 确保实际的 input.files 始终与累积器同步
            input.files = filesAccumulator.files;

            // 2. 渲染文件
            Array.from(filesAccumulator.files).forEach(function (file) {
                var item = el('div', 'preview-item');
                var img = el('img');
                img.style.width = '100%';
                img.style.height = '100%';
                img.style.objectFit = 'cover';

                var objUrl = URL.createObjectURL(file);
                urlMap.set(item, objUrl);
                img.src = objUrl;

                item.appendChild(img);

                var btn = el('button', 'preview-remove-btn');
                btn.type = 'button';
                btn.textContent = '✖';
                btn.title = 'Remove';

                // 移除按钮逻辑：从累积器中删除并重新渲染
                btn.addEventListener('click', function (e) {
                    e.preventDefault();

                    var currentItems = Array.from(grid.children);
                    var removeIndex = currentItems.indexOf(item);

                    if (removeIndex !== -1) {
                        filesAccumulator.items.remove(removeIndex);
                        renderAllPreviews(); // 重新渲染界面
                    }
                });
                item.appendChild(btn);
                grid.appendChild(item);
            });
        }

        // 【核心逻辑】将新文件添加到累积器
        function accumulateFiles(newFiles) {
            var filesToProcess = Array.from(newFiles).filter(f => f.type.startsWith('image/'));
            var addedCount = 0;

            filesToProcess.forEach(function (file) {
                if (filesAccumulator.items.length < MAX_PREVIEW_COUNT) {
                    filesAccumulator.items.add(file);
                    addedCount++;
                }
            });

            if (addedCount > 0) {
                renderAllPreviews(); // 只有在有新文件添加时才重新渲染
            }
        }

        // 1. Drag & Drop 事件 (修改为累积模式)
        //var dropzone = document.querySelector('.dashed-dropzone[for="previewImagesInput"]');
        var dropzone = document.querySelector('.dashed-dropzone[data-for="previewImagesInput"]');
        console.log("dropzone "+ dropzone);

        if (dropzone) {
            dropzone.addEventListener('dragover', function (e) { e.preventDefault(); dropzone.classList.add('dragover'); });
            dropzone.addEventListener('dragleave', function () { dropzone.classList.remove('dragover'); });

            dropzone.addEventListener('drop', function (e) {
                e.preventDefault();
                dropzone.classList.remove('dragover');

                var files = e.dataTransfer.files || [];
                accumulateFiles(files); // 调用新的累积函数
            });
        }

        // 2. input change 事件 (点击选择文件 - 修改为累积模式)
        input.addEventListener('change', function (e) {
            // 将新选择的文件添加到累积器
            accumulateFiles(e.target.files);

            // 清空 input 的值，以便下次选择相同文件时依然触发 change 事件
            e.target.value = '';
        });

        // 暴露清空函数 (用于外部调用)
        window._devPreviewImagesClear = function () {
            filesAccumulator = new DataTransfer(); // 重置累积器
            renderAllPreviews();
        };

        // --- 【新增】媒体初始化函数，必须在 DOMContentLoaded 中调用 ---
        window._initExistingMedia = function () {

            var deletedUrls = []; // 用于存储被删除的现有媒体URL

            // 获取所有现有的预览图片和视频项
            var existingItems = document.querySelectorAll('#previewGrid .preview-item[data-existing="true"], #trailerGrid .preview-item[data-existing="true"]');

            // 监听现有媒体的移除按钮
            existingItems.forEach(function (item) {
                var removeBtn = item.querySelector('.preview-remove-btn');
                var mediaUrl = item.getAttribute('data-src');

                if (removeBtn && mediaUrl) {
                    removeBtn.addEventListener('click', function (e) {
                        e.preventDefault();

                        // 1. 标记为删除（视觉上）
                        item.style.opacity = '0.5';
                        item.style.pointerEvents = 'none';
                        item.dataset.removed = "true";

                        // 2. 将 URL 添加到待删除列表
                        deletedUrls.push(mediaUrl);

                        // 3. 更新隐藏输入框
                        document.getElementById('deletedMediaInput').value = JSON.stringify(deletedUrls);

                        // 可选：将 DOM 元素移除
                        // item.remove(); 
                    });
                }
            });
        };

        // 确保在 DOM 加载完成后调用初始化函数
        document.addEventListener('DOMContentLoaded', function () {
            // ... (其他 DOMContentLoaded 逻辑) ...

            // 调用新的初始化函数
            window._initExistingMedia();
        });
    })();

    // =================================================================
    // 2. Trailer preview (videos) input (max 2) - 修正：拖放、文件累积、上限检查
    // =================================================================
    (function () {
        var input = document.getElementById('trailersInput');
        var grid = document.getElementById('trailerGrid');
        if (!input || !grid) return;

        // 【核心】文件累积器
        var filesAccumulator = new DataTransfer();
        var MAX_TRAILER_COUNT = 2;
        var urlMap = new Map();

        // 渲染单个视频预览（保持原有的 DOM 创建逻辑）
        function addVideoPreviewFromFile(file, item) {
            // ... (使用 el 创建 video, label, btn 的原始逻辑) ...
            var video = el('video');
            video.controls = true;
            video.style.width = '100%';
            video.style.height = '100%';
            video.style.objectFit = 'cover';

            var objUrl = URL.createObjectURL(file);
            urlMap.set(item, objUrl);
            video.src = objUrl;

            item.appendChild(video);

            var label = el('div', 'preview-item-label');
            label.textContent = file.name || '';
            item.appendChild(label);
        }

        // 【重构】渲染所有当前累积的文件
        function renderAllPreviews() {
            // 1. 清理旧的 DOM 和 URL
            urlMap.forEach(function (url) { try { URL.revokeObjectURL(url); } catch (e) { } });
            urlMap.clear();
            while (grid.firstChild) grid.removeChild(grid.firstChild);

            // 确保实际的 input.files 始终与累积器同步
            input.files = filesAccumulator.files;

            // 2. 渲染文件
            Array.from(filesAccumulator.files).forEach(function (file) {
                var item = el('div', 'preview-item');
                addVideoPreviewFromFile(file, item); // 调用视频预览创建

                var btn = el('button', 'preview-remove-btn');
                btn.type = 'button';
                btn.textContent = '✖';
                btn.title = 'Remove';

                // 移除按钮的事件处理
                btn.addEventListener('click', function (e) {
                    e.preventDefault();

                    var currentItems = Array.from(grid.children);
                    var removeIndex = currentItems.indexOf(item);

                    if (removeIndex !== -1) {
                        filesAccumulator.items.remove(removeIndex);
                        renderAllPreviews(); // 重新渲染界面
                    }
                });
                item.appendChild(btn);
                grid.appendChild(item);
            });
        }

        // 【核心逻辑】将新文件添加到累积器
        function accumulateFiles(newFiles) {
            var filesToProcess = Array.from(newFiles).filter(f => f.type.startsWith('video/'));
            var addedCount = 0;

            filesToProcess.forEach(function (file) {
                if (filesAccumulator.items.length < MAX_TRAILER_COUNT) {
                    filesAccumulator.items.add(file);
                    addedCount++;
                }
            });

            if (addedCount > 0) {
                renderAllPreviews();
            }
        }

        // 1. Drag & Drop 事件 (修复拖放和累积)
        var dropzone = document.querySelector('.dashed-dropzone[for="trailersInput"]');
        if (dropzone) {
            dropzone.addEventListener('dragover', function (e) { e.preventDefault(); dropzone.classList.add('dragover'); });
            dropzone.addEventListener('dragleave', function () { dropzone.classList.remove('dragover'); });

            dropzone.addEventListener('drop', function (e) {
                e.preventDefault();
                dropzone.classList.remove('dragover');

                var files = e.dataTransfer.files || [];
                accumulateFiles(files); // 调用新的累积函数
            });
        }

        // 2. input change 事件 (修复点击上传和累积)
        input.addEventListener('change', function (e) {
            accumulateFiles(e.target.files);
            e.target.value = ''; // 清除 input 值
        });

        window._devTrailerClear = function () {
            filesAccumulator = new DataTransfer();
            renderAllPreviews();
        };
    })();
})();


 //Client-side upload validation (images/videos/zip) — debug-enabled (fixed insertBefore crash)
(function () {
    var form = document.querySelector('form[enctype="multipart/form-data"]');
    if (!form) return;
    console.log('[dev] developer.js: client validation loaded for form', form);

    var MAX_THUMB = 5 * 1024 * 1024;
    var MAX_PREVIEW = 5 * 1024 * 1024;
    var MAX_TRAILER = 50 * 1024 * 1024;
    var MAX_ZIP = 200 * 1024 * 1024;
    var MAX_PREVIEW_COUNT = 8;
    var MAX_TRAILER_COUNT = 2;

    function showErrors(errors) {
        var containerId = 'clientUploadErrors';
        var existing = document.getElementById(containerId);
        if (existing) existing.remove();

        var div = document.createElement('div');
        div.id = containerId;
        div.className = 'upload-errors';
        div.setAttribute('role', 'alert');
        div.style.background = '#2b000a';
        div.style.color = '#ffd7df';
        div.style.padding = '10px';
        div.style.borderRadius = '6px';
        div.style.marginBottom = '12px';

        var html = '<strong>Validation errors:</strong><ul style="margin:8px 0 0 18px;">';
        errors.forEach(function (e) { html += '<li>' + e + '</li>'; });
        html += '</ul>';
        div.innerHTML = html;

        // Use prepend to avoid insertBefore errors when the reference node isn't a child
        // This ensures the error block is reliably placed at the top of the form.
        try {
            form.prepend(div);
            // scroll to the inserted div
            var rect = div.getBoundingClientRect();
            window.scrollTo({ top: rect.top + window.scrollY - 20, behavior: 'smooth' });
        } catch (ex) {
            // fallback: append to body if prepend fails for some reason
            console.warn('[dev] failed to prepend error div to form, falling back to body:', ex);
            document.body.insertBefore(div, document.body.firstChild);
            window.scrollTo({ top: div.getBoundingClientRect().top + window.scrollY - 20, behavior: 'smooth' });
        }
    }

    form.addEventListener('submit', function (ev) {
        console.log('[dev] submit event fired');
        var fileInputs = Array.from(form.querySelectorAll('input[type="file"]'))
            .map(function (i) { return { input: i, files: Array.from(i.files || []) }; });

        console.log('[dev] fileInputs:', fileInputs.map(fi => ({ id: fi.input.id, name: fi.input.name, count: fi.files.length })));

        var errors = [];

        var previewInput = fileInputs.find(f => f.input && f.input.id === 'previewImagesInput');
        var trailerInput = fileInputs.find(f => f.input && f.input.id === 'trailersInput');
        var thumbnailInput = fileInputs.find(f => f.input && f.input.id === 'thumbnailInput');
        var zipInput = fileInputs.find(f => f.input && f.input.name === 'GameZip');

        var previewCount = previewInput ? previewInput.files.length : 0;
        var trailerCount = trailerInput ? trailerInput.files.length : 0;

        if (previewCount > MAX_PREVIEW_COUNT) errors.push('Preview images: maximum ' + MAX_PREVIEW_COUNT + ' files allowed.');
        if (trailerCount > MAX_TRAILER_COUNT) errors.push('Trailer videos: maximum ' + MAX_TRAILER_COUNT + ' files allowed.');

        if (thumbnailInput && thumbnailInput.files.length > 0) {
            var f = thumbnailInput.files[0];
            if (!f.type.startsWith('image/')) errors.push('Thumbnail must be an image.');
            if (f.size > MAX_THUMB) errors.push('Thumbnail exceeds ' + (MAX_THUMB / (1024 * 1024)) + ' MB.');
        }

        if (previewInput) {
            previewInput.files.forEach(function (f) {
                if (!f.type.startsWith('image/')) errors.push('Preview file "' + f.name + '" must be an image.');
                if (f.size > MAX_PREVIEW) errors.push('Preview file "' + f.name + '" exceeds ' + (MAX_PREVIEW / (1024 * 1024)) + ' MB.');
            });
        }

        if (trailerInput) {
            trailerInput.files.forEach(function (f) {
                if (!f.type.startsWith('video/')) errors.push('Trailer "' + f.name + '" must be a video.');
                if (f.size > MAX_TRAILER) errors.push('Trailer "' + f.name + '" exceeds ' + (MAX_TRAILER / (1024 * 1024)) + ' MB.');
            });
        }

        if (zipInput && zipInput.files.length > 0) {
            var f = zipInput.files[0];
            var ext = (f.name.split('.').pop() || '').toLowerCase();
            if (ext !== 'zip') errors.push('Game package must be a .zip file.');
            if (f.size > MAX_ZIP) errors.push('Game package exceeds ' + (MAX_ZIP / (1024 * 1024)) + ' MB.');
        }

        if (errors.length) {
            console.warn('[dev] validation failed, preventing submit:', errors);
            ev.preventDefault();
            showErrors(errors);
        } else {
            console.log('[dev] validation passed, allowing submit to proceed');
            // allow submit — do not call preventDefault
        }
    }, { passive: false });
})();

// Game stats chart on game details page
(function () {
    var canvas = document.getElementById('gameStatsChart');
    if (!canvas) return;
    var ctx = canvas.getContext('2d');
    var chart = null;

    function fetchAndRender(hours) {
        var gameId = canvas.getAttribute('data-gameid');
        var url = '/Developer/GetGameData?id=' + encodeURIComponent(gameId) + '&hours=' + (hours || 48);
        fetch(url, { cache: 'no-store' }).then(function (res) {
            if (!res.ok) return Promise.reject(res.statusText);
            return res.json();
        }).then(function (payload) {
            payload.exposure = (payload.exposure || []).map(Number);
            payload.sales = (payload.sales || []).map(Number);
            payload.revenue = (payload.revenue || []).map(Number);

            var datasets = [
                { label: 'Exposure', data: payload.exposure, borderColor: '#b6ff3e', backgroundColor: 'transparent', tension: 0.2, pointRadius: 3 },
                { label: 'Sales', data: payload.sales, borderColor: '#3eeaff', backgroundColor: 'transparent', tension: 0.2, pointRadius: 3 },
                { label: 'Revenue', data: payload.revenue, borderColor: '#c06cff', backgroundColor: 'transparent', tension: 0.2, pointRadius: 3 }
            ];

            if (chart) chart.destroy();
            chart = new Chart(ctx, {
                type: 'line',
                data: { labels: payload.labels || [], datasets: datasets },
                options: {
                    animation: { duration: 600 },
                    responsive: true,
                    maintainAspectRatio: false,
                    scales: {
                        x: { grid: { color: 'rgba(255,255,255,0.04)' }, ticks: { color: '#bbb' } },
                        y: { grid: { color: 'rgba(255,255,255,0.04)' }, ticks: { color: '#bbb' }, beginAtZero: true }
                    },
                    plugins: { legend: { labels: { color: '#ddd' } } },
                    interaction: { mode: 'index', intersect: false },
                    elements: { line: { borderWidth: 2 } }
                }
            });
        }).catch(function (err) {
            console.warn('Failed to load game data', err);
        });
    }

    // wire zoom buttons
    var buttons = document.querySelectorAll('.dashboard-controls .btn[data-hours]');
    buttons.forEach(function (b) {
        b.addEventListener('click', function () {
            buttons.forEach(function (x) { x.classList.remove('active'); });
            b.classList.add('active');
            fetchAndRender(parseInt(b.getAttribute('data-hours')));
        });
    });

    // Zoom buttons (data-hours) — toggle .active and reload data
    //var hourButtons = document.querySelectorAll('.dashboard-controls .btn[data-hours]');
    ////console.log(hourButtons);
    //if (hourButtons && hourButtons.length > 0) {
    //    hourButtons.forEach(function (btn) {
    //        btn.addEventListener('click', function () {
    //            hourButtons.forEach(function (b) { b.classList.remove('active'); });
    //            btn.classList.add('active');
    //            var h = parseInt(btn.getAttribute('data-hours')) || 48;
    //            loadData(h);
    //        });
    //    });
    //}

    // data-type buttons handling: exclusive selection, show/hide datasets
    function applyDataTypeSelection() {
        //console.log('Applying data type selection');
        var active = document.querySelector('.dashboard-controls .btn[data-type].active');
        var type = active ? active.getAttribute('data-type') : 'all';
        console.log(chart);
        if (!chart) return;
        chart.data.datasets.forEach(function (ds) {
            var label = ((ds.label || '') + '').toLowerCase();
            if (type === 'all') ds.hidden = false;
            else ds.hidden = (label !== type);
        });
        chart.update();
    }

    var typeButtons = document.querySelectorAll('.dashboard-controls .btn[data-type]');
    console.log(typeButtons);
    if (typeButtons && typeButtons.length > 0) {
        typeButtons.forEach(function (btn) {
            btn.addEventListener('click', function () {
                typeButtons.forEach(function (b) { b.classList.remove('active'); });
                btn.classList.add('active');
                applyDataTypeSelection();
            });
        });
    }

    // initial load
    fetchAndRender(48);
})();

// developer game details page
// simple thumb -> main media switch (reuses same data-src attributes)
(function () {
    var main = document.getElementById('dev-main-media');
    var thumbs = document.getElementById('dev-gd-thumbs');
    if (!main || !thumbs) return;

    // 【新增函数】使用 Canvas 从视频 URL 生成缩略图 URL
    function generateVideoThumbnail(videoUrl, callback) {
        var video = document.createElement('video');
        video.src = videoUrl;
        video.crossOrigin = 'anonymous'; // 解决跨域限制（如果视频和网站不在同一域名）

        video.onloadeddata = function () {
            // 确保视频有足够的时间加载到第一帧
            video.currentTime = 0.5; // 捕获 0.5 秒那一帧
        };

        video.onseeked = function () {
            // 视频定位到指定时间后触发
            var canvas = document.createElement('canvas');
            canvas.width = video.videoWidth;
            canvas.height = video.videoHeight;

            var ctx = canvas.getContext('2d');
            ctx.drawImage(video, 0, 0, canvas.width, canvas.height);

            // 将 Canvas 内容转换为 Data URL (Base64 图片)
            var thumbnailUrl = canvas.toDataURL('image/jpeg');

            // 返回生成的 URL
            callback(thumbnailUrl);

            // 清理内存
            video.remove();
            canvas.remove();
        };

        video.onerror = function () {
            // 如果加载视频失败（例如格式不支持或跨域问题），返回一个占位图 URL
            callback('/images/placeholder.png');
            video.remove();
        };

        // 必须开始加载视频
        video.load();
    }

    // 主媒体切换逻辑保持不变
    thumbs.addEventListener('click', function (e) {
        var btn = e.target.closest('.dev-gd-thumb');
        if (!btn) return;
        var src = btn.getAttribute('data-src');
        var type = btn.getAttribute('data-type');

        // replace main media content
        if (type === 'video') {
            main.innerHTML = '<video controls autoplay style="width:100%;height:100%;object-fit:cover"><source src="' + src + '"></video>';
        } else {
            main.innerHTML = '<img src="' + src + '" alt="" style="width:100%;height:100%;object-fit:cover" />';
        }
    });

    // 【新增】初始化逻辑：生成所有视频缩略图
    document.addEventListener('DOMContentLoaded', function () {
        var videoThumbs = thumbs.querySelectorAll('.dev-gd-thumb[data-type="video"]');

        videoThumbs.forEach(function (btn) {
            var videoUrl = btn.getAttribute('data-src');
            var imgElement = btn.querySelector('img');

            if (imgElement && videoUrl) {
                // 确保 img 元素是空的（如果 Razor 渲染了占位图，就覆盖它）
                imgElement.src = '';

                // 异步生成缩略图
                generateVideoThumbnail(videoUrl, function (thumbnailUrl) {
                    // 更新 img 元素的 src
                    imgElement.src = thumbnailUrl;
                    // 可选：移除视频的播放图标占位
                    var playIcon = btn.querySelector('.play-icon');
                    if (playIcon) playIcon.style.display = 'none';
                });
            }
        });
    });
})();


// Developer preview remove buttons
// simple client-side handling for existing preview remove: mark element as removed and remove from DOM
(function () {
    document.addEventListener('click', function (e) {
        var btn = e.target.closest('.preview-remove-btn');
        if (!btn) return;
        var item = btn.closest('.preview-item');
        if (!item) return;

        // mark removed to be processed by server if you implement server-side removal; for now remove visually
        item.dataset.removed = "1";
        item.remove();
    });
})();

// Payment method selection handling
// 确保这段代码在 developer.js 中，并且包裹在 DOMContentLoaded 事件中
document.addEventListener('DOMContentLoaded', function () {
    // 1. 获取所有支付按钮元素和隐藏输入框元素
    const paymentButtons = document.querySelectorAll('.btn-payment-option');
    const hiddenInput = document.getElementById('hiddenPaymentMethod');

    // 如果页面上没有这些元素（例如不在支付页面时），则直接返回，避免报错
    if (paymentButtons.length === 0 || !hiddenInput) {
        return;
    }

    // 2. 为每个按钮添加点击事件监听器
    paymentButtons.forEach(button => {
        button.addEventListener('click', function () {
            // a. 重置：移除所有按钮的 'active' 类名 (变回普通颜色)
            paymentButtons.forEach(btn => btn.classList.remove('active'));

            // b. 高亮：给当前被点击的按钮添加 'active' 类名 (变粉色)
            this.classList.add('active');

            // c. 更新数据：获取被点击按钮的 data-value 值，赋给隐藏输入框
            const selectedValue = this.getAttribute('data-value');
            hiddenInput.value = selectedValue;

            // (可选) 打印日志方便调试确认值是否改变
            console.log("Selected payment method:", hiddenInput.value);
        });
    });
});

$(document).ready(function () {
    //// 缓存选择器
    //var $countrySelect = $('#CountrySelect');
    //var $stateContainer = $('#StateContainer');
    //var $stateSelect = $('#State');

    //// 定义切换显示状态的函数
    //function toggleStateField() {
    //    // 检查选中的值是否为 'Malaysia'
    //    if ($countrySelect.val() === 'Malaysia') {
    //        $stateContainer.slideDown(200); // 慢速显示
    //    } else {
    //        $stateContainer.slideUp(200);   // 慢速隐藏

    //        // 【重要】隐藏时清空 State 的值，防止提交无效数据或触发错误的后端验证
    //        $stateSelect.val('');
    //    }
    //}

    //// 1. 页面加载时执行一次 (处理浏览器回退或默认值)
    //toggleStateField();

    //// 2. 当 Country 下拉列表改变时，执行切换函数
    //$countrySelect.on('change', function () {
    //    toggleStateField();
    //});

    //// 告诉 Select2 库将 ID 为 'StateDropdown' 的元素转换为可搜索的浮动列表
    ////$('#StateDropdown').select2();
    //$('#StateDropdown').select2({
    //    width: '100%',
    //    placeholder: '-- Select State/Territory --',
    //    allowClear: true,
    //});

    //// 隐藏原生 <select> （Select2 会替代它显示浮动框）
    //$('#StateDropdown').hide();

    // 定义每个国家对应的 State/Province 列表
    // 当 Country 改变时更新 State
    const statesByCountry = {
        "Malaysia": [
            "Johor", "Kedah", "Kelantan", "Melaka", "Negeri Sembilan",
            "Pahang", "Perak", "Perlis", "Pulau Pinang", "Sabah",
            "Sarawak", "Selangor", "Terengganu", "Kuala Lumpur", "Putrajaya"
        ],
        "United States": ["California", "New York", "Texas", "Florida", "..."],
        "Singapore": ["Central", "East", "North", "North-East", "West"]
    };

    const $countrySelect = $('#CountrySelect');
    const $stateSelect = $('#StateSelect');

    // 当 Country 改变时更新 State
    $countrySelect.on('change', function () {

        $('#StateHidden').val($(this).val());

        const country = $(this).val();
        const states = statesByCountry[country] || [];

        // 清空 State 下拉
        $stateSelect.empty();

        // 添加默认空选项
        $stateSelect.append('<option value="">-- Select State/Territory --</option>');

        // 添加新的选项
        states.forEach(function (state) {
            $stateSelect.append(`<option value="${state}">${state}</option>`);
        });
    });

    // 页面加载时触发一次更新（用于默认值或回退）
    $countrySelect.trigger('change');

    // ----------------- 原有按钮点击逻辑 -----------------
    $('.btn-payment-option').on('click', function () {
        var $clickedButton = $(this);
        var targetId = $clickedButton.data('target');
        var paymentValue = $clickedButton.data('value');

        // 切换按钮 active 样式
        $('.btn-payment-option').removeClass('active');
        $clickedButton.addClass('active');

        // 显示对应详情
        $('.method-details').addClass('hidden');
        $(targetId).removeClass('hidden');

        // 更新隐藏字段的值
        $('#HiddenPaymentMethod').val(paymentValue);

        console.log("Selected payment method:", paymentValue);
    });

    // 1. 获取隐藏字段的值（后端渲染进来的 SelectedPaymentMethod）
    var selectedMethod = $('#HiddenPaymentMethod').val();

    if (selectedMethod) {
        // 2. 找到对应按钮
        var $btn = $('.btn-payment-option[data-value="' + selectedMethod + '"]');

        if ($btn.length) {
            // 3. 触发点击事件，执行切换逻辑
            console.log("Auto-selecting payment method button:", $btn);
            $btn.click();
        }
    }
});