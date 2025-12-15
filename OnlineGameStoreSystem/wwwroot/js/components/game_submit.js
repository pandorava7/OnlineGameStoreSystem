document.getElementById('publishForm').addEventListener('submit', function (e) {
    // 1. 找到 tag 容器
    const activeFilters = document.getElementById('activeFilters');
    const pills = activeFilters.querySelectorAll('.filter-pill');

    // 2. 清空之前生成的 hidden inputs
    const hiddenContainer = document.getElementById('tagHiddenInputsContainer');
    hiddenContainer.innerHTML = '';

    // 3. 提取文本并生成 hidden input
    pills.forEach((pill, index) => {
        // pill.textContent 包含 “✕”，去掉最后一个字符（或用正则）
        let text = pill.textContent.trim();
        text = text.replace(/\u2715$/, '').trim(); // ✕ 的 unicode 是 2715
        if (text) {
            const input = document.createElement('input');
            input.type = 'hidden';
            input.name = `Tags[${index}]`; // MVC 会正确绑定成 List<string>
            input.value = text;
            hiddenContainer.appendChild(input);
        }
    });

    // 4. 可选：验证数量
    const count = pills.length;
    if (count < 1 || count > 10) {
        console.log("Tag count validation failed:", count);
        //e.preventDefault(); // 阻止提交
        //alert("Please select between 1 and 10 tags.");
        //return false;
    }

    // 5. 提交表单，MVC 后端会自动把 Tags[] 解析为 List<string>
    console.log("Form submitted with", count, "tags.");
    //e.preventDefault(); // 阻止提交
});



// 初始化
document.addEventListener('DOMContentLoaded', function () {

    const tagInput = document.getElementById("tagInput");
    const tagSuggestions = document.getElementById("tagSuggestions");
    const activeFilters = document.getElementById("activeFilters");

    let activeTags = [];

    function renderActiveTags() {
        if (!activeFilters) return;
        activeFilters.innerHTML = '';
        activeTags.forEach(t => {
            const pill = document.createElement('span');
            pill.className = 'filter-pill';
            pill.innerHTML = `${t} <button aria-label="remove-${t}">✕</button>`;
            pill.querySelector('button')?.addEventListener('click', (e) => {
                // 阻止触发父元素的事件
                e.preventDefault();
                removeTag(t)
            });
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

    // 找到currentTags元素
    const currentTags = document.getElementById("currentTags");
    if (currentTags) {
        // 获取current-tag，提取文本内容并addTag
        const existingTags = currentTags.querySelectorAll(".current-tag");
        existingTags.forEach(tagElem => {
            const tagText = tagElem.textContent.trim();
            addTag(tagText);
        });
    }
});

