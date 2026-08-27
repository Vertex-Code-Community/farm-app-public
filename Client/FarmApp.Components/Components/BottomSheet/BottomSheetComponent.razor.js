export function init(sheet, dotnetRef, minHeight, allowDrag) {
    if (!sheet) return;
    if (allowDrag === undefined || allowDrag === null) {
        allowDrag = true;
    }

    const overlay = sheet.previousElementSibling;
    const CLOSE_THRESHOLD = 50;
    const OPEN_THRESHOLD = 50;
    const DURATION = 150;
    const handle = sheet.querySelector('.sheet-touchzone');

    const defaultTransition = `transform ${DURATION}ms linear, opacity ${DURATION}ms linear, height 0.3s linear`;

    let startY = 0;
    let dragging = false;
    let isSheetOpen = true;

    const hasMinHeight = minHeight !== "0px";

    const animateClose = () => {
        isSheetOpen = false;
        sheet.style.transition = defaultTransition;
        sheet.style.transform = `translateY(calc(100% - ${minHeight}))`;

        if (overlay) {
            overlay.style.opacity = '0';
            overlay.style.pointerEvents = "none";
        }

        if (!hasMinHeight) {
            setTimeout(() => {
                dotnetRef.invokeMethodAsync('NotifySheetClosed');
            }, DURATION);
        } else {
            dotnetRef.invokeMethodAsync('SetAtMinHeight', true);
        }
    };

    const animateOpen = () => {
        isSheetOpen = true;
        sheet.style.transition = defaultTransition;
        sheet.style.transform = `translateY(0)`;
        if (overlay) {
            overlay.style.opacity = '1';
            overlay.style.pointerEvents = "all";
        }

        if (!hasMinHeight) {
            sheet.style.opacity = "1";
        }

        if (hasMinHeight) {
            dotnetRef.invokeMethodAsync('SetAtMinHeight', false);
        }
    };

    sheet._animateClose = animateClose;
    sheet._animateOpen = animateOpen;

    if (overlay) {
        overlay.onclick = animateClose;

        let overlayStartX = 0;
        let overlayStartY = 0;
        let overlayStartTime = 0;

        overlay.addEventListener('touchstart', (e) => {
            overlayStartX = e.touches[0].clientX;
            overlayStartY = e.touches[0].clientY;
            overlayStartTime = Date.now();
        }, { passive: true });

        overlay.addEventListener('touchend', (e) => {
            const endX = e.changedTouches[0].clientX;
            const endY = e.changedTouches[0].clientY;
            const endTime = Date.now();

            const distanceX = Math.abs(endX - overlayStartX);
            const distanceY = Math.abs(endY - overlayStartY);
            const timeElapsed = endTime - overlayStartTime;

            if (distanceX < 15 && distanceY < 15 && timeElapsed < 300) {
                animateClose();

                if (e.cancelable) {
                    e.preventDefault();
                }
            }
        }, { passive: false });
    }

    let currentTranslateY = 0;
    let startTranslateY = 0;
    let sheetHeight;

    const getCurrentY = () => {
        const style = window.getComputedStyle(sheet);
        sheetHeight = parseFloat(style.height);
        const matrix = new WebKitCSSMatrix(style.transform);
        return matrix.m42;
    };

    const onStart = y => {
        startY = y;
        dragging = true;
        startTranslateY = getCurrentY();

        sheet.style.transition = 'none';
    };


    const onMove = y => {
        if (!dragging) return;

        const delta = y - startY;
        let newTranslateY = startTranslateY + delta;

        const minHeightPx = parseFloat(minHeight);
        const bottomLimit = sheetHeight - minHeightPx;

        if (newTranslateY < 0) {
            newTranslateY = 0;
        } 
        if (newTranslateY > bottomLimit) {
            newTranslateY = bottomLimit
        }

        if (newTranslateY >= 0 && newTranslateY <= bottomLimit) {
            sheet.style.transform = `translateY(${newTranslateY}px)`;
        } 

        const progress = Math.max(0, Math.min(1, 1 - (newTranslateY / bottomLimit)));
        if (overlay) overlay.style.opacity = progress;
    };

    const onEnd = y => {
        if (!dragging) return;
        dragging = false;
        const delta = y - startY;

        if (delta > CLOSE_THRESHOLD) {
            animateClose();
        } 
        else if (delta < -OPEN_THRESHOLD) {
            animateOpen();
        }
        else {
            if (isSheetOpen) {
                animateOpen();
            } else {
                animateClose();
            }
        }
    };

    const onMoveHandler = e => onMove(e.touches ? e.touches[0].clientY : e.clientY);
    const onEndHandler = e => {
        onEnd(e.changedTouches ? e.changedTouches[0].clientY : e.clientY);
        document.removeEventListener('mousemove', onMoveHandler);
        document.removeEventListener('mouseup', onEndHandler);
        document.removeEventListener('touchmove', onMoveHandler);
        document.removeEventListener('touchend', onEndHandler);
    };

    if (allowDrag) {
        handle.addEventListener('mousedown', e => {
            onStart(e.clientY);
            document.addEventListener('mousemove', onMoveHandler);
            document.addEventListener('mouseup', onEndHandler);
        });

        handle.addEventListener('touchstart', e => {
            onStart(e.touches[0].clientY);
            document.addEventListener('touchmove', onMoveHandler, { passive: false });
            document.addEventListener('touchend', onEndHandler);
        }, { passive: true });
    }
}

export function initSheetObserver (wrapper, content) {
    const resizeObserver = new ResizeObserver(entries => {
        for (let entry of entries) {
            const newHeight = entry.target.offsetHeight;
            wrapper.style.height = newHeight + 54 + 'px';
        }
    });

    resizeObserver.observe(content);
};

export function triggerCloseAnimation(sheet) {
    if (sheet && typeof sheet._animateClose === 'function') {
        sheet._animateClose();
    }
}

export function triggerOpenAnimation(sheet) {
    if (sheet && typeof sheet._animateOpen === "function") {
        sheet._animateOpen();
    }
}