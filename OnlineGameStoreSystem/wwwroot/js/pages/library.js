function initLibraryFilters(config) {
    // --------- 价格滑条 ---------
    const priceRange = document.getElementById(config.priceRangeId);
    const priceValue = document.getElementById(config.priceValueId);
    const priceLabels = config.priceLabels || [];

    if (priceRange && priceValue && priceLabels.length) {
        function syncPriceLabel() {
            const idx = Number(priceRange.value);
            priceValue.textContent = priceLabels[idx] || '';
        }
        priceRange.addEventListener('input', syncPriceLabel);
        syncPriceLabel();
    }

    // --------- Toggle Switches ---------
    if (config.toggleIds && Array.isArray(config.toggleIds)) {
        config.toggleIds.forEach(id => {
            const el = document.getElementById(id);
            if (!el) return;
            el.addEventListener('click', () => el.classList.toggle('on'));
        });
    }

    // --------- Tag 筛选 ---------
    const tagInput = document.getElementById(config.tagInputId);
    const tagAddBtn = document.getElementById(config.tagAddBtnId);
    const tagSuggestions = document.getElementById(config.tagSuggestionsId);
    const activeFilters = document.getElementById(config.activeFiltersId);

    let ALL_TAGS = [];
    let activeTags = [];

    async function fetchAllTags(url = '/api/tag') {
        try {
            const res = await fetch(url);
            if (!res.ok) throw new Error('Failed to fetch tags');
            ALL_TAGS = await res.json();
        } catch (err) {
            console.error(err);
        }
    }
    fetchAllTags();

    function renderSuggestions(filter) {
        if (!tagSuggestions) return;
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
        if (!activeFilters) return;
        activeFilters.innerHTML = '';
        activeTags.forEach(t => {
            const pill = document.createElement('span');
            pill.className = 'filter-pill';
            pill.innerHTML = `${t} <button aria-label="remove-${t}">✕</button>`;
            pill.querySelector('button')?.addEventListener('click', () => removeTag(t));
            activeFilters.appendChild(pill);
        });
    }

    function addTag(tag) {
        if (!tag || activeTags.includes(tag)) return;
        activeTags.push(tag);
        renderActiveTags();
        if (tagInput) tagInput.value = '';
        if (tagSuggestions) tagSuggestions.innerHTML = '';
    }

    function removeTag(tag) {
        if (!tag) return;
        activeTags = activeTags.filter(t => t !== tag);
        renderActiveTags();
    }

    if (tagInput) {
        tagInput.addEventListener('input', (e) => renderSuggestions(e.target.value));
        tagInput.addEventListener('keydown', (e) => {
            if (e.key === 'Enter') {
                e.preventDefault();
                const val = tagInput.value.trim();
                if (!val) return;
                const found = ALL_TAGS.find(t => t.toLowerCase() === val.toLowerCase());
                addTag(found || val);
            }
        });
    }

    if (tagAddBtn) {
        tagAddBtn.addEventListener('click', () => {
            const val = tagInput?.value.trim();
            if (!val) return;
            const found = ALL_TAGS.find(t => t.toLowerCase() === val.toLowerCase());
            addTag(found || val);
        });
    }

    // --------- Filter 按钮选择逻辑 ---------
    const filterOptions = document.querySelectorAll(config.filterOptionsSelector || '');
    let currentSort = ''; // 当前排序字段
    let currentOrder = 'asc'; // asc / desc

    if (filterOptions.length) {
        filterOptions.forEach(btn => {
            btn.addEventListener('click', () => {
                filterOptions.forEach(b => b.classList.remove('active'));
                btn.classList.add('active');
                currentSort = btn.dataset.sort || btn.textContent.trim().toLowerCase();
            });
        });
    }

    // --------- 排序倒序切换按钮 ---------
    const sortToggleBtn = document.getElementById(config.sortToggleBtnId);
    if (sortToggleBtn) {
        sortToggleBtn.addEventListener('click', () => {
            currentOrder = currentOrder === 'asc' ? 'desc' : 'asc';
            sortToggleBtn.textContent = currentOrder === 'asc' ? '⬆' : '⬇';
        });
    }

    // --------- 搜索触发 ---------
    const mainSearchInput = document.getElementById(config.searchInputId);
    const mainSearchBtn = document.getElementById(config.searchBtnId);

    function doSearch() {
        if (!mainSearchInput) return;
        const term = mainSearchInput.value.trim();
        const params = new URLSearchParams();

        if (term) params.set('term', term);
        if (priceRange && priceLabels.length) params.set('price', priceLabels[Number(priceRange.value)]);

        if (config.toggleIds && Array.isArray(config.toggleIds)) {
            config.toggleIds.forEach(id => {
                const el = document.getElementById(id);
                if (el) params.set(id.replace('Switch', ''), el.classList.contains('on'));
            });
        }

        if (activeTags.length) params.set('tags', activeTags.join(','));

        if (currentSort) params.set('sort', currentSort);
        if (currentOrder) params.set('order', currentOrder);

        const url = config.searchUrl + '?' + params.toString();
        console.log('Search URL:', url);
        window.location.href = url;
    }

    if (mainSearchBtn) mainSearchBtn.addEventListener('click', doSearch);
    if (mainSearchInput) mainSearchInput.addEventListener('keydown', (e) => { if (e.key === 'Enter') doSearch(); });

    return {
        addTag,
        removeTag,
        fetchAllTags,
        activeTags,
        doSearch
    };
}

