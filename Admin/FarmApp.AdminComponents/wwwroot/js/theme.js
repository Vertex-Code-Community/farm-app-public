window.themeService = (function () {
    const storageKey = 'app-theme'; // values: 'light' | 'dark' | 'system'
    let dotNetObject = null;

    function getStoredTheme() {
        try {
            return localStorage.getItem(storageKey);
        } catch (_) {
            return null;
        }
    }

    function setStoredTheme(value) {
        try {
            if (value === null) {
                localStorage.removeItem(storageKey);
            } else {
                localStorage.setItem(storageKey, value);
            }
        } catch (_) { }
    }

    function getSystemPrefersDark() {
        return window.matchMedia && window.matchMedia('(prefers-color-scheme: dark)').matches;
    }

    function applyThemeClass(isDark) {
        const root = document.documentElement;
        root.classList.toggle('dark', !!isDark);
    }

    function getCurrentTheme() {
        const stored = getStoredTheme();
        return stored || 'system';
    }

    function getIsDark() {
        const current = getCurrentTheme();
        if (current === 'light') return false;
        if (current === 'dark') return true;
        return getSystemPrefersDark();
    }

    function reevaluateAndApply() {
        const isDark = getIsDark();
        applyThemeClass(isDark);
        return isDark;
    }

    function subscribeToSystemChanges() {
        if (!window.matchMedia) return;
        const media = window.matchMedia('(prefers-color-scheme: dark)');
        if (typeof media.addEventListener === 'function') {
            media.addEventListener('change', onSystemSchemeChange);
        } else if (typeof media.addListener === 'function') {
            media.addListener(onSystemSchemeChange);
        }
    }

    function onSystemSchemeChange(e) {
        // Only notify .NET if current theme is 'system'
        if (getCurrentTheme() !== 'system') return;
        if (dotNetObject) {
            dotNetObject.invokeMethodAsync('OnSystemSchemeChanged', !!e.matches);
        }
        reevaluateAndApply();
    }

    return {
        initialize: function (objRef) {
            dotNetObject = objRef;
            subscribeToSystemChanges();
            reevaluateAndApply();
        },
        getCurrentTheme: function () {
            return getCurrentTheme();
        },
        getIsDark: function () {
            return getIsDark();
        },
        setTheme: function (theme) {
            const normalized = (theme || '').toLowerCase();
            const allowed = ['light', 'dark', 'system'];
            const value = allowed.includes(normalized) ? normalized : 'system';
            setStoredTheme(value === 'system' ? null : value);
            return reevaluateAndApply();
        },
        toggleTheme: function () {
            const current = getCurrentTheme();
            let next;
            if (current === 'system') {
                // When toggling from system, choose the opposite of system's current state
                next = getSystemPrefersDark() ? 'light' : 'dark';
            } else {
                next = current === 'dark' ? 'light' : 'dark';
            }
            setStoredTheme(next);
            return reevaluateAndApply();
        }
    };
})();
