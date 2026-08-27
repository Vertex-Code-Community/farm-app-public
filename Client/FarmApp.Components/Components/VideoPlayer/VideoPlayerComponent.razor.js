export function initializePlayer(videoPlayer, dotnetHelper) {
    const video = videoPlayer.querySelector(`.video-el`);
    const playBtn = videoPlayer.querySelector(`.play-btn`);

    const currentTimeEl = videoPlayer.querySelector('.current.time');
    const totalTimeEl = videoPlayer.querySelector('.total.time');

    const timelineEl = videoPlayer.querySelector(`.timeline`);
    const watchedTimelineEl = videoPlayer.querySelector('.watched');
    const loadedTimelineEl = videoPlayer.querySelector('.loaded');

    const formatTime = (time) => {
        const minutes = Math.floor(time / 60);
        const seconds = Math.floor(time % 60);
        return `${minutes}:${seconds < 10 ? '0' : ''}${seconds}`;
    };

    const playVideo = () => {
        video.play();
        videoPlayer.classList.add('playing');
    }

    const pauseVideo = () => {
        video.pause();
        videoPlayer.classList.remove('playing');
    }

    const handleVideoClick = () => {
        if (video.paused) {
            playVideo();
        } else {
            pauseVideo();
        }
    }

    const updateCurTime = () => {
        if (isScrubbing) return;

        if (!isNaN(video.duration)) {
            const percent = (video.currentTime / video.duration) * 100;
            watchedTimelineEl.style.width = `${percent}%`;
            currentTimeEl.textContent = formatTime(video.currentTime);
        }
    }

    playBtn.addEventListener('click', handleVideoClick);

    video.addEventListener(`click`, handleVideoClick);

    const onMetadataLoaded = () => {
        totalTimeEl.textContent = formatTime(video.duration);
    };

    if (video.readyState >= 1) {
        onMetadataLoaded();
    } else {
        video.addEventListener('loadedmetadata', onMetadataLoaded);
    }

    video.addEventListener('timeupdate', updateCurTime);

    video.addEventListener('progress', () => {
        if (video.buffered.length > 0) {
            const bufferedEnd = video.buffered.end(video.buffered.length - 1);
            const duration = video.duration;
            const percent = (bufferedEnd / duration) * 100;
            loadedTimelineEl.style.width = `${percent}%`;
        }
    });

    video.addEventListener('ended', () => {
        videoPlayer.classList.remove('playing');
        watchedTimelineEl.style.width = `0%`;
        video.currentTime = 0;
    });

    let isScrubbing = false;
    let scrubTimeout;

    const scrub = (e) => {
        if (isNaN(video.duration) || !isFinite(video.duration) || video.duration === 0) return;

        const rect = timelineEl.getBoundingClientRect();
        const clientX = e.touches ? e.touches[0].clientX : e.clientX;
        const offsetX = clientX - rect.left;
        const width = rect.width;
        const percent = Math.min(Math.max(offsetX / width, 0), 1);

        const newTime = percent * video.duration;

        watchedTimelineEl.style.width = `${percent * 100}%`;
        currentTimeEl.textContent = formatTime(newTime);

        clearTimeout(scrubTimeout);
        scrubTimeout = setTimeout(() => {
            video.currentTime = newTime;
        }, 10);
    };

    timelineEl.addEventListener('mousedown', (e) => {
        isScrubbing = true;
        scrub(e);
    });

    window.addEventListener('mousemove', (e) => {
        if (isScrubbing) scrub(e);
    });

    window.addEventListener('mouseup', () => {
        isScrubbing = false;
    });

    timelineEl.addEventListener('touchstart', (e) => {
        isScrubbing = true;
        scrub(e);
    });

    timelineEl.addEventListener('touchmove', (e) => {
        if (isScrubbing) {
            if (e.cancelable) {
                e.preventDefault();
            }
            scrub(e);
        }
    }, { passive: false })

    window.addEventListener('touchend', () => {
        isScrubbing = false;
    });

    const observer = new IntersectionObserver((entries) => {
        entries.forEach(entry => {
            if (!entry.isIntersecting) {
                if (!video.paused) {
                    pauseVideo();
                }
            }
        });
    }, {
        threshold: 0.3
    });

    observer.observe(videoPlayer);

    video.addEventListener('canplay', () => {
        if (video.currentTime === 0) {
            video.currentTime = 0.001;
        }
    }, { once: true });

    return {
        dispose: () => {
            observer.disconnect();
            video.pause();
            video.src = "";
            video.load();
        }
    };
}