export function initStaggeredReveal(containerId) {
    const container = document.getElementById(containerId);
    if (!container) return;

    let globalIndex = 0;
    let timeoutId = null;

    const processNewElements = (root) => {
        const targets = [];
        if (root.hasAttribute && root.hasAttribute('data-animate')) targets.push(root);
        const descendants = root.querySelectorAll ? root.querySelectorAll('[data-animate]') : [];
        targets.push(...descendants);

        targets.forEach((el) => {
            if (el.dataset.animating === 'true' || el.dataset.revealed === 'true') return;

            el.dataset.animating = 'true';
            el.classList.add('reveal-item');
            el.style.transitionDelay = `${globalIndex * 0.1}s`;
            globalIndex++;

            const cleanup = () => {
                el.classList.remove('reveal-item', 'is-visible');
                el.style.transitionDelay = '';
                el.dataset.animating = 'false';
                el.dataset.revealed = 'true';
                el.removeEventListener('transitionend', cleanup);
            };

            el.addEventListener('transitionend', cleanup);

            requestAnimationFrame(() => {
                requestAnimationFrame(() => {
                    el.classList.add('is-visible');
                });
            });
        });

        clearTimeout(timeoutId);
        timeoutId = setTimeout(() => { globalIndex = 0; }, 300);
    };

    requestAnimationFrame(() => {
        processNewElements(container);
    });

    const observer = new MutationObserver((mutations) => {
        for (const mutation of mutations) {
            for (const node of mutation.addedNodes) {
                if (node.nodeType === 1) processNewElements(node);
            }
        }
    });

    observer.observe(container, { childList: true, subtree: true });
};

export function initAutoHeight() {
    const observer = new MutationObserver(() => {
        const invalidFields = document.querySelectorAll('.invalid.modified:not(:focus-within)');
        const offset = invalidFields.length * 28;

        document.documentElement.style.setProperty('--global-error-offset', `${offset}px`);
    });

    observer.observe(document.body, { childList: true, subtree: true, attributes: true });
}