
function bchSetThemeToHtml(newTheme) {
    if (!newTheme) return;
    
    const htmlElement = document.documentElement;

    htmlElement.classList.forEach(cls => {
        if (cls.startsWith('bch-theme-')) {
            htmlElement.classList.remove(cls);
        }
    });

    htmlElement.classList.add(newTheme);
}

function bchClearThemeToHtml() {
    const htmlElement = document.documentElement;

    htmlElement.classList.forEach(cls => {
        if (cls.startsWith('bch-theme-')) {
            htmlElement.classList.remove(cls);
        }
    });
}