const state = new WeakMap();

window.heightAnimator = {
    measure(element) {
        if (!element) return;

        state.set(element, {
            prevHeight: element.offsetHeight
        });
    },

    animate(element) {
        if (!element) return;

        const data = state.get(element);
        if (!data) return;

        const prevHeight = data.prevHeight;

        element.style.height = 'auto';
        const newHeight = element.scrollHeight;

        if (Math.abs(newHeight - prevHeight) < 1) {
            state.delete(element);
            return;
        }

        element.style.transition = 'none';
        element.style.height = prevHeight + 'px';

        element.offsetHeight;

        const delta = Math.abs(newHeight - prevHeight);
        const duration = Math.min(400, Math.max(150, delta * 0.5));

        element.style.transition = `height ${duration}ms ease`;
        element.style.height = newHeight + 'px';

        const cleanup = () => {
            element.style.height = 'auto';
            element.style.transition = '';
            element.removeEventListener('transitionend', cleanup);
        };

        element.addEventListener('transitionend', cleanup);

        state.delete(element);
    },

    transition(element, updateCallback) {
        if (!element) return;

        this.measure(element);

        updateCallback();

        requestAnimationFrame(() => {
            this.animate(element);
        });
    }
};