const data = () => ({
    gif: {
        width: 640,
        height: 480
    },
    snapped: false,
    snap() {
        const { width, height } = this.gif;
        this.snapped = true;
        this.$refs.canvas.getContext('2d').drawImage(this.$refs.video, 0, 0, width, height);
        
    },
    submit() {
        this.$refs.canvas.toBlob(function (blob) {
            const formData = new FormData();
            formData.append('UploadedFile', blob, 'UploadedFile.png');

            fetch('', {
                method: 'post',
                body: formData,
            }).then(() => location.reload())
        });
    },
    cancel() {
        this.snapped = false;
    },
    initCamera() {
        const video = this.$refs.video;
        // Get access to the camera!
        if (video && navigator.mediaDevices && navigator.mediaDevices.getUserMedia) {
            // Not adding `{ audio: true }` since we only want video now
            navigator.mediaDevices.getUserMedia({ video: true }).then(function (stream) {
                video.srcObject = stream;
                video.play();
            });
        }
    }
});