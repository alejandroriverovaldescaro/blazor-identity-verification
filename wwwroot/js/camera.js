window.cameraInterop = {
    videoElement: null,
    stream: null,

    async startCamera(videoElementId) {
        try {
            this.videoElement = document.getElementById(videoElementId);
            
            if (!this.videoElement) {
                throw new Error('Video element not found');
            }

            // Request camera access
            this.stream = await navigator.mediaDevices.getUserMedia({
                video: {
                    width: { ideal: 1280 },
                    height: { ideal: 720 },
                    facingMode: 'user'
                },
                audio: false
            });

            this.videoElement.srcObject = this.stream;
            await this.videoElement.play();
            
            return true;
        } catch (error) {
            console.error('Error accessing camera:', error);
            return false;
        }
    },

    stopCamera() {
        if (this.stream) {
            this.stream.getTracks().forEach(track => track.stop());
            this.stream = null;
        }
        if (this.videoElement) {
            this.videoElement.srcObject = null;
        }
    },

    captureImage(videoElementId, canvasElementId) {
        try {
            const video = document.getElementById(videoElementId);
            const canvas = document.getElementById(canvasElementId);
            
            if (!video || !canvas) {
                throw new Error('Video or canvas element not found');
            }

            const context = canvas.getContext('2d');
            
            // Resize image to reasonable size for face detection (max 1920x1080)
            const maxWidth = 1920;
            const maxHeight = 1080;
            let width = video.videoWidth;
            let height = video.videoHeight;
            
            if (width > maxWidth || height > maxHeight) {
                const ratio = Math.min(maxWidth / width, maxHeight / height);
                width = width * ratio;
                height = height * ratio;
            }
            
            canvas.width = width;
            canvas.height = height;
            
            context.drawImage(video, 0, 0, width, height);
            
            // Convert canvas to JPEG with 0.8 quality to reduce size while maintaining face clarity
            return canvas.toDataURL('image/jpeg', 0.8);
        } catch (error) {
            console.error('Error capturing image:', error);
            return null;
        }
    },

    downloadImage(dataUrl, filename) {
        const link = document.createElement('a');
        link.href = dataUrl;
        link.download = filename;
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);
    }
};
