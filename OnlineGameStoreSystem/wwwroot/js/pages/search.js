document.addEventListener('DOMContentLoaded', () => {
    // ----------------- 价格滑动条逻辑 -----------------
    const priceRange = document.getElementById('priceRange');
    const priceValue = document.getElementById('priceValue');
    const priceLabels = ['under RM15', 'under RM30', 'under RM60', 'under RM150', 'under RM300', 'Any Price'];

    function syncPriceLabel() {
        const idx = Number(priceRange.value);
        priceValue.textContent = priceLabels[idx];
    }
    priceRange.addEventListener('input', syncPriceLabel);
    // 默认设置为 Any Price（最右）
    syncPriceLabel();

    // 切换开关通用函数
    function makeSwitch(id) {
        const el = document.getElementById(id);
        el.addEventListener('click', () => {
            el.classList.toggle('on');
            // 这里可以触发过滤逻辑，比如发事件或调用搜索
            // console.log(id, el.classList.contains('on'))
        });
    }
    ['hideFreeSwitch', 'onlyDiscountSwitch', 'hideOwnedSwitch', 'hideWishlistSwitch'].forEach(makeSwitch);

    // ----------------- 标签筛选逻辑（示例，本地 mock 数据） -----------------
    const tagInput = document.getElementById('tagInput');
    const tagSuggestions = document.getElementById('tagSuggestions');
    const activeFilters = document.getElementById('activeFilters');
    const tagAddBtn = document.getElementById('tagAddBtn');

    // mock 标签库，实际应由后端提供
    let ALL_TAGS = []; // 先空数组

    // 页面加载时获取所有标签
    async function fetchAllTags() {
        try {
            const res = await fetch('/api/tag');
            if (!res.ok) throw new Error('Failed to fetch tags');
            ALL_TAGS = await res.json();
        } catch (err) {
            console.error(err);
        }
    }

    // 先调用获取标签
    fetchAllTags();

    let activeTags = [];

    function renderSuggestions(filter) {
        tagSuggestions.innerHTML = '';
        if (!filter) return;
        const q = filter.toLowerCase();
        const matched = ALL_TAGS.filter(t => t.toLowerCase().includes(q) && !activeTags.includes(t));
        matched.slice(0, 8).forEach(t => {
            const pill = document.createElement('div');
            pill.className = 'tag-pill';
            pill.textContent = t;
            pill.addEventListener('click', () => addTag(t));
            tagSuggestions.appendChild(pill);
        });
    }

    function renderActiveTags() {
        activeFilters.innerHTML = '';
        activeTags.forEach(t => {
            const pill = document.createElement('span');
            pill.className = 'filter-pill';
            pill.innerHTML = `${t} <button aria-label="remove-${t}">✕</button>`;
            pill.querySelector('button').addEventListener('click', () => {
                removeTag(t);
            });
            activeFilters.appendChild(pill);
        });
    }

    function addTag(tag) {
        if (!activeTags.includes(tag)) {
            activeTags.push(tag);
            renderActiveTags();
            tagInput.value = '';
            tagSuggestions.innerHTML = '';
            // TODO: 触发搜索/筛选逻辑
        }
    }
    function removeTag(tag) {
        activeTags = activeTags.filter(t => t !== tag);
        renderActiveTags();
        // TODO: 触发搜索/筛选逻辑
    }

    tagInput.addEventListener('input', (e) => {
        renderSuggestions(e.target.value);
    });
    tagInput.addEventListener('keydown', (e) => {
        if (e.key === 'Enter') {
            e.preventDefault();
            // 如果输入正好匹配已有标签或是自定义标签都允许添加
            const val = tagInput.value.trim();
            if (!val) return;
            // 优先匹配现有标签（大小写不敏感）
            const found = ALL_TAGS.find(t => t.toLowerCase() === val.toLowerCase());
            addTag(found || val);
        }
    });
    tagAddBtn.addEventListener('click', () => {
        const val = tagInput.value.trim(); if (!val) return; const found = ALL_TAGS.find(t => t.toLowerCase() === val.toLowerCase()); addTag(found || val);
    });

    // ----------------- Search Trigger -----------------
    const mainSearchInput = document.getElementById('searchInput');
    const mainSearchBtn = document.getElementById('searchSubmit');

    function doSearch() {
        const term = mainSearchInput.value.trim();
        const params = new URLSearchParams();

        if (term) params.set('term', term);
        // price
        params.set('price', priceLabels[Number(priceRange.value)]);
        // toggles
        params.set('hideFree', document.getElementById('hideFreeSwitch').classList.contains('on'));
        params.set('onlyDiscount', document.getElementById('onlyDiscountSwitch').classList.contains('on'));
        params.set('hideOwned', document.getElementById('hideOwnedSwitch').classList.contains('on'));
        params.set('hideWishlist', document.getElementById('hideWishlistSwitch').classList.contains('on'));
        if (activeTags.length) params.set('tags', activeTags.join(','));

        // Do searching query
        const url = '/Search?' + params.toString();
        console.log('search url ->', url);
        window.location.href = url;
    }
    mainSearchBtn.addEventListener('click', doSearch);
    mainSearchInput.addEventListener('keydown', (e) => { if (e.key === 'Enter') { doSearch(); } });


    // ----------------- 恢复页面状态 -----------------
    function restoreSearchState(params) {

        // -------- 恢复搜索词 --------
        if (params.has('term') && window.mainSearchInput) {
            mainSearchInput.value = params.get('term');
        }

        // -------- 恢复价格范围 --------
        if (params.has('price') && window.priceLabels && window.priceRange) {
            const priceValue = params.get('price');
            const index = priceLabels.indexOf(priceValue);
            if (index >= 0) {
                priceRange.value = index;

                // 若你已有更新 label 的函数，这里调用它
                if (typeof updatePriceLabel === "function") {
                    updatePriceLabel();
                }
            }
        }

        // -------- 恢复 toggle 开关 --------
        const toggleKeys = [
            "hideFree",
            "onlyDiscount",
            "hideOwned",
            "hideWishlist"
        ];

        toggleKeys.forEach(key => {
            if (params.has(key) && params.get(key) === "true") {
                const el = document.getElementById(key + "Switch");
                if (el) el.classList.add("on");
            }
        });

        // -------- 恢复 tag --------
        if (params.has("tags") && typeof addTag === "function") {
            const tagList = params.get("tags").split(",");
            tagList.forEach(tag => {
                tag = tag.trim();
                if (tag) addTag(tag);
            });
        }
    }

    const params = new URLSearchParams(window.location.search);
    restoreSearchState(params);
});