function restoreLibraryFiltersState(libFilters, config) {
    if (!libFilters || !config) return;

    const params = new URLSearchParams(window.location.search);

    // -------- 恢复搜索词 --------
    if (params.has('term')) {
        const inputEl = document.getElementById(config.searchInputId);
        if (inputEl) inputEl.value = params.get('term');
    }

    // -------- 恢复价格范围 --------
    if (params.has('price') && config.priceLabels && config.priceRangeId) {
        const priceValue = params.get('price');
        const index = config.priceLabels.indexOf(priceValue);
        const priceEl = document.getElementById(config.priceRangeId);
        if (index >= 0 && priceEl) priceEl.value = index;
        // 同步显示 label
        if (typeof priceEl?.dispatchEvent === "function") {
            priceEl.dispatchEvent(new Event('input'));
        }
    }

    // -------- 恢复 toggle 开关 --------
    if (config.toggleIds && Array.isArray(config.toggleIds)) {
        config.toggleIds.forEach(id => {
            const key = id.replace('Switch', '');
            if (params.has(key) && params.get(key) === "true") {
                const el = document.getElementById(id);
                if (el) el.classList.add('on');
            }
        });
    }

    // -------- 恢复 tag --------
    if (params.has("tags")) {
        const tagStr = params.get("tags");
        if (tagStr && typeof libFilters.addTag === "function") {
            tagStr.split(",").forEach(tag => {
                tag = tag.trim();
                if (tag) libFilters.addTag(tag);
            });
        }
    }

    // -------- 恢复 filtering 按钮 --------
    if (config.filterOptionsSelector) {
        const filterBtns = document.querySelectorAll(config.filterOptionsSelector);
        const sort = params.get("sort");
        console.log(sort);
        if (sort) {
            filterBtns.forEach(btn => {
                btn.classList.remove("active");
                const btnSort = btn.dataset.sort || btn.textContent.trim().toLowerCase();
                if (btnSort === sort.toLowerCase()) {
                    btn.classList.add("active");
                }
            });
        }
    }

    // -------- 恢复排序顺序 --------
    const sortToggleBtn = document.getElementById(config.sortToggleBtnId);
    const order = params.get("order");
    if (sortToggleBtn && order) {
        libFilters.currentOrder = order;
        sortToggleBtn.textContent = order === 'asc' ? '⬆' : '⬇';
    }
}

document.addEventListener('DOMContentLoaded', () => {
    const libFilters = initLibraryFilters({
        searchInputId: 'searchInput',
        searchBtnId: 'searchSubmit',
        searchUrl: '/library',
        priceRangeId: 'priceRange',
        priceValueId: 'priceValue',
        priceLabels: ['under RM15', 'under RM30', 'under RM60', 'under RM150', 'under RM300', 'Any Price'],
        toggleIds: ['hideFreeSwitch', 'onlyDiscountSwitch', 'hideOwnedSwitch', 'hideWishlistSwitch'],
        tagInputId: 'tagInput',
        tagAddBtnId: 'tagAddBtn',
        tagSuggestionsId: 'tagSuggestions',
        activeFiltersId: 'activeFilters',
        filterOptionsSelector: '.filter-section .option'
    });

    // 外部也可以直接调用：
    // libFilters.addTag('RPG');

    // 恢复页面状态
    restoreLibraryFiltersState(libFilters, {
        searchInputId: 'searchInput',
        priceRangeId: 'priceRange',
        priceLabels: ['under RM15', 'under RM30', 'under RM60', 'under RM150', 'under RM300', 'Any Price'],
        toggleIds: ['hideFreeSwitch', 'onlyDiscountSwitch', 'hideOwnedSwitch', 'hideWishlistSwitch'],
        filterOptionsSelector: '.filter-section .option',
        sortToggleBtnId: 'sortToggleBtn'
    });
});
