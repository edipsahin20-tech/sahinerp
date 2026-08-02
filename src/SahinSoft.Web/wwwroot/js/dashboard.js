(() => {
    "use strict";

    const dataElement = document.getElementById("dashboardChartData");
    if (!dataElement) return;

    let dashboardData;
    try {
        dashboardData = JSON.parse(dataElement.textContent || "{}");
    } catch {
        return;
    }

    const money = new Intl.NumberFormat("tr-TR", {
        style: "currency",
        currency: "TRY",
        maximumFractionDigits: 0
    });

    const palette = {
        grid: "#e9edf2",
        label: "#8992a2",
        teal: "#10a898",
        tealFill: "rgba(16, 168, 152, .10)",
        purple: "#7554dc",
        purpleFill: "rgba(117, 84, 220, .07)",
        slate: "#718096"
    };

    function setupCanvas(canvas) {
        const rect = canvas.getBoundingClientRect();
        const ratio = Math.min(window.devicePixelRatio || 1, 2);
        canvas.width = Math.max(1, Math.round(rect.width * ratio));
        canvas.height = Math.max(1, Math.round(rect.height * ratio));
        const ctx = canvas.getContext("2d");
        ctx.setTransform(ratio, 0, 0, ratio, 0, 0);
        return { ctx, width: rect.width, height: rect.height };
    }

    function niceMax(values) {
        const maximum = Math.max(0, ...values);
        if (maximum === 0) return 100;
        const power = 10 ** Math.floor(Math.log10(maximum));
        return Math.ceil(maximum / power) * power;
    }

    function drawGrid(ctx, width, height, padding, maxValue) {
        ctx.lineWidth = 1;
        ctx.font = "10px Segoe UI, sans-serif";
        ctx.textAlign = "right";
        ctx.textBaseline = "middle";
        for (let i = 0; i <= 4; i++) {
            const y = padding.top + ((height - padding.top - padding.bottom) * i / 4);
            ctx.strokeStyle = palette.grid;
            ctx.beginPath();
            ctx.moveTo(padding.left, y);
            ctx.lineTo(width - padding.right, y);
            ctx.stroke();
            ctx.fillStyle = palette.label;
            ctx.fillText(new Intl.NumberFormat("tr-TR", { notation: "compact", maximumFractionDigits: 1 }).format(maxValue * (4 - i) / 4), padding.left - 7, y);
        }
    }

    function buildPoints(values, width, height, padding, maxValue) {
        const plotWidth = width - padding.left - padding.right;
        const plotHeight = height - padding.top - padding.bottom;
        return values.map((value, index) => ({
            x: padding.left + (values.length <= 1 ? plotWidth / 2 : plotWidth * index / (values.length - 1)),
            y: padding.top + plotHeight - ((Number(value) || 0) / maxValue * plotHeight),
            value: Number(value) || 0
        }));
    }

    function roundedLine(ctx, points) {
        if (!points.length) return;
        ctx.beginPath();
        ctx.moveTo(points[0].x, points[0].y);
        for (let i = 1; i < points.length; i++) {
            const previous = points[i - 1];
            const current = points[i];
            const middleX = (previous.x + current.x) / 2;
            ctx.bezierCurveTo(middleX, previous.y, middleX, current.y, current.x, current.y);
        }
    }

    function drawArea(ctx, points, baseline, stroke, fill) {
        if (!points.length) return;
        roundedLine(ctx, points);
        ctx.lineTo(points.at(-1).x, baseline);
        ctx.lineTo(points[0].x, baseline);
        ctx.closePath();
        ctx.fillStyle = fill;
        ctx.fill();
        roundedLine(ctx, points);
        ctx.strokeStyle = stroke;
        ctx.lineWidth = 2;
        ctx.stroke();
        points.forEach(point => {
            ctx.beginPath();
            ctx.arc(point.x, point.y, 2.25, 0, Math.PI * 2);
            ctx.fillStyle = "#fff";
            ctx.fill();
            ctx.strokeStyle = stroke;
            ctx.lineWidth = 1.4;
            ctx.stroke();
        });
    }

    function drawLabels(ctx, labels, width, height, padding, step = 1) {
        const plotWidth = width - padding.left - padding.right;
        ctx.fillStyle = palette.label;
        ctx.font = "10px Segoe UI, sans-serif";
        ctx.textAlign = "center";
        ctx.textBaseline = "bottom";
        labels.forEach((label, index) => {
            if (index % step !== 0 && index !== labels.length - 1) return;
            const x = padding.left + (labels.length <= 1 ? plotWidth / 2 : plotWidth * index / (labels.length - 1));
            ctx.fillText(label, x, height - 1);
        });
    }

    function attachTooltip(canvas, labels, series, pointProvider) {
        const host = canvas.parentElement;
        host.style.position = "relative";
        let tooltip = host.querySelector(".dash-chart-tooltip");
        if (!tooltip) {
            tooltip = document.createElement("div");
            tooltip.className = "dash-chart-tooltip";
            tooltip.hidden = true;
            host.appendChild(tooltip);
        }

        canvas.onmousemove = event => {
            const points = pointProvider();
            if (!points?.length) return;
            const rect = canvas.getBoundingClientRect();
            const mouseX = event.clientX - rect.left;
            let nearest = 0;
            let distance = Number.POSITIVE_INFINITY;
            points.forEach((point, index) => {
                const currentDistance = Math.abs(point.x - mouseX);
                if (currentDistance < distance) {
                    distance = currentDistance;
                    nearest = index;
                }
            });
            tooltip.innerHTML = `<strong>${labels[nearest]}</strong>${series.map(item => `<span><i style="background:${item.color}"></i>${item.label}: ${money.format(Number(item.values[nearest]) || 0)}</span>`).join("")}`;
            tooltip.hidden = false;
            const left = Math.min(Math.max(8, mouseX + 12), rect.width - tooltip.offsetWidth - 8);
            tooltip.style.left = `${left}px`;
            tooltip.style.top = "8px";
        };
        canvas.onmouseleave = () => { tooltip.hidden = true; };
    }

    function createCashChart() {
        const canvas = document.getElementById("cashFlowChart");
        if (!canvas || !dashboardData.cash) return () => {};
        let points = [];
        const render = () => {
            const { ctx, width, height } = setupCanvas(canvas);
            ctx.clearRect(0, 0, width, height);
            const padding = { top: 8, right: 8, bottom: 21, left: 42 };
            const collection = dashboardData.cash.collection || [];
            const payment = dashboardData.cash.payment || [];
            const maximum = niceMax([...collection, ...payment]);
            drawGrid(ctx, width, height, padding, maximum);
            const collectionPoints = buildPoints(collection, width, height, padding, maximum);
            const paymentPoints = buildPoints(payment, width, height, padding, maximum);
            drawArea(ctx, paymentPoints, height - padding.bottom, palette.purple, palette.purpleFill);
            drawArea(ctx, collectionPoints, height - padding.bottom, palette.teal, palette.tealFill);
            drawLabels(ctx, dashboardData.cash.labels || [], width, height, padding, width < 650 ? 2 : 1);
            points = collectionPoints;
        };
        attachTooltip(canvas, dashboardData.cash.labels || [], [
            { label: "Tahsilat", values: dashboardData.cash.collection || [], color: palette.teal },
            { label: "Ödeme", values: dashboardData.cash.payment || [], color: palette.purple }
        ], () => points);
        return render;
    }

    function createInvoiceChart() {
        const canvas = document.getElementById("invoiceTrendChart");
        if (!canvas || !dashboardData.invoices) return () => {};
        let points = [];
        const render = () => {
            const { ctx, width, height } = setupCanvas(canvas);
            ctx.clearRect(0, 0, width, height);
            const padding = { top: 8, right: 6, bottom: 21, left: 42 };
            const sales = dashboardData.invoices.sales || [];
            const purchase = dashboardData.invoices.purchase || [];
            const labels = dashboardData.invoices.labels || [];
            const maximum = niceMax([...sales, ...purchase]);
            drawGrid(ctx, width, height, padding, maximum);
            const plotWidth = width - padding.left - padding.right;
            const plotHeight = height - padding.top - padding.bottom;
            const groupWidth = plotWidth / Math.max(1, labels.length);
            const barWidth = Math.min(18, groupWidth * .28);
            points = [];
            labels.forEach((label, index) => {
                const center = padding.left + (groupWidth * index) + groupWidth / 2;
                const saleValue = Number(sales[index]) || 0;
                const purchaseValue = Number(purchase[index]) || 0;
                const saleHeight = saleValue / maximum * plotHeight;
                const purchaseHeight = purchaseValue / maximum * plotHeight;
                ctx.fillStyle = palette.teal;
                ctx.fillRect(center - barWidth - 2, padding.top + plotHeight - saleHeight, barWidth, saleHeight);
                ctx.fillStyle = palette.slate;
                ctx.fillRect(center + 2, padding.top + plotHeight - purchaseHeight, barWidth, purchaseHeight);
                points.push({ x: center, y: padding.top, value: saleValue });
            });
            ctx.fillStyle = palette.label;
            ctx.font = "10px Segoe UI, sans-serif";
            ctx.textAlign = "center";
            ctx.textBaseline = "bottom";
            labels.forEach((label, index) => ctx.fillText(label, padding.left + (groupWidth * index) + groupWidth / 2, height - 1));
        };
        attachTooltip(canvas, dashboardData.invoices.labels || [], [
            { label: "Satış", values: dashboardData.invoices.sales || [], color: palette.teal },
            { label: "Alış", values: dashboardData.invoices.purchase || [], color: palette.slate }
        ], () => points);
        return render;
    }

    const renders = [createCashChart(), createInvoiceChart()];
    renders.forEach(render => render());
    let resizeTimer;
    window.addEventListener("resize", () => {
        clearTimeout(resizeTimer);
        resizeTimer = setTimeout(() => renders.forEach(render => render()), 120);
    });
})();
