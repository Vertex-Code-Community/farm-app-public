// Measures elements marked [data-map-chrome] and reports their viewport rects (CSS px) to .NET,
// so the native touch resolver can route taps without hardcoded control geometry.

let dotnetRef = null;
let ro = null;
let mo = null;
let observed = new WeakSet();
let scheduled = false;
let lastJson = null;

export function init(ref) {
    dotnetRef = ref;

    ro = new ResizeObserver(schedule);
    mo = new MutationObserver(() => { bind(); schedule(); });
    mo.observe(document.body, {
        childList: true,
        subtree: true,
        attributes: true,
        attributeFilter: ['class', 'style', 'data-map-chrome', 'hidden']
    });

    document.addEventListener('transitionend', schedule, true);
    window.addEventListener('resize', schedule);
    window.addEventListener('scroll', schedule, true);

    bind();
    schedule();
}

function bind() {
    if (!ro) return;
    document.querySelectorAll('[data-map-chrome]').forEach(el => {
        if (!observed.has(el)) {
            ro.observe(el);
            observed.add(el);
        }
    });
}

function schedule() {
    if (scheduled) return;
    scheduled = true;
    requestAnimationFrame(() => {
        scheduled = false;
        measure();
    });
}

function measure() {
    if (!dotnetRef) return;

    const rects = [];
    document.querySelectorAll('[data-map-chrome]').forEach(el => {
        const r = el.getBoundingClientRect();
        if (r.width > 0 && r.height > 0) {
            rects.push({
                left: r.left,
                top: r.top,
                right: r.right,
                bottom: r.bottom
            });
        }
    });

    // Avoid spamming .NET when nothing actually moved (e.g. compass rotation re-renders).
    const json = JSON.stringify(rects);
    if (json === lastJson) return;
    lastJson = json;

    dotnetRef.invokeMethodAsync('OnChromeRects', rects);
}

export function dispose() {
    document.removeEventListener('transitionend', schedule, true);
    window.removeEventListener('resize', schedule);
    window.removeEventListener('scroll', schedule, true);
    if (ro) { ro.disconnect(); ro = null; }
    if (mo) { mo.disconnect(); mo = null; }
    observed = new WeakSet();
    dotnetRef = null;
    lastJson = null;
}
