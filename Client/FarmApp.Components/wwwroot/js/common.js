function navigateTo(route) {
    DotNet.invokeMethodAsync("FarmApp.Services", 'NavigateToFromJs', route);
}

function getActiveTouchesLength() {
    return document.touches ? document.touches.length : 0;
}

function onRippleEffectTouchStartListener(e, center = false) {
    const rect = e.currentTarget.getBoundingClientRect();
    if (!e.touches.length) return;
    
    const touch = e.touches[0];
    
    const x = center ? rect.width * 0.5 : touch.clientX - rect.left;
    const y = center ? rect.height * 0.5 : touch.clientY - rect.top;

    const ripples = document.createElement('span');
    ripples.classList.add('ripple-span');
    ripples.style.left = x + 'px';
    ripples.style.top = y + 'px';

    e.currentTarget.appendChild(ripples);

    setTimeout(() => {
        ripples.remove()
    },600);
}

function onRippleEffectClickListener(e, center = false) {
    const rect = e.currentTarget.getBoundingClientRect();

    const x = center ? rect.width * 0.5 : e.clientX - rect.left;
    const y = center ? rect.height * 0.5 : e.clientY - rect.top;

    const ripples = document.createElement('span');
    ripples.classList.add('ripple-span');
    ripples.style.left = x + 'px';
    ripples.style.top = y + 'px';

    e.currentTarget.appendChild(ripples);

    setTimeout(() => {
        ripples.remove()
    },600);
}


window.mediaPreview = {
    createUrl: function (bytes, contentType) {
        const uint8 = new Uint8Array(bytes);
        const blob = new Blob([uint8], { type: contentType });
        return URL.createObjectURL(blob);
    },
    async createVideoThumbnail(videoUrl) {
        return new Promise((resolve) => {
            const video = document.createElement("video");
            video.src = videoUrl;
            video.muted = true;
            video.playsInline = true;

            video.addEventListener("loadeddata", () => {
                video.currentTime = Math.min(0.2, video.duration / 2);
            });

            video.addEventListener("seeked", () => {
                const canvas = document.createElement("canvas");
                canvas.width = video.videoWidth;
                canvas.height = video.videoHeight;

                const ctx = canvas.getContext("2d");
                ctx.drawImage(video, 0, 0);

                resolve(canvas.toDataURL("image/jpeg", 0.7));
            });
        });
    },
    revokeUrl: function (url) {
        URL.revokeObjectURL(url);
    }
};
window.hideStartupLoader = () => {
    document.body.classList.remove("startup");
    const el = document.getElementById("startup-loader");
    if (!el) return;

    el.classList.add("hide");
    setTimeout(() => el.remove(), 250);
};
