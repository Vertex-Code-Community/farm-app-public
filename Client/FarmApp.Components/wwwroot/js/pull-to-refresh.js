let pullState = {
    startY: 0,
    pulling: false,
    threshold: 70,
    container: null,
    loader: null
};

window.attachPullListener = function (dotnetRef) {
    pullState.container = document.querySelector('.content-wrapper');
    pullState.loader = document.querySelector('.pull-loader-wrapper');

    const handleTouchStart = e => {
        if (document.querySelector('.overlay') || document.querySelector('.modal-wrapper')) {
            pullState.pulling = false;
            return;
        }

        const isAtTop = pullState.container ? pullState.container.scrollTop <= 0 : true;
        if (isAtTop) {
            pullState.startY = e.touches[0].clientY;
            pullState.pulling = true;
        }
    };

    const handleTouchMove = e => {
        if (!pullState.pulling) return;

        const currentY = e.touches[0].clientY;
        const diff = currentY - pullState.startY;

        if (diff > 0 && pullState.container.scrollTop <= 0) {
            if (e.cancelable) e.preventDefault();

            const move = Math.pow(diff, 0.85);

            if (diff > pullState.threshold) {
                pullState.pulling = false;
                dotnetRef.invokeMethodAsync("OnPullTriggered");
            }
        }
    };

    const handleTouchEnd = () => {
        pullState.pulling = false;
    };

    document.addEventListener('touchstart', handleTouchStart, { passive: true });
    document.addEventListener('touchmove', handleTouchMove, { passive: false });
    document.addEventListener('touchend', handleTouchEnd);

    window._pullHandlers = { handleTouchStart, handleTouchMove, handleTouchEnd };
};

window.removePullListener = function () {
    if (window._pullHandlers) {
        document.removeEventListener('touchstart', window._pullHandlers.handleTouchStart);
        document.removeEventListener('touchmove', window._pullHandlers.handleTouchMove);
        document.removeEventListener('touchend', window._pullHandlers.handleTouchEnd);
    }
};