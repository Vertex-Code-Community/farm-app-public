let activeShield = null;
let Selector = null;
let GuideToureEl = null;
let CutoutEl = null;
let BubbleEl = null;
let CurrentObserver = null;
let DotNetHelper = null;

let OutlineTarget = true;
let AllowTargetClick = true;

const CUTOUT_PADDING = () => OutlineTarget ? 6 : 0;

export async function init(dotNetHelper) {
    DotNetHelper = dotNetHelper;
}

export function updateSelector(guideEl, targetSelector, outlineTarget, allowTargetClick) { 
    if (!guideEl || !targetSelector) return;

    GuideToureEl = guideEl;
    CutoutEl = guideEl.querySelector(".overlay-cutout");
    BubbleEl = guideEl.querySelector(".tour-bubble");
    Selector = targetSelector;

    OutlineTarget = outlineTarget;
    AllowTargetClick = allowTargetClick;

    if (!activeShield) createShield();
    if (!CurrentObserver) setupObserver();

    const target = document.querySelector(Selector);

    if (target) {
        refreshGuideTour(target);
    } else {
        GuideToureEl.classList.add("hidden");
    }
}

export function stopTour() {
    if (activeShield) {
        window.removeEventListener("click", activeShield, true);
        window.removeEventListener("mousedown", activeShield, true);
        window.removeEventListener("touchstart", activeShield, { capture: true, passive: false });
        activeShield = null;
    }

    if (CurrentObserver) {
        CurrentObserver.disconnect();
        CurrentObserver = null;
    }

    if (GuideToureEl) {
        GuideToureEl.classList.add("hidden");
    }

    Selector = null;
}

function setupObserver() {
    CurrentObserver = new MutationObserver((mutations) => {
        if (!Selector) return;

        const isInternal = mutations.every(m => GuideToureEl.contains(m.target));
        if (isInternal) return;

        const target = document.querySelector(Selector);

        if (!target) {
            GuideToureEl.classList.add("hidden");
            return;
        }

        refreshGuideTour(target);
    });

    CurrentObserver.observe(document.body, { childList: true, subtree: true });
}

function refreshGuideTour(target) {
    positionCutout(target);
    positionBubble(target);
    GuideToureEl.classList.remove("hidden");
}

function positionCutout(target) {
    if (!CutoutEl) return;

    if (target != document.body) {
        const rect = target.getBoundingClientRect();

        const width = `${rect.width + (CUTOUT_PADDING() * 2)}px`;
        const height = `${rect.height + (CUTOUT_PADDING() * 2)}px`;
        const top = `${rect.top - CUTOUT_PADDING()}px`;
        const left = `${rect.left - CUTOUT_PADDING()}px`;

        const borderRadius = window.getComputedStyle(target).borderRadius;
        CutoutEl.style.cssText = `top: ${top}; left: ${left}; width: ${width}; height: ${height}; border-radius: ${borderRadius};
        ${!OutlineTarget ? "border: none" : ""}`;
    }
    else {
        CutoutEl.style.cssText = `top: 0px; left: 0px; width: 0px; height: 0px;`;
    }

}

function positionBubble(target) {
    if (!BubbleEl) return;
    const arrowEl = BubbleEl.querySelector(".bubble-triangle");
    if (!arrowEl) return;

    if (target != document.body) {
        const rect = target.getBoundingClientRect();
        const bubbleStyles = window.getComputedStyle(BubbleEl);
        const bubbleWidth = parseFloat(bubbleStyles.width);
        const bubbleHeight = parseFloat(bubbleStyles.height);

        const padding = CUTOUT_PADDING();
        const arrowWidth = 18;
        const bubbleOffset = 6 + 16;

        let top;
        let left;
        let arrowStyles = "";
        let isBottomArrow = false;

        const verticalFreeSpace = (window.innerHeight - rect.height) / 2;
        if (rect.top > verticalFreeSpace) {
            top = rect.top - bubbleHeight - padding - bubbleOffset;
            arrowStyles += "transform: rotate(180deg); bottom: -16px;";
            isBottomArrow = true;
        } else {
            top = rect.top + rect.height + padding + bubbleOffset;
            arrowStyles += "top: -16px;";
        }
        let bubbleLeft = rect.left + (rect.width / 2) - (bubbleWidth / 2);

        bubbleLeft = Math.max(16, Math.min(bubbleLeft, window.innerWidth - bubbleWidth - 16));
        left = `${bubbleLeft}px`;

        const targetCenterRelative = (rect.left + rect.width / 2) - bubbleLeft;

        const arrowLeftPos = Math.max(0, Math.min(targetCenterRelative - (arrowWidth / 2), bubbleWidth - arrowWidth));
        arrowStyles += `left: ${arrowLeftPos}px;`;

        const maxRadius = 12;
        const distFromLeft = arrowLeftPos - bubbleLeft;
        const distFromRight = (bubbleWidth) - ((arrowLeftPos) + arrowWidth);

        const rL = `${Math.min(maxRadius, distFromLeft)}px`;
        const rR = `${Math.min(maxRadius, distFromRight)}px`;
        const borderRadius = isBottomArrow ? `12px 12px ${rR} ${rL}` : `${rL} ${rR} 12px 12px`;

        arrowEl.style.cssText = arrowStyles;
        BubbleEl.style.cssText = `top: ${top}px; left: ${left}; border-radius: ${borderRadius};`;
    } else {
        arrowEl.style.cssText = "display: none";
        BubbleEl.style.cssText = `top: 50%; left: 50%; transform: translate(-50%, -50%); border-radius: 8px;`;
    }
}

function createShield() {
    activeShield = (e) => {
        const currentTarget = document.querySelector(Selector);
        if (!currentTarget) return;

        const notifActive = document.querySelector(".bch-component.bch-modal-overlay.notification-container");
        if (notifActive) return;

        const isTarget = AllowTargetClick && currentTarget.contains(e.target);

        const isCloseBtn = e.target.closest(".close-guide");
        const isNextButton = e.target.closest(".tour-guide-next-btn");
        const isSkipButton = e.target.closest(".tour-guide-skip-btn");

        if (isTarget || isCloseBtn || isNextButton || isSkipButton) {
            if (DotNetHelper) {
                if (isTarget || isNextButton) {
                    DotNetHelper.invokeMethodAsync('OnTargetClicked');
                }
                if (isCloseBtn || isSkipButton) {
                    DotNetHelper.invokeMethodAsync('OnSkipClicked');
                }
            }
            stopTour();
            return;
        }

        e.preventDefault();
        e.stopPropagation();
    }

    window.addEventListener("click", activeShield, true);
    window.addEventListener("mousedown", activeShield, true);
    window.addEventListener("touchstart", activeShield, { capture: true, passive: true });
}