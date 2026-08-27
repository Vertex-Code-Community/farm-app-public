const bchSnapSliderMap = {};

export function initializeSnapSlider(scroller, dotnetRef, scrollerId, leftShift) {
    if (!scroller) return;

    registerSnapFeedbackRef(dotnetRef, scrollerId, leftShift);
    scroller.addEventListener('scroll', onSnapSliderScroll, { passive: true });
}

function registerSnapFeedbackRef(dotnetRef, scrollerId, leftShift) {
    bchSnapSliderMap[scrollerId] = {
        index: 3,
        dotnetRef: dotnetRef,
        leftShift: leftShift,
        triggered: false
    };
}

export function releaseSnapFeedbackRef(scrollerId) {
    const scroller = document.getElementById(scrollerId);
    if (scroller) {
        scroller.removeEventListener('scroll', onSnapSliderScroll);
    }

    delete bchSnapSliderMap[scrollerId];
}

function onSnapSliderScroll(event) {
    const scrollerId = event.target.id;
    if (!scrollerId) return;

    const state = bchSnapSliderMap[scrollerId];
    if (!state) return;

    const firstChildElement = event.target.firstElementChild;
    const childWidth = firstChildElement.clientWidth;

    const scrollLeft = event.target.scrollLeft - firstChildElement.offsetLeft + childWidth;
    const index = (scrollLeft / childWidth);

    const difference = index - state.index;
    const differenceAbs = Math.abs(difference);

    if (differenceAbs >= 0.90) {
        if (state.triggered) return;

        state.triggered = true;
        const direction = Math.sign(difference);
        state.dotnetRef.invokeMethodAsync('OnNextCalledFromScrollListenerAsync', direction);
    } else {
        state.triggered = false;
    }
}

export function snapSliderScrollTo(scrollerId, scrollToRight) {
    const scroller = document.getElementById(scrollerId);
    if (!scroller) return;

    const secondElement = scroller.children[1];
    const fourthElement = scroller.children[3];
    if (!secondElement || !fourthElement) return;

    scroller.scrollTo({
        left: scrollToRight ? fourthElement.offsetLeft : secondElement.offsetLeft,
        top: 0,
        behavior: 'smooth'
    });
}