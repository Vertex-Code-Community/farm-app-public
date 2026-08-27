const dropdownListeners = {};

export function registerClickOutside (elementId, dotnetHelper) {
    const listener = (e) => {
        const el = document.getElementById(elementId);
        if (el && !el.contains(e.target)) {
            dotnetHelper.invokeMethodAsync('CloseSelector');
        }
    };
    window.addEventListener('mousedown', listener);
    window.addEventListener('touchstart', listener);

    dropdownListeners[elementId] = listener;
};

export function unregisterClickOutside (elementId) {
    const listener = dropdownListeners[elementId];
    if (listener) {
        window.removeEventListener('mousedown', listener);
        window.removeEventListener('touchstart', listener);
        delete dropdownListeners[elementId];
    }
};