export function initInfiniteScroll(element, currentIndex, itemsPerCycle, dotnetHelper) {
    if (!element || !dotnetHelper) return;

    const itemHeight = 30;
    const cycleHeight = itemHeight * itemsPerCycle;
    const visualCenter = 90; 


    const setStyles = (el, rotateX, transformOrigin, transition) => {
        el.style.transform = `rotateX(${rotateX}deg)`;
        el.style.transformOrigin = `center center ${transformOrigin}px`;
        el.style.transition = transition;
    }

    const updateVisuals = (isScrolling) => {
        const items = element.querySelectorAll('.picker-item');
        const containerRect = element.getBoundingClientRect();

        items.forEach((item) => {
            const text = item.querySelector('.picker-item-text');
            if (!text) return;

            if (isScrolling) {
                text.removeAttribute("style");
                return;
            }

            const itemRect = item.getBoundingClientRect();
            const dist = Math.round(containerRect.top - itemRect.top);

            if (dist === 0) { 
                setStyles(text, 50, 24, "0.3s");
            } 
            else if (dist === -30) {
                setStyles(text, 40, 12, "0.2s");
            }
            else if (dist === -60) {
                setStyles(text, 35, 4, "0.1s");
            }
            else if (dist === -90) {
                text.removeAttribute("style");
            }
            else if (dist === -120) {
                setStyles(text, -35, 4, "0.1s");
            }
            else if (dist === -150) {
                setStyles(text, -40, 12, "0.2s");
            }
            else if (dist === -180) {
                setStyles(text, -50, 24, "0.3s");
            }
        });
    };

    const onScroll = () => {
        const top = element.scrollTop;

        updateVisuals(true);

        if (top < cycleHeight) {
            element.style.scrollSnapType = 'none';
            element.scrollTop = top + cycleHeight;
            requestAnimationFrame(() => element.style.scrollSnapType = 'y mandatory');
            return;
        } else if (top > cycleHeight * 2) {
            element.style.scrollSnapType = 'none';
            element.scrollTop = top - cycleHeight;
            requestAnimationFrame(() => element.style.scrollSnapType = 'y mandatory');
            return;
        }

        clearTimeout(element.scrollTimeout);
        element.scrollTimeout = setTimeout(() => {
            const centerPosition = element.scrollTop + visualCenter;
            const index = Math.round(centerPosition / itemHeight) - itemsPerCycle;
            const safeIndex = (index % itemsPerCycle + itemsPerCycle) % itemsPerCycle;
            
            dotnetHelper.invokeMethodAsync('UpdateSelectedFromScroll', safeIndex);

            // Snap back to 3D cylinder
            updateVisuals(false);
        }, 150);
    };

    const initialTarget = (cycleHeight + (currentIndex * itemHeight)) - visualCenter;
    element.style.scrollSnapType = 'none';
    element.scrollTop = initialTarget;

    setTimeout(() => {
        element.style.scrollSnapType = 'y mandatory';
        element.addEventListener('scroll', onScroll, { passive: true });
        updateVisuals(false);
    }, 100);
}

export function scrollToIndex(element, index) {
    if (!element) return;
    const itemHeight = 30;
    const itemsPerCycle = element.querySelectorAll('.picker-item').length / 3;
    const cycleHeight = itemHeight * itemsPerCycle;
    const targetY = (cycleHeight + (index * itemHeight)) - 90;

    element.scrollTo({ top: targetY, behavior: 'instant' });
}