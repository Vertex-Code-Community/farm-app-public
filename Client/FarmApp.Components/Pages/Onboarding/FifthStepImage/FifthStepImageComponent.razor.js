const TWO_PI = Math.PI * 2;

const easeInCubic = t => t * t * t;
const easeOutElastic = t => {
    const c4 = TWO_PI / 3;
    return t === 0 ? 0 : t === 1 ? 1
        : Math.pow(2, -10 * t) * Math.sin((t * 10 - 0.75) * c4) + 1;
};

const easeOutQuart = t => 1 - Math.pow(1 - t, 4);

function lerp(a, b, t) { return a + (b - a) * t; }

function animate({ duration, onUpdate, onComplete, easing = t => t }) {
    let start = null;
    let rafId;
    function frame(ts) {
        if (!start) start = ts;
        const rawT = Math.min((ts - start) / duration, 1);
        onUpdate(easing(rawT), rawT);
        if (rawT < 1) {
            rafId = requestAnimationFrame(frame);
        } else {
            onComplete?.();
        }
    }
    rafId = requestAnimationFrame(frame);
    return () => cancelAnimationFrame(rafId);
}

function triggerLeafFall(leafEl, dotNetHelper) {
    const startTop = -40;
    const targetTop = 74;

    const duration = 1500;

    animate({
        duration,
        easing: t => t,
        onUpdate: (t) => {

            // ── Drop ─────────────────────────────────────────────────────
            // Smooth gravity: slow start, fast middle, cushioned landing
            const drop = easeOutQuart(t);
            const currentTop = lerp(startTop, targetTop, drop);

            // ── Subtle 3D catch-air ───────────────────────────────────────
            // Peaks mid-fall, gone by landing so it sits flat at rest
            const tiltEnv = Math.sin(t * Math.PI);
            const rotX = 18 * tiltEnv;

            leafEl.style.opacity = Math.min(t / 0.1, 1);
            leafEl.style.top = `${currentTop}px`;
            leafEl.style.transform =
                `translateX(-50%) ` +
                `perspective(280px) ` +
                `rotateX(${rotX}deg)`;
        },
        onComplete: () => {
            if (dotNetHelper) {
                dotNetHelper.invokeMethodAsync('NotifyAnimationFinished');
            }
        }
    });
}

// ── State ────────────────────────────────────────────────────────────────────

let orbitAngle = -Math.PI / 2; // start at top
let orbitRafId = null;
let lastTs = null;
let orbitIcons = [];           // { el, baseAngle } — removed when sucked in
let containerEl = null;
let orbitRadius = 111;
let iconSize = 50;

// ── Orbit loop ───────────────────────────────────────────────────────────────

function orbitFrame(ts) {
    if (lastTs !== null) {
        const dt = (ts - lastTs) / 1000;
        orbitAngle += dt * (TWO_PI / 12); // one rotation per 12s
    }
    lastTs = ts;

    orbitIcons.forEach(({ el, baseAngle }) => {
        const angle = orbitAngle + baseAngle;
        // Translate from the element's own center (left:50%, top:50% in CSS)
        const x = Math.cos(angle) * orbitRadius;
        const y = Math.sin(angle) * orbitRadius;
        el.style.transform = `translate(calc(-50% + ${x}px), calc(-50% + ${y}px))`;
    });

    orbitRafId = requestAnimationFrame(orbitFrame);
}

// ── Public API ───────────────────────────────────────────────────────────────

export function startOrbit(orbitRing, iconEls) {
    containerEl = orbitRing;
    const count = iconEls.length;

    // Center each icon via absolute positioning at 50%/50%
    iconEls.forEach((el, i) => {
        el.style.position = 'absolute';
        el.style.left = '50%';
        el.style.top = '50%';
        el.style.width = `${iconSize}px`;
        el.style.height = `${iconSize}px`;
    });

    orbitIcons = iconEls.map((el, i) => ({
        el,
        baseAngle: (TWO_PI / count) * i
    }));

    lastTs = null;
    orbitRafId = requestAnimationFrame(orbitFrame);
}

export function startSuckIn(orbitRing, centerIcon, iconEls, leafEl, dotNetHelper) {
    const container = orbitRing.parentElement;
    const containerRect = container.getBoundingClientRect();
    const centerX = containerRect.left + containerRect.width / 2;
    const centerY = containerRect.top + containerRect.height / 2;
    let centerScale = 1;

    iconEls.forEach((el, i) => {
        setTimeout(() => {

            // Remove from orbit loop so orbitFrame stops moving it
            orbitIcons = orbitIcons.filter(o => o.el !== el);

            // Snapshot real screen position before reparenting
            const iconRect = el.getBoundingClientRect();
            const fromX = iconRect.left - containerRect.left;
            const fromY = iconRect.top - containerRect.top;
            const toX = containerRect.width / 2 - iconSize / 2;
            const toY = containerRect.height / 2 - iconSize / 2;

            // Reparent into container, locked at current position
            el.style.left = `${fromX}px`;
            el.style.top = `${fromY}px`;
            el.style.transform = 'none';
            container.appendChild(el);

            // Force reflow
            el.getBoundingClientRect();

            let spinAngle = 0;

            animate({
                duration: 700,
                easing: easeInCubic,
                onUpdate: (t) => {
                    const x = lerp(fromX, toX, t);
                    const y = lerp(fromY, toY, t);
                    spinAngle = lerp(0, 360, t);
                    const scale = lerp(1, 0, t);
                    const opacity = lerp(1, 0, Math.pow(t, 0.6));
                    el.style.left = `${x}px`;
                    el.style.top = `${y}px`;
                    el.style.transform = `rotate(${spinAngle}deg) scale(${scale})`;
                    el.style.opacity = opacity;
                },
                onComplete: () => {
                    el.style.display = 'none';

                    const fromScale = centerScale;
                    const toScale = centerScale + 0.04;
                    centerScale = toScale;

                    animate({
                        duration: 500,
                        easing: easeOutElastic,
                        onUpdate: (t) => {
                            centerIcon.style.transform = `scale(${lerp(fromScale, toScale, t)})`;
                        },
                        onComplete: () => {
                            if (i === iconEls.length - 4) {
                                triggerLeafFall(leafEl, dotNetHelper);
                            }
                        }
                    });

                }
            });

        }, i * 250);
    });
}