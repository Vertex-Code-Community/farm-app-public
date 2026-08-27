window.keyboardLock = {
    disableScroll: function () {
        window.addEventListener('scroll', this.preventScroll, { passive: false });

        document.addEventListener('focusin', this.scrollToTop, true);
    },
    enableScroll: function () {
        window.removeEventListener('scroll', this.preventScroll);
        document.removeEventListener('focusin', this.scrollToTop, true);
    },
    preventScroll: function (e) {
        window.scrollTo(0, 0);
        e.preventDefault();
    },
    scrollToTop: function () {
        setTimeout(() => {
            window.scrollTo(0, 0);
            document.body.scrollTop = 0;
        }, 10);
    }
};