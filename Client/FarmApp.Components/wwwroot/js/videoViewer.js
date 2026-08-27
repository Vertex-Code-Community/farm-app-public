window.videoViewer = {

    play: video => video.play(),

    pause: video => video.pause(),

    seek: (video, time) => video.currentTime = time,

    setVolume: (video, volume) => video.volume = volume,

    getDuration: video => video.duration || 0,

    stopVideo: video => {
        if (!video)
            return;

        video.pause();

        video.currentTime = 0;

        video.src = video.src;
    }
};