const lottieAnimation = {};

function loadLottieAnimation(id, urlPath, speed = 1) {
    const animation = bodymovin.loadAnimation({
        container: document.getElementById(id),
        renderer: 'svg',
        loop: true,
        autoplay: true,
        path: urlPath
    });
    
    animation.setSpeed(speed);
    lottieAnimation[id] = animation;

    // console.log(animation);
    //
    // animation.addEventListener('data_ready', function() {
    //     console.log('LOTTIE Animation data is loaded');
    // });
    //
    // animation.addEventListener('DOMLoaded', function() {
    //     console.log('LOTTIE DOM is ready');
    // });
    //
    // animation.addEventListener('complete', function() {
    //     console.log('LOTTIE Animation is completed');
    // });
    //
    // // Use this to catch loading errors
    // animation.addEventListener('data_failed', function() {
    //     console.error('Failed to load animation data');
    // });
}

function loadLottieAnimationWithSegments(id, urlPath, left, right, speed = 1) {
    const animation = bodymovin.loadAnimation({
        container: document.getElementById(id),
        renderer: 'svg',
        loop: true,
        autoplay: true,
        path: urlPath
    });

    animation.setSpeed(speed);
    animation.playSegments([left, right], true);

    lottieAnimation[id] = animation;
}

function destroyLottieAnimation(id) {
    const animation = lottieAnimation[id];
    if (!animation) return;

    animation.destroy()
}