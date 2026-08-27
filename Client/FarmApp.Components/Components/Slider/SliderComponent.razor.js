let isScrolling = false;

export function initializeSlider(element, dotnetHelper) {
    const observer = new IntersectionObserver((entries) => {
        if (isScrolling) return;

        entries.forEach(entry => {
            if (entry.isIntersecting) {
                const index = parseInt(entry.target.getAttribute('data-index'));
                dotnetHelper.invokeMethodAsync('OnScrollIndexChanged', index);
            }
        });
    }, {
        root: element,
        threshold: 0.6
    });

    const items = element.querySelectorAll('.slider-item');
    items.forEach(item => observer.observe(item));

    return {
        dispose: () => {
            observer.disconnect();
        }
    };
}

export function scrollToIndex(container, index, behavior="smooth") {
    if (!container) return;

    const target = container.querySelector(`[data-index="${index}"]`);

    if (target) {
        isScrolling = true;

        target.scrollIntoView({
            behavior,
            block: 'nearest',
            inline: 'center'
        });

        setTimeout(() => {
            isScrolling = false;
        }, behavior == "smooth" ? 500 : 100);
    }
}