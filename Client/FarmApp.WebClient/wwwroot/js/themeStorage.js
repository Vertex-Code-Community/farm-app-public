window.themeStorage = {
    get: () => localStorage.getItem("app_theme"),
    set: v => localStorage.setItem("app_theme", v)
};