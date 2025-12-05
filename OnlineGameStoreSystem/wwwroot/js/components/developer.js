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

// Dashboard chart (Chart.js) with mock fallback and other app logic
(function () {
    function ensureChartAndInit() {
        if (typeof Chart === 'undefined') {
            setTimeout(ensureChartAndInit, 200);
            return;
        }

        var canvas = document.getElementById('devStatsChart');
        var ctx = canvas ? canvas.getContext('2d') : null;
        var chart = null;

        function generateMockData(hours) {
            var points = 12;
            var labels = [];
            var exposure = [];
            var sales = [];
            var revenue = [];

            var now = new Date();
            var totalMs = (hours > 0 ? hours : 48) * 60 * 60 * 1000;
            var step = totalMs / points;

            for (var i = points - 1; i >= 0; i--) {
                var d = new Date(now.getTime() - (i * step));
                labels.push(hours <= 48 ? d.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' }) : d.toLocaleDateString());
                var s = Math.max(0, Math.round(3 + Math.sin(i * 0.8) * 6 + i));
                var r = +(s * (5 + (i % 5))).toFixed(2);
                var e = s * 200 + (i * 8);
                sales.push(s);
                revenue.push(r);
                exposure.push(e);
            }

            return { labels: labels, exposure: exposure, sales: sales, revenue: revenue };
        }

        async function loadData(hours) {
            if (!ctx) return;
            var url = '/Developer/GetDashboardData?hours=' + (hours || 48);
            var payload = null;
            try {
                var res = await fetch(url, { cache: 'no-store' });
                if (res.ok) payload = await res.json();
            } catch (err) {
                console.warn('Failed to fetch dashboard data, using mock. Error:', err);
            }

            if (!payload || !Array.isArray(payload.labels) || payload.labels.length === 0) {
                payload = generateMockData(hours || 48);
            }

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
                data: { labels: payload.labels, datasets: datasets },
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

            applyDataTypeSelection();
        }

        // Zoom buttons (data-hours) — toggle .active and reload data
        var hourButtons = document.querySelectorAll('.dashboard-controls .btn[data-hours]');
        if (hourButtons && hourButtons.length > 0) {
            hourButtons.forEach(function (btn) {
                btn.addEventListener('click', function () {
                    hourButtons.forEach(function (b) { b.classList.remove('active'); });
                    btn.classList.add('active');
                    var h = parseInt(btn.getAttribute('data-hours')) || 48;
                    loadData(h);
                });
            });
        }

        // data-type buttons handling: exclusive selection, show/hide datasets
        function applyDataTypeSelection() {
            var active = document.querySelector('.dashboard-controls .btn[data-type].active');
            var type = active ? active.getAttribute('data-type') : 'all';
            if (!chart) return;
            chart.data.datasets.forEach(function (ds) {
                var label = ((ds.label || '') + '').toLowerCase();
                if (type === 'all') ds.hidden = false;
                else ds.hidden = (label !== type);
            });
            chart.update();
        }

        var typeButtons = document.querySelectorAll('.dashboard-controls .btn[data-type]');
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
        loadData(48);
    }

    ensureChartAndInit();
})();

// Image preview helpers (use Object URLs for file previews)
(function () {
    function el(tag, cls) { var e = document.createElement(tag); if (cls) e.className = cls; return e; }

    // Preview images input (max 8)
    (function () {
        var input = document.getElementById('previewImagesInput');
        var grid = document.getElementById('previewGrid');
        if (!input || !grid) return;

        // revoke map to track object URLs created by this script
        var urlMap = new Map();

        function clearGrid() {
            urlMap.forEach(function (url) { try { URL.revokeObjectURL(url); } catch (e) { } });
            urlMap.clear();
            while (grid.firstChild) grid.removeChild(grid.firstChild);
        }

        function addImagePreviewFromFile(file) {
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
            btn.addEventListener('click', function (e) {
                e.preventDefault();
                var url = urlMap.get(item);
                if (url) try { URL.revokeObjectURL(url); } catch (er) { }
                urlMap.delete(item);
                item.remove();
            });
            item.appendChild(btn);

            grid.appendChild(item);
        }

        // Accept drag-drop on dashed-dropzone label (if present)
        var dropzone = document.querySelector('.dashed-dropzone[for="previewImagesInput"]');
        if (dropzone) {
            dropzone.addEventListener('dragover', function (e) { e.preventDefault(); dropzone.classList.add('dragover'); });
            dropzone.addEventListener('dragleave', function () { dropzone.classList.remove('dragover'); });
            dropzone.addEventListener('drop', function (e) {
                e.preventDefault();
                dropzone.classList.remove('dragover');
                var files = Array.from(e.dataTransfer.files || []).filter(f => f.type.startsWith('image/'));
                // append files to input.files is tricky; handle directly
                files.slice(0, 8).forEach(addImagePreviewFromFile);
            });
        }

        input.addEventListener('change', function () {
            // If user explicitly selects files, clear existing previews and show new ones
            var files = Array.from(input.files || []).filter(f => f.type.startsWith('image/'));
            // enforce max 8
            if (files.length > 8) files = files.slice(0, 8);

            clearGrid();
            files.forEach(addImagePreviewFromFile);
        });

        // expose clearGrid for possible use (not required)
        window._devPreviewImagesClear = clearGrid;
    })();

    // Trailer preview (videos) input (max 2)
    (function () {
        var input = document.getElementById('trailersInput');
        var grid = document.getElementById('trailerGrid');
        if (!input || !grid) return;

        var urlMap = new Map();

        function clearGrid() {
            urlMap.forEach(function (url) { try { URL.revokeObjectURL(url); } catch (e) { } });
            urlMap.clear();
            while (grid.firstChild) grid.removeChild(grid.firstChild);
        }

        function addVideoPreviewFromFile(file) {
            var item = el('div', 'preview-item');
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

            var btn = el('button', 'preview-remove-btn');
            btn.type = 'button';
            btn.textContent = '✖';
            btn.title = 'Remove';
            btn.addEventListener('click', function (e) {
                e.preventDefault();
                var url = urlMap.get(item);
                if (url) try { URL.revokeObjectURL(url); } catch (er) { }
                urlMap.delete(item);
                item.remove();
            });
            item.appendChild(btn);

            grid.appendChild(item);
        }

        var dropzone = document.querySelector('.dashed-dropzone[for="trailersInput"]');
        if (dropzone) {
            dropzone.addEventListener('dragover', function (e) { e.preventDefault(); dropzone.classList.add('dragover'); });
            dropzone.addEventListener('dragleave', function () { dropzone.classList.remove('dragover'); });
            dropzone.addEventListener('drop', function (e) {
                e.preventDefault();
                dropzone.classList.remove('dragover');
                var files = Array.from(e.dataTransfer.files || []).filter(f => f.type.startsWith('video/'));
                // show up to 2
                files.slice(0, 2).forEach(addVideoPreviewFromFile);
            });
        }

        input.addEventListener('change', function () {
            var files = Array.from(input.files || []).filter(f => f.type.startsWith('video/'));
            if (files.length > 2) files = files.slice(0, 2);

            clearGrid();
            files.forEach(addVideoPreviewFromFile);
        });

        window._devTrailerClear = clearGrid;
    })();
})();


// Client-side upload validation (images/videos/zip) — debug-enabled (fixed insertBefore crash)
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

    // initial load
    fetchAndRender(48);
})();

// developer game details page
// simple thumb -> main media switch (reuses same data-src attributes)
(function () {
    var main = document.getElementById('dev-main-media');
    var thumbs = document.getElementById('dev-gd-thumbs');
    if (!main || !thumbs) return;

    thumbs.addEventListener('click', function (e) {
        var btn = e.target.closest('.dev-gd-thumb');
        if (!btn) return;
        var src = btn.getAttribute('data-src');
        var type = btn.getAttribute('data-type');
        // replace main media content
        if (type === 'video') {
            main.innerHTML = '<video controls style="width:100%;height:100%;object-fit:cover"><source src="' + src + '"></video>';
        } else {
            main.innerHTML = '<img src="' + src + '" alt="" style="width:100%;height:100%;object-fit:cover" />';
        }
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