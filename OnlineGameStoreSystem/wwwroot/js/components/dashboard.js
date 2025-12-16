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

        async function loadData(hours) {
            console.log('Loading dashboard data for last', hours, 'hours');
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
            payload.labels = payload.labels.map(x => {
                const d = new Date(x);

                if (hours <= 48) {
                    // 2 days: show time
                    return d.toLocaleTimeString([], {
                        hour: '2-digit',
                        minute: '2-digit'
                    });
                }

                if (hours <= 168) {
                    // 1 week: show date + time
                    return d.toLocaleDateString([], {
                        month: 'short',
                        day: 'numeric'
                    }) + ' ' + d.toLocaleTimeString([], {
                        hour: '2-digit',
                        minute: '2-digit'
                    });
                }

                if (hours <= 720) {
                    // 1 month: show date only
                    return d.toLocaleDateString([], {
                        month: 'short',
                        day: 'numeric'
                    });
                }

                // 1 year+: show month
                return d.toLocaleDateString([], {
                    year: 'numeric',
                    month: 'short'
                });
            });

            // 保存原始值
            const originalExposure = (payload.exposure || []).map(Number);
            const originalSales = (payload.sales || []).map(Number);
            const originalRevenue = (payload.revenue || []).map(Number);

            // === 创建用于绘图的缩放数据，不改原始值 ===
            const exposureFactor = 0.001;
            const salesFactor = 1;
            const revenueFactor = 0.01;

            const exposureData = originalExposure.map(v => v * exposureFactor);
            const salesData = originalSales.map(v => v * salesFactor);
            const revenueData = originalRevenue.map(v => v * revenueFactor);

            // 图表 dataset 用缩放后的数据
            const datasets = [
                {
                    label: 'Exposure',
                    data: exposureData,
                    borderColor: '#b6ff3e',
                    backgroundColor: 'transparent',
                    tension: 0.2,
                    pointRadius: 3,
                    originalData: originalExposure // 保存原始数据
                },
                {
                    label: 'Sales',
                    data: salesData,
                    borderColor: '#3eeaff',
                    backgroundColor: 'transparent',
                    tension: 0.2,
                    pointRadius: 3,
                    originalData: originalSales
                },
                {
                    label: 'Revenue',
                    data: revenueData,
                    borderColor: '#c06cff',
                    backgroundColor: 'transparent',
                    tension: 0.2,
                    pointRadius: 3,
                    originalData: originalRevenue
                }
            ];

            if (chart) chart.destroy();
            chart = new Chart(ctx, {
                type: 'line',
                data: { labels: payload.labels, datasets: datasets },
                options: {
                    plugins: {
                        legend: { labels: { color: '#ddd' } },
                        tooltip: {
                            callbacks: {
                                label: function (ctx) {
                                    const ds = ctx.dataset;
                                    const idx = ctx.dataIndex;

                                    if (ds.originalData && ds.originalData[idx] != null) {
                                        return ds.label + ': ' + ds.originalData[idx];
                                    }

                                    return ds.label + ': ' + ctx.parsed.y;
                                }
                            }
                        }
                    },
                    animation: { duration: 600 },
                    responsive: true,
                    maintainAspectRatio: false,
                    scales: {
                        x: { grid: { color: 'rgba(255,255,255,0.04)' }, ticks: { color: '#bbb' } },
                        y: {
                            beginAtZero: true,
                            grid: { color: 'rgba(255,255,255,0.04)' },
                            ticks: {
                                color: '#bbb',
                                callback: function (value) {
                                    // 这里只是一个示例：假设 exposure 在显示
                                    return Math.round(value / 0.001);
                                }
                            }
                        }
                    },
                    interaction: { mode: 'index', intersect: false },
                    elements: { line: { borderWidth: 2 } }
                }
            });

            applyDataTypeSelection();
        }

        // Zoom buttons (data-hours) — toggle .active and reload data
        var hourButtons = document.querySelectorAll('.dashboard-controls .btn[data-hours]');
        //console.log(hourButtons);
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
        loadData(48);
    }

    ensureChartAndInit();
})();