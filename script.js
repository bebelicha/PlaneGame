const videoElement = document.getElementById('input_video');
const canvasElement = document.getElementById('output_canvas');
const canvasCtx = canvasElement.getContext('2d');

let mouthOpenDetected = false;

function onResults(results) {
  canvasCtx.save();
  // Clear the canvas with a transparent background
  canvasCtx.clearRect(0, 0, canvasElement.width, canvasElement.height);

  if (results.multiFaceLandmarks) {
    for (const landmarks of results.multiFaceLandmarks) {
      // The following indices correspond to landmarks for the inner lips.
      const upperLip = landmarks[13];
      const lowerLip = landmarks[14];

      const mouthOpening = Math.hypot(upperLip.x - lowerLip.x, upperLip.y - lowerLip.y);
      
      // Threshold for mouth opening detection. You may need to adjust this value.
      const MOUTH_OPEN_THRESHOLD = 0.04;

      if (mouthOpening > MOUTH_OPEN_THRESHOLD && !mouthOpenDetected) {
        mouthOpenDetected = true;
        sendMouthOpenToUnity();
      } else if (mouthOpening <= MOUTH_OPEN_THRESHOLD) {
        mouthOpenDetected = false;
      }

      // Optionally, draw landmarks for debugging, but not the video frame.
      drawConnectors(canvasCtx, landmarks, FACEMESH_LIPS, {color: '#E0E0E0'});
    }
  }
  canvasCtx.restore();
}

const faceMesh = new FaceMesh({locateFile: (file) => {
  return `https://cdn.jsdelivr.net/npm/@mediapipe/face_mesh/${file}`;
}});
faceMesh.setOptions({
  maxNumFaces: 1,
  refineLandmarks: true,
  minDetectionConfidence: 0.5,
  minTrackingConfidence: 0.5
});
faceMesh.onResults(onResults);

const camera = new Camera(videoElement, {
  onFrame: async () => {
    await faceMesh.send({image: videoElement});
  },
  width: 1280,
  height: 800
});
camera.start();

function sendMouthOpenToUnity() {
  if (unityInstance) {
    const data = {
      type: "mouthOpen",
      value: 1, // 1 for mouth open detected
      captureTime: Date.now()
    };
    const jsonData = JSON.stringify(data);
    unityInstance.SendMessage('DateReceiver', 'Date', jsonData);
    console.log("Mouth open detected, data sent to Unity:", jsonData);
  } else {
    console.error("Unity instance not ready.");
  }
}
