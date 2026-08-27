function getScroller(host) {
    if (!host) {
        return null;
    }

    return host.querySelector(".bch-scroller");
}

export function centerSlider(host) {
    const scroller = getScroller(host);
    if (!scroller || scroller.children.length < 3) {
        return;
    }

    const centerChild = scroller.children[2];
    if (!centerChild) {
        return;
    }

    scroller.scrollTo({
        left: centerChild.offsetLeft,
        top: 0,
        behavior: "auto"
    });
}

export function scrollSliderByDirection(host, scrollToRight) {
    const scroller = getScroller(host);
    if (!scroller) {
        return;
    }

    if (typeof window.bchSnapSliderScrollTo === "function" && scroller.id) {
        window.bchSnapSliderScrollTo(scroller.id, scrollToRight);
        return;
    }

    const targetIndex = scrollToRight ? 3 : 1;
    const targetChild = scroller.children[targetIndex];
    if (!targetChild) {
        return;
    }

    scroller.scrollTo({
        left: targetChild.offsetLeft,
        top: 0,
        behavior: "smooth"
    });
}
