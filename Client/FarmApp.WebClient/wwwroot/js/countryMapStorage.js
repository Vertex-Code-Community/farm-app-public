window.countryMapStorage = {
    get: () => localStorage.getItem("app_map_country"),
    set: v => localStorage.setItem("app_map_country", v)
};